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
* @file UnitTestAircraft.cs
* @date Created: 2007/11/26
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

#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// Unit Tests for the Aircraft Class
    /// </summary>
    public partial class Aircraft : Craft
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            TestReloadingWeaponPod();
            TestParitalXeniumRefuel();
            TestXeniumRefuel();
            TestRepair();
            TestDestruction();
        }

        /// <summary>
        /// Create a damaged craft, check that it repairs as predicted
        /// </summary>
        [Conditional("DEBUG")]
        private static void TestRepair()
        {
            Outpost outpost = OutpostInventory.ConstructTestOutpost();

            // Add damaged craft
            Aircraft aircraft = (Aircraft)Xenocide.StaticTables.ItemList["ITEM_XC-22_ECLIPSE"].Manufacture();
            aircraft.HullDamage = aircraft.MaxDamage;
            outpost.Inventory.Add(aircraft, false);

            // Progress time
            // step size should be 90% of time required to repair a unit of damage
            // number of steps should be just under enough to repair craft
            double stepSize = 0.9 / repairRate;
            int steps = (int)((aircraft.MaxDamage / 0.9)) - 1;
            for (int i = 0; i < steps; ++i)
            {
                aircraft.Update(stepSize);
            }
            Debug.Assert(0 < aircraft.HullDamage);

            // now this should finish the repair
            aircraft.Update(stepSize * 2);
            Debug.Assert(0.0 == aircraft.HullDamage);

            // another update has no effect
            aircraft.Update(stepSize * 2);
            Debug.Assert(0.0 == aircraft.HullDamage);
        }

        /// <summary>
        /// Create a xenium fueled craft, and check refuel time
        /// </summary>
        [Conditional("DEBUG")]
        private static void TestXeniumRefuel()
        {
            Outpost outpost = OutpostInventory.ConstructTestOutpost();
             
            // Add craft (with empty fuel tank)
            Aircraft aircraft = (Aircraft)Xenocide.StaticTables.ItemList["ITEM_XC-22_ECLIPSE"].Manufacture();
            outpost.Inventory.Add(aircraft, false);
            aircraft.fuel = 0.0;

            // add enough xenium to outpost to allow refuel
            outpost.Inventory.AddTestItem("ITEM_XENIUM-122", (int)aircraft.MaxFuel);

            // Progress time
            // step size should be 90% of time required for a unit of fuel
            // number of steps should be just enough to fuel craft
            double stepSize = 0.9 / aircraft.RefuelRate; 
            int steps = (int)(aircraft.MaxFuel / 0.9);
            for (int i = 0; i < steps; ++i)
            {
                aircraft.Update(stepSize);
            }
            Debug.Assert(aircraft.fuel < aircraft.MaxFuel);

            // now this should finish the refuel
            aircraft.Update(stepSize * 2);
            Debug.Assert(aircraft.fuel == aircraft.MaxFuel);

            // another update has no effect
            aircraft.Update(stepSize * 2);
            Debug.Assert(aircraft.fuel == aircraft.MaxFuel);
        }

        /// <summary>
        /// Create a xenium fueled craft, and check refuel when insufficent fuel
        /// </summary>
        [Conditional("DEBUG")]
        private static void TestParitalXeniumRefuel()
        {
            Outpost outpost = OutpostInventory.ConstructTestOutpost();

            // Add craft (with empty fuel tank)
            Aircraft aircraft = (Aircraft)Xenocide.StaticTables.ItemList["ITEM_XC-22_ECLIPSE"].Manufacture();
            outpost.Inventory.Add(aircraft, false);
            aircraft.fuel = 0.0;

            // add insufficient xenium to outpost to allow refuel
            outpost.Inventory.AddTestItem("ITEM_XENIUM-122", (int)(aircraft.MaxFuel / 3));

            // Progress time
            // step size should be 90% of time required for a unit of fuel
            // number of steps should be just enough to fuel craft
            double stepSize = 0.9 / aircraft.RefuelRate;
            int steps = (int)(aircraft.MaxFuel / 0.9);
            for (int i = 0; i < steps; ++i)
            {
                aircraft.Update(stepSize);
            }
            
            // check craft is only part full
            Debug.Assert(aircraft.fuel == ((int)(aircraft.MaxFuel / 3)));
            
            // refuel again
            outpost.Inventory.AddTestItem("ITEM_XENIUM-122", (int)(aircraft.MaxFuel / 3));
            aircraft.Update(stepSize * steps);
            aircraft.Update(stepSize * steps);
            Debug.Assert(aircraft.fuel == (int)(2 * aircraft.MaxFuel / 3));
        }

        /// <summary>
        /// Test ability to reload weapon pods
        /// </summary>
        [Conditional("DEBUG")]
        private static void TestReloadingWeaponPod()
        {
            Outpost outpost = OutpostInventory.ConstructTestOutpost();

            // Add craft 
            Aircraft aircraft = (Aircraft)Xenocide.StaticTables.ItemList["ITEM_XC-22_ECLIPSE"].Manufacture();
            outpost.Inventory.Add(aircraft, false);

            // Test case of pod that doesn't use ammo
            aircraft.WeaponPods[0] = new WeaponPod("ITEM_LASER_CANNON");
            aircraft.Update(24 * 3600 * 1000);

            // Test case of pod that does use ammo
            WeaponPod pod = new WeaponPod("ITEM_CANNON");
            aircraft.WeaponPods[0] = pod;
            
            // add insufficient ammo
            Item clip     = Xenocide.StaticTables.ItemList["ITEM_CANNON_CLIP"].Manufacture();
            clip.ShotsLeft = pod.ClipSize / 3;
            outpost.Inventory.Add(clip, false);

            // Progress time
            // step size should be 90% of time required for a unit of fuel
            // number of steps should be just enough to fuel craft
            double stepSize = 0.9 / pod.ReloadRate;
            int steps = (int)(pod.ClipSize / 0.9);
            for (int i = 0; i < steps; ++i)
            {
                aircraft.Update(stepSize);
            }

            // check craft is only part full
            Debug.Assert(pod.ShotsLeft == pod.ClipSize / 3);

            // won't get any more full
            aircraft.Update(stepSize * steps);
            Debug.Assert(pod.ShotsLeft == pod.ClipSize / 3);

            // fully fill
            clip.ShotsLeft = pod.ClipSize;
            outpost.Inventory.Add(clip, false);
            aircraft.Update(stepSize * steps);
            Debug.Assert(pod.ShotsLeft == pod.ClipSize);
            Debug.Assert(outpost.Inventory.NumberInArmory(clip.ItemInfo.Id) == (pod.ClipSize / 3));

            // won't go any more
            aircraft.Update(stepSize * steps);
            Debug.Assert(pod.ShotsLeft == pod.ClipSize);
            Debug.Assert(outpost.Inventory.NumberInArmory(clip.ItemInfo.Id) == (pod.ClipSize / 3));
        }

        /// <summary>
        /// Test that destroying a craft kills the soldiers it carries.
        /// </summary>
        [Conditional("DEBUG")]
        private static void TestDestruction()
        {
            Outpost outpost = OutpostInventory.ConstructTestOutpost();

            // Add craft and soldiers
            Aircraft aircraft = Xenocide.StaticTables.ItemList["ITEM_XC-11_CONDOR"].Manufacture() as Aircraft;
            outpost.Inventory.Add(aircraft, false);

            Person soldier0 = Xenocide.StaticTables.ItemList["ITEM_PERSON_SOLDIER"].Manufacture() as Person;
            Person soldier1 = Xenocide.StaticTables.ItemList["ITEM_PERSON_SOLDIER"].Manufacture() as Person;
            Person soldier2 = Xenocide.StaticTables.ItemList["ITEM_PERSON_SOLDIER"].Manufacture() as Person;
            outpost.Inventory.Add(soldier0, false);
            outpost.Inventory.Add(soldier1, false);
            outpost.Inventory.Add(soldier2, false);

            // add soldiers to aircraft
            aircraft.Soldiers.Add(soldier1, 0);
            aircraft.Soldiers.Add(soldier2, 1);

            // destroy craft.  should have freed up space in outpost
            Debug.Assert(1 == outpost.Statistics.Capacities[aircraft.ItemInfo.StorageType].Available);
            Debug.Assert(47 == outpost.Statistics.Capacities[soldier0.ItemInfo.StorageType].Available);
            aircraft.OnDestroyed();
            Debug.Assert(2 == outpost.Statistics.Capacities[aircraft.ItemInfo.StorageType].Available);
            Debug.Assert(49 == outpost.Statistics.Capacities[soldier0.ItemInfo.StorageType].Available);
        }

        #endregion UnitTests
    }
}
