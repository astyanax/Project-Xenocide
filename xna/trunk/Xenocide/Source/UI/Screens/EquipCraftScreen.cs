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
* @file EquipCraftScreen.cs
* @date Created: 2007/07/14
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
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.StaticData.Items;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// This is the screen that allows user set craft's weapons and crew
    /// </summary>
    public class EquipCraftScreen : Screen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="selectedOutpostIndex">Index to outpost that owns the craft</param>
        public EquipCraftScreen(int selectedOutpostIndex)
            : base("EquipCraftScreen")
        {
            this.selectedOutpostIndex = selectedOutpostIndex;
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // add text giving the name of the selected base
            baseNameText = GuiBuilder.CreateText(CeguiId + "_baseNameText");
            AddWidget(baseNameText, 0.01f, 0.06f, 0.2275f, 0.04f);
            baseNameText.Text = Util.StringFormat(Strings.SCREEN_EQUIP_CRAFT_BASE_NAME,
                SelectedOutpost.Name);

            // The craft and weapons grids
            InitializeCraftGrid();
            InitializeWeaponsGrid();
            PopulateCraftGrid();
            PopulateWeaponsGrid();

            // text describing Pod 1
            pod1Text = GuiBuilder.CreateText(CeguiId + "_pod1Text");
            AddWidget(pod1Text, 0.01f, 0.55f, 0.2275f, 0.16125f);

            // text describing Pod 2
            pod2Text = GuiBuilder.CreateText(CeguiId + "_pod2Text");
            AddWidget(pod2Text, 0.31f, 0.55f, 0.2275f, 0.16125f);

            // other buttons
            emptyPod1Button = AddButton("BUTTON_EMPTY_POD_1", 0.7475f, 0.75f, 0.2275f, 0.04125f);
            emptyPod2Button = AddButton("BUTTON_EMPTY_POD_2", 0.7475f, 0.80f, 0.2275f, 0.04125f);
            setPod1Button = AddButton("BUTTON_SET_POD_1", 0.7475f, 0.85f, 0.2275f, 0.04125f);
            setPod2Button = AddButton("BUTTON_SET_POD_2", 0.7475f, 0.90f, 0.2275f, 0.04125f);
            closeButton = AddButton("BUTTON_CLOSE", 0.7475f, 0.95f, 0.2275f, 0.04125f);

            emptyPod1Button.Clicked += new CeGui.GuiEventHandler(OnEmptyPod1Button);
            emptyPod2Button.Clicked += new CeGui.GuiEventHandler(OnEmptyPod2Button);
            setPod1Button.Clicked += new CeGui.GuiEventHandler(OnSetPod1Button);
            setPod2Button.Clicked += new CeGui.GuiEventHandler(OnSetPod2Button);
            closeButton.Clicked += new CeGui.GuiEventHandler(OnCloseButton);

            weaponsGrid.MouseDoubleClicked += new CeGui.MouseEventHandler(OnWeaponGridMouseDoubleClicked);
        }

        private CeGui.Widgets.StaticText baseNameText;
        private CeGui.Widgets.StaticText pod1Text;
        private CeGui.Widgets.StaticText pod2Text;
        private CeGui.Widgets.MultiColumnList craftGrid;
        private CeGui.Widgets.MultiColumnList weaponsGrid;
        private CeGui.Widgets.PushButton emptyPod1Button;
        private CeGui.Widgets.PushButton emptyPod2Button;
        private CeGui.Widgets.PushButton setPod1Button;
        private CeGui.Widgets.PushButton setPod2Button;
        private CeGui.Widgets.PushButton closeButton;

        /// <summary>
        /// Create the grid that shows the craft
        /// </summary>
        private void InitializeCraftGrid()
        {
            craftGrid = GuiBuilder.CreateGrid("craftGrid");
            AddWidget(craftGrid, 0.01f, 0.13f, 0.70f, 0.42f);
            craftGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_CRAFT_NAME, craftGrid.ColumnCount, 0.20f);
            craftGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_FUEL, craftGrid.ColumnCount, 0.13f);
            craftGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_HULL, craftGrid.ColumnCount, 0.13f);
            craftGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_PODS, craftGrid.ColumnCount, 0.13f);
            craftGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_AMMO, craftGrid.ColumnCount, 0.13f);
            craftGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_CREW, craftGrid.ColumnCount, 0.13f);
            craftGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_HWP, craftGrid.ColumnCount, 0.13f);

            craftGrid.SelectionChanged += new WindowEventHandler(OnCraftGridSelectionChanged);
        }

        /// <summary>
        /// Create MultiColumnListBox which holds weapons
        /// </summary>
        private void InitializeWeaponsGrid()
        {
            weaponsGrid = GuiBuilder.CreateGrid("weaponsGrid");
            AddWidget(weaponsGrid, 0.01f, 0.75f, 0.70f, 0.20f);
            weaponsGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_ITEM, weaponsGrid.ColumnCount, 0.50f);
            weaponsGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_QUANTITY_IN_BASE, weaponsGrid.ColumnCount, 0.12f);
            weaponsGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_CLIP_SIZE, weaponsGrid.ColumnCount, 0.13f);
            weaponsGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_ROUNDS_IN_BASE, weaponsGrid.ColumnCount, 0.23f);
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
                rowToCraft[rowNum] = aircraft;
            }
        }

        /// <summary>
        /// Put the list of items available equip a craft with on the grid
        /// </summary>
        private void PopulateWeaponsGrid()
        {
            rowToWeapon.Clear();
            weaponsGrid.ResetList();

            foreach (Item i in SelectedOutpost.Inventory.ListContents())
            {
                // if the item is a craft weapon, add it to the weaponsGrid
                if (i.ItemInfo is CraftWeaponItemInfo)
                {
                    WeaponRow weaponRow = new WeaponRow(i, SelectedOutpost.Inventory);
                    CeGui.ListboxTextItem listboxItem = Util.CreateListboxItem(weaponRow.Name);
                    int rowNum = weaponsGrid.AddRow(listboxItem, 0);
                    listboxItem.ID = rowNum;

                    Util.AddNumericElementToGrid(weaponsGrid, 1, rowNum, weaponRow.OnHand);
                    Util.AddNumericElementToGrid(weaponsGrid, 2, rowNum, weaponRow.ClipSize);
                    Util.AddNumericElementToGrid(weaponsGrid, 3, rowNum, weaponRow.ClipsInBase);

                    // and record weapon associated with this row
                    rowToWeapon[rowNum] = weaponRow;
                }
            }
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>Handle user clicking on the "Empty Pod 1" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnEmptyPod1Button(object sender, GuiEventArgs e)
        {
            EmptyPod(1);
        }

        /// <summary>Handle user clicking on the "Empty Pod 2" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnEmptyPod2Button(object sender, GuiEventArgs e)
        {
            EmptyPod(2);
        }

        /// <summary>Handle user clicking on the "Set Pod 1" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnSetPod1Button(object sender, GuiEventArgs e)
        {
            EquipPod(1);
        }

        /// <summary>Handle user clicking on the "Set Pod 2" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnSetPod2Button(object sender, GuiEventArgs e)
        {
            EquipPod(2);
        }

        /// <summary>
        /// User has double clicked on the weapons grid, assume user wants to arm craft with this weapon
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        private void OnWeaponGridMouseDoubleClicked(object sender, MouseEventArgs e)
        {
            TryEquipCraftWithSelectedWeapon();
        }

        /// <summary>React to user pressing the Close button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnCloseButton(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.ScheduleScreen(new BasesScreen(selectedOutpostIndex));
        }

        /// <summary>Handles user clicking on a craft in the grid</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnCraftGridSelectionChanged(object sender, WindowEventArgs e)
        {
            CeGui.Widgets.ListboxItem item = craftGrid.GetFirstSelectedItem();
            if (item != null)
            {
                DrawPodInformation(rowToCraft[item.ID]);
            }
        }

        #endregion event handlers

        /// <summary>
        /// Try to equip the selected craft with the selected weapon
        /// </summary>
        private void TryEquipCraftWithSelectedWeapon()
        {
            CeGui.Widgets.ListboxItem selectedRow = GetSelectedCraft();
            if (null != selectedRow)
            {
                Aircraft aircraft = rowToCraft[selectedRow.ID];
                for (int podId = 0; podId < aircraft.WeaponPods.Count; ++podId)
                {
                    // can only equip if there's an empty pod to put it in
                    if (null == aircraft.WeaponPods[podId])
                    {
                        EquipPod(podId + 1);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Empty weapon pod of selected craft
        /// </summary>
        /// <param name="podId">empty Pod 1 or Pod 2</param>
        private void EmptyPod(int podId)
        {
            CeGui.Widgets.ListboxItem selectedRow = GetSelectedCraft();
            if (null != selectedRow)
            {
                Aircraft aircraft = rowToCraft[selectedRow.ID];

                // can only empty pod if craft has a pod and it's not empty
                if ((podId <= aircraft.WeaponPods.Count) && (null != aircraft.WeaponPods[podId - 1]))
                {
                    // put the weapon in the base, remove from craft & update display
                    SelectedOutpost.Inventory.Add(aircraft.WeaponPods[podId - 1], false);
                    aircraft.WeaponPods[podId - 1] = null;
                    Refresh(selectedRow);
                }
            }
        }

        /// <summary>
        /// Put selected weapon into specified pod of selected craft
        /// </summary>
        /// <param name="podId">empty Pod 1 or Pod 2</param>
        private void EquipPod(int podId)
        {
            CeGui.Widgets.ListboxItem selectedRow = GetSelectedCraft();
            if (null != selectedRow)
            {
                Aircraft aircraft = rowToCraft[selectedRow.ID];

                // need to have a weapon selected and craft actually has a pod 
                WeaponRow weaponRow = GetSelectedWeapon();
                if ((null != weaponRow) && (podId <= aircraft.WeaponPods.Count))
                {
                    if (weaponRow.EquipPod(aircraft, podId))
                    {
                        Refresh(selectedRow);
                    }
                }
            }
        }

        /// <summary>
        /// Redraw display
        /// </summary>
        /// <param name="selectedRow">Currently selected row in craftGrid</param>
        private void Refresh(CeGui.Widgets.ListboxItem selectedRow)
        {
            Aircraft aircraft = rowToCraft[selectedRow.ID];

            // Update craft grid (pods & ammo)
            int row = craftGrid.GetRowIndexOfItem(selectedRow);
            CeGui.Widgets.GridReference position = new CeGui.Widgets.GridReference(row, 3);
            craftGrid.GetItemAtGridReference(position).Text = aircraft.PodCountStatus;
            position.Column = 4;
            craftGrid.GetItemAtGridReference(position).Text = aircraft.AmmoStatus;

            // Redraw pod info for selected craft
            DrawPodInformation(aircraft);

            // Redraw weapons available in base
            PopulateWeaponsGrid();
        }

        /// <summary>
        /// Get currently selected Craft from Craft Grid.  Give error message if nothing selected
        /// or craft is not in an outpost
        /// </summary>
        /// <returns>selected row of craftGrid</returns>
        private CeGui.Widgets.ListboxItem GetSelectedCraft()
        {
            CeGui.Widgets.ListboxItem selectedItem = craftGrid.GetFirstSelectedItem();
            if (null == selectedItem)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_CRAFT_SELECTED);
            }
            else
            {
                Aircraft aircraft = rowToCraft[selectedItem.ID];
                if (!aircraft.InBase)
                {
                    selectedItem = null;
                    Util.ShowMessageBox(Strings.MSGBOX_CRAFT_NOT_IN_BASE, aircraft.Name);
                }
            }
            return selectedItem;
        }

        /// <summary>
        /// Get currently selected Weapon from Weapons Grid.  Give error message if nothing selected 
        /// </summary>
        /// <returns>currently selected Weapon</returns>
        private WeaponRow GetSelectedWeapon()
        {
            CeGui.Widgets.ListboxItem selectedItem = weaponsGrid.GetFirstSelectedItem();
            if (null == selectedItem)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_WEAPON_SELECTED);
                return null;
            }
            return rowToWeapon[selectedItem.ID];
        }

        /// <summary>
        /// Show the information for the weapon pods for a craft
        /// </summary>
        /// <param name="aircraft">craft to show weapon pod information</param>
        private void DrawPodInformation(Aircraft aircraft)
        {
            // Start assuming craft has no pods
            pod1Text.Hide();
            pod2Text.Hide();

            // Document Pod 1, if it exists
            if (0 < aircraft.WeaponPods.Count)
            {
                DrawPodInformation(pod1Text, aircraft.WeaponPods[0], 1);
            }

            // Document Pod 2, if it exists
            if (1 < aircraft.WeaponPods.Count)
            {
                DrawPodInformation(pod2Text, aircraft.WeaponPods[1], 2);
            }
        }

        /// <summary>
        /// Update the details for a specific weapon pod
        /// </summary>
        /// <param name="textControl">Control to write details to</param>
        /// <param name="pod">Pod to get information for</param>
        /// <param name="podId">Pod 1 or Pod 2?</param>
        private static void DrawPodInformation(CeGui.Widgets.StaticText textControl, WeaponPod pod, int podId)
        {

            textControl.Show();
            StringBuilder info = new StringBuilder(Util.StringFormat(Strings.SCREEN_EQUIP_CRAFT_POD_NAME, podId));
            info.Append(Util.Linefeed);
            // can't just call pod.PodInformationString(), because pod may be null
            info.Append(WeaponPod.PodInformationString(pod));
            textControl.Text = info.ToString();
        }

        /// <summary>
        /// The data backing a row in the weaponsGrid
        /// </summary>
        private class WeaponRow
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="item">Item to build WeaponRow</param>
            /// <param name="inventory">inventory item will be taken from</param>
            public WeaponRow(Item item, OutpostInventory inventory)
            {
                this.item = item;
                this.inventory = inventory;
            }

            /// <summary>
            /// Equip pod with this weapon
            /// </summary>
            /// <param name="aircraft">Aircraft owning the pod</param>
            /// <param name="podId">Pod 1 or Pod 2?</param>
            public bool EquipPod(Aircraft aircraft, int podId)
            {
                // can only put weapon in pod if pod is empty
                if (null == aircraft.WeaponPods[podId - 1])
                {
                    // construct a pod, remove equivelent from inventory and add to craft
                    WeaponPod pod = (WeaponPod)Weapon.Manufacture();
                    aircraft.WeaponPods[podId - 1] = pod;
                    inventory.Remove(pod);
                    return true;
                }
                else
                {
                    Util.ShowMessageBox(Strings.MSGBOX_POD_ALREADY_HAS_WEAPON);
                    return false;
                }
            }

            /// <summary>
            /// Number of weapons of this type on hand
            /// </summary>
            public int OnHand { get { return inventory.NumberInInventory(item.ItemInfo); } }

            /// <summary>
            /// Number of rounds stored in a full clip
            /// </summary>
            public string ClipSize { get { return Weapon.ClipSizeString(); } }

            /// <summary>
            /// Number of rounds available in base
            /// </summary>
            public string ClipsInBase
            {
                get
                {
                    if (null == Weapon.Clip)
                    {
                        return Strings.SCREEN_EQUIP_CRAFT_IRRELEVANT;
                    }
                    else
                    {
                        return Util.StringFormat("{0}", inventory.NumberInArmory(Weapon.Clip.Id));
                    }
                }
            }

            /// <summary>
            /// Name of weapon, to show to user
            /// </summary>
            public String Name { get { return item.Name; } }

            /// <summary>
            /// Get the weapon details of the item
            /// </summary>
            private CraftWeaponItemInfo Weapon { get { return item.ItemInfo as CraftWeaponItemInfo; } }

            #region Fields

            /// <summary>
            /// Type of item
            /// </summary>
            private Item item;

            /// <summary>
            /// inventory items are stored in
            /// </summary>
            private OutpostInventory inventory;

            #endregion Fields
        }

        #region Fields

        /// <summary>
        /// The outpost purchases will be sent to
        /// </summary>
        private Outpost SelectedOutpost { get { return Xenocide.GameState.GeoData.Outposts[selectedOutpostIndex]; } }

        // index specifying the outpost that items will be taken from
        private int selectedOutpostIndex;

        /// <summary>
        /// Map rows in weaponsGrid to weapons
        /// </summary>
        private Dictionary<int, WeaponRow> rowToWeapon = new Dictionary<int, WeaponRow>();

        /// <summary>
        /// Map row in craftGrid to actual craft
        /// </summary>
        private Dictionary<int, Aircraft> rowToCraft = new Dictionary<int, Aircraft>();

        #endregion Fields
    }
}
