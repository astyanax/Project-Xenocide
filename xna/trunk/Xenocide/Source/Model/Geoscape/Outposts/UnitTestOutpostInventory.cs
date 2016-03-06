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
* @file UnitTestOutpostInventory.cs
* @date Created: 2007/09/10
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

using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape.Vehicles;

#endregion

namespace ProjectXenocide.Model.Geoscape.Outposts
{
    /// <summary>
    /// Unit tests for OutpostInventory class
    /// </summary>
    public partial class OutpostInventory
    {

        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            AddRemovePrisoners();
            AddItemsToOutpost();
            RemoveItemsFromOutpost();
            TestRemoveOneRoundAtATime();
            AddPartiallyLoadedWeapon();
            TestDecreaseAmmoRoundsInArmory();
            TestShipping();
        }

        /// <summary>
        /// Put weapon with partial clip into the outpost
        /// </summary>
        [Conditional("DEBUG")]
        private static void AddPartiallyLoadedWeapon()
        {
            Outpost outpost = ConstructTestOutpost();
            Item launcher = Xenocide.StaticTables.ItemList["ITEM_CANNON"].Manufacture();
            launcher.ShotsLeft  = 20;

            Debug.Assert(500 == outpost.Statistics.Capacities[launcher.ItemInfo.StorageType].Available);
            outpost.Inventory.Add(launcher, false);
            Debug.Assert(484 == outpost.Statistics.Capacities[launcher.ItemInfo.StorageType].Available);
            outpost.Inventory.Add(launcher, false);
            Debug.Assert(469 == outpost.Statistics.Capacities[launcher.ItemInfo.StorageType].Available);
            outpost.Inventory.Add(launcher, false);
            Debug.Assert(453 == outpost.Statistics.Capacities[launcher.ItemInfo.StorageType].Available);
            outpost.Inventory.Add(launcher, false);
            Debug.Assert(438 == outpost.Statistics.Capacities[launcher.ItemInfo.StorageType].Available);
        }

        /// <summary>
        /// Try sending a cargo crate
        /// </summary>
        [Conditional("DEBUG")]
        private static void TestShipping()
        {
            Outpost outpost = ConstructTestOutpost();

            Item launcher = Xenocide.StaticTables.ItemList["ITEM_CANNON"].Manufacture();
            launcher.ShotsLeft = 20;
            Item launcher2 = Xenocide.StaticTables.ItemList["ITEM_CANNON"].Manufacture();
            launcher2.ShotsLeft = 20;
            Item ammo = Xenocide.StaticTables.ItemList["ITEM_CANNON_CLIP"].Manufacture();
            Item aircraft = Xenocide.StaticTables.ItemList["ITEM_XC-1_GRYPHON"].Manufacture();
            Debug.Assert(500 == outpost.Statistics.Capacities[launcher.ItemInfo.StorageType].Available);

            Shipment shipment = new Shipment(outpost, Shipment.CalcEta());
            shipment.Add(aircraft);
            shipment.Add(launcher);
            shipment.Add(launcher2);
            shipment.Ship();
            Debug.Assert(470 == outpost.Statistics.Capacities[launcher.ItemInfo.StorageType].Available);
            Debug.Assert(0 == outpost.Inventory.NumberInInventory(launcher.ItemInfo));
            Debug.Assert(0 == outpost.Inventory.NumberInInventory(ammo.ItemInfo));
            Debug.Assert(0 == outpost.Inventory.NumberInInventory(aircraft.ItemInfo));
            shipment.OnShipmentArrive();
            Debug.Assert(469 == outpost.Statistics.Capacities[launcher.ItemInfo.StorageType].Available);
            Debug.Assert(2 == outpost.Inventory.NumberInInventory(launcher.ItemInfo));
            Debug.Assert(1 == outpost.Inventory.NumberInInventory(ammo.ItemInfo));
            Debug.Assert(1 == outpost.Inventory.NumberInInventory(aircraft.ItemInfo));
        }

