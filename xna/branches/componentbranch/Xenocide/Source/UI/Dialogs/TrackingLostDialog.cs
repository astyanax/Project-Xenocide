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
using Xenocide.UI.Screens;

using Xenocide.Model.Geoscape;
using Xenocide.Model.Geoscape.Craft;
using Xenocide.Utils;
using Microsoft.Xna.Framework;

#endregion

namespace Xenocide.UI.Dialogs
{
    class TrackingLostDialog : Dialog
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hunter">Last known position of craft (UFO) that was being tracked</param>
        /// <param name="target">Craft that was tracking the UFO</param>
        public TrackingLostDialog(Game game, GeoPosition target, Craft hunter)
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
            AddWidget(textWindow, 0.02f, 0.073f, 0.96f, 0.45f);
            textWindow.Text = Util.StringFormat("Craft {0} lost tracking on UFO.", hunter.Name);

            // buttons
            returnButton    = AddButton("BUTTON_RETURN_TO_BASE", 0.10f, 0.55f, 0.80f, 0.10f);
            patrolButton    = AddButton("BUTTON_PATROL",         0.10f, 0.70f, 0.80f, 0.10f);
            lastKnownButton = AddButton("BUTTON_LAST_POSITION",  0.10f, 0.85f, 0.80f, 0.10f);

            returnButton.Clicked    += new CeGui.GuiEventHandler(OnReturnClicked);
            patrolButton.Clicked    += new CeGui.GuiEventHandler(OnPatrolClicked);
            lastKnownButton.Clicked += new CeGui.GuiEventHandler(OnLastKnownClicked);
        }

        private CeGui.Widgets.StaticText  textWindow;
        private CeGui.Widgets.PushButton  returnButton;
        private CeGui.Widgets.PushButton  patrolButton;
        private CeGui.Widgets.PushButton  lastKnownButton;

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>user wants craft to return to base</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnReturnClicked(object sender, CeGui.GuiEventArgs e)
        {
            // nothing else to do, craft has already been set to return to base
            ScreenManager.CloseDialog(this);
        }

        /// <summary>user wants craft to patrol current position</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnPatrolClicked(object sender, CeGui.GuiEventArgs e)
        {
            SetPatrol(hunter.Position);
        }

        /// <summary>user wants craft to patrol UFO's last known position</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnLastKnownClicked(object sender, CeGui.GuiEventArgs e)
        {
            SetPatrol(target);
        }

        /// <summary>
        /// Set the craft's mission to patrol
        /// </summary>
        /// <param name="position">position the craft is to patrol</param>
        private void SetPatrol(GeoPosition position)
        {
            hunter.Mission.Abort();
            hunter.Mission = new PatrolMission(hunter, position);
            ScreenManager.CloseDialog(this);
        }

        #endregion event handlers

        #region Fields

        /// <summary>
        /// Last known position of craft (UFO) that was being tracked
        /// </summary>
        private GeoPosition target;

        /// <summary>
        /// Craft that was tracking the UFO
        /// </summary>
        private Craft hunter;

        #endregion
    }
}
