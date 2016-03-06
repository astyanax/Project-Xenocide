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
* @file UnitTestCombatantFactory.cs
* @date Created: 2008/01/06
* @author File creator: David Teviotdale
* @author Credits: nil
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;


using ProjectXenocide.Utils;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Battlescape.Combatants;

#endregion

namespace ProjectXenocide.Model.StaticData.Battlescape
{
    /// <summary>
    /// Tests the Combatants class
    /// </summary>
    public partial class CombatantFactory
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            Xenocide.StaticTables.CombatantFactory.TestMakeAlien();
        }

        /// <summary>
        /// Test the MakeAlien() function
        /// </summary>
        [Conditional("DEBUG")]
        private void TestMakeAlien()
        {
            // fetch race/rank combination that exists
            CombatantInfo info = combatantInfos[FindIndex(Race.Morlock, AlienRank.Soldier)];
            Debug.Assert((Race.Morlock == info.Race) && (AlienRank.Soldier == info.Rank));

            // fetch race/rank combination that doesn't exist
            info = combatantInfos[FindIndex(Race.Cloak, AlienRank.Navigator)];
            Debug.Assert((Race.Cloak == info.Race) && (AlienRank.Soldier == info.Rank));

            // fetch a terror species directly
            info = combatantInfos[FindIndex(Race.Silabrate, AlienRank.Terrorist)];
            Debug.Assert((Race.Silabrate == info.Race) && (AlienRank.Terrorist == info.Rank));

            // try for terrorist via ownerRace
            info = combatantInfos[FindIndex(Race.Viper, AlienRank.Terrorist)];
            Debug.Assert((Race.Spawn == info.Race) && (AlienRank.Terrorist == info.Rank));

            // this race has two terror unit types
            bool silabrateFound    = false;
            bool ventriculantFound = false;
            for (int i = 0; i < 50; ++i)
            {
                info = combatantInfos[FindIndex(Race.Morlock, AlienRank.Terrorist)];
                if (Race.Silabrate == info.Race)
                {
                    silabrateFound = true;
                }
                if (Race.Ventriculant == info.Race)
                {
                    ventriculantFound = true;
                }
            }
            Debug.Assert(silabrateFound && ventriculantFound);
        }

        #endregion UnitTests
    }
}