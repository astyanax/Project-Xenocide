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
* @file UnitTestShootOrder.cs
* @date Created: 2008/02/06
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

using ProjectXenocide.Utils;
using ProjectXenocide.Model.StaticData.Items;

#endregion

namespace ProjectXenocide.Model.Battlescape.Combatants
{
    /// <summary>
    /// Unit tests for ShootOrder (and ShootActionInfo)
    /// </summary>
    public partial class ShootOrder : Order
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            TestLineOfFire();
            TestUpdateVisibility();
            TestAmmoUsage();
        }

        /// <summary>Set initial conditions to run tests</summary>
        /// <param name="battlescape">battlescape that will be created</param>
        [Conditional("DEBUG")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#",
            Justification = "We're not idiots")]
        public static void TestSetup(ref Battle battlescape)
        {
            Mission mission = new MockMission();
            battlescape = new Battle(mission);
            Combatant alien = battlescape.Teams[0].Combatants[0];
            Combatant soldier = battlescape.Teams[1].Combatants[0];
            Terrain terrain = battlescape.Terrain;

            // Position combatants
            soldier.Heading = -MathHelper.PiOver2;
            soldier.Stats[Statistic.TimeUnitsLeft] = 60;
            Vector3 pos = new Vector3(3, 2, 2);
            terrain.MoveCombatant(soldier, new MoveData(), new MoveData(pos));
            terrain.ToFloorCenter(ref pos);
            soldier.Position = pos;

            alien.Stats[Statistic.TimeUnitsLeft] = 20;
            pos = new Vector3(1, 2, 2);
            terrain.MoveCombatant(alien, new MoveData(alien.Position), new MoveData(pos));
            terrain.ToFloorCenter(ref pos);
            alien.Position = pos;
        }

        /// <summary>
        /// Test Line of sight calcs()
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestLineOfFire()
        {
            Battle battlescape = null;
            TestSetup(ref battlescape);
            Combatant soldier = battlescape.Teams[1].Combatants[0];
            Terrain   terrain = battlescape.Terrain;
            Vector3   pos     = new Vector3();

            // line of sight, but not line of fire (blocked by alien)
            Debug.Assert(terrain.IsLineOfSight(soldier.Position, new Vector3(0, 2, 2)));
            Debug.Assert(!terrain.IsLineOfSight(soldier.Position, new Vector3(0, 2, 2), VisibilityChecks.LineOfFire, ref pos));
            Debug.Assert(1 == (int)pos.X);
            Debug.Assert(2 == (int)pos.Y);
            Debug.Assert(2 == (int)pos.Z);
            // if alien is target, line of fire isn't blocked.
            Debug.Assert(terrain.IsLineOfSight(soldier.Position, new Vector3(1, 2, 2), VisibilityChecks.LineOfFire, ref pos));
        }

        /// <summary>
        /// Find a shoot action that an item has
        /// </summary>
        /// <param name="weapon">to search for shoot action</param>
        /// <param name="action">found action</param>
        [Conditional("DEBUG")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#",
            Justification = "We're not idiots")]
        public static void TestGetShootAction(Item weapon, ref ShootActionInfo action)
        {
            foreach (ActionInfo actionInfo in weapon.ItemInfo.Actions)
            {
                action = actionInfo as ShootActionInfo;
                if (null != action)
                {
                    return;
                }
            }
            return;
        }

        /// <summary>
        /// Test consumption of ammo()
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestAmmoUsage()
        {
            Battle battlescape = null;
            TestSetup(ref battlescape);
            Combatant soldier = battlescape.Teams[1].Combatants[0];
            // soldier is equiped with assault rifle in right hand
            Item rifle = soldier.Inventory.ItemAt(0, 0);
            Debug.Assert("ITEM_ASSAULT_RIFLE" == rifle.ItemInfo.Id);
            ShootActionInfo action = null;
            TestGetShootAction(rifle, ref action);
            Vector3 target = new Vector3(2, 2, 2);
            int lastAmmo = rifle.ShotsLeft;

            // shoot should not be possible because soldier is looking wrong way
            Debug.Assert(ActionError.WrongDirection == action.IsLocationOk(battlescape, rifle, soldier, target));

            // turn to correct direction
            soldier.Heading = -MathHelper.Pi;
            Debug.Assert(ActionError.None == action.IsLocationOk(battlescape, rifle, soldier, target));

            while (action.Shots <= lastAmmo)
            {
                soldier.Stats[Statistic.TimeUnitsLeft] = 2000;
                Debug.Assert(ActionError.None == action.ActionAvailable(battlescape, rifle, soldier));
                Debug.Assert(ActionError.None == action.IsLocationOk(battlescape, rifle, soldier, target));
                soldier.Order = action.Start(battlescape, rifle, soldier, target);
                while (null != soldier.Order)
                {
                    // fire and move bullet to target
                    soldier.BattlescapeUpdate(1);
                    soldier.BattlescapeUpdate(1);
                }
                Debug.Assert(rifle.ShotsLeft == (lastAmmo - action.Shots));
                lastAmmo = rifle.ShotsLeft;
            }
            Debug.Assert(ActionError.InsufficientAmmo == action.ActionAvailable(battlescape, rifle, soldier));

            // now try laser
            Item laser = Xenocide.StaticTables.ItemList["ITEM_LASER_PISTOL"].Manufacture();
            soldier.Stats[Statistic.TimeUnitsLeft] = 2000;
            Debug.Assert(ActionError.None == action.ActionAvailable(battlescape, laser, soldier));
            Debug.Assert(ActionError.None == action.IsLocationOk(battlescape, laser, soldier, target));
            soldier.Order = action.Start(battlescape, laser, soldier, target);
            while (null != soldier.Order)
            {
                // fire and move bullet to target
                soldier.BattlescapeUpdate(1);
                soldier.BattlescapeUpdate(1);
            }
        }

        /// <summary>
        /// Test terrain's UpdateVisibility()
        /// </summary>
        /// <remarks>not really the right place for this test, but the test rig is all set up for it</remarks>
        [Conditional("DEBUG")]
        public static void TestUpdateVisibility()
        {
            Battle battlescape = null;
            TestSetup(ref battlescape);
            Combatant alien = battlescape.Teams[0].Combatants[0];
            Combatant soldier = battlescape.Teams[1].Combatants[0];

            // after setup, alien should see soldier, but solider should not see alien.
            battlescape.UpdateVisibility(soldier);
            Debug.Assert(1 == soldier.OponentsViewing);
            Debug.Assert(0 == soldier.OpponentsInView);
            Debug.Assert(0 == alien.OponentsViewing);
            Debug.Assert(1 == alien.OpponentsInView);

            // turn solider so alien is in soldier's field of view
            soldier.Heading = -(MathHelper.PiOver2 + MathHelper.PiOver4);
            battlescape.UpdateVisibility(soldier);
            Debug.Assert(1 == soldier.OponentsViewing);
            Debug.Assert(1 == soldier.OpponentsInView);
            Debug.Assert(1 == alien.OponentsViewing);
            Debug.Assert(1 == alien.OpponentsInView);

            // turn alien so soldier leaves field of view
            alien.Heading = +MathHelper.PiOver2;
            battlescape.UpdateVisibility(soldier);
            Debug.Assert(0 == soldier.OponentsViewing);
            Debug.Assert(1 == soldier.OpponentsInView);
            Debug.Assert(1 == alien.OponentsViewing);
            Debug.Assert(0 == alien.OpponentsInView);

            // move alien to long way away?
            alien.Position = new Vector3(-18.5f, 2, 2.5f);
            battlescape.UpdateVisibility(soldier);
            Debug.Assert(0 == soldier.OponentsViewing);
            Debug.Assert(0 == soldier.OpponentsInView);
            Debug.Assert(0 == alien.OponentsViewing);
            Debug.Assert(0 == alien.OpponentsInView);
        }



        #endregion UnitTests
    }
}
