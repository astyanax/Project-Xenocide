using System.Collections.Generic;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Screens
{
    public partial class PurchaseScreen
    {
        /// <summary>
        /// Handles all game logic for the purchase screen: item availability checks,
        /// cost calculations, inventory capacity validation, and purchase execution.
        /// </summary>
        /// <remarks>
        /// ARCHITECTURE: This controller owns all game state queries and mutations
        /// for purchasing. The Screen class delegates to this controller for business
        /// logic and updates GUI grids based on results.
        ///
        /// GAME MECHANICS:
        /// - Items must be researched (available via TechManager) to be purchased
        /// - Purchased items are added to a Shipment with an ETA
        /// - Bank is debited for the total cost
        /// - Inventory capacity is checked before purchase
        /// </remarks>
        private class PurchaseController
        {
            private readonly Outpost outpost;

            public PurchaseController(Outpost outpost)
            {
                this.outpost = outpost;
            }

            /// <summary>
            /// Checks if an item is available for purchase.
            /// </summary>
            /// <param name="item">The item to check</param>
            /// <returns>True if item can be purchased</returns>
            public static bool IsAvailableForPurchase(ItemInfo item)
            {
                return item.CanPurchase && Xenocide.GameState.GeoData.XCorp.TechManager.IsAvailable(item.Id);
            }

            /// <summary>
            /// Calculates the total cost of items in the shopping list.
            /// </summary>
            /// <param name="shoppingList">Dictionary mapping item index to quantity</param>
            /// <returns>Total cost</returns>
            public static int CalculateTotalCost(Dictionary<int, int> shoppingList)
            {
                int cost = 0;
                foreach (var kvp in shoppingList)
                {
                    cost += (Xenocide.StaticTables.ItemList[kvp.Key].BuyPrice * kvp.Value);
                }
                return cost;
            }

            /// <summary>
            /// Checks if all items in the shopping list fit in the outpost's inventory.
            /// Shows message box if they don't fit.
            /// </summary>
            /// <param name="shoppingList">Dictionary mapping item index to quantity</param>
            /// <returns>True if all items fit</returns>
            public bool CanFitAll(Dictionary<int, int> shoppingList)
            {
                bool canFit = true;

                // Allocate space and check
                foreach (var kvp in shoppingList)
                {
                    ItemInfo item = Xenocide.StaticTables.ItemList[kvp.Key];
                    for (int i = 0; i < kvp.Value; ++i)
                    {
                        if (!outpost.Inventory.CanFit(item))
                        {
                            canFit = false;
                        }
                        outpost.Inventory.AllocateSpace(item);
                    }
                }

                // Release allocated space
                foreach (var kvp in shoppingList)
                {
                    ItemInfo item = Xenocide.StaticTables.ItemList[kvp.Key];
                    for (int i = 0; i < kvp.Value; ++i)
                    {
                        outpost.Inventory.ReleaseSpace(item);
                    }
                }

                if (!canFit)
                {
                    Util.ShowMessageBox(Strings.MSGBOX_CANT_FIT);
                }

                return canFit;
            }

            /// <summary>
            /// Executes the purchase: debits bank, creates shipment, and ships items.
            /// </summary>
            /// <param name="shoppingList">Dictionary mapping item index to quantity</param>
            public void ExecutePurchase(Dictionary<int, int> shoppingList)
            {
                int totalCost = CalculateTotalCost(shoppingList);
                Xenocide.GameState.GeoData.XCorp.Bank.Debit(totalCost);

                Shipment shipment = new Shipment(outpost, Shipment.CalcEta());
                foreach (var kvp in shoppingList)
                {
                    ItemInfo item = Xenocide.StaticTables.ItemList[kvp.Key];
                    for (int i = 0; i < kvp.Value; ++i)
                    {
                        shipment.Add(item.Manufacture());
                    }
                }
                shipment.Ship();
            }

            /// <summary>
            /// Gets the item quantity currently in the outpost's inventory.
            /// </summary>
            public int GetItemCount(ItemInfo item)
            {
                return outpost.Inventory.NumberInInventory(item);
            }

            /// <summary>
            /// Gets the current bank balance for display.
            /// </summary>
            public static string GetFundsDisplay()
            {
                return Util.StringFormat(Strings.SCREEN_PURCHASE_FUNDS,
                    Xenocide.GameState.GeoData.XCorp.Bank.CurrentBalance);
            }

            /// <summary>
            /// Checks if the bank can afford the given amount.
            /// </summary>
            public static bool CanAfford(int amount)
            {
                return Xenocide.GameState.GeoData.XCorp.Bank.CanAfford(amount);
            }
        }
    }
}
