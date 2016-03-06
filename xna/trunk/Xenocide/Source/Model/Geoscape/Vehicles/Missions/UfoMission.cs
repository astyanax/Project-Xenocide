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
* @file UfoMission.cs
* @date Created: 2007/08/27
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using ProjectXenocide.Model.Geoscape.Geography;

#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// Base class that all UFO missions derive from
    /// 1. UFO starts at a random location on the globe
    /// 2. UFO proceeds to target site
    /// 3. At target site, UFO lands, and waits for a bit. (How long is "a bit"?)
    /// 4. UFO takes off again, moves short, random distance away
    /// 5. Lands and waits for a bit.
    /// 6. Steps 4 and 5 are repeated a number of times
    /// 7. UFO moves to final destination (which depends on mission goals)
    /// 7. UFO moves to departure point
    /// 8. UFO leaves Earth
    /// If UFO is disturbed at a landing site, it will proceed to departure point and leave earth.
    /// If UFO is attacked while in the air, and not destroyed, it will continue on its mission.
    /// </summary>
    [Serializable]
    public abstract class UfoMission : Mission
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="craft">craft that "owns" this mission</param>
        /// <param name="target">The center of the area the UFO is targeting</param>
        /// <param name="landings">Number of times the craft is to land</param>
        /// <param name="sublandings">Number of points the UFO will visit between landings</param>
        protected UfoMission(Craft craft, GeoPosition target, int landings, int subLandings)
            :
            base(craft)
        {
            this.target   = new GeoPosition(target);
            this.landings = landings;
            this.subLandings = subLandings;
            this.baseSubLandings = subLandings;
            SetState(new MoveToPositionState(this, target, MissionState.MoveToPositionStateType.LandingDestination, craft.MaxSpeed));
        }

        /// <summary>
        /// Craft has been at a "sample site" for long enough
        /// </summary>
        public override void OnTimerFinished()
        {
            var stateType = MissionState.MoveToPositionStateType.LandingDestination;
            SetState(new MoveToPositionState(this, CalcNewDestination(stateType), stateType,Craft.MaxSpeed));
        }

        /// <summary>
        /// We've reached a destination, so either land and sample it or we're done
        /// </summary>
        public override void OnDestinationReached()
        {
            ProjectXenocide.Utils.Util.GeoTimeDebugWriteLine("Destination {0}", landings);

            // if UFO is supposed to explore this area, start the "touring" the area.
            if (subLandings == baseSubLandings)
            {
                SetState(new MoveToPositionState(this,
                                                 CalcNewDestination(MissionState.MoveToPositionStateType.PointOfInterest),
                                                 MissionState.MoveToPositionStateType.PointOfInterest,
                                                 Craft.MaxSpeed * GetRandomSpeedModifier()));
                return;
            }

            if (1 < landings)
            {
                // "sample" this site
                SetState(new WaitState(this, CalcSecondsOnGround()));
                subLandings = baseSubLandings;
            }
            else if (1 == landings)
            {
                OnFinalLandingSiteReached();
            }
            else
            {
                // tell owner we're done
                Craft.OnMissionFinished();
            }
            --landings;
        }

        public override void OnSubDestinationReached()
        {
            ProjectXenocide.Utils.Util.GeoTimeDebugWriteLine("Point {0} of {1}", subLandings, baseSubLandings);
            if (subLandings > 1)
            {
                SetState(new MoveToPositionState(this,
                                                 CalcNewDestination(MissionState.MoveToPositionStateType.PointOfInterest),
                                                 MissionState.MoveToPositionStateType.PointOfInterest,
                                                 Craft.MaxSpeed * GetRandomSpeedModifier()));
            }
            else
            {
                SetState(new MoveToPositionState(this,
                                                 target,
                                                 MissionState.MoveToPositionStateType.LandingDestination,
                                                 Craft.MaxSpeed));
            }

            subLandings--;
        }
        /// <summary>
        /// Respond to this craft finishing a dogfight with another craft
        /// </summary>
        public override void OnDogfightFinished()
        {
            // if UFO lost the dogfight and crashed it will remain on ground for 
            // 12 hours before repairs are completed and it flys away
            // otherwise, it continues its mission
            if (Craft.IsCrashed)
            {
                landings = 0;
                SetState(new WaitState(this, 12 * 3600));
            }
        }

        /// <summary>
        /// Pick a random landing site somewhere near the target
        /// </summary>
        /// <returns>Landing site</returns>
        public GeoPosition PickRandomLandingSite(MissionState.MoveToPositionStateType state)
        {
            int range = (state == MissionState.MoveToPositionStateType.LandingDestination) ? 1000 : 250;
            int rndRange = Xenocide.Rng.Next(range);
            GeoPosition site = target.RandomLocationDistantBykm(range + rndRange);

            if (state == MissionState.MoveToPositionStateType.LandingDestination)
            {
                site = Xenocide.GameState.GeoData.Planet.GetClosestLand(site);
            }
            return site;
        }

        public double GetRandomSpeedModifier()
        {
            double minimumSpeed = 0.6;
            minimumSpeed += (Xenocide.Rng.Next(400))/1000f;
            return minimumSpeed;
        }

        /// <summary>
        /// Called when UFO reaches final landing site.  May be:
        /// City for terror mission
        /// X-Corp outpost if retaliation
        /// Alien Base if supply
        /// </summary>
        protected abstract void OnFinalLandingSiteReached();

        /// <summary>
        /// Called when UFO decides it's time to go for the final landing site.  May be:
        /// For most missions, just pick somewhere at random near the target
        /// City for terror mission
        /// X-Corp outpost if retaliation
        /// Alien Base if supply
        /// </summary>
        protected abstract GeoPosition OnCalcFinalLandingSite();

        /// <summary>
        /// Length of time UFO is to remain on ground when it lands
        /// </summary>
        /// <returns>Time on ground, in seconds</returns>
        protected static double CalcSecondsOnGround()
        {
            // ToDo, at moment this is hard coded, may replace with RNG later
            // 2 hours
            return 2.0 * 60.0 * 60.0;
        }

        /// <summary>
        /// Figure out where UFO should go next
        /// </summary>
        /// <returns>Where UFO should go next</returns>
        private GeoPosition CalcNewDestination(MissionState.MoveToPositionStateType stateType)
        {
            if (stateType == MissionState.MoveToPositionStateType.PointOfInterest)
            {
                return PickRandomLandingSite(stateType);
            }

            if (1 < landings)
            {
                // We've got another sampling to do, pick somewhere around target location
                return PickRandomLandingSite(stateType);
            }
            if (1 == landings)
            {
                return OnCalcFinalLandingSite();
            }
            else
            {
                // We've finished, so depart earth, exit 1/4 world away
                return target.RandomLocation((float)Math.PI / 2.0f);
            }
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

        /// <summary>
        /// Number of investagative sub stops a UFO makes prior to landing
        /// </summary>
        private int subLandings;

        /// <summary>
        /// the original amount of subLandings specifed.  When we target a new main destination,
        /// we need to set back to this amount
        /// </summary>
        private int baseSubLandings;
        #endregion
    }
}
