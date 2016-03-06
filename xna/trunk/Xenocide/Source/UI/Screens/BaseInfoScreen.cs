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
using System.Text;

using CeGui;


using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Facilities;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    class BaseInfoScreen : Screen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="selectedOutpostIndex">Index to outpost screen is to show</param>
        public BaseInfoScreen(int selectedOutpostIndex)
            : base("BaseInfoScreen")
        {
            this.selectedOutpostIndex = selectedOutpostIndex;
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // combo box to allow user to pick outpost to work on
            outpostsListComboBox = GuiBuilder.CreateComboBox("outpostsListComboBox");
            AddWidget(outpostsListComboBox, 0.7475f, 0.06f, 0.2275f, 0.40f);
            Misc.PopulateHumanBasesList(outpostsListComboBox, selectedOutpostIndex);
            outpostsListComboBox.ListSelectionAccepted += new WindowEventHandler(OnOutpostSelectionChanged);

            // The girds detailing staff and facilities in outpost
            InitializeStaffGrid();
            InitializeFacilitiesGrid();

            // other buttons
            transfersButton = AddButton("BUTTON_TRANSFERS", 0.7475f, 0.80f, 0.2275f, 0.04125f);
            storesButton = AddButton("BUTTON_STORES", 0.7475f, 0.85f, 0.2275f, 0.04125f);
            costsButton = AddButton("BUTTON_MONTHLY_COSTS", 0.7475f, 0.90f, 0.2275f, 0.04125f);
            okButton = AddButton("BUTTON_OK", 0.7475f, 0.95f, 0.2275f, 0.04125f);

            // edit box for outpost name
            nameEditBox = AddEditBox("EDITBOX_NAME", 0.01f, 0.06f, 0.70f, 0.12f);
            nameEditBox.Font = FontManager.Instance.GetFont("LargeBaseName");
            nameEditBox.Text = SelectedOutpost.Name;
            nameEditBox.TextAccepted += new WindowEventHandler(OnOutpostNameChange);

            transfersButton.Clicked += new CeGui.GuiEventHandler(OnTransfersButton);
            storesButton.Clicked += new CeGui.GuiEventHandler(OnStoresButton);
            costsButton.Clicked += new CeGui.GuiEventHandler(OnMonthlyCostsButton);
            okButton.Clicked += new CeGui.GuiEventHandler(ShowBasesScreen);
        }

        private CeGui.Widgets.MultiColumnList staffGrid;
        private CeGui.Widgets.MultiColumnList facilitiesGrid;
        private CeGui.Widgets.ComboBox outpostsListComboBox;
        private CeGui.Widgets.PushButton transfersButton;
        private CeGui.Widgets.PushButton storesButton;
        private CeGui.Widgets.PushButton costsButton;
        private CeGui.Widgets.PushButton okButton;
        private CeGui.Widgets.EditBox nameEditBox;

        /// <summary>
        /// Creates and populates a MultiColumnListBox which holds summary details for staff in this outpost
        /// </summary>
        private void InitializeStaffGrid()
        {
            staffGrid = GuiBuilder.CreateGrid("staffGrid");
            AddWidget(staffGrid, 0.01f, 0.22f, 0.70f, 0.18f);
            staffGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_STAFF, staffGrid.ColumnCount, 0.69f);
            staffGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_IDLE, staffGrid.ColumnCount, 0.15f);
            staffGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_STAFF_TOTAL, staffGrid.ColumnCount, 0.15f);

            PopulateStaffGrid();
        }

        /// <summary>
        /// Creates and populates a MultiColumnListBox which holds summary details for facilities in this outpost
        /// </summary>
        private void InitializeFacilitiesGrid()
        {
            facilitiesGrid = GuiBuilder.CreateGrid("facilitiesGrid");
            AddWidget(facilitiesGrid, 0.01f, 0.47f, 0.70f, 0.52f);
            facilitiesGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_SPACE_TYPE, facilitiesGrid.ColumnCount, 0.54f);
            facilitiesGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_IN_USE, facilitiesGrid.ColumnCount, 0.15f);
            facilitiesGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_TOTAL, facilitiesGrid.ColumnCount, 0.15f);
            facilitiesGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_BUILDING, facilitiesGrid.ColumnCount, 0.15f);

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
            // figure out number of people of this type and number working
            string typeName = Xenocide.StaticTables.ItemList[staffType].Name;
            int total = Util.SequenceLength(SelectedOutpost.ListStaff(staffType));
            int idle = Util.SequenceLength(SelectedOutpost.ListStaff(staffType, false));

            // create the row
            CeGui.Widgets.ListboxItem listboxItem = Util.CreateListboxItem(typeName);
            int rowNum = staffGrid.AddRow(listboxItem, 0);
            Util.AddNumericElementToGrid(staffGrid, 1, rowNum, idle);
            Util.AddNumericElementToGrid(staffGrid, 2, rowNum, total);
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
            uint baseDefenseStrength = 0;
            uint defensesInUse = 0;
            uint defensesUnderConstruction = 0;
            foreach (FacilityHandle f in SelectedOutpost.Floorplan.Facilities)
            {
                DefenseFacilityInfo df = f.FacilityInfo as DefenseFacilityInfo;
                if (df != null)
                {
                    if (!f.IsUnderConstruction)
                    {
                        baseDefenseStrength += (uint)df.DefenseStrength;
                        ++defensesInUse;
                    }
                    else
                    {
                        defensesUnderConstruction += (uint)df.DefenseStrength;
                    }
                }
            }

            // Defense rating
            AddRowToFacilityGrid(Strings.SCREEN_BASEINFO_ROW_DEFENSE_STRENGTH, defensesInUse,
                baseDefenseStrength, defensesUnderConstruction);
        }

        /// <summary>
        /// Add the stats for this facility type to the grid
        /// </summary>
        /// <param name="facilityName">identifer for type of facility</param>
        private void AddUniqueFacilityStatsToGrid(String facilityName)
        {
            FacilityHandle facility = SelectedOutpost.Floorplan.FindUniqueFacility(facilityName);
            String name = Xenocide.StaticTables.FacilityList[facilityName].Name;
            if (null == facility)
            {
                if (Xenocide.GameState.GeoData.XCorp.TechManager.IsAvailable(facilityName))
                {
                    AddRowToFacilityGrid(name, 0, 0, 0);
                }
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
            Util.AddNumericElementToGrid(facilitiesGrid, 1, rowNum, inUse);
            Util.AddNumericElementToGrid(facilitiesGrid, 2, rowNum, total);
            Util.AddNumericElementToGrid(facilitiesGrid, 3, rowNum, building);
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>user wants to look at a different outpost</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnOutpostSelectionChanged(object sender, WindowEventArgs e)
        {
            CeGui.Widgets.ListboxItem item = outpostsListComboBox.SelectedItem;
            if (item != null)
            {
                selectedOutpostIndex = outpostsListComboBox.GetItemIndex(item);
                // Need to completely redraw screen
                ScreenManager.ScheduleScreen(new BaseInfoScreen(selectedOutpostIndex));
            }
        }

        /// <summary>user wants to change name of this outpost</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnOutpostNameChange(object sender, WindowEventArgs e)
        {
            string text = nameEditBox.Text;
            bool valid = true;

            // If name is identical, do nothing (in case enter pressed repeatedly)
            if (SelectedOutpost.Name == text)
            {
                return;
            }

            // Ensure something was given
            if (String.IsNullOrEmpty(text))
            {
                Util.ShowMessageBox(Strings.MSGBOX_BASE_NEEDS_NAME);
                valid = false;
            }
            else
            {
                // See if name already exists for a different outpost. For this comparison
                // ignore upper/lower case.
                foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
                {
                    if ((outpost != SelectedOutpost)
                        && text.Equals(outpost.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Util.ShowMessageBox(Strings.MSGBOX_BASE_NAMES_ARE_UNIQUE, text);
                        valid = false;
                        break;
                    }
                }
            }

            // If name is valid update it
            if (valid)
            {
                SelectedOutpost.Name = text;
                outpostsListComboBox.Text = text;
                outpostsListComboBox[selectedOutpostIndex].Text = text;
                Util.ShowMessageBox(Strings.MSGBOX_BASE_NAME_CHANGED);
            }
            else
            {
                // Put current name back into box
                nameEditBox.Text = SelectedOutpost.Name;
            }
        }

        /// <summary>User has clicked the "Transfers" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnTransfersButton(object sender, CeGui.GuiEventArgs e)
        {
            ShowTransfersScreen();
        }

        /// <summary>User has clicked the "Stores" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnStoresButton(object sender, CeGui.GuiEventArgs e)
        {
            ShowStoresScreen();
        }

        /// <summary>User has clicked the "Monthly Costs" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnMonthlyCostsButton(object sender, CeGui.GuiEventArgs e)
        {
            ShowMonthlyCostsScreen();
        }

        /// <summary>Replace this screen with matching BasesScreen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowBasesScreen(object sender, CeGui.GuiEventArgs e)
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
        /// The outpost we're showing the details for
        /// </summary>
        private Outpost SelectedOutpost { get { return Xenocide.GameState.GeoData.Outposts[selectedOutpostIndex]; } }

        // index specifying the outpost that screen is showing
        private int selectedOutpostIndex;

        #endregion Fields
    }
}
