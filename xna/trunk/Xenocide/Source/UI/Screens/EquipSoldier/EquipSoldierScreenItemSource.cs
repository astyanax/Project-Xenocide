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
* @file EquipSoldierScreenItemSource.cs
* @date Created: 2007/12/23
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using CeGui;


using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.UI.Scenes;
#endregion

// alias Vector2
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// This is the screen that lets player set the items a soldier carries
    /// </summary>
    public partial class EquipSoldierScreen
    {
        /// <summary>
        /// Represents where we're getting items to add to a soldier (or where we're dumping the items
        /// removed from the soldier)
        /// </summary>
        private abstract class ItemSource
        {
            /// <summary>
            ///  Fetch the item on the "ground" area of the screen at the position given by the X and Y co-ordinates
            /// </summary>
            /// <param name="x">cell co-ordinate (NOT pixel)</param>
            /// <param name="y">cell co-ordinate (NOT pixel)</param>
            /// <returns>Item, if there was an item at that position, else null</returns>
            public Item FetchItem(int x, int y)
            {
                // find item in list that is at this position
                int offset = 0;
                int i = GetLeftmostItemIndex();
                while ((i < itemList.Count) && (offset < InventoryLocation.GroundWidthInCells))
                {
                    ItemEntry entry = itemList[i];
                    if ((offset <= x) && (x < offset + entry.CarryInfo.X) && (y < entry.CarryInfo.Y))
                    {
                        // item found, return it, and update list of items on ground
                        Item item =  Remove(entry.Item);
                        if (0 == --entry.Count)
                        {
                            // no more items of this type at this position, so remove entry from list
                            itemList.RemoveAt(i);
                        }
                        else
                        {
                            itemList[i] = entry;
                        }
                        return item;
                    }

                    // try next
                    offset += itemList[i].CarryInfo.X;
                    ++i;
                }
                // if get here, no item found
                return null;
            }

            /// <summary>
            ///  Remove an item from Source
            /// </summary>
            /// <param name="item">type of item to remove</param>
            /// <returns>removed item</returns>
            protected abstract Item Remove(Item item);

            /// <summary>
            /// Put an item back into the store
            /// </summary>
            /// <param name="item">Item being dropped</param>
            public abstract void ReplaceItem(Item item);

            /// <summary>
            /// Draw the items on the "ground" area of the screen
            /// </summary>
            /// <param name="sceneWindow">where to draw the scene on the Window</param>
            /// <param name="scene">scene that will draw the items</param>
            public void Draw(Rectangle sceneWindow, EquipSoldierScene scene)
            {
                int offset = 0;
                int i = GetLeftmostItemIndex();
                while ((i < itemList.Count) && (offset < InventoryLocation.GroundWidthInCells))
                {
                    scene.DrawItemOnGround(sceneWindow, itemList[i].CarryInfo, offset, itemList[i].Count);
                    offset += itemList[i].CarryInfo.X;
                    ++i;
                }
            }

            /// <summary>
            /// Scroll the list of items to show on the "ground" area of the screen to the left
            /// </summary>
            public void ScrollLeft()
            {
                // if we're not hard against the left margin, scroll items one position to the left
                int index = GetLeftmostItemIndex();
                if (0 < index)
                {
                    scrollOffset -= itemList[index - 1].CarryInfo.X;
                }
                Debug.Assert(0 <= scrollOffset, "Invariant violated");
            }

            /// <summary>
            /// Scroll the list of items to show on the "ground" area of the screen to the right
            /// </summary>
            public void ScrollRight()
            {
                // calc maximum offset value we can have
                int maxOffset = -(int)(InventoryLocation.GroundWidthInCells);
                foreach (ItemEntry entry in itemList)
                {
                    maxOffset += entry.CarryInfo.X;
                }

                // if we can, scroll items one item to right
                if (scrollOffset < maxOffset)
                {
                    scrollOffset += itemList[GetLeftmostItemIndex()].CarryInfo.X;
                }
            }

            /// <summary>
            /// Check if X-Corp soldiers can carry this type of item
            /// </summary>
            /// <param name="item">type of item</param>
            /// <returns>true if soldier can carry this type of item</returns>
            public static bool SoldierCanCarry(ItemInfo item)
            {
                // item must be carryable and of researched technology
                return (null != item.CarryInfo) &&
                    Xenocide.GameState.GeoData.XCorp.TechManager.IsAvailable(item.Id);
            }

            /// <summary>
            /// Return index to item that is currently being drawn at leftmost position on "ground"
            /// </summary>
            /// <returns></returns>
            private int GetLeftmostItemIndex()
            {
                int itemOffset = 0;
                for (int i = 0; i < itemList.Count; ++i)
                {
                    if (itemOffset == scrollOffset)
                    {
                        // we've found the leftmost item.
                        return i;
                    }

                    // move onto next item
                    itemOffset += itemList[i].CarryInfo.X;
                }

                // if we get here, something's wrong, or there are no items.
                Debug.Assert(0 == itemList.Count);
                return 0;
            }

            /// <summary>
            /// Details of an item on the "ground" area of the screen
            /// </summary>
            protected struct ItemEntry
            {
                /// <summary>
                /// Ctor
                /// </summary>
                /// <param name="item">a representative instance of the item</param>
                /// <param name="count">number of items of this type available (when in outpost)</param>
                public ItemEntry(Item item, int count)
                {
                    this.item   = item;
                    this.count  = count;
                }

                /// <summary>a representative instance of the item</summary>
                public Item Item { get { return item; } }

                /// <summary>number of items of this type available (when in outpost)</summary>
                public int Count { get { return count; } set { count = value; } }

                /// <summary>
                /// The CarryInfo of the specified item
                /// </summary>
                public CarryInfo CarryInfo { get { return item.ItemInfo.BattlescapeInfo.CarryInfo; } }

                /// <summary>a representative instance of the item</summary>
                private Item item;

                /// <summary>number of items of this type available (when in outpost)</summary>
                private int count;

            }

            #region Fields

            /// <summary>
            /// The items on the "ground" area of the screen
            /// </summary>
            protected List<ItemEntry> ItemList { get { return itemList; } }

            /// <summary>
            /// The items on the "ground" area of the screen
            /// </summary>
            private List<ItemEntry> itemList = new List<ItemEntry>();

            /// <summary>
            /// Number of cells to the right we have scrolled the items on the ground
            /// </summary>
            private int scrollOffset;

            #endregion Fields
        }

        /// <summary>
        /// Represents the outpost where we're getting items to add to a soldier (or where we're dumping the items
        /// removed from the soldier)
        /// </summary>
        private class OutpostItemSource : ItemSource
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="outpostInventory">Outpost being used as item source/sink</param>
            public OutpostItemSource(OutpostInventory outpostInventory)
            {
                this.outpostInventory = outpostInventory;
                BuildItemList();
            }

            /// <summary>
            /// Build list of items to show on "Ground" area of screen
            /// </summary>
            private void BuildItemList()
            {
                ItemList.Clear();
                foreach (Item item in outpostInventory.ListContents())
                {
                    // skip Items player's soldiers can't carry
                    if (SoldierCanCarry(item.ItemInfo))
                    {
                        ItemList.Add(new ItemEntry(item, outpostInventory.NumberInInventory(item.ItemInfo)));
                    }
                }
            }

            /// <summary>
            ///  Remove an item from Source
            /// </summary>
            /// <param name="item">item to remove</param>
            /// <returns>removed item</returns>
            protected override Item Remove(Item item)
            {
                // if not unique, get an item of this type from outpost
                // Because if we're dealing with multiple items, we need to instanciate them
                // as we equip the soldier with them.
                if (!item.ItemInfo.IsUnique)
                {
                    item = Xenocide.StaticTables.ItemList[item.ItemInfo.Id].FromOutpost(outpostInventory);
                }
                outpostInventory.Remove(item);
                return item;
            }

            /// <summary>
            /// Put an item back into the store
            /// </summary>
            /// <param name="item">Item being dropped</param>
            public override void ReplaceItem(Item item)
            {
                // put item into Outpost's inventory
                Debug.Assert(null != item);
                outpostInventory.Add(item, false);
                BuildItemList();
            }

            #region Fields

            /// <summary>Outpost being used as item source/sink</summary>
            private OutpostInventory outpostInventory;

            #endregion Fields
        }
    }
}
