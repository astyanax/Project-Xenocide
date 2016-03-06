#region Copyright
/*
--------------------------------------------------------------------------------
This source file is part of Xenocide
  by  Project Xenocide Team

For the latest info on Xenocide, see http://www.projectxenocide.com/

This work is licensed under the Creative Commons
Attribution-NonCommercial-ShareAlike 2.5 License.

To view a copy of this license, visit
http://creativecommons.org/licenses/by-nc-sa/2.5/
or send a letter to Creative Commons, 543 Howard Street, 5th Floor,
San Francisco, California, 94105, USA.
--------------------------------------------------------------------------------
*/

/*
* @file BattleLog.cs
* @date Created: 2007/07/23
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using ProjectXenocide.Utils;

using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Items;

#endregion

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// Record the events that happen during a battle
    /// </summary>
    public class BattleLog
    {
        /// <summary>
        /// Record an event in the log
        /// </summary>
        /// <param name="details">String to use to format the details of the event</param>
        /// <param name="args">the rest of the details of the event</param>
        public void Record(string details, params Object[] args)
        {
            entries.Add(new LogEntry(now, Util.StringFormat(details, args)));
        }

        /// <summary>
        /// Advance time by specified iterval
        /// </summary>
        /// <param name="seconds">Number of seconds to advance time by</param>
        public void UpdateTime(double seconds)
        {
            now += seconds;
        }

        /// <summary>
        /// an event in the log
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible",
            Justification="we can handle nested classes")]
        public class LogEntry
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="time">Time the event occured at</param>
            /// <param name="details">What happened</param>
            public LogEntry(double time, String details)
            {
                this.time = time;
                this.details = details;
            }

            /// <summary>
            /// Time the event occured at
            /// </summary>
            public double Time { get { return time; } }

            /// <summary>
            /// What happened
            /// </summary>
            public String Details { get { return details; } }

            /// <summary>
            /// Time the event occured at
            /// </summary>
            private double time;

            /// <summary>
            /// What happened
            /// </summary>
            private String details;
        }

        #region Fields

        /// <summary>
        /// The time, according to the log
        /// </summary>
        public double Now { get { return now; } }

        /// <summary>
        /// The events that have been recorded
        /// </summary>
        public IList<LogEntry> Entries { get { return entries; } }

        /// <summary>
        /// The time, according to the log
        /// </summary>
        private double now;

        /// <summary>
        /// The events that have been recorded
        /// </summary>
        private List<LogEntry> entries = new List<LogEntry>();

        #endregion
    }
}
