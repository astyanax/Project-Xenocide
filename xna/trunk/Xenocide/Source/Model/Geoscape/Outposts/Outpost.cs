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
* @file Outpost.cs
* @date Created: 2007/02/04
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.StaticData.Facilities;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Utils;

#endregion

namespace ProjectXenocide.Model.Geoscape.Outposts
{
    /// <summary>
    /// An X-Corp Base
    /// </summary>
    [Serializable]
    public class Outpost : IGeoPosition
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Outpost(GeoPosition position, String name) 
        {
            this.position = position;
            this.name     = name;

            statistics = new OutpostStatistics();
            floorplan  = new Floorplan(statistics);
            inventory  = new OutpostInventory(statistics.Capacities, this);
        }

        /// <summary>
        /// Update the outpost's state, to allow for passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        /// <remarks>At moment, just pump the aircraft and facilities owned by the outpost.</remarks>
        public void Update(double milliseconds)
        {
            // can't use foreach, because aircraft may be removed from collection
            for (int i = Fleet.Count - 1; 0 <= i; --i)
            {
                Fleet[i].Update(milliseconds);
            }
        }

        /// <summary>
        /// Configure the first outpost the player gets
        /// </summary>
        public void SetupPlayersFirstBase()
        {
            // add facilities to outpost
            floorplan.SetupPlayersFirstBase();

            // stock base with craft and inventory
            EquipFirstOutpostWithSupplies();

            // assign soldiers to first craft in outpost that can carry them
            AssignStartingSoldiersToTransportAircraft();
        }

        /// <summary>
        /// Calculate how much the maintenance of the facilities in the outpost will cost this month
        /// </summary>
        /// <returns>the calculated cost</returns>
        public int CalcFacilityMaintenance()
        {
            return floorplan.CalcFacilityMaintenance();
        }

        /// <summary>
        /// Calculate how much the maintenance of the craft in the outpost will cost this month
        /// </summary>
        /// <returns>the calculated cost</returns>
        public int CalcCraftMaintenance()
        {
            int cost = 0;
            foreach (Craft a in Fleet)
            {
                cost += a.ItemInfo.MonthlyCharge;
            }
            return cost;
        }

        /// <summary>
        /// Calculate how much the salaries of the staff in the outpost will cost this month
        /// </summary>
        /// <returns>the calculated cost</returns>
        public int CalcStaffSalaries()
        {
            return inventory.CalcStaffSalaries();
        }

        /// <summary>
        /// Cost of all items that are in transit, that have a monthly charge
        /// </summary>
        /// <returns>total cost of the items</returns>
        public int CalcInTransitMonthlyCharge()
        {
            return inventory.CalcInTransitMonthlyCharge();
        }

        /// <summary>
        /// An aircraft has been destroyed (in combat)
        /// </summary>
        /// <param name="aircraft">craft that has been destroyed</param>
        public void OnCraftDestroyed(Craft aircraft)
        {
            inventory.Remove(aircraft);
        }

        /// <summary>
        /// Is a UFO showing on the Base's radar?
        /// </summary>
        /// <param name="ufoPosition">Position of UFO to check</param>
        /// <param name="alreadySeen">was UFO visible last tick?</param>
        /// <returns>true if UFO is showing</returns>
        public bool IsOnRadar(GeoPosition ufoPosition, bool alreadySeen)
        {
            return Statistics.IsOnRadar(Position, ufoPosition, alreadySeen);
        }

        /// <summary>
        /// Percentage chance that a UFO will detect this base
        /// </summary>
        /// <returns>Percentage chance that a UFO will detect this base</returns>
        public int Detectability()
        {
            // if base has a working neural shield, then odds of detection are 1%, otherwise they're 25%
            if (floorplan.HasWorkingFacility("FAC_NEURAL_SHIELDING_FACILITY"))
            {
                return StartSettings.ShieldedXCorpOutpostDetectability;
            }
            else
            {
                return StartSettings.XCorpOutpostDetectability;
            }
        }

