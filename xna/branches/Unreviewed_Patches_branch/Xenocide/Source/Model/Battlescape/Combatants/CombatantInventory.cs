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
* @file CombatantInventory.cs
* @date Created: 2007/10/22
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape.Outposts;

namespace ProjectXenocide.Model.Battlescape.Combatants
{
    /// <summary>
    /// The places where a combatant may store items
    /// </summary>
    public enum Zone
    {
        /// <summary></summary>
        None,

        /// <summary></summary>
        RightHand,

        /// <summary></summary>
        LeftHand,

        /// <summary></summary>
        RightShoulder,

        /// <summary></summary>
        LeftShoulder,

        /// <summary></summary>
        RightLeg,

        /// <summary></summary>
        LeftLeg,

        /// <summary></summary>
        BackPack,

        /// <summary></summary>
        Belt,

        /// <summary></summary>
        Armor,

        /// <summary></summary>
        Ground
    }

    /// <summary>
    /// The items being carried by a combatant
    /// </summary>
    [Serializable]
    public partial class CombatantInventory
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="combatant">Combatant this inventory belongs to</param>
        public CombatantInventory(Combatant combatant)
        {
            this.combatant = combatant;
        }

        /// <summary>
        /// Get item stored at specific location in inventory
        /// </summary>
        /// <param name="x">column position</param>
        /// <param name="y">row position</param>
        /// <returns>the item, or null if no item there</returns>
        public Item ItemAt(int x, int y)
        {
            foreach (Slot slot in contents)
            {
                CarryInfo carryInfo = slot.Item.ItemInfo.CarryInfo;
                int width  = carryInfo.X;
                int height = carryInfo.Y;

                // hands are special case, item only takes up one unit in "cells"
                if (InventoryLayout.IsHand(slot.X, slot.Y))
                {
                    width  = 1;
                    height = 1;
                }

                if ((slot.X <= x) && (x < (slot.X + width)) && (slot.Y <= y) && (y < (slot.Y + height)))
                {
                    return slot.Item;
                }
            }
            // no item at this position
            return null;
        }

        /// <summary>
        /// Remove item from inventory
        /// </summary>
        /// <param name="item">item to remove</param>
        public void Remove(Item item)
        {
            int index = Find(item);
            Debug.Assert(NotFound != index);
            contents.RemoveAt(index);

            // if this item is armor, update combatant's armor property
            if (item.ItemInfo.IsArmor)
            {
                combatant.Armor = Xenocide.StaticTables.ArmorList.NoArmor;
            }
        }

        /// <summary>
        /// Can specified item be put into specified location in inventory?
        /// </summary>
        /// <param name="item">item to insert</param>
        /// <param name="x">column for top left corner of item</param>
        /// <param name="y">row for top left corner of item</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification="Will throw if item is null")]
        public bool CanFit(Item item, int x, int y)
        {
            // armor slot is special case, can only take armor type items
            if (InventoryLayout.IsArmor(x, y))
            {
                // can only put armor items into armor position
                if (!item.ItemInfo.IsArmor)
                {
                    return false;
                }
                else
                {
                    return IsEmpty(x, y);
                }
            }

            // can't put armor items anywhere but in the armor slot
            if (item.ItemInfo.IsArmor)
            {
                return false;
            }

            // hands are special cases (one item, of size up to 2 x 3)
            if (InventoryLayout.IsHand(x, y) && InventoryLayout.CanFitInHand(item))
            {
                return IsEmpty(x, y);
            }
            else
            {
                // check it doesn't fall off the grid
                CarryInfo carryInfo = item.ItemInfo.CarryInfo;
                if ((InventoryLayout.CellsWidth < x + carryInfo.X) || (InventoryLayout.CellsHeight < y + carryInfo.Y))
                {
                    return false;
                }

                // check there's empty cells to take it
                Zone zone = InventoryLayout.GetZone(x, y);

                // can't put items in cell that doesn't exist
                if (Zone.None == zone)
                {
                    return false;
                }

                for (int i = 0; i < carryInfo.X; ++i)
                {
                    for (int j = 0; j < carryInfo.Y; ++j)
                    {
                        if ((InventoryLayout.GetZone(x + i, y + j) != zone) || !IsEmpty(x + i, y + j))
                        {
                            return false;
                        }
                    }
                }
                // if get here, item will fit
                return true;
            }
        }

