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

namespace ProjectXenocide.Model.Geoscape.Vehicles
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
    /// <remarks>This behaviour is same for harvest, abduction and infiltration</remarks>
    [Serializable]
    public class ResearchMission : UfoMission
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="craft">craft that "owns" this mission</param>
        /// <param name="target">The center of the area the UFO is targeting</param>
        /// <param name="landings">Number of times the craft is to land</param>
        public ResearchMission(Craft craft, GeoPosition target, int landings)
            :
            base(craft, target, landings)
        {
        }

        /// <summary>
        /// Called when UFO reaches final landing site.  May be:
        /// </summary>
        protected override void OnFinalLandingSiteReached()
        {
            // for most mision types, land
            SetState(new WaitState(this, CalcSecondsOnGround()));
        }

        /// <summary>
        /// Called when UFO decides it's time to go for the final landing site.  May be:
        /// </summary>
        protected override GeoPosition OnCalcFinalLandingSite()
        {
            // for most missions, just pick yet another landing site
            return PickRandomLandingSite();
        }
    }
}
