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
* @file UnitTestPathfinder.cs
* @date Created: 2008/01/20
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;

using Microsoft.Xna.Framework;
using ProjectXenocide.Model.Battlescape.Combatants;

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// Code to test the Pathfinder class.
    /// </summary>
    public partial class Pathfinder
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            OpenList.RunTests();
            TestFindPath();
        }

        /// <summary>
        /// Code to test the Pathfinder.OpenList class.
        /// </summary>
        private partial class OpenList
        {
            /// <summary>
            /// Run set of tests
            /// </summary>
            [Conditional("DEBUG")]
            public static void RunTests()
            {
                OpenList olist = new OpenList();
                MoveData dummy = new MoveData();
                olist.Add(10, dummy);
                olist.Add(3, dummy);
                olist.Add(15, dummy);
                Debug.Assert( 3.0f == olist.list[0].cost);
                Debug.Assert(10.0f == olist.list[1].cost);
                Debug.Assert(15.0f == olist.list[2].cost);
            }
        }

        /// <summary>
        /// test FindPath()
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestFindPath()
        {
            Mission mission  = new MockMission();
            Battle battlescape = new Battle(mission);
            Terrain terrain  = battlescape.Terrain;
            Pathfinder finder  = new Pathfinder(terrain);
            Combatant  soldier = battlescape.Teams[1].Combatants[0];
            Combatant  alien   = battlescape.Teams[0].Combatants[0];

            List<MoveData> path = new List<MoveData>();
            Debug.Assert(finder.FindPath(0, 0, 0, false, 2, 0, 1, path));
            Debug.Assert(4 == path.Count);
            Debug.Assert(path[0] == new MoveData(0, 0, 0, 2));
            Debug.Assert(path[1] == new MoveData(1, 0, 0, 2));
            Debug.Assert(path[2] == new MoveData(2, 0, 0, 2));
            Debug.Assert(path[3] == new MoveData(2, 0, 1, 2));

            // test FindCombatantAt
            Debug.Assert(null == battlescape.FindCombatantAt(new Vector3(2, 0, 0)));
            Debug.Assert(soldier == battlescape.FindCombatantAt(new Vector3(0, 0, 0)));
            Debug.Assert(alien   == battlescape.FindCombatantAt(new Vector3(3, 1, 1)));
            Debug.Assert(alien   == battlescape.FindCombatantAt(alien.Position));

            // move alien to block path
            terrain.MoveCombatant(alien, new MoveData(3, 1, 1, 0), new MoveData(2, 0, 0, 0));
            Debug.Assert(!finder.FindPath(0, 0, 0, false, 2, 0, 1, path));

            // move so this path is blocked, but another is open
            terrain.MoveCombatant(alien, new MoveData(2, 0, 0, 0), new MoveData(1, 0, 0, 0));
            Debug.Assert(finder.FindPath(0, 0, 0, false, 2, 0, 1, path));
            Debug.Assert(8 == path.Count);
            Debug.Assert(path[0] == new MoveData(0, 0, 0, 2));
            Debug.Assert(path[1] == new MoveData(0, 0, 1, 2));
            Debug.Assert(path[2] == new MoveData(0, 0, 2, 2));
            Debug.Assert(path[3] == new MoveData(1, 0, 2, 2));
            Debug.Assert(path[4] == new MoveData(2, 1, 1, 2));
            Debug.Assert(path[5] == new MoveData(2, 1, 0, 2));
            Debug.Assert(path[6] == new MoveData(2, 0, 0, 2));
            Debug.Assert(path[7] == new MoveData(2, 0, 1, 2));

            Debug.Assert(!finder.FindPath(1, 0, 2, false, 0, 1, 2, path));
        }

        #endregion UnitTests
    }
}
