#region Copyright (C) 2004-2012 Zabaleta Asociados SRL
//
// Trx Framework - <http://www.trxframework.org/>
// Copyright (C) 2004-2012  Zabaleta Asociados SRL
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;
using Trx.Logging;
using Trx.Utilities.Dom2Obj;

namespace Trx.Server
{
    public class FileBasedServicesProvider : IServicesProvider
    {
        private readonly Queue<FileSystemEventArgs> _fileSystemChangesQueue = new Queue<FileSystemEventArgs>();
        private readonly object _lockObj = new object();
        private readonly Dictionary<string, ServiceDescriptor> _services = new Dictionary<string, ServiceDescriptor>();
        private string _deployDirectory;
        private int _fileSystemChangeBatchId;
        private FileSystemWatcher _fileWatcher;
        private bool _isRunning;
        private ILogger _logger;
        private int _pollInterval = 5000;
        private int _serviceId;
        private Timer _timer;
        private TrxServer _trxServer;

        public string DeployDirectory
        {
            get { return _deployDirectory; }
            set { _deployDirectory = value; }
        }

        public int PollInterval
        {
            get { return _pollInterval; }
            set { _pollInterval = value; }
        }

        public bool MoveFailedFilesToErrorsFolder { get; set; }

        #region IServicesProvider Members
        public ILogger Logger
        {
            get
            {
                return _logger ?? (_logger = LogManager.GetLogger(
                    MethodBase.GetCurrentMethod().DeclaringType.ToString()));
            }
            set
            {
                if (value == null)
                    _logger = LogManager.GetLogger(
                        MethodBase.GetCurrentMethod().DeclaringType.ToString());
                else
                    _logger = value;
            }
        }

        public void Start(TrxServer trxServer)
        {
            if (_isRunning)
                return;

            lock (_lockObj)
            {
                if (_isRunning)
                    return;

                if (_deployDirectory == null)
                    _deployDirectory = trxServer.ConfigDirectory + "\\services";

                if (!Directory.Exists(_deployDirectory))
                    Directory.CreateDirectory(_deployDirectory);

                _trxServer = trxServer;

                foreach (var file in Directory.GetFiles(_deployDirectory))
                    DeployService(file, true);

                _timer = new Timer(OnTimer, null, _pollInterval, _pollInterval);

                StartWatchingDeployDirectory();

                Logger.Info(string.Format("File based services provider starts watching on directory '{0}'", _deployDirectory));

                _isRunning = true;
            }
        }

        public void Stop()
        {
            lock (_lockObj)
            {
                StopWatchingDeployDirectory();

                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer = null;

                _trxServer = null;

                _isRunning = false;
            }
        }
        #endregion

        private void FileSystemCreated(object sender, FileSystemEventArgs e)
        {
            lock (_fileSystemChangesQueue)
                _fileSystemChangesQueue.Enqueue(e);
        }

        private void FileSystemChanged(object sender, FileSystemEventArgs e)
        {
            lock (_fileSystemChangesQueue)
                _fileSystemChangesQueue.Enqueue(e);
        }

        private void FileSystemRenamed(object sender, RenamedEventArgs e)
        {
            lock (_fileSystemChangesQueue)
                _fileSystemChangesQueue.Enqueue(e);
        }

        private void FileSystemDeleted(object sender, FileSystemEventArgs e)
        {
            lock (_fileSystemChangesQueue)
                _fileSystemChangesQueue.Enqueue(e);
        }

        private void StartWatchingDeployDirectory()
        {
            _fileSystemChangesQueue.Clear();

            _fileWatcher = new FileSystemWatcher(_deployDirectory, "*.xml")
                               {
                                   IncludeSubdirectories = false,
                                   NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
                               };
            _fileWatcher.Created += FileSystemCreated;
            _fileWatcher.Changed += FileSystemChanged;
            _fileWatcher.Renamed += FileSystemRenamed;
            _fileWatcher.Deleted += FileSystemDeleted;

            _fileWatcher.EnableRaisingEvents = true;
        }

        private void StopWatchingDeployDirectory()
        {
            if (_fileWatcher == null)
                return;

            _fileWatcher.EnableRaisingEvents = false;

            _fileWatcher.Created -= FileSystemCreated;
            _fileWatcher.Changed -= FileSystemChanged;
            _fileWatcher.Renamed -= FileSystemRenamed;
            _fileWatcher.Deleted -= FileSystemDeleted;

            _fileWatcher = null;

            _fileSystemChangesQueue.Clear();
        }

        private void ProcessFileSystemChange(FileSystemEventArgs args)
        {
            var key = args.Name.ToLower();
            if (!_services.ContainsKey(key))
                return;

            var service = _services[key];
            if (service.FileSystemChangeBatchId == _fileSystemChangeBatchId)
                // Changes already processed
                return;

            var doc = new XmlDocument();
            using (var reader = new XmlTextReader(service.ConfigFileName))
                doc.Load(reader);

            // Determine change level, if it's a property value change update it, otherwise redeploy the service.
            if (Digester.StructureHasChanged(service.DigesterContext, doc))
            {
                // Redeploy.
                _services.Remove(key);
                Logger.Info(string.Format("Service config file '{0}' structure changes detected, updating by redeploying it ...", args.Name));
                _trxServer.UndeployService(service.TrxService);
                Thread.Sleep(1000); // Wait a bit.
                DeployService(service.ConfigFileName, false);
            }
            else
            {
                // Process property value changes.
                Logger.Info(string.Format("A change in service config file '{0}' has been detected, updating value properties ...",
                    args.Name));
                Digester.UpdateValueProperties(service.DigesterContext, doc, Logger);
            }
            Logger.Info(string.Format("Changes in file '{0}' have been processed.", args.Name));

            service.FileSystemChangeBatchId = _fileSystemChangeBatchId;
        }

