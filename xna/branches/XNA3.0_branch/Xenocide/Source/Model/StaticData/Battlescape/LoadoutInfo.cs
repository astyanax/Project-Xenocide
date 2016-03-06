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
* @file LoadoutInfo.cs
* @date Created: 2008/02/11
* @author File creator: David Teviotdale
* @author Credits: nil
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;


using ProjectXenocide.Utils;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Battlescape.Combatants;

#endregion

namespace ProjectXenocide.Model.StaticData.Battlescape
{
        /// <summary>
    /// List of items that a combatant may be equiped with
        /// </summary>
    [Serializable]
    public class LoadoutInfo
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="node">XML node holding data to construct LoadoutInfo</param>
        /// <param name="manager">Namespace used in combatant.xml</param>
        public LoadoutInfo(XPathNavigator node, XmlNamespaceManager manager)
        {
            foreach (XPathNavigator itemNode in node.Select("c:item", manager))
            {
                ItemEntry entry = new ItemEntry();
                entry.Name = Util.GetStringAttribute(itemNode, "name");
                entry.X    = Util.GetIntAttribute(   itemNode, "x");
                entry.Y    = Util.GetIntAttribute(   itemNode, "y");
                items.Add(entry);
            }
        }

        /// <summary>Load a combatant with his/her initial equipement</summary>
        /// <param name="inventory">to put the stuff into</param>
        public void Equip(CombatantInventory inventory)
        {
            foreach (ItemEntry entry in items)
            {
                // create item
                Item item = Xenocide.StaticTables.ItemList[entry.Name].Manufacture();
                if (item.HoldsAmmo && !item.IsClip)
                {
                    item.LoadDefaultAmmo();
                }
                inventory.Insert(item, entry.X, entry.Y);
            }
        }

        #region Fields

        /// <summary>Details of an item to give combatant</summary>
        [Serializable]
        private struct ItemEntry
        {
            /// <summary>Name of item</summary>
            public string Name;

            /// <summary>Position of item in combatant's inventory</summary>
            public int X;

            /// <summary>Position of item in combatant's inventory</summary>
            public int Y;
        }

        /// <summary>Type of InventoryLayout combatant has</summary>
        private List<ItemEntry> items = new List<ItemEntry>();

        #endregion
    }
}
