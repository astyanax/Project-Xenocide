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
* @file MakeTransferScreen.cs
* @date Created: 2007/09/24
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
    /// This is the screen that allows user to move items between outposts
    /// </summary>
    public class MakeTransferScreen : Screen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="sourceOutpostIndex">Index to outpost items will be taken from</param>
        /// <param name="destinationOutpostIndex">Index to outpost items will be sent to</param>
        public MakeTransferScreen(int sourceOutpostIndex, int destinationOutpostIndex)
            : base("MakeTransferScreen")
        {
            this.sourceOutpostIndex = sourceOutpostIndex;
            this.destinationOutpostIndex = destinationOutpostIndex;
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // add text naming source outpost
            sourceText = GuiBuilder.CreateText(CeguiId + "_sourceText");
            AddWidget(sourceText, 0.01f, 0.06f, 0.2275f, 0.04125f);
            sourceText.Text = Util.StringFormat(Strings.SCREEN_TRANSFER_SOURCE, SourceOutpost.Name);

            // add text giving the running total cost of the transfer
            totalCostText = GuiBuilder.CreateText(CeguiId + "_totalCostText");
            AddWidget(totalCostText, 0.35f, 0.06f, 0.2275f, 0.04125f);
            UpdateTotalCost();

            // combo box to allow user to pick destination outpost for items
            outpostsListComboBox = GuiBuilder.CreateComboBox("outpostsListComboBox");
            AddWidget(outpostsListComboBox, 0.7475f, 0.06f, 0.2275f, 0.40f);
            Misc.PopulateHumanBasesList(outpostsListComboBox, destinationOutpostIndex);
            outpostsListComboBox.ListSelectionAccepted += new WindowEventHandler(OnOutpostSelectionChanged);

            // The gird of items available for purchase
            InitializeGrid();
            PopulateGrid();

            // other buttons
            moveMoreButton = AddButton("BUTTON_MOVE_MORE", 0.7475f, 0.80f, 0.2275f, 0.04125f, "PlanetView\\zoomin.ogg");
            moveLessButton = AddButton("BUTTON_MOVE_LESS", 0.7475f, 0.85f, 0.2275f, 0.04125f, "PlanetView\\zoomout.ogg");
            confirmButton = AddButton("BUTTON_CONFIRM", 0.7475f, 0.90f, 0.2275f, 0.04125f);
            cancelButton = AddButton("BUTTON_CANCEL", 0.7475f, 0.95f, 0.2275f, 0.04125f);

            moveMoreButton.Clicked += new CeGui.GuiEventHandler(OnMoveMoreButton);
            moveLessButton.Clicked += new CeGui.GuiEventHandler(OnMoveLessButton);
            confirmButton.Clicked += new CeGui.GuiEventHandler(OnConfirmButton);
            cancelButton.Clicked += new CeGui.GuiEventHandler(OnCancelButton);
        }

        private CeGui.Widgets.StaticText sourceText;
        private CeGui.Widgets.StaticText totalCostText;
        private CeGui.Widgets.ComboBox outpostsListComboBox;
        private CeGui.Widgets.MultiColumnList grid;
        private CeGui.Widgets.PushButton moveMoreButton;
        private CeGui.Widgets.PushButton moveLessButton;
        private CeGui.Widgets.PushButton confirmButton;
        private CeGui.Widgets.PushButton cancelButton;

        /// <summary>
        /// Create MultiColumnListBox which holds items that can be transfered
        /// </summary>
        private void InitializeGrid()
        {
            grid = GuiBuilder.CreateGrid("transferGrid");
            AddWidget(grid, 0.01f, 0.13f, 0.70f, 0.86f);
            grid.AddColumn(Strings.SCREEN_TRANSFER_COLUMN_ITEM, grid.ColumnCount, 0.58f);
            grid.AddColumn(Strings.SCREEN_TRANSFER_COLUMN_QUANTITY_IN_BASE, grid.ColumnCount, 0.12f);
            grid.AddColumn(Strings.SCREEN_TRANSFER_COLUMN_QUANTITY_DESTINAION, grid.ColumnCount, 0.15f);
            grid.AddColumn(Strings.SCREEN_TRANSFER_COLUMN_QUANTITY, grid.ColumnCount, 0.12f);
        }

        /// <summary>
        /// Put the list of items that can be transfered into the grid
        /// </summary>
        private void PopulateGrid()
        {
            foreach (Item i in SourceOutpost.Inventory.ListContents())
            {
                if (i.CanRemoveFromOutpost)
                {
                    AddRowToGrid(new TransactionLineItem(i, SourceOutpost.Inventory, DestinationOutpost.Inventory));
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
            Util.AddNumericElementToGrid(grid, 2, rowNum, lineItem.DestinationCount);
            Util.AddNumericElementToGrid(grid, 3, rowNum, lineItem.NumMoving);

            // and record number of items of this type user has
            TransferList[rowNum] = lineItem;
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>Handle user clicking on the "Transfer More" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnMoveMoreButton(object sender, GuiEventArgs e)
        {
            CeGui.Widgets.ListboxItem selectedItem = GetSelectedItem();
            if (null != selectedItem)
            {
                // update count of items
                TransactionLineItem lineItem = TransferList[selectedItem.ID];
                if (lineItem.NumMoving < lineItem.MaxMovable)
                {
                    ++lineItem.NumMoving;

                    // check that this item doesn't put us over the limit
                    if (!CanManageTransfer())
                    {
                        --lineItem.NumMoving;
                    }

                    // update display on screen
                    UpdateDetails(selectedItem, lineItem);
                }
            }
        }

        /// <summary>Handle user clicking on the "Transfer Less" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnMoveLessButton(object sender, GuiEventArgs e)
        {
            CeGui.Widgets.ListboxItem selectedItem = GetSelectedItem();
            if (null != selectedItem)
            {
                TransactionLineItem lineItem = TransferList[selectedItem.ID];
                if (0 < lineItem.NumMoving)
                {
                    --lineItem.NumMoving;

                    // update display on screen
                    UpdateDetails(selectedItem, lineItem);
                }
            }
        }

        /// <summary>Handle user clicking on the "Confirm" button</summary>
        /// <remarks>That is, ship the items the user has selected to the destination outpost</remarks>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnConfirmButton(object sender, GuiEventArgs e)
        {
            // Get the money from selling the items
            Xenocide.GameState.GeoData.XCorp.Bank.Debit(CalculateTotalCost());

            // and ship items to destination
            Shipment shipment = new Shipment(DestinationOutpost, Shipment.CalcEta());
            foreach (TransactionLineItem lineItem in TransferList.Values)
            {
                lineItem.RemoveItems(SourceOutpost.Inventory, shipment);
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

        /// <summary>user has selected an entry in the destination outpost combo box</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnOutpostSelectionChanged(object sender, WindowEventArgs e)
        {
            CeGui.Widgets.ListboxItem item = outpostsListComboBox.SelectedItem;
            if (item != null)
            {
                ChangeDestinationOutpost(outpostsListComboBox.GetItemIndex(item));
            }
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
        private void UpdateTotalCost()
        {
            totalCostText.Text = Util.StringFormat(Strings.SCREEN_TRANSFER_TOTAL_COST, CalculateTotalCost());
        }

        /// <summary>
        /// Update the screen to reflect the latest changes
        /// </summary>
        /// <param name="selectedItem">row in gird that is selected</param>
        /// <param name="lineItem">LineItem with number of items of this type being sold</param>
        private void UpdateDetails(CeGui.Widgets.ListboxItem selectedItem, TransactionLineItem lineItem)
        {
            UpdateTotalCost();

            // update quantity column of row in grid
            int row = grid.GetRowIndexOfItem(selectedItem);
            CeGui.Widgets.GridReference position = new CeGui.Widgets.GridReference(row, 3);
            grid.GetItemAtGridReference(position).Text = Util.ToString(lineItem.NumMoving);
        }

        /// <summary>
        /// Figure out what the total cost to ship all the items will be
        /// </summary>
        /// <returns>total cost</returns>
        private int CalculateTotalCost()
        {
            int cost = 0;
            foreach (TransactionLineItem lineItem in TransferList.Values)
            {
                cost += lineItem.ShippingCost;
            }
            return cost;
        }

        /// <summary>
        /// Check if player can afford cost of transfer, and destination outpost has room for everything
        /// </summary>
        /// <returns>true if transfer can be done</returns>
        private bool CanManageTransfer()
        {
            if (!TransactionLineItem.CanFit(DestinationOutpost.Inventory, TransferList.Values))
            {
                Util.ShowMessageBox(Strings.MSGBOX_DESTINATION_CANT_FIT_ITEMS);
                return false;
            }
            else
            {
                return Xenocide.GameState.GeoData.XCorp.Bank.CanAfford(CalculateTotalCost());
            }
        }

        /// <summary>user wants to look at a different outpost</summary>
        /// <param name="newDestinationIndex">Outpost to make destination for items</param>
        private void ChangeDestinationOutpost(int newDestinationIndex)
        {
            // if new destination is same as current one nothing to do
            if (newDestinationIndex != destinationOutpostIndex)
            {
                // destination can't be same as source
                if (newDestinationIndex == sourceOutpostIndex)
                {
                    Util.ShowMessageBox(Strings.MSGBOX_SOURCE_AND_DESTINATION_SAME);
                }
                else
                {
                    ScreenManager.ScheduleScreen(new MakeTransferScreen(sourceOutpostIndex, newDestinationIndex));
                }
            }
        }

        /// <summary>
        /// Close this screen and go back to the Bases Screen
        /// </summary>
        private void GoToBasesScreen()
        {
            ScreenManager.ScheduleScreen(new BasesScreen(sourceOutpostIndex));
        }

        #region Fields

        /// <summary>
        /// The outpost items will be taken from
        /// </summary>
        private Outpost SourceOutpost { get { return Xenocide.GameState.GeoData.Outposts[sourceOutpostIndex]; } }

        /// <summary>
        /// The outpost items will be sent to
        /// </summary>
        private Outpost DestinationOutpost { get { return Xenocide.GameState.GeoData.Outposts[destinationOutpostIndex]; } }

        // index specifying the outpost that items will be taken from
        private int sourceOutpostIndex;

        // index specifying the outpost that items will be sent to
        private int destinationOutpostIndex;

        /// <summary>
        /// The list of items user is transferring
        /// Format is row in grid, details
        /// </summary>
        private Dictionary<int, TransactionLineItem> TransferList = new Dictionary<int, TransactionLineItem>();

        #endregion Fields
    }
}
