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
* @file EquipSoldierScreenBattlescapeController.cs
* @date Created: 2008/03/02
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


using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.UI.Scenes;
using ProjectXenocide.Model.Battlescape;
#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>This is the screen that lets player set the items a soldier carries</summary>
    public partial class EquipSoldierScreen
    {
        /// <summary>
        /// Specifies behaviour of screen, for combatant on a battlescape.  Modes:
        /// 1. X-Corp soldier on battlescape
        /// 2. Alien soldier being probed on battlescape
        /// 3. Alien soldier being mind controlled on battlescape
        /// </summary>
        private class BattlescapeController : Controller
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="equipSoldierScreen">The actual screen</param>
            /// <param name="battlescape">The battlescape the combatant is on</param>
            /// <param name="combatant">Combatant this screen will show</param>
            public BattlescapeController(EquipSoldierScreen equipSoldierScreen, Battle battlescape, Combatant combatant)
                : base(equipSoldierScreen)
            {
                this.combatant   = combatant;
                this.battlescape = battlescape;
            }

            #region Create the CeGui widgets

            /// <summary>add Widgets to the screen</summary>
            public override void CreateCeguiWidgets()
            {
            }

            #endregion Create the CeGui widgets

            #region event handlers

            /// <summary>React to user pressing the Close button</summary>
            public override void OnCloseButton()
            {
                ReturnToParentScreen();
            }

            #endregion event handlers

            #region methods

            /// <summary>
            /// Close this screen and go back to the Screen that created this one 
            /// (either SolidersList or Battlescape)
            /// </summary>
            private void ReturnToParentScreen()
            {
                EquipSoldierScreen.ReleaseMovingItem();
                EquipSoldierScreen.ScreenManager.PopScreen();
            }

            #endregion methods

            #region fields

            /// <summary>Combatant this screen is currently showing</summary>
            public override Combatant Combatant { get { return combatant; } }

            /// <summary>Combatant this screen is showing</summary>
            private Combatant combatant;

            /// <summary>The battlescape the combatant is on</summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields",
                Justification="ToDo: will use later, when finish implementing")]
            private Battle battlescape;

            #endregion
        }
    }
}
