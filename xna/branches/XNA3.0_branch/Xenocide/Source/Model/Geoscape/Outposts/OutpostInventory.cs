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
* @file OutpostInventory.cs
* @date Created: 2007/06/25
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

using ProjectXenocide.Utils;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape.Vehicles;

#endregion

namespace ProjectXenocide.Model.Geoscape.Outposts
{
    /// <summary>
    /// Tracks the actual items being stored in an outpost
    /// </summary>
    [Serializable]
    public partial class OutpostInventory
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacities">The storage limits of the outpost</param>
        /// <param name="outpost">The outpost that owns this inventory</param>
        public OutpostInventory(OutpostCapacities capacities, Outpost outpost)
        {
            this.capacities = capacities;
            this.outpost    = outpost;
        }

        /// <summary>
        /// Does outpost have space to fit another item of this type?
        /// </summary>
        /// <param name="item">item to put into outpost</param>
        /// <returns>true if outpost has space</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if item == null")]
        public bool CanFit(ItemInfo item)
        {
            return item.StorageUnits <= capacities[item.StorageType].Available;
        }

        /// <summary>
        /// Does outpost have space to fit another item of this type?
        /// </summary>
        /// <remarks>There is a small bug here, in that if the clip is not completely full
        /// and the base has an incomplete clip, then we don't need to allow for the mag size
        /// However, I can't be bothered fixing it, as we'll only see it when base is almost completely full
        /// </remarks>
        /// <param name="item">item to put into outpost</param>
        /// <returns>true if outpost has space</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if item == null")]
        public bool CanFit(Item item)
        {
            int space = item.ItemInfo.StorageUnits;

            // allow for any clips in item
            if (item.HoldsAmmo && !item.IsClip)
            {
                int clipSize = (item.AmmoInfo as ClipItemInfo).ClipSize;
                space += item.AmmoInfo.StorageUnits * ((item.ShotsLeft + clipSize - 1) / clipSize);
            }
            return (uint)space <= capacities[item.ItemInfo.StorageType].Available;
        }

        /// <summary>
        /// Consume storage space in outpost to hold an item
        /// </summary>
        /// <param name="item">the type of item to save space for</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if item == null")]
        public void AllocateSpace(ItemInfo item)
        {
            capacities[item.StorageType].Use((uint)item.StorageUnits);
        }

        /// <summary>
        /// Reserve storage capacity to hold item in transit when it arrives
        /// </summary>
        /// <param name="item">the type of item to reserve space for</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if item == null")]
        public void ReserveSpace(ItemInfo item)
        {
            capacities[item.StorageType].Reserve((uint)item.StorageUnits);
        }

        /// <summary>
        /// Release storage capacity reserved to hold item in transit. (Presumably the item arrived))
        /// </summary>
        /// <param name="item">the type of item space was reserved for</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if item == null")]
        public void ClearReservation(ItemInfo item)
        {
            capacities[item.StorageType].ClearReservation((uint)item.StorageUnits);
        }

        /// <summary>
        /// Recover storage space in outpost that we've allocated for holding an item
        /// </summary>
        /// <param name="item">the type of item we're removing</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if item == null")]
        public void ReleaseSpace(ItemInfo item)
        {
            capacities[item.StorageType].Release((uint)item.StorageUnits);
        }

        /// <summary>
        /// Add an item held in an Item to outpost's storage
        /// </summary>
        /// <remarks>Uses double dereference to store item</remarks>
        /// <param name="item">item to add</param>
        /// <param name="spaceAlreadyRecorded">true if we've already recorded space item is using. e.g. We're finishing production/transfer</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if item == null")]
        public void Add(Item item, bool spaceAlreadyRecorded)
        {
            item.ItemInfo.AddTo(this, item, spaceAlreadyRecorded);
        }

        /// <summary>
        /// Remove item specified by Item from outpost's storage
        /// </summary>
        /// <remarks>Uses double dereference to remove item</remarks>
        /// <param name="item">item to remove</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if item == null")]
        public void Remove(Item item)
        {
            item.ItemInfo.Remove(this, item);
        }

