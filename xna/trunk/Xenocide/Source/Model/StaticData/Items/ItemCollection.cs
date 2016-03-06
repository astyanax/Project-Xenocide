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
* @file ItemCollection
* @date Created: 2007/06/17
* @author File creator: David Teviotdale
* @author Credits: nil
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

#endregion

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// List of all items (craft, weapons, ammo, etc.) in Xenocide
    /// </summary>
    public sealed class ItemCollection : IEnumerable<ItemInfo>
    {
        /// <summary>
        /// Load the list of items from a file
        /// </summary>
        /// <remarks>Can't do this in the ItemCollection constructor becase some items in the list need to refer 
        /// to other items.during their construction, so the items need to be able to find the ItemList.</remarks>
        /// <param name="filename">Name of the XML file</param>
        /// <param name="xnetEntryList">X-Net entries to check against</param>
        public void Populate(string filename, XNetEntryCollection xnetEntryList)
        {
            items = new SortedList<string, ItemInfo>();
            
            // Set up XPathNavigator
            const string xmlns = "ItemConfig";
            XPathNavigator nav = Util.MakeValidatingXPathNavigator(filename, xmlns);
            XmlNamespaceManager manager = new XmlNamespaceManager(nav.NameTable);
            manager.AddNamespace("i", xmlns);

            // Process the XML file
            foreach (XPathNavigator itemElement in nav.Select("/i:itemList/i:item", manager))
            {
                ItemInfo item = Factory(itemElement, manager);
                items.Add(item.Id, item);
            }
            ValidateList(xnetEntryList);
        }

        /// <summary>
        /// Retreive an Item by position
        /// </summary>
        /// <param name="pos">position (zero based)</param>
        /// <returns>the Item</returns>
        public ItemInfo this[int pos] { get { return items.Values[pos]; } }

        /// <summary>
        /// Retreive an Item by name
        /// </summary>
        /// <param name="id">internal name used to ID the item type</param>
        /// <returns>the Item</returns>
        public ItemInfo this[string id] { get { return items[id]; } }

        /// <summary>
        /// Return the index to the item with the specified id
        /// </summary>
        /// <param name="id">id to search for</param>
        /// <returns>the index</returns>
        public int IndexOf(string id)
        {
            return items.IndexOfKey(id);
        }
        
        /// <summary>
        /// Implement generics IEnumerable&lt;T&gt; interface
        /// </summary>
        /// <returns>The Enumerator</returns>
        IEnumerator<ItemInfo> IEnumerable<ItemInfo>.GetEnumerator()
        {
            return items.Values.GetEnumerator();
        }

        /// <summary>
        /// Implement IEnumerable interface
        /// </summary>
        /// <returns>The Enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.Values.GetEnumerator();
        }

        /// <summary>
        /// The list of item types
        /// </summary>
        private SortedList<string, ItemInfo> items;

        /// <summary>
        /// Check that the list of items we've loaded is good
        /// </summary>
        /// <param name="xnetEntryList">X-Net entries to check against</param>
        /// <remarks>Will throw if list isn't valid</remarks>
        private void ValidateList(XNetEntryCollection xnetEntryList)
        {
            // check there's an X-Net entry for each item
            foreach (ItemInfo i in items.Values)
            {
                if ((null == xnetEntryList.FindEntryFor(i.Id)))
                {
                    // ToDo: Connect up when X-Net is finished
                    // throw new XPathException(Util.StringFormat(Strings.Get(EXCEPTION_NO_XNET_ENTRY_FOUND, i.Name));
                }
            }
        }

        /// <summary>
        /// Construct correct type of Item from an XML element
        /// </summary>
        /// <param name="element">Navigator to XML element</param>
        /// <param name="manager">Namespace used in item.xml</param>
        /// <returns>the Item</returns>
        private static ItemInfo Factory(XPathNavigator element, XmlNamespaceManager manager)
        {
            // get the type of XML node (which tells us the type of item to build)
            string type = element.GetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance");
            Debug.Assert(!String.IsNullOrEmpty(type));

            if (type == "itemType")
            {
                return new BaseOnlyItemInfo(element, manager);
            }
            else if (type == "craftItemType")
            {
                return new AircraftItemInfo(element, manager);
            }
            else if (type == "ufoItemType")
            {
                return new UfoItemInfo(element, manager);
            }
            else if ((type == "craftClipItemType") || (type == "clipItemType"))
            {
                return new ClipItemInfo(element, manager);
            }
            else if (type == "craftWeaponItemType")
            {
                return new CraftWeaponItemInfo(element, manager);
            }
            else if (type == "personItemType")
            {
                return new PersonItemInfo(element, manager);
            }
            // ToDo: these types haven't been implemented yet.
            else if (
                   (type == "troopItemType")
                || (type == "rangedWeaponItemType")
                || (type == "guidedWeaponItemType")
                || (type == "meleeWeaponItemType")
                || (type == "equipmentItemType")
                || (type == "grenadeWeaponItemType")
            )
            {
                return new BaseOnlyItemInfo(element, manager);
            }
            else
            {
                // someone's created a new item type
                Debug.Assert(false);
                return null;
            }
        }
    }
}
