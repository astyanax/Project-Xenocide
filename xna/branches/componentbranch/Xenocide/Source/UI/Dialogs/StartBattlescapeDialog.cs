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
* @file TrackingLostDialog.cs
* @date Created: 2007/03/11
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using CeGui;
using Xenocide.Resources;
using Xenocide.UI.Screens;

using Xenocide.Model.Geoscape;
using Xenocide.Model.Geoscape.Craft;
using Microsoft.Xna.Framework;

#endregion

namespace Xenocide.UI.Dialogs
{
    class StartBattlescapeDialog : Dialog
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hunter">Craft that is landing troops</param>
        /// <param name="target">Craft that is on ground</param>
        public StartBattlescapeDialog(Game game, Craft hunter, Craft target)
            : base(game, new System.Drawing.SizeF(0.5f, 0.3f))
        {
            this.target = target;
            this.hunter = hunter;
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // static text to show the message
            textWindow = GuiBuilder.CreateText(CeguiId + "_text");
            AddWidget(textWindow, 0.02f, 0.15f, 0.96f, 0.55f);
            textWindow.Text = Strings.ResourceManager.GetString("MSGBOX_CRASH_RECOVERY");
            textWindow.HorizontalFormat = HorizontalTextFormat.WordWrapLeft;

            // buttons
            okButton = AddButton("BUTTON_OK", 0.10f, 0.85f, 0.80f, 0.10f);

            okButton.Clicked    += new CeGui.GuiEventHandler(OnOkClicked);
        }

        private CeGui.Widgets.StaticText  textWindow;
        private CeGui.Widgets.PushButton  okButton;

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>User clicked the "OK" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnOkClicked(object sender, CeGui.GuiEventArgs e)
        {
            DoBattlescape();
        }

        /// <summary>
        /// Simulate a battlescape battle having taken place
        /// </summary>
        private void DoBattlescape()
        {
            // Tell UFO that it has been killed
            target.OnDestroyed();

            // Tell hunter fight's over (it won)
            hunter.OnDogfightFinished();

            // return to geoscape
            ScreenManager.CloseDialog(this);
        }

        #endregion event handlers

        #region Fields

        /// <summary>
        /// Craft that is on ground
        /// </summary>
        private Craft target;

        /// <summary>
        /// Craft that is landing troops
        /// </summary>
        private Craft hunter;

        #endregion
    }
}