        /// <summary>
        /// Add an item of specific type to outpost's storage
        /// </summary>
        /// <param name="item">item to add</param>
        /// <param name="spaceAlreadyRecorded">true if we've already recorded space item is using. e.g. We're finishing production/transfer</param>
        public void Add(ItemInfo item, bool spaceAlreadyRecorded)
        {
            AddToArmory(item, spaceAlreadyRecorded, 1);
        }

        /// <summary>
        /// Add a magazine containing ammunition to outpost's storage
        /// </summary>
        /// <param name="ammo">ammo type to add</param>
        /// <param name="shotsLeft">ammo left in the magazine</param>
        /// <param name="spaceAlreadyRecorded">true if we've already recorded space item is using. e.g. We're finishing production/transfer</param>
        public void Add(ItemInfo ammo, int shotsLeft, bool spaceAlreadyRecorded)
        {
            if (0 < shotsLeft)
            {
                AddToArmory(ammo, spaceAlreadyRecorded, shotsLeft);
            }
        }

        /// <summary>
        /// Remove an item from the outpost's storage
        /// </summary>
        /// <param name="item">type of item to remove</param>
        /// <param name="quantity">should be 1, unless we're adding an ammo clip, in which case it's number of rounds</param>
        /// <param name="recoverSpace">false if removing item won't free up space.  (e.g. Partial clip)</param>
        public void Remove(ItemInfo item, int quantity, bool recoverSpace)
        {
            Debug.Assert((1 == quantity) || (item is ClipItemInfo));
            Debug.Assert(armory.ContainsKey(item.Id));

            // remove from armory
            armory[item.Id] -= quantity;
            Debug.Assert(0 <= armory[item.Id]);
            if (0 == armory[item.Id])
            {
                armory.Remove(item.Id);
            }

            // update outpost's capacity
            if (recoverSpace)
            {
                ReleaseSpace(item);
            }
        }

        /// <summary>
        /// Add a craft to outpost's storage
        /// </summary>
        /// <param name="craft">craft to add</param>
        /// <param name="spaceAlreadyRecorded">true if we've already recorded space item is using. e.g. We're finishing production/transfer</param>
        public void Add(Craft craft, bool spaceAlreadyRecorded)
        {
            Debug.Assert(!Fleet.Contains(craft), "Craft already in outpost");
            fleet.Add(craft);
            craft.HomeBase = outpost;
            if (!spaceAlreadyRecorded)
            {
                AllocateSpace(craft.ItemInfo);
            }
        }

        /// <summary>
        /// Remove craft from outpost's storage
        /// </summary>
        /// <param name="craft">craft to remove</param>
        public void Remove(Craft craft)
        {
            Debug.Assert(fleet.Contains(craft), "Craft not in outpost");
            fleet.Remove(craft);
            craft.HomeBase = null;
            ReleaseSpace(craft.ItemInfo);
        }

        /// <summary>
        /// Add a person to outpost's staff
        /// </summary>
        /// <param name="person">person to add</param>
        /// <param name="spaceAlreadyRecorded">true if we've already recorded space item is using. e.g. We're finishing production/transfer</param>
        public void Add(Person person, bool spaceAlreadyRecorded)
        {
            Debug.Assert(!staff.Contains(person), "Person already in outpost");
            staff.Add(person);
            person.Outpost = outpost;
            if (!spaceAlreadyRecorded)
            {
                AllocateSpace(person.ItemInfo);
            }
        }

        /// <summary>
        /// Remove person from outpost's staff
        /// </summary>
        /// <param name="person">person to remove</param>
        public void Remove(Person person)
        {
            Debug.Assert(staff.Contains(person), "Person not in outpost");
            Debug.Assert(null == person.Aircraft, "Deleting person assigned to aircraft");
            staff.Remove(person);
            person.Outpost = null;
            ReleaseSpace(person.ItemInfo);
        }

        /// <summary>
        /// Add a shipment to outpost
        /// </summary>
        /// <param name="shipment">shipment to add</param>
        public void Add(Shipment shipment)
        {
            Debug.Assert(!shipments.Contains(shipment), "Shipment already heading for outpost");
            shipments.Add(shipment);
        }

