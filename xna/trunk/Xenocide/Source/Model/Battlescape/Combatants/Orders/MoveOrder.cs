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
* @file MoveOrder.cs
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

#endregion

namespace ProjectXenocide.Model.Battlescape.Combatants
{
    /// <summary>
    /// Instruction telling combatant to change position on the battlescape
    /// </summary>
    public partial class MoveOrder : Order
    {
        /// <summary>Ctor for move that will change combatant's location</summary>
        /// <param name="combatant">The combatant performing the order</param>
        /// <param name="battlescape">the combatant's environment</param>
        /// <param name="path">the cells on the route the combatant is to follow</param>
        public MoveOrder(Combatant combatant, Battle battlescape, IList<MoveData> path)
            :
            base(combatant, battlescape)
        {
            this.path = path;
        }

        /// <summary>Ctor for movement that's just a turn in place</summary>
        /// <param name="combatant">The combatant performing the order</param>
        /// <param name="battlescape">the combatant's environment</param>
        /// <param name="heading">direction combatant is to face</param>
        public MoveOrder(Combatant combatant, Battle battlescape, double heading)
            :
            base(combatant, battlescape)
        {
            turnOnly = true;
            path = new List<MoveData>();
            path.Add(new MoveData(combatant.Position));
            double deltaX = 0.5 + Math.Cos(heading);
            double deltaZ = 0.5 - Math.Sin(heading);
            path.Add(new MoveData((int)(path[0].X + deltaX), 0, (int)(path[0].Z + deltaZ), 0));
        }

