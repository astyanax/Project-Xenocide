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
* @file BattlescapeScreenStates.cs
* @date Created: 2008/02/03
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


using ProjectXenocide.Utils;
using ProjectXenocide.Model;

using ProjectXenocide.UI.Dialogs;

using System.Threading;
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.UI.Scenes.Battlescape;

#endregion

namespace ProjectXenocide.UI.Screens
{
/*
  This file holds the Battlescape's nested ScreenState classes
*/

    public partial class BattlescapeScreen
    {
        /// <summary>
        /// Control behaviour, based on state screen is in.
        /// </summary>
        private abstract class ScreenState
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="battlescapeScreen">The parent battlescape</param>
            protected ScreenState(BattlescapeScreen battlescapeScreen)
            {
                this.battlescapeScreen = battlescapeScreen;
            }

            /// <summary>Update any model data</summary>
            /// <param name="gameTime">Time since last update() call</param>
            public virtual void Update(GameTime gameTime){ /* default behaviour is do nothing */ }

            /// <summary>React to user clicking left mouse button in the 3D battlescape scene</summary>
            /// <param name="pos">Cell in battlescape where mouse was clicked</param>
            public virtual void OnLeftMouseDownInScene(Vector3 pos) { /* default behaviour is do nothing */ }

            /// <summary>React to user clicking right mouse button in the 3D battlescape scene</summary>
            /// <param name="pos">Cell in battlescape where mouse was clicked</param>
            public virtual void OnRightMouseDownInScene(Vector3 pos) { /* default behaviour is do nothing */ }

            /// <summary>React to user moving cursor to new cell of battlescape</summary>
            /// <param name="pos">Cell in battlescape mouse was moved to</param>
            public virtual void OnMouseMoveInScene(Vector3 pos) { /* default behaviour is do nothing */ }

            /// <summary>Called when Screen enters a state</summary>
            public virtual void OnEnterState() { /* default behaviour is do nothing */ }

            /// <summary>Called when Screen exits a state</summary>
            public virtual void OnExitState() { /* default behaviour is do nothing */ }

            #region buttons being clicked

            /// <summary>User has clicked the Equipment button</summary>
            public virtual void OnEquipmentButton() { /* default behaviour is do nothing */ }

            /// <summary>User has clicked the either Right or Left hand button</summary>
            /// <param name="right">true if it was the "right hand" button</param>
            public virtual void OnHandButton(bool right) { /* default behaviour is do nothing */ }

            /// <summary>User has clicked the "Finish Turn" button</summary>
            public virtual void OnFinishTurnButton() { /* default behaviour is do nothing */ }

            /// <summary>User has clicked the "Abort Mission" button</summary>
            public virtual void OnAbortButton() { /* default behaviour is do nothing */ }

            #endregion buttons being clicked

            #region Fields

            /// <summary>The parent BattlescapeScreen</summary>
            public BattlescapeScreen BattlescapeScreen { get { return battlescapeScreen; } }

            /// <summary>The Battlescape</summary>
            public Battle Battlescape { get { return battlescapeScreen.battlescape; } }

            /// <summary>The parent BattlescapeScreen</summary>
            private BattlescapeScreen battlescapeScreen;

            #endregion Fields
        }

        /// <summary>
        /// Screen behaviour, when battlescape is starting a turn
        /// </summary>
        private class StartTurnScreenState : ScreenState
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="battlescapeScreen">The parent battlescapeScreen</param>
            public StartTurnScreenState(BattlescapeScreen battlescapeScreen) : base(battlescapeScreen) { }

            /// <summary>Update any model data</summary>
            /// <param name="gameTime">Time since last update() call</param>
            public override void Update(GameTime gameTime)
            {
                // ToDo: implement pumping state machine owned by Battlescape until it reports aliens 
                // and civilians have finished the actions they do before the X-Corp soldiers do anything

                // then let player start giving orders to the X-Corp soldiers 
                //... start with to first soldier in X-Corp list
                Combatant combatant = Battlescape.Teams[Team.XCorp].Combatants[0];
                BattlescapeScreen.ChangeState(new MoveOrderCombatantScreenState(BattlescapeScreen, combatant));
            }

            /// <summary>Called when Screen enters this state</summary>
            public override void OnEnterState()
            {
                Battlescape.OnStartTurn();
            }
        }

        /// <summary>
        /// Screen behaviour, when combatant is processing order
        /// </summary>
        private class CombatantActivityScreenState : ScreenState
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="battlescapeScreen">The parent battlescapeScreen</param>
            /// <param name="combatant">Combatant that is performing action</param>
            public CombatantActivityScreenState(BattlescapeScreen battlescapeScreen, Combatant combatant)
                : base(battlescapeScreen)
            {
                this.combatant = combatant;
            }

            /// <summary>Update any model data</summary>
            /// <param name="gameTime">Time since last update() call</param>
            public override void Update(GameTime gameTime)
            {
                // ToDo: this code is just a temp hack to get MoveAction working.
                // This logic needs to be moved into a state machine owned by the Battlescape.
                if (combatant.BattlescapeUpdate(gameTime.ElapsedRealTime.TotalMilliseconds / 1000.0f))
                {
                    // action finished, wait for another order
                    BattlescapeScreen.ChangeState(new MoveOrderCombatantScreenState(BattlescapeScreen, combatant));
                }
            }

            #region Fields

            /// <summary>Combatant player is giving order to</summary>
            private Combatant combatant;

            #endregion Fields
        }
    }
}