        /// <summary>
        /// Put specified item into specified location in inventory?
        /// </summary>
        /// <param name="item">item to insert</param>
        /// <param name="x">column for top left corner of item</param>
        /// <param name="y">row for top left corner of item</param>
        public void Insert(Item item, int x, int y)
        {
            Debug.Assert(CanFit(item, x, y));
            Debug.Assert(NotFound == Find(item));
            contents.Add(new Slot(item, x, y));

            // if this item is armor, update combatant's armor property
            if (item.ItemInfo.IsArmor)
            {
                combatant.Armor = item.ItemInfo.Armor;
            }
        }

        /// <summary>
        /// Record current loadout (for automatic reload when return to outpost)
        /// </summary>
        public void RecordLoadout()
        {
            defaultLoadout = SnapshotLoadout() as List<Slot>;
        }

        /// <summary>
        /// Take a copy of the loadout
        /// </summary>
        /// <returns>Copy of the loadout</returns>
        public IList<Slot> SnapshotLoadout()
        {
            List<Slot> snapshot = new List<Slot>();
            foreach (Slot s in contents)
            {
                snapshot.Add(new Slot(s));
            }
            return snapshot;
        }

        /// <summary>
        /// Adjust contents to match default Loadout
        /// </summary>
        /// <param name="inventory">Where to put surplus items/get missing items from</param>
        /// <returns>true if was able to restore everything</returns>
        public bool RestoreLoadout(OutpostInventory inventory)
        {
            // put everything into the outpost's inventory
            Unload(inventory);

            // now get the items listed in the default Loadout
            bool success = true;
            foreach (Slot s in defaultLoadout)
            {
                Item temp = s.Item.FetchFromOutpost(inventory, ref success);
                if (null != temp)
                {
                    Insert(temp, s.X, s.Y);
                }
            }
            return success;
        }

        /// <summary>
        /// Put everyting being carried into an Outpost's inventory
        /// </summary>
        /// <param name="inventory">where to put the contents</param>
        public void Unload(OutpostInventory inventory)
        {
            for (int i = contents.Count - 1; 0 <= i; --i)
            {
                Slot s = contents[i];
                Remove(s.Item);
                inventory.Add(s.Item, false);
            }
        }

        /// <summary>
        /// Is the specified position in the inventory empty?
        /// </summary>
        /// <param name="x">column position</param>
        /// <param name="y">row position</param>
        /// <returns>true if location is empty</returns>
        private bool IsEmpty(int x, int y)
        {
            return (null == ItemAt(x, y));
        }

        /// <summary>
        /// Find a specific item in the inventory
        /// </summary>
        /// <param name="item">item to find</param>
        /// <returns>index to slot holding item, or -1 if item not found</returns>
        public int Find(Item item)
        {
            for (int i = 0; i < Contents.Count; ++i)
            {
                if (Contents[i].Item == item)
                {
                    return i;
                }
            }

            // not found
            return NotFound;
        }

        /// <summary>
        ///  An item stored in a combatant's inventory, and its position
        /// </summary>
        [Serializable]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible",
            Justification="We can handle nested classes")]
        public class Slot
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="item">The Item being held</param>
            /// <param name="x">column with top left corner of item</param>
            /// <param name="y">row with top left corner of item</param>
            public Slot(Item item, int x, int y)
            {
                this.item = item;
                this.x    = x;
                this.y    = y;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="rhs">to be copied</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
                Justification = "will throw when rhs is null")]
            public Slot(Slot rhs)
                :
                this(rhs.Item.Clone(), rhs.x, rhs.y)
            {
            }

            #region Fields

            /// <summary>
            /// The Item being held
            /// </summary>
            public Item Item { get { return item; } }

            /// <summary>
            /// column with top left corner of item
            /// </summary>
            public int X { get { return x; } }

            /// <summary>
            /// row with top left corner of item
            /// </summary>
            public int Y { get { return y; } }

            /// <summary>
            /// The Item being held
            /// </summary>
            private Item item;

            /// <summary>
            /// column with top left corner of item
            /// </summary>
            private int x;

            /// <summary>
            /// row with top left corner of item
            /// </summary>
            private int y;

            #endregion Fields
        }

        #region Fields

        /// <summary>
        /// The items being carried by the combatant
        /// </summary>
        public IList<Slot> Contents { get { return contents; } }

        /// <summary>
        /// Magic number, int Find(Item) didn't find anything
        /// </summary>
        public const int NotFound = -1;

        /// <summary>
        /// The items being carried by the combatant
        /// </summary>
        private List<Slot> contents = new List<Slot>();

        /// <summary>
        /// loadout that system will try to restore each time soldier returns to outpost
        /// </summary>
        private List<Slot> defaultLoadout = new List<Slot>();

        /// <summary>
        /// Combatant this inventory belongs to
        /// </summary>
        private Combatant combatant;

        #endregion Fields
    }
}
