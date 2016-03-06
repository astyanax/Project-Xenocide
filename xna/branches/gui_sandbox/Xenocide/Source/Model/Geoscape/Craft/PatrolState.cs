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
using System.Text;
using System.Diagnostics;

using Xenocide.UI.Dialogs;
using Xenocide.Utils;

#endregion

namespace Xenocide.Model.Geoscape.Craft
{
    /// <summary>
    /// State that represents Craft moving to a position, then hanging around
    /// the position until it runs low on fuel
    /// </summary>
    [Serializable]
    public class PatrolState : MissionState
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mission">mission that owns this state</param>
        /// <param name="destination">position the craft is to patrol</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Is validated in base class")]
        public PatrolState(Mission mission, GeoPosition destination)
            :
            base(mission, mission.Craft.MaxSpeed)
        {
            this.destination = new GeoPosition(destination);
        }

        /// <summary>
        /// Respond to craft running low on fuel
        /// </summary>
        public override void OnFuelLow()
        {
            // if we're low on fuel, must "return to base"
            Xenocide.ScreenManager.QueueDialog(
                new MessageBoxDialog(
                    Util.StringFormat("Aircraft {0} reports fuel low.  Returning to base.", Mission.Craft.Name)
                )
            );
            Mission.SetState(new ReturnToBaseState(Mission));
        }

        /// <summary>
        /// Change the craft, based on time elapsed
        /// </summary>
        /// <param name="milliseconds">Time that has passed</param>
        protected override void UpdateState(double milliseconds)
        {
            Craft craft = Mission.Craft;

            // get azimuth and distance to destination.
            float targetDistance = craft.Position.GetDistance(destination);
            float azimuth = craft.Position.GetAzimuth(destination);

            // figure out how far we can travel in this time slice
            double range = craft.MaxSpeed * milliseconds / 1000.0;

            // now move craft towards target, or put it AT target
            // note that reaching traget doesn't complete mision. Runing low in fuel does
            if (targetDistance <= range)
            {
                craft.Position = destination;
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
        /// position the craft is to patrol
        /// </summary>
        private GeoPosition destination;
    }
}
