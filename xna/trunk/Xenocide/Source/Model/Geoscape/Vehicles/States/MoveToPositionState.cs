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
* @file MoveToPositionState.cs
* @date Created: 2007/02/10
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using ProjectXenocide.Utils;


#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// State that represents Craft moving to somewhere
    /// </summary>
    [Serializable]
    public class MoveToPositionState : MissionState
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mission">mission that owns this state</param>
        /// <param name="destination">Where the craft is going</param>
        /// <param name="stateType">Is this a point of interest, or a landing site?</param>
        /// <param name="craftSpeed">How fast to go there</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Is validated in base class")]
        public MoveToPositionState(Mission mission, GeoPosition destination, MoveToPositionStateType stateType , double craftSpeed)
            :
            base(mission, craftSpeed)
        {
            this.destination = new GeoPosition(destination);
            this.stateType = stateType;
        }

        /// <summary>
        /// Change the craft, based on time elapsed
        /// </summary>
        /// <param name="milliseconds">Time that has passed</param>
        protected override void UpdateState(double milliseconds)
        {
            Craft craft = Mission.Craft;

            // get azimuth and distance to destination.
            float targetDistance = craft.Position.Distance(destination);
            float azimuth = craft.Position.GetAzimuth(destination);

            // figure out how far we can travel in this time slice
            double range = CurrentSpeed * milliseconds / 1000.0;

            // now move craft towards target, or put it AT target
            if (targetDistance <= range)
            {
                Util.GeoTimeDebugWriteLine("{0} has reached {1}", craft.Name, stateType.ToString());
                craft.Position = destination;

                if (stateType == MoveToPositionStateType.LandingDestination)
                {
                    Mission.OnDestinationReached();
                }
                else if (stateType == MoveToPositionStateType.PointOfInterest)
                {
                    Mission.OnSubDestinationReached();
                }
            }
            else
            {
                craft.Position = craft.Position.GetEndpoint(azimuth, range);

                // if we're low on fuel, tell the mission
                if (!craft.ConsumeFuel(milliseconds))
                {
                    Mission.OnFuelLow();
                }
            }
        }

        /// <summary>
        /// Where the craft is going
        /// </summary>
        private GeoPosition destination;

        /// <summary>Is this a point of interest, or a landing site?<summary>
        private MoveToPositionStateType stateType;
    }
}