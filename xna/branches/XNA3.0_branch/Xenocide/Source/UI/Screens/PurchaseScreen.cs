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
* @file PurchaseScreen.cs
* @date Created: 2007/06/25
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
using ProjectXenocide.Model.StaticData.Items;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// This is the screen that allows user to buy items for a base
    /// </summary>
    public class PurchaseScreen : Screen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="selectedBaseIndex">Index to outpost purchases will be sent to</param>
        public PurchaseScreen(int selectedBaseIndex)
            : base("PurchaseScreen")
        {
            this.selectedBaseIndex = selectedBaseIndex;
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // add text giving the available funds
            fundsText = GuiBuilder.CreateText(CeguiId + "_fundsText");
            AddWidget(fundsText, 0.01f, 0.06f, 0.2275f, 0.04125f);
            fundsText.Text = Util.StringFormat(Strings.SCREEN_PURCHASE_FUNDS,
                Xenocide.GameState.GeoData.XCorp.Bank.CurrentBalance);

            // add text giving the running total of the items selected for purchase
            totalCostText = GuiBuilder.CreateText(CeguiId + "_totalCostText");
            AddWidget(totalCostText, 0.35f, 0.06f, 0.2275f, 0.04125f);
            UpdateTotalCost();

            // The gird of items available for purchase
            InitializeGrid();
            PopulateGrid();

            // other buttons
            buyMoreButton = AddButton("BUTTON_BUY_MORE", 0.7475f, 0.80f, 0.2275f, 0.04125f, "PlanetView\\zoomin.ogg");
            buyLessButton = AddButton("BUTTON_BUY_LESS", 0.7475f, 0.85f, 0.2275f, 0.04125f, "PlanetView\\zoomout.ogg");
            confirmButton = AddButton("BUTTON_CONFIRM", 0.7475f, 0.90f, 0.2275f, 0.04125f);
            cancelButton = AddButton("BUTTON_CANCEL", 0.7475f, 0.95f, 0.2275f, 0.04125f);

            buyMoreButton.Clicked += new CeGui.GuiEventHandler(OnBuyMoreButton);
            buyLessButton.Clicked += new CeGui.GuiEventHandler(OnBuyLessButton);
            confirmButton.Clicked += new CeGui.GuiEventHandler(OnConfirmButton);
            cancelButton.Clicked += new CeGui.GuiEventHandler(OnCancelButton);
        }

        private CeGui.Widgets.StaticText fundsText;
        private CeGui.Widgets.StaticText totalCostText;
        private CeGui.Widgets.MultiColumnList grid;
        private CeGui.Widgets.PushButton buyMoreButton;
        private CeGui.Widgets.PushButton buyLessButton;
        private CeGui.Widgets.PushButton confirmButton;
        private CeGui.Widgets.PushButton cancelButton;

        /// <summary>
        /// Create MultiColumnListBox which holds items available for purchase
        /// </summary>
        private void InitializeGrid()
        {
            grid = GuiBuilder.CreateGrid("purchaseGrid");
            AddWidget(grid, 0.01f, 0.13f, 0.70f, 0.86f);
            grid.AddColumn(Strings.SCREEN_PURCHASE_COLUMN_ITEM, grid.ColumnCount, 0.58f);
            grid.AddColumn(Strings.SCREEN_PURCHASE_COLUMN_QUANTITY_IN_BASE, grid.ColumnCount, 0.12f);
            grid.AddColumn(Strings.SCREEN_PURCHASE_COLUMN_COST_PER_UNIT, grid.ColumnCount, 0.13f);
            grid.AddColumn(Strings.SCREEN_PURCHASE_COLUMN_QUANTITY, grid.ColumnCount, 0.12f);
        }

        /// <summary>
        /// Put the list of items available for purchase into the grid
        /// </summary>
        private void PopulateGrid()
        {
            foreach (ItemInfo i in Xenocide.StaticTables.ItemList)
            {
                if (AvailableForPurchase(i))
                {
                    AddRowToGrid(i);
                }
            }
        }

        /// <summary>
        /// Can items of this type be purchased?
        /// </summary>
        /// <param name="item">type of item</param>
        /// <returns>true if items can be purchased</returns>
        private static bool AvailableForPurchase(ItemInfo item)
        {
            // Note, with initial tree, all buyable items don't need researching.
            // however this might not be true with later research trees
            return item.CanPurchase && Xenocide.GameState.GeoData.XCorp.TechManager.IsAvailable(item.Id);
        }

        /// <summary>
        /// Add a row to the grid
        /// </summary>
        /// <param name="item">item to put on grid</param>
        private void AddRowToGrid(ItemInfo item)
        {
            // add item to grid
            CeGui.ListboxTextItem listboxItem = Util.CreateListboxItem(item.Name);
            listboxItem.ID = Xenocide.StaticTables.ItemList.IndexOf(item.Id);
            int rowNum = grid.AddRow(listboxItem, 0);
            Util.AddNumericElementToGrid(grid, 1, rowNum, SelectedBase.Inventory.NumberInInventory(item));
            Util.AddNumericElementToGrid(grid, 2, rowNum, item.BuyPrice);
            Util.AddNumericElementToGrid(grid, 3, rowNum, 0);

            // and record number of items of this type user is purchasing
            ShoppingList[listboxItem.ID] = 0;
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>Handle user clicking on the "Buy More" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnBuyMoreButton(object sender, GuiEventArgs e)
        {
            CeGui.Widgets.ListboxItem selectedItem = GetSelectedItem();
            if (null != selectedItem)
            {
                // update count of items
                int itemListIndex = selectedItem.ID;
                ++ShoppingList[itemListIndex];

                // if shopping list is no longer valid, revert
                if (!CanFit() ||
                    !Xenocide.GameState.GeoData.XCorp.Bank.CanAfford(CalculateTotalCost())
                )
                {
                    --ShoppingList[itemListIndex];
                }

                // update display on screen
                UpdateDetails(selectedItem, itemListIndex);
            }
        }

        /// <summary>Handle user clicking on the "Buy Less" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnBuyLessButton(object sender, GuiEventArgs e)
        {
            CeGui.Widgets.ListboxItem selectedItem = GetSelectedItem();
            if (null != selectedItem)
            {
                // update count of items
                int itemListIndex = selectedItem.ID;
                if (0 < ShoppingList[itemListIndex])
                {
                    --ShoppingList[itemListIndex];

                    // update display on screen
                    UpdateDetails(selectedItem, itemListIndex);
                }
            }
        }

        /// <summary>Handle user clicking on the "Confirm" button</summary>
        /// <remarks>That is, buy all the items the user has selected</remarks>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnConfirmButton(object sender, GuiEventArgs e)
        {
            // Pay for the items
            Xenocide.GameState.GeoData.XCorp.Bank.Debit(CalculateTotalCost());

            Shipment shipment = new Shipment(SelectedBase, Shipment.CalcEta());
            foreach (KeyValuePair<int, int> kvp in ShoppingList)
            {
                ItemInfo item = Xenocide.StaticTables.ItemList[kvp.Key];
                for (int i = 0; i < kvp.Value; ++i)
                {
                    shipment.Add(item.Manufacture());
                }
            }
            shipment.Ship();

            GoToBasesScreen();
        }

        /// <summary>React to user pressing the Cancel button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnCancelButton(object sender, CeGui.GuiEventArgs e)
        {
            GoToBasesScreen();
        }

        #endregion event handlers

        // Get currently selected item from Grid.  Give error message if nothing selected
        private CeGui.Widgets.ListboxItem GetSelectedItem()
        {
            CeGui.Widgets.ListboxItem selectedItem = grid.GetFirstSelectedItem();
            if (null == selectedItem)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_PURCHASE_SELECTED);
            }
            return selectedItem;
        }

        /// <summary>
        /// Populate the Total Cost field on the dialog
        /// </summary>
        private void UpdateTotalCost()
        {
            totalCostText.Text = Util.StringFormat(Strings.SCREEN_PURCHASE_TOTAL_COST, CalculateTotalCost());
        }

        /// <summary>
        /// Update the screen to reflect the latest changes
        /// </summary>
        /// <param name="selectedItem">row in gird that is selected</param>
        /// <param name="itemListIndex">Index into StaticData.ItemList for data on item</param>
        private void UpdateDetails(CeGui.Widgets.ListboxItem selectedItem, int itemListIndex)
        {
            UpdateTotalCost();

            // update quantity column of row in grid
            int row = grid.GetRowIndexOfItem(selectedItem);
            CeGui.Widgets.GridReference position = new CeGui.Widgets.GridReference(row, 3);
            grid.GetItemAtGridReference(position).Text = Util.StringFormat("{0}", ShoppingList[itemListIndex]);
        }

        /// <summary>
        /// Figure out what the total cost of the purchases is going to be
        /// </summary>
        /// <returns>total cost</returns>
        private int CalculateTotalCost()
        {
            int cost = 0;
            foreach (KeyValuePair<int, int> kvp in ShoppingList)
            {
                cost += (Xenocide.StaticTables.ItemList[kvp.Key].BuyPrice * kvp.Value);
            }
            return cost;
        }

        /// <summary>
        /// Check if user fit everything on the shopping list into the outpost's inventory
        /// <remarks>
        /// 1. Gives a warning dialog if user has insufficent funds
        /// 2. It's not very efficient, but it runs in response to user input, so doesn't need to be
        /// </remarks>
        /// </summary>
        /// <returns>true if user can fit everything on the list</returns>
        private bool CanFit()
        {
            bool canFit = true;

            // first put everything into the base
            foreach (KeyValuePair<int, int> kvp in ShoppingList)
            {
                ItemInfo item = Xenocide.StaticTables.ItemList[kvp.Key];
                for (int i = 0; i < kvp.Value; ++i)
                {
                    if (!SelectedBase.Inventory.CanFit(item))
                    {
                        canFit = false;
                    }
                    SelectedBase.Inventory.AllocateSpace(item);
                }
            }

            // now take them all out again
            foreach (KeyValuePair<int, int> kvp in ShoppingList)
            {
                ItemInfo item = Xenocide.StaticTables.ItemList[kvp.Key];
                for (int i = 0; i < kvp.Value; ++i)
                {
                    SelectedBase.Inventory.ReleaseSpace(item);
                }
            }

            if (!canFit)
            {
                Util.ShowMessageBox(Strings.MSGBOX_CANT_FIT);
            }
            return canFit;
        }

        /// <summary>
        /// Close this screen and go back to the Bases Screen
        /// </summary>
        private void GoToBasesScreen()
        {
            ScreenManager.ScheduleScreen(new BasesScreen(selectedBaseIndex));
        }

        #region Fields

        /// <summary>
        /// The outpost purchases will be sent to
        /// </summary>
        private Outpost SelectedBase { get { return Xenocide.GameState.GeoData.Outposts[selectedBaseIndex]; } }

        // index specifying the outpost that purchases will be sent to
        private int selectedBaseIndex;

        // The list of items we're buying
        // Format is ItemList index, count
        private Dictionary<int, int> ShoppingList = new Dictionary<int, int>();

        #endregion Fields
    }
}
