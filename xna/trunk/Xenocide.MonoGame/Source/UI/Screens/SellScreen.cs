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
    /// This is the screen that allows user to sell items stored in an outpost
    /// </summary>
    public class SellScreen : GumScreen
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

        #region Create the Gum controls

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateGumControls()
        {
            if (GumRoot != null)
            {
                WireButton("sellMoreButton", OnSellMoreButton);
                WireButton("sellLessButton", OnSellLessButton);
                WireButton("confirmButton", OnConfirmButton);
                WireButton("cancelButton", OnCancelButton);

                fundsText = new Label();
                AddChild(fundsText);
                fundsText.Text = Util.StringFormat(Strings.SCREEN_SELL_FUNDS,
                    Xenocide.GameState.GeoData.XCorp.Bank.CurrentBalance);

                totalValueText = new Label();
                AddChild(totalValueText);
                UpdateTotalValue();

                InitializeGrid();
                PopulateGrid();
                return;
            }

            // add text giving the available funds
            fundsText = new Label();
            RootContainer.AddChild(fundsText);
            fundsText.Text = Util.StringFormat(Strings.SCREEN_SELL_FUNDS,
                Xenocide.GameState.GeoData.XCorp.Bank.CurrentBalance);

            // add text giving the running total of the items selected to sell
            totalValueText = new Label();
            RootContainer.AddChild(totalValueText);
            UpdateTotalValue();

            // The grid of items available for purchase
            InitializeGrid();
            PopulateGrid();

            // other buttons
            sellMoreButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_SELL_MORE") };
            RootContainer.AddChild(sellMoreButton);
            sellLessButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_SELL_LESS") };
            RootContainer.AddChild(sellLessButton);
            confirmButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_CONFIRM") };
            RootContainer.AddChild(confirmButton);
            cancelButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_CANCEL") };
            RootContainer.AddChild(cancelButton);

            sellMoreButton.Click += OnSellMoreButton;
            sellLessButton.Click += OnSellLessButton;
            confirmButton.Click += OnConfirmButton;
            cancelButton.Click += OnCancelButton;
        }

        private Label fundsText;
        private Label totalValueText;
        private GridPanel grid;
        private Button sellMoreButton;
        private Button sellLessButton;
        private Button confirmButton;
        private Button cancelButton;

        /// <summary>
        /// Create GridPanel which holds items available to sell
        /// </summary>
        private void InitializeGrid()
        {
            grid = new GridPanel();
            AddChild(grid.Visual);
            grid.AddColumn(Strings.SCREEN_SELL_COLUMN_ITEM, (int)(0.58f * 800));
            grid.AddColumn(Strings.SCREEN_SELL_COLUMN_QUANTITY_IN_BASE, (int)(0.12f * 800));
            grid.AddColumn(Strings.SCREEN_SELL_COLUMN_VALUE_PER_UNIT, (int)(0.13f * 800));
            grid.AddColumn(Strings.SCREEN_SELL_COLUMN_QUANTITY, (int)(0.12f * 800));
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
            int rowNum = grid.RowCount;
            grid.AddRow(rowNum, lineItem.Name,
                Util.ToString(lineItem.SourceCount),
                Util.ToString(lineItem.SellPrice),
                Util.ToString(lineItem.NumMoving));

            // and record number of items of this type user is selling
            SalesList[rowNum] = lineItem;
        }

        #endregion Create the Gum controls

        #region event handlers

        /// <summary>Handle user clicking on the "Sell More" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnSellMoreButton(object sender, EventArgs e)
        {
            int? tag = GetSelectedTag();
            if (tag.HasValue)
            {
                // update count of items
                int rowNum = tag.Value;
                TransactionLineItem lineItem = SalesList[rowNum];
                if (lineItem.NumMoving < lineItem.MaxMovable)
                {
                    ++lineItem.NumMoving;

                    // update display on screen
                    UpdateDetails(rowNum, lineItem);
                }
            }
        }

        /// <summary>Handle user clicking on the "Sell Less" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnSellLessButton(object sender, EventArgs e)
        {
            int? tag = GetSelectedTag();
            if (tag.HasValue)
            {
                int rowNum = tag.Value;
                TransactionLineItem lineItem = SalesList[rowNum];
                if (0 < lineItem.NumMoving)
                {
                    --lineItem.NumMoving;

                    // update display on screen
                    UpdateDetails(rowNum, lineItem);
                }
            }
        }

        /// <summary>Handle user clicking on the "Confirm" button</summary>
        /// <remarks>That is, buy all the items the user has selected</remarks>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnConfirmButton(object sender, EventArgs e)
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
        private void OnCancelButton(object sender, EventArgs e)
        {
            GoToBasesScreen();
        }

        #endregion event handlers

        // Get currently selected item from Grid.  Give error message if nothing selected
        private int? GetSelectedTag()
        {
            if (grid.SelectedRow == null)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_SALE_SELECTED);
                return null;
            }
            return (int)grid.GetSelectedTag();
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
        /// <param name="rowNum">row in grid that is selected</param>
        /// <param name="lineItem">LineItem with number of items of this type being sold</param>
        private void UpdateDetails(int rowNum, TransactionLineItem lineItem)
        {
            UpdateTotalValue();

            // update quantity column of row in grid
            int row = grid.GetRowIndexByTag(rowNum);
            grid.SetCell(row, 3, Util.ToString(lineItem.NumMoving));
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
