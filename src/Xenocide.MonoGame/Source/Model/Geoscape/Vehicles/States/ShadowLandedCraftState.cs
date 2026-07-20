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
* @file ShadowLandedCraftState.cs
* @date Created: 2026/07/20
* @author File creator: Xenocide Agent
* @author Credits: none
*/
#endregion

#region Using Statements

using System;

using ProjectXenocide.Model.Geoscape.GeoEvents;
using ProjectXenocide.Utils;

#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// State where an armed craft shadows a landed UFO, waiting for it to launch.
    /// When the UFO takes off, the craft immediately re-engages (triggers dogfight).
    /// </summary>
    /// <remarks>
    /// This replaces the old behavior of switching to PatrolState when an armed
    /// interceptor reaches a landed UFO. PatrolState had no reference to the UFO
    /// and no mechanism to detect the UFO launching, so the interceptor would
    /// loiter until fuel ran out while the UFO escaped.
    /// </remarks>
    [Serializable]
    public class ShadowLandedCraftState : MissionState
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mission">mission that owns this state</param>
        /// <param name="target">the landed UFO being shadowed</param>
        public ShadowLandedCraftState(Mission mission, Craft target)
            :
            base(mission, 0.0)
        {
            this.target = target;
        }

        /// <summary>
        /// Re-register as hunter when entering this state.
        /// </summary>
        public override void OnEnterState()
        {
            base.OnEnterState();
            target.AddHunter(Mission.Craft);
        }

        /// <summary>
        /// Remove hunter registration when leaving.
        /// </summary>
        public override void OnExitState()
        {
            base.OnExitState();
            target.RemoveHunter(Mission.Craft);
        }

        /// <summary>
        /// Respond to a craft on the Geoscape being destroyed
        /// </summary>
        public override void OnCraftDestroyed(Craft destroyedCraft)
        {
            if (destroyedCraft == target)
            {
                Mission.SetState(new ReturnToBaseState(Mission));
            }
        }

        /// <summary>
        /// Respond to this craft loosing sight of the craft it is hunting
        /// </summary>
        public override void OnPreyTrackingLost()
        {
            OnPreyGone();
        }

        /// <summary>
        /// Respond to prey escaping
        /// </summary>
        public override void OnPreyGone()
        {
            Mission.SetState(new ReturnToBaseState(Mission));
            Xenocide.GameState.GeoData.QueueEvent(
                new TrackingLostGeoEvent(target.Position, Mission.Craft)
            );
            target = null;
        }

        /// <summary>
        /// Respond to craft running low on fuel
        /// </summary>
        public override void OnFuelLow()
        {
            Xenocide.GameState.GeoData.QueueEvent(new FuelLowGeoEvent(Mission.Craft));
            Mission.SetState(new ReturnToBaseState(Mission));
        }

        /// <summary>
        /// Respond to dogfight finishing (after UFO launched and dogfight completed)
        /// </summary>
        public override void OnDogfightFinished()
        {
            Mission.SetState(new ReturnToBaseState(Mission));
        }

        /// <summary>
        /// Hold position near the landed UFO. Each tick, check if the UFO has launched.
        /// If it has, transition back to InterceptCraftState which will immediately
        /// trigger OnInAttackRange() and start the dogfight.
        /// </summary>
        protected override void UpdateState(double milliseconds)
        {
            Craft craft = Mission.Craft;

            // Stay at the UFO's position
            craft.Position = target.Position;

            // Consume fuel while loitering
            if (!craft.ConsumeFuel(milliseconds))
            {
                Mission.OnFuelLow();
                return;
            }

            // Check if UFO has launched (no longer landed)
            if (!target.Mission.IsLanded)
            {
                Util.GeoTimeDebugWriteLine("{0} detects {1} has launched, re-engaging",
                    craft.Name, target.Name);
                Mission.SetState(new InterceptCraftState(Mission, target));
            }
        }

        /// <summary>
        /// The landed UFO being shadowed
        /// </summary>
        private Craft target;
    }
}
