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
* @file LocationOrderCombatantScreenState.cs
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


using ProjectXenocide.Utils;
using ProjectXenocide.Model;

using ProjectXenocide.UI.Dialogs;

using System.Threading;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Battlescape.Combatants;
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
        /// Screen behaviour, waiting for player to give location needed by an order
        /// </summary>
        private class LocationOrderCombatantScreenState : OrderCombatantScreenState
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="battlescapeScreen">The parent battlescapeScreen</param>
            /// <param name="item">item doing the action</param>
            /// <param name="combatant">Combatant player is giving order to</param>
            /// <param name="actionInfo">action to get location for</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                Justification="FxCop false positive")]
            public LocationOrderCombatantScreenState(BattlescapeScreen battlescapeScreen, Item item, Combatant combatant,
                ActionInfo actionInfo)
                : base(battlescapeScreen, combatant)
            {
                this.item       = item;
                this.actionInfo = actionInfo;
            }

            /// <summary>Called when Screen enters this state</summary>
            public override void OnEnterState()
            {
                base.OnEnterState();
                BattlescapeScreen.scene.WantedCellCursorColor = noTargetColor;
            }

            /// <summary>Update any model data</summary>
            /// <param name="gameTime">Time since last update() call</param>
            public override void Update(GameTime gameTime)
            {
                // if escape key pressed, return to move mode
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    BattlescapeScreen.ChangeState(new MoveOrderCombatantScreenState(BattlescapeScreen, Combatant));
                }
            }

            /// <summary>React to user moving cursor to new cell of battlescape</summary>
            /// <param name="pos">Cell in battlescape mouse was moved to</param>
            public override void OnMouseMoveInScene(Vector3 pos)
            {
                // If mouse has moved over a possible target, set aming reticle
                if (Battlescape.Terrain.IsOnTerrain(pos))
                {
                    // cleanup pos, so that it's in same co-ords as combatant
                    pos.X = (int)pos.X + 0.5f;
                    pos.Z = (int)pos.Z + 0.5f;

                    if (ActionError.None == actionInfo.IsLocationOk(Battlescape, item, Combatant, pos))
                    {
                        Combatant target = Battlescape.FindCombatantAt(pos);
                        Color cursorColor = ((null != target) && (Combatant.TeamId != target.TeamId)) ? Color.Red : noTargetColor;
                        BattlescapeScreen.scene.WantedCellCursorColor = cursorColor;
                    }
                    else
                    {
                        BattlescapeScreen.scene.WantedCellCursorColor = Color.White;
                    }
                }
            }

            /// <summary>React to user clicking left mouse button in the 3D battlescape scene</summary>
            /// <param name="pos">Cell in battlescape where mouse was clicked</param>
            public override void OnLeftMouseDownInScene(Vector3 pos)
            {
                if (ActionError.None == actionInfo.IsLocationOk(Battlescape, item, Combatant, pos))
                {
                    Combatant.Order = actionInfo.Start(Battlescape, item, Combatant, pos, Combatant.ActiveArm.Both);
                    BattlescapeScreen.ChangeState(new CombatantActivityScreenState(BattlescapeScreen, Combatant));
                }
            }

            #region buttons being clicked
            #endregion buttons being clicked

            #region Fields

            /// <summary>item doing the action</summary>
            private Item item;

            /// <summary>action to get location for</summary>
            private ActionInfo actionInfo;

            /// <summary>Color to draw Cell Cursor when there is no target under the cursor</summary>
            static readonly Color noTargetColor = Color.Gold;

            #endregion Fields
        }
    }
}
