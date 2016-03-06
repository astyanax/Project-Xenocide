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
* @file AircraftItemInfo.cs
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

using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Geoscape.Outposts;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// The different fuels aircraft can use
    /// </summary>
    public enum FuelType
    {
        /// <summary>
        /// Most craft are fueled by Xenium
        /// </summary>
        Xenium,

        /// <summary>
        /// Some older models are hydrogen fueled
        /// </summary>
        Hydrogen,
    }

    /// <summary>
    /// The non-changing information about an X-Corp Aircraft Type
    /// </summary>
    [Serializable]
    public class AircraftItemInfo : CraftItemInfo
    {
        /// <summary>
        /// Construct a AircraftItemInfo from an XML file
        /// </summary>
        /// <param name="itemNode">XML node holding data to construct AircraftItemInfo</param>
        /// <param name="manager">Namespace used in item.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if itemNode == null")]
        public AircraftItemInfo(XPathNavigator itemNode, XmlNamespaceManager manager)
            : base(itemNode, manager)
        {
            // craftInfo element
            XPathNavigator craftInfoNode = itemNode.SelectSingleNode("i:craftInfo", manager);
            XPathNavigator specsNode = craftInfoNode.SelectSingleNode("i:specs", manager);

            // engine
            XPathNavigator engine = specsNode.SelectSingleNode("i:engine", manager);
            acceleration = Util.GetIntAttribute(engine, "acceleration");
            ExtractMaxSpeed(engine);
            fuelConsumptionRate = Util.GetDoubleAttribute(engine, "consumptionRate");
            maxFuel = Util.GetIntAttribute(engine, "maxFuel");

            //... fuel type
            fuelType = StringToFuelType(Util.GetStringAttribute(engine, "fuelType"));
            ExtractHullElement(specsNode, manager);

            // maintenance
            XPathNavigator mantainance = craftInfoNode.SelectSingleNode("i:mantainance", manager);
            monthlyMaintenanceCost = Util.GetIntAttribute(mantainance, "montlyCost");
            repairCost = Util.GetIntAttribute(mantainance, "repairCost");

            // capacity
            XPathNavigator capacity = craftInfoNode.SelectSingleNode("i:capacity", manager);
            NumHardpoints = Util.GetIntAttribute(capacity, "numHardpoints");
            maxHumans = Util.GetIntAttribute(capacity, "maxHumans");
            maxXcaps = Util.GetIntAttribute(capacity, "maxXcaps");
        }

        #region Methods

        /// <summary>
        /// Construct an Item for an item of this type, in the state it
        /// would be if just purchased
        /// </summary>
        /// <returns>constructed Item</returns>
        public override Item Manufacture()
        {
            return new Aircraft(this);
        }

        /// <summary>
        /// Add an aircraft held in an Item to an outpost's storage
        /// </summary>
        /// <param name="inventory">inventory to put item into</param>
        /// <param name="item">aircraft to add</param>
        /// <param name="spaceAlreadyRecorded">true if we've already recorded space item is using. e.g. We're finishing production/transfer</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if inventory == null")]
        public override void AddTo(OutpostInventory inventory, Item item, bool spaceAlreadyRecorded)
        {
            // put the item into the outpost, casting the Item to the correct type so
            // double dereference works
            inventory.Add(item as Aircraft, spaceAlreadyRecorded);
        }

        /// <summary>
        /// Remove craft of this type from inventory
        /// </summary>
        /// <param name="inventory">Inventory to remove craft from</param>
        /// <param name="item">Details of craft to remove</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if inventory == null")]
        public override void Remove(OutpostInventory inventory, Item item)
        {
            // Remove the craft from the outpost, casting the Item to the correct type so
            // double dereference works
            inventory.Remove(item as Aircraft);
        }

        /// <summary>
        /// Add stats specific to this aircraft type to string collection for display on X-Net
        /// </summary>
        /// <param name="stats">string collection to append strings to</param>
        protected override void XNetStatisticsCore(StringCollection stats)
        {
            // Stats common to all craft (Speed & Damage Capacity)
            base.XNetStatisticsCore(stats);

            // Aircraft specific stats
            stats.Add(Util.StringFormat(Strings.ITEM_STATS_CRAFT_ACCELERATION, Acceleration));
            stats.Add(Util.StringFormat(Strings.ITEM_STATS_CRAFT_FUEL_CAPACITY, MaxFuel));
            stats.Add(Util.StringFormat(Strings.ITEM_STATS_CRAFT_WEAPON_PODS, NumHardpoints));
            stats.Add(Util.StringFormat(Strings.ITEM_STATS_CRAFT_CARGO_SPACE, MaxHumans));
            stats.Add(Util.StringFormat(Strings.ITEM_STATS_CRAFT_HWP_CAPACITY, MaxXcaps));
        }

        /// <summary>
        /// Convert the fuel type string stored in the XML file into the FuelType enumeration
        /// </summary>
        /// <param name="fuelTypeString">string from XML file to convert</param>
        /// <returns>Type of fuel used by this aircraft</returns>
        private static FuelType StringToFuelType(String fuelTypeString)
        {
            if (fuelTypeString == "hydrogen")
            {
                return FuelType.Hydrogen;
            }
            else
            {
                return FuelType.Xenium;
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// number of Gravities craft can accelerate at
        /// </summary>
        public int Acceleration { get { return acceleration; } }

        /// <summary>
        /// rate craft burns fuel at (in units/hour)
        /// </summary>
        public double FuelConsumptionRate { get { return fuelConsumptionRate; } }

        /// <summary>
        /// maximum fuel craft caries (in units)
        /// </summary>
        public int MaxFuel { get { return maxFuel; } }

        /// <summary>
        /// maximum fuel craft caries (in units)
        /// </summary>
        public FuelType FuelType { get { return fuelType; } }

        /// <summary>
        /// What it will cost (in $) to repair each point of hull damage
        /// </summary>
        public int RepairCost { get { return repairCost; } }

        /// <summary>
        /// Number of soldiers craft can carry
        /// </summary>
        public int MaxHumans { get { return maxHumans; } }

        /// <summary>
        /// Number of Xcaps craft can carry
        /// </summary>
        public int MaxXcaps { get { return maxXcaps; } }

        /// <summary>
        /// What it costs per month to keep an item of this type
        /// </summary>
        public override int MonthlyCharge { get { return monthlyMaintenanceCost; } }

        /// <summary>
        /// number of Gravities craft can accelerate at
        /// </summary>
        private int acceleration;

        /// <summary>
        /// rate craft burns fuel at (in units/hour)
        /// </summary>
        private double fuelConsumptionRate;

        /// <summary>
        /// maximum fuel craft caries (in units)
        /// </summary>
        private int maxFuel;

        /// <summary>
        /// maximum fuel craft caries (in units)
        /// </summary>
        private FuelType fuelType;

        /// <summary>
        /// What craft will cost per month (in $) to own
        /// </summary>
        private int monthlyMaintenanceCost;

        /// <summary>
        /// What it will cost (in $) to repair each point of hull damage
        /// </summary>
        private int repairCost;

        /// <summary>
        /// Number of soldiers craft can carry
        /// </summary>
        private int maxHumans;

        /// <summary>
        /// Number of Xcaps craft can carry
        /// </summary>
        private int maxXcaps;

        #endregion
    }
}