        /// <summary>
        /// Outpost tries to shoot down attacking UFO
        /// </summary>
        /// <param name="craft">craft attacking the base</param>
        /// <param name="log">record of combat</param>
        /// <returns>result of attempt</returns>
        public AttackResult Attack(Craft craft, BattleLog log)
        {
            // if base has grav shield, every facility gets 2 shots, otherwise 1.
            int numShots = 1;
            if (floorplan.HasWorkingFacility("FAC_GRAVITY_SHIELD_FACILITY"))
            {
                numShots = 2;
            }

            // let each facility shoot at UFO
            AttackResult result = AttackResult.Nothing;
            for (int i = 0; i < numShots; ++i)
            {
                foreach (FacilityHandle handle in Floorplan.Facilities)
                {
                    DefenseFacilityInfo facility = handle.FacilityInfo as DefenseFacilityInfo;
                    if (null != facility)
                    {
                        result = facility.Shoot(craft, log);
                        if ((AttackResult.OpponentCrashed == result) || (AttackResult.OpponentDestroyed == result))
                        {
                            return result;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Outpost has been destroyed
        /// </summary>
        public void OnDestroyed()
        {
            // eliminate craft
            // ToDo: might only destroy craft in the base, assign other craft to surviving outposts
            // Also, may need to cleanup soldiers assigned to craft
            Fleet.Clear();

            // Remove scientists from any projects they were working on
            Xenocide.GameState.GeoData.XCorp.ResearchManager.OnOutpostDestroyed(this);

            // Cancel any manufacturing activity in outpost
            buildProjectManager.OnOutpostDestroyed();

            // elminiate facilities, statistics and inventory
            inventory.OnOutpostDestroyed();
            statistics.OnOutpostDestroyed();
            floorplan.OnOutpostDestroyed();
            statistics = null;
            inventory  = null;

            // eliminate outpost itself
            Xenocide.GameState.GeoData.Outposts.Remove(this);
        }

        /// <summary>
        /// Get list of all Xcaps in base
        /// </summary>
        /// <returns>list of xcaps</returns>
        public IEnumerable<Item> ListXcaps()
        {
            return inventory.ListXcaps();
        }

        /// <summary>
        /// Get list of all people in base of specified type
        /// </summary>
        /// <param name="type">name of type (index to StaticTables.ItemList)</param>
        /// <returns>list of people</returns>
        public IEnumerable<Person> ListStaff(String type)
        {
            return inventory.ListStaff(type);
        }

        /// <summary>
        /// Get list of all crafts in base of specified type
        /// </summary>
        /// <param name="type">name of type (index to StaticTables.ItemList)</param>
        /// <returns>list of crafts</returns>
        public IEnumerable<Craft> ListCrafts(String type)
        {
            return inventory.ListCrafts(type);
        }

        /// <summary>
        /// Get list of all people in base of specified type, who are working or not
        /// </summary>
        /// <param name="type">name of type (index to StaticTables.ItemList)</param>
        /// <param name="areWorking">want people who are working, or are idle</param>
        /// <returns>list of people</returns>
        public IEnumerable<Person> ListStaff(String type, bool areWorking)
        {
            return inventory.ListStaff(type, areWorking);
        }

        /// <summary>
        /// Gets the number of items specified that are in transit
        /// </summary>
        /// <param name="type">name of type (index to StaticTables.ItemList)</param>
        /// <returns>number of items in shipments</returns>
        public int CountItemsInTransit(string type)
        {
            return inventory.CountItemsInTransit(type);
        }

        /// <summary>Apply one day of healing to all injured staff</summary>
        public void HealInjuredStaff()
        {
            foreach (Person person in inventory.Staff)
            {
                person.DailyHealing();
            }
        }

        /// <summary>
        /// Equip outpost with the supplies user initially starts with
        /// </summary>
        private void EquipFirstOutpostWithSupplies()
        {
            foreach (StartSettings.Stock item in Xenocide.StaticTables.StartSettings.Stocks)
            {
                for (int i = 0; i < item.Quantity; ++i)
                {
                    Inventory.Add(item.Construct(), false);
                }
            }
        }

        // assign soldiers to first craft in outpost that can carry them
        private void AssignStartingSoldiersToTransportAircraft()
        {
            // find first aircraft that can carry troops
            foreach (Craft craft in Fleet)
            {
                if (craft.CanCarrySoldiers)
                {
                    // assign soldiers to the craft
                    Aircraft aircraft = craft as Aircraft;
                    int i = aircraft.MaxHumans;
                    foreach (Person person in ListStaff("ITEM_PERSON_SOLDIER"))
                    {
                        // if craft is full, exit
                        if (0 == i)
                        {
                            break;
                        }
                        --i;
                        aircraft.Soldiers[person] = (aircraft.MaxHumans - i);
                    }
                    break;
                }
            }
        }

        #region Fields

        /// <summary>
        /// Where the outpost is on the globe
        /// </summary>
        public GeoPosition Position { get { return position; } set { position = value; } }

        /// <summary>
        /// The aircraft owned by the outpost
        /// </summary>
        public IList<Craft> Fleet { get { return inventory.Fleet; } }
        
        /// <summary>
        /// Where the outpost is on the globe
        /// </summary>
        private GeoPosition position;

        /// <summary>
        /// Layout of facilities in the outpost
        /// </summary>
        public Floorplan Floorplan { get { return floorplan; } }

        /// <summary>
        /// Layout of facilities in the outpost
        /// </summary>
        private Floorplan floorplan;

        /// <summary>
        /// The Name this outpost has been given
        /// </summary>
        public String Name { get { return name; } set { name = value; } }

        /// <summary>
        /// The Name this outpost has been given
        /// </summary>
        private String name;

        /// <summary>
        /// The capabilities of the outpost
        /// </summary>
        public OutpostStatistics Statistics { get { return statistics; } }

        /// <summary>
        /// Items being manufactured in this outpost
        /// </summary>
        public BuildProjectManager BuildProjectManager { get { return buildProjectManager; } }

        /// <summary>
        /// The capabilities of the outpost
        /// </summary>
        private OutpostStatistics statistics;

        /// <summary>
        /// The items stored in the outpost
        /// </summary>
        public OutpostInventory Inventory { get { return inventory; } }

        /// <summary>
        /// IGeoPosition interface
        /// </summary>
        /// <returns>true</returns>
        public bool IsKnownToXCorp { get { return true; } }

        /// <summary>
        /// The items stored in the outpost
        /// </summary>
        private OutpostInventory inventory;

        /// <summary>
        /// Items being manufactured in this outpost
        /// </summary>
        private BuildProjectManager buildProjectManager = new BuildProjectManager();

        #endregion Fields
    }
}
