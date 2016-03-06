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
* @file UnitTestCombatant.cs
* @date Created: 2008/03/16
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Battlescape;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;

namespace ProjectXenocide.Model.Battlescape.Combatants
{
    /// <summary>Unit Tests for Combatant</summary>
    public partial class Combatant
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            TestHealing();
            TestFatalWoundsAndBleeding();
        }

        /// <summary>Basic Alive/Dead test</summary>
        [Conditional("DEBUG")]
        private static void TestHealing()
        {
            // get a soldier
            GeoData   geoData   = Xenocide.GameState.GeoData;
            Person soldier      = Xenocide.StaticTables.ItemList["ITEM_PERSON_SOLDIER"].Manufacture() as Person;
            geoData.Outposts.Add(OutpostInventory.ConstructTestOutpost());
            geoData.Outposts[0].Inventory.Add(soldier, false);

            // injure the solider
            Combatant combatant = soldier.Combatant;
            int injury = 5;
            combatant.Stats[Statistic.Health]       = injury;
            combatant.Stats[Statistic.InjuryDamage] = injury;

            // 0 HP isn't dead, -1 is
            Debug.Assert(!combatant.IsDead);
            ++combatant.Stats[Statistic.InjuryDamage];
            Debug.Assert(combatant.IsDead);
            --combatant.Stats[Statistic.Health];
            Debug.Assert(combatant.IsDead);
        }

        /// <summary>What it says on the tin :-)</summary>
        [Conditional("DEBUG")]
        private static void TestFatalWoundsAndBleeding()
        {
            // get a soldier
            Mission mission = new MockMission();
            Battle battlescape = new Battle(mission);
            Combatant combatant = battlescape.Teams[1].Combatants[0];

            // Setup random generator
            List<int> randomNumbers = new List<int>();
            randomNumbers.Add(14); // Damage
            randomNumbers.Add(1);  // Body part
            randomNumbers.Add(1);  // Fatal wounds
            randomNumbers.Add(13); // Damage
            randomNumbers.Add(2);  // Body part
            randomNumbers.Add(2);  // Fatal wounds
            // randomNumbers.Add(10);
            Xenocide.Rng.RigDice(randomNumbers);

            // Set health of soldier
            combatant.Stats[Statistic.Health] = 35;
            combatant.Stats[Statistic.TimeUnits] = 60;

            // Run OnStartTurn
            combatant.OnStartTurn();

            //Verify time units, stamina and accuracy
            Debug.Assert(combatant.Stats[Statistic.TimeUnitsLeft] == 60);
            Debug.Assert(combatant.Accuracy(ActiveArm.Both) == 70);
            Debug.Assert(combatant.Stats[Statistic.StaminaLeft] == 50);

            // Hit the soldier (should use up first random number for damage and second random 
            // number for fatal wounds body part and third number for the fata wounds)
            combatant.Hit(new DamageInfo(50, DamageType.Plasma), new Vector3(1, 0, 0));
            Debug.Assert(combatant.Stats[Statistic.InjuryDamage] == 15);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsHead] == 0);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsBody] == 2);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsLeftArm] == 0);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsRightArm] == 0);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsLeftLeg] == 0);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsRightLeg] == 0);
            Debug.Assert(combatant.TotalFatalWounds == 2);
            //Hit the soldier again
            combatant.Hit(new DamageInfo(50, DamageType.Plasma), new Vector3(1, 0, 0));
            Debug.Assert(combatant.Stats[Statistic.InjuryDamage] == 29);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsHead] == 0);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsBody] == 2);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsLeftArm] == 3);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsRightArm] == 0);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsLeftLeg] == 0);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsRightLeg] == 0);
            Debug.Assert(combatant.TotalFatalWounds == 5);

            // Let the soldier bleed
            combatant.Bleed();
            Debug.Assert(combatant.Stats[Statistic.InjuryDamage] == 34);

            // Heal one wound
            combatant.Heal(BodyParts.Body);
            Debug.Assert(combatant.Stats[Statistic.InjuryDamage] == 34);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsHead] == 0);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsBody] == 1);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsLeftArm] == 3);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsRightArm] == 0);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsLeftLeg] == 0);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsRightLeg] == 0);
            Debug.Assert(combatant.TotalFatalWounds == 4);

            // limb that has no fatal wound
            combatant.Heal(BodyParts.LeftLeg);
            Debug.Assert(combatant.Stats[Statistic.InjuryDamage] == 31);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsLeftLeg] == 0);
            Debug.Assert(combatant.TotalFatalWounds == 4);

            // Let the soldier bleed again, the soldier should die from fatal wounds.
            combatant.Bleed();
            Debug.Assert(combatant.Stats[Statistic.InjuryDamage] == 35);
            combatant.Bleed();
            Debug.Assert(combatant.Stats[Statistic.InjuryDamage] == 39);
            Debug.Assert(combatant.IsDead);

            // Perform post mission cleanup
            combatant.PostMissionCleanup();
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsHead] == 0);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsBody] == 0);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsLeftArm] == 0);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsRightArm] == 0);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsLeftLeg] == 0);
            Debug.Assert(combatant.Stats[Statistic.FatalWoundsRightLeg] == 0);

            // Wake up the soldier from the dead and test wound recovery days
            combatant.Stats[Statistic.InjuryDamage] = 17;
            Debug.Assert(!combatant.IsDead);
            Debug.Assert(combatant.IsInjured);
            combatant.PostMissionCleanup();
            Debug.Assert(combatant.Stats[Statistic.InjuryDamage] == 17);

            // Test wound recovery
            combatant.DailyHealing();
            Debug.Assert(combatant.Stats[Statistic.InjuryDamage] == 16);
            combatant.DailyHealing();
            Debug.Assert(combatant.Stats[Statistic.InjuryDamage] == 15);
            for (int i = 0; i < 14; i++)
            {
                combatant.DailyHealing();
            }
            Debug.Assert(combatant.Stats[Statistic.InjuryDamage] == 1);
            combatant.DailyHealing();
            Debug.Assert(combatant.Stats[Statistic.InjuryDamage] == 0);
            combatant.DailyHealing();
            Debug.Assert(combatant.Stats[Statistic.InjuryDamage] == 0);

            // Injure the combatant everywhere
            combatant.Stats[Statistic.FatalWoundsBody] = 2;
            combatant.Stats[Statistic.FatalWoundsHead] = 2;
            combatant.Stats[Statistic.FatalWoundsLeftArm] = 2;
            combatant.Stats[Statistic.FatalWoundsLeftLeg] = 2;
            combatant.Stats[Statistic.FatalWoundsRightArm] = 3;
            combatant.Stats[Statistic.FatalWoundsRightLeg] = 3;
            combatant.Stats[Statistic.TimeUnits] = 100;
            combatant.Stats[Statistic.TimeUnitsLeft] = 100;
            combatant.Stats[Statistic.StaminaLeft] = 0;
            combatant.OnStartTurn();

            // Check time units, energy and accuracy
            Debug.Assert(combatant.Stats[Statistic.TimeUnitsLeft] == 50);
            Debug.Assert(combatant.Accuracy(ActiveArm.Both) == 49);
            Debug.Assert(combatant.Stats[Statistic.StaminaLeft] == 10);
        }

        #endregion UnitTests
    }
}
