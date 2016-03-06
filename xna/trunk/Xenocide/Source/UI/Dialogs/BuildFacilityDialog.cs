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
* @file BuildFacilityDialog.cs
* @date Created: 2007/04/10
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using CeGui;
using ProjectXenocide.UI.Screens;


using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Facilities;
using ProjectXenocide.Utils;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Dialogs
{
    /// <summary>
    /// Dialog where user selects the type of facility to be built
    /// </summary>
    class BuildFacilityDialog : Dialog
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="basesScreen">The bases screen we need to send the user's selection to</param>
        public BuildFacilityDialog(BasesScreen basesScreen)
            : base("Content/Layouts/BuildFacilityDialog.layout")
        {
            this.basesScreen = basesScreen;
        }
        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            grid = (CeGui.Widgets.MultiColumnList)WindowManager.Instance.GetWindow(gridFacilitiesName);
            grid.AddColumn(Strings.DLG_BUILDFACILITY_COLUMN_FACILITY, grid.ColumnCount, 0.49f);
            grid.AddColumn(Strings.DLG_BUILDFACILITY_COLUMN_COST, grid.ColumnCount, 0.15f);
            grid.AddColumn(Strings.DLG_BUILDFACILITY_COLUMN_BUILD_TIME, grid.ColumnCount, 0.15f);
            grid.AddColumn(Strings.DLG_BUILDFACILITY_COLUMN_MAINTENANCE, grid.ColumnCount, 0.20f);

            AddFacilitiesToGrid();

            // facilitiesGrid.SelectionChanged += new WindowEventHandler(OnGridSelectionChanged);
        }

        private CeGui.Widgets.MultiColumnList grid;

        /// <summary>
        /// Add the available facilities to the grid
        /// </summary>
        private void AddFacilitiesToGrid()
        {
            int index = 0;
            foreach (FacilityInfo facility in Xenocide.StaticTables.FacilityList)
            {
                // skip Facilities player isn't allowed
                if (CanBuildFacility(facility.Id))
                {
                    // add facility to grid
                    CeGui.Widgets.ListboxItem listboxItem = Util.CreateListboxItem(facility.Name);
                    listboxItem.ID = index;
                    int rowNum = grid.AddRow(listboxItem, 0);
                    Util.AddNumericElementToGrid(grid, 1, rowNum, facility.BuildCost);
                    grid.SetGridItem(2, rowNum,
                        Util.CreateListboxItem(
                            Util.StringFormat(Strings.DLG_BUILDFACILITY_BUILD_TIME_FORMAT, facility.BuildDays)
                    ));
                    Util.AddNumericElementToGrid(grid, 3, rowNum, facility.MonthlyMaintenance);
                }
                ++index;
            }
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>user has selected a facility</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnOkClicked(object sender, CeGui.GuiEventArgs e)
        {
            // Get the facility the user selected
            CeGui.Widgets.ListboxItem item = grid.GetFirstSelectedItem();
            if (null == item)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_FACILITY_SELECTED);
            }
            else
            {
                FacilityInfo info = Xenocide.StaticTables.FacilityList[item.ID];

                // Check that user has sufficient funds to build this facility
                // if funds are insufficient, warning dialog will automatically be given
                if (Xenocide.GameState.GeoData.XCorp.Bank.CanAfford(info.BuildCost))
                {
                    if (info.LimitIsOnePerOutpost && (null != basesScreen.SelectedBaseFloorplan.FindUniqueFacility(info.Id)))
                    {
                        // this is a "only one per base" facility, and there's already one in the base
                        Util.ShowMessageBox(Strings.MSGBOX_ONLY_ONE_FACILITY_PER_BASE, info.Name);
                    }
                    else
                    {
                        // Now set up the BasesScreen to be adding this facility
                        basesScreen.BuildFacility(new FacilityHandle(item.ID));
                        ScreenManager.CloseDialog(this);
                    }
                }
            }
        }

        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnGridMouseDoubleClicked(object sender, MouseEventArgs e)
        {
            OnOkClicked(sender, new GuiEventArgs());
        }

        /// <summary>user wants to cancel adding a facility to base</summary>
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
        /// Check if player is allowed to build this kind of facility
        /// </summary>
        /// <param name="facilityId">type of facility player wants to build</param>
        /// <returns>true if player is allowed to build</returns>
        private static bool CanBuildFacility(string facilityId)
        {
            // bases are limited to one access facility,
            // and player can't build facilities he doesn't have the tech for
            return (facilityId != "FAC_BASE_ACCESS_FACILITY") &&
                Xenocide.GameState.GeoData.XCorp.TechManager.IsAvailable(facilityId);
        }

        #region Fields

        /// <summary>
        /// The bases screen we send the user's selection to
        /// </summary>
        private BasesScreen basesScreen;

        #endregion

        #region Constants

        private const string gridFacilitiesName = "gridFacilities";

        #endregion
    }
}
