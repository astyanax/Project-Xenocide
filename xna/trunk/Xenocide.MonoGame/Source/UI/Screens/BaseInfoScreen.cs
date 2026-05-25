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
    class BaseInfoScreen : GumScreen
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

        #region Create the Gum controls

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateGumControls()
        {
            if (GumRoot != null)
            {
                WireButton("transfersButton", OnTransfersButton);
                WireButton("storesButton", OnStoresButton);
                WireButton("costsButton", OnMonthlyCostsButton);
                WireButton("okButton", ShowBasesScreen);

                outpostsListComboBox = new ComboBox();
                outpostsListComboBox.Visual.X = 20;
                outpostsListComboBox.Visual.Y = 20;
                outpostsListComboBox.Visual.Width = 300;
                AddChild(outpostsListComboBox);
                foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
                    outpostsListComboBox.Items.Add(outpost.Name);
                outpostsListComboBox.SelectedIndex = selectedOutpostIndex;
                outpostsListComboBox.SelectionChanged += (s, args) => OnOutpostSelectionChanged(s, EventArgs.Empty);

                nameEditBox = new TextBox();
                nameEditBox.Visual.X = 20;
                nameEditBox.Visual.Y = 55;
                nameEditBox.Visual.Width = 300;
                AddChild(nameEditBox);
                nameEditBox.Text = SelectedOutpost.Name;
                nameEditBox.PreviewTextInput += (s, args) => OnOutpostNameChange(s, EventArgs.Empty);

                InitializeStaffGrid();
                staffGrid.Visual.X = 20;
                staffGrid.Visual.Y = 90;
                staffGrid.Visual.Width = 380;

                InitializeFacilitiesGrid();
                facilitiesGrid.Visual.X = 420;
                facilitiesGrid.Visual.Y = 90;
                facilitiesGrid.Visual.Width = 380;
                return;
            }

            // combo box to allow user to pick outpost to work on
            outpostsListComboBox = new ComboBox();
            RootContainer.AddChild(outpostsListComboBox);
            foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
            {
                outpostsListComboBox.Items.Add(outpost.Name);
            }
            outpostsListComboBox.SelectedIndex = selectedOutpostIndex;
            outpostsListComboBox.SelectionChanged += (s, args) => OnOutpostSelectionChanged(s, EventArgs.Empty);

            // The girds detailing staff and facilities in outpost
            InitializeStaffGrid();
            InitializeFacilitiesGrid();

            // other buttons
            transfersButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_TRANSFERS") };
            RootContainer.AddChild(transfersButton);
            storesButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_STORES") };
            RootContainer.AddChild(storesButton);
            costsButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_MONTHLY_COSTS") };
            RootContainer.AddChild(costsButton);
            okButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_OK") };
            RootContainer.AddChild(okButton);

            // edit box for outpost name
            nameEditBox = new TextBox();
            RootContainer.AddChild(nameEditBox);
            nameEditBox.Text = SelectedOutpost.Name;
            nameEditBox.PreviewTextInput += (s, args) => OnOutpostNameChange(s, EventArgs.Empty);

            transfersButton.Click += OnTransfersButton;
            storesButton.Click += OnStoresButton;
            costsButton.Click += OnMonthlyCostsButton;
            okButton.Click += ShowBasesScreen;
        }

        private GridPanel staffGrid;
        private GridPanel facilitiesGrid;
        private ComboBox outpostsListComboBox;
        private Button transfersButton;
        private Button storesButton;
        private Button costsButton;
        private Button okButton;
        private TextBox nameEditBox;

        /// <summary>
        /// Creates and populates a GridPanel which holds summary details for staff in this outpost
        /// </summary>
        private void InitializeStaffGrid()
        {
            staffGrid = new GridPanel();
            AddChild(staffGrid.Visual);
            staffGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_STAFF, (int)(0.69f * 800));
            staffGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_IDLE, (int)(0.15f * 800));
            staffGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_STAFF_TOTAL, (int)(0.15f * 800));

            PopulateStaffGrid();
        }

        /// <summary>
        /// Creates and populates a GridPanel which holds summary details for facilities in this outpost
        /// </summary>
        private void InitializeFacilitiesGrid()
        {
            facilitiesGrid = new GridPanel();
            AddChild(facilitiesGrid.Visual);
            facilitiesGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_SPACE_TYPE, (int)(0.54f * 800));
            facilitiesGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_IN_USE, (int)(0.15f * 800));
            facilitiesGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_TOTAL, (int)(0.15f * 800));
            facilitiesGrid.AddColumn(Strings.SCREEN_BASEINFO_COLUMN_BUILDING, (int)(0.15f * 800));

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
            int rowNum = staffGrid.RowCount;
            staffGrid.AddRow(rowNum, typeName, idle.ToString(), total.ToString());
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
            int rowNum = facilitiesGrid.RowCount;
            facilitiesGrid.AddRow(rowNum, typeName, inUse.ToString(), total.ToString(), building.ToString());
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
                outpostsListComboBox.Items[selectedOutpostIndex] = text;
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
        /// The outpost we're showing the details for
        /// </summary>
        private Outpost SelectedOutpost { get { return Xenocide.GameState.GeoData.Outposts[selectedOutpostIndex]; } }

        // index specifying the outpost that screen is showing
        private int selectedOutpostIndex;

        #endregion Fields
    }
}
