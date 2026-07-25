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

using Gum.Forms;
using Gum.Forms.Controls;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.Utils;

using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// This is the screen that allows user to buy items for a base
    /// </summary>
    /// <remarks>
    /// ARCHITECTURE: GUI layer only — all game logic is delegated to the nested Controller
    /// class (in Purchase/PurchaseScreenController.cs). This screen manages the item grid
    /// and handles purchase interactions.
    /// </remarks>
    public partial class PurchaseScreen : GumScreen
    {
        private ScreenLayout layout;
        private ContentArea content;

        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="selectedBaseIndex">Index to outpost purchases will be sent to</param>
        public PurchaseScreen(int selectedBaseIndex)
            : base("PurchaseScreen")
        {
            this.selectedBaseIndex = selectedBaseIndex;
            this.controller = new PurchaseController(SelectedBase);
        }

        protected override bool HasGumxLayout => false;

        #region Create the Gum controls

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateGumControls()
        {
            layout = new ScreenLayout();
            layout.AddToRoot();
            content = new ContentArea(layout.ContentPanel);

            layout.AddButton(XenocideResourceManager.Get("BUTTON_BUY_MORE"), OnBuyMoreButton);
            layout.AddButton(XenocideResourceManager.Get("BUTTON_BUY_LESS"), OnBuyLessButton);
            layout.AddButton(XenocideResourceManager.Get("BUTTON_CONFIRM"), OnConfirmButton);
            layout.AddButton(XenocideResourceManager.Get("BUTTON_CANCEL"), OnCancelButton);

            fundsText = new Label();
            fundsText.Text = Util.StringFormat(Strings.SCREEN_PURCHASE_FUNDS,
                Xenocide.GameState.GeoData.XCorp.Bank.CurrentBalance);
            content.Panel.AddChild(fundsText);

            totalCostText = new Label();
            content.Panel.AddChild(totalCostText);
            UpdateTotalCost();

            InitializeGrid();
            content.AddGrid(grid);
            PopulateGrid();
        }

        private Label fundsText;
        private Label totalCostText;
        private GridPanel grid;

        /// <summary>
        /// Create GridPanel which holds items available for purchase
        /// </summary>
        private void InitializeGrid()
        {
            grid = new GridPanel();
            grid.AddColumn(Strings.SCREEN_PURCHASE_COLUMN_ITEM, (int)(0.58f * 800));
            grid.AddColumn(Strings.SCREEN_PURCHASE_COLUMN_QUANTITY_IN_BASE, (int)(0.12f * 800));
            grid.AddColumn(Strings.SCREEN_PURCHASE_COLUMN_COST_PER_UNIT, (int)(0.13f * 800));
            grid.AddColumn(Strings.SCREEN_PURCHASE_COLUMN_QUANTITY, (int)(0.12f * 800));
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
            return PurchaseController.IsAvailableForPurchase(item);
        }

        /// <summary>
        /// Add a row to the grid
        /// </summary>
        /// <param name="item">item to put on grid</param>
        private void AddRowToGrid(ItemInfo item)
        {
            int itemIndex = Xenocide.StaticTables.ItemList.IndexOf(item.Id);
            grid.AddRow(itemIndex, item.Name,
                Util.ToString(controller.GetItemCount(item)),
                Util.ToString(item.BuyPrice),
                "0");

            ShoppingList[itemIndex] = 0;
        }

        #endregion Create the Gum controls

        #region event handlers

        /// <summary>Handle user clicking on the "Buy More" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnBuyMoreButton(object sender, EventArgs e)
        {
            int? tag = GetSelectedTag();
            if (tag.HasValue)
            {
                int itemListIndex = tag.Value;
                ++ShoppingList[itemListIndex];

                int totalCost = PurchaseController.CalculateTotalCost(ShoppingList);
                if (!controller.CanFitAll(ShoppingList) ||
                    !PurchaseController.CanAfford(totalCost))
                {
                    --ShoppingList[itemListIndex];
                }

                UpdateDetails(itemListIndex);
            }
        }

        /// <summary>Handle user clicking on the "Buy Less" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnBuyLessButton(object sender, EventArgs e)
        {
            int? tag = GetSelectedTag();
            if (tag.HasValue)
            {
                int itemListIndex = tag.Value;
                if (0 < ShoppingList[itemListIndex])
                {
                    --ShoppingList[itemListIndex];

                    UpdateDetails(itemListIndex);
                }
            }
        }

        /// <summary>Handle user clicking on the "Confirm" button</summary>
        /// <remarks>That is, buy all the items the user has selected</remarks>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnConfirmButton(object sender, EventArgs e)
        {
            controller.ExecutePurchase(ShoppingList);
            GoToBasesScreen();
        }

        /// <summary>React to user pressing the Cancel button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnCancelButton(object sender, EventArgs e)
        {
            GoToBasesScreen();
        }

        #endregion event handlers

        private int? GetSelectedTag()
        {
            if (grid.SelectedRow == null)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_PURCHASE_SELECTED);
                return null;
            }
            return (int)grid.GetSelectedTag();
        }

        /// <summary>
        /// Populate the Total Cost field on the dialog
        /// </summary>
        private void UpdateTotalCost()
        {
            totalCostText.Text = Util.StringFormat(Strings.SCREEN_PURCHASE_TOTAL_COST,
                PurchaseController.CalculateTotalCost(ShoppingList));
        }

        /// <summary>
        /// Update the screen to reflect the latest changes
        /// </summary>
        /// <param name="itemListIndex">Index into StaticData.ItemList for data on item</param>
        private void UpdateDetails(int itemListIndex)
        {
            UpdateTotalCost();

            int row = grid.GetRowIndexByTag(itemListIndex);
            grid.SetCell(row, 3, Util.StringFormat("{0}", ShoppingList[itemListIndex]));
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
        /// Controller handling game logic for purchasing.
        /// </summary>
        private PurchaseController controller;

        /// <summary>
        /// The outpost purchases will be sent to
        /// </summary>
        private Outpost SelectedBase { get { return Xenocide.GameState.GeoData.Outposts[selectedBaseIndex]; } }

        private int selectedBaseIndex;

        private Dictionary<int, int> ShoppingList = new Dictionary<int, int>();

        #endregion Fields
    }
}
