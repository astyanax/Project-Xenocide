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
* @file AircraftOrdersDialog.cs
* @date Created: 2007/08/19
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using CeGui;

using ProjectXenocide.UI.Screens;

using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Dialogs
{
    /// <summary>
    /// Dialog that lets player change the orders given to an aircraft
    /// </summary>
    class AircraftOrdersDialog : Dialog
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="craft">Craft to give orders to</param>
        public AircraftOrdersDialog(Aircraft craft)
            : base("Content/Layouts/AircraftOrdersDialog.layout")
        {
            this.craft = craft;
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            CeGui.Widgets.StaticText txtDetails = (CeGui.Widgets.StaticText)WindowManager.Instance.GetWindow(txtDetailsName);
            txtDetails.Text = MakeDialogText();
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>user wants craft to return to base</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnReturnClicked(object sender, CeGui.GuiEventArgs e)
        {
            SetReturnToBaseMission();
        }

        /// <summary>user wants craft to go to new location</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnTargetClicked(object sender, CeGui.GuiEventArgs e)
        {
            NewTarget();
        }

        /// <summary>no changes are wanted</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnCancelClicked(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.CloseDialog(this);
        }

        /// <summary>
        /// Set the craft's mission to "return to home base"
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "FxCop false positive")]
        private void SetReturnToBaseMission()
        {
            craft.Mission.Abort();
            craft.Mission = new PatrolMission(craft, craft.HomeBase.Position);
            craft.Mission.SetState(new ReturnToBaseState(craft.Mission));
            ScreenManager.CloseDialog(this);
        }

        /// <summary>
        /// Let user pick a new Target for this Craft
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "FxCop false positive")]
        private void NewTarget()
        {
            GeoscapeScreen geoscapeScreen = new GeoscapeScreen();
            geoscapeScreen.State = new GeoscapeScreen.TargetingScreenState(geoscapeScreen, craft);
            ScreenManager.ScheduleScreen(geoscapeScreen);
            ScreenManager.CloseDialog(this);
        }

        #endregion event handlers

        /// <summary>
        /// Create text to show on dialog
        /// </summary>
        /// <returns>text to show</returns>
        private String MakeDialogText()
        {
            StringBuilder sb = new StringBuilder(craft.Name);
            sb.Append(Util.Linefeed);
            sb.Append(Util.StringFormat(Strings.MSGBOX_AIRCRAFT_ORDERS_BASE, craft.HomeBase.Name));
            sb.Append(Util.Linefeed);
            sb.Append(Util.StringFormat(Strings.MSGBOX_AIRCRAFT_ORDERS_SPEED, craft.MetersPerSecond));
            sb.Append(Util.Linefeed);
            sb.Append(Util.StringFormat(Strings.MSGBOX_AIRCRAFT_ORDERS_FUEL, craft.FuelPercent));
            sb.Append(Util.Linefeed);
            foreach (WeaponPod pod in craft.WeaponPods)
            {
                if (null != pod)
                {
                    sb.Append(pod.PodInformationString());
                    sb.Append(Util.Linefeed);
                }
            }
            return sb.ToString();
        }

        #region Fields

        /// <summary>
        /// Craft to give orders to
        /// </summary>
        private Aircraft craft;

        #endregion

        #region Constants

        private const string txtDetailsName = "txtDetails";

        #endregion
    }
}
