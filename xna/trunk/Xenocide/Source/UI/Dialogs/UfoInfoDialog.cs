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
* @file UfoInfoDialog.cs
* @date Created: 2007/08/19
* @author File creator: David Teviotdale
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
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Geography;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Dialogs
{
    /// <summary>
    /// Dialog that shows information on UFO on the Geoscape
    /// </summary>
    class UfoInfoDialog : Dialog
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ufo">UFO to show information on</param>
        public UfoInfoDialog(Ufo ufo)
            : base("Content/Layouts/UfoInfoDialog.layout")
        {
            this.ufo = ufo;
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            WindowManager.Instance.GetWindow(txtDetailsName).Text = MakeDialogText();
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>close the dialog</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnCancelClicked(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.CloseDialog(this);
        }

        #endregion event handlers

        /// <summary>
        /// Create text to show on dialog
        /// </summary>
        /// <returns>text to show</returns>
        private String MakeDialogText()
        {
            StringBuilder sb = new StringBuilder(ufo.Name);
            sb.Append(Util.Linefeed);
            sb.Append(Util.StringFormat(Strings.MSGBOX_UFOINFO_SIZE, ufo.UfoItemInfo.UfoSize));
            sb.Append(Util.Linefeed);
            sb.Append(Util.StringFormat(Strings.MSGBOX_UFOINFO_SPEED, ufo.MetersPerSecond));

            // if we can decode the transmissions, add extra information
            if (UfoWithinDecodeTransmissionsRange())
            {
                sb.Append(Util.Linefeed);
                sb.Append(Util.StringFormat(Strings.MSGBOX_UFOINFO_CRAFT_TYPE, ufo.UfoItemInfo.Name));
                sb.Append(Util.Linefeed);
                sb.Append(Util.StringFormat(Strings.MSGBOX_UFOINFO_RACE, Races.DisplayString(ufo.Race)));
                sb.Append(Util.Linefeed);
                sb.Append(Util.StringFormat(Strings.MSGBOX_UFOINFO_MISSION, ufo.Task.Name));
                sb.Append(Util.Linefeed);

                // zone
                GeoPosition target = ufo.Task.Centroid;
                PlanetRegion region = Xenocide.GameState.GeoData.Planet.GetRegionAtLocation(target);
                sb.Append(Util.StringFormat(Strings.MSGBOX_UFOINFO_ZONE, region.Name));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Is the UFO in range of an X-Corp outpost that can decode the UFO's transmissions?
        /// </summary>
        /// <returns>true if in range</returns>
        private bool UfoWithinDecodeTransmissionsRange()
        {
            bool decoding = false;
            foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
            {
                decoding |= (outpost.IsOnRadar(ufo.Position, true) & outpost.Statistics.CanDecodeTransmissions());
            }
            return decoding;
        }

        #region Fields

        /// <summary>
        /// UFO to show information on
        /// </summary>
        private Ufo ufo;

        #endregion

        #region Constants

        private const string txtDetailsName = "txtDetails";

        #endregion
    }
}
