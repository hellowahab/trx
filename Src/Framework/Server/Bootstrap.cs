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
using Trx.Logging;

namespace Trx.Server
{
    public class Bootstrap
    {
        private const string RootDirectory = "/";
        private const string BinDirectory = "bin";
        private const string TrxServerConfigFile = "trxserver.xml";
        private readonly string _configDirectory;
        private readonly string[] _configDirectoryFullDepth;
        private readonly Queue<FileSystemChange> _fileSystemChangesQueue = new Queue<FileSystemChange>();

        private readonly object _lockObj = new object();
        private readonly int _pollInterval;
        private readonly TrxServerTupleSpaceProvider _tupleSpaceProvider;

        // Dictionary key is lowcase Trx Server instance name.
        private readonly Dictionary<string, TrxServerHost> _trxServerInstances = new Dictionary<string, TrxServerHost>();

        private readonly bool _useSharedBaseDirectory;
        private FileSystemWatcher _directoryWatcher;

        // Used to track batchs of file system changes in ProcessFileSystemChanges
        private int _fileSystemChangeBatchId;
        private FileSystemWatcher _fileWatcher;

        private bool _isRunning;
        private ILogger _logger;

        private Timer _timer;

        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        /// <param name="logger">
        /// Logger to use. Can be null.
        /// </param>
        /// <param name="configDirectory">
        /// The root configuration directory.
        /// </param>
        /// <param name="reloadAppOnChanges">
        /// When true the framework reloads applications when Trx Server config file or one of the loaded
        /// assemblies in the bin directory changes.
        /// </param>
        /// <param name="useSharedBaseDirectory">
        /// True to set the base directory of the app domains of the deployed Trx Server the same as
        /// the domain hosting this class (and additionally use bin folder to load local assemblies).
        /// </param>
        /// <param name="pollInterval">
        /// File system changes batch poll interval in milliseconds.
        /// </param>
        public Bootstrap(ILogger logger, string configDirectory, bool reloadAppOnChanges, bool useSharedBaseDirectory,
            int pollInterval)
        {
            Logger = logger;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            if (!Directory.Exists(configDirectory))
                throw new ApplicationException(string.Format("Config directory '{0}' doesn't exists.", configDirectory));

            _useSharedBaseDirectory = useSharedBaseDirectory;
            _pollInterval = pollInterval;
            _configDirectory = Path.GetFullPath(configDirectory);
            _configDirectoryFullDepth = _configDirectory.Split(Path.DirectorySeparatorChar);
            ReloadAppOnChanges = reloadAppOnChanges;
            _tupleSpaceProvider = new TrxServerTupleSpaceProvider(this);
        }

        public bool UseSharedBaseDirectory
        {
            get { return _useSharedBaseDirectory; }
        }

