using System.Collections.Generic;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Screens
{
    public partial class SellScreen
    {
        /// <summary>
        /// Handles all game logic for the sell screen: calculating sale values,
        /// executing sales, and managing inventory.
        /// </summary>
        /// <remarks>
        /// ARCHITECTURE: This controller owns all game state queries and mutations
        /// for selling. The Screen class delegates to this controller for business
        /// logic and updates GUI grids based on results.
        ///
        /// GAME MECHANICS:
        /// - Items with CanRemoveFromOutpost = true can be sold
        /// - Sale value is calculated from item's SellPrice
        /// - Bank is credited with total sale value
        /// - Items are removed from outpost inventory
        /// </remarks>
        private class SellController
        {
            private readonly Outpost outpost;

            public SellController(Outpost outpost)
            {
                this.outpost = outpost;
            }

            /// <summary>
            /// Gets items available for sale from the outpost inventory.
            /// </summary>
            /// <returns>List of TransactionLineItem for each sellable item</returns>
            public List<TransactionLineItem> GetSellableItems()
            {
                var items = new List<TransactionLineItem>();
                foreach (Item i in outpost.Inventory.ListContents())
                {
                    if (i.CanRemoveFromOutpost)
                    {
                        items.Add(new TransactionLineItem(i, outpost.Inventory));
                    }
                }
                return items;
            }

            /// <summary>
            /// Calculates the total value of all items in the sales list.
            /// </summary>
            /// <param name="salesList">Dictionary of line items being sold</param>
            /// <returns>Total sale value</returns>
            public static int CalculateTotalValue(Dictionary<int, TransactionLineItem> salesList)
            {
                int value = 0;
                foreach (TransactionLineItem lineItem in salesList.Values)
                {
                    value += lineItem.Value;
                }
                return value;
            }

            /// <summary>
            /// Executes the sale: credits bank and removes items from inventory.
            /// </summary>
            /// <param name="salesList">Dictionary of line items being sold</param>
            public void ExecuteSale(Dictionary<int, TransactionLineItem> salesList)
            {
                // Credit the bank with total sale value
                int totalValue = CalculateTotalValue(salesList);
                Xenocide.GameState.GeoData.XCorp.Bank.Credit(totalValue);

                // Remove items from inventory
                foreach (TransactionLineItem lineItem in salesList.Values)
                {
                    lineItem.RemoveItems(outpost.Inventory, null);
                }
            }

            /// <summary>
            /// Checks if a line item can sell more (has room to increase quantity).
            /// </summary>
            /// <param name="lineItem">The line item to check</param>
            /// <returns>True if more items can be sold</returns>
            public static bool CanSellMore(TransactionLineItem lineItem)
            {
                return lineItem.NumMoving < lineItem.MaxMovable;
            }

            /// <summary>
            /// Checks if a line item can sell less (has items to decrease quantity).
            /// </summary>
            /// <param name="lineItem">The line item to check</param>
            /// <returns>True if items can be unsold</returns>
            public static bool CanSellLess(TransactionLineItem lineItem)
            {
                return lineItem.NumMoving > 0;
            }

            /// <summary>
            /// Gets the current bank balance for display.
            /// </summary>
            public static string GetFundsDisplay()
            {
                return Util.StringFormat(Strings.SCREEN_SELL_FUNDS,
                    Xenocide.GameState.GeoData.XCorp.Bank.CurrentBalance);
            }
        }
    }
}
