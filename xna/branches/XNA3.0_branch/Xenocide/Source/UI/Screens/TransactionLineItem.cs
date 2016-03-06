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
* @file TransactionLineItem.cs
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

#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// A line in a sale or transfer operation
    /// </summary>
    public  class TransactionLineItem
    {
        /// <summary>
        /// Constructor (used for transfering)
        /// </summary>
        /// <param name="item">Item to build line item for</param>
        /// <param name="source">inventory item will be taken from</param>
        /// <param name="destination">inventory item will be put into</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if item or source is null")]
        public TransactionLineItem(Item item, OutpostInventory source, OutpostInventory destination)
        {
            this.item        = item;
            this.sourceCount = source.NumberInInventory(item.ItemInfo);
            this.maxMovable  = (item.ItemInfo.IsUnique) ? 1 : sourceCount;
            if (null != destination)
            {
                destinationCount = destination.NumberInInventory(item.ItemInfo);
            }
        }

        /// <summary>
        /// Constructor (used for selling)
        /// </summary>
        /// <param name="item">Item to build line item for</param>
        /// <param name="source">inventory item will be taken from</param>
        public TransactionLineItem(Item item, OutpostInventory source)
            :
            this(item, source, null)
        {
        }

        /// <summary>
        /// Remove the items specifed by this lineItem from the inventory
        /// </summary>
        /// <param name="inventory">Inventory to remove items from</param>
        /// <param name="shipment">to put items into, if supplied.  (can be null)</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if inventory or shipment is null")]
        public void RemoveItems(OutpostInventory inventory, Shipment shipment)
        {
            // If item is unique, then we just delete the item.
            // Otherwise, we need to remove and delete an instance of each item.
            // This is because clips may contain different amounts of ammo.
            if (item.ItemInfo.IsUnique && (1 == numMoving))
            {
                if (null != shipment)
                {
                    shipment.Add(item);
                    item.OnTransfer();
                }
                else
                {
                    item.OnSell();
                }
                inventory.Remove(item);
            }
            else
            {
                for (int i = 0; i < numMoving; ++i)
                {
                    Item temp = item.ItemInfo.FromOutpost(inventory);
                    inventory.Remove(temp);
                    if (null != shipment)
                    {
                        shipment.Add(temp);
                    }
                }
            }
        }

        /// <summary>
        /// Check if can fit everything on the list into the outpost's inventory
        /// <remarks>
        /// It's not very efficient, but it runs in response to user input, so doesn't need to be
        /// </remarks>
        /// </summary>
        /// <param name="inventory">inventory to put everything</param>
        /// <param name="list">the items to try and put in the inventory</param>
        /// <returns>true if list fits into the inventory</returns>
        public static bool CanFit(OutpostInventory inventory, IEnumerable<TransactionLineItem> list)
        {
            bool canFit = true;

            // first put everything into the base
            foreach (TransactionLineItem line in list)
            {
                for (int i = 0; i < line.NumMoving; ++i)
                {
                    if (!inventory.CanFit(line.Item))
                    {
                        canFit = false;
                    }
                    inventory.AllocateSpace(line.Item);
                }
            }

            // now take them all out again
            foreach (TransactionLineItem line in list)
            {
                for (int i = 0; i < line.NumMoving; ++i)
                {
                    inventory.ReleaseSpace(line.Item);
                }
            }

            return canFit;
        }

        #region Fields

        /// <summary>
        /// Number of these items in the source inventory
        /// </summary>
        public int SourceCount { get { return sourceCount; } }

        /// <summary>
        /// Number of these items in the destination inventory
        /// </summary>
        public int DestinationCount { get { return destinationCount; } }

        /// <summary>
        /// Maximum number of items that can be in this line item
        /// Not the same as number in source inventory, as unique items must
        /// have their own lines.  E.g. If there's 2 Gryphon in a base, then
        /// each Gryphon must have it's own line item
        /// </summary>
        public int MaxMovable { get { return maxMovable; } }

        /// <summary>
        /// Number of these items the player is selling
        /// </summary>
        public int NumMoving { get { return numMoving; } set { numMoving = value; } }

        /// <summary>
        /// Income from selling numSelling of these items
        /// </summary>
        public int Value { get { return numMoving * SellPrice; } }

        /// <summary>
        /// Cost to ship the item.
        /// Items cost 1% of their buy or sell price, whichever is greater
        /// </summary>
        public int ShippingCost
        {
            get { return numMoving * Math.Max(item.ItemInfo.BuyPrice, item.ItemInfo.SellPrice) / 100; }
        }

        /// <summary>
        /// Name of item, to show to user
        /// </summary>
        public String Name { get { return item.Name; } }

        /// <summary>
        /// Income we would get for selling item
        /// </summary>
        public int SellPrice { get { return item.ItemInfo.SellPrice; } }

        /// <summary>
        /// "Item" information for this line
        /// </summary>
        public ItemInfo Item { get { return item.ItemInfo; } }

        /// <summary>
        /// Type of item
        /// </summary>
        private Item item;

        /// <summary>
        /// Number of these items in the source inventory
        /// </summary>
        private int sourceCount;

        /// <summary>
        /// Number of these items in the destination inventory
        /// </summary>
        private int destinationCount;

        /// <summary>
        /// Maximum number of items that can be in this line item
        /// Not the same as number in source inventory, as unique items must
        /// have their own lines.  E.g. If there's 2 Gryphon in a base, then
        /// each Gryphon must have it's own line item
        /// </summary>
        private int maxMovable;

        /// <summary>
        /// Number of these items the player is selling/transfering
        /// </summary>
        private int numMoving;

        #endregion Fields
    }
}
