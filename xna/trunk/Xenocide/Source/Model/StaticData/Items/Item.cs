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
* @file Item.cs
* @date Created: 2007/06/25
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
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// Base class of a concrete instance of an item
    /// Essentially, represents a "naked" item in the Geoscape
    /// </summary>
    [Serializable]
    public partial class Item
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>This constructor is for Items that don't use ammo</remarks>
        /// <param name="itemInfo">static properties of the item</param>
        public Item(ItemInfo itemInfo)
            :
            this(itemInfo, null, 0)
        {
        }

        /// <summary>
        /// Constructor for items that hold ammo (e.g. Guns and Clips)
        /// </summary>
        /// <param name="itemInfo">static properties of the item</param>
        /// <param name="ammoInfo">type of ammo being held</param>
        /// <param name="shotsLeft">number of rounds held</param>
        public Item(ItemInfo itemInfo, ItemInfo ammoInfo, int shotsLeft)
        {
            CheckItemHoldsThisAmmo(itemInfo, ammoInfo);
            this.itemInfo  = itemInfo;
            this.ammoInfo  = ammoInfo;
            this.shotsLeft = shotsLeft;
            CheckShotsLeft();
        }

        /// <summary>
        /// Called when item is sold.  Do any necessary processing here
        /// </summary>
        public virtual void OnSell()
        {
            // default is do nothing
        }

        /// <summary>
        /// Called when item is transferred.  Do any necessary processing here
        /// </summary>
        public virtual void OnTransfer()
        {
            // default is do nothing
        }

        /// <summary>
        /// Make a copy of this object
        /// </summary>
        /// <returns>the copy</returns>
        public Item Clone()
        {
            if (ItemInfo.IsUnique)
            {
                throw new ArgumentException(Strings.EXCEPTION_CLONING_UNIQUE_ITEM);
            }
            return new Item(itemInfo, ammoInfo, shotsLeft);
        }

        /// <summary>
        /// Get an item matching this item from an Outpost's inventory
        /// </summary>
        /// <param name="inventory">Where to get item(s) from</param>
        /// <param name="success">Set to false if we are unable to fully reconstruct item, otherwise left alone</param>
        /// <returns>Item (if can find)</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw when inventory is null")]
        public Item FetchFromOutpost(OutpostInventory inventory, ref bool success)
        {
            // if outpost doesn't have one of these, exit
            if (0 == inventory.NumberInInventory(itemInfo))
            {
                success = false;
                return null;
            }

            // get item from inventory
            Item temp = itemInfo.FromOutpost(inventory);
            inventory.Remove(temp);

            // and if this is an item that needs ammo, try to fully load it.
            if (HoldsAmmo)
            {
                int wanted = itemInfo.AmmoInfo.Capacity;

                // if this is a clip, it's already been loaded as much as possible
                if (!IsClip)
                {
                    temp.ammoInfo = ammoInfo;
                    // get enough ammo to fully load weapon or quantity in base
                    temp.shotsLeft = inventory.DecreaseAmmoRoundsInArmory(ammoInfo, wanted);
                }
                if (temp.shotsLeft < wanted)
                {
                    success = false;
                }
            }
            return temp;
        }

        /// <summary>
        /// Try loading this item with ammo from another item
        /// </summary>
        /// <param name="clip">item to get ammo from</param>
        /// <returns>what's left after loading this item</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if clip is null")]
        public Item Load(Item clip)
        {
            Debug.Assert(IsAmmoValid(clip));

            // if this item is out of ammo, assume same type of ammo as clip
            if (0 == shotsLeft)
            {
                ammoInfo = clip.AmmoInfo;
            }

            // is clip ammo same or different to ammo currently in this item?
            if (AmmoInfo == clip.AmmoInfo)
            {
                // ammo same. Take ammo from clip until clip empty or this item is full
                int rounds = Math.Min(ItemInfo.AmmoInfo.Capacity - shotsLeft, clip.shotsLeft);
                clip.shotsLeft -= rounds;
                shotsLeft += rounds;
                return (0 == clip.shotsLeft) ? null : clip;
            }
            else
            {
                // ammo different, eject clip, insert new one
                Debug.Assert(clip.shotsLeft <= ItemInfo.AmmoInfo.Capacity);
                Item ejectedClip = AmmoItem;
                AmmoInfo = clip.AmmoInfo;
                shotsLeft = clip.shotsLeft;
                return ejectedClip;
            }
        }

        /// <summary>If item is weapon, load the item with it's default ammo</summary>
        public void LoadDefaultAmmo()
        {
            Debug.Assert(HoldsAmmo && !IsClip);
            shotsLeft = ItemInfo.AmmoInfo.Capacity;
        }

        /// <summary>
        /// Remove the clip from this item
        /// </summary>
        /// <returns>Clip from this item (or null if item lacks a clip)</returns>
        public Item Unload()
        {
            if ((null == ammoInfo) || (0 == shotsLeft) || IsClip)
            {
                // item doesn't have a clip
                return null;
            }
            else
            {
                Item ejectedClip = AmmoItem;
                shotsLeft = 0;
                return ejectedClip;
            }
        }

        /// <summary>
        /// Can we put the ammo held in the clip into this item?
        /// </summary>
        /// <param name="clip">clip holding ammo </param>
        /// <returns>true if ammo can be used by this item</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if clip is null")]
        public bool IsAmmoValid(Item clip)
        {
            return clip.IsClip && (null != ItemInfo.AmmoInfo) && ItemInfo.AmmoInfo.IsAmmoValid(clip.ItemInfo);
        }

        /// <summary>Check if item has enough ammo for an action</summary>
        /// <param name="shots">number of shots in the action</param>
        /// <returns>true if there is sufficient ammo</returns>
        public bool HasEnoughAmmo(int shots)
        {
            // need to allow for weapons that don't use ammo
            return (!HoldsAmmo || (shots <= ShotsLeft));
        }

        /// <summary>"consume" the ammo used by an action</summary>
        /// <param name="shots">number of shots in the action</param>
        public void ConsumeAmmo(int shots)
        {
            // allow for weapons that do not hold ammo
            if (HoldsAmmo)
            {
                Debug.Assert(shots <= shotsLeft);
                shotsLeft -= shots;
            }
        }

        #region Fields

        /// <summary>
        /// The static properties of the item 
        /// </summary>
        public ItemInfo ItemInfo { get { return itemInfo; } }

        /// <summary>
        /// The ammo (if any) the item is holding
        /// </summary>
        public ItemInfo AmmoInfo { get { return ammoInfo; } set { CheckItemHoldsThisAmmo(itemInfo, value); ammoInfo = value; } }

        /// <summary>Damage done by item (or ammo in the item)</summary>
        public DamageInfo DamageInfo { get { return (null == ammoInfo) ? itemInfo.DamageInfo : ammoInfo.DamageInfo; } }

        /// <summary>
        /// Number of rounds of ammo the item has
        /// </summary>
        public int ShotsLeft { get { return shotsLeft; } set { shotsLeft = value; CheckShotsLeft(); } }

        /// <summary>
        /// Return an Item representing the clip in the item
        /// <remarks>Calling this when item lacks a clip is a mistake</remarks>
        /// </summary>
        public Item AmmoItem
        {
            get
            {
                // Check this item is holding ammo
                Debug.Assert(null != ammoInfo);

                // Check this item ISN'T a clip alreay
                Debug.Assert(!IsClip);
                return new Item(ammoInfo, ammoInfo, ShotsLeft);
            }
        }

        /// <summary>
        /// Player readable identifier for the item represented by this handle
        /// </summary>
        public virtual string Name
        {
            get
            {
                // for most items, the name is the same as the item type
                return itemInfo.Name;
            }
            protected set { /* we will need this later, when we deal with craft that have unique names */ } 
        }

        /// <summary>
        /// Can this item be removed (sold or transfered) from this outpost?
        /// </summary>
        /// <remarks>Generally, if person is working, they can't be moved</remarks>
        public virtual bool CanRemoveFromOutpost { get { return true; } }

        /// <summary>
        /// Can this item hold ammo?
        /// </summary>
        public bool HoldsAmmo { get { return (null != ammoInfo); } }

        /// <summary>
        /// Is this item a clip?
        /// </summary>
        public bool IsClip { get { return itemInfo == ammoInfo; } }

        /// <summary>
        /// The static properties of the item 
        /// </summary>
        private ItemInfo itemInfo;

        /// <summary>
        /// The ammo (if any) the item is holding
        /// </summary>
        private ItemInfo ammoInfo;

        /// <summary>
        /// Number of rounds of ammo the item has
        /// </summary>
        private int shotsLeft;

        /// <summary>
        /// Check that specified item can contain specifed Ammo
        /// </summary>
        /// <param name="itemInfo"></param>
        /// <param name="ammoInfo"></param>
        [Conditional("DEBUG")]
        private static void CheckItemHoldsThisAmmo(ItemInfo itemInfo, ItemInfo ammoInfo)
        {
            Debug.Assert((null == ammoInfo) || itemInfo.AmmoInfo.IsAmmoValid(ammoInfo));
        }

        /// <summary>
        /// Check that if someones putting ammo into item, it can take ammo
        /// </summary>
        [Conditional("DEBUG")]
        private void CheckShotsLeft()
        {
            Debug.Assert(HoldsAmmo || (0 == shotsLeft));
        }

        #endregion Fields
    }
}
