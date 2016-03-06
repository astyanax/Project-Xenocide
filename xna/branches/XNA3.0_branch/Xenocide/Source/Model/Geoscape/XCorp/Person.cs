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
* @file Person.cs
* @date Created: 2007/09/10
* @author File creator: dteviot
* @author Credits: nil
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;


using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Battlescape.Combatants;

#endregion


namespace ProjectXenocide.Model.Geoscape
{
    /// <summary>
    /// A person emplyed by X-Corp
    /// </summary>
    [Serializable]
    public class Person : Item, IComparable<Person>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="personType">type of person (from StaticTables.ItemList)</param>
        public Person(ItemInfo personType)
            :
            base(personType)
        {
            name = CreatePersonName(personType);

            // only soldiers can be combatants (at the moment)
            if ("ITEM_PERSON_SOLDIER" == personType.Id)
            {
                combatant = Xenocide.StaticTables.CombatantFactory.MakeXCorpSoldier();
            }
        }

        #region implement IComparable
        /// <summary>
        /// Implement IComparable&lt;T&gt;/>
        /// </summary>
        /// <param name="other">person to compare to this</param>
        /// <returns></returns>
        public Int32 CompareTo(Person other)
        {
            return this.name.CompareTo(other.name);
        }
        #endregion

        /// <summary>
        /// Changes this person's name, permanently.
        /// </summary>
        /// <param name="newName">The name to change to.</param>
        public void Rename(string newName)
        {
            name = newName;
        }

        /// <summary>
        /// Generate a name to give this person
        /// </summary>
        /// <param name="personType">type of person (from StaticTables.ItemList)</param>
        /// <returns>name</returns>
        /// <remarks>Can't do this with virtual function, because it's called in constructor</remarks>
        private static string CreatePersonName(ItemInfo personType)
        {
            // Soldiers are special case
            if ("ITEM_PERSON_SOLDIER" == personType.Id)
            {
                return Xenocide.StaticTables.PeopleNames.CreatePersonName();
            }
            else
            {
                return Xenocide.GameState.GeoData.XCorp.CreateItemName(personType);
            }
        }

        /// <summary>
        /// Adjust available capacity, due to person starting/stopping doing activity
        /// </summary>
        /// <param name="capacityName">Type of capacity that activity uses</param>
        /// <param name="starting">is person starting or stopping activity</param>
        private void AdjustOutpostCapacity(string capacityName, bool starting)
        {
            OutpostCapacityInfo capacity = outpost.Statistics.Capacities[capacityName];
            if (starting)
            {
                Debug.Assert(CanWork, "No spare capacity");
                capacity.Use(1);
            }
            else
            {
                capacity.Release(1);
            }
        }

        /// <summary>
        /// Called when item is sold.  Do any necessary processing here
        /// </summary>
        public override void OnSell()
        {
            // Strip inventory
            if (null != combatant)
            {
                combatant.Inventory.Unload(outpost.Inventory);
            }

            // behaviour is idential to being transfered
            OnTransfer();
        }

        /// <summary>
        /// Called when item is transferred.  Do any necessary processing here
        /// </summary>
        public override void OnTransfer()
        {
            Debug.Assert(CanRemoveFromOutpost);
            // if Psi Training, stop.
            if (PsiTraining)
            {
                PsiTraining = false;
            }
        }

        /// <summary>Apply one day of healing to person</summary>
        public void DailyHealing()
        {
            if (null != combatant)
            {
                combatant.DailyHealing();
            }
        }

        /// <summary>Called after battlescape if person died</summary>
        /// <param name="bodyRecovered">true if body was recovered</param>
        public void DiedOnMission(bool bodyRecovered)
        {
            if (null != combatant)
            {
                combatant.DiedOnMission(bodyRecovered, outpost.Inventory);
            }
            if (null != Aircraft)
            {
                Aircraft.Remove(this);
            }
            outpost.Inventory.Remove(this);
        }

        #region Fields

        /// <summary>
        /// Name of this person
        /// </summary>
        public override String Name { get { return name; } }

        /// <summary>
        /// Can this person be removed (fired or transfered) from this outpost?
        /// </summary>
        /// <remarks>Generally, if person is working or assigned to an aircraft, they can't be moved</remarks>
        public override bool CanRemoveFromOutpost { get { return !IsWorking && (null == Aircraft); } }

        /// <summary>
        /// Is person able to work?
        /// </summary>
        public bool CanWork
        {
            get { return 0 < outpost.Statistics.Capacities[PersonItemInfo.WorksIn].Available; }
        }

        /// <summary>
        /// Is person working at moment?
        /// </summary>
        public virtual bool IsWorking
        {
            get { return isWorking; }
            set
            {
                Debug.Assert(isWorking ^ value, "Must toggle isWorking");
                AdjustOutpostCapacity(PersonItemInfo.WorksIn, value);
                isWorking = value;
            }
        }

        /// <summary>
        /// The outpost this person is stationed in
        /// </summary>
        public Outpost Outpost
        {
            get { return outpost; }
            set
            {
                // we can't just move a person from one outpost to another
                Debug.Assert((null == value) ^ (null == outpost));
                outpost = value;
            }
        }

        /// <summary>
        /// The Item object holding the static properties of this type of person
        /// </summary>
        public PersonItemInfo PersonItemInfo { get { return ItemInfo as PersonItemInfo; } }

        /// <summary>
        /// Is person undergoing Psi Training?
        /// </summary>
        public bool PsiTraining
        {
            get { return psiTraining; }
            set
            {
                Debug.Assert(psiTraining ^ value, "Must toggle psiTraining");
                AdjustOutpostCapacity("STORAGE_PSI_TRAINING", value);
                psiTraining = value;
            }
        }

        /// <summary>
        /// Person's characteristics on a battlescape
        /// </summary>
        public Combatant Combatant { get { return combatant; } }

        /// <summary>
        /// Aircraft this person is assigned to (if assigned to an aircraft)
        /// </summary>
        public Aircraft Aircraft
        {
            get
            {
                foreach (Craft craft in outpost.Fleet)
                {
                    Aircraft aircraft = craft as Aircraft;
                    if (aircraft.Soldiers.ContainsKey(this))
                    {
                        return aircraft;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Name of this person
        /// </summary>
        private String name;

        /// <summary>
        /// Is person working at moment.
        /// </summary>
        private bool isWorking;

        /// <summary>
        /// The outpost this person is stationed in
        /// </summary>
        private Outpost outpost;

        /// <summary>
        /// Is person undergoing Psi Training?
        /// </summary>
        private bool psiTraining;

        /// <summary>
        /// Person's characteristics on a battlescape
        /// </summary>
        private Combatant combatant;

    #endregion Fields

    }
}
