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
* @file Aircraft.cs
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

using Xenocide.Model.Geoscape.HumanBases;

#endregion

namespace Xenocide.Model.Geoscape.Craft
{
    /// <summary>
    /// A craft owned by X-Corp
    /// </summary>
    /// <remarks>Note, some of these are capable of space travel</remarks>
    [Serializable]
    public class Aircraft : Craft
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxSpeed">Fastest speed craft is capable of</param>
        /// <param name="homeBase">Craft's home base</param>
        public Aircraft(double maxSpeed, HumanBase homeBase)
            :
            base(maxSpeed, homeBase)
        {
            Mission = new NoOrdersMission(this);

            // Craft is always built fully fueled
            fuel = maxFuel;
        }

        /// <summary>
        /// Find out if this craft should be drawn on the Geoscape
        /// </summary>
        /// <returns>true if this craft shold be drawn</returns>
        override public bool CanDrawOnGeoscape() { return !InBase; }

        /// <summary>
        /// Update internal fuel reserves, to reflect passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        /// <returns>false if craft is fully refueled</returns>
        /// <remarks>Only covers case of craft being Xenium fueled</remarks>
        public override bool Refuel(double milliseconds)
        {
            Debug.Assert(fuel <= maxFuel);
            
            // if craft is fully fueled, nothing to do.
            if (maxFuel <= fuel)
            {
                return false;
            }
            
            // figure out how much extra fuel we can put in craft
            double increment = refuelRate * milliseconds / (3600 * 1000);
            double maxIncrement = maxFuel - fuel;
            if (maxIncrement < increment)
            {
                increment = maxIncrement;
            }

            TakeFuelFromBaseSupplies((int)(fuel + increment) - (int)fuel);
            fuel += increment;

            return (fuel < maxFuel);
        }

        /// <summary>
        /// Update internal fuel reserves, to reflect fuel consumed while on mission
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        /// <returns>false if Fuel at "return to base" level</returns>
        public override bool ConsumeFuel(double milliseconds)
        {
            // first figure out fuel left
            fuel -= consumptionRate * milliseconds / (3600 * 1000);

            // don't let it go negative
            if (fuel < 0.0)
            {
                fuel = 0.0;
            }

            return IsFuelLow();
        }

        /// <summary>
        /// Does craft need to return to base for refueling?
        /// </summary>
        /// <returns>false if craft has more than enough fuel to reach base</returns>
        public override bool IsFuelLow()
        {
            //... if tank is more than half full, then obviously not a problem
            if (0.5 < (fuel / maxFuel))
            {
                return true;
            }
            else
            {
                // MaxSpeed is rad/sec, consumption rate is units/hour, distance is radians
                double range    = MaxSpeed * fuel * 3600 / consumptionRate;
                double distance = Position.GetDistance(HomeBase.Position);
                return (distance < range);
            }
        }

        /// <summary>
        /// Player readable identifier for this craft
        /// </summary>
        public override string Name { get { return "<Aircraft ToDo>"; } }

        /// <summary>
        /// Remove the fuel going into the craft from the base's stores
        /// </summary>
        /// <param name="units">Number of units to remove</param>
        /// <returns>true if was able to obtain the fuel</returns>
        /// <remarks>ToDo: implement</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1801:AvoidUnusedParameters",
            Justification = "ToDo: function still under construction")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "ToDo: function still under construction")]
        private bool TakeFuelFromBaseSupplies(int units)
        {
            return true;
        }

        #region Fields

        /// <summary>
        /// Amount of fuel currently on board (in units)
        /// </summary>
        public override Double Fuel { get { return fuel; } }

        /// <summary>
        /// Amount of fuel currently on board (in units)
        /// </summary>
        private double fuel;

        /// <summary>
        /// How quickly craft can refuel, in units/hour
        /// </summary>
        /// <remarks>currently hardcoded as xenium only</remarks>
        /// <remarks>ToDo: allow to be hydrogen or xenium</remarks>
        private double refuelRate = xeniumRefuelRate;

        /// <summary>
        /// How quickly craft consumes fuel, in units/hour
        /// </summary>
        /// <remarks>currently hardcoded</remarks>
        /// <remarks>ToDo: read from item.xml</remarks>
        private double consumptionRate = 1.2;

        /// <summary>
        /// Maximum fuel craft can carry (litres)
        /// </summary>
        /// <remarks>currently hardcoded</remarks>
        /// <remarks>ToDo: read from item.xml</remarks>
        private double maxFuel = 12.0;

        /// <summary>
        /// How quickly craft can refuel, in units/hour
        /// </summary>
        private const double xeniumRefuelRate = 2.0;

        /// <summary>
        /// How quickly craft can refuel, in units/hour
        /// </summary>
        private const double hydrogenRefuelRate = 100.0;

        /*
        /// <summary>
        /// Number of missiles on board
        /// </summary>
        private int missiles;

        /// <summary>
        /// Maximum number of missiles that can be carried
        /// </summary>
        private int maxMissiles;
        */ 

        #endregion
    }
}
