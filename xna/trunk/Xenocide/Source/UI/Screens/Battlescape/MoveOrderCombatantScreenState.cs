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
* @file MoveOrderCombatantScreenState.cs
* @date Created: 2008/02/16
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


using ProjectXenocide.Utils;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.UI.Scenes.Battlescape;

#endregion

namespace ProjectXenocide.UI.Screens
{
    /*
      This file is the screen state, when battlescape is waiting for waiting for player to give or 
     finish giving a combatant an order
    */

    public partial class BattlescapeScreen
    {
        /// <summary>
        /// Screen behaviour, when player giving a combatant an order to move 
        /// </summary>
        private class MoveOrderCombatantScreenState : OrderCombatantScreenState
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="battlescapeScreen">The parent battlescapeScreen</param>
            /// <param name="combatant">Combatant player is giving order to</param>
            public MoveOrderCombatantScreenState(BattlescapeScreen battlescapeScreen, Combatant combatant)
                : base(battlescapeScreen, combatant)
            {
            }

            /// <summary>Called when Screen enters this state</summary>
            public override void OnEnterState()
            {
                base.OnEnterState();
                BattlescapeScreen.scene.WantedCellCursorColor = Color.Blue;
            }

            /// <summary>React to user moving cursor to new cell of battlescape</summary>
            /// <param name="pos">Cell in battlescape mouse was moved to</param>
            public override void OnMouseMoveInScene(Vector3 pos)
            {
                BattlescapeScreen.scene.ShowPath = false;

                // if combatant is dead or stunned, nothing to do
                if (!Combatant.CanTakeOrders)
                {
                    return;
                }

                // draw path, showing how far combatant can move
                if (Battlescape.Terrain.IsOnTerrain(pos))
                {
                    List<MoveData> path   = new List<MoveData>();
                    if (Combatant.FindPath(pos, path))
                    {
                        // there's a path to destination, see how far along it we can go
                        int max = (new MoveOrder(Combatant, Battlescape, path)).MaximumPathCells();
                        if (0 < max)
                        {
                            // remove cells beyond move range
                            if (max < path.Count - 1)
                            {
                                path.RemoveRange(max + 1, path.Count - 1 - max);
                            }
                            BattlescapeScreen.scene.PathMeshBuilder.Path = path;
                            BattlescapeScreen.scene.ShowPath = true;
                        }
                    }
                }
            }

            /// <summary>React to user clicking left mouse button in the 3D battlescape scene</summary>
            /// <param name="pos">Cell in battlescape where mouse was clicked</param>
            public override void OnLeftMouseDownInScene(Vector3 pos)
            {
                // figure out what we do with target cell,
                // is there a combatant in the cell?
                Combatant target = Battlescape.FindCombatantAt(pos);
                if (null != target)
                {
                    switch (target.TeamId)
                    {
                        case Team.Aliens:
                            // if cheat on, give order to alien, otherwise ignore.
                            if (Xenocide.StaticTables.StartSettings.Cheats.PlayerControlsAliens)
                            {
                                BattlescapeScreen.ChangeState(new MoveOrderCombatantScreenState(BattlescapeScreen, target));
                            }
                            return;

                        case Team.XCorp:
                            // selected a (probably different) X-Corp unit
                            BattlescapeScreen.ChangeState(new MoveOrderCombatantScreenState(BattlescapeScreen, target));
                            return;

                        case Team.Civilians:
                            // ignore civilians
                            return;

                        default:
                            // should never get here
                            Debug.Assert(false);
                            break;
                    }
                }

                // if combatant is dead or stunned, nothing to do
                if (!Combatant.CanTakeOrders)
                {
                    return;
                }

                // Assume it's a move command
                pos = BattlescapeScene.RoundToCell(pos);
                if (Battlescape.Terrain.IsOnTerrain(pos))
                {
                    // check there's a path to the new position
                    List<MoveData> path = new List<MoveData>();
                    if (Combatant.FindPath(pos, path))
                    {
                        Combatant.Order = new MoveOrder(Combatant, Battlescape, path);
                        BattlescapeScreen.ChangeState(new CombatantActivityScreenState(BattlescapeScreen, Combatant));
                    }
                }
            }

            /// <summary>React to user clicking right mouse button in the 3D battlescape scene</summary>
            /// <param name="pos">Cell in battlescape where mouse was clicked</param>
            /// <remarks>Right click is a turn command</remarks>
            public override void OnRightMouseDownInScene(Vector3 pos)
            {
                // if combatant is dead or stunned, nothing to do
                if (!Combatant.CanTakeOrders)
                {
                    return;
                }

                // figure how far to turn
                double turn = Combatant.CalcTurnAngle(pos);

                // if we're already facing the right quadrant, don't do anything.
                if ((MathHelper.PiOver4 - 0.1f) <= (float)Math.Abs(turn))
                {
                    Combatant.Order = new MoveOrder(Combatant, Battlescape, Combatant.Heading + turn);
                    BattlescapeScreen.ChangeState(new CombatantActivityScreenState(BattlescapeScreen, Combatant));
                }
            }

            #region buttons being clicked

            /// <summary>User has clicked the equipment button.  So bring up equipment screen for selected combatant</summary>
            public override void OnEquipmentButton()
            {
                // if combatant is dead or stunned, nothing to do
                if (Combatant.CanTakeOrders)
                {
                    BattlescapeScreen.ScreenManager.PushScreen(new EquipSoldierScreen(Combatant, Battlescape, false));
                }
            }

            /// <summary>User has clicked the either Right or Left hand button</summary>
            /// <param name="right">true if it was the "right hand" button</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)",
                Justification="ToDo: String is temporary, to cover case while code under construction, will be removed when problem fixed.")]
            public override void OnHandButton(bool right)
            {
                // if combatant is dead or stunned, nothing to do
                if (!Combatant.CanTakeOrders)
                {
                    return;
                }

                // get item in hand
                Item weapon = Combatant.Inventory.ItemAt(right ? 0 : 1, 0);
                if (null != weapon)
                {
                    // ToDo: Item can't be used if not researched. Note, some actions (e.g. throw) can be done without needing research

                    // ToDo: remove this safty check when more actions are implemented
                    if (0 == weapon.ItemInfo.Actions.Count)
                    {
                        Util.ShowMessageBox(String.Format("Sorry, {0} doesn't have any actions yet.", weapon.Name));
                        return;
                    }

                    BattlescapeScreen.ScreenManager.ShowDialog(
                        new PickActionDialog(BattlescapeScreen, weapon, Combatant, right));
                }
            }

            #endregion buttons being clicked

            #region Fields
            #endregion Fields
        }
    }
}
