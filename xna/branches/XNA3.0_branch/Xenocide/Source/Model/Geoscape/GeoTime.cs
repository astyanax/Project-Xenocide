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
* @file GeoTime.cs
* @date Created: 2007/01/28
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

using ProjectXenocide.Model.Geoscape.GeoEvents;


#endregion

namespace ProjectXenocide.Model.Geoscape
{
    /// <summary>
    /// Holds most of the stuff relating to progress of time in Geoscape
    /// </summary>
    /// RK: Revisit this as a Simulation chain of ITimer objects with manual updating and/or
    ///     scheduling. If you need more information I have already code to handle that I can
    ///     provide you as example (it is an already proven approach in Simulation code at work) ;)
    [Serializable]
    public class GeoTime
    {
        /// <summary>
        /// Set to "start of new game" condition
        /// </summary>
        public void SetToStartGameCondition()
        {
            time = gameStartTime;
            timeRatio = 0.0f;

            // schedule repeating events
            scheduler = new Scheduler();
            repeatingActivities = new RepeatingActivities(time);
        }

        /// <summary>
        /// Advance the geoscape's time by supplied milliseconds
        /// </summary>
        /// <param name="milliseconds">Game time milliseconds</param>
        public void AddMilliseconds(double milliseconds)
        {
            time = time.AddMilliseconds(milliseconds);
            Scheduler.Update(time);
            repeatingActivities.Process(time);
        }

        /// <summary>
        /// Convert a real time time span into equivelent game milliseconds
        /// </summary>
        /// <param name="timespan"></param>
        /// <returns></returns>
        public double RealTimeToGameTime(TimeSpan timespan)
        {
            return timespan.TotalMilliseconds * timeRatio;
        }
        
        /// <summary>
        /// Get Geoscape's "now" in a format suitable for display
        /// </summary>
        /// <returns>Geoscape's "now" in a format suitable for display</returns>
        public override string ToString() 
        { 
            return time.ToString("yyyy-MM-dd HH:mm:ss", Thread.CurrentThread.CurrentCulture); 
        }

        /// <summary>
        /// Stop the progress of game time
        /// </summary>
        public void StopTime()
        {
            timeRatio = 0.0f;
        }

        /// <summary>
        /// Add an appointment to the scheduler
        /// </summary>
        /// <remarks>Saves us the effort of pulling GeoTime's Time member</remarks>
        /// <param name="appointment">to add</param>
        public void Add(Appointment appointment)
        {
            Scheduler.Add(appointment);
        }

        /// <summary>
        /// Create and schedule a delegating appointment
        /// </summary>
        /// <param name="delay">how long from now will the appointment occur</param>
        /// <param name="action">action to take</param>
        /// <returns>appointment that has been created</returns>
        public DelegatingAppointment MakeAppointment(TimeSpan delay, DelegatingAppointment.ProcessAction action)
        {
            DelegatingAppointment appointment = new DelegatingAppointment(Time + delay, action);
            Add(appointment);
            return appointment;
        }

        /// <summary>What day of the campaign is this?</summary>
        /// <returns>Day of compaign, zero based</returns>
        public int DayNumber()
        {
            return (int)(((time - gameStartTime) + new TimeSpan(12, 0, 0)).TotalDays) + 1;
        }

        #region Fields

        /// <summary>
        /// "Now" on the geoscape
        /// </summary>
        public DateTime Time { get { return time; } }

        /// <summary>
        /// Ratio of geoscape time to real time
        /// </summary>
        public float TimeRatio { get { return timeRatio; } set { timeRatio = value; } }

        /// <summary>
        /// Events that will occur at known point in the future
        /// </summary>
        public Scheduler Scheduler { get { return scheduler; } }

        /// <summary>
        /// "Now" on the geoscape
        /// </summary>
        private DateTime time = gameStartTime;

        /// <summary>
        /// This is time the game "starts" at
        /// </summary>
        private static readonly DateTime gameStartTime = new DateTime(2020, 1, 1, 12, 0, 0);

        /// <summary>
        /// Ratio of geoscape time to real time
        /// </summary>
        private float timeRatio;

        /// <summary>
        /// Events that will occur at known point in the future
        /// </summary>
        private Scheduler scheduler = new Scheduler();

        /// <summary>The activities that need to be done every 30 minutes on the Geoscape</summary>
        private RepeatingActivities repeatingActivities;

        #endregion Fields
    }
}
