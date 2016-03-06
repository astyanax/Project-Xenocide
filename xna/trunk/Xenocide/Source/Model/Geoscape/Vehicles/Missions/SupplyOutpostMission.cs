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
* @file SupplyOutpostMission.cs
* @date Created: 2007/08/27
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
    /// 2. UFO proceeds to within 20 km of Alien Outpost
    /// 3. At target site, UFO lands, and waits for a bit. (How long is "a bit"?)
    /// 4. UFO moves to departure point
    /// 5. UFO leaves Earth
    /// If UFO is disturbed at a landing site, it will proceed to departure point and leave earth.
    /// If UFO is attacked while in the air, and not destroyed, it will continue on its mission.
    /// Mission is success if UFO reaches landing site and finishes unloading
    /// </summary>
    /// <remarks>This mission also serves as an Outpost Construction mission</remarks>
    [Serializable]
    public class SupplyOutpostMission : UfoMission
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="craft">craft that "owns" this mission</param>
        /// <param name="target">The center of the area the UFO is targeting</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if target is null")]
        public SupplyOutpostMission(Craft craft, GeoPosition target)
            :
            base(craft, target.RandomLocationDistantBykm(20), 1,0)
        {
        }

        /// <summary>
        /// Respond to timer finishing counting down.
        /// </summary>
        public override void OnTimerFinished()
        {
            // as the only timer is the unloading cargo timer, the cargo must have been unloaded
            unloadedCargo = true;
            base.OnTimerFinished();
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
            return PickRandomLandingSite(MissionState.MoveToPositionStateType.LandingDestination);
        }

        #region Fields

        /// <summary>
        /// Was the UFO successful in its part of the mission?
        /// </summary>
        /// <remarks>If UFO dropped of its cargo, mission was a success</remarks>
        public override bool Success { get { return unloadedCargo; } }

        /// <summary>
        /// Did the craft unload it's cargo (if it was carrying cargo)
        /// </summary>
        private bool unloadedCargo;

        #endregion
    }
}