        /// <summary>
        /// Put items into the outpost
        /// </summary>
        [Conditional("DEBUG")]
        private static void AddItemsToOutpost()
        {
            Outpost outpost = ConstructTestOutpost();
            OutpostInventory inventory = outpost.Inventory;

            // create and add a gryphon
            Item aircraft = Xenocide.StaticTables.ItemList["ITEM_XC-1_GRYPHON"].Manufacture();
            Debug.Assert(inventory.CanFit(aircraft));
            Debug.Assert(0 == inventory.NumberInInventory(aircraft.ItemInfo));
            outpost.Inventory.Add(aircraft, false);
            Debug.Assert(1 == inventory.NumberInInventory(aircraft.ItemInfo));

            // add a second, and confirm base is full
            Item gryphon = Xenocide.StaticTables.ItemList["ITEM_XC-1_GRYPHON"].Manufacture();
            Debug.Assert(inventory.CanFit(gryphon));
            outpost.Inventory.Add(gryphon, false);
            Debug.Assert(2 == inventory.NumberInInventory(aircraft.ItemInfo));
            Debug.Assert(!inventory.CanFit(gryphon));

            // add a cannon
            Item launcher = Xenocide.StaticTables.ItemList["ITEM_CANNON"].Manufacture();
            Debug.Assert(500 == outpost.Statistics.Capacities[launcher.ItemInfo.StorageType].Available);
            Debug.Assert(inventory.CanFit(launcher));
            Debug.Assert(0 == inventory.NumberInInventory(launcher.ItemInfo));
            outpost.Inventory.Add(launcher, false);
            Debug.Assert(1 == inventory.NumberInInventory(launcher.ItemInfo));
            Debug.Assert(485 == outpost.Statistics.Capacities[launcher.ItemInfo.StorageType].Available);

            // add a fully loaded clip
            Item clip = Xenocide.StaticTables.ItemList["ITEM_CANNON_CLIP"].Manufacture();
            Debug.Assert(inventory.CanFit(clip));
            Debug.Assert(0 == inventory.NumberInInventory(clip.ItemInfo));
            outpost.Inventory.Add(clip, false);
            Debug.Assert(1 == inventory.NumberInInventory(clip.ItemInfo));
            Debug.Assert(484 == outpost.Statistics.Capacities[launcher.ItemInfo.StorageType].Available);

            // add partially loaded clip
            for (int i = 0; i < (clip.ItemInfo as ClipItemInfo).ClipSize; ++i)
            {
                clip.ShotsLeft = 1;
                outpost.Inventory.Add(clip, false);
                Debug.Assert(2 == inventory.NumberInInventory(clip.ItemInfo));
                Debug.Assert(483 == outpost.Statistics.Capacities[launcher.ItemInfo.StorageType].Available);
            }
            outpost.Inventory.Add(clip, false);
            Debug.Assert(3 == inventory.NumberInInventory(clip.ItemInfo));
            Debug.Assert(482 == outpost.Statistics.Capacities[launcher.ItemInfo.StorageType].Available);

            // add a loaded cannon
            launcher.ShotsLeft = ((CraftWeaponItemInfo)launcher.ItemInfo).ClipSize;
            Debug.Assert(inventory.CanFit(launcher));
            outpost.Inventory.Add(launcher, false);
            Debug.Assert(7 == inventory.NumberInInventory(clip.ItemInfo));
            Debug.Assert(2 == inventory.NumberInInventory(launcher.ItemInfo));
            Debug.Assert(463 == outpost.Statistics.Capacities[launcher.ItemInfo.StorageType].Available);

            // now try to fill base to just short of maximum
            int size      = launcher.ItemInfo.StorageUnits + (clip.ItemInfo.StorageUnits * 4);
            int spaceLeft = (int)outpost.Statistics.Capacities[launcher.ItemInfo.StorageType].Available;
            for (int i = 0; i < spaceLeft / size; ++i)
            {
                Debug.Assert(inventory.CanFit(launcher));
                outpost.Inventory.Add(launcher, false);
            }
            Debug.Assert(!inventory.CanFit(launcher));

            // try people
            Item engineer = Xenocide.StaticTables.ItemList["ITEM_PERSON_ENGINEER"].Manufacture();
            Debug.Assert(inventory.CanFit(engineer));
            Debug.Assert(0 == inventory.NumberInInventory(engineer.ItemInfo));
            Debug.Assert(50 == outpost.Statistics.Capacities[engineer.ItemInfo.StorageType].Available);
            outpost.Inventory.Add(engineer, false);
            Debug.Assert(1 == inventory.NumberInInventory(engineer.ItemInfo));
            Debug.Assert(49 == outpost.Statistics.Capacities[engineer.ItemInfo.StorageType].Available);

            Person scientist = Xenocide.StaticTables.ItemList["ITEM_PERSON_SCIENTIST"].Manufacture() as Person;
            Debug.Assert(inventory.CanFit(scientist));
            Debug.Assert(0 == inventory.NumberInInventory(scientist.ItemInfo));
            outpost.Inventory.Add(scientist, false);
            Debug.Assert(1 == inventory.NumberInInventory(scientist.ItemInfo));
            Debug.Assert(48 == outpost.Statistics.Capacities[engineer.ItemInfo.StorageType].Available);

            // engineer can't work, no workshop in base.  But scientist can, so put him to work
            Debug.Assert(!(engineer as Person).CanWork);
            Debug.Assert(scientist.CanWork);
            Debug.Assert(!scientist.IsWorking);
            Debug.Assert(50 == outpost.Statistics.Capacities[scientist.PersonItemInfo.WorksIn].Available);
            scientist.IsWorking = true;
            Debug.Assert(scientist.IsWorking);
            Debug.Assert(49 == outpost.Statistics.Capacities[scientist.PersonItemInfo.WorksIn].Available);
            scientist.IsWorking = false;
            Debug.Assert(!scientist.IsWorking);
            Debug.Assert(50 == outpost.Statistics.Capacities[scientist.PersonItemInfo.WorksIn].Available);
        }