        private void DeployService(string fullPath, bool log)
        {
            string fileName = Path.GetFileName(fullPath);
            if (fileName == null)
                return;

            var key = fileName.ToLower();
            if (_services.ContainsKey(key))
                return;

            if (log)
                Logger.Info(string.Format("New service config file '{0}' detected, deploying ...", fileName));

            ServiceDescriptor service;
            try
            {
                DigestContext digestContext;
                var obj = Digester.DigestFile(fullPath, out digestContext);
                if (obj is ITrxService)
                    service = new ServiceDescriptor(obj as ITrxService, digestContext);
                else
                    service = new ServiceDescriptor(new WrappedObjectInTrxService(obj), digestContext);
            }
            catch (Exception e)
            {
                if (MoveFailedFilesToErrorsFolder)
                    // Move file to errors directory
                    try
                    {
                        if (!Directory.Exists(_deployDirectory + @"\errors"))
                            Directory.CreateDirectory(_deployDirectory + @"\errors");

                        DateTime now = DateTime.Now;
                        fileName = String.Format("{0:0000}{1:00}{2:00}-{3:00}{4:00}{5:00}.{6:000}.{7}",
                            now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond, fileName);
                        File.Move(fullPath, _deployDirectory + @"\errors\" + fileName);
                        Logger.Error(string.Format("Can't process file '{0}', moved to errors folder.", fileName), e);
                    }
                    catch (Exception inner)
                    {
                        Logger.Error("Can't move failed service file '" + fullPath + "' to errors directory.", inner);
                    }
                else
                    Logger.Error(string.Format("Can't process file '{0}'.", fileName), e);
                return;
            }

            _serviceId++;
            service.TrxService.Id = _serviceId;

            if (string.IsNullOrEmpty(service.TrxService.Name))
                // Default name to xml file name
                service.TrxService.Name = fileName;

            _trxServer.DeployService(service.TrxService);

            if (service.TrxService.State != TrxServiceState.Started)
                return;

            service.ConfigFileName = fullPath;
            service.FileSystemChangeBatchId = _fileSystemChangeBatchId;
            _services.Add(key, service);
        }

        private void ProcessFileSystemCreated(FileSystemEventArgs args)
        {
            if (_trxServer == null)
                return;

            DeployService(args.FullPath, true);
        }

        private void ProcessFileSystemDeleted(FileSystemEventArgs args)
        {
            if (_trxServer == null)
                return;

            var key = args.Name.ToLower();
            if (!_services.ContainsKey(key))
                return;

            var service = _services[key];
            _services.Remove(key);

            Logger.Info(string.Format("Service config file '{0}' has been deleted, undeploying ...", args.Name));

            _trxServer.UndeployService(service.TrxService);
        }

        private void ProcessFileSystemRenamed(RenamedEventArgs args)
        {
            if (args == null)
                return;

            var oldKey = args.OldName.ToLower();
            if (!_services.ContainsKey(oldKey))
                return;

            // Rename key
            var service = _services[oldKey];
            _services.Remove(oldKey);

            string newKey = args.Name.ToLower();
            if (_services.ContainsKey(newKey))
            {
                Logger.Warn(string.Format("Renamed service from file '{0}' to file '{1}', already registered",
                    args.OldName, args.Name));
                return;
            }

            if (service.TrxService.Name == args.OldName)
                // Default name to xml file name
                service.TrxService.Name = args.Name;

            service.ConfigFileName = args.FullPath;
            _services.Add(newKey, service);

            Logger.Info(string.Format("Renamed service config file '{0}' to '{1}'", args.OldName, args.Name));
        }

        private void ProcessFileSystemChanges()
        {
            lock (_fileSystemChangesQueue)
            {
                try
                {
                    if (_fileSystemChangesQueue.Count > 0)
                        // Compute new batch id. Used to group changes in one ProcessFileSystemChanges
                        // and avoid event reprocessing.
                        _fileSystemChangeBatchId++;

                    while (_fileSystemChangesQueue.Count > 0)
                    {
                        FileSystemEventArgs args = _fileSystemChangesQueue.Dequeue();

                        switch (args.ChangeType)
                        {
                            case WatcherChangeTypes.Changed:
                                ProcessFileSystemChange(args);
                                break;
                            case WatcherChangeTypes.Created:
                                ProcessFileSystemCreated(args);
                                break;
                            case WatcherChangeTypes.Deleted:
                                ProcessFileSystemDeleted(args);
                                break;
                            case WatcherChangeTypes.Renamed:
                                ProcessFileSystemRenamed(args as RenamedEventArgs);
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("Exception caught processing file system changes", e);
                }
            }
        }

        private void OnTimer(object state)
        {
            lock (_lockObj)
                if (_isRunning)
                    ProcessFileSystemChanges();
        }

        #region Nested type: ServiceDescriptor
        private class ServiceDescriptor
        {
            private readonly ITrxService _trxService;
            private readonly DigestContext _digesterContext;

            public ServiceDescriptor(ITrxService trxService, DigestContext digesterContext)
            {
                _trxService = trxService;
                _digesterContext = digesterContext;
            }

            public DigestContext DigesterContext
            {
                get { return _digesterContext; }
            }

            public ITrxService TrxService
            {
                get { return _trxService; }
            }

            public int FileSystemChangeBatchId { get; set; }

            public string ConfigFileName { get; set; }
        }
        #endregion
    }
}