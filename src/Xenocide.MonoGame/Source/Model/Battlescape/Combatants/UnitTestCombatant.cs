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

using System.Diagnostics;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Battlescape;
using ProjectXenocide.Model.StaticData.Items;

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
        }

        /// <summary>Basic Alive/Dead test</summary>
        [Conditional("DEBUG")]
        private static void TestHealing()
        {
            // get a soldier
            GeoData geoData = Xenocide.GameState.GeoData;
            Person soldier = Xenocide.StaticTables.ItemList["ITEM_PERSON_SOLDIER"].Manufacture() as Person;
            geoData.Outposts.Add(OutpostInventory.ConstructTestOutpost());
            geoData.Outposts[0].Inventory.Add(soldier, false);

            // injure the solider
            Combatant combatant = soldier.Combatant;
            int injury = 5;
            combatant.Stats[Statistic.Health] = injury;
            combatant.Stats[Statistic.InjuryDamage] = injury;

            // 0 HP isn't dead, -1 is
            Debug.Assert(!combatant.IsDead);
            ++combatant.Stats[Statistic.InjuryDamage];
            Debug.Assert(combatant.IsDead);
            --combatant.Stats[Statistic.Health];
            Debug.Assert(combatant.IsDead);
        }

        #endregion UnitTests
    }
}
