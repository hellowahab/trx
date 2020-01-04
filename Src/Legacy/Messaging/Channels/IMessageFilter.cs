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

namespace Trx.Messaging.Channels
{
    /// <summary>
    /// Users should implement this interface to implement customised message
    /// filtering.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Users should implement this interface to implement customized message
    /// filtering.
    /// </para>
    /// <para>
    /// This abstract class assumes and also imposes that filters be
    /// organized in a linear chain. The <see cref="Decide"/>
    /// method of each filter is called sequentially, in the order of their 
    /// addition to the chain.
    /// </para>
    /// <para>
    /// The <see cref="Decide"/> method must return one of the integer constants
    /// <see cref="MessageFilterDecision.Deny"/>, 
    /// <see cref="MessageFilterDecision.Neutral"/> or
    /// <see cref="MessageFilterDecision.Accept"/>.
    /// </para>
    /// <para>
    /// If the value <see cref="MessageFilterDecision.Deny"/> is returned,
    /// then the message is dropped immediately without consulting with the
    /// remaining filters.
    /// </para>
    /// <para>
    /// If the value <see cref="MessageFilterDecision.Neutral"/> is returned,
    /// then the next filter in the chain is consulted. If there are no more
    /// filters in the chain, then the message is processed. Thus, in the
    /// presence of no filters, the default behaviour is to process all messages.
    /// </para>
    /// <para>
    /// If the value <see cref="MessageFilterDecision.Accept"/> is returned,
    /// then the message is processed without consulting the remaining filters.
    /// </para>
    /// </remarks>
    public interface IMessageFilter
    {
        /// <summary>
        /// Property to get and set the next filter in the filter chain of
        /// responsibility.
        /// </summary>
        /// <value>
        /// The next filter in the chain
        /// </value>
        /// <remarks>
        /// Filters are typically composed into chains. This property allows the
        /// next filter in the chain to be accessed.
        /// </remarks>
        IMessageFilter Next { get; set; }

        /// <summary>
        /// Decide if the message should be processed and apply transfomartions
        /// if required.
        /// </summary>
        /// <param name="channel">
        /// The channel where the message is processed.
        /// </param>
        /// <param name="message">
        /// The message to filter.
        /// </param>
        /// <returns>
        /// The decision of the filter, and transformations in message if done
        /// by the filter.
        /// </returns>
        /// <remarks>
        /// <para>
        /// If the decision is <see cref="MessageFilterDecision.Deny"/>,
        /// then the message will be dropped. If the decision is
        /// <see cref="MessageFilterDecision.Neutral"/>, then the next
        /// filter, if any, will be invoked. If the decision is
        /// <see cref="MessageFilterDecision.Accept"/> then the message will be
        /// processed without consulting with other filters in the chain.
        /// </para>
        /// </remarks>
        MessageFilterDecision Decide(IChannel channel, Message message);
    }
}