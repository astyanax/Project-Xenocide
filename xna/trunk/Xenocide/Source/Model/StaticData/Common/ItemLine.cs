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
* @file ItemLine.cs
* @date Created: 2007/08/13
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

using ProjectXenocide.Utils;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape.Outposts;

#endregion

namespace ProjectXenocide.Model.StaticData
{
    /// <summary>
    /// Records quantity of item type
    /// 1. Initial inventory in a base.
    /// 2. Requirements to build something.
    /// 3. Salvage from a UFO
    /// </summary>
    [Serializable]
    public class ItemLine
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemId">type of item</param>
        /// <param name="quantity">How many units there are of the item</param>
        public ItemLine(String itemId, int quantity)
        {
            this.itemId   = itemId;
            this.quantity = quantity;
        }

        /// <summary>
        /// Construct from XML file
        /// </summary>
        /// <param name="node">XML node holding data</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if itemNode == null")]
        public ItemLine(XPathNavigator node)
            : this(
                Util.GetStringAttribute(node, "type"), 
                Util.GetIntAttribute(node,    "quantity")
            )
        {
        }

        /// <summary>
        /// Construct item handle used to place item of this type in outpost
        /// </summary>
        /// <returns>the handle</returns>
        public virtual Item Construct()
        {
            return ItemInfo.Manufacture();
        }

        /// <summary>
        /// Does specified outpost hold at least quantity of this item type?
        /// </summary>
        /// <param name="outpost">outpost to check</param>
        /// <returns>true if outpost holds at least this quantity</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if outpost == null")]
        public bool Contains(Outpost outpost)
        {
            return (quantity <= outpost.Inventory.NumberInInventory(ItemInfo));
        }

        /// <summary>
        /// Remove at least quantity of this item type from an outpost
        /// </summary>
        /// <param name="outpost">outpost to remove from</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if outpost == null")]
        public void Remove(Outpost outpost)
        {
            Debug.Assert(Contains(outpost));
            Item temp = ItemInfo.FromOutpost(outpost.Inventory);
            for (int i = 0; i < quantity; ++i)
            {
                outpost.Inventory.Remove(temp);
            }
        }

        #region Fields

        /// <summary>
        /// Type of item
        /// </summary>
        public String ItemId { get { return itemId; } }

        /// <summary>
        /// How many units there are of the item
        /// </summary>
        public int Quantity { get { return quantity; } set { quantity = value; } }

        /// <summary>
        /// The common characteristics of every item of this type
        /// </summary>
        public ItemInfo ItemInfo { get { return Xenocide.StaticTables.ItemList[itemId]; } }

        /// <summary>
        /// Type of item
        /// </summary>
        private String itemId;

        /// <summary>
        /// How many units there are of the item
        /// </summary>
        private int quantity;

        #endregion Fields
    }
}
