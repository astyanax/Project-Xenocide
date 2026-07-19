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
using System.Globalization;
using System.Text;

using Gum.Forms;
using Gum.Forms.Controls;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.Utils;

using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// In this screen soldiers and xcaps get assigned to aircraft.
    /// </summary>
    /// <remarks>
    /// ARCHITECTURE: GUI layer only — all game logic is delegated to the nested Controller
    /// class (in AssignToCraft/AssignToCraftScreenController.cs). This screen manages
    /// three grids (craft, soldiers, xcaps) and handles user interactions.
    /// </remarks>
    public partial class AssignToCraftScreen : GumScreen
    {
        /// <summary>
        /// Constructs a screen listing the soldiers stationed at the given base.
        /// </summary>
        public AssignToCraftScreen(int selectedOutpostIndex)
            : base("AssignToCraftScreen", @"Content/Textures/UI/BasesScreenBackground.png")
        {
            this.selectedOutpostIndex = selectedOutpostIndex;
            this.soldiers = new List<Person>(SelectedOutpost.ListStaff("ITEM_PERSON_SOLDIER"));
            this.xcaps = new List<Item>(SelectedOutpost.ListXcaps());
            this.controller = new Controller(SelectedOutpost);
        }

        #region Create the Gum controls

        /// <summary>
        /// add the buttons and grids to the screen
        /// </summary>
        protected override void CreateGumControls()
        {
            if (GumRoot != null)
            {
                WireButton("closeButton", OnCloseButton);
                WireButton("addXcapButton", OnAddXcapButton);
                WireButton("removeXcapButton", OnRemoveXcapButton);
                WireButton("addSoldierButton", OnAddSoldierButton);
                WireButton("removeSoldierButton", OnRemoveSoldierButton);
                WireButton("soldierUpButton", OnSoldierUpButton);
                WireButton("soldierDownButton", OnSoldierDownButton);

                baseNameText = new Label() { Text = Util.StringFormat(Strings.SCREEN_EQUIP_CRAFT_BASE_NAME, SelectedOutpost.Name) };
                baseNameText.Visual.X = 20;
                baseNameText.Visual.Y = 20;
                AddChild(baseNameText);

                InitializeCraftGrid();
                craftGrid.Visual.X = 20;
                craftGrid.Visual.Y = 50;
                craftGrid.Visual.Width = 750;
                PopulateCraftGrid();

                InitializeSoldierGrid();
                soldierGrid.Visual.X = 20;
                soldierGrid.Visual.Y = 370;
                soldierGrid.Visual.Width = 750;
                PopulateSoldierGrid();

                InitializeXcapGrid();
                xcapGrid.Visual.X = 20;
                xcapGrid.Visual.Y = 690;
                xcapGrid.Visual.Width = 750;
                PopulateXcapGrid();
                return;
            }

            baseNameText = new Label() { Text = Util.StringFormat(Strings.SCREEN_EQUIP_CRAFT_BASE_NAME, SelectedOutpost.Name) };
            RootContainer.AddChild(baseNameText);

            InitializeCraftGrid();
            PopulateCraftGrid();

            InitializeSoldierGrid();
            PopulateSoldierGrid();

            InitializeXcapGrid();
            PopulateXcapGrid();

            addXcapButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_ADD_XCAP") };
            RootContainer.AddChild(addXcapButton);
            removeXcapButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_REMOVE_XCAP") };
            RootContainer.AddChild(removeXcapButton);
            addSoldierButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_ADD_SOLDIER") };
            RootContainer.AddChild(addSoldierButton);
            removeSoldierButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_REMOVE_SOLDIER") };
            RootContainer.AddChild(removeSoldierButton);
            soldierUpButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_SOLDIER_UP") };
            RootContainer.AddChild(soldierUpButton);
            soldierDownButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_SOLDIER_DOWN") };
            RootContainer.AddChild(soldierDownButton);
            closeButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_CLOSE") };
            RootContainer.AddChild(closeButton);

            addXcapButton.Click += OnAddXcapButton;
            removeXcapButton.Click += OnRemoveXcapButton;
            addSoldierButton.Click += OnAddSoldierButton;
            removeSoldierButton.Click += OnRemoveSoldierButton;
            soldierUpButton.Click += OnSoldierUpButton;
            soldierDownButton.Click += OnSoldierDownButton;
            closeButton.Click += OnCloseButton;
        }

        private Label baseNameText;
        private GridPanel craftGrid;
        private GridPanel soldierGrid;
        private GridPanel xcapGrid;
        private Button closeButton;
        private Button addXcapButton;
        private Button removeXcapButton;
        private Button addSoldierButton;
        private Button removeSoldierButton;
        private Button soldierUpButton;
        private Button soldierDownButton;

        private void InitializeCraftGrid()
        {
            craftGrid = new GridPanel();
            craftGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_CRAFT_NAME, 140);
            craftGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_FUEL, 90);
            craftGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_HULL, 90);
            craftGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_PODS, 90);
            craftGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_AMMO, 90);
            craftGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_CREW, 90);
            craftGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_HWP, 90);
            AddChild(craftGrid.Visual);

            craftGrid.SelectionChanged += OnCraftGridSelectionChanged;
        }

        private void InitializeSoldierGrid()
        {
            soldierGrid = new GridPanel();
            soldierGrid.AddColumn(Strings.SCREEN_ASSIGN_CRAFT_COLUMN_SOLDIER_NAME, 280);
            soldierGrid.AddColumn(Strings.SCREEN_ASSIGN_CRAFT_COLUMN_ASSIGNED_CRAFT, 230);
            soldierGrid.AddColumn(Strings.SCREEN_ASSIGN_CRAFT_COLUMN_POSITION_CRAFT, 160);
            AddChild(soldierGrid.Visual);
        }

        private void InitializeXcapGrid()
        {
            xcapGrid = new GridPanel();
            xcapGrid.AddColumn(Strings.SCREEN_ASSIGN_CRAFT_COLUMN_XCAP_TYPE, 350);
            xcapGrid.AddColumn(Strings.SCREEN_ASSIGN_CRAFT_COLUMN_AVAILABLE, 175);
            xcapGrid.AddColumn(Strings.SCREEN_ASSIGN_CRAFT_COLUMN_ASSIGNED_COUNT, 170);
            AddChild(xcapGrid.Visual);
        }


        private void PopulateCraftGrid()
        {
            foreach (Craft craft in SelectedOutpost.Fleet)
            {
                Aircraft aircraft = (Aircraft)craft;

                if (aircraft.CanCarrySoldiers && aircraft.InBase)
                {
                    craftGrid.AddRow(aircraft,
                        aircraft.Name,
                        aircraft.FuelPercent.ToString(CultureInfo.InvariantCulture),
                        aircraft.HullPercent.ToString(CultureInfo.InvariantCulture),
                        aircraft.PodCountStatus.ToString(CultureInfo.InvariantCulture),
                        aircraft.AmmoStatus.ToString(CultureInfo.InvariantCulture),
                        aircraft.SoldierCountStatus.ToString(CultureInfo.InvariantCulture),
                        aircraft.XcapCountStatus.ToString());

                    rowToCraft.Add(aircraft);
                }
            }
        }

        private void PopulateSoldierGrid()
        {
            foreach (Person soldier in this.soldiers)
            {
                string craftName = Strings.UNASSIGNED;
                string position = Strings.NON_APPLICABLE;

                foreach (Craft craft in SelectedOutpost.Fleet)
                {
                    Aircraft aircraft = craft as Aircraft;

                    if (aircraft.CanCarrySoldiers && aircraft.Soldiers.TryGetValue(soldier, out var soldierPosition))
                    {
                        craftName = aircraft.Name;
                        position = Util.ToString(soldierPosition);
                        break;
                    }
                }

                soldierGrid.AddRow(soldier, soldier.Name, craftName, position);
                rowToSoldier.Add(soldier);
            }
        }

        private void PopulateXcapGrid()
        {
            foreach (Item invXcap in this.xcaps)
            {
                xcapGrid.AddRow(invXcap,
                    invXcap.Name,
                    Util.ToString(SelectedOutpost.Inventory.NumberInArmory(invXcap.ItemInfo.Id)),
                    Strings.NON_APPLICABLE);
                rowToXcap.Add(invXcap);
            }
        }

        private void UpdateXcapGrid()
        {
            Aircraft aircraft = GetSelectedCraft();

            xcapGrid.Clear();
            rowToXcap.Clear();

            PopulateXcapGrid();

            if (aircraft != null)
            {
                foreach (Item xcap in aircraft.XCaps)
                {
                    if (!this.xcaps.Contains(xcap))
                    {
                        int xcapsFound = Controller.CountItemsOnCraft(xcap.ItemInfo.Id, aircraft);
                        xcapGrid.AddRow(xcap,
                            xcap.Name,
                            Util.ToString(0),
                            Util.ToString(xcapsFound));
                        rowToXcap.Add(xcap);
                    }
                }
            }
        }

        private void UpdateSoldierAndCraft()
        {
            Aircraft craft = GetSelectedCraft();

            if (craft != null)
            {
                int craftRow = craftGrid.GetRowIndexByTag(craft);
                if (craftRow >= 0)
                    craftGrid.SetCell(craftRow, 5, craft.SoldierCountStatus);
            }
            else
            {
                craftGrid.Clear();
                rowToCraft.Clear();
                PopulateCraftGrid();
            }

            Person soldier = GetSelectedSoldier();

            if (soldier != null)
            {
                int soldierRow = soldierGrid.GetRowIndexByTag(soldier);
                if (soldierRow < 0) return;

                if (craft != null)
                {
                    if (craft.Soldiers.TryGetValue(soldier, out var craftSoldierPosition))
                    {
                        soldierGrid.SetCell(soldierRow, 1, craft.Name);
                        UpdateSoldierPosition(soldier, craftSoldierPosition);
                    }
                    else
                    {
                        soldierGrid.SetCell(soldierRow, 1, Strings.UNASSIGNED);
                        soldierGrid.SetCell(soldierRow, 2, Strings.NON_APPLICABLE);
                    }
                }
                else
                {
                    soldierGrid.SetCell(soldierRow, 1, Strings.UNASSIGNED);
                    soldierGrid.SetCell(soldierRow, 2, Strings.NON_APPLICABLE);
                }
            }
        }

        private void UpdateSoldierPosition(Person soldier, int position)
        {
            int row = soldierGrid.GetRowIndexByTag(soldier);
            if (row < 0) return;
            soldierGrid.SetCell(row, 2, position.ToString(CultureInfo.InvariantCulture.NumberFormat));
        }

        private void UpdateXcapAndCraft()
        {
            Aircraft craft = GetSelectedCraft();
            if (craft != null)
            {
                int craftRow = craftGrid.GetRowIndexByTag(craft);
                if (craftRow >= 0)
                    craftGrid.SetCell(craftRow, 6, craft.XcapCountStatus);
            }

            Item xcap = GetSelectedXcap();
            if (xcap != null && craft != null)
            {
                int xcapRow = xcapGrid.GetRowIndexByTag(xcap);
                if (xcapRow < 0) return;

                xcapGrid.SetCell(xcapRow, 1, Util.ToString(SelectedOutpost.Inventory.NumberInArmory(xcap.ItemInfo.Id)));
                xcapGrid.SetCell(xcapRow, 2, Util.ToString(Controller.CountItemsOnCraft(xcap.ItemInfo.Id, craft)));
            }
        }

        #endregion Create the Gum controls

        #region event handlers

        private void OnCloseButton(object sender, EventArgs e)
        {
            ScreenManager.ScheduleScreen(new SoldiersListScreen(selectedOutpostIndex));
        }

        private void OnSoldierUpButton(object sender, EventArgs e)
        {
            RepositionSoldier(1);
        }

        private void OnSoldierDownButton(object sender, EventArgs e)
        {
            RepositionSoldier(-1);
        }

        private void OnAddSoldierButton(object sender, EventArgs e)
        {
            TryAssignSoldierToCraft();
        }

        private void OnRemoveSoldierButton(object sender, EventArgs e)
        {
            if (TryUnassignSoldierFromCraft())
            {
                UpdateSoldierAndCraft();
            }
        }

        private void OnAddXcapButton(object sender, EventArgs e)
        {
            TryAssignXcapToCraft();
        }

        private void OnRemoveXcapButton(object sender, EventArgs e)
        {
            if (TryUnassignXcapFromCraft())
            {
                UpdateXcapAndCraft();
            }
        }

        private void OnCraftGridSelectionChanged(object sender, EventArgs e)
        {
            UpdateXcapGrid();
        }

        #endregion event handlers

        #region Private functions

        private bool TryAssignSoldierToCraft()
        {
            Aircraft selectedAircraft = GetSelectedCraft();
            Person selectedSoldier = GetSelectedSoldier();

            if (Controller.TryAssignSoldierToCraft(selectedAircraft, selectedSoldier))
            {
                UpdateSoldierAndCraft();
                return true;
            }
            return false;
        }

        private bool TryUnassignSoldierFromCraft()
        {
            Person selectedSoldier = GetSelectedSoldier();
            return Controller.TryUnassignSoldierFromCraft(selectedSoldier);
        }

        private bool TryAssignXcapToCraft()
        {
            Aircraft selectedAircraft = GetSelectedCraft();
            Item xcap = GetSelectedXcap();

            if (controller.TryAssignXcapToCraft(selectedAircraft, xcap))
            {
                UpdateXcapAndCraft();
                return true;
            }
            return false;
        }

        private bool TryUnassignXcapFromCraft()
        {
            Aircraft selectedAircraft = GetSelectedCraft();
            Item xcap = GetSelectedXcap();
            return controller.TryUnassignXcapFromCraft(selectedAircraft, xcap);
        }

        private void RepositionSoldier(int distance)
        {
            Person soldier = GetSelectedSoldier();
            if (Controller.TryRepositionSoldier(soldier, distance))
            {
                Aircraft craft = soldier.Aircraft;
                if (craft != null)
                {
                    // Refresh all soldier positions for this craft
                    foreach (var pair in craft.Soldiers)
                    {
                        UpdateSoldierPosition(pair.Key, pair.Value);
                    }
                }
            }
        }

        private Aircraft GetSelectedCraft()
        {
            Aircraft aircraft = craftGrid.GetSelectedTag() as Aircraft;
            if (null == aircraft)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_CRAFT_SELECTED);
            }
            return aircraft;
        }

        private Person GetSelectedSoldier()
        {
            Person soldier = soldierGrid.GetSelectedTag() as Person;
            if (null == soldier)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_SOLDIER_SELECTED);
            }
            return soldier;
        }

        private Item GetSelectedXcap()
        {
            return xcapGrid.GetSelectedTag() as Item;
        }

        #endregion

        #region Fields

        private Outpost SelectedOutpost { get { return Xenocide.GameState.GeoData.Outposts[selectedOutpostIndex]; } }

        private int selectedOutpostIndex;

        private readonly List<Person> soldiers;

        private readonly List<Item> xcaps;

        private List<Aircraft> rowToCraft = new List<Aircraft>();

        private List<Person> rowToSoldier = new List<Person>();

        private List<Item> rowToXcap = new List<Item>();

        private Controller controller;

        #endregion Fields
    }
}
