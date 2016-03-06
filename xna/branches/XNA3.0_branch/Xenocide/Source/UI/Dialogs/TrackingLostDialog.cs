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

using ProjectXenocide.UI.Screens;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Utils;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Dialogs
{
    /// <summary>
    /// Dialog that asks player what to do when an aircraft looses sight of the UFO its tracking
    /// </summary>
    class TrackingLostDialog : Dialog
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hunter">Last known position of craft (UFO) that was being tracked</param>
        /// <param name="target">Craft that was tracking the UFO</param>
        public TrackingLostDialog(GeoPosition target, Craft hunter)
            : base("Content/Layouts/TrackingLostDialog.layout")
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
            WindowManager.Instance.GetWindow(txtDetailsName).Text = 
                Util.StringFormat(Strings.DLG_TRACKINGLOST_LOST_TRACKING, hunter.Name);
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
            // nothing else to do, craft has already been set to return to base
            ScreenManager.CloseDialog(this);
        }

        /// <summary>user wants craft to patrol current position</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnPatrolClicked(object sender, CeGui.GuiEventArgs e)
        {
            SetPatrol(hunter.Position);
        }

        /// <summary>user wants craft to patrol UFO's last known position</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnLastKnownClicked(object sender, CeGui.GuiEventArgs e)
        {
            SetPatrol(target);
        }

        /// <summary>
        /// Set the craft's mission to patrol
        /// </summary>
        /// <param name="position">position the craft is to patrol</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "FxCop false positive")]
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

        #region Constants

        private const string txtDetailsName = "txtDetails";

        #endregion
    }
}