        /// <summary>
        /// Handle shipment arriving at outpost
        /// </summary>
        /// <param name="shipment">shipment that removed</param>
        public void OnShipmentArrive(Shipment shipment)
        {
            Debug.Assert(shipments.Contains(shipment), "Shipment not heading for outpost");
            shipments.Remove(shipment);
        }

        /// <summary>
        /// Called when outpost has been destroyed
        /// </summary>
        public void OnOutpostDestroyed()
        {
            // tell shipments.
            for (int i = shipments.Count - 1; 0 <= i; --i)
            {
                shipments[i].OnOutpostDestroyed();
                shipments.RemoveAt(i);
            }
        }

        /// <summary>
        /// Get the number of items of the specified type that are in the outpost
        /// </summary>
        /// <remarks>Use double dispatch to figure out the number</remarks>
        /// <param name="item">type of item</param>
        /// <returns>Number of items</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if item == null")]
        public int NumberInInventory(ItemInfo item)
        {
            return item.NumberInInventory(this);
        }

        /// <summary>
        /// Return the raw count of an item type in the armory.  (e.g. guns, clips, etc.)
        /// </summary>
        /// <param name="itemType">name of this type of item</param>
        /// <returns>number in items of type in armory.  in Units for guns, or Rounds (for clips)</returns>
        public int NumberInArmory(string itemType)
        {
            int number = 0;
            if (armory.ContainsKey(itemType))
            {
                number = armory[itemType];
            }
            return number;
        }

        /// <summary>
        /// Remove lesser of specified number of rounds (NOT CLIPS) from Armory, or all rounds of type in armory
        /// Used to reload partialy expended clips/magazines
        /// </summary>
        /// <param name="ammoType">Type of ammo</param>
        /// <param name="count">number of rounds</param>
        /// <returns>number of rounds removed</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification="Want to throw if ammoType is null")]
        public int DecreaseAmmoRoundsInArmory(ItemInfo ammoType, int count)
        {
            int clipSize = (ammoType as ClipItemInfo).ClipSize;
            count = Math.Min(NumberInArmory(ammoType.Id), count);
            if (0 < count)
            {
                // remove full clips
                Item item = new Item(ammoType, ammoType, clipSize);
                for (int i = 0; i < count / clipSize; ++i)
                {
                    Remove(item);
                }

                // remove partial clip
                item.ShotsLeft = count % clipSize;
                if (0 < item.ShotsLeft)
                {
                    Remove(item);
                }
            }
            return count;
        }
        
        /// <summary>
        /// Return the types of items in the base, for display on sell/transfer screens
        /// </summary>
        public IEnumerable<Item> ListContents()
        {
            foreach (Craft aircraft in Fleet)
            {
                yield return aircraft;
            }

            foreach (String itemType in armory.Keys)
            {
                if (0 < armory[itemType])
                {
                    yield return Xenocide.StaticTables.ItemList[itemType].FromOutpost(this);
                }
            }

            foreach (Person person in Staff)
            {
                yield return person;
            }
        }

        /// <summary>
        /// Total cost of all items that are in transit that have a monthly charge
        /// </summary>
        /// <returns>total cost of the items</returns>
        public int CalcInTransitMonthlyCharge()
        {
            int cost = 0;
            foreach (Shipment shipment in Shipments)
            {
                foreach (Item item in shipment.Items)
                {
                    cost += item.ItemInfo.MonthlyCharge;
                }
            }
            return cost;
        }

        /// <summary>
        /// Calculate how much the salaries of the staff in the outpost will cost this month
        /// </summary>
        /// <returns>the calculated cost</returns>
        public int CalcStaffSalaries()
        {
            int cost = 0;
            foreach (Person person in Staff)
            {
                cost += person.ItemInfo.MonthlyCharge;
            }
            return cost;
        }

