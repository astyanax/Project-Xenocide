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
* @file UnitTestCombatantInventory.cs
* @date Created: 2007/10/22
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape.Outposts;

namespace ProjectXenocide.Model.Battlescape.Combatants
{
    /// <summary>
    /// Unit Tests for CombatantInventory
    /// </summary>
    public partial class CombatantInventory
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            TestCanFit();
            TestInsertAndRemove();
            TestLoadout();
            TestArmor();
        }

        /// <summary>
        /// Basic can fit tests
        /// </summary>
        [Conditional("DEBUG")]
        private static void TestCanFit()
        {
            Combatant combatant = new Combatant(null, Team.XCorp);
            CombatantInventory ci = combatant.Inventory;
            Item pistol = Xenocide.StaticTables.ItemList["ITEM_PISTOL"].Manufacture();           // 1 x 2
            Item heClip = Xenocide.StaticTables.ItemList["ITEM_R_C_HE_CLIP"].Manufacture();      // 2 x 1
            Item cannon = Xenocide.StaticTables.ItemList["ITEM_REPEATER_CANNON"].Manufacture();  // 2 x 3
            Item rifle  = Xenocide.StaticTables.ItemList["ITEM_ASSAULT_RIFLE"].Manufacture();    // 1 x 3

            // off grid
            Debug.Assert( ci.CanFit(cannon, 5, 0));
            Debug.Assert(!ci.CanFit(cannon, 6, 0));
            Debug.Assert(!ci.CanFit(cannon, 5, 2));
            Debug.Assert( ci.CanFit(rifle,  6, 0));
            Debug.Assert(!ci.CanFit(rifle,  6, 2));

            // too tall for zone
            Debug.Assert(!ci.CanFit(rifle,  6, 1));
            Debug.Assert( ci.CanFit(pistol, 0, 1));
            Debug.Assert(!ci.CanFit(pistol, 1, 1));

            // too wide for zone
            Debug.Assert( ci.CanFit(heClip, 1, 2));
            Debug.Assert(!ci.CanFit(heClip, 2, 2));

            // fits in hand
            Debug.Assert(ci.CanFit(cannon, 1, 0));
            Debug.Assert(ci.CanFit(cannon, 0, 0));
        }

        /// <summary>
        /// Test we can insert and remove items
        /// </summary>
        [Conditional("DEBUG")]
        private static void TestInsertAndRemove()
        {
            Combatant combatant = new Combatant(null, Team.XCorp);
            CombatantInventory ci = combatant.Inventory;
            Item pistol = Xenocide.StaticTables.ItemList["ITEM_PISTOL"].Manufacture();           // 1 x 2
            Item heClip = Xenocide.StaticTables.ItemList["ITEM_R_C_HE_CLIP"].Manufacture();      // 2 x 1
            Item cannon = Xenocide.StaticTables.ItemList["ITEM_REPEATER_CANNON"].Manufacture();  // 2 x 3
            Item rifle = Xenocide.StaticTables.ItemList["ITEM_ASSAULT_RIFLE"].Manufacture();     // 1 x 3
            Item flash = Xenocide.StaticTables.ItemList["ITEM_FLASHPOD"].Manufacture();          // 1 x 1

            Debug.Assert(null == ci.ItemAt(4, 0));
            ci.Insert(pistol, 4, 0);
            Debug.Assert(pistol == ci.ItemAt(4, 0));
            Debug.Assert(pistol == ci.ItemAt(4, 1));
            ci.Insert(cannon, 5, 0);
            Debug.Assert(cannon == ci.ItemAt(6, 2));

            // should be unable to put clip here, space blocked by cannon
            Debug.Assert(!ci.CanFit(heClip, 4, 2));

            // can do it after removing the cannon
            ci.Remove(cannon);
            Debug.Assert(null == ci.ItemAt(6, 2));
            Debug.Assert(ci.CanFit(heClip, 4, 2));
            ci.Insert(heClip, 4, 2);

            // check putting one item in a hand fills it.
            Debug.Assert(ci.CanFit(rifle, 1, 0));
            ci.Insert(cannon, 1, 0);
            Debug.Assert(!ci.CanFit(rifle, 1, 0));
            ci.Insert(rifle, 0, 0);
            Debug.Assert(!ci.CanFit(cannon, 1, 0));
            ci.Remove(cannon);
            Debug.Assert(ci.CanFit(cannon, 1, 0));
            Debug.Assert(!ci.CanFit(cannon, 2, 0));
            Debug.Assert(ci.CanFit(flash, 0, 1));
            Debug.Assert(ci.CanFit(flash, 1, 1));
        }

        /// <summary>
        /// Check recording and restoring loadout
        /// </summary>
        [Conditional("DEBUG")]
        private static void TestLoadout()
        {
            // try simple case of laser rifle and rocket in backpack
            Combatant combatant = new Combatant(null, Team.XCorp);
            CombatantInventory ci = combatant.Inventory;
            Item rocket = Xenocide.StaticTables.ItemList["ITEM_R_L_LARGE_ROCKET"].Manufacture();
            ci.Insert(rocket, 5, 0);
            Item laser = Xenocide.StaticTables.ItemList["ITEM_LASER_RIFLE"].Manufacture();
            ci.Insert(laser, 4, 0);

            ci.RecordLoadout();
            Outpost outpost = OutpostInventory.ConstructTestOutpost();

            // restore
            Debug.Assert(ci.RestoreLoadout(outpost.Inventory));
            Debug.Assert(2 == ci.contents.Count);
            Debug.Assert(ci.ItemAt(4, 0).ItemInfo.Id == "ITEM_LASER_RIFLE");
            rocket = ci.ItemAt(5, 0);
            Debug.Assert(rocket.ItemInfo.Id == "ITEM_R_L_LARGE_ROCKET");
            Debug.Assert(rocket.ShotsLeft == 1);

            // Put semi loaded Repeater cannon in left hand,
            // Put fully loaded plasma pistol in right hand
            Item cannon = Xenocide.StaticTables.ItemList["ITEM_REPEATER_CANNON"].Manufacture();
            cannon.AmmoInfo = Xenocide.StaticTables.ItemList["ITEM_R_C_HE_CLIP"];
            cannon.ShotsLeft = 4;
            ci.Insert(cannon, 1, 0);

            Item pistol = Xenocide.StaticTables.ItemList["ITEM_PLASMA_PISTOL"].Manufacture();
            pistol.AmmoInfo = Xenocide.StaticTables.ItemList["ITEM_PLASMA_PISTOL_CLIP"];
            pistol.ShotsLeft = 26;
            ci.Insert(pistol, 0, 0);

            // record loadout
            ci.RecordLoadout();

            // set up base with insufficient ammo to reload plasma pistol
            // but sufficent to reload repeater cannon
            Item plasmaClip = Xenocide.StaticTables.ItemList["ITEM_PLASMA_PISTOL_CLIP"].Manufacture();
            plasmaClip.ShotsLeft = 13;
            outpost.Inventory.Add(plasmaClip, false);

            Item heClip = Xenocide.StaticTables.ItemList["ITEM_R_C_HE_CLIP"].Manufacture();
            outpost.Inventory.Add(heClip, false);

            ci.Remove(cannon);
            cannon.ShotsLeft = 0;
            outpost.Inventory.Add(cannon, false);
            ci.Remove(pistol);
            pistol.ShotsLeft = 0;
            outpost.Inventory.Add(pistol, false);

            // restore, check pistol is only part loaded
            Debug.Assert(!ci.RestoreLoadout(outpost.Inventory));

            // check items
            Debug.Assert(4 == ci.contents.Count);
            Item newPistol = ci.ItemAt(0, 0);
            Item newCannon = ci.ItemAt(1, 0);
            Debug.Assert(newPistol.ItemInfo.Id == "ITEM_PLASMA_PISTOL");
            Debug.Assert(newPistol.AmmoInfo.Id == "ITEM_PLASMA_PISTOL_CLIP");
            Debug.Assert(newPistol.ShotsLeft == 13);
            Debug.Assert(newCannon.ItemInfo.Id == "ITEM_REPEATER_CANNON");
            Debug.Assert(newCannon.AmmoInfo.Id == "ITEM_R_C_HE_CLIP");
            Debug.Assert(newCannon.ShotsLeft == 14);

            Debug.Assert(ci.ItemAt(4, 0).ItemInfo.Id == "ITEM_LASER_RIFLE");
        }

        /// <summary>
        /// Test adding and removing armor
        /// </summary>
        [Conditional("DEBUG")]
        private static void TestArmor()
        {
            Combatant combatant = new Combatant(null, Team.XCorp);
            CombatantInventory ci = combatant.Inventory;
            Item pistol = Xenocide.StaticTables.ItemList["ITEM_PISTOL"].Manufacture();
            Item armor1 = Xenocide.StaticTables.ItemList["ITEM_COMBAT_POWERSUIT"].Manufacture();
            Item armor2 = Xenocide.StaticTables.ItemList["ITEM_ANTI_GRAV_POWERSUIT"].Manufacture();

            Debug.Assert("None" == combatant.Armor.Id);
            Debug.Assert(!combatant.Flyer);

            // can only put armor into armor slot
            Debug.Assert(!ci.CanFit(pistol, 4, 3));

            // can't put armor anywhere but armor slot
            Debug.Assert(!ci.CanFit(armor1, 0, 0));
            Debug.Assert(!ci.CanFit(armor1, 4, 0));
            Debug.Assert( ci.CanFit(armor1, 4, 3));
            Debug.Assert( ci.CanFit(armor2, 4, 3));

            // carrying armor sets combatant's armor property
            ci.Insert(armor1, 4, 3);
            Debug.Assert("ITEM_COMBAT_POWERSUIT" == combatant.Armor.Id);
            Debug.Assert(!ci.CanFit(armor2, 4, 3));
            Debug.Assert(!combatant.Flyer);

            // removing armor sets combatant's armor property to null.
            ci.Remove(armor1);
            Debug.Assert(ci.CanFit(armor2, 4, 3));
            Debug.Assert("None" == combatant.Armor.Id);

            // changing armor can adjust combatant's Flyer property
            Debug.Assert(!combatant.Flyer);
            ci.Insert(armor2, 4, 3);
            Debug.Assert(combatant.Flyer);
            ci.Remove(armor2);
            Debug.Assert(!combatant.Flyer);
        }

        #endregion UnitTests
    }
}
