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
* @file CraftClipItem.cs
* @date Created: 2007/06/17
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
using System.Drawing;
using System.Xml;
using System.Xml.XPath;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

using ProjectXenocide.Utils;

using ProjectXenocide.Model.Geoscape.Outposts;

#endregion

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// The non-changing information about ammo carried by an aircraft or soldier
    /// </summary>
    [Serializable]
    public class ClipItemInfo : ItemInfo
    {
        /// <summary>
        /// Construct a ClipItemInfo from an XML file
        /// </summary>
        /// <param name="itemNode">XML node holding data to construct ClipItemInfo</param>
        /// <param name="manager">Namespace used in item.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if itemNode == null")]
        public ClipItemInfo(XPathNavigator itemNode, XmlNamespaceManager manager)
            : base(itemNode, manager)
        {
            // damage element
            XPathNavigator damage = itemNode.SelectSingleNode("i:damage", manager);
            weaponDamage = Util.GetIntAttribute(damage, "amount");

            // Check we've got ammo
            Debug.Assert(1 == AmmoInfo.Ammos.Count);
        }

        #region Methods

        /// <summary>
        /// Return number of clips of this type stored in an Outpost's inventory
        /// </summary>
        /// <remarks>This is part of a double dispatch lookup</remarks>
        /// <param name="inventory">Inventory of outpost to check</param>
        /// <returns>Number of clips held</returns>
        public override int NumberInInventory(OutpostInventory inventory)
        {
            int rounds = base.NumberInInventory(inventory);

            // allow for last clip to be only partly loaded
            return (rounds + ClipSize - 1) / ClipSize;
        }

        /// <summary>
        /// Construct an Item for an item of this type, in the state it
        /// would be if just purchased
        /// </summary>
        /// <returns>constructed Item</returns>
        public override Item Manufacture()
        {
            return new Item(this, this, ClipSize);
        }

        /// <summary>
        /// Construct an Item for an item of this type, in the state it
        /// would be if removed from the outpost's inventory
        /// </summary>
        /// <param name="inventory">Inventory of outpost item would come from</param>
        /// <returns>constructed Item</returns>
        public override Item FromOutpost(OutpostInventory inventory)
        {
            // clip size is min of (rounds in outpost, ClipSize)
            int rounds = base.NumberInInventory(inventory);
            rounds = (int)MathHelper.Min(rounds, ClipSize);
            return new Item(this, this, rounds);
        }

        /// <summary>
        /// Remove clip of this type from inventory
        /// </summary>
        /// <param name="inventory">Inventory to remove clip from</param>
        /// <param name="item">Details of clip to remove</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if item or inventory == null")]
        public override void Remove(OutpostInventory inventory, Item item)
        {
            // figure out if we're removing a partial clip
            int looseRounds = base.NumberInInventory(inventory) % ClipSize;
            if (0 == looseRounds)
            {
                looseRounds = ClipSize;
            }
            bool recoverSpace = (looseRounds <= item.ShotsLeft);
            inventory.Remove(item.ItemInfo, item.ShotsLeft, recoverSpace);
        }

        /// <summary>
        /// Add a item held in an Item to an outpost's storage
        /// </summary>
        /// <param name="inventory">inventory to put item into</param>
        /// <param name="item">item to add</param>
        /// <param name="spaceAlreadyRecorded">true if we've already recorded space item is using. e.g. We're finishing production/transfer</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if item or inventory == null")]
        public override void AddTo(OutpostInventory inventory, Item item, bool spaceAlreadyRecorded)
        {
            // allow for weapon holding more ammo than fits in a clip
            while (ClipSize < item.ShotsLeft)
            {
                inventory.Add(this, ClipSize, spaceAlreadyRecorded);
                item.ShotsLeft -= ClipSize;
            }
            
            // if this is a partly full clip, we may already have allocated space for it
            int looseRounds = base.NumberInInventory(inventory) % ClipSize;
            if ((0 < looseRounds) && ((looseRounds + item.ShotsLeft) <= ClipSize))
            {
                spaceAlreadyRecorded = true;
            }

            inventory.Add(this, item.ShotsLeft, spaceAlreadyRecorded);
        }

        /// <summary>
        /// Add stats specific to this Ammo to string collection for display on X-Net
        /// </summary>
        /// <param name="stats">string collection to append strings to</param>
        protected override void XNetStatisticsCore(StringCollection stats) {}

        #endregion

        #region Fields

        /// <summary>
        /// Maximum number of rounds in ammo clip
        /// </summary>
        public int ClipSize { get { return AmmoInfo.Capacity; } }

        /// <summary>
        /// damage (in hull points) inflicted by weapon
        /// </summary>
        public int WeaponDamage { get { return weaponDamage; } }

        /// <summary>
        /// damage (in hull points) inflicted by weapon
        /// </summary>
        private int weaponDamage;

        #endregion
    }
}
