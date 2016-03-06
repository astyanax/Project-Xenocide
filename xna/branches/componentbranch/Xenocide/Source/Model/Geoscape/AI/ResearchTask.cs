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
* @file ResearchTask.cs
* @date Created: 2007/02/11
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using Xenocide.Model.Geoscape;
using Xenocide.Model.Geoscape.Craft;

#endregion

namespace Xenocide.Model.Geoscape.AI
{
    /// <summary>
    /// Alien Overmind sending a series of UFOs to explore a region of the earth
    /// (Usually involves multiple UFOs)
    /// </summary>
    [Serializable]
    public class ResearchTask : InvasionTask
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="overmind">Overmind that owns this task</param>
        public ResearchTask(Overmind overmind)
            :
            base(overmind)
        {
            SpawnUfo();
        }

        /// <summary>
        /// Update the alien activity, to allow for passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        /// <remarks>At moment, just pump the Ufo.</remarks>
        public override void Update(double milliseconds)
        {
            // if there are UFOs, pump them
            if (0 < Ufos.Count)
            {
                base.Update(milliseconds);
            }
            else
            {
                // no UFOs, so countdown to next launch
                nextLaunch -= milliseconds / 1000.0;
                if (nextLaunch <= 0.0)
                {
                    SpawnUfo();

                    // reset for next launch
                    nextLaunch = launchInterval;
                }
            }
        }

        /// <summary>
        /// Called when UFO has finished the mission the task set it
        /// </summary>
        /// <param name="ufo">UFO that has finished the mission</param>
        override public void OnMissionFinished(Ufo ufo)
        {
            // we're done with that UFO
            RemoveUfo(ufo);
        }

        /// <summary>
        /// Called when a UFO has been destroyed (by enemy action)
        /// </summary>
        /// <param name="ufo">UFO that was destroyed</param>
        public override void OnUfoDestroyed(Ufo ufo)
        {
            // we're done with that UFO
            RemoveUfo(ufo);
        }

        /// <summary>
        /// Create a UFO to do a research mission
        /// </summary>
        private void SpawnUfo()
        {
            // start mission 
            GeoPosition start = GeoPosition.RandomLocation();
            double speed = GeoPosition.KilometersToRadians(2000.0) / 3600.0;   // 2000 kph (approx) in radians/sec
            Ufo ufo = new Ufo(speed, start, this);
            ufo.Mission = new ResearchMission(ufo, GeoPosition.RandomLocation(start, Math.PI / 2.0));
            AddUfo(ufo);
        }

        /// <summary>
        /// Time remaining before next UFO launch
        /// </summary>
        private double nextLaunch = launchInterval;

        /// <summary>
        /// Time between UFO missions
        /// </summary>
        private const double launchInterval = 6 * 3600;  // 6 hours between launches
    }
}
