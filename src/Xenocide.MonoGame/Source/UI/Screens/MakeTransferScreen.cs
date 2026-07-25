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
    /// This is the screen that allows user to move items between outposts
    /// </summary>
    /// <remarks>
    /// ARCHITECTURE: GUI layer only — all game logic is delegated to the nested Controller
    /// class (in MakeTransfer/MakeTransferScreenController.cs). This screen manages the item
    /// grid and handles transfer interactions.
    /// </remarks>
    public partial class MakeTransferScreen : GumScreen
    {
        private ScreenLayout layout;
        private ContentArea content;

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
            this.controller = new TransferController(SourceOutpost, DestinationOutpost);
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

            layout.AddButton(XenocideResourceManager.Get("BUTTON_MOVE_MORE"), OnMoveMoreButton);
            layout.AddButton(XenocideResourceManager.Get("BUTTON_MOVE_LESS"), OnMoveLessButton);
            layout.AddButton(XenocideResourceManager.Get("BUTTON_CONFIRM"), OnConfirmButton);
            layout.AddButton(XenocideResourceManager.Get("BUTTON_CANCEL"), OnCancelButton);

            sourceText = new Label() { Text = Util.StringFormat(Strings.SCREEN_TRANSFER_SOURCE, SourceOutpost.Name) };
            content.Panel.AddChild(sourceText);

            totalCostText = new Label();
            content.Panel.AddChild(totalCostText);
            UpdateTotalCost();

            outpostsListComboBox = new ComboBox();
            outpostsListComboBox.Visual.Width = 300;
            content.Panel.AddChild(outpostsListComboBox);
            Misc.PopulateHumanBasesList(outpostsListComboBox, destinationOutpostIndex);
            outpostsListComboBox.SelectionChanged += (s, a) => OnOutpostSelectionChanged(s, EventArgs.Empty);

            InitializeGrid();
            content.AddGrid(grid);
            PopulateGrid();
        }

        private Label sourceText;
        private Label totalCostText;
        private ComboBox outpostsListComboBox;
        private GridPanel grid;

        private void InitializeGrid()
        {
            grid = new GridPanel();
            grid.AddColumn(Strings.SCREEN_TRANSFER_COLUMN_ITEM, 400);
            grid.AddColumn(Strings.SCREEN_TRANSFER_COLUMN_QUANTITY_IN_BASE, 84);
            grid.AddColumn(Strings.SCREEN_TRANSFER_COLUMN_QUANTITY_DESTINAION, 105);
            grid.AddColumn(Strings.SCREEN_TRANSFER_COLUMN_QUANTITY, 84);
        }

        private void PopulateGrid()
        {
            foreach (TransactionLineItem lineItem in controller.GetTransferableItems())
            {
                AddRowToGrid(lineItem);
            }
        }

        private void AddRowToGrid(TransactionLineItem lineItem)
        {
            grid.AddRow(lineItem,
                lineItem.Name,
                Util.ToString(lineItem.SourceCount),
                Util.ToString(lineItem.DestinationCount),
                Util.ToString(lineItem.NumMoving));
            transferItems.Add(lineItem);
        }

        #endregion Create the Gum controls

        #region event handlers

        private void OnMoveMoreButton(object sender, EventArgs e)
        {
            TransactionLineItem lineItem = GetSelectedItem();
            if (lineItem != null)
            {
                if (lineItem.NumMoving < lineItem.MaxMovable)
                {
                    ++lineItem.NumMoving;

                    if (!CanManageTransfer())
                    {
                        --lineItem.NumMoving;
                    }

                    UpdateDetails(lineItem);
                }
            }
        }

        private void OnMoveLessButton(object sender, EventArgs e)
        {
            TransactionLineItem lineItem = GetSelectedItem();
            if (lineItem != null)
            {
                if (0 < lineItem.NumMoving)
                {
                    --lineItem.NumMoving;

                    UpdateDetails(lineItem);
                }
            }
        }

        private void OnConfirmButton(object sender, EventArgs e)
        {
            controller.ExecuteTransfer(transferItems);
            GoToBasesScreen();
        }

        private void OnCancelButton(object sender, EventArgs e)
        {
            GoToBasesScreen();
        }

        private void OnOutpostSelectionChanged(object sender, EventArgs e)
        {
            if (outpostsListComboBox.SelectedObject != null)
            {
                ChangeDestinationOutpost(outpostsListComboBox.SelectedIndex);
            }
        }

        #endregion event handlers

        private TransactionLineItem GetSelectedItem()
        {
            TransactionLineItem lineItem = grid.GetSelectedTag() as TransactionLineItem;
            if (null == lineItem)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_SALE_SELECTED);
            }
            return lineItem;
        }

        private void UpdateTotalCost()
        {
            totalCostText.Text = Util.StringFormat(Strings.SCREEN_TRANSFER_TOTAL_COST,
                TransferController.CalculateTotalCost(transferItems));
        }

        private void UpdateDetails(TransactionLineItem lineItem)
        {
            UpdateTotalCost();

            int row = grid.GetRowIndexByTag(lineItem);
            if (row < 0) return;
            grid.SetCell(row, 3, Util.ToString(lineItem.NumMoving));
        }

        private bool CanManageTransfer()
        {
            return controller.CanManageTransfer(transferItems);
        }

        private void ChangeDestinationOutpost(int newDestinationIndex)
        {
            if (TransferController.AreDifferentOutposts(sourceOutpostIndex, newDestinationIndex))
            {
                ScreenManager.ScheduleScreen(new MakeTransferScreen(sourceOutpostIndex, newDestinationIndex));
            }
        }

        private void GoToBasesScreen()
        {
            ScreenManager.ScheduleScreen(new BasesScreen(sourceOutpostIndex));
        }

        #region Fields

        /// <summary>
        /// Controller handling game logic for transfers.
        /// </summary>
        private TransferController controller;

        private Outpost SourceOutpost { get { return Xenocide.GameState.GeoData.Outposts[sourceOutpostIndex]; } }

        private Outpost DestinationOutpost { get { return Xenocide.GameState.GeoData.Outposts[destinationOutpostIndex]; } }

        private int sourceOutpostIndex;

        private int destinationOutpostIndex;

        private List<TransactionLineItem> transferItems = new List<TransactionLineItem>();

        #endregion Fields
    }
}
