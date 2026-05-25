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
    /// This is the screen that allows user set craft's weapons and crew
    /// </summary>
    public class EquipCraftScreen : GumScreen
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

        #region Create the Gum controls

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateGumControls()
        {
            if (GumRoot != null)
            {
                WireButton("emptyPod1Button", OnEmptyPod1Button);
                WireButton("emptyPod2Button", OnEmptyPod2Button);
                WireButton("setPod1Button", OnSetPod1Button);
                WireButton("setPod2Button", OnSetPod2Button);
                WireButton("closeButton", OnCloseButton);

                baseNameText = new Label() { Text = Util.StringFormat(Strings.SCREEN_EQUIP_CRAFT_BASE_NAME, SelectedOutpost.Name) };
                baseNameText.Visual.X = 20;
                baseNameText.Visual.Y = 20;
                AddChild(baseNameText);
                pod1Text = new Label();
                pod1Text.Visual.X = 20;
                pod1Text.Visual.Y = 50;
                AddChild(pod1Text);
                pod2Text = new Label();
                pod2Text.Visual.X = 20;
                pod2Text.Visual.Y = 70;
                AddChild(pod2Text);

                InitializeCraftGrid();
                craftGrid.Visual.X = 20;
                craftGrid.Visual.Y = 100;
                craftGrid.Visual.Width = 750;

                InitializeWeaponsGrid();
                weaponsGrid.Visual.X = 20;
                weaponsGrid.Visual.Y = 420;
                weaponsGrid.Visual.Width = 750;
                PopulateCraftGrid();
                PopulateWeaponsGrid();
                weaponsGrid.SelectionChanged += OnWeaponGridSelectionChanged;
                return;
            }

            baseNameText = new Label() { Text = Util.StringFormat(Strings.SCREEN_EQUIP_CRAFT_BASE_NAME, SelectedOutpost.Name) };
            RootContainer.AddChild(baseNameText);

            InitializeCraftGrid();
            InitializeWeaponsGrid();
            PopulateCraftGrid();
            PopulateWeaponsGrid();

            pod1Text = new Label();
            RootContainer.AddChild(pod1Text);

            pod2Text = new Label();
            RootContainer.AddChild(pod2Text);

            emptyPod1Button = new Button() { Text = XenocideResourceManager.Get("BUTTON_EMPTY_POD_1") };
            RootContainer.AddChild(emptyPod1Button);
            emptyPod2Button = new Button() { Text = XenocideResourceManager.Get("BUTTON_EMPTY_POD_2") };
            RootContainer.AddChild(emptyPod2Button);
            setPod1Button = new Button() { Text = XenocideResourceManager.Get("BUTTON_SET_POD_1") };
            RootContainer.AddChild(setPod1Button);
            setPod2Button = new Button() { Text = XenocideResourceManager.Get("BUTTON_SET_POD_2") };
            RootContainer.AddChild(setPod2Button);
            closeButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_CLOSE") };
            RootContainer.AddChild(closeButton);

            emptyPod1Button.Click += OnEmptyPod1Button;
            emptyPod2Button.Click += OnEmptyPod2Button;
            setPod1Button.Click += OnSetPod1Button;
            setPod2Button.Click += OnSetPod2Button;
            closeButton.Click += OnCloseButton;

            weaponsGrid.SelectionChanged += OnWeaponGridSelectionChanged;
        }

        private Label baseNameText;
        private Label pod1Text;
        private Label pod2Text;
        private GridPanel craftGrid;
        private GridPanel weaponsGrid;
        private Button emptyPod1Button;
        private Button emptyPod2Button;
        private Button setPod1Button;
        private Button setPod2Button;
        private Button closeButton;

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

        private void InitializeWeaponsGrid()
        {
            weaponsGrid = new GridPanel();
            weaponsGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_ITEM, 350);
            weaponsGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_QUANTITY_IN_BASE, 84);
            weaponsGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_CLIP_SIZE, 90);
            weaponsGrid.AddColumn(Strings.SCREEN_EQUIP_CRAFT_COLUMN_ROUNDS_IN_BASE, 160);
            AddChild(weaponsGrid.Visual);
        }

        private void PopulateCraftGrid()
        {
            foreach (Craft craft in SelectedOutpost.Fleet)
            {
                Aircraft aircraft = (Aircraft)craft;
                craftGrid.AddRow(aircraft,
                    aircraft.Name,
                    aircraft.FuelPercent.ToString(),
                    aircraft.HullPercent.ToString(),
                    aircraft.PodCountStatus.ToString(),
                    aircraft.AmmoStatus.ToString(),
                    aircraft.SoldierCountStatus.ToString(),
                    aircraft.XcapCountStatus.ToString());
            }
        }

        private void PopulateWeaponsGrid()
        {
            weaponsGrid.Clear();

            foreach (Item i in SelectedOutpost.Inventory.ListContents())
            {
                if (i.ItemInfo is CraftWeaponItemInfo)
                {
                    WeaponRow weaponRow = new WeaponRow(i, SelectedOutpost.Inventory);
                    weaponsGrid.AddRow(weaponRow,
                        weaponRow.Name,
                        weaponRow.OnHand.ToString(),
                        weaponRow.ClipSize.ToString(),
                        weaponRow.ClipsInBase.ToString());
                }
            }
        }

        #endregion Create the Gum controls

        #region event handlers

        private void OnEmptyPod1Button(object sender, EventArgs e)
        {
            EmptyPod(1);
        }

        private void OnEmptyPod2Button(object sender, EventArgs e)
        {
            EmptyPod(2);
        }

        private void OnSetPod1Button(object sender, EventArgs e)
        {
            EquipPod(1);
        }

        private void OnSetPod2Button(object sender, EventArgs e)
        {
            EquipPod(2);
        }

        private void OnWeaponGridSelectionChanged(object sender, EventArgs e)
        {
            TryEquipCraftWithSelectedWeapon();
        }

        private void OnCloseButton(object sender, EventArgs e)
        {
            ScreenManager.ScheduleScreen(new BasesScreen(selectedOutpostIndex));
        }

        private void OnCraftGridSelectionChanged(object sender, EventArgs e)
        {
            Aircraft aircraft = GetSelectedCraft();
            if (aircraft != null)
            {
                DrawPodInformation(aircraft);
            }
        }

        #endregion event handlers

        private void TryEquipCraftWithSelectedWeapon()
        {
            Aircraft aircraft = GetSelectedCraft();
            if (null != aircraft)
            {
                for (int podId = 0; podId < aircraft.WeaponPods.Count; ++podId)
                {
                    if (null == aircraft.WeaponPods[podId])
                    {
                        EquipPod(podId + 1);
                        return;
                    }
                }
            }
        }

        private void EmptyPod(int podId)
        {
            Aircraft aircraft = GetSelectedCraft();
            if (null != aircraft)
            {
                if ((podId <= aircraft.WeaponPods.Count) && (null != aircraft.WeaponPods[podId - 1]))
                {
                    SelectedOutpost.Inventory.Add(aircraft.WeaponPods[podId - 1], false);
                    aircraft.WeaponPods[podId - 1] = null;
                    Refresh(aircraft);
                }
            }
        }

        private void EquipPod(int podId)
        {
            Aircraft aircraft = GetSelectedCraft();
            if (null != aircraft)
            {
                WeaponRow weaponRow = GetSelectedWeapon();
                if ((null != weaponRow) && (podId <= aircraft.WeaponPods.Count))
                {
                    if (weaponRow.EquipPod(aircraft, podId))
                    {
                        Refresh(aircraft);
                    }
                }
            }
        }

        private void Refresh(Aircraft aircraft)
        {
            int row = craftGrid.GetRowIndexByTag(aircraft);
            if (row < 0) return;

            craftGrid.SetCell(row, 1, aircraft.FuelPercent.ToString());
            craftGrid.SetCell(row, 2, aircraft.HullPercent.ToString());
            craftGrid.SetCell(row, 3, aircraft.PodCountStatus.ToString());
            craftGrid.SetCell(row, 4, aircraft.AmmoStatus.ToString());
            craftGrid.SetCell(row, 5, aircraft.SoldierCountStatus.ToString());
            craftGrid.SetCell(row, 6, aircraft.XcapCountStatus.ToString());

            DrawPodInformation(aircraft);

            PopulateWeaponsGrid();
        }

        private Aircraft GetSelectedCraft()
        {
            Aircraft aircraft = craftGrid.GetSelectedTag() as Aircraft;
            if (null == aircraft)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_CRAFT_SELECTED);
                return null;
            }

            if (!aircraft.InBase)
            {
                Util.ShowMessageBox(Strings.MSGBOX_CRAFT_NOT_IN_BASE, aircraft.Name);
                return null;
            }
            return aircraft;
        }

        private WeaponRow GetSelectedWeapon()
        {
            WeaponRow weapon = weaponsGrid.GetSelectedTag() as WeaponRow;
            if (null == weapon)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_WEAPON_SELECTED);
            }
            return weapon;
        }

        private void DrawPodInformation(Aircraft aircraft)
        {
            pod1Text.Text = "";
            pod2Text.Text = "";

            if (0 < aircraft.WeaponPods.Count)
            {
                DrawPodInformation(pod1Text, aircraft.WeaponPods[0], 1);
            }

            if (1 < aircraft.WeaponPods.Count)
            {
                DrawPodInformation(pod2Text, aircraft.WeaponPods[1], 2);
            }
        }

        private static void DrawPodInformation(Label textControl, WeaponPod pod, int podId)
        {
            StringBuilder info = new StringBuilder(Util.StringFormat(Strings.SCREEN_EQUIP_CRAFT_POD_NAME, podId));
            info.Append(Util.Linefeed);
            info.Append(WeaponPod.PodInformationString(pod));
            textControl.Text = info.ToString();
        }

        private class WeaponRow
        {
            public WeaponRow(Item item, OutpostInventory inventory)
            {
                this.item = item;
                this.inventory = inventory;
            }

            public bool EquipPod(Aircraft aircraft, int podId)
            {
                if (null == aircraft.WeaponPods[podId - 1])
                {
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

            public int OnHand { get { return inventory.NumberInInventory(item.ItemInfo); } }

            public string ClipSize { get { return Weapon.ClipSizeString(); } }

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

            public String Name { get { return item.Name; } }

            private CraftWeaponItemInfo Weapon { get { return item.ItemInfo as CraftWeaponItemInfo; } }

            #region Fields

            private Item item;

            private OutpostInventory inventory;

            #endregion Fields
        }

        #region Fields

        private Outpost SelectedOutpost { get { return Xenocide.GameState.GeoData.Outposts[selectedOutpostIndex]; } }

        private int selectedOutpostIndex;

        #endregion Fields
    }
}
