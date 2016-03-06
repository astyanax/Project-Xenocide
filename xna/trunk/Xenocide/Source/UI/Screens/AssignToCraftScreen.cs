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
* @file AssignToCraftScreen.cs
* @date Created: 2007/11/05
* @author File creator: Oded Coster
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CeGui;
using ProjectXenocide.Model.Geoscape;

using ProjectXenocide.Utils;
using CeGui.Widgets;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.StaticData.Items;
using System.Globalization;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// In this screen soldiers and xcaps get assigned to aircraft.
    /// </summary>
    public class AssignToCraftScreen : Screen
    {
        /// <summary>
        /// Constructs a screen listing the soldiers stationed at the given base.
        /// </summary>
        public AssignToCraftScreen(int selectedOutpostIndex)
            : base("AssignToCraftScreen", @"Content\Textures\UI\BasesScreenBackground.png")
        {
            this.selectedOutpostIndex = selectedOutpostIndex;
            this.soldiers = new List<Person>(SelectedOutpost.ListStaff("ITEM_PERSON_SOLDIER"));
            this.xcaps = new List<Item>(SelectedOutpost.ListXcaps());
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons and grids to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // add text giving the name of the selected base
            baseNameText = GuiBuilder.CreateText(CeguiId + "_baseNameText");
            AddWidget(baseNameText, 0.01f, 0.06f, 0.2275f, 0.04f);
            baseNameText.Text = Util.StringFormat(Strings.SCREEN_EQUIP_CRAFT_BASE_NAME,
                SelectedOutpost.Name);

            // The craft, soldiers and xcaps grids
            InitializeCraftGrid();
            PopulateCraftGrid();

            InitializeSoldierGrid();
            PopulateSoldierGrid();

            InitializeXcapGrid();
            PopulateXcapGrid();

            addXcapButton = AddButton("BUTTON_ADD_XCAP", 0.7475f, 0.65f, 0.2275f, 0.04125f);
            removeXcapButton = AddButton("BUTTON_REMOVE_XCAP", 0.7475f, 0.70f, 0.2275f, 0.04125f);
            addSoldierButton = AddButton("BUTTON_ADD_SOLDIER", 0.7475f, 0.75f, 0.2275f, 0.04125f);
            removeSoldierButton = AddButton("BUTTON_REMOVE_SOLDIER", 0.7475f, 0.80f, 0.2275f, 0.04125f);
            soldierUpButton = AddButton("BUTTON_SOLDIER_UP", 0.7475f, 0.85f, 0.2275f, 0.04125f);
            soldierDownButton = AddButton("BUTTON_SOLDIER_DOWN", 0.7475f, 0.90f, 0.2275f, 0.04125f);
            closeButton = AddButton("BUTTON_CLOSE", 0.7475f, 0.95f, 0.2275f, 0.04125f);

            addXcapButton.Clicked += new CeGui.GuiEventHandler(OnAddXcapButton);
            removeXcapButton.Clicked += new CeGui.GuiEventHandler(OnRemoveXcapButton);
            addSoldierButton.Clicked += new CeGui.GuiEventHandler(OnAddSoldierButton);
            removeSoldierButton.Clicked += new CeGui.GuiEventHandler(OnRemoveSoldierButton);
            soldierUpButton.Clicked += new CeGui.GuiEventHandler(OnSoldierUpButton);
            soldierDownButton.Clicked += new CeGui.GuiEventHandler(OnSoldierDownButton);
            closeButton.Clicked += new CeGui.GuiEventHandler(OnCloseButton);

            soldierGrid.MouseDoubleClicked += new CeGui.MouseEventHandler(OnSoldierOrCraftGridMouseDoubleClicked);
            craftGrid.MouseDoubleClicked += new CeGui.MouseEventHandler(OnSoldierOrCraftGridMouseDoubleClicked);
            xcapGrid.MouseDoubleClicked += new CeGui.MouseEventHandler(OnXCapGridMouseDoubleClicked);
        }

        private CeGui.Widgets.StaticText baseNameText;
        private CeGui.Widgets.MultiColumnList craftGrid;
        private CeGui.Widgets.MultiColumnList soldierGrid;
        private CeGui.Widgets.MultiColumnList xcapGrid;
        private CeGui.Widgets.PushButton closeButton;
        private CeGui.Widgets.PushButton addXcapButton;
        private CeGui.Widgets.PushButton removeXcapButton;
        private CeGui.Widgets.PushButton addSoldierButton;
        private CeGui.Widgets.PushButton removeSoldierButton;

        //Todo Replace Up/Down buttons with some other mechanism to reassign craft positions
        private CeGui.Widgets.PushButton soldierUpButton;
        private CeGui.Widgets.PushButton soldierDownButton;

        /// <summary>
        /// Create the grid that shows the craft
        /// </summary>
        private void InitializeCraftGrid()
        {
            craftGrid = AddGrid(0.01f, 0.13f, 0.70f, 0.22f,
                Strings.SCREEN_EQUIP_CRAFT_COLUMN_CRAFT_NAME, 0.20f,
                Strings.SCREEN_EQUIP_CRAFT_COLUMN_FUEL, 0.13f,
                Strings.SCREEN_EQUIP_CRAFT_COLUMN_HULL, 0.13f,
                Strings.SCREEN_EQUIP_CRAFT_COLUMN_PODS, 0.13f,
                Strings.SCREEN_EQUIP_CRAFT_COLUMN_AMMO, 0.13f,
                Strings.SCREEN_EQUIP_CRAFT_COLUMN_CREW, 0.13f,
                Strings.SCREEN_EQUIP_CRAFT_COLUMN_HWP, 0.13f
            );

            craftGrid.SelectionChanged += new WindowEventHandler(OnCraftGridSelectionChanged);
        }

        /// <summary>
        /// Create the grid that shows the soldiers
        /// </summary>
        private void InitializeSoldierGrid()
        {
            soldierGrid = AddGrid(0.01f, 0.36f, 0.70f, 0.32f,
                Strings.SCREEN_ASSIGN_CRAFT_COLUMN_SOLDIER_NAME, 0.40f,
                Strings.SCREEN_ASSIGN_CRAFT_COLUMN_ASSIGNED_CRAFT, 0.33f,
                Strings.SCREEN_ASSIGN_CRAFT_COLUMN_POSITION_CRAFT, 0.23f
            );
        }

        /// <summary>
        /// Create the grid that shows the soldiers
        /// </summary>
        private void InitializeXcapGrid()
        {
            xcapGrid = AddGrid(0.01f, 0.69f, 0.70f, 0.25f,
                Strings.SCREEN_ASSIGN_CRAFT_COLUMN_XCAP_TYPE, 0.50f,
                Strings.SCREEN_ASSIGN_CRAFT_COLUMN_AVAILABLE, 0.25f,
                Strings.SCREEN_ASSIGN_CRAFT_COLUMN_ASSIGNED_COUNT, 0.24f
            );
        }


        /// <summary>
        /// Put the list of craft into the grid
        /// </summary>
        private void PopulateCraftGrid()
        {
            foreach (Craft craft in SelectedOutpost.Fleet)
            {
                // add craft to grid
                Aircraft aircraft = (Aircraft)craft;

                if (aircraft.CanCarrySoldiers && aircraft.InBase)
                {
                    CeGui.ListboxTextItem listboxItem = Util.CreateListboxItem(aircraft.Name);
                    int rowNum = craftGrid.AddRow(listboxItem, 0);
                    listboxItem.ID = rowNum;

                    Util.AddNumericElementToGrid(craftGrid, 1, rowNum, aircraft.FuelPercent);
                    Util.AddNumericElementToGrid(craftGrid, 2, rowNum, aircraft.HullPercent);
                    Util.AddNumericElementToGrid(craftGrid, 3, rowNum, aircraft.PodCountStatus);
                    Util.AddNumericElementToGrid(craftGrid, 4, rowNum, aircraft.AmmoStatus);
                    Util.AddNumericElementToGrid(craftGrid, 5, rowNum, aircraft.SoldierCountStatus);
                    Util.AddNumericElementToGrid(craftGrid, 6, rowNum, aircraft.XcapCountStatus);

                    // record craft associated with this line
                    rowToCraft.Add(aircraft);
                }
            }
        }

        /// <summary>
        /// Put the list of soldiers into the grid
        /// </summary>
        private void PopulateSoldierGrid()
        {
            foreach (Person soldier in this.soldiers)
            {
                CeGui.ListboxTextItem listboxItem = Util.CreateListboxItem(soldier.Name);
                int rowNum = soldierGrid.AddRow(listboxItem, 0);
                listboxItem.ID = rowNum;
                rowToSoldier.Add(soldier);

                bool soldierFound = false;

                foreach (Craft craft in SelectedOutpost.Fleet)
                {
                    Aircraft aircraft = craft as Aircraft;

                    if (aircraft.CanCarrySoldiers && aircraft.Soldiers.ContainsKey(soldier))
                    {
                        Util.AddStringElementToGrid(soldierGrid, 1, rowNum, aircraft.Name);
                        Util.AddNumericElementToGrid(soldierGrid, 2, rowNum, aircraft.Soldiers[soldier]);
                        soldierFound = true;
                    }
                }

                if (!soldierFound)
                {
                    Util.AddStringElementToGrid(soldierGrid, 1, rowNum, Strings.UNASSIGNED);
                    Util.AddStringElementToGrid(soldierGrid, 2, rowNum, Strings.NON_APPLICABLE);
                }
            }
        }

        /// <summary>
        /// Put the list of soldiers into the grid
        /// </summary>
        private void PopulateXcapGrid()
        {
            foreach (Item invXcap in this.xcaps)
            {
                CeGui.ListboxTextItem listboxItem = Util.CreateListboxItem(invXcap.Name);
                int rowNum = xcapGrid.AddRow(listboxItem, 0);
                listboxItem.ID = rowNum;
                rowToXcap.Add(invXcap);

                Util.AddNumericElementToGrid(xcapGrid, 1, rowNum, SelectedOutpost.Inventory.NumberInArmory(invXcap.ItemInfo.Id));
                Util.AddStringElementToGrid(xcapGrid, 2, rowNum, Strings.NON_APPLICABLE);
            }
        }

        /// <summary>
        /// Update xcap grid display to correspond to currently selected aircraft
        /// </summary>
        private void UpdateXcapGrid(CeGui.Widgets.ListboxItem selectedRow)
        {
            Aircraft aircraft = rowToCraft[selectedRow.ID];

            xcapGrid.ResetList();
            rowToXcap.Clear();

            PopulateXcapGrid();

            //Add xcaps that are on craft but not in inventory
            foreach (Item xcap in aircraft.XCaps)
            {
                if (!this.xcaps.Contains(xcap))
                {
                    CeGui.ListboxTextItem listboxItem = Util.CreateListboxItem(xcap.Name);
                    int rowNum = xcapGrid.AddRow(listboxItem, 0);
                    listboxItem.ID = rowNum;
                    rowToXcap.Add(xcap);

                    int xcapsFound = CountItemsOnCraft(xcap.ItemInfo.Id, aircraft);

                    Util.AddNumericElementToGrid(xcapGrid, 1, rowNum, 0);
                    Util.AddNumericElementToGrid(xcapGrid, 2, rowNum, xcapsFound);
                }
            }

        }

        /// <summary>
        /// Updates selected craft and selected soldier
        /// </summary>
        private void UpdateSoldierAndCraft()
        {

            //Update craft
            CeGui.Widgets.ListboxItem craftItem = craftGrid.GetFirstSelectedItem();

            if (null != craftItem)
            {
                Aircraft aircraft = rowToCraft[craftItem.ID];
                int row = craftGrid.GetRowIndexOfItem(craftItem);
                CeGui.Widgets.GridReference position = new CeGui.Widgets.GridReference(row, 5);
                craftGrid.GetItemAtGridReference(position).Text = aircraft.SoldierCountStatus;
            }
            else
            {
                //No craft selected
                craftGrid.ResetList();
                rowToCraft.Clear();
                PopulateCraftGrid();
            }

            //Update soldier
            CeGui.Widgets.ListboxItem soldierItem = soldierGrid.GetFirstSelectedItem();

            if (null != soldierItem)
            {
                Person soldier = rowToSoldier[soldierItem.ID];

                int row = soldierGrid.GetRowIndexOfItem(soldierItem);
                CeGui.Widgets.GridReference position = new CeGui.Widgets.GridReference(row, 1);

                if (null != craftItem)
                {
                    Aircraft aircraft = rowToCraft[craftItem.ID];
                    if (aircraft.Soldiers.ContainsKey(soldier))
                    {
                        soldierGrid.GetItemAtGridReference(position).Text = aircraft.Name;
                        UpdateSoldierPosition(soldier, aircraft.Soldiers[soldier]);
                    }
                    else
                    {
                        soldierGrid.GetItemAtGridReference(position).Text = Strings.UNASSIGNED;
                        position.Column = 2;
                        soldierGrid.GetItemAtGridReference(position).Text = Strings.NON_APPLICABLE;
                    }
                }
                else
                {
                    soldierGrid.GetItemAtGridReference(position).Text = Strings.UNASSIGNED;
                    position.Column = 2;
                    soldierGrid.GetItemAtGridReference(position).Text = Strings.NON_APPLICABLE;
                }
            }
        }


        /// <summary>
        /// Updates soldier row with position
        /// </summary>
        /// <param name="soldier">Soldier row to be updated</param>
        /// <param name="position">position to be updated to</param>
        private void UpdateSoldierPosition(Person soldier, int position)
        {
            int row = rowToSoldier.IndexOf(soldier);
            CeGui.Widgets.GridReference gridPosition = new CeGui.Widgets.GridReference(row, 2);

            soldierGrid.GetItemAtGridReference(gridPosition).Text = position.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <summary>
        /// Updates selected craft and selected xcap
        /// </summary>
        private void UpdateXcapAndCraft()
        {
            CeGui.Widgets.ListboxItem craftItem = craftGrid.GetFirstSelectedItem();

            if (null != craftItem)
            {
                Aircraft aircraft = rowToCraft[craftItem.ID];
                int row = craftGrid.GetRowIndexOfItem(craftItem);
                CeGui.Widgets.GridReference position = new CeGui.Widgets.GridReference(row, 6);
                craftGrid.GetItemAtGridReference(position).Text = aircraft.XcapCountStatus;
            }

            //Update xcap
            CeGui.Widgets.ListboxItem xcapItem = xcapGrid.GetFirstSelectedItem();

            if (null != xcapItem)
            {
                Aircraft aircraft = rowToCraft[craftItem.ID];
                Item xcap = rowToXcap[xcapItem.ID];

                int row = xcapGrid.GetRowIndexOfItem(xcapItem);
                CeGui.Widgets.GridReference position = new CeGui.Widgets.GridReference(row, 1);
                xcapGrid.GetItemAtGridReference(position).Text = SelectedOutpost.Inventory.NumberInArmory(xcap.ItemInfo.Id).ToString(CultureInfo.InvariantCulture.NumberFormat);
                position.Column = 2;
                xcapGrid.GetItemAtGridReference(position).Text = CountItemsOnCraft(xcap.ItemInfo.Id, aircraft).ToString(CultureInfo.InvariantCulture.NumberFormat);

            }
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>React to user pressing the Close button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnCloseButton(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.ScheduleScreen(new SoldiersListScreen(selectedOutpostIndex));
        }


        /// <summary>React to user pressing the soldier up button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnSoldierUpButton(object sender, CeGui.GuiEventArgs e)
        {
            RepositionSoldier(1);
        }

        /// <summary>React to user pressing the soldier down button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnSoldierDownButton(object sender, CeGui.GuiEventArgs e)
        {
            RepositionSoldier(-1);
        }

        /// <summary>React to user pressing the Assign Soldier button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnAddSoldierButton(object sender, CeGui.GuiEventArgs e)
        {
            TryAssignSoldierToCraft();
        }

        /// <summary>React to user pressing the Remove Soldier button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnRemoveSoldierButton(object sender, CeGui.GuiEventArgs e)
        {
            if (TryUnassignSoldierFromCraft())
            {
                UpdateSoldierAndCraft();
            }
        }

        /// <summary>React to user pressing the Assign Xcap button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnAddXcapButton(object sender, CeGui.GuiEventArgs e)
        {
            TryAssignXcapToCraft();
        }

        /// <summary>React to user pressing the Remove Xcap button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnRemoveXcapButton(object sender, CeGui.GuiEventArgs e)
        {
            if (TryUnassignXcapFromCraft())
            {
                UpdateXcapAndCraft();
            }
        }

        /// <summary>Handles user clicking on a craft in the grid</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnCraftGridSelectionChanged(object sender, WindowEventArgs e)
        {
            CeGui.Widgets.ListboxItem item = craftGrid.GetFirstSelectedItem();
            if (item != null)
            {
                //Update the xcaps grid
                UpdateXcapGrid(item);
            }
        }

        /// <summary>
        /// User has double clicked on the soldier grid.  Try to assign soldier to current aircraft
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        private void OnSoldierOrCraftGridMouseDoubleClicked(object sender, MouseEventArgs e)
        {
            TryAssignSoldierToCraft();
        }

        /// <summary>
        /// User has double clicked on the X-Cap grid.  Try to assign xcap to current aircraft
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        private void OnXCapGridMouseDoubleClicked(object sender, MouseEventArgs e)
        {
            TryAssignXcapToCraft();
        }

        #endregion event handlers

        #region Private functions
        /// <summary>
        /// Try to assign the selected soldier to the selected craft
        /// </summary>
        /// <returns>true if suceeded, false if not</returns>
        private bool TryAssignSoldierToCraft()
        {
            //Get selected craft
            Aircraft selectedAircraft = GetSelectedCraft(true);

            if (null != selectedAircraft)
            {
                Person selectedSoldier = GetSelectedSoldier(true);

                if (null != selectedSoldier)
                {
                    Aircraft craftWithSoldier = selectedSoldier.Aircraft;

                    //Assign/unassign soldier according to:
                    // - If unassigned, assign to this craft
                    // - If assigned to this craft, show error
                    // - If assigned to another craft, show error

                    //Check that max has not been exceeded (before assigning)
                    if (selectedAircraft.Soldiers.Count < selectedAircraft.MaxHumans)
                    {
                        if (null == craftWithSoldier)
                        {
                            //Add soldier to next available position
                            selectedAircraft.Soldiers.Add(selectedSoldier, GetNextAvailablePosition(selectedAircraft));
                            UpdateSoldierAndCraft();
                            return true;
                        }
                        else if (selectedAircraft == craftWithSoldier)
                        {
                            //Todo possibly unassign from craft
                            Util.ShowMessageBox(Strings.MSGBOX_SOLDIER_ALREADY_ASSIGNED_THIS_CRAFT);
                        }
                        else
                        {
                            //Todo Possibly unassign from craftWithSoldier and reassign to this selectedAircraft.
                            Util.ShowMessageBox(Strings.MSGBOX_SOLDIER_ALREADY_ASSIGNED_OTHER_CRAFT);
                        }
                    }
                    else
                    {
                        Util.ShowMessageBox(Strings.MSGBOX_CRAFT_FULL_HUMANS);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Try to unassign the selected soldier from the selected craft
        /// </summary>
        /// <returns>true if suceeded, false if not</returns>
        private bool TryUnassignSoldierFromCraft()
        {
            Person selectedSoldier = GetSelectedSoldier(true);

            if (null != selectedSoldier)
            {
                Aircraft craftWithSoldier = selectedSoldier.Aircraft;

                if (null == craftWithSoldier)
                {
                    Util.ShowMessageBox(Strings.MSGBOX_SOLDIER_NOT_ASSIGNED);
                }
                else
                {
                    craftWithSoldier.Remove(selectedSoldier);
                    return true;
                }

            }

            return false;
        }

        /// <summary>
        /// Try to assign the selected xcap to the selected craft
        /// </summary>
        /// <returns>true if suceeded, false if not</returns>
        private bool TryAssignXcapToCraft()
        {
            //Get selected craft
            Aircraft selectedAircraft = GetSelectedCraft(true);

            if (null != selectedAircraft)
            {
                CeGui.Widgets.ListboxItem selectedXcap = xcapGrid.GetFirstSelectedItem();

                if (null != selectedXcap)
                {
                    Item xcap = rowToXcap[selectedXcap.ID];
                    if (SelectedOutpost.Inventory.NumberInArmory(xcap.ItemInfo.Id) == 0)
                    {
                        Util.ShowMessageBox(Strings.MSGBOX_NO_MORE_XCAPS_OUTPOST);
                    }
                    else
                    {
                        //Check that max has not been exceeded (before assigning)
                        if (selectedAircraft.XCaps.Count < selectedAircraft.MaxXcaps)
                        {
                            SelectedOutpost.Inventory.Remove(xcap);
                            selectedAircraft.XCaps.Add(xcap);
                            UpdateXcapAndCraft();
                            return true;
                        }
                        else
                        {
                            Util.ShowMessageBox(Strings.MSGBOX_CRAFT_FULL_XCAPS);
                        }
                    }
                }
                else
                {
                    Util.ShowMessageBox(Strings.MSGBOX_NO_XCAP_SELECTED);
                }
            }

            return false;
        }

        /// <summary>
        /// Try to unassign the selected xcap from the selected craft
        /// </summary>
        /// <returns>true if suceeded, false if not</returns>
        private bool TryUnassignXcapFromCraft()
        {
            //Get selected craft
            Aircraft selectedAircraft = GetSelectedCraft(true);

            if (null != selectedAircraft)
            {
                CeGui.Widgets.ListboxItem selectedXcap = xcapGrid.GetFirstSelectedItem();

                if (null != selectedXcap)
                {
                    Item xcap = rowToXcap[selectedXcap.ID];
                    if (selectedAircraft.XCaps.Count == 0)
                    {
                        Util.ShowMessageBox(Strings.MSGBOX_NO_MORE_XCAPS_CRAFT);
                    }
                    else
                    {
                        selectedAircraft.XCaps.Remove(xcap);
                        SelectedOutpost.Inventory.Add(xcap, false);
                        return true;
                    }
                }
                else
                {
                    Util.ShowMessageBox(Strings.MSGBOX_NO_XCAP_SELECTED);
                }
            }

            return false;
        }

        /// <summary>
        /// Counts items of a type that are in the aircraft
        /// </summary>
        /// <param name="type">type of items</param>
        /// <param name="aircraft">aircraft to search</param>
        /// <returns>number of items</returns>
        private static int CountItemsOnCraft(string type, Aircraft aircraft)
        {
            int itemsFound = 0;

            foreach (Item xcap in aircraft.XCaps)
            {
                if (xcap.ItemInfo.Id == type)
                {
                    itemsFound++;
                }
            }

            return itemsFound;
        }

        /// <summary>
        /// Finds the next available position on aircraft
        /// </summary>
        /// <param name="aircraft">Aircraft that is searched on</param>
        /// <returns>Index of available position</returns>
        private static int GetNextAvailablePosition(Aircraft aircraft)
        {
            for (int i = 1; i <= aircraft.MaxHumans; i++)
            {
                if (!aircraft.Soldiers.ContainsValue(i))
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>
        /// Get currently selected Craft from Craft Grid.  Give error message if nothing selected
        /// </summary>
        /// <returns>aircraft corresponding to selected row of craftGrid</returns>
        private Aircraft GetSelectedCraft(bool warn)
        {
            CeGui.Widgets.ListboxItem selectedItem = craftGrid.GetFirstSelectedItem();
            if (null == selectedItem)
            {
                if (warn)
                {
                    Util.ShowMessageBox(Strings.MSGBOX_NO_CRAFT_SELECTED);
                }
                return null;
            }
            else
            {
                Aircraft aircraft = rowToCraft[selectedItem.ID];
                Debug.Assert(aircraft.InBase);
                return aircraft;
            }
        }

        /// <summary>
        /// Get currently selected Soldier from Soldier Grid.  Give error message if nothing selected
        /// </summary>
        /// <param name="warn">Give warning if no soldier selected?</param>
        /// <returns>soldier corresponding to selected row of craftGrid</returns>
        private Person GetSelectedSoldier(bool warn)
        {
            CeGui.Widgets.ListboxItem selectedItem = soldierGrid.GetFirstSelectedItem();
            if (null == selectedItem)
            {
                if (warn)
                {
                    Util.ShowMessageBox(Strings.MSGBOX_NO_SOLDIER_SELECTED);
                }
                return null;
            }
            else
            {
                return rowToSoldier[selectedItem.ID];
            }
        }

        /// <summary>
        /// Repositions soldier within craft
        /// </summary>
        /// <param name="distance">distance to new position</param>
        private void RepositionSoldier(int distance)
        {
            Person soldier = GetSelectedSoldier(false);
            if (null != soldier)
            {
                Aircraft craft = soldier.Aircraft;
                if (null != craft)
                {
                    //int currentPosition = craft.Soldiers[soldier];
                    int newPosition = craft.Soldiers[soldier] + distance;

                    //Is movement possible?
                    if (newPosition < 1 || newPosition > craft.MaxHumans)
                    {
                        Util.ShowMessageBox(Strings.MSGBOX_NO_POSITION);
                    }
                    else
                    {
                        // If there's a soldier already at newPosition, move soldier
                        foreach (KeyValuePair<Person, int> pair in craft.Soldiers)
                        {
                            if (pair.Value == newPosition)
                            {
                                craft.Soldiers[pair.Key] = craft.Soldiers[soldier];
                                UpdateSoldierPosition(pair.Key, craft.Soldiers[soldier]);
                                break;
                            }
                        }

                        // put soldier into new position
                        craft.Soldiers[soldier] = newPosition;
                        UpdateSoldierPosition(soldier, newPosition);
                    }
                }
                else
                {
                    Util.ShowMessageBox(Strings.MSGBOX_SOLDIER_NOT_ASSIGNED);
                }
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// The outpost where craft and soldiers are stationed
        /// </summary>
        private Outpost SelectedOutpost { get { return Xenocide.GameState.GeoData.Outposts[selectedOutpostIndex]; } }

        // index specifying the outpost where craft and soldiers are stationed
        private int selectedOutpostIndex;

        /// <summary>
        /// The soldiers listed on this screen.
        /// </summary>
        private readonly List<Person> soldiers;

        /// <summary>
        /// The xcaps listed on this screen.
        /// </summary>
        private readonly List<Item> xcaps;

        /// <summary>
        /// Map row in craftGrid to actual craft
        /// </summary>
        private List<Aircraft> rowToCraft = new List<Aircraft>();

        /// <summary>
        /// Map row in soldierGrid to actual soldier
        /// </summary>
        private List<Person> rowToSoldier = new List<Person>();

        /// <summary>
        /// Map row in xcapGrid to actual xcap
        /// </summary>
        private List<Item> rowToXcap = new List<Item>();

        #endregion Fields
    }
}
