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
* @file BaseInfoScreen.cs
* @date Created: 2007/01/21
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
using Xenocide.Utils;
using Xenocide.Model.Geoscape.HumanBases;
using Microsoft.Xna.Framework;

#endregion

namespace Xenocide.UI.Screens
{
    class BaseInfoScreen : Screen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="selectedBaseIndex">Index to human base screen is to show</param>
        /// <param name="screenManager">the screen manager</param>
        public BaseInfoScreen(Game game, int selectedBaseIndex)
            : base(game, "BaseInfoScreen")
        {
            this.selectedBaseIndex = selectedBaseIndex;
        }

        public override void Initialize()
        {
            humanBaseService = (IHumanBaseService)Game.Services.GetService(typeof(IHumanBaseService));
            base.Initialize();
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // combo box to allow user to pick base to work on
            basesListComboBox = GuiBuilder.CreateComboBox("basesListComboBox");
            AddWidget(basesListComboBox, 0.7475f, 0.06f, 0.2275f, 0.40f);
            Misc.PopulateHumanBasesList(humanBaseService.HumanBases, basesListComboBox, selectedBaseIndex);
            basesListComboBox.ListSelectionAccepted += new WindowEventHandler(OnBaseSelectionChanged);

            // The girds detailing staff and facilities in base
            InitializeStaffGrid();
            InitializeFacilitiesGrid();
            
            // other buttons
            transfersButton = AddButton("BUTTON_TRANSFERS",     0.7475f, 0.80f, 0.2275f, 0.04125f);
            storesButton    = AddButton("BUTTON_STORES",        0.7475f, 0.85f, 0.2275f, 0.04125f);
            costsButton     = AddButton("BUTTON_MONTHLY_COSTS", 0.7475f, 0.90f, 0.2275f, 0.04125f);
            okButton        = AddButton("BUTTON_OK",            0.7475f, 0.95f, 0.2275f, 0.04125f);

            transfersButton.Clicked += new CeGui.GuiEventHandler(unimplemented);
            storesButton.Clicked    += new CeGui.GuiEventHandler(unimplemented);
            costsButton.Clicked     += new CeGui.GuiEventHandler(unimplemented);
            okButton.Clicked        += new CeGui.GuiEventHandler(ShowBasesScreen);
        }

        private CeGui.Widgets.MultiColumnList staffGrid;
        private CeGui.Widgets.MultiColumnList facilitiesGrid;
        private CeGui.Widgets.ComboBox        basesListComboBox;
        private CeGui.Widgets.PushButton      transfersButton;
        private CeGui.Widgets.PushButton      storesButton;
        private CeGui.Widgets.PushButton      costsButton;
        private CeGui.Widgets.PushButton      okButton;