        /// <summary>
        /// Calculates how many items of a specific type are in transit
        /// </summary>
        /// <param name="itemType">type of items used for calculation</param>
        /// <returns>the number of specified items</returns>
        public int CountItemsInTransit(string itemType)
        {
            int count = 0;
            foreach (Shipment shipment in Shipments)
            {
                foreach (Item item in shipment.Items)
                {
                    if (item.ItemInfo.Id == itemType)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Get list of all xcaps in base
        /// </summary>
        /// <returns>list of xcaps</returns>
        public IEnumerable<Item> ListXcaps()
        {
            foreach (String itemType in armory.Keys)
            {
                ItemInfo info = Xenocide.StaticTables.ItemList[itemType];
                if (info.IsXCap)
                {
                    yield return info.FromOutpost(this);
                }
            }
        }

        /// <summary>
        /// Get list of all people in base of specified type
        /// </summary>
        /// <param name="type">name of type (index to StaticTables.ItemList)</param>
        /// <returns>list of people</returns>
        public IEnumerable<Person> ListStaff(String type)
        {
            return Util.FilterColection(Staff,
                delegate(Person person) { return (person.ItemInfo.Id == type); });
        }

        /// <summary>
        /// Get list of all crafts in base of specified type
        /// </summary>
        /// <param name="type">name of type (index to StaticTables.ItemList)</param>
        /// <returns>list of crafts</returns>
        public IEnumerable<Craft> ListCrafts(String type)
        {
            return Util.FilterColection(Fleet,
                delegate(Craft craft) { return (craft.ItemInfo.Id == type); });
        }

        /// <summary>
        /// Get list of all people in base of specified type, who are working or not
        /// </summary>
        /// <param name="type">name of type (index to StaticTables.ItemList)</param>
        /// <param name="areWorking">want people who are working, or are idle</param>
        /// <returns>list of people</returns>
        public IEnumerable<Person> ListStaff(String type, bool areWorking)
        {
            return Util.FilterColection(Staff,
                delegate(Person person) { return (person.ItemInfo.Id == type) && (person.IsWorking == areWorking); });
        }

        /// <summary>
        /// Add a gun or clip to the armory
        /// </summary>
        /// <param name="item">type to item to add</param>
        /// <param name="spaceAlreadyRecorded">true if we've already recorded space item is using. e.g. We're finishing production/transfer</param>
        /// <param name="quantity">should be 1, unless we're adding an ammo clip, in which case it's number of rounds</param>
        private void AddToArmory(ItemInfo item, bool spaceAlreadyRecorded, int quantity)
        {
            Debug.Assert((1 == quantity) || (item is ClipItemInfo));

            // record that item is in the Armory
            if (armory.ContainsKey(item.Id))
            {
                armory[item.Id] += quantity;
            }
            else
            {
                armory[item.Id] = quantity;
            }

            // record space used, if not already done
            if (!spaceAlreadyRecorded)
            {
                AllocateSpace(item);
            }
        }

        #region Fields

        /// <summary>
        /// The aircraft owned by the outpost
        /// </summary>
        public IList<Craft> Fleet { get { return fleet; } }

        /// <summary>
        /// The people at the outpost
        /// </summary>
        public IList<Person> Staff { get { return staff; } }

        /// <summary>
        /// Shipments being sent to this outpost
        /// </summary>
        public IList<Shipment> Shipments { get { return shipments; } }

        /// <summary>
        /// The storage limits of the outpost
        /// </summary>
        private OutpostCapacities capacities;

        /// <summary>
        /// The craft owned by the outpost
        /// </summary>
        private List<Craft> fleet = new List<Craft>();

        /// <summary>
        /// The outpost that owns this inventory
        /// </summary>
        private Outpost outpost;

        /// <summary>
        /// The "standard" items, such as guns, clips, etc. in the outpost are stored here.
        /// Storage format is:
        /// string is name of type of item, as index to StaticTables.ItemList
        /// second int is count.  units for items, rounds for ammo.
        /// </summary>
        private Dictionary<String, int> armory = new Dictionary<String, int>();

        /// <summary>
        /// The people at the outpost
        /// </summary>
        private List<Person> staff = new List<Person>();

        /// <summary>
        /// Shipments being sent to this outpost
        /// </summary>
        private List<Shipment> shipments = new List<Shipment>();

        #endregion
    }
}
