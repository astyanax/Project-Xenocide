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
* @file UnitTestTerrainPathfinding.cs
* @date Created: 2008/01/19
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

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// Test cases for the pathfinding functions associated with a terrain
    /// </summary>
    public partial class Terrain
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            TestIsLineOfSight();
            Mission mission = new MockMission();
            Terrain terrain = mission.CreateTerrain();
            terrain.TestCanMoveUp();
            terrain.TestCanMoveDown();
            terrain.TestCanMoveOff();
            terrain.TestCanMoveHorizontal();
            terrain.TestListAccessbleNeighbours();
        }

        /// <summary>
        /// Test IsLineOfSight()
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestIsLineOfSight()
        {
            TerrainBuilder builder = new TestTerrainBuilder(TestTerrain.LineOfSight);
            Terrain        terrain = new Terrain(new MockMission(builder));
            builder.BuildCells(terrain);
            Debug.Assert( terrain.IsLineOfSight(new Vector3(), new Vector3()));
            Debug.Assert( terrain.IsLineOfSight(new Vector3(0, 1, 0), new Vector3(0, 0, 2)));
            Debug.Assert( terrain.IsLineOfSight(new Vector3(), new Vector3(1, 0, 0)));
            Debug.Assert( terrain.IsLineOfSight(new Vector3(), new Vector3(1, 0, 1)));
            Debug.Assert(!terrain.IsLineOfSight(new Vector3(), new Vector3(1, 0, 2)));
            Debug.Assert( terrain.IsLineOfSight(new Vector3(), new Vector3(1, 0, 3)));

            Debug.Assert( terrain.IsLineOfSight(new Vector3(1, 0, 0), new Vector3(0, 0, 0)));
            Debug.Assert( terrain.IsLineOfSight(new Vector3(1, 0, 0), new Vector3(0, 0, 1)));
            Debug.Assert(!terrain.IsLineOfSight(new Vector3(1, 0, 0), new Vector3(0, 0, 2)));
            Debug.Assert( terrain.IsLineOfSight(new Vector3(1, 0, 0), new Vector3(0, 0, 3)));

            // toy vertical tests
            Debug.Assert( terrain.IsLineOfSight(new Vector3(0, 0, 0), new Vector3(0, 1, 0)));
            Debug.Assert( terrain.IsLineOfSight(new Vector3(0, 1, 0), new Vector3(0, 0, 0)));
            Debug.Assert(!terrain.IsLineOfSight(new Vector3(0, 6, 0), new Vector3(0, 7, 0)));
            Debug.Assert(!terrain.IsLineOfSight(new Vector3(0, 7, 0), new Vector3(0, 6, 0)));
        }

        /// <summary>
        /// Test CanMoveUp()
        /// </summary>
        [Conditional("DEBUG")]
        public void TestCanMoveUp()
        {
            // can't go up because can't fly
            Debug.Assert(!CanMoveUp(1, 0, 1, false));

            // can go up because can fly
            Debug.Assert(CanMoveUp(1, 0, 1, true));

            // can go up, grav lift
            Debug.Assert(CanMoveUp(2, 0, 0, false));

            // can't go up, top level
            Debug.Assert(!CanMoveUp(2, 1, 1, true));

            // can't go up, solid floor above
            Debug.Assert(!CanMoveUp(2, 0, 1, true));
        }

        /// <summary>
        /// Test CanMoveDown()
        /// </summary>
        [Conditional("DEBUG")]
        public void TestCanMoveDown()
        {
            // can't go down, bottom level
            Debug.Assert(!CanMoveDown(0, 0, 0));

            // can go down, grav lift
            Debug.Assert(CanMoveDown(2, 1, 0));

            // can't go down, solid floor
            Debug.Assert(!CanMoveDown(2, 1, 1));

            // can go down, no floor
            Debug.Assert(CanMoveDown(1, 1, 1));

            // can't go down, no floor, but cell blocked
            Debug.Assert(!CanMoveDown(3, 1, 0));
        }

        /// <summary>
        /// Test CanMoveOff()
        /// </summary>
        [Conditional("DEBUG")]
        public void TestCanMoveOff()
        {
            // can't move off a cell that's blocked
            Debug.Assert(!GetGroundFace(3, 0, 0).CanMoveOff(true));

            // can't move off a cell because we can't fly
            Debug.Assert(!GetGroundFace(1, 1, 1).CanMoveOff(false));

            // can move off the cell, because we can fly
            Debug.Assert(GetGroundFace(1, 1, 1).CanMoveOff(true));

            // cell has floor so we can move off it
            Debug.Assert(GetGroundFace(0, 0, 0).CanMoveOff(false));
            Debug.Assert(GetGroundFace(0, 0, 2).CanMoveOff(false));
            Debug.Assert(GetGroundFace(1, 0, 2).CanMoveOff(false));
        }

        /// <summary>
        /// Check if a combatant can move east from a specified cell
        /// </summary>
        /// <param name="x">location of starting cell</param>
        /// <param name="y">location of starting cell</param>
        /// <param name="z">location of starting cell</param>
        /// <param name="canFly">allowed to use flying to get to destination cell?</param>
        /// <param name="expected">value "CanMoveHorizontal" is expected to return</param>
        [Conditional("DEBUG")]
        private void TestCanMoveEast(int x, int y, int z, bool canFly, int expected)
        {
            Debug.Assert(expected == CanMoveHorizontal(x, y, z, canFly, x + 1, z, HasWestWall));
        }

        /// <summary>
        /// Test CanMoveHorizontal()
        /// </summary>
        [Conditional("DEBUG")]
        public void TestCanMoveHorizontal()
        {
            // can't go east, at eastmost point
            TestCanMoveEast(3, 0, 2, false, 2);

            // can't go east, we can't fly
            TestCanMoveEast(1, 1, 1, false, 2);

            // can go east, we can fly
            TestCanMoveEast(1, 1, 1, true, 0);

            // going east will take us up a level
            TestCanMoveEast(1, 0, 2, false, 1);

            // going east will keep us on same level
            TestCanMoveEast(0, 0, 0, false, 0);

            // can't go east, blocked by a wall
            TestCanMoveEast(1, 0, 1, false, 2);

            // can't go east, steps are too high
            TestCanMoveEast(2, 0, 2, false, 2);

            // can't go east, destination is blocked
            TestCanMoveEast(2, 0, 0, false, 2);

            // can go east, height is just part step
            TestCanMoveEast(0, 0, 2, false, 0);
        }

        /// <summary>
        /// Test ListAccessbleNeighbours()
        /// </summary>
        [Conditional("DEBUG")]
        public void TestListAccessbleNeighbours()
        {
            List<MoveData> neighbours = new List<MoveData>();
            ListAccessbleNeighbours(2, 2, 0, false, neighbours);
            Debug.Assert(2 == neighbours.Count);
            Debug.Assert(neighbours[0] == new MoveData(3, 2, 0, 0));
            Debug.Assert(neighbours[0] != new MoveData(1, 2, 0, 0));
            Debug.Assert(neighbours[1] == new MoveData(1, 2, 0, 0));

            ListAccessbleNeighbours(3, 2, 1, false, neighbours);
            Debug.Assert(2 == neighbours.Count);
            Debug.Assert(neighbours[0] == new MoveData(3, 2, 0, 0));
            Debug.Assert(neighbours[1] == new MoveData(3, 2, 2, 0));

            ListAccessbleNeighbours(2, 2, 2, false, neighbours);
            Debug.Assert(2 == neighbours.Count);
            Debug.Assert(neighbours[0] == new MoveData(3, 2, 2, 0));
            Debug.Assert(neighbours[1] == new MoveData(1, 2, 2, 0));

            ListAccessbleNeighbours(1, 2, 1, false, neighbours);
            Debug.Assert(5 == neighbours.Count);
            Debug.Assert(neighbours[0] == new MoveData(1, 2, 0, 0));
            Debug.Assert(neighbours[1] == new MoveData(1, 2, 2, 0));
            Debug.Assert(neighbours[2] == new MoveData(0, 2, 1, 0));
            Debug.Assert(neighbours[3] == new MoveData(0, 1, 0, 0));
            Debug.Assert(neighbours[4] == new MoveData(0, 2, 2, 0));

            // no neighbour case
            ListAccessbleNeighbours(2, 2, 1, false, neighbours);
            Debug.Assert(0 == neighbours.Count);
        }

        #endregion UnitTests
    }
}