        /// <summary>
        /// Creates and populates a MultiColumnListBox which holds summary details for staff in this base
        /// </summary>
        private void InitializeStaffGrid()
        {
            staffGrid = GuiBuilder.CreateGrid("staffGrid");
            AddWidget(staffGrid, 0.01f, 0.22f, 0.70f, 0.18f);
            staffGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_STAFF,       staffGrid.ColumnCount, 0.69f);
            staffGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_IDLE,        staffGrid.ColumnCount, 0.15f);
            staffGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_STAFF_TOTAL, staffGrid.ColumnCount, 0.15f);

            PopulateStaffGrid();
        }

        /// <summary>
        /// Creates and populates a MultiColumnListBox which holds summary details for facilities in this base
        /// </summary>
        private void InitializeFacilitiesGrid()
        {
            facilitiesGrid = GuiBuilder.CreateGrid("facilitiesGrid");
            AddWidget(facilitiesGrid, 0.01f, 0.47f, 0.70f, 0.52f);
            facilitiesGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_SPACE_TYPE, facilitiesGrid.ColumnCount, 0.54f);
            facilitiesGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_IN_USE,     facilitiesGrid.ColumnCount, 0.15f);
            facilitiesGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_TOTAL,      facilitiesGrid.ColumnCount, 0.15f);
            facilitiesGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_BUILDING,   facilitiesGrid.ColumnCount, 0.15f);

            PopulateFacilitiesGrid();
        }

        /// <summary>
        /// Put the statistics into the staff grid
        /// <remarks>ToDo: This is just a dummy stub.</remarks>
        /// </summary>
        private void PopulateStaffGrid()
        {
            // ToDo: replace stub
            AddRowToStaffGrid("Soldiers");
            AddRowToStaffGrid("Engineers");
            AddRowToStaffGrid("Scientists");
        }

        /// <remarks>ToDo: This is just a dummy stub.</remarks>
        private void AddRowToStaffGrid(string name)
        {
            // ToDo: replace stub
            CeGui.Widgets.ListboxItem listboxItem = Util.CreateListboxItem(name);
            int rowNum = staffGrid.AddRow(listboxItem, 0);
            AddNumericElementToGrid(staffGrid, 1, rowNum, 0);
            AddNumericElementToGrid(staffGrid, 2, rowNum, 0);
        }

        /// <summary>
        /// Put the statistics into the facilities grid
        /// </summary>
        private void PopulateFacilitiesGrid()
        {
            // Do the capacities
            foreach (String typeName in BaseCapacities.CapacityTypes)
            {
                BaseCapacityInfo info = SelectedBase.Statistics.Capacities[typeName];
                AddRowToFacilityGrid(
                    BaseCapacities.ToDisplayString(typeName),
                    info.InUse,
                    info.Total,
                    info.Building);
            }

            // The unique facilities
            AddUniqueFacilityStatsToGrid("FAC_SHORT_RANGE_NEUDAR");
            AddUniqueFacilityStatsToGrid("FAC_LONG_RANGE_NEUDAR");
            AddUniqueFacilityStatsToGrid("FAC_TACHYON_EMISSIONS_DETECTOR");
            AddUniqueFacilityStatsToGrid("FAC_GRAVITY_SHIELD_FACILITY");
            AddUniqueFacilityStatsToGrid("FAC_NEURAL_SHIELDING_FACILITY");

            // ToDo: Defense rating                
            AddRowToFacilityGrid("Defense Strength", 0, 0, 0);
        }

        /// <summary>
        /// Add the stats for this facility type to the grid
        /// </summary>
        /// <param name="facilityName"></param>
        private void AddUniqueFacilityStatsToGrid(String facilityName)
        {
            //ToDo: skip items that have not yet been researched

            FacilityHandle facility = SelectedBase.Floorplan.FindUniqueFacility(facilityName);
            String name = Xenocide.StaticTables.FacilityList[facilityName].Name;
            if (null == facility)
            {
                AddRowToFacilityGrid(name, 0, 0, 0);
            }
            else if (facility.IsUnderConstruction)
            {
                AddRowToFacilityGrid(name, 0, 0, 1);
            }
            else
            {
                AddRowToFacilityGrid(name, 1, 1, 0);
            }
        }

        /// <summary>
        /// Add a row to the facilities grid
        /// </summary>
        /// <param name="typeName">value to put in the type name column</param>
        /// <param name="inUse">value to put in the inUse column</param>
        /// <param name="total">value to put in the total column</param>
        /// <param name="building">value to put in the bulding column</param>
        private void AddRowToFacilityGrid(string typeName, uint inUse, uint total, uint building)
        {
            CeGui.Widgets.ListboxItem listboxItem = Util.CreateListboxItem(typeName);
            int rowNum = facilitiesGrid.AddRow(listboxItem, 0);
            AddNumericElementToGrid(facilitiesGrid, 1, rowNum, inUse);
            AddNumericElementToGrid(facilitiesGrid, 2, rowNum, total);
            AddNumericElementToGrid(facilitiesGrid, 3, rowNum, building);
        }

        /// <summary>
        /// Add a cell holding an integer value to a MultiColumnList
        /// </summary>
        /// <param name="grid">the multicolumn list</param>
        /// <param name="column">to put the element in (zero based)</param>
        /// <param name="row">to put the element in (zero based)</param>
        /// <param name="value">integer to show in the element</param>
        private static void AddNumericElementToGrid(CeGui.Widgets.MultiColumnList grid, int column, int row, uint value)
        {
            grid.SetGridItem(column, row, Util.CreateListboxItem(Util.StringFormat("{0}", value)));
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>user wants to look at a different base</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnBaseSelectionChanged(object sender, WindowEventArgs e)
        {
            CeGui.Widgets.ListboxItem item = basesListComboBox.SelectedItem;
            if (item != null)
            {
                selectedBaseIndex = basesListComboBox.GetItemIndex(item);
                // Need to completely redraw screen
                ScreenManager.ScheduleScreen(new BaseInfoScreen(Game, selectedBaseIndex));
            }
        }

        /// <summary>Replace this screen with matching BasesScreen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowBasesScreen(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.ScheduleScreen(new BasesScreen(Game, selectedBaseIndex));
        }

        #endregion event handlers

        #region Fields

        private IHumanBaseService humanBaseService;

        /// <summary>
        /// The base we're showing the details for
        /// </summary>
        private HumanBase SelectedBase { get { return humanBaseService.HumanBases[selectedBaseIndex]; } }

        // index specifying the human base that screen is showwing
        private int selectedBaseIndex;

        #endregion Fields
    }
}