        /// <summary>
        /// Try removing items from an outpost
        /// </summary>
        [Conditional("DEBUG")]
        private static void RemoveItemsFromOutpost()
        {
            Outpost outpost = ConstructTestOutpost();
            OutpostInventory inventory = outpost.Inventory;

            // create and add a gryphon
            Item gryphon = Xenocide.StaticTables.ItemList["ITEM_XC-1_GRYPHON"].Manufacture();
            Debug.Assert(inventory.CanFit(gryphon));
            Debug.Assert(0 == inventory.NumberInInventory(gryphon.ItemInfo));
            outpost.Inventory.Add(gryphon, false);
            Debug.Assert(1 == inventory.NumberInInventory(gryphon.ItemInfo));

            // add a second, and confirm base is full
            Item condor = Xenocide.StaticTables.ItemList["ITEM_XC-11_CONDOR"].Manufacture();
            Debug.Assert(inventory.CanFit(condor));
            Debug.Assert(0 == inventory.NumberInInventory(condor.ItemInfo));
            outpost.Inventory.Add(condor, false);
            Debug.Assert(1 == inventory.NumberInInventory(condor.ItemInfo));
            Debug.Assert(!inventory.CanFit(gryphon));
            Debug.Assert(!inventory.CanFit(condor));

            // add 4 missile Launchers with clips
            Item clip     = Xenocide.StaticTables.ItemList["ITEM_GAIA_MISSILE_CLIP"].Manufacture();
            Item launcher = Xenocide.StaticTables.ItemList["ITEM_GAIA_MISSILE_LAUNCHER"].Manufacture();
            launcher.ShotsLeft = (clip.ItemInfo as ClipItemInfo).ClipSize;
            outpost.Inventory.Add(launcher, false);
            outpost.Inventory.Add(launcher, false);
            outpost.Inventory.Add(launcher, false);
            outpost.Inventory.Add(launcher, false);
            Debug.Assert(408 == outpost.Statistics.Capacities[launcher.ItemInfo.StorageType].Available);
            
            // list the contents of the base
            foreach (Item item in inventory.ListContents())
            {
                Debug.WriteLine(item.ItemInfo.Name);
            }

            // remove condor.
            inventory.Remove(condor);
            Debug.Assert(inventory.CanFit(gryphon));

            // Remove a missile launcher
            Item l = launcher.ItemInfo.FromOutpost(inventory);
            inventory.Remove(l);
            Debug.Assert(428 == outpost.Statistics.Capacities[launcher.ItemInfo.StorageType].Available);

            // Remove whole clip
            Item c = clip.ItemInfo.FromOutpost(inventory);
            inventory.Remove(c);
            Debug.Assert(431 == outpost.Statistics.Capacities[launcher.ItemInfo.StorageType].Available);

            // try people
            Item engineer = Xenocide.StaticTables.ItemList["ITEM_PERSON_ENGINEER"].Manufacture();
            Debug.Assert(50 == outpost.Statistics.Capacities[engineer.ItemInfo.StorageType].Available);
            outpost.Inventory.Add(engineer, false);
            Debug.Assert(49 == outpost.Statistics.Capacities[engineer.ItemInfo.StorageType].Available);

            Item scientist = Xenocide.StaticTables.ItemList["ITEM_PERSON_SCIENTIST"].Manufacture();
            outpost.Inventory.Add(scientist, false);
            Debug.Assert(48 == outpost.Statistics.Capacities[engineer.ItemInfo.StorageType].Available);

            inventory.Remove(engineer);
            Debug.Assert(49 == outpost.Statistics.Capacities[scientist.ItemInfo.StorageType].Available);
            inventory.Remove(scientist);
            Debug.Assert(50 == outpost.Statistics.Capacities[scientist.ItemInfo.StorageType].Available);
        }

