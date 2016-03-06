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
* @file ResearchMission.cs
* @date Created: 2007/02/10
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Xenocide.Model.Geoscape.Craft
{
    /// <summary>
    /// Mission where
    /// 1. UFO starts at a random location on the globe
    /// 2. UFO proceeds to target site
    /// 3. At target site, UFO lands, and waits for a bit. (How long is "a bit"?)
    /// 4. UFO takes off again, moves short, random distance away
    /// 5. Lands and waits for a bit.
    /// 6. Steps 4 and 5 are repeated a number of times
    /// 7. UFO moves to departure point
    /// 8. UFO leaves Earth
    /// If UFO is disturbed at a landing site, it will proceed to departure point and leave earth.
    /// If UFO is attacked while in the air, and not destroyed, it will continue on its mission.
    /// </summary>
    [Serializable]
    public class ResearchMission : Mission
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="craft">craft that "owns" this mission</param>
        /// <param name="target">The geoposition UFO is to research</param>
        public ResearchMission(Craft craft, GeoPosition target)
            :
            base(craft)
        {
            this.target = new GeoPosition(target);
            landings = CalcNumberLandings();
            SetState(new MoveToPositionState(this, target));
        }

        /// <summary>
        /// Craft has been at a "sample site" for long enough
        /// </summary>
        public override void OnTimerFinished()
        {
            SetState(new MoveToPositionState(this, CalcNewDestination()));
        }

        /// <summary>
        /// We've reached a destination, so either land and sample it or we're done
        /// </summary>
        public override void OnDestinationReached()
        {
            if (0 < landings)
            {
                // "sample" this site
                SetState(new WaitState(this, CalcSecondsOnGround()));
                landings--;
            }
            else
            {
                // tell owner we're done
                Craft.OnMissionFinished();
            }
        }

        /// <summary>
        /// Respond to this craft finishing a dogfight with another craft
        /// </summary>
        public override void OnDogfightFinished()
        {
            //ToDo: Fully implement

            // currently this is just a stub, assumes the UFO lost the dogfight and crashed
            // it will remain on ground for 12 hours before repairs are completed and it flys away
            landings = 0;
            SetState(new WaitState(this, 12 * 3600));
        }

        /// <summary>
        /// Number of landings UFO is to do around the research site
        /// </summary>
        /// <returns>Number of landings</returns>
        private static int CalcNumberLandings()
        {
            // ToDo, at moment this is hard coded, may replace with RNG later
            return 3;
        }

        /// <summary>
        /// Length of time UFO is to remain on ground when it lands
        /// </summary>
        /// <returns>Time on ground, in seconds</returns>
        private static double CalcSecondsOnGround()
        {
            // ToDo, at moment this is hard coded, may replace with RNG later
            // 2 hours
            return 2.0 * 60.0 * 60.0;
        }

        /// <summary>
        /// Figure out where UFO should go next
        /// </summary>
        /// <returns>Where UFO should go next</returns>
        private GeoPosition CalcNewDestination()
        {
            double distance = 0.0;
            if (0 < landings)
            {
                // We've got another sampling to do, pick somewhere around target location
                distance = GeoPosition.KilometersToRadians(1000 + Xenocide.Rng.Next(1000));
            }
            else
            {
                // We've finished "sampling" so depart earth, exit 1/4 world away
                distance = (float)Math.PI / 2.0f;
            }
            return GeoPosition.RandomLocation(target, distance);
        }

        #region Fields

        /// <summary>
        /// The geoposition UFO is to research
        /// </summary>
        private GeoPosition target;

        /// <summary>
        /// Number of landings the UFO has left to do
        /// </summary>
        private int landings;

        #endregion
    }
}
