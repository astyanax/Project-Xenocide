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
* @file InterceptCraftState.cs
* @date Created: 2007/02/18
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


using ProjectXenocide.Model.Geoscape.GeoEvents;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.Utils;
using ProjectXenocide.Model.Battlescape;

#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// State that represents Craft moving to intercept another craft
    /// </summary>
    [Serializable]
    public class InterceptCraftState : MissionState
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mission">mission that owns this state</param>
        /// <param name="target">craft we are trying to intercept</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Is validated in base class")]
        public InterceptCraftState(Mission mission, Craft target)
            :
            base(mission, mission.Craft.MaxSpeed)
        {
            this.target = target;
        }

        /// <summary>
        /// Anything that needs to be done when mission enters this state
        /// </summary>
        public override void OnEnterState() 
        {
            // Tell target we're hunting it
            base.OnEnterState();
            target.AddHunter(Mission.Craft);
        }

        /// <summary>
        /// Anything that needs to be done when mission leaves this state
        /// </summary>
        public override void OnExitState() 
        {
            // Tell target we're no longer hunting it
            base.OnExitState();
            target.RemoveHunter(Mission.Craft);
        }

        /// <summary>
        /// Respond to a craft on the Geoscape being destroyed
        /// </summary>
        /// <param name="destroyedCraft">craft that's been destroyed</param>
        public override void OnCraftDestroyed(Craft destroyedCraft)
        {
            // if it's the target, then return to base, otherwise ignore
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
            // same behaviour as OnPreyGone
            OnPreyGone();
        }

        /// <summary>
        /// Respond to prey escaping
        /// </summary>
        public override void OnPreyGone()
        {
            // Default behaviour is return to base
            Mission.SetState(new ReturnToBaseState(Mission));

            // schedule event, to give user chance to change mission/state
            Xenocide.GameState.GeoData.QueueEvent(
                new TrackingLostGeoEvent(target.Position, Mission.Craft)
            );

            // and remove from prey
            target = null;
        }

        /// <summary>
        /// Respond to craft running low on fuel
        /// </summary>
        public override void OnFuelLow() 
        {
            // if we're low on fuel, must "return to base"
            Xenocide.GameState.GeoData.QueueEvent(new FuelLowGeoEvent(Mission.Craft));
            Mission.SetState(new ReturnToBaseState(Mission));
        }

        /// <summary>
        /// Respond to this craft reaching position where it can attack another craft
        /// </summary>
        public override void OnInAttackRange()
        {
            GeoEvent geoevent = null;
            // if target is on ground && we're carrying troops, offer battlescape mission
            // else, start shooing at UFO (if we're armed.)
            if ((0 == target.Mission.State.CurrentSpeed) && Mission.Craft.IsCarryingSoldiers)
            {
                UfoSiteMission battlescapeMission = new UfoSiteMission(target, Mission.Craft);
                geoevent = new StartBattlescapeGeoEvent(battlescapeMission);
            }
            else if (Mission.Craft.IsArmed)
            {
                geoevent = new StartAeroscapeGeoEvent(Mission.Craft, target);
            }
            if (null != geoevent)
            {
                Xenocide.GameState.GeoData.QueueEvent(geoevent);
            }
        }

        /// <summary>
        /// Respond to this craft finishing a dogfight with another craft
        /// </summary>
        public override void OnDogfightFinished()
        {
            Mission.SetState(new ReturnToBaseState(Mission));
        }

        /// <summary>
        /// Change the craft, based on time elapsed
        /// </summary>
        /// <param name="milliseconds">Time that has passed</param>
        protected override void UpdateState(double milliseconds)
        {
            Craft craft = Mission.Craft;
            Vector2 intercept = CalcInterceptCourse(craft, target);

            // figure out how far we can travel in this time slice
            double distance = craft.MaxSpeed * milliseconds / 1000.0;

            // now move craft towards target, or put it AT target
            if (intercept.Y <= distance)
            {
                Util.GeoTimeDebugWriteLine("{0} has reached attack range of prey", craft.Name);
                craft.Position = target.Position;
                Mission.OnInAttackRange();
            }
            else
            {
                craft.Position = craft.Position.GetEndpoint(intercept.X, distance);
                if (!craft.ConsumeFuel(milliseconds))
                {
                    Util.GeoTimeDebugWriteLine("{0} is low on fuel", craft.Name);
                    Mission.OnFuelLow();
                }
            }
        }

        /// <summary>
        /// Compute distance and azimuth for hunter's intercept course
        /// </summary>
        /// <param name="hunter">Craft trying to do the intercept</param>
        /// <param name="prey">Craft being intercepted</param>
        /// <returns>Intercept course, in form azimuth, distance</returns>
        /// <remarks>at moment, is pretty stupid, it heads for where prey currently is</remarks>
        private static Vector2 CalcInterceptCourse(Craft hunter, Craft prey)
        {
            // get azimuth and distance to destination.
            float distance = hunter.Position.Distance(prey.Position);
            float azimuth = hunter.Position.GetAzimuth(prey.Position);
            return new Vector2(azimuth, distance);
        }

        /// <summary>
        /// The craft we are trying to intercept
        /// </summary>
        private Craft target { get { return Mission.Craft.Prey; } set { Mission.Craft.Prey = value; } }
    }
}
