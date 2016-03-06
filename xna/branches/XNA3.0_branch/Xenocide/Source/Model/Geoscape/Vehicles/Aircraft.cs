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
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization;



using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape.GeoEvents;
using ProjectXenocide.Model.Geoscape.AI;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// A craft owned by X-Corp
    /// </summary>
    /// <remarks>Note, some of these are capable of space travel</remarks>
    [Serializable]
    public partial class Aircraft : Craft
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="craftType">type of aircraft (from StaticTables.ItemList)</param>
        public Aircraft(ItemInfo craftType)
            :
            base(craftType, CreateAircraftName(craftType))
        {
            Mission = new NoOrdersMission(this);
            // Craft is always built fully fueled
            fuel = MaxFuel;
        }

        /// <summary>
        /// Called when aircraft is transferred between outposts.  Do any necessary processing here
        /// </summary>
        public override void OnTransfer()
        {
            RemoveSoldiersAndXCaps();
            base.OnTransfer();
        }

        /// <summary>
        /// Is a craft showing on this craft's radar?
        /// </summary>
        /// <param name="otherPosition">Position of other craft</param>
        /// <returns>true if other craft is showing</returns>
        public override bool IsOnRadar(GeoPosition otherPosition)
        {
            return !InBase && Position.IsWithin(otherPosition, radarRange);
        }

        /// <summary>
        /// Respond to this craft being destroyed
        /// </summary>
        /// <remarks>tell owner Aircraft no longer exists</remarks>
        public override void OnDestroyed()
        {
            base.OnDestroyed();

            // kill any soldiers on the craft
            while (0 < soldiers.Count)
            {
                IEnumerator<Person> itr = soldiers.Keys.GetEnumerator();
                itr.MoveNext();
                Person soldier = itr.Current;
                Remove(soldier);
                HomeBase.Inventory.Remove(soldier);
            }

            // tell owner that we've been destroyed
            HomeBase.OnCraftDestroyed(this);

            // adjust score
            Xenocide.GameState.GeoData.AddScore(Participant.Alien, ItemInfo.Score, Position);
        }

        /// <summary>
        /// Update internal fuel reserves, to reflect passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        /// <returns>false if craft is fully refueled</returns>
        /// <remarks>Only covers case of craft being Xenium fueled</remarks>
        public override bool Refuel(double milliseconds)
        {
            Debug.Assert(fuel <= MaxFuel);

            // if craft is fully fueled, nothing to do.
            if (MaxFuel <= fuel)
            {
                return false;
            }

            // figure out how much fuel should have been put in craft, based on elapsed time
            double increment = Math.Min((RefuelRate * milliseconds) + surplusFuel, (MaxFuel - fuel));

            if (AircraftItemInfo.FuelType == FuelType.Hydrogen)
            {
                // special case, hydrogen fuel is unlimited
                fuel += increment;
            }
            else
            {
                fuel += TakeXeniumFuelFromOutpostSupplies(increment);
            }

            return (fuel < MaxFuel);
        }

        /// <summary>
        /// Update internal ammunition reserves, to reflect passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        /// <returns>false if craft is fully reloaded</returns>
        /// <remarks>Default behaviour (used by UFOs) is do nothing</remarks>
        public override bool Reload(double milliseconds)
        {
            // delegate to weapon pods
            bool full = true;
            foreach (WeaponPod pod in WeaponPods)
            {
                if (null != pod)
                {
                    full &= !pod.Reload(milliseconds, HomeBase);
                }
            }
            return !full;
        }

        /// <summary>
        /// Update craft's "health", to reflect passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        /// <returns>false if craft is fully repaired</returns>
        /// <remarks>Default behaviour (used by UFOs) is do nothing</remarks>
        public override bool Repair(double milliseconds)
        {
            bool damaged = false;
            if (0.0 < HullDamage)
            {
                HullDamage -= repairRate * milliseconds;

                if (HullDamage <= 0.0)
                {
                    HullDamage = 0.0;
                }
                else
                {
                    damaged = true;
                }
            }
            return damaged;
        }

        /// <summary>
        /// Update internal fuel reserves, to reflect fuel consumed while on mission
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        /// <returns>false if Fuel at "return to base" level</returns>
        public override bool ConsumeFuel(double milliseconds)
        {
            // first figure out fuel left
            fuel -= FuelConsumptionRate * milliseconds / (3600 * 1000);

            // don't let it go negative
            if (fuel < 0.0)
            {
                fuel = 0.0;
            }

            return IsFuelLow();
        }

        /// <summary>
        /// Craft has entered an Outpost
        /// </summary>
        public override void EnterOutpost()
        {
            // set up for refueling & rearming
            fuel = Math.Round(fuel);
            surplusFuel = 0.0;
            outpostOutOfFuel = false;
            foreach (WeaponPod pod in WeaponPods)
            {
                if (null != pod)
                {
                    pod.PrepareForReload();
                }
            }

            // and default behaviour
            base.EnterOutpost();
        }

        /// <summary>
        /// Does craft need to return to outpost for refueling?
        /// </summary>
        /// <returns>false if craft has more than enough fuel to reach outpost</returns>
        public override bool IsFuelLow()
        {
            //... if tank is more than half full, then obviously not a problem
            if (0.5 < (fuel / MaxFuel))
            {
                return true;
            }
            else
            {
                // MaxSpeed is rad/sec, consumption rate is units/hour, distance is radians
                double range = MaxSpeed * fuel * 3600 / FuelConsumptionRate;
                double distance = Position.Distance(HomeBase.Position);
                return (distance < range);
            }
        }

        /// <summary>
        /// Equip aircraft with default weapons.  (As many cannons as it can carry)
        /// </summary>
        public void ArmWithDefaultWeapons()
        {
            for (int i = 0; i < WeaponPods.Count; i++)
            {
                WeaponPod pod = new WeaponPod("ITEM_CANNON");
                if (pod.UsesAmmo)
                {
                    pod.ShotsLeft = pod.Weapon.ClipSize;
                }
                WeaponPods[i] = pod;
            }
        }

        /// <summary>
        /// Check if there are any previously undetected Alien Outposts in range of this craft
        /// </summary>
        public void LookForAlienOutposts()
        {
            foreach (AlienSite site in Xenocide.GameState.GeoData.Overmind.Sites)
            {
                if (!site.IsKnownToXCorp && Position.IsWithin(site.Position, radarRange))
                {
                    site.IsKnownToXCorp = true;
                    MessageBoxGeoEvent.Queue(Strings.MSGBOX_ALIEN_ACTIVITY_DISCOVERED, site.Name);
                }
            }
        }

        /// <summary>
        /// Remove a person from the soldiers assigned to this aircraft
        /// </summary>
        /// <param name="person"></param>
        public void Remove(Person person)
        {
            Debug.Assert(soldiers.ContainsKey(person), "Person not assigned to this aircraft");
            soldiers.Remove(person);
        }

        /// <summary>
        /// Build up string of form [x]/[y] where [x] is number of soldiers and [y] is max humans
        /// </summary>
        public string SoldierCountStatus
        {
            get
            {
                return Util.StringFormat(Strings.SOLDIER_COUNT_STATUS, Soldiers.Count, AircraftItemInfo.MaxHumans);
            }
        }

        /// <summary>
        /// Build up string of form [x]/[y] where [x] is number of xcaps and [y] is max xcaps 
        /// </summary>
        public string XcapCountStatus
        {
            get
            {
                return Util.StringFormat(Strings.XCAP_COUNT_STATUS, XCaps.Count, AircraftItemInfo.MaxXcaps);
            }
        }

        /// <summary>
        /// Remove the xenium going into the craft from the outpost's stores
        /// </summary>
        /// <param name="units">Number of units going into craft</param>
        /// <returns>number of units to remove from outpost's store</returns>
        private int TakeXeniumFuelFromOutpostSupplies(double units)
        {
            // Assumes refueling occurs in integer units
            Debug.Assert(0 == (fuel - (int)fuel));

            // number of units to remove from outpost
            int unitsToRemove = (int)units;

            // excess fuel (time was not integer number of units)
            // remember for next update
            surplusFuel = units - unitsToRemove;

            // check quantity of fuel in outpost
            Item xenium = Xenocide.StaticTables.ItemList["ITEM_XENIUM-122"].Manufacture();
            int available = HomeBase.Inventory.NumberInInventory(xenium.ItemInfo);

            // remove as much fuel as we need/is available, whichever is less
            for (int i = 0; (i < unitsToRemove) && (i < available); ++i)
            {
                HomeBase.Inventory.Remove(xenium);

                // we haven't run out of fuel... yet.
                outpostOutOfFuel = false;
            }

            // handle case of insufficient fuel
            if (available < unitsToRemove)
            {
                unitsToRemove = available;
                surplusFuel = 0.0;

                // tell user (if we haven't already)
                if (!outpostOutOfFuel)
                {
                    outpostOutOfFuel = true;
                    MessageBoxGeoEvent.Queue(
                        Strings.MGSBOX_BASE_OUT_OF_CRAFT_SUPPLIES, HomeBase.Name, xenium.Name, Name
                    );
                }
            }

            return unitsToRemove;
        }

        /// <summary>
        /// Remove the soldiers and XCaps assigned to this craft
        /// </summary>
        private void RemoveSoldiersAndXCaps()
        {
            // Remove any soldiers assigned to the craft
            soldiers.Clear();

            // Remove any xcaps assigned to the craft
            foreach (Item xcap in xcaps)
            {
                HomeBase.Inventory.Add(xcap, false);
            }
            xcaps.Clear();
        }

        /// <summary>
        /// Create a name for this craft
        /// </summary>
        /// <param name="craftType">type of aircraft (from StaticTables.ItemList)</param>
        /// <returns>the created name</returns>
        private static String CreateAircraftName(ItemInfo craftType)
        {
            return Xenocide.GameState.GeoData.XCorp.CreateItemName(craftType);
        }

        #region Fields

        /// <summary>
        /// Human capcity of aircraft
        /// </summary>
        public int MaxHumans { get { return AircraftItemInfo.MaxHumans; } }

        /// <summary>
        /// Xcaps capacity of aircraft
        /// </summary>
        public int MaxXcaps { get { return AircraftItemInfo.MaxXcaps; } }

        /// <summary>
        /// Amount of fuel currently on board (in units)
        /// </summary>
        public override Double Fuel { get { return fuel; } }

        /// <summary>
        /// State of craft's fuel, to show on LaunchIntercept dialog
        /// </summary>
        public int FuelPercent { get { return Util.ToPercent(fuel, MaxFuel); } }

        /// <summary>
        /// State of craft assigned soldiers
        /// </summary>
        public SortedList<Person, int> Soldiers { get { return soldiers; } }

        /// <summary>
        /// State of craft assigned xcaps
        /// </summary>
        public IList<Item> XCaps { get { return xcaps; } }

        /// <summary>
        /// Amount of fuel currently on board (in units)
        /// </summary>
        private double fuel;

        /// <summary>
        /// How quickly craft can refuel, in units/hour
        /// </summary>
        private double RefuelRate
        {
            get { return (AircraftItemInfo.FuelType == FuelType.Hydrogen) ? hydrogenRefuelRate : xeniumRefuelRate; }
        }

        /// <summary>
        /// How quickly craft consumes fuel, in units/hour
        /// </summary>
        private double FuelConsumptionRate { get { return AircraftItemInfo.FuelConsumptionRate; } }

        /// <summary>
        /// Maximum fuel craft can carry (litres)
        /// </summary>
        private double MaxFuel { get { return AircraftItemInfo.MaxFuel; } }

        /// <summary>
        /// How quickly craft can refuel, in units/millisecond.  (Default is 2 per hour)
        /// </summary>
        private const double xeniumRefuelRate = 2.0 / (3600.0 * 1000.0);

        /// <summary>
        /// How quickly craft can refuel, in units/millisecond.  (Default is 100 per hour)
        /// </summary>
        private const double hydrogenRefuelRate = 100.0 / (3600.0 * 1000.0);

        /// <summary>
        /// Has craft sustained sufficient damage to force it to crash land?
        /// (Aircraft never crash, they're just destroyed)
        /// </summary>
        public override bool IsCrashed { get { return false; } }

        /// <summary>
        /// Rate at which we can repair damage to an aircraft (in points/millisecond). Base rate is 25 points/day
        /// </summary>
        private const double repairRate = 25.0 / (24.0 * 3600.0 * 1000.0);

        /// <summary>
        /// Can this craft carry troops?
        /// </summary>
        public override bool CanCarrySoldiers { get { return 0 < AircraftItemInfo.MaxHumans; } }

        /// <summary>
        /// Is this craft carrying troops?
        /// </summary>
        public override bool IsCarryingSoldiers { get { return 0 < soldiers.Count; } }

        /// <summary>
        /// The Item object holding the static properties of this type of aircraft
        /// </summary>
        private AircraftItemInfo AircraftItemInfo { get { return ItemInfo as AircraftItemInfo; } }

        /// <summary>
        /// any fuel units left over increment left over from previous Refuel() call
        /// </summary>
        private double surplusFuel;

        /// <summary>
        /// Record that outpost ran out of fuel, and we've informed the user
        /// </summary>
        private bool outpostOutOfFuel;

        /// <summary>
        /// Range of craft's radar  (all craft have the same range, 700 nautical miles)
        /// </summary>
        private static readonly float radarRange = (float)GeoPosition.KnotsToRadians(700);

        /// <summary>
        /// List of soldiers assigned to this aircraft and their respective position
        /// </summary>
        private SortedList<Person, int> soldiers = new SortedList<Person, int>();

        /// <summary>
        /// List of xcaps assigned to this aircraft
        /// </summary>
        private List<Item> xcaps = new List<Item>();

        #endregion

    }
}
