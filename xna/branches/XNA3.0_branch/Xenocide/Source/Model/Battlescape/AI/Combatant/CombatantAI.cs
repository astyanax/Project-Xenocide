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
* @file TeamAI.cs
* @date Created: 2008/03/09
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

using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.Model.StaticData.Items;

#endregion

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// The AI logic for a single combatant on the battlescape
    /// </summary>
    [Serializable]
    public class CombatantAI
    {
        /// <summary>Ctor</summary>
        /// <param name="teamAI">AI of the team this AI belongs to</param>
        /// <param name="combatant">Combatant controlled by this AI</param>
        public CombatantAI(TeamAI teamAI, Combatant combatant)
        {
            this.teamAI    = teamAI;
            this.combatant = combatant;
        }

        /// <summary>Update AI based on passage of time</summary>
        /// <param name="seconds">length of time that has passed</param>
        /// <returns>false if AI has nothing more to do this turn</returns>
        public bool Update(double seconds)
        {
            // if combatant is procesing an order, continue processing it
            if (null != combatant.Order)
            {
                combatant.BattlescapeUpdate(seconds);
                return true;
            }

            // if there's a target, either shoot it, or move closer
            if (PickTarget() && (TryAttackTarget() || TryMoveTowardsTarget()))
            {
                return true;
            }

            // if get here, nothing more for combatant to do
            //... wipe target selection, will re-calc at start of next turn
            target = null;
            return false;
        }

        private bool PickTarget()
        {
            // if target is dead, target is no longer valid
            if (null != target)
            {
                if (target.IsDead)
                {
                    target = null;
                }
            }

            // if target is visible, it stays target
            if ((null != target) && (0 != (combatant.OpponentsInView & (1 << target.PlaceInTeam))))
            {
                return true;
            }

            // else if other living enemy is visible, visible enemy is target
            if (0 != combatant.OpponentsInView)
            {
                for (int i = 0; i < 32; ++i)
                {
                    if ((0 != (combatant.OpponentsInView & (1 << i))) && !teamAI.Enemy.Combatants[i].IsDead)
                    {
                        target = teamAI.Enemy.Combatants[i];
                        return true;
                    }
                }
            }

            // if still don't have a target pick closest living enemy as target
            if (null == target)
            {
                float mindist = float.MaxValue;
                foreach (Combatant enemy in teamAI.Enemy.Combatants)
                {
                    float distance = Vector3.DistanceSquared(combatant.Position, enemy.Position);
                    if ((distance < mindist) && !enemy.IsDead)
                    {
                        target = enemy;
                        mindist = distance;
                    }
                }
            }
            return (null != target);
        }

        /// <summary>Try and move this combatant towards the target</summary>
        /// <returns>true if successful</returns>
        private bool TryMoveTowardsTarget()
        {
            // Need to remove the target from the cell it's currently in
            // because we can't pathfind to a cell that's occupied.
            int  cellIndex = teamAI.Battlescape.Terrain.CellIndex(target.Position);
            Cell cellCopy  = teamAI.Battlescape.Terrain[cellIndex];
            cellCopy.CombatantId = 0;
            teamAI.Battlescape.Terrain[cellIndex] = cellCopy;

            // try and move one cell towards target
            bool canMove = false;
            if (combatant.FindPath(target.Position, path))
            {
                // if path is 2 units, then we're beside target and just need to turn.
                if (2 == path.Count)
                {
                    // figure how far to turn, if already facing the right quadrant, don't do anything.
                    double turn = combatant.CalcTurnAngle(path[1].Vector3);
                    if ((MathHelper.PiOver4 - 0.1f) <= (float)Math.Abs(turn))
                    {
                        // only move if combatant has sufficient TUs to move
                        MoveOrder order = new MoveOrder(combatant, teamAI.Battlescape, combatant.Heading + turn);
                        if (MoveOrder.CalcTurnCost(turn) <= combatant.Stats[Statistic.TimeUnitsLeft])
                        {
                            combatant.Order = order;
                            canMove = true;
                        }
                    }
                }
                else
                {
                    // only move if combatant has sufficient TUs to move
                    List<MoveData> step = new List<MoveData>();
                    step.Add(path[0]);
                    step.Add(path[1]);
                    MoveOrder order = new MoveOrder(combatant, teamAI.Battlescape, step);
                    if (1 == order.MaximumPathCells())
                    {
                        combatant.Order = order;
                        canMove = true;
                    }
                }
            }

            // put target back on battlescape
            cellCopy.CombatantId = target.CombatantId;
            teamAI.Battlescape.Terrain[cellIndex] = cellCopy;

            return canMove;
        }

        /// <summary>Try and attack the target</summary>
        /// <returns>true if able to order an attack</returns>
        private bool TryAttackTarget()
        {
            Debug.Assert(null != target);
            Item item = combatant.Inventory.ItemAt(0, 0);
            if (null != item)
            {
                ActionInfo action = GetAttackAction(item);
                if ((null != action) &&
                    (ActionError.None == action.IsLocationOk(teamAI.Battlescape, item, combatant, target.Position)))
                {
                    combatant.Order = action.Start(teamAI.Battlescape, item, combatant, target.Position);
                    return true;
                }
            }
            // if get here, attack not possible
            return false;
        }

        /// <summary>Get first attack we find that can be performed by item combatant is currently holding</summary>
        /// <param name="item">Item to check and see if it has an attack mode</param>
        /// <returns>the attack, or null if none possible</returns>
        private ActionInfo GetAttackAction(Item item)
        {
            foreach (ActionInfo action in item.ItemInfo.Actions)
            {
                // ignore actions we don't have time for
                if (ActionError.None == action.ActionAvailable(teamAI.Battlescape, item, combatant))
                {
                    // ToDo: add "isAttack" property to action
                    if (action is ShootActionInfo)
                    {
                        return action;
                    }
                }
            }
            // if get here, no action available
            return null;
        }

        #region Fields

        /// <summary>AI of the team this AI belongs to</summary>
        private TeamAI teamAI;

        /// <summary>Combatant controlled by this AI</summary>
        private Combatant combatant;

        /// <summary>Combatant this AI is currently trying to kill</summary>
        private Combatant target;

        /// <summary>Where combatant is going</summary>
        private List<MoveData> path = new List<MoveData>();

        #endregion
    }
}
