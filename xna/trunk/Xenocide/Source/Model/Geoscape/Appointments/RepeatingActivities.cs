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
* @file RepeatingActivities.cs
* @date Created: 2007/08/07
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using CeGui;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.Model.Geoscape.GeoEvents;
using ProjectXenocide.Model.StaticData;

#endregion

namespace ProjectXenocide.Model
{
    /// <summary>
    /// The activities that need to be done every 30 minutes on the Geoscape
    /// </summary>
    [Serializable]
    public class RepeatingActivities
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="now">The current Geoscape time</param>
        public RepeatingActivities(DateTime now)
        {
            nextEventTime = now + ThirtyMinutes;

            // number of 30 minute intervals until tomorrow
            Debug.Assert((0 == now.Minute) && (0 == now.Second));
            newDayCounter = (24 - now.Hour) * 2;

            // and allow us to know when next month comes around
            lastMonth = Xenocide.GameState.GeoData.GeoTime.Time.Month;
        }

        /// <summary>
        /// The activities that need to be done every 30 minutes
        /// </summary>
        /// <param name="now">The current Geoscape time</param>
        public void Process(DateTime now)
        {
            while (nextEventTime <= now)
            {
                nextEventTime += ThirtyMinutes;
                HalfHourActivity();
                if (0 == --newDayCounter)
                {
                    newDayCounter = 48;
                    StartOfDayActivity();
                }
            }
        }

        /// <summary>
        /// The activity that needs to be done every 30 minutes
        /// </summary>
        private static void HalfHourActivity()
        {
            foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
            {
                // update radars
                outpost.Statistics.UpdateRadarDetection();

                foreach (Craft craft in outpost.Fleet)
                {
                    if (!craft.InBase)
                    {
                        // score for aircraft on geoscape
                        Xenocide.GameState.GeoData.AddScore(Participant.XCorp, StartSettings.CraftFlyingScore, craft.Position);

                        // check for undetected alien outposts
                        // note, in X-COM 1, don't always detect outposts immediately
                        (craft as Aircraft).LookForAlienOutposts();
                    }
                }
            }

            // score for UFOs on geoscape
            foreach (Ufo ufo in Xenocide.GameState.GeoData.Overmind.Ufos)
            {
                Xenocide.GameState.GeoData.AddScore(Participant.Alien, StartSettings.CraftFlyingScore, ufo.Position);
            }
        }

        /// <summary>
        /// The activity that needs to be done at the start of each day
        /// </summary>
        private void StartOfDayActivity()
        {
            Xenocide.GameState.GeoData.StartOfDayActivities();

            // check for start of month
            int thisMonth = Xenocide.GameState.GeoData.GeoTime.Time.Month;
            if (thisMonth != lastMonth)
            {
                lastMonth = thisMonth;
                StartOfMonthActivity();
            }
        }

        /// <summary>
        /// The activity that needs to be done at the start of each month
        /// </summary>
        private static void StartOfMonthActivity()
        {
            Xenocide.GameState.GeoData.StartOfMonth();
            Xenocide.GameState.GeoData.QueueEvent(new StartOfMonthGeoEvent());
        }

        #region Fields

        /// <summary>Interval between updates</summary>
        private static readonly TimeSpan ThirtyMinutes = new TimeSpan(0, 30, 0);

        /// <summary>Time we next need need to check for events</summary>
        private DateTime nextEventTime;

        /// <summary>
        /// Track number of 30 minute periods before the start of the next day
        /// </summary>
        private int newDayCounter;

        /// <summary>
        /// What was the previous month?
        /// </summary>
        private int lastMonth;

        #endregion Fields
    }
}
