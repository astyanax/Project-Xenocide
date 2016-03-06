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
* @file UnitTestMoveOrder.cs
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

#endregion

namespace ProjectXenocide.Model.Battlescape.Combatants
{
    /// <summary>
    /// Unit tests for MoveOrder
    /// </summary>
    public partial class MoveOrder : Order
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            TestCalcTurnAngle();
            TestRotate();
            TestTranslateHorizontal();
            TestTranslateVertical();
            TestTranslateCost();
            TestCalcTurnCost();
            TestMaximumPathCells();
        }

        /// <summary>
        /// Test CalcTurnAngle()
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestCalcTurnAngle()
        {
            Mission mission = new MockMission();
            Battle battlescape = new Battle(mission);
            Combatant combatant = battlescape.Teams[1].Combatants[0];

            List<MoveData> path = new List<MoveData>();
            path.Add(new MoveData(0, 0, 1, 0));
            path.Add(new MoveData(0, 0, 0, 0));
            MoveOrder order = new MoveOrder(combatant, battlescape, path);
            Util.DebugTestFloatValuesSame(Math.PI / 2, order.CalcTurnAngle(0, 0));

            path[0] = new MoveData(1, 0, 0, 0);
            path[1] = new MoveData(0, 0, 1, 0);
            Util.DebugTestFloatValuesSame(MathHelper.PiOver2, order.CalcTurnAngle(3 * MathHelper.Pi / 4, 0));

            path[0] = new MoveData(1, 0, 1, 0);
            path[1] = new MoveData(0, 0, 0, 0);
            Util.DebugTestFloatValuesSame(-MathHelper.PiOver2, order.CalcTurnAngle(-3 * MathHelper.Pi / 4, 0));
        }

        /// <summary>
        /// Test CalcTurnCost()
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestCalcTurnCost()
        {
            Mission mission = new MockMission();
            Battle battlescape = new Battle(mission);
            Combatant combatant = battlescape.Teams[1].Combatants[0];

            List<MoveData> path = new List<MoveData>();
            path.Add(new MoveData(5, 0, 5, 0));
            path.Add(new MoveData(4, 0, 5, 0));
            MoveOrder order = new MoveOrder(combatant, battlescape, path);
            Debug.Assert(4 == order.CalcTurnCost(0, 0));

            Debug.Assert(0 == order.CalcTurnCost(MathHelper.Pi, 0));
            Debug.Assert(1 == order.CalcTurnCost(-3 * MathHelper.Pi / 4, 0));
            Debug.Assert(1 == order.CalcTurnCost(3 * MathHelper.Pi / 4, 0));
        }

        /// <summary>
        /// Test Rotate()
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestRotate()
        {
            Mission mission = new MockMission();
            Battle battlescape = new Battle(mission);
            Combatant  combatant = battlescape.Teams[1].Combatants[0];
            combatant.Stats[Statistic.TimeUnitsLeft] = 20;

            // should turn -90 degrees
            List<MoveData> path = new List<MoveData>();
            path.Add(new MoveData(0, 0, 0, 0));
            path.Add(new MoveData(0, 0, 1, 0));
            MoveOrder order = new MoveOrder(combatant, battlescape, path);

            // turn half way
            Debug.Assert(0 == combatant.Heading);
            order.Update(0.125);
            Util.DebugTestFloatValuesSame(-MathHelper.PiOver4, combatant.Heading);
            Debug.Assert(FinishCode.Executing == order.Finished);
            Debug.Assert(Movement.Rotation == order.movement);
            Debug.Assert(0 == order.unusedSeconds);

            // turn rest of way
            order.Update(0.25);
            Util.DebugTestFloatValuesSame(-MathHelper.PiOver2, combatant.Heading);
            Debug.Assert(Movement.Translation == order.movement);
            Debug.Assert((0.1249f <= order.unusedSeconds) && (order.unusedSeconds <= 0.1251f));
        }

        /// <summary>
        /// Test Translate() with a horizontal move
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestTranslateHorizontal()
        {
            Mission mission = new MockMission();
            Battle battlescape = new Battle(mission);
            Combatant combatant = battlescape.Teams[1].Combatants[0];

            // move one cell south, and up a stair
            List<MoveData> path = new List<MoveData>();
            path.Add(new MoveData(0, 0, 1, 0));
            path.Add(new MoveData(0, 0, 2, 0));
            MoveOrder order = new MoveOrder(combatant, battlescape, path);
            combatant.Heading = -MathHelper.PiOver2;
            combatant.Stats[Statistic.TimeUnitsLeft] = 20;
            Vector3 pos = new Vector3(0, 0, 1);
            battlescape.Terrain.MoveCombatant(combatant, new MoveData(), new MoveData(pos));
            battlescape.Terrain.ToFloorCenter(ref pos);
            combatant.Position = pos;

            // toggle from rotate to move
            Debug.Assert(Movement.Rotation == order.movement);
            order.Update(0);
            Debug.Assert(Movement.Translation == order.movement);
            Debug.Assert(0 == order.unusedSeconds);

            // move half way
            order.Update(0.125);
            Debug.Assert(-MathHelper.PiOver2 == combatant.Heading);
            Debug.Assert(FinishCode.Executing == order.Finished);
            Debug.Assert(Movement.Translation == order.movement);
            Debug.Assert(0 == order.unusedSeconds);
            Debug.Assert(combatant.Position == new Vector3(0.5f, (0.33f / 2), 2f));

            // move rest of way
            order.Update(0.25);
            Debug.Assert(combatant.Position == new Vector3(0.5f, (0.33f), 2.5f));
            Debug.Assert(-MathHelper.PiOver2 == combatant.Heading);
            Debug.Assert(Movement.Translation == order.movement);
            Debug.Assert((0.1249f <= order.unusedSeconds) && (order.unusedSeconds <= 0.1251f));
            Debug.Assert(FinishCode.Normal == order.Finished);
        }

        /// <summary>
        /// Test Translate() with a vertical move
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestTranslateVertical()
        {
            Mission mission = new MockMission();
            Battle battlescape = new Battle(mission);
            Combatant combatant = battlescape.Teams[1].Combatants[0];

            // move one cell up a grav lift
            List<MoveData> path = new List<MoveData>();
            path.Add(new MoveData(2, 0, 0, 0));
            path.Add(new MoveData(2, 1, 0, 0));
            MoveOrder order = new MoveOrder(combatant, battlescape, path);
            combatant.Heading = -MathHelper.PiOver4;
            combatant.Stats[Statistic.TimeUnitsLeft] = 20;
            Vector3 pos = new Vector3(2, 0, 0);
            battlescape.Terrain.ToFloorCenter(ref pos);
            combatant.Position = pos;
            battlescape.Terrain.MoveCombatant(combatant, new MoveData(), new MoveData(pos));

            // toggle from rotate to move
            Debug.Assert(Movement.Rotation == order.movement);
            order.Update(0);
            Debug.Assert(Movement.Translation == order.movement);
            Debug.Assert(0 == order.unusedSeconds);

            // move half way
            order.Update(0.125);
            Debug.Assert(-MathHelper.PiOver4 == combatant.Heading);
            Debug.Assert(FinishCode.Executing == order.Finished);
            Debug.Assert(Movement.Translation == order.movement);
            Debug.Assert(0 == order.unusedSeconds);
            Debug.Assert(combatant.Position == new Vector3(2.5f, 0.5f, 0.5f));

            // move rest of way
            order.Update(0.25);
            Debug.Assert(combatant.Position == new Vector3(2.5f, 1, 0.5f));
            Debug.Assert(-MathHelper.PiOver4 == combatant.Heading);
            Debug.Assert(Movement.Translation == order.movement);
            Debug.Assert((0.1249f <= order.unusedSeconds) && (order.unusedSeconds <= 0.1251f));
            Debug.Assert(FinishCode.Normal == order.Finished);
        }

        /// <summary>
        /// Test Translate() across multiple cells
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestTranslateCost()
        {
            Mission mission = new MockMission();
            Battle battlescape = new Battle(mission);
            Combatant combatant = battlescape.Teams[1].Combatants[0];

            // move one cell up a grav lift
            List<MoveData> path = new List<MoveData>();
            path.Add(new MoveData(0, 0, 0, 0));
            path.Add(new MoveData(1, 0, 0, 0));
            path.Add(new MoveData(2, 0, 0, 0));
            path.Add(new MoveData(3, 0, 0, 0));
            MoveOrder order = new MoveOrder(combatant, battlescape, path);
            combatant.Heading = 0;
            Vector3 pos = new Vector3(2, 0, 0);
            battlescape.Terrain.ToFloorCenter(ref pos);
            combatant.Position = pos;

            // just enough points to move 2 cells
            combatant.Stats[Statistic.TimeUnitsLeft] = 16;

            // toggle from rotate to move
            Debug.Assert(Movement.Rotation == order.movement);
            order.Update(0);
            Debug.Assert(Movement.Translation == order.movement);
            Debug.Assert(0 == order.unusedSeconds);

            // move one cell
            order.Update(0.25);
            Debug.Assert(combatant.Position == new Vector3(1.5f, 0.0f, 0.5f));
            Debug.Assert(FinishCode.Executing == order.Finished);
            combatant.Stats[Statistic.TimeUnitsLeft] = 8;

            // move second cell
            order.Update(0.25);
            Debug.Assert(combatant.Position == new Vector3(2.5f, 0.0f, 0.5f));
            Debug.Assert(FinishCode.Executing == order.Finished);
            combatant.Stats[Statistic.TimeUnitsLeft] = 0;

            // try a move when have insufficent points
            order.Update(0.25);
            Debug.Assert(combatant.Position == new Vector3(2.5f, 0.0f, 0.5f));
            Debug.Assert(FinishCode.Interupted == order.Finished);
        }

        /// <summary>
        /// Test MaximumPathCells()
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestMaximumPathCells()
        {
            Mission mission = new MockMission();
            Battle battlescape = new Battle(mission);
            Combatant combatant = battlescape.Teams[1].Combatants[0];
            combatant.Heading = 0;

            List<MoveData> path = new List<MoveData>();
            path.Add(new MoveData(1, 0, 1, 0));
            path.Add(new MoveData(1, 0, 0, 0));
            path.Add(new MoveData(0, 0, 0, 0));
            MoveOrder order = new MoveOrder(combatant, battlescape, path);

            // insufficent points to turn and move
            combatant.Stats[Statistic.TimeUnitsLeft] = 9;
            Debug.Assert(0 == order.MaximumPathCells());

            // sufficent points to turn and move one cell
            combatant.Stats[Statistic.TimeUnitsLeft] = 10;
            Debug.Assert(1 == order.MaximumPathCells());

            // insufficent points to turn and move two cells
            combatant.Stats[Statistic.TimeUnitsLeft] = 19;
            Debug.Assert(1 == order.MaximumPathCells());

            // sufficent points to turn and move two cells
            combatant.Stats[Statistic.TimeUnitsLeft] = 20;
            Debug.Assert(2 == order.MaximumPathCells());

            // way more points than needed, stop at last cell on path
            combatant.Stats[Statistic.TimeUnitsLeft] = 50;
            Debug.Assert(2 == order.MaximumPathCells());
        }

        #endregion UnitTests
    }
}