        /// <summary>
        /// Remove a clip, in stages
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestRemoveOneRoundAtATime()
        {
            Outpost outpost = ConstructTestOutpost();
            OutpostInventory inventory = outpost.Inventory;

            Item clip = Xenocide.StaticTables.ItemList["ITEM_CANNON_CLIP"].Manufacture();
            outpost.Inventory.Add(clip, false);
            outpost.Inventory.Add(clip, false);
            Debug.Assert(498 == outpost.Statistics.Capacities[clip.ItemInfo.StorageType].Available);

            clip.ShotsLeft = 49;
            inventory.Remove(clip);
            Debug.Assert(498 == outpost.Statistics.Capacities[clip.ItemInfo.StorageType].Available);
            clip.ShotsLeft = 1;
            inventory.Remove(clip);
            Debug.Assert(499 == outpost.Statistics.Capacities[clip.ItemInfo.StorageType].Available);
            clip.ShotsLeft = 50;
            inventory.Remove(clip);
            Debug.Assert(500 == outpost.Statistics.Capacities[clip.ItemInfo.StorageType].Available);
        }

        /// <summary>
        /// Test the DecreaseAmmoRoundsInArmory()
        /// </summary>
        public static void TestDecreaseAmmoRoundsInArmory()
        {
            Outpost outpost = ConstructTestOutpost();
            OutpostInventory inventory = outpost.Inventory;

            Item clip = Xenocide.StaticTables.ItemList["ITEM_CANNON_CLIP"].Manufacture();
            for (int i = 0; i < 4; ++i)
            {
                inventory.Add(clip, false);
            }
            Debug.Assert(496 == outpost.Statistics.Capacities[clip.ItemInfo.StorageType].Available);
            Debug.Assert(200 == inventory.NumberInArmory(clip.ItemInfo.Id));
            
            // Remove 2.5 clips.
            Debug.Assert(125 == inventory.DecreaseAmmoRoundsInArmory(clip.ItemInfo, 125));
            Debug.Assert(75  == inventory.NumberInArmory(clip.ItemInfo.Id));
            Debug.Assert(498 == outpost.Statistics.Capacities[clip.ItemInfo.StorageType].Available);

            // Remove all but one round from partial clip
            Debug.Assert(24  == inventory.DecreaseAmmoRoundsInArmory(clip.ItemInfo, 24));
            Debug.Assert(51  == inventory.NumberInArmory(clip.ItemInfo.Id));
            Debug.Assert(498 == outpost.Statistics.Capacities[clip.ItemInfo.StorageType].Available);

            // remove last round from partial clip
            Debug.Assert(1   == inventory.DecreaseAmmoRoundsInArmory(clip.ItemInfo, 1));
            Debug.Assert(50  == inventory.NumberInArmory(clip.ItemInfo.Id));
            Debug.Assert(499 == outpost.Statistics.Capacities[clip.ItemInfo.StorageType].Available);

            // try removing more than is in base
            Debug.Assert(50  == inventory.DecreaseAmmoRoundsInArmory(clip.ItemInfo, 51));
            Debug.Assert(0   == inventory.NumberInArmory(clip.ItemInfo.Id));
            Debug.Assert(500 == outpost.Statistics.Capacities[clip.ItemInfo.StorageType].Available);

            // try removing when base is empty
            Debug.Assert(0   == inventory.DecreaseAmmoRoundsInArmory(clip.ItemInfo, 201));
            Debug.Assert(0   == inventory.NumberInArmory(clip.ItemInfo.Id));
            Debug.Assert(500 == outpost.Statistics.Capacities[clip.ItemInfo.StorageType].Available);
        }

