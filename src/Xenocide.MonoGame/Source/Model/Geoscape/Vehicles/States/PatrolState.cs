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
* @file PatrolState.cs
* @date Created: 2007/03/11
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using ProjectXenocide.Model.Geoscape.GeoEvents;
using ProjectXenocide.Utils;

#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// State that represents Craft moving to a position, then hanging around
    /// the position until it runs low on fuel
    /// </summary>
    [Serializable]
    public class PatrolState : MissionState
    {
        /// <summary>
        /// Constructor for a single patrol point
        /// </summary>
        /// <param name="mission">mission that owns this state</param>
        /// <param name="destination">position the craft is to patrol</param>
        public PatrolState(Mission mission, GeoPosition destination)
            :
            this(mission, new List<GeoPosition> { destination })
        {
        }

        /// <summary>
        /// Constructor for a patrol route (craft visits each waypoint in turn, then
        /// loiters at the last one).
        /// </summary>
        /// <param name="mission">mission that owns this state</param>
        /// <param name="waypoints">ordered route the craft is to patrol</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Is validated in base class")]
        public PatrolState(Mission mission, IList<GeoPosition> waypoints)
            :
            base(mission, mission.Craft.MaxSpeed)
        {
            Debug.Assert((waypoints != null) && (0 < waypoints.Count));

            this.waypoints = new List<GeoPosition>();
            foreach (GeoPosition waypoint in waypoints)
            {
                this.waypoints.Add(new GeoPosition(waypoint));
            }
            currentIndex = 0;
        }

        /// <summary>The ordered route the craft patrols.</summary>
        public IReadOnlyList<GeoPosition> Waypoints { get { return waypoints; } }

        /// <summary>Index of the waypoint the craft is currently heading for.</summary>
        public int CurrentWaypointIndex { get { return currentIndex; } }

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
        /// Change the craft, based on time elapsed
        /// </summary>
        /// <param name="milliseconds">Time that has passed</param>
        protected override void UpdateState(double milliseconds)
        {
            Craft craft = Mission.Craft;
            GeoPosition destination = waypoints[currentIndex];

            // get azimuth and distance to destination.
            float targetDistance = craft.Position.Distance(destination);
            float azimuth = craft.Position.GetAzimuth(destination);

            // figure out how far we can travel in this time slice
            double range = craft.MaxSpeed * milliseconds / 1000.0;

            // now move craft towards target, or put it AT target
            // note that reaching target doesn't complete mission. Running low in fuel does
            if (targetDistance <= range)
            {
                craft.Position = destination;

                // advance to the next waypoint (loiter at the last one)
                if (currentIndex < (waypoints.Count - 1))
                {
                    ++currentIndex;
                }
            }
            else
            {
                craft.Position = craft.Position.GetEndpoint(azimuth, range);
            }

            // if we're low on fuel, tell the mission
            if (!craft.ConsumeFuel(milliseconds))
            {
                Mission.OnFuelLow();
            }
        }

        /// <summary>
        /// Ordered route the craft patrols
        /// </summary>
        private List<GeoPosition> waypoints;

        /// <summary>
        /// Waypoint the craft is currently heading for
        /// </summary>
        private int currentIndex;
    }
}
