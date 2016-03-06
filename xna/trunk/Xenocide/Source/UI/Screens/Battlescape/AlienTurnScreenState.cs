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
* @file AlienTurnScreenState.cs
* @date Created: 2008/03/16
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
  This file holds Battlescape's nested ScreenState classes
*/

    public partial class BattlescapeScreen
    {
        /// <summary>Screen behaviour, when Alien AI is having it's moves after X-Corp's turn</summary>
        private class AlienTurnScreenState : ScreenState
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="battlescapeScreen">The parent battlescapeScreen</param>
            public AlienTurnScreenState(BattlescapeScreen battlescapeScreen) : base(battlescapeScreen) { }

            /// <summary>Called when Screen enters a state</summary>
            public override void OnEnterState()
            {
                Battlescape.AlienAI.OnStartTeamTurn();
            }

            /// <summary>Update any model data</summary>
            /// <param name="gameTime">Time since last update() call</param>
            public override void Update(GameTime gameTime)
            {
                if (!Battlescape.AlienAI.Update(gameTime.ElapsedRealTime.TotalMilliseconds / 1000.0f))
                {
                    // End of turn processing.
                    // e.g. bleeding from wounds, recovering from stun, etc.
                    Battlescape.OnEndOfTurn();

                    CheckForEndOfBattle();
                }
            }

            /// <summary>check for end of battle conditions, and if so, exit battlescape</summary>
            private void CheckForEndOfBattle()
            {
                if (Battlescape.Teams[Team.Aliens].IsDefeated())
                {
                    BattlescapeScreen.FinishMission(BattleFinish.XCorpVictory);
                }
                else if (Battlescape.Teams[Team.XCorp].IsDefeated())
                {
                    BattlescapeScreen.FinishMission(BattleFinish.AlienVictory);
                }
                else
                {
                    // start next turn
                    BattlescapeScreen.ChangeState(new StartTurnScreenState(BattlescapeScreen));
                }
            }
        }
    }
}