        /// <summary>
        /// Test adding and removing prisoners (and corpses)
        /// </summary>
        public static void AddRemovePrisoners()
        {
            Outpost outpost = ConstructTestOutpost();
            OutpostInventory inventory = outpost.Inventory;

            Item greySoldier = Xenocide.StaticTables.ItemList["ITEM_GREY_SOLDIER"].Manufacture();
            Item greyCorpse  = Xenocide.StaticTables.ItemList["ITEM_GREY_CORPSE"].Manufacture();
            Debug.Assert(100 == outpost.Statistics.Capacities[greySoldier.ItemInfo.StorageType].Available);
            Debug.Assert(500 == outpost.Statistics.Capacities[greyCorpse.ItemInfo.StorageType].Available);

            // try adding
            inventory.Add(greySoldier, false);
            Debug.Assert( 96 == outpost.Statistics.Capacities[greySoldier.ItemInfo.StorageType].Available);
            Debug.Assert(500 == outpost.Statistics.Capacities[greyCorpse.ItemInfo.StorageType].Available);

            inventory.Add(greyCorpse, false);
            Debug.Assert( 96 == outpost.Statistics.Capacities[greySoldier.ItemInfo.StorageType].Available);
            Debug.Assert(496 == outpost.Statistics.Capacities[greyCorpse.ItemInfo.StorageType].Available);

            // try removing
            inventory.Remove(greySoldier);
            Debug.Assert(100 == outpost.Statistics.Capacities[greySoldier.ItemInfo.StorageType].Available);
            Debug.Assert(496 == outpost.Statistics.Capacities[greyCorpse.ItemInfo.StorageType].Available);

            inventory.Remove(greyCorpse);
            Debug.Assert(100 == outpost.Statistics.Capacities[greySoldier.ItemInfo.StorageType].Available);
            Debug.Assert(500 == outpost.Statistics.Capacities[greyCorpse.ItemInfo.StorageType].Available);
        }

        /// <summary>
        /// Construct a base with storage facilities, for testing purposes
        /// </summary>
        /// <returns>the constructed base</returns>
        public static Outpost ConstructTestOutpost()
        {
            Outpost outpost = new Outpost(new GeoPosition(), "testOutpost");

            // layout is this (because we must have an access lift)
            //  +------+------+------+------+
            //  |             |             |
            //  +    Pad 1    +     Pad 2   +
            //  |             |             |
            //  +------+------+------+------+
            //  | Lift | Store|
            //  +------+------+
            //  |Baraks| Xeno |
            //  +------+------+
            //  | Lab  |
            //  +------+

            FacilityHandle lift = new FacilityHandle("FAC_BASE_ACCESS_FACILITY", 0, 2);
            outpost.Floorplan.AddFacility(lift);
            FacilityHandle store1 = new FacilityHandle("FAC_STORAGE_FACILITY", 1, 2);
            outpost.Floorplan.AddFacility(store1);
            FacilityHandle Pad1 = new FacilityHandle("FAC_LANDING_PAD", 0, 0);
            outpost.Floorplan.AddFacility(Pad1);
            FacilityHandle Pad2 = new FacilityHandle("FAC_LANDING_PAD", 2, 0);
            outpost.Floorplan.AddFacility(Pad2);
            FacilityHandle barack = new FacilityHandle("FAC_BARRACKS_FACILITY", 0, 3);
            outpost.Floorplan.AddFacility(barack);
            FacilityHandle xenoContainment = new FacilityHandle("FAC_XENOMORPH_HOLDING_FACILITY", 1, 3);
            outpost.Floorplan.AddFacility(xenoContainment);
            FacilityHandle lab = new FacilityHandle("FAC_RESEARCH_FACILITY", 0, 4);
            outpost.Floorplan.AddFacility(lab);

            return outpost;
        }

        /// <summary>
        /// Create test item(s) and put into the inventory
        /// </summary>
        /// <param name="itemType">type of the item</param>
        /// <param name="quantity">quantity to create</param>
        [Conditional("DEBUG")]
        public void AddTestItem(String itemType, int quantity)
        {
            ItemInfo item = Xenocide.StaticTables.ItemList[itemType];
            for (int i = 0; i < quantity; ++i)
            {
                Add(item.Manufacture(), false);
            }
        }

        #endregion UnitTests

    }
}
