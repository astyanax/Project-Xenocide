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
using Microsoft.Xna.Framework;

#endregion

namespace Xenocide.Model.Geoscape
{
    public delegate void TimeEventCompletedHandler(ScheduledTimeEvent e);

    [Serializable]
    public class ScheduledTimeEvent
    {
        public ScheduledTimeEvent()
        {
            multiplier = 1.0;
        }

        public TimeEventCompletedHandler EventCompleted;
        
        private TimeSpan duration;
        private double multiplier;

        public bool Advance(double milliseconds)
        {
            duration.Subtract(new TimeSpan(0,0,0,0,(int)(milliseconds*multiplier)));
            if (duration <= TimeSpan.Zero)
            {
                if (EventCompleted != null)
                    EventCompleted(this);
                return true;
            }
            else
            {
                return false;
            }
        }

        public TimeSpan TimeLeft
        {
            get
            {
                return new TimeSpan((long)((double)duration.Ticks/multiplier));
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return duration;
            }
            set
            {
                duration = value;
            }
        }

        public double Multiplier
        {
            get
            {
                return multiplier;
            }
            set
            {
                multiplier = value;
            }
        }
    }

    /// <summary>
    /// Holds most of the stuff relating to progress of time in Geoscape
    /// </summary>
    /// RK: Revisit this as a Simulation chain of ITimer objects with manual updating and/or
    ///     scheduling. If you need more information I have already code to handle that I can
    ///     provide you as example (it is an already proven approach in Simulation code at work) ;)
    [Serializable]
    public class GeoTime : GameStateComponent
    {
        public GeoTime()
        {
            time = gameStartTime;
            timeRatio = 0.0f;
        }

        protected override void OnGameSet()
        {
            ((IGeoTimeService)Game.Services.GetService(typeof(IGeoTimeService))).GeoTime = this;
        }

        /// <summary>
        /// "Now" on the geoscape
        /// </summary>
        public DateTime Time { get { return time; } set { time = value; } }

        /// <summary>
        /// Ratio of geoscape time to real time
        /// </summary>
        public float TimeRatio { get { return timeRatio; } set { timeRatio = value; } }

        public void Update(GameTime gameTime)
        {
            double gameMilliseconds = RealTimeToGameTime(gameTime.ElapsedRealTime);
            // and at this point we will loop, 
            // so we don't updating the model in steps bigger than maxStep
            // note, this approach may underflow, when gameMilliseconds is too small
            // also, if any events have been queued for processing, we need to exit the loop
            // also, we don't spend more than 0.1 seconds on this processing each frame
            DateTime start = DateTime.Now;
            TimeSpan maxProcessing = new TimeSpan(1000000);
            do
            {
                double step = gameMilliseconds;
                if (maxTimeStep < gameMilliseconds)
                {
                    step = maxTimeStep;
                }
                AddMilliseconds(step);
                //TODO: move this update into a delegate
                //Overmind.Update(step);

                //TODO: move HumanBaseUpdate into event-scheduler, or something else
                // can't use foreach, because tasks may be removed from collection
                /*
                for (int i = HumanBases.Count - 1; 0 <= i; --i)
                {
                    HumanBases[i].Update(step);
                }
                 */
                for (int i = 0; i < timeEvents.Count; i++)
                {
                    if (timeEvents[i].Advance(step))
                    {
                        timeEvents.RemoveAt(i);
                        --i;
                        if (i < 0)
                            break;
                    }
                }
                gameMilliseconds -= step;
            }
            while ((0.0 < gameMilliseconds) &&
                ((DateTime.Now - start) < maxProcessing));
        }

        /// <summary>
        /// Advance the geoscape's time by supplied milliseconds
        /// </summary>
        /// <param name="milliseconds">Game time milliseconds</param>
        public void AddMilliseconds(double milliseconds)
        {
            time = time.AddMilliseconds(milliseconds);
        }

        /// <summary>
        /// Convert a real time time span into equivelent game milliseconds
        /// </summary>
        /// <param name="timespan"></param>
        /// <returns></returns>
        private double RealTimeToGameTime(TimeSpan timespan)
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

        /// <summary>
        /// Not allowed to update GameState in steps larger than 60 seconds
        /// </summary>
        private const double maxTimeStep = 60000;

        private List<ScheduledTimeEvent> timeEvents;
        public IList<ScheduledTimeEvent> TimeEvents
        {
            get
            {
                return timeEvents;
            }
        }
    }
}
