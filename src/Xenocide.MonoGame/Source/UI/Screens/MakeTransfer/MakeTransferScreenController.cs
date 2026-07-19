using System.Collections.Generic;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Screens
{
    public partial class MakeTransferScreen
    {
        /// <summary>
        /// Handles all game logic for the transfer screen: cost calculations,
        /// capacity validation, and transfer execution.
        /// </summary>
        /// <remarks>
        /// ARCHITECTURE: This controller owns all game state queries and mutations
        /// for transfers. The Screen class delegates to this controller for business
        /// logic and updates GUI grids based on results.
        ///
        /// GAME MECHANICS:
        /// - Items are transferred between outposts via Shipment
        /// - Transfer cost is calculated from item shipping costs
        /// - Destination inventory capacity is checked
        /// - Bank is debited for total transfer cost
        /// - Items are removed from source and added to shipment
        /// </remarks>
        private class TransferController
        {
            private readonly Outpost sourceOutpost;
            private readonly Outpost destinationOutpost;

            public TransferController(Outpost sourceOutpost, Outpost destinationOutpost)
            {
                this.sourceOutpost = sourceOutpost;
                this.destinationOutpost = destinationOutpost;
            }

            /// <summary>
            /// Gets items available for transfer from the source outpost.
            /// </summary>
            /// <returns>List of TransactionLineItem for each transferable item</returns>
            public List<TransactionLineItem> GetTransferableItems()
            {
                var items = new List<TransactionLineItem>();
                foreach (Item i in sourceOutpost.Inventory.ListContents())
                {
                    if (i.CanRemoveFromOutpost)
                    {
                        items.Add(new TransactionLineItem(i, sourceOutpost.Inventory, destinationOutpost.Inventory));
                    }
                }
                return items;
            }

            /// <summary>
            /// Calculates the total shipping cost for all items in the transfer list.
            /// </summary>
            /// <param name="transferItems">List of items being transferred</param>
            /// <returns>Total cost</returns>
            public static int CalculateTotalCost(List<TransactionLineItem> transferItems)
            {
                int cost = 0;
                foreach (TransactionLineItem lineItem in transferItems)
                {
                    cost += lineItem.ShippingCost;
                }
                return cost;
            }

            /// <summary>
            /// Checks if the destination can fit all items and bank can afford the cost.
            /// Shows message box on failure.
            /// </summary>
            /// <param name="transferItems">List of items being transferred</param>
            /// <returns>True if transfer is possible</returns>
            public bool CanManageTransfer(List<TransactionLineItem> transferItems)
            {
                if (!TransactionLineItem.CanFit(destinationOutpost.Inventory, transferItems))
                {
                    Util.ShowMessageBox(Strings.MSGBOX_DESTINATION_CANT_FIT_ITEMS);
                    return false;
                }

                int totalCost = CalculateTotalCost(transferItems);
                if (!Xenocide.GameState.GeoData.XCorp.Bank.CanAfford(totalCost))
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Executes the transfer: debits bank, creates shipment, and ships items.
            /// </summary>
            /// <param name="transferItems">List of items being transferred</param>
            public void ExecuteTransfer(List<TransactionLineItem> transferItems)
            {
                int totalCost = CalculateTotalCost(transferItems);
                Xenocide.GameState.GeoData.XCorp.Bank.Debit(totalCost);

                Shipment shipment = new Shipment(destinationOutpost, Shipment.CalcEta());
                foreach (TransactionLineItem lineItem in transferItems)
                {
                    lineItem.RemoveItems(sourceOutpost.Inventory, shipment);
                }
                shipment.Ship();
            }

            /// <summary>
            /// Checks if the source and destination are different outposts.
            /// </summary>
            /// <param name="sourceIndex">Source outpost index</param>
            /// <param name="destinationIndex">Destination outpost index</param>
            /// <returns>True if outposts are different</returns>
            public static bool AreDifferentOutposts(int sourceIndex, int destinationIndex)
            {
                if (sourceIndex == destinationIndex)
                {
                    Util.ShowMessageBox(Strings.MSGBOX_SOURCE_AND_DESTINATION_SAME);
                    return false;
                }
                return true;
            }
        }
    }
}
