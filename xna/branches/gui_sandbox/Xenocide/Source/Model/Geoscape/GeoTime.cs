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

#endregion

namespace Xenocide.Model.Geoscape
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
        /// "Now" on the geoscape
        /// </summary>
        public DateTime Time { get { return time; } }

        /// <summary>
        /// Ratio of geoscape time to real time
        /// </summary>
        public float TimeRatio { get { return timeRatio; } set { timeRatio = value; } }

        /// <summary>
        /// Set to "start of new game" condition
        /// </summary>
        public void SetToStartGameCondition()
        {
            time = gameStartTime;
            timeRatio = 0.0f;
        }

        /// <summary>
        /// Advance the geoscape's time by supplied milliseconds
        /// </summary>
        /// <param name="milliseconds">Game time milliseconds</param>
        public void AddMilliseconds(double milliseconds)
        {
            DateTime oldTime = time;
            time = time.AddMilliseconds(milliseconds);

            if (oldTime.DayOfYear != time.DayOfYear)
            {
                TimeSpan span = time.Subtract(oldTime);
                int days = span.Days + 1;
                while (days-- > 0)
                {
                    System.Console.WriteLine("Day passed");
                    DayPassed();
                }
            }
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
        /// Delegate definition of callbacks that are called each "gameday"
        /// </summary>
        public delegate void DayPassedHandler();

        /// <summary>
        /// Delegate for "gameday" callbacks
        /// </summary>
        public DayPassedHandler DayPassed;

        /// <summary>
        /// "Now" on the geoscape
        /// </summary>
        private DateTime time = gameStartTime;

        /// <summary>
        /// This is time the game "starts" at
        /// </summary>
        private static DateTime gameStartTime = new DateTime(2020, 1, 1);
        
        /// <summary>
        /// Ratio of geoscape time to real time
        /// </summary>
        private float timeRatio;
    }
}
