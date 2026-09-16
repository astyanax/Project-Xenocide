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
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Gum.Forms;
using Gum.Forms.Controls;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Facilities;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.Utils;

using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// Displays detailed information about an outpost including facilities,
    /// staff, defense strength, and provides access to monthly costs.
    /// </summary>
    /// <remarks>
    /// ARCHITECTURE: Screen delegates to BaseInfoScreenController for game logic.
    /// Uses ScreenLayout for standard layout: scrollable content area (left),
    /// button bar (right) with Transfers/Stores/Costs/OK buttons, and a status
    /// bar (bottom). Content includes outpost selector, name editor, staff grid,
    /// and facilities grid.
    /// </remarks>
    sealed partial class BaseInfoScreen : GumScreen
    {
        private ScreenLayout layout;
        private ContentArea content;

        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="selectedOutpostIndex">Index to outpost screen is to show</param>
        public BaseInfoScreen(int selectedOutpostIndex)
            : base("BaseInfoScreen")
        {
            this.selectedOutpostIndex = selectedOutpostIndex;
            this.controller = new Controller(SelectedOutpost);
        }

        protected override bool HasGumxLayout => false;

        #region Create the Gum controls

        /// <summary>
        /// Builds the screen layout using ScreenLayout and ContentArea.
        /// </summary>
        /// <summary>This screen edits the base name, so global letter shortcuts must not fire.</summary>
        public override bool HandlesTextInput => true;

        protected override void CreateGumControls()
        {
            layout = new ScreenLayout();
            layout.AddToRoot();
            content = new ContentArea(layout.ContentPanel);

            // Button bar (right side)
            layout.AddButton(XenocideResourceManager.Get("BUTTON_TRANSFERS"), OnTransfersButton);
            layout.AddButton(XenocideResourceManager.Get("BUTTON_STORES"), OnStoresButton);
            layout.AddButton(XenocideResourceManager.Get("BUTTON_MONTHLY_COSTS"), OnMonthlyCostsButton);
            layout.AddButton(XenocideResourceManager.Get("BUTTON_OK"), ShowBasesScreen);

            // Outpost selector combo box
            outpostsListComboBox = new ComboBox();
            outpostsListComboBox.Visual.Width = 300;
            content.Panel.AddChild(outpostsListComboBox);
            foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
                outpostsListComboBox.Items.Add(outpost.Name);
            outpostsListComboBox.SelectedIndex = selectedOutpostIndex;
            outpostsListComboBox.SelectionChanged += (s, args) => OnOutpostSelectionChanged(s, EventArgs.Empty);

            // Outpost name editor
            nameEditBox = new TextBox();
            nameEditBox.Visual.Width = 300;
            content.Panel.AddChild(nameEditBox);
            nameEditBox.Text = SelectedOutpost.Name;
            nameEditBox.PreviewTextInput += (s, args) => OnOutpostNameChange(s, EventArgs.Empty);

            content.AddSpacer(10);

            // Staff grid
            InitializeStaffGrid();

            content.AddSpacer(10);

            // Facilities grid
            InitializeFacilitiesGrid();
        }

        private StyledGrid staffGrid;
        private StyledGrid facilitiesGrid;
        private ComboBox outpostsListComboBox;
        private TextBox nameEditBox;

        /// <summary>
        /// Creates and populates a GridPanel which holds summary details for staff in this outpost
        /// </summary>
        private void InitializeStaffGrid()
        {
            staffGrid = new StyledGrid();
            staffGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_STAFF, (int)(0.69f * 800));
            staffGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_IDLE, (int)(0.15f * 800));
            staffGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_STAFF_TOTAL, (int)(0.15f * 800));
            content.AddGrid(staffGrid);

            PopulateStaffGrid();
        }

        /// <summary>
        /// Creates and populates a GridPanel which holds summary details for facilities in this outpost
        /// </summary>
        private void InitializeFacilitiesGrid()
        {
            facilitiesGrid = new StyledGrid();
            facilitiesGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_SPACE_TYPE, (int)(0.54f * 800));
            facilitiesGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_IN_USE, (int)(0.15f * 800));
            facilitiesGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_TOTAL, (int)(0.15f * 800));
            facilitiesGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_BUILDING, (int)(0.15f * 800));
            content.AddGrid(facilitiesGrid);

            PopulateFacilitiesGrid();
        }

        /// <summary>
        /// Put the statistics into the staff grid
        /// </summary>
        private void PopulateStaffGrid()
        {
            AddRowToStaffGrid("ITEM_PERSON_SOLDIER");
            AddRowToStaffGrid("ITEM_PERSON_ENGINEER");
            AddRowToStaffGrid("ITEM_PERSON_SCIENTIST");
        }

        /// <summary>
        /// Add a row of information to the Staff Grid
        /// </summary>
        /// <param name="staffType">Type of people this row is about</param>
        private void AddRowToStaffGrid(string staffType)
        {
            string typeName = Controller.GetStaffTypeName(staffType);
            var (idle, total) = controller.GetStaffCounts(staffType);

            int rowNum = staffGrid.RowCount;
            staffGrid.AddRow(rowNum, typeName, idle.ToString(CultureInfo.InvariantCulture), total.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Put the statistics into the facilities grid
        /// </summary>
        private void PopulateFacilitiesGrid()
        {
            // Do the capacities
            foreach (String typeName in OutpostCapacities.CapacityTypes)
            {
                OutpostCapacityInfo info = SelectedOutpost.Statistics.Capacities[typeName];
                AddRowToFacilityGrid(
                    OutpostCapacities.ToDisplayString(typeName),
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

            CalcDefenseStrength();
        }

        /// <summary>
        /// Calculate the outpost's defensive strength and put on grid
        /// </summary>
        private void CalcDefenseStrength()
        {
            var (inUse, total, building) = controller.GetDefenseStrength();
            AddRowToFacilityGrid(Strings.SCREEN_BASEINFO_ROW_DEFENSE_STRENGTH, inUse, total, building);
        }

        /// <summary>
        /// Add the stats for this facility type to the grid
        /// </summary>
        /// <param name="facilityName">identifer for type of facility</param>
        private void AddUniqueFacilityStatsToGrid(String facilityName)
        {
            String name = Controller.GetFacilityName(facilityName);
            var stats = controller.GetUniqueFacilityStats(facilityName);
            if (stats.HasValue)
            {
                AddRowToFacilityGrid(name, stats.Value.inUse, stats.Value.total, stats.Value.building);
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
            int rowNum = facilitiesGrid.RowCount;
            facilitiesGrid.AddRow(rowNum, typeName, inUse.ToString(CultureInfo.InvariantCulture), total.ToString(CultureInfo.InvariantCulture), building.ToString(CultureInfo.InvariantCulture));
        }

        #endregion Create the Gum controls

        #region event handlers

        /// <summary>user wants to look at a different outpost</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnOutpostSelectionChanged(object sender, EventArgs e)
        {
            int index = outpostsListComboBox.SelectedIndex;
            if (index >= 0)
            {
                selectedOutpostIndex = index;
                // Need to completely redraw screen
                ScreenManager.ScheduleScreen(new BaseInfoScreen(selectedOutpostIndex));
            }
        }

        /// <summary>user wants to change name of this outpost</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnOutpostNameChange(object sender, EventArgs e)
        {
            string text = nameEditBox.Text;

            if (controller.TryRenameOutpost(text))
            {
                outpostsListComboBox.Text = text;
                outpostsListComboBox.Items[selectedOutpostIndex] = text;
            }
            else
            {
                // Put current name back into box
                nameEditBox.Text = controller.GetCurrentName();
            }
        }

        /// <summary>User has clicked the "Transfers" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnTransfersButton(object sender, EventArgs e)
        {
            ShowTransfersScreen();
        }

        /// <summary>User has clicked the "Stores" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnStoresButton(object sender, EventArgs e)
        {
            ShowStoresScreen();
        }

        /// <summary>User has clicked the "Monthly Costs" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnMonthlyCostsButton(object sender, EventArgs e)
        {
            ShowMonthlyCostsScreen();
        }

        /// <summary>Replace this screen with matching BasesScreen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowBasesScreen(object sender, EventArgs e)
        {
            ScreenManager.ScheduleScreen(new BasesScreen(selectedOutpostIndex));
        }

        #endregion event handlers

        /// <summary>Got to screen that show the shipments for this outpost</summary>
        private void ShowTransfersScreen()
        {
            ScreenManager.ScheduleScreen(new ShowTransfersScreen(selectedOutpostIndex));
        }

        /// <summary>Got to screen that show the stores for this outpost</summary>
        private void ShowStoresScreen()
        {
            ScreenManager.ScheduleScreen(new StoresScreen(selectedOutpostIndex));
        }

        /// <summary>Go to screen that show the monthly costs for this outpost</summary>
        private void ShowMonthlyCostsScreen()
        {
            ScreenManager.ScheduleScreen(new MonthlyCostsScreen(selectedOutpostIndex));
        }

        #region Fields

        /// <summary>
        /// Controller handling game logic for base information.
        /// </summary>
        private Controller controller;

        /// <summary>
        /// The outpost we're showing the details for
        /// </summary>
        private Outpost SelectedOutpost { get { return Xenocide.GameState.GeoData.Outposts[selectedOutpostIndex]; } }

        // index specifying the outpost that screen is showing
        private int selectedOutpostIndex;

        #endregion Fields
    }
}