        /// <summary>Spend time performing the order</summary>
        /// <param name="seconds">time to update order's progress by</param>
        public override void Update(double seconds)
        {
            Debug.Assert(FinishCode.Executing == Finished);
            seconds += unusedSeconds;
            switch (movement)
            {
                case Movement.Rotation:
                    Rotate(seconds);
                    break;

                case Movement.Translation:
                    Translate(seconds);
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
        }

        /// <summary>How far along path can combatant travel?</summary>
        /// <returns>index to cell on path</returns>
        public int MaximumPathCells()
        {
            double heading = Combatant.Heading;
            int    tu      = Combatant.Stats[Statistic.TimeUnitsLeft];
            int    index   = 0;
            while (index < (path.Count - 1))
            {
                tu -= CalcTurnCost(heading, index);
                heading += CalcTurnAngle(heading, index);
                if (Math.PI < Math.Abs(heading))
                {
                    heading += Math.PI * -2 * Math.Sign(heading);
                }
                tu -= CalcTranslationCost(index);
                if (tu < 0)
                {
                    break;
                }
                index++;
            }
            return index;
        }

        /// <summary>See if movement has caused a reaction</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification="ToDo: function still to be implemented")]
        private void CheckForReactions()
        {
            Battlescape.UpdateVisibility(Combatant);
            //ToDo: implement
        }

        /// <summary>Combatant is turning about Y axis</summary>
        /// <param name="seconds">time combatant can spend turning</param>
        private void Rotate(double seconds)
        {
            if (PayForMove())
            {
                // figure out how much to turn, and how long it will take
                double turnAngle = CalcTurnAngle(Combatant.Heading, cellIndex);
                double timeNeeded = Math.Abs(turnAngle) / turningSpeed;

                if (seconds < timeNeeded)
                {
                    // insufficient time to turn, turn as much as possible
                    Combatant.Heading += (float)(turnAngle * seconds / timeNeeded);
                    unusedSeconds = 0;
                }
                else
                {
                    // enough time to turn, so finish turn, and pepare to move
                    Combatant.Heading += (float)turnAngle;
                    unusedSeconds = (seconds - timeNeeded);
                    movement = Movement.Translation;

                    // if move is just doing a turn, we're done
                    if (turnOnly)
                    {
                        Finished = FinishCode.Normal;
                    }

                    // reset TUs check for for next cell
                    movePaidFor = false;

                    // recalc visibility, might see something new.
                    Battlescape.UpdateVisibility(Combatant);
                }
            }
        }

        /// <summary>Check if combatant has sufficient TUs to move to next cell (and pay if necessary)</summary>
        /// <returns>true if combatant has paid for move to next cell</returns>
        private bool PayForMove()
        {
            if (!movePaidFor)
            {
                // figure out cost of move
                int cost = 0;
                if (Movement.Rotation == movement)
                {
                    cost = CalcTurnCost(Combatant.Heading, cellIndex);
                }
                else
                {
                    cost = CalcTranslationCost(cellIndex);
                }

                // if we can afford move, pay it
                movePaidFor = (cost <= Combatant.Stats[Statistic.TimeUnitsLeft]);
                if (movePaidFor)
                {
                    Combatant.Stats[Statistic.TimeUnitsLeft] -= cost;
                }
                else
                {
                    Finished = FinishCode.Interupted;
                }
            }
            return movePaidFor;
        }

        /// <summary>Combatant is moving X, Y, Z coords</summary>
        /// <param name="seconds">time combatant can spend moving</param>
        private void Translate(double seconds)
        {
            if (PayForMove())
            {
                // figure out how far to move, and how long it will take
                // note that pure vertical movement is special case
                int deltaX = path[cellIndex + 1].X - path[cellIndex].X;
                int deltaZ = path[cellIndex + 1].Z - path[cellIndex].Z;

                Vector3 end = path[cellIndex + 1].Vector3;
                Battlescape.Terrain.ToFloorCenter(ref end);
                Vector3 delta = end - Combatant.Position;

                // if moving horizontally, ignore vertical component of movement for time calcs.
                Vector3 hDelta = new Vector3(delta.X, 0, delta.Z);
                if ((0 == deltaX) && (0 == deltaZ))
                {
                    // pure vertical case
                    hDelta = delta;
                }

                double timeNeeded = hDelta.Length() / movingSpeed;
                if (seconds < timeNeeded)
                {
                    // insufficient time to move, move as far as possible
                    Combatant.Position += delta * (float)(seconds / timeNeeded);
                    unusedSeconds = 0;
                }
                else
                {
                    // enough time to move, so finish move
                    Combatant.Position = end;
                    unusedSeconds = (seconds - timeNeeded);
                    FinishTranslation();
                }
            }
        }

        /// <summary>Process combatant has moved into another cell</summary>
        private void FinishTranslation()
        {
            ++cellIndex;
            Battlescape.Terrain.MoveCombatant(Combatant, path[cellIndex - 1], path[cellIndex]);
            CheckForReactions();

            // if no reactions, see what to do next
            if (FinishCode.Executing == Finished)
            {
                // are there more moves?
                if (cellIndex < (path.Count - 1))
                {
                    // yes, more moves to do
                    //... check if we need to change direction
                    if (0 != CalcTurnAngle(Combatant.Heading, cellIndex))
                    {
                        movement = Movement.Rotation;
                    }

                    // reset TUs check for for next cell
                    movePaidFor = false;
                }
                else
                {
                    // no more moves. Indicate done
                    Finished = FinishCode.Normal;
                }
            }
        }

        /// <summary>
        /// Calculate shortest angle to turn through to get from a heading to angle to enter next cell on path
        /// </summary>
        /// <param name="startHeading">starting heading, in radians</param>
        /// <param name="index">index to entry in path that has location of cell</param>
        /// <returns>shortest angle, in radians</returns>
        private double CalcTurnAngle(double startHeading, int index)
        {
            int deltaX = path[index + 1].X - path[index].X;
            int deltaZ = path[index + 1].Z - path[index].Z;
            return Terrain.CalcTurnAngle(startHeading, deltaX, deltaZ);
        }

        /// <summary>Figure out what it's going to cost to move a cell to next on on path</summary>
        /// <param name="index">index to entry in path that has location of current cell</param>
        /// <returns>cost, in TUs</returns>
        private int CalcTranslationCost(int index)
        {
            int deltaX = path[index + 1].X - path[index].X;
            int deltaZ = path[index + 1].Z - path[index].Z;

            if ((0 == deltaX) || (0 == deltaZ))
            {
                // north/south/east/west/up/down movement
                // cost is same for all
                return movingCost;
            }
            else
            {
                // must be NE/NW/SE/SW movement
                return movingCost * 3 / 2;
            }
        }

        /// <summary>Figure out what it's going to cost to turn from a heading to next on on path</summary>
        /// <returns>cost, in TUs</returns>
        /// <param name="startHeading">the heading, in radians</param>
        /// <param name="index">index to entry in path that has location of cell</param>
        private int CalcTurnCost(double startHeading, int index)
        {
            return CalcTurnCost(CalcTurnAngle(startHeading, index));
        }

        /// <summary>Figure out the TUs it will cost to turn a given number of radians</summary>
        /// <returns>cost, in TUs</returns>
        /// <param name="angle">turning distance, in radians</param>
        public static int CalcTurnCost(double angle)
        {
            // add a bit more to angle to deal with quantization
            double bias = MathHelper.ToRadians(15);
            return (int)((Math.Abs(angle) + bias) / MathHelper.PiOver4);
        }

        /// <summary>What sort of movement is being done?</summary>
        private enum Movement
        {
            /// <summary>Combatant is turning around Y axis</summary>
            Rotation,

            /// <summary>Combatant is moving to new location</summary>
            Translation,
        }

        #region Fields

        /// <summary>the cells on the route the combatant is to follow</summary>
        private IList<MoveData> path;

        /// <summary>which cell on the path is combatant at?</summary>
        private int cellIndex;

        /// <summary>The unused time from previous update</summary>
        private double unusedSeconds;

        /// <summary>The unused time from previous updates</summary>
        private const double secondsPerCell = 1.0f / 3.0f;

        /// <summary>Is combatant moving or turning?</summary>
        private Movement movement;

        /// <summary>How fast combatant can turn, in radians/sec</summary>
        private const double turningSpeed = Math.PI * 2;

        /// <summary>How may TUs it costs a combatant to turn 45 degrees</summary>
        private const int turningCost = 1;

        /// <summary>How fast combatant can move, cells/second</summary>
        private const double movingSpeed = 4;

        /// <summary>How may TUs it costs a combatant to move between two east/west adjacent cells</summary>
        private const int movingCost = 8;

        /// <summary>Have we debited combatant for TU cost of move we're making?</summary>
        private bool movePaidFor;

        /// <summary>Is this move just a turn in place?</summary>
        private bool turnOnly;

        #endregion
    }
}
