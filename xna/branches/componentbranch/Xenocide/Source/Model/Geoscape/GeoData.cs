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
    public class GeoData : GameStateComponent
    {
        public GeoData()
        {
            researchGraph = new ResearchGraph(Xenocide.StaticTables.DataDirectory + "research.xml");
            geoTime = new GeoTime();
        }

        protected override void OnGameSet()
        {
            geoTime.Game = Game;
            researchGraph.Game = Game;
            ((IHumanBaseService)Game.Services.GetService(typeof(IHumanBaseService))).HumanBases = humanBases;
        }

        /// <summary>
        /// "Now" on the geoscape
        /// </summary>
        //public GeoTime GeoTime { get { return geoTime; } }

        /// <summary>
        /// The Alien overmind
        /// </summary>
        public Overmind Overmind { get { return overmind; } }
        
        /// <summary>
        /// Update the model by supplied number of real time seconds
        /// </summary>
        /// <param name="gameTime">Real time seconds</param>
        /*
        public void Update(GameTime gameTime)
        {
            // if there are any events queued for processing
            // all we do this update cycle is process the topmost item in the queue
            if (0 < eventQueue.Count)
            {
                //TODO: this will have to be reviewed
                //ScreenManager.AssertNoDialogsQueuedOrShowing();
                Util.GeoTimeDebugWriteLine("Dequeuing event {0}", eventQueue.Peek().GetType().Name);
                eventQueue.Dequeue().Process();
            }
            else
            {
                PumpUpdate(gameTime);
            }
        }
         */
            
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
        private GeoTime geoTime;

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

    }
}
