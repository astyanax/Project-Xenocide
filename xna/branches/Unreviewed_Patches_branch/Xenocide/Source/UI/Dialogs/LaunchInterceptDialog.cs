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
* @file LaunchInterceptDialog.cs
* @date Created: 2007/07/08
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using CeGui;
using CeGui.Renderers.Xna;


using ProjectXenocide.UI.Screens;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Geoscape.Outposts;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Dialogs
{
    class LaunchInterceptDialog : Dialog
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LaunchInterceptDialog()
            : base("Content/Layouts/LaunchInterceptDialog.layout")
        {
        }

        #region Create the CeGui widgets

        /// <summary>
        /// Create the widgets that are on the dialog
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            grid = (CeGui.Widgets.MultiColumnList)WindowManager.Instance.GetWindow(gridAircraftName);
            grid.AddColumn(Strings.DLG_LAUNCH_INTERCEPT_COLUMN_NAME, grid.ColumnCount, 0.15f);
            grid.AddColumn(Strings.DLG_LAUNCH_INTERCEPT_COLUMN_BASE, grid.ColumnCount, 0.15f);
            grid.AddColumn(Strings.DLG_LAUNCH_INTERCEPT_COLUMN_FUEL, grid.ColumnCount, 0.10f);
            grid.AddColumn(Strings.DLG_LAUNCH_INTERCEPT_COLUMN_HULL, grid.ColumnCount, 0.10f);
            grid.AddColumn(Strings.DLG_LAUNCH_INTERCEPT_COLUMN_PODS, grid.ColumnCount, 0.10f);
            grid.AddColumn(Strings.DLG_LAUNCH_INTERCEPT_COLUMN_AMMO, grid.ColumnCount, 0.10f);
            grid.AddColumn(Strings.DLG_LAUNCH_INTERCEPT_COLUMN_CREW, grid.ColumnCount, 0.10f);
            grid.AddColumn(Strings.DLG_LAUNCH_INTERCEPT_COLUMN_HWP, grid.ColumnCount, 0.10f);

            PopulateGrid();
        }

        private CeGui.Widgets.MultiColumnList grid;

        /// <summary>
        /// Put the list of craft into the grid
        /// </summary>
        private void PopulateGrid()
        {
            foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
            {
                foreach (Craft craft in outpost.Fleet)
                {
                    AddRowToGrid((Aircraft)craft);
                }
            }
        }

        /// <summary>
        /// Add a row (with details of a craft) to the grid
        /// </summary>
        /// <param name="aircraft">aircraft to put in the row</param>
        private void AddRowToGrid(Aircraft aircraft)
        {
            // add craft to grid
            CeGui.ListboxTextItem listboxItem = Util.CreateListboxItem(aircraft.Name);
            int rowNum = grid.AddRow(listboxItem, 0);
            listboxItem.ID = rowNum;

            Util.AddStringElementToGrid(grid, 1, rowNum, aircraft.HomeBase.Name);
            Util.AddNumericElementToGrid(grid, 2, rowNum, aircraft.FuelPercent);
            Util.AddNumericElementToGrid(grid, 3, rowNum, aircraft.HullPercent);
            Util.AddNumericElementToGrid(grid, 4, rowNum, aircraft.PodCountStatus);
            Util.AddNumericElementToGrid(grid, 5, rowNum, aircraft.AmmoStatus);
            Util.AddNumericElementToGrid(grid, 6, rowNum, aircraft.SoldierCountStatus);
            Util.AddNumericElementToGrid(grid, 7, rowNum, aircraft.XcapCountStatus);

            // record craft associated with this line
            rowToCraft[rowNum] = aircraft;
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>
        /// User has double clicked on the grid, assume wants to give orders to selected craft
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnGridMouseDoubleClicked(object sender, MouseEventArgs e)
        {
            CeGui.Widgets.ListboxItem selectedItem = grid.GetFirstSelectedItem();

            //ignore random doubleclicks
            if (null != selectedItem)
            {
                BringUpGeoscapeInTargetingMode(selectedItem);
            }

        }

        /// <summary>
        /// User has double clicked on OK, assume wants to give orders to selected craft
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnOkClicked(object sender, CeGui.GuiEventArgs e)
        {
            CeGui.Widgets.ListboxItem selectedItem = grid.GetFirstSelectedItem();
            if (null == selectedItem)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_INTERCEPT_CRAFT_SELECTED);
            }
            else
            {
                BringUpGeoscapeInTargetingMode(selectedItem);
            }
        }

        /// <summary>
        /// Go to geoscape, to select target for craft
        /// </summary>
        /// <param name="selectedItem">the craft user has selected</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "FxCop false positive")]
        private void BringUpGeoscapeInTargetingMode(CeGui.Widgets.ListboxItem selectedItem)
        {
            Xenocide.AudioSystem.PlaySound("Menu\\buttonclick2_changesetting.ogg");
            // Bring up Geoscape, in Targeting mode
            Aircraft aircraft = rowToCraft[selectedItem.ID];
            GeoscapeScreen geoscapeScreen = new GeoscapeScreen();
            geoscapeScreen.State = new GeoscapeScreen.TargetingScreenState(geoscapeScreen, aircraft);
            ScreenManager.ScheduleScreen(geoscapeScreen);
            ScreenManager.CloseDialog(this);
        }

        /// <summary>
        /// User has decided not to launch a craft
        /// </summary>
        /// <param name="sender">not used</param>
        /// <param name="e">not used</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnCancelClicked(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.CloseDialog(this);
        }

        #endregion event handlers

        #region Fields

        /// <summary>
        /// Map row in grid to craft
        /// </summary>
        private Dictionary<int, Aircraft> rowToCraft = new Dictionary<int, Aircraft>();

        #endregion Fields

        #region Constants

        private const string gridAircraftName = "gridAircraft";

        #endregion
    }
}
