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
* @file OrderCombatantScreenState.cs
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
      This file is the screen state, when battlescape is waiting for waiting for player to give or 
     finish giving a combatant an order
    */

    public partial class BattlescapeScreen
    {
        /// <summary>
        /// Screen behaviour, when waiting for player to give/finish giving a combatant an order
        /// </summary>
        private abstract partial class OrderCombatantScreenState : ScreenState
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="battlescapeScreen">The parent battlescapeScreen</param>
            /// <param name="combatant">Combatant player is giving order to</param>
            public OrderCombatantScreenState(BattlescapeScreen battlescapeScreen, Combatant combatant)
                : base(battlescapeScreen)
            {
                this.combatant = combatant;
            }

            /// <summary>Called when Screen enters this state</summary>
            public override void OnEnterState()
            {
                BattlescapeScreen.ShowCombatantStats(combatant);
            }

            /// <summary>Called when Screen exits this state</summary>
            public override void OnExitState()
            {
                BattlescapeScreen.scene.ShowPath = false;
                BattlescapeScreen.ShowCombatantStats(null);
            }

            #region buttons being clicked

            /// <summary>User has clicked the "Finish Turn" button</summary>
            public override void OnFinishTurnButton()
            {
                BattlescapeScreen.ChangeState(new AlienTurnScreenState(BattlescapeScreen));
            }

            /// <summary>User has clicked the "Abort Mission" button</summary>
            public override void OnAbortButton()
            {
                BattlescapeScreen.FinishMission(BattleFinish.Aborted);
            }

            #endregion buttons being clicked

            #region Fields

            /// <summary>Combatant player is giving order to</summary>
            public Combatant Combatant { get { return combatant; } }

            /// <summary>Combatant player is giving order to</summary>
            private Combatant combatant;

            #endregion Fields
        }
    }
}
