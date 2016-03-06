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
* @file GeoData.cs
* @date Created: 2007/01/28
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.Model.Geoscape.GeoEvents;
using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape.Geography;


#endregion

namespace ProjectXenocide.Model.Geoscape
{
    /// <summary>
    /// Root class that holds all "geoscape" (as in model-view) data that is loaded/saved to file
    /// </summary>
    [Serializable]
    public class GeoData
    {
        /// <summary>
        /// Add a new base to the list of bases
        /// </summary>
        /// <param name="longitude">longitude of the base (in radians)</param>
        /// <param name="latitude">latitude of the base (in radians)</param>
        /// <param name="name">Name for the base</param>
        public void AddOutpost(float longitude, float latitude, String name)
        {
            GeoPosition position  = new GeoPosition(longitude, latitude);
            Outpost     outpost   = new Outpost(position, name);
            Outposts.Add(outpost);
        }

        /// <summary>
        /// Set to "start of new game" condition
        /// </summary>
        public void SetToStartGameCondition()
        {
            GeoTime.SetToStartGameCondition();
            overmind = new Overmind();
            SetPlayerToStartGameCondition();
            eventQueue.Clear();
        }

        /// <summary>Do Activities that need to be done at the start of each day</summary>
        public void StartOfDayActivities()
        {
            // Add alien outposts to overmind's score
            foreach (AlienSite site in Overmind.Sites)
            {
                Planet.AddScore(Participant.Alien, site.DailyScore, site.Position);
            }

            foreach (Outpost outpost in Outposts)
            {
                outpost.HealInjuredStaff();
            }
        }

        /// <summary>
        /// Update the model by supplied number of real time seconds
        /// </summary>
        /// <param name="gameTime">Real time seconds</param>
        public void Update(GameTime gameTime)
        {
            // if there are any events queued for processing
            // all we do this update cycle is process the topmost item in the queue
            if (0 < eventQueue.Count)
            {
                Xenocide.ScreenManager.AssertNoDialogsQueuedOrShowing();
                Util.GeoTimeDebugWriteLine("Dequeuing event {0}", eventQueue.Peek().GetType().Name);
                eventQueue.Dequeue().Process();
            }
            else
            {
                PumpUpdate(gameTime);
            }
        }
            
        /// <summary>
        /// Pump the update() call to all objects contained by the GeoData
        /// </summary>
        /// <param name="gameTime">Real time seconds</param>
        public void PumpUpdate(GameTime gameTime)
        {
            // get elapsed time plus any leftover time from last iteration
            double gameMilliseconds = GeoTime.RealTimeToGameTime(gameTime.ElapsedGameTime) + leftoverTimeStep;
            leftoverTimeStep = 0.0;

            // and at this point we will loop, 
            // so we don't updating the model in steps bigger than maxStep
            // note, this approach may underflow, when gameMilliseconds is too small
            // also, if any events have been queued for processing, we need to exit the loop
            // also, we don't spend more than 0.1 seconds on this processing each frame
            // also, don't want to increment time by steps smaller than minTimeStep (to minimise 
            // quantization errors where updates are sliced so small they round down to nothing)
            DateTime start = DateTime.Now;
            TimeSpan maxProcessing = new TimeSpan(1000000);
            do
            {
                double step = gameMilliseconds;
                if (maxTimeStep < gameMilliseconds)
                {
                    step = maxTimeStep;
                }
                else if (gameMilliseconds < minTimeStep)
                {
                    // save leftover time for next iteration
                    leftoverTimeStep = gameMilliseconds;
                    return;
                }
                geoTime.AddMilliseconds(step);
                Overmind.Update(step);
                
                // can't use foreach, because tasks may be removed from collection
                for (int i = Outposts.Count - 1; 0 <= i; --i)
                {
                    Outposts[i].Update(step);
                }

                //researchGraph.Update(gameMilliseconds);
                gameMilliseconds -= step;
            }
            while ((0.0 < gameMilliseconds) && (0 == eventQueue.Count) && 
                ((DateTime.Now - start) < maxProcessing));
        }

        /// <summary>
        /// Put event into the queue to be processed
        /// </summary>
        /// <param name="geoevent">event to put into queue</param>
        public void QueueEvent(GeoEvent geoevent)
        {
            Util.GeoTimeDebugWriteLine("Queuing event {0}", geoevent.GetType().Name);
            eventQueue.Enqueue(geoevent);
        }

        /// <summary>
        /// Do start of month processing
        /// </summary>
        public void StartOfMonth()
        {
            planet.StartOfMonth();
            xcorp.StartOfMonth();
            overmind.StartOfMonth();
        }

        /// <summary>
        /// Adjust score
        /// </summary>
        /// <param name="side">which side is being changed</param>
        /// <param name="points">size of the change</param>
        /// <param name="position">location on geoscape of event</param>
        public void AddScore(Participant side, float points, GeoPosition position)
        {
            // if occured at a location, record against location
            if (null != position)
            {
                // update points, because we may get charge against both region and country
                points = planet.AddScore(side, points, position);
            }
            // keep track of totals
            xcorp.AddScore(side, points);
        }

        #region Fields

        /// <summary>
        /// "Now" on the geoscape
        /// </summary>
        public GeoTime GeoTime { get { return geoTime; } }

        /// <summary>
        /// All the bases owned by X-Corp
        /// </summary>
        public IList<Outpost> Outposts { get { return outposts; } }

        /// <summary>
        /// The Alien overmind
        /// </summary>
        public Overmind Overmind { get { return overmind; } }

        /// <summary>
        /// The planet represented by this geoscape- contains region, country and city information
        /// </summary>
        public Planet Planet { get { return planet; } }

        /// <summary>
        /// X-Corp
        /// </summary>
        public XCorp XCorp { get { return xcorp; } }

        /// <summary>
        /// "Now" on the geoscape
        /// </summary>
        private GeoTime geoTime = new GeoTime();

        /// <summary>
        /// All the bases (outposts) owned by X-Corp
        /// </summary>
        private List<Outpost> outposts = new List<Outpost>();

        /// <summary>
        /// The Alien overmind
        /// </summary>
        private Overmind overmind = new Overmind();

        /// <summary>
        /// Events that have occured on the Geoscape and are waiting to be processed
        /// </summary>
        private Queue<GeoEvent> eventQueue = new Queue<GeoEvent>();

        /// <summary>
        /// The current planet represented by the Geoscape
        /// </summary>
        private Planet planet;

        /// <summary>
        /// X-Corp
        /// </summary>
        private XCorp xcorp;

        #endregion Fields

        /// <summary>
        /// Not allowed to update GameState in steps larger than 60 seconds
        /// </summary>
        private const double maxTimeStep = 60000;

        /// <summary>
        /// Not allowed to update GameState in steps smaller than 5 seconds
        /// </summary>
        private const double minTimeStep = 5000;

        /// <summary>
        /// Leftover amount of time when smaller than minimum update time
        /// </summary>
        private double leftoverTimeStep;

        #region Test code

        /// <summary>
        /// Set the Player's forces to their "start of game" condition
        /// </summary>
        private void SetPlayerToStartGameCondition()
        {
            Outposts.Clear();
            planet = PlanetParser.Parse(Xenocide.StaticTables.DataDirectory + "planets.xml");
            xcorp = new XCorp();
            xcorp.StartOfMonth();
        }

        #endregion
    }
}
