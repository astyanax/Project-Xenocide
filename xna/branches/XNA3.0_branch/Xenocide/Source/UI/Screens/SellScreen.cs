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
* @file SellScreen.cs
* @date Created: 2007/07/01
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
    /// This is the screen that allows user to sell items stored in an outpost
    /// </summary>
    public class SellScreen : Screen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="selectedOutpostIndex">Index to outpost items will be taken from</param>
        public SellScreen(int selectedOutpostIndex)
            : base("SellScreen")
        {
            this.selectedOutpostIndex = selectedOutpostIndex;
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
            fundsText.Text = Util.StringFormat(Strings.SCREEN_SELL_FUNDS,
                Xenocide.GameState.GeoData.XCorp.Bank.CurrentBalance);

            // add text giving the running total of the items selected to sell
            totalValueText = GuiBuilder.CreateText(CeguiId + "_totalCostText");
            AddWidget(totalValueText, 0.35f, 0.06f, 0.2275f, 0.04125f);
            UpdateTotalValue();

            // The grid of items available for purchase
            InitializeGrid();
            PopulateGrid();

            // other buttons
            sellMoreButton = AddButton("BUTTON_SELL_MORE", 0.7475f, 0.80f, 0.2275f, 0.04125f, "PlanetView\\zoomin.ogg");
            sellLessButton = AddButton("BUTTON_SELL_LESS", 0.7475f, 0.85f, 0.2275f, 0.04125f, "PlanetView\\zoomout.ogg");
            confirmButton = AddButton("BUTTON_CONFIRM", 0.7475f, 0.90f, 0.2275f, 0.04125f);
            cancelButton = AddButton("BUTTON_CANCEL", 0.7475f, 0.95f, 0.2275f, 0.04125f);

            sellMoreButton.Clicked += new CeGui.GuiEventHandler(OnSellMoreButton);
            sellLessButton.Clicked += new CeGui.GuiEventHandler(OnSellLessButton);
            confirmButton.Clicked += new CeGui.GuiEventHandler(OnConfirmButton);
            cancelButton.Clicked += new CeGui.GuiEventHandler(OnCancelButton);
        }

        private CeGui.Widgets.StaticText fundsText;
        private CeGui.Widgets.StaticText totalValueText;
        private CeGui.Widgets.MultiColumnList grid;
        private CeGui.Widgets.PushButton sellMoreButton;
        private CeGui.Widgets.PushButton sellLessButton;
        private CeGui.Widgets.PushButton confirmButton;
        private CeGui.Widgets.PushButton cancelButton;

        /// <summary>
        /// Create MultiColumnListBox which holds items available to sell
        /// </summary>
        private void InitializeGrid()
        {
            grid = GuiBuilder.CreateGrid("sellGrid");
            AddWidget(grid, 0.01f, 0.13f, 0.70f, 0.86f);
            grid.AddColumn(Strings.SCREEN_SELL_COLUMN_ITEM, grid.ColumnCount, 0.58f);
            grid.AddColumn(Strings.SCREEN_SELL_COLUMN_QUANTITY_IN_BASE, grid.ColumnCount, 0.12f);
            grid.AddColumn(Strings.SCREEN_SELL_COLUMN_VALUE_PER_UNIT, grid.ColumnCount, 0.13f);
            grid.AddColumn(Strings.SCREEN_SELL_COLUMN_QUANTITY, grid.ColumnCount, 0.12f);
        }

        /// <summary>
        /// Put the list of items available to sell into the grid
        /// </summary>
        private void PopulateGrid()
        {
            foreach (Item i in SelectedOutpost.Inventory.ListContents())
            {
                if (i.CanRemoveFromOutpost)
                {
                    AddRowToGrid(new TransactionLineItem(i, SelectedOutpost.Inventory));
                }
            }
        }

        /// <summary>
        /// Add a row to the grid
        /// </summary>
        /// <param name="lineItem">details to put on grid</param>
        private void AddRowToGrid(TransactionLineItem lineItem)
        {
            // add item to grid
            CeGui.ListboxTextItem listboxItem = Util.CreateListboxItem(lineItem.Name);
            int rowNum = grid.AddRow(listboxItem, 0);
            listboxItem.ID = rowNum;

            Util.AddNumericElementToGrid(grid, 1, rowNum, lineItem.SourceCount);
            Util.AddNumericElementToGrid(grid, 2, rowNum, lineItem.SellPrice);
            Util.AddNumericElementToGrid(grid, 3, rowNum, lineItem.NumMoving);

            // and record number of items of this type user is selling
            SalesList[rowNum] = lineItem;
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>Handle user clicking on the "Sell More" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnSellMoreButton(object sender, GuiEventArgs e)
        {
            CeGui.Widgets.ListboxItem selectedItem = GetSelectedItem();
            if (null != selectedItem)
            {
                // update count of items
                TransactionLineItem lineItem = SalesList[selectedItem.ID];
                if (lineItem.NumMoving < lineItem.MaxMovable)
                {
                    ++lineItem.NumMoving;

                    // update display on screen
                    UpdateDetails(selectedItem, lineItem);
                }
            }
        }

        /// <summary>Handle user clicking on the "Sell Less" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnSellLessButton(object sender, GuiEventArgs e)
        {
            CeGui.Widgets.ListboxItem selectedItem = GetSelectedItem();
            if (null != selectedItem)
            {
                TransactionLineItem lineItem = SalesList[selectedItem.ID];
                if (0 < lineItem.NumMoving)
                {
                    --lineItem.NumMoving;

                    // update display on screen
                    UpdateDetails(selectedItem, lineItem);
                }
            }
        }

        /// <summary>Handle user clicking on the "Confirm" button</summary>
        /// <remarks>That is, buy all the items the user has selected</remarks>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnConfirmButton(object sender, GuiEventArgs e)
        {
            // Get the money from selling the items
            Xenocide.GameState.GeoData.XCorp.Bank.Credit(CalculateTotalValue());

            // and now get rid of the items
            foreach (TransactionLineItem lineItem in SalesList.Values)
            {
                lineItem.RemoveItems(SelectedOutpost.Inventory, null);
            }

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
                Util.ShowMessageBox(Strings.MSGBOX_NO_SALE_SELECTED);
            }
            return selectedItem;
        }

        /// <summary>
        /// Populate the Total Value field on the dialog
        /// </summary>
        private void UpdateTotalValue()
        {
            totalValueText.Text = Util.StringFormat(Strings.SCREEN_SELL_TOTAL_VALUE, CalculateTotalValue());
        }

        /// <summary>
        /// Update the screen to reflect the latest changes
        /// </summary>
        /// <param name="selectedItem">row in gird that is selected</param>
        /// <param name="lineItem">LineItem with number of items of this type being sold</param>
        private void UpdateDetails(CeGui.Widgets.ListboxItem selectedItem, TransactionLineItem lineItem)
        {
            UpdateTotalValue();

            // update quantity column of row in grid
            int row = grid.GetRowIndexOfItem(selectedItem);
            CeGui.Widgets.GridReference position = new CeGui.Widgets.GridReference(row, 3);
            grid.GetItemAtGridReference(position).Text = Util.ToString(lineItem.NumMoving);
        }

        /// <summary>
        /// Figure out what the total value of the sales is going to be
        /// </summary>
        /// <returns>total value</returns>
        private int CalculateTotalValue()
        {
            int value = 0;
            foreach (TransactionLineItem lineItem in SalesList.Values)
            {
                value += lineItem.Value;
            }
            return value;
        }

        /// <summary>
        /// Close this screen and go back to the Bases Screen
        /// </summary>
        private void GoToBasesScreen()
        {
            ScreenManager.ScheduleScreen(new BasesScreen(selectedOutpostIndex));
        }

        #region Fields

        /// <summary>
        /// The outpost items will be taken from
        /// </summary>
        private Outpost SelectedOutpost { get { return Xenocide.GameState.GeoData.Outposts[selectedOutpostIndex]; } }

        // index specifying the outpost that items will be taken from
        private int selectedOutpostIndex;

        /// <summary>
        /// The list of items we're selling
        /// Format is row in grid, details
        /// </summary>
        private Dictionary<int, TransactionLineItem> SalesList = new Dictionary<int, TransactionLineItem>();

        #endregion Fields
    }
}
