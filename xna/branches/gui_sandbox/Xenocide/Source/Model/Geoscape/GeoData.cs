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
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using Xenocide.Model.Geoscape.HumanBases;
using Xenocide.Model.Geoscape.Craft;
using Xenocide.Model.Geoscape.AI;
using Xenocide.Model.Geoscape.GeoEvents;
using Xenocide.Utils;
using Xenocide.Model.Geoscape.Research;


#endregion

namespace Xenocide.Model.Geoscape
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
        public void AddHumanBase(float longitude, float latitude, String name)
        {
            GeoPosition position  = new GeoPosition(longitude, latitude);
            HumanBase   humanBase = new HumanBase(position, name);
            humanBases.Add(humanBase);
        }

        /// <summary>
        /// Set to "start of new game" condition
        /// </summary>
        public void SetToStartGameCondition()
        {
            GeoTime.SetToStartGameCondition();
            Overmind.SetToStartGameCondition();
            SetPlayerToStartGameCondition();
        }

        /// <summary>
        /// "Now" on the geoscape
        /// </summary>
        public GeoTime GeoTime { get { return geoTime; } }

        /// <summary>
        /// All the bases owned by X-Corp
        /// </summary>
        public IList<HumanBase> HumanBases { get { return humanBases; } }

        /// <summary>
        /// The current ResearchGraph
        /// </summary>
        public ResearchGraph ResearchGraph { get { return researchGraph; } }

        /// <summary>
        /// The Alien overmind
        /// </summary>
        public Overmind Overmind { get { return overmind; } }
        
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
            double gameMilliseconds = GeoTime.RealTimeToGameTime(gameTime.ElapsedRealTime);
            // and at this point we will loop, 
            // so we don't updating the model in steps bigger than maxStep
            // note, this approach may underflow, when gameMilliseconds is too small
            // also, if any events have been queued for processing, we need to exit the loop
            do
            {
                double step = gameMilliseconds;
                if (maxTimeStep < gameMilliseconds)
                {
                    step = maxTimeStep;
                }
                geoTime.AddMilliseconds(step);
                Overmind.Update(step);
                
                // can't use foreach, because tasks may be removed from collection
                for (int i = HumanBases.Count - 1; 0 <= i; --i)
                {
                    HumanBases[i].Update(step);
                }

                //researchGraph.Update(gameMilliseconds);
                gameMilliseconds -= step;
            }
            while ((0.0 < gameMilliseconds) && (0 == eventQueue.Count));
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
        /// "Now" on the geoscape
        /// </summary>
        private GeoTime geoTime = new GeoTime();

        /// <summary>
        /// All the bases owned by X-Corp
        /// </summary>
        private List<HumanBase> humanBases = new List<HumanBase>();

        private ResearchGraph researchGraph;
        
        /// <summary>
        /// The Alien overmind
        /// </summary>
        private Overmind overmind = new Overmind();

        /// <summary>
        /// Events that have occured on the Geoscape and are waiting to be processed
        /// </summary>
        private Queue<GeoEvent> eventQueue = new Queue<GeoEvent>();

        /// <summary>
        /// Not allowed to update GameState in steps larger than 60 seconds
        /// </summary>
        private const double maxTimeStep = 60000;

        #region Test code

        /// <summary>
        /// Set the Player's forces to their "start of game" condition
        /// </summary>
        private void SetPlayerToStartGameCondition()
        {
            HumanBases.Clear();
            researchGraph = new ResearchGraph(Xenocide.StaticTables.DataDirectory + "research.xml");
        }

        #endregion
    }
}
