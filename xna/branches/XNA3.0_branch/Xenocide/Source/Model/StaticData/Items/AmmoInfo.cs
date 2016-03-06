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
* @file AmmoInfo.cs
* @date Created: 2007/11/10
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Collections.Specialized;


using ProjectXenocide.Utils;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// Information on the ammunition an item can hold
    /// </summary>
    [Serializable]
    public class AmmoInfo
    {
        /// <summary>
        /// Construct AmmoInfo from information in an XML element
        /// </summary>
        /// <param name="ammoElement">XML element holding data to construct AmmoInfo</param>
        /// <param name="manager">Namespace used in item.xml</param>
        public AmmoInfo(XPathNavigator ammoElement, XmlNamespaceManager manager)
        {
            this.capacity = Util.GetIntAttribute(ammoElement, "capacity");
            foreach (XPathNavigator ammoName in ammoElement.Select("i:ammoName", manager))
            {
                ammos.Add(Util.GetStringAttribute(ammoName, "name"));
            }
        }

        /// <summary>
        /// Check if this ammo appears on the list of ammos
        /// </summary>
        /// <param name="ammo">ammo to check for</param>
        /// <returns>true if it's valid</returns>
        public bool IsAmmoValid(ItemInfo ammo)
        {
            // null is always valid
            return ((null == ammo) || (-1 != ammos.IndexOf(ammo.Id)));
        }

        /// <summary>
        /// Add stats specific to ammo used for display on X-Net
        /// </summary>
        /// <param name="stats">string collection to append strings to</param>
        public void XNetStatistics(StringCollection stats)
        {
            stats.Add(Util.StringFormat(Strings.ITEM_STATS_CRAFT_CLIP_SIZE, Capacity));
            if (1 < ammos.Count)
            {
                foreach (string ammoName in ammos)
                {
                    ClipItemInfo clip = GetClipInfo(ammoName);
                    stats.Add(Util.StringFormat(Strings.ITEM_STATS_CLIP_DAMAGE, clip.WeaponDamage, clip.Name));
                }
            }
            else
            {
                ClipItemInfo clip = GetClipInfo(ammos[0]);
                stats.Add(Util.StringFormat(Strings.ITEM_STATS_CRAFT_CLIP_DAMAGE, clip.WeaponDamage));
            }
        }

        /// <summary>
        /// Return info on specified ammo
        /// </summary>
        /// <param name="name">type of ammo to look up</param>
        /// <returns>information on the ammo</returns>
        private static ClipItemInfo GetClipInfo(string name)
        {
            return Xenocide.StaticTables.ItemList[name] as ClipItemInfo;
        }

        #region Fields

        /// <summary>
        /// The maximum number of rounds the item can hold
        /// </summary>
        public int Capacity { get { return capacity; } }

        /// <summary>
        /// The types of ammo that can be held by item
        /// </summary>
        public IList<string> Ammos { get { return ammos; } }

        /// <summary>
        /// The maximum number of ronds the item can hold
        /// </summary>
        private int capacity;

        /// <summary>
        /// The types of ammo that can be held by item
        /// </summary>
        private List<string> ammos = new List<string>();

        #endregion Fields
    }

}
