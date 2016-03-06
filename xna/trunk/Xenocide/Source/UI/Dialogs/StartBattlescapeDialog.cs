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
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using CeGui;


using ProjectXenocide.Utils;

using ProjectXenocide.UI.Screens;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Battlescape;

#endregion

namespace ProjectXenocide.UI.Dialogs
{
    class StartBattlescapeDialog : Dialog
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mission">Details for battlescape mission</param>
        public StartBattlescapeDialog(Mission mission)
            : base("Content/Layouts/StartBattlescapeDialog.layout")
        {
            this.mission = mission;
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            WindowManager.Instance.GetWindow(txtDetailsName).Text = mission.MakeStartMissionText();
            // only show this button if the cheat is set to true.
            WindowManager.Instance.GetWindow(btnAutoComplete).Visible = Xenocide.StaticTables.StartSettings.Cheats.AllowAutoWinBattlescape;
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>User clicked the "OK" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnOkClicked(object sender, CeGui.GuiEventArgs e)
        {
            DoBattlescape();
        }

        /// <summary>User clicked the "Cancel" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnCancelClicked(object sender, CeGui.GuiEventArgs e)
        {
            DoCancel();
        }

        /// <summary>User clicked the "Auto Complete" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnAutoCompleteClicked(object sender, CeGui.GuiEventArgs e)
        {
            Xenocide.GameState.Battlescape = new Battle(mission);
            mission.OnFinish(Xenocide.GameState.Battlescape, BattleFinish.XCorpVictory);
            Xenocide.GameState.Battlescape.PostMissionCleanup();
            Xenocide.GameState.Battlescape = null;
            ScreenManager.ScheduleScreen(new BattlescapeReportScreen(mission));
            ScreenManager.CloseDialog(this);
        }
        #endregion event handlers

        /// <summary>
        /// Simulate a battlescape battle having taken place
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "FxCop false positive")]
        private void DoBattlescape()
        {
            // Setup the battlescape, and go to it
            Xenocide.GameState.Battlescape = new Battle(mission);
            ScreenManager.ScheduleScreen(new BattlescapeScreen());
            ScreenManager.CloseDialog(this);
        }

        /// <summary>
        /// Handle user deciding not to start the battlescape
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "FxCop false positive")]
        private void DoCancel()
        {
            mission.DontStart();

            // return to geoscape
            ScreenManager.CloseDialog(this);
        }

        #region Fields

        /// <summary>
        /// Details for battlescape mission
        /// </summary>
        private Mission mission;

        #endregion

        #region Constants

        private const string txtDetailsName = "txtDetails";
        private const string btnAutoComplete = "btnAutoComplete";

        #endregion
    }
}
