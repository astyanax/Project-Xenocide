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
* @file EquipSoldierScreenController.cs
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
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.UI.Scenes;
#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>This is the screen that lets player set the items a soldier carries</summary>
    public partial class EquipSoldierScreen
    {
        /// <summary>
        /// Specifies behaviour of screen, based on mode screen is running in.  Modes:
        /// 1. Equip soldier in outpost
        /// 2. X-Corp soldier on battlescape
        /// 3. Alien soldier being probed on battlescape
        /// 4. Alien soldier being mind controlled on battlescape
        /// </summary>
        private abstract class Controller
        {
            /// <summary>Constructor</summary>
            /// <param name="equipSoldierScreen">The actual screen</param>
            protected Controller(EquipSoldierScreen equipSoldierScreen)
            {
                this.equipSoldierScreen = equipSoldierScreen;
            }

            #region Create the CeGui widgets

            /// <summary>add Widgets to the screen</summary>
            public abstract void CreateCeguiWidgets();

            #endregion Create the CeGui widgets

            #region event handlers

            /// <summary>React to user pressing the Close button</summary>
            public abstract void OnCloseButton();

            #endregion event handlers

            #region fields

            /// <summary>Combatant this screen is currently showing</summary>
            public abstract Combatant Combatant { get; }

            /// <summary>The actual screen</summary>
            public EquipSoldierScreen EquipSoldierScreen { get { return equipSoldierScreen; } }

            /// <summary>The actual screen</summary>
            private EquipSoldierScreen equipSoldierScreen;

            #endregion
        }
    }
}