        public bool ReloadAppOnChanges { get; set; }

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

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                LogManager.GetLogger().Error("Unhandled exception catched", e.ExceptionObject as Exception);
            }
            catch
            {
            }
        }

        private string GetTrxServerConfigFile(string configDirectory)
        {
            return string.Format("{0}\\TrxServer.xml", configDirectory);
        }

        private string GetBinDirectory(string configDirectory)
        {
            return string.Format("{0}\\bin", configDirectory);
        }

        private string GetInstanceName(string deployDirectory)
        {
            if (deployDirectory == RootDirectory)
                return @"\\Root";

            return @"\\Root\" + deployDirectory;
        }

        public TrxServerHost GetTrxServerHost(string instanceName)
        {
            string key = instanceName.ToLower();
            TrxServerHost host;
            return _trxServerInstances.TryGetValue(key, out host) ? host : null;
        }

        private void RemoveTrxServerHost(string instanceName)
        {
            string key = instanceName.ToLower();
            if (_trxServerInstances.ContainsKey(key))
                _trxServerInstances.Remove(key);
        }

        private void FileSystemCreated(object sender, FileSystemEventArgs e)
        {
            lock (_fileSystemChangesQueue)
                _fileSystemChangesQueue.Enqueue(new FileSystemChange(e, File.Exists(e.FullPath)));
        }

        private void FileSystemChanged(object sender, FileSystemEventArgs e)
        {
            lock (_fileSystemChangesQueue)
            {
                if (sender == _fileWatcher && !File.Exists(e.FullPath))
                    // It's a directory change informed by the file watcher, ignore it.
                    return;
                _fileSystemChangesQueue.Enqueue(new FileSystemChange(e, true));
            }
        }

        private void FileSystemRenamed(object sender, RenamedEventArgs e)
        {
            lock (_fileSystemChangesQueue)
                _fileSystemChangesQueue.Enqueue(new FileSystemChange(e, File.Exists(e.FullPath)));
        }

        private void FileSystemDeleted(object sender, FileSystemEventArgs e)
        {
            lock (_fileSystemChangesQueue)
                _fileSystemChangesQueue.Enqueue(new FileSystemChange(e, sender == _fileWatcher));
        }

        /// <summary>
        /// Watch root directory on subdirectories changes.
        /// </summary>
        private void StartWatchingRootDirectory()
        {
            _fileSystemChangesQueue.Clear();

            _directoryWatcher = new FileSystemWatcher(_configDirectory, "*.*")
                                    {
                                        IncludeSubdirectories = false,
                                        NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.LastWrite
                                    };
            _directoryWatcher.Created += FileSystemCreated;
            _directoryWatcher.Renamed += FileSystemRenamed;
            _directoryWatcher.Deleted += FileSystemDeleted;

            _fileWatcher = new FileSystemWatcher(_configDirectory, "*.*")
                               {
                                   IncludeSubdirectories = true,
                                   NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
                               };
            _fileWatcher.Created += FileSystemCreated;
            _fileWatcher.Changed += FileSystemChanged;
            _fileWatcher.Renamed += FileSystemRenamed;
            _fileWatcher.Deleted += FileSystemDeleted;

            _directoryWatcher.EnableRaisingEvents = true;
            _fileWatcher.EnableRaisingEvents = true;
        }

        private void StopWatchingRootDirectory()
        {
            _directoryWatcher.EnableRaisingEvents = false;
            _fileWatcher.EnableRaisingEvents = false;

            _directoryWatcher.Created -= FileSystemCreated;
            _directoryWatcher.Changed -= FileSystemChanged;
            _directoryWatcher.Renamed -= FileSystemRenamed;
            _directoryWatcher.Deleted -= FileSystemDeleted;

            _directoryWatcher = null;

            _fileWatcher.Created -= FileSystemCreated;
            _fileWatcher.Changed -= FileSystemChanged;
            _fileWatcher.Renamed -= FileSystemRenamed;
            _fileWatcher.Deleted -= FileSystemDeleted;

            _fileWatcher = null;

            _fileSystemChangesQueue.Clear();
        }

        /// <summary>
        /// Check if element deserves attention.
        /// </summary>
        /// <param name="fullDepth">
        /// Path raising the file system change event.
        /// </param>
        /// <param name="isFile">
        /// Indicates if the item producing the event is a file or a directory.
        /// </param>
        /// <returns>
        /// The deploy key, or null if not a valid one.
        /// </returns>
        private string IsProcessableElement(string[] fullDepth, bool isFile)
        {
            string item = fullDepth[fullDepth.Length - 1].ToLower();
            if (isFile)
            {
                if (fullDepth.Length <= (_configDirectoryFullDepth.Length + 2) &&
                    item == TrxServerConfigFile)
                {
                    string dir = fullDepth.Length == (_configDirectoryFullDepth.Length + 1)
                        ? RootDirectory
                        : fullDepth[fullDepth.Length - 2];
                    return dir.ToLower() == BinDirectory ? null : GetInstanceName(dir);
                    // It's a Trx Server config file.
                }

                if (fullDepth[fullDepth.Length - 2].ToLower() == BinDirectory &&
                    (fullDepth.Length - 1) <= (_configDirectoryFullDepth.Length + 2) &&
                        (item.Length > 4) && item.Substring(item.Length - 4, 4) == ".dll")
                    // A dll file directly under the base directory.
                    return fullDepth.Length == (_configDirectoryFullDepth.Length + 2)
                        ? GetInstanceName(RootDirectory)
                        : GetInstanceName(fullDepth[fullDepth.Length - 3]);
            }
            else
            {
                if (fullDepth.Length <= (_configDirectoryFullDepth.Length + 2) &&
                    item == BinDirectory)
                    // It's a bin directory.
                    return fullDepth.Length == (_configDirectoryFullDepth.Length + 1)
                        ? GetInstanceName(RootDirectory)
                        : GetInstanceName(fullDepth[fullDepth.Length - 2]);

                if (fullDepth.Length == (_configDirectoryFullDepth.Length + 1))
                    // It's a directory under config directory.
                    return GetInstanceName(fullDepth[fullDepth.Length - 1]);
            }

            return null;
        }

        private void ProcessFileSystemChange(string instanceName, string item)
        {
            TrxServerHost host = GetTrxServerHost(instanceName);
            if (host == null)
                // Change on a directory without a Trx Server deployed on it.
                return;

            if (host.FileSystemChangeBatchId == _fileSystemChangeBatchId)
                // Already processed in this ProcessFileSystemChanges iteration.
                return;

            string lowCaseItem = item.ToLower();
            if (ReloadAppOnChanges &&
                ((lowCaseItem == TrxServerConfigFile) || // Trx Server config file changed?
                    ((lowCaseItem.Substring(lowCaseItem.Length - 4, 4) == ".dll") &&
                        host.IsALoadedAssembly(lowCaseItem)))) // A loaded assembly change?
            {
                Logger.Info(string.Format("Change detected on file '{0}', reloading Trx Server instance '{1}' ...", item,
                    host.InstanceName));
                // Redeploy application.
                try
                {
                    _tupleSpaceProvider.LoadingInstance(instanceName);
                    RemoveTrxServerHost(instanceName);
                    host.Unload();
                    host = CreateTrxServerHost(instanceName, host.ConfigDirectory,
                        GetTrxServerConfigFile(host.ConfigDirectory));
                    if (host == null)
                        // Failed Trx Server instance
                        return;
                    host.Start();
                }
                finally
                {
                    _tupleSpaceProvider.LoadFinished(instanceName);
                }
            }
        }

        private void ProcessFileSystemCreate(string instanceName, string item, string fullPath, bool log)
        {
            if (GetTrxServerHost(instanceName) != null)
                // Already deployed, maybe it's create for the config file after a create of the
                // deploy directory.
                return;

            string configFileName = null;
            string configDirectory = null;
            if (item.ToLower() == TrxServerConfigFile)
            {
                configFileName = fullPath;
                configDirectory = Path.GetDirectoryName(fullPath);
            }
            else if (GetInstanceName(item) == instanceName)
            {
                // It's a new deploy directory
                configFileName = GetTrxServerConfigFile(fullPath);
                if (File.Exists(configFileName))
                    // Deploy only if config file exists.
                    configDirectory = fullPath;
            }

            if (configFileName == null | configDirectory == null)
                return;

            if (log)
                Logger.Info(string.Format("New Trx Server instance '{0}' detected on directory '{1}', deploying...",
                    instanceName, configDirectory));
            // Deploy application.
            TrxServerHost host = CreateTrxServerHost(instanceName, configDirectory, configFileName);
            if (host == null)
                // Failed Trx Server instance
                return;
            host.Start();
        }

        private void ProcessFileSystemDelete(string instanceName, string item)
        {
            TrxServerHost host = GetTrxServerHost(instanceName);
            if (host == null)
                // Change on a directory without a Trx Server deployed on it.
                return;

            string lowCaseItem = item.ToLower();
            if ((lowCaseItem == TrxServerConfigFile) || // Trx Server config file deleted?
                (GetInstanceName(item) == instanceName)) // A deploy directory deleted?
            {
                Logger.Info(string.Format("{1} of Trx Server instance '{0}' was deleted, undeploying...", instanceName,
                    lowCaseItem == TrxServerConfigFile ? "Config file" : "Directory"));
                RemoveTrxServerHost(instanceName);
                // Shutdown application.
                host.Unload();
            }
        }

        private void ProcessFileSystemRename(string instanceName, string newFullPath,
            string item, string oldInstanceName)
        {
            if (item.ToLower() == TrxServerConfigFile)
            {
                // Trx Server config file renamed.
                ProcessFileSystemCreate(instanceName, item, newFullPath, true);
                return;
            }

            TrxServerHost host = GetTrxServerHost(oldInstanceName);
            if (host == null)
                // Rename of a directory without a Trx Server deployed on it.
                return;

            if (instanceName == null || instanceName == oldInstanceName)
                Logger.Info(string.Format("Trx Server config file of instance '{0}' was renamed, undeploying...",
                    oldInstanceName));
            else
                Logger.Info(string.Format("Trx Server instance '{0}' renamed to '{1}', redeploying...",
                    oldInstanceName,
                    instanceName));

            RemoveTrxServerHost(oldInstanceName);
            // Shutdown application.
            host.Unload();

            if (instanceName != null)
                // Redeploy.
                ProcessFileSystemCreate(instanceName, item, newFullPath, false);
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
                        FileSystemChange change = _fileSystemChangesQueue.Dequeue();

                        var rea = change.EventArgs as RenamedEventArgs;
                        string[] fullDepth = change.EventArgs.FullPath.Split(Path.DirectorySeparatorChar);
                        string oldInstanceName = null;
                        if (rea != null)
                            oldInstanceName = IsProcessableElement(rea.OldFullPath.Split(Path.DirectorySeparatorChar),
                                change.IsFile);
                        string instanceName = IsProcessableElement(fullDepth, change.IsFile);
                        if (instanceName == null)
                        {
                            if (rea == null)
                                continue;
                            if (oldInstanceName == null)
                                continue;
                        }

                        string item = fullDepth[fullDepth.Length - 1];
                        switch (change.EventArgs.ChangeType)
                        {
                            case WatcherChangeTypes.Changed:
                                ProcessFileSystemChange(instanceName, item);
                                break;
                            case WatcherChangeTypes.Created:
                                ProcessFileSystemCreate(instanceName, item, change.EventArgs.FullPath, true);
                                break;
                            case WatcherChangeTypes.Deleted:
                                ProcessFileSystemDelete(instanceName, item);
                                break;
                            case WatcherChangeTypes.Renamed:
                                ProcessFileSystemRename(instanceName, change.EventArgs.FullPath, item, oldInstanceName);
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

        private TrxServerHost CreateTrxServerHost(string instanceName, string configDirectory,
            string configFileName)
        {
            var host = new TrxServerHost(Logger, instanceName,
                configDirectory, configFileName, GetBinDirectory(configDirectory),
                _useSharedBaseDirectory, _tupleSpaceProvider) {FileSystemChangeBatchId = _fileSystemChangeBatchId};

            _trxServerInstances.Add(instanceName.ToLower(), host);

            return host.Failed ? null : host;
        }

        /// <summary>
        /// Start running.
        /// </summary>
        public void Start()
        {
            if (_isRunning)
                return;

            lock (_lockObj)
            {
                if (_isRunning)
                    return;

                var logUnit = new LogUnit(
                    "Detected configuration:",
                    string.Format("base config directory is '{0}'", _configDirectory),
                    string.Format("applications {0}using shared base bin folder{1}",
                        (_useSharedBaseDirectory ? string.Empty : "NOT "),
                        (_useSharedBaseDirectory ? " '" + AppDomain.CurrentDomain.BaseDirectory + "'" : string.Empty)),
                    string.Format("applications {0}reloading on assembly updates in local bin directory",
                        (ReloadAppOnChanges ? string.Empty : "NOT ")));

                string configFileName = GetTrxServerConfigFile(_configDirectory);
                var configFiles = new List<HostInfo>();
                if (File.Exists(configFileName))
                    configFiles.Add(new HostInfo(GetInstanceName(RootDirectory), _configDirectory, configFileName));
                foreach (string subDir in Directory.GetDirectories(_configDirectory))
                {
                    configFileName = GetTrxServerConfigFile(subDir);
                    if (File.Exists(configFileName))
                    {
                        string dirName = Path.GetFileName(subDir);
                        if (dirName != null)
                            configFiles.Add(new HostInfo(GetInstanceName(dirName), subDir, configFileName));
                    }
                }

                if (configFiles.Count == 0)
                    logUnit.Messages.Add("no Trx Server configuration files detected");
                else
                {
                    logUnit.Messages.Add("detected Trx Server configuration files:");
                    foreach (HostInfo configFile in configFiles)
                        logUnit.Messages.Add("   " + configFile.ConfigFileName);
                }

                Logger.Info(logUnit);

                foreach (HostInfo configFile in configFiles)
                    CreateTrxServerHost(configFile.InstanceName, configFile.ConfigDirectory,
                        configFile.ConfigFileName);

                try
                {
                    foreach (TrxServerHost instance in _trxServerInstances.Values)
                        // Prevent client tuple spaces to race for an instance wich will
                        // load after their already loaded instance.
                        _tupleSpaceProvider.LoadingInstance(instance.InstanceName);
                    foreach (TrxServerHost instance in _trxServerInstances.Values)
                        instance.Start();
                }
                finally
                {
                    foreach (TrxServerHost instance in _trxServerInstances.Values)
                        _tupleSpaceProvider.LoadFinished(instance.InstanceName);
                }

                _timer = new Timer(OnTimer, null, _pollInterval, _pollInterval);

                StartWatchingRootDirectory();

                _isRunning = true;
            }

            Logger.Info("System is now up and running");
        }

        public void Shutdown()
        {
            Logger.Info("Executing a graceful shutdown");

            lock (_lockObj)
            {
                StopWatchingRootDirectory();

                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer = null;

                foreach (TrxServerHost instance in _trxServerInstances.Values)
                    instance.Stop();

                foreach (TrxServerHost instance in _trxServerInstances.Values)
                    AppDomain.Unload(instance.Domain);

                _trxServerInstances.Clear();

                _isRunning = false;
            }

            Logger.Info("System is now stopped");
        }

        #region Nested type: FileSystemChange
        private class FileSystemChange
        {
            private readonly FileSystemEventArgs _eventArgs;
            private readonly bool _isFile;

            public FileSystemChange(FileSystemEventArgs eventArgs, bool isFile)
            {
                _eventArgs = eventArgs;
                _isFile = isFile;
            }

            public bool IsFile
            {
                get { return _isFile; }
            }

            public FileSystemEventArgs EventArgs
            {
                get { return _eventArgs; }
            }
        }
        #endregion

        #region Nested type: HostInfo
        private class HostInfo
        {
            private readonly string _configDirectory;
            private readonly string _configFileName;
            private readonly string _instanceName;

            public HostInfo(string instanceName, string configDirectory, string configFileName)
            {
                _instanceName = instanceName;
                _configDirectory = configDirectory;
                _configFileName = configFileName;
            }

            public string ConfigFileName
            {
                get { return _configFileName; }
            }

            public string ConfigDirectory
            {
                get { return _configDirectory; }
            }

            public string InstanceName
            {
                get { return _instanceName; }
            }
        }
        #endregion
    }
}