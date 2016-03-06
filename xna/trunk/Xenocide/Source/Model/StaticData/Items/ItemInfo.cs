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
* @file ItemInfo.cs
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
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Battlescape;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// Base class holding the static attributes (loaded from XML file) for every 
    /// type of item (craft, weapon, ammo, etc.) in Xenocide
    /// </summary>
    /// <remarks>This is part of a flyweight pattern.</remarks>
    [Serializable]
    public abstract class ItemInfo
    {
        /// <summary>
        /// Construct an Item from an XML file
        /// </summary>
        /// <param name="itemNode">XML node holding data to construct ItemInfo</param>
        /// <param name="manager">Namespace used in item.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if itemNode == null")]
        protected ItemInfo(XPathNavigator itemNode, XmlNamespaceManager manager)
        {
            id    = Util.GetStringAttribute(itemNode, "id");
            name  = Util.GetStringAttribute(itemNode, "name");
            score = Util.GetFloatAttribute(itemNode,  "score");

            // storage element
            XPathNavigator storage = itemNode.SelectSingleNode("i:storage", manager);
            storageType  = Util.GetStringAttribute(storage, "type");
            storageUnits = Util.GetIntAttribute(storage,    "units");

            // price element
            XPathNavigator price = itemNode.SelectSingleNode("i:price", manager);
            buyPrice  = Util.GetIntAttribute(price, "buy");
            sellPrice = Util.GetIntAttribute(price, "sell");

            // construction details (optional)
            XPathNavigator construct = itemNode.SelectSingleNode("i:construct", manager);
            if (null != construct)
            {
                buildInfo = new BuildInfo(construct, manager);
            }

            // X-Cap details (optional)
            XPathNavigator xcapNode = itemNode.SelectSingleNode("i:xcapInfo", manager);
            if (null != xcapNode)
            {
                xCapChassis = Util.ParseEnum<Race>(Util.GetStringAttribute(xcapNode, "chassis"));
            }

            // Battlescape details (optional)
            XPathNavigator battlescapeNode = itemNode.SelectSingleNode("i:battlescapeInfo", manager);
            if (null != battlescapeNode)
            {
                battlescapeInfo = new BattlescapeInfo(battlescapeNode, manager);
            }

            // Actions item can perform
            foreach (XPathNavigator actionNode in itemNode.Select("i:actions/i:*", manager))
            {
                // ToDo: once all actions are implemented, remove the test for null
                ActionInfo actionInfo = ActionInfo.Factory(actionNode);
                if (null != actionInfo)
                {
                    actions.Add(actionInfo);
                }
            }

            // ammo that can be held
            XPathNavigator ammoNode = itemNode.SelectSingleNode("i:ammo", manager);
            if (null != ammoNode)
            {
                ammoInfo = new AmmoInfo(ammoNode, manager);
            }

            // damage item can do (if it's a weapon or ammo)
            XPathNavigator damageNode = itemNode.SelectSingleNode("i:damage", manager);
            if (null != damageNode)
            {
                damageInfo = new DamageInfo(damageNode);
            }

            // if this item is a suit of armor, link to armor
            XPathNavigator armorNode = itemNode.SelectSingleNode("i:armorInfo", manager);
            if (null != armorNode)
            {
                string armorType = Util.GetStringAttribute(armorNode, "armorType");
                armorIndex = Xenocide.StaticTables.ArmorList.IndexOf(armorType);
            }
        }

        #region Methods

        /// <summary>
        /// Return number of items of this type stored in an Outpost's inventory
        /// </summary>
        /// <remarks>This is part of a double dispatch lookup</remarks>
        /// <param name="inventory">Inventory of outpost to check</param>
        /// <returns>Number of items held</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if inventory == null")]
        public virtual int NumberInInventory(OutpostInventory inventory)
        {
            return inventory.NumberInArmory(Id);
        }

        /// <summary>
        /// Construct an Item for an item of this type, in the state it
        /// would be if just purchased
        /// </summary>
        /// <returns>constructed Item</returns>
        public virtual Item Manufacture()
        {
            if (null == ammoInfo)
            {
                // item doesn't take a magazine
                return new Item(this);
            }
            else
            {
                return new Item(this, Xenocide.StaticTables.ItemList[ammoInfo.Ammos[0]], 0);
            }
        }

        /// <summary>
        /// Construct an Item for an item of this type, in the state it
        /// would be if removed from the outpost's inventory
        /// </summary>
        /// <param name="inventory">Inventory of outpost item would come from</param>
        /// <returns>constructed Item</returns>
        public virtual Item FromOutpost(OutpostInventory inventory)
        {
            // This function should NOT be called for unique items, because we don't 
            // know which of them is wanted.
            Debug.Assert(!IsUnique);
            
            // default behaviour, item is same as newly created one.
            return Manufacture();
        }

        /// <summary>
        /// Remove item of this type from inventory
        /// </summary>
        /// <param name="inventory">Inventory to remove item from</param>
        /// <param name="item">Details of item to remove</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if inventory or item == null")]
        public virtual void Remove(OutpostInventory inventory, Item item)
        {
            inventory.Remove(item.ItemInfo, 1, true);
        }

        /// <summary>
        /// Add a item held in an Item to an outpost's storage
        /// </summary>
        /// <param name="inventory">inventory to put item into</param>
        /// <param name="item">item to add</param>
        /// <param name="spaceAlreadyRecorded">true if we've already recorded space item is using. e.g. We're finishing production/transfer</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if item or inventory == null")]
        public virtual void AddTo(OutpostInventory inventory, Item item, bool spaceAlreadyRecorded)
        {
            // put the item
            inventory.Add(this, spaceAlreadyRecorded);
            
            // if item holds ammo, and item isn't a clip, put the ammo in also
            if (item.HoldsAmmo && !item.IsClip)
            {
                // Insert the clip separately, so we know correct logic is followed
                item.AmmoInfo.AddTo(inventory, item.AmmoItem, spaceAlreadyRecorded);
            }
        }

        /// <summary>
        /// Check if all conditions are satisfied to build an instance of this item
        /// </summary>
        /// <param name="techManager">technologies available to player</param>
        /// <param name="outpost">outpost where item will be constructed</param>
        /// <param name="bank">player's available funds</param>
        /// <returns>null if conditions satisfied, otherwise string saying what's wrong</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if outpost, bank or techManager is null")]
        public string CanStartManufacture(TechnologyManager techManager, Outpost outpost, Bank bank)
        {
            if (!techManager.IsAvailable(id))
            {
                return Strings.ERROR_INSUFFICIENT_RESEARCH_FOR_BUILD;
            }
            if (bank.CurrentBalance < BuildInfo.Dollars)
            {
                return Strings.ERROR_INSUFFICIENT_FUNDS_FOR_BUILD;
            }
            if (!outpost.Inventory.CanFit(this))
            {
                return Strings.ERROR_INSUFFICIENT_SPACE_FOR_BUILT_ITEM;
            }
            return BuildInfo.CanBuildHere(outpost);
        }

        /// <summary>
        /// Update outpost's state with changes due to build starting.
        /// Namely, remove materials required, and grab space
        /// </summary>
        /// <param name="techManager">technologies available to player</param>
        /// <param name="outpost">outpost where item will be constructed</param>
        /// <param name="bank">player's available funds</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if outpost, bank or techManager is null")]
        public void StartBuild(TechnologyManager techManager, Outpost outpost, Bank bank)
        {
            Debug.Assert(null == CanStartManufacture(techManager, outpost, bank));

            // debit construction costs
            bank.Debit(BuildInfo.Dollars);

            // reserve space for finished product
            outpost.Inventory.ReserveSpace(this);

            // take everything else manufacture needs
            BuildInfo.StartBuild(outpost);
        }

        /// <summary>
        /// Update outpost's state with changes due to build finishing.
        /// Namely, release the space consumed by the build
        /// </summary>
        /// <param name="outpost">outpost to adjust</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if outpost == null")]
        public void ReleaseBuildResources(Outpost outpost)
        {
            // free the space we reserved for the finished product
            outpost.Inventory.ClearReservation(this);

            // free any other resources we're using
            BuildInfo.ReleaseBuildResources(outpost);
        }

        /// <summary>
        /// Add stats specific to this item type to string collection for display on X-Net
        /// </summary>
        /// <param name="stats">string collection to append strings to</param>
        protected abstract void XNetStatisticsCore(StringCollection stats);

        #endregion

        #region Fields

        /// <summary>
        /// Localized Name (of item) to show to player
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Internal name used by C# code to refer to this item
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// Effect this item has on score for the month
        /// </summary>
        public float Score { get { return score; } }

        /// <summary>
        /// Type of OutpostCapacity container used to hold this type of item in an outpost's inventory
        /// </summary>
        public String StorageType { get { return storageType; } }

        /// <summary>
        /// Amount of storage space an item of this type needs in a outpost's inventory
        /// </summary>
        public int StorageUnits { get { return storageUnits; } }

        /// <summary>
        /// How much it costs (in $) to buy a one of these items
        /// </summary>
        public int BuyPrice { get { return buyPrice; } }

        /// <summary>
        /// Income (in $) gained from selling one of these items
        /// </summary>
        public int SellPrice { get { return sellPrice; } }

        /// <summary>
        /// What is required for X-Corp to build this item
        /// </summary>
        /// <remarks>If null, X-Corp can't build this item</remarks>
        public BuildInfo BuildInfo { get { return buildInfo; } }

        /// <summary>
        /// Information about item when on a battlescape
        /// </summary>
        /// <remarks>If null, item doesn't exist on battlescape</remarks>
        public BattlescapeInfo BattlescapeInfo { get { return battlescapeInfo; } }

        /// <summary>
        /// Information related to a soldier carring an item
        /// </summary>
        /// <remarks>If null, soldiers can't carry this item</remarks>
        public CarryInfo CarryInfo { get { return (null == battlescapeInfo) ? null : battlescapeInfo.CarryInfo; } }

        /// <summary>
        /// Info related to ammo that this item can hold
        /// </summary>
        public AmmoInfo AmmoInfo { get { return ammoInfo; } }

        /// <summary>damage item can do (if it's a weapon or ammo)</summary>
        public DamageInfo DamageInfo { get { return damageInfo; } }

        /// <summary>
        /// Return string collection describing item statistics for display in X-Net
        /// </summary>
        public StringCollection XNetStatistics
        {
            get
            {
                StringCollection stats = new StringCollection();

                // Ammo info (if it holds ammo)
                if (null != AmmoInfo)
                {
                    AmmoInfo.XNetStatistics(stats);
                }

                // Armor info (if item is soldier armor)
                if (null != Armor)
                {
                    Armor.XNetStatistics(stats);
                }

                // Any actions item is capable of
                foreach (ActionInfo action in Actions)
                {
                    action.XNetStatistics(stats);
                }

                // get type specific stats
                XNetStatisticsCore(stats);

                return stats;
            }
        }

        /// <summary>
        /// A buy Price of zero (or less) means item can't be bought
        /// </summary>
        public bool CanPurchase { get { return (0 < BuyPrice); } }

        /// <summary>
        /// Are instances of this type of object distinct or interchangable?
        /// e.g. Each craft is unique, but a Plasma rifle identical to any other plasma rifle.
        /// </summary>
        public virtual bool IsUnique { get { return false; } }

        /// <summary>
        /// What it costs per month to keep an item of this type
        /// </summary>
        public virtual int MonthlyCharge { get { return 0; } }

        /// <summary>
        /// Is this item armor that X-Corp soldiers can wear?
        /// </summary>
        public bool IsArmor { get { return (NotArmor != armorIndex); } }

        /// <summary>
        /// If this item is armor that X-Corp soldier can wear, the armor
        /// </summary>
        public Armor Armor { get { return IsArmor ? Xenocide.StaticTables.ArmorList[armorIndex] : null; } }

        /// <summary>
        /// Returns true if this item is an X-Cap
        /// </summary>
        public bool IsXCap { get { return (Race.None != xCapChassis); } }

        /// <summary>
        /// If item is an X-Cap, then this is the X-Cap's chassis type
        /// </summary>
        /// <remarks>Ideally, this would be facored out into an XCapInfo class,
        /// but, as it has a single field, that seems overkill at the moment</remarks>
        public Race XCapChassis { get { Debug.Assert(IsXCap); return xCapChassis; } }

        /// <summary>Actions a combatant can do with this item</summary>
        public IList<ActionInfo> Actions { get { return actions; } }

        /// <summary>
        /// Localized Name (of item) to show to player
        /// </summary>
        private string name;

        /// <summary>
        /// Internal name used by C# code to refer to this type of item
        /// </summary>
        private string id;

        /// <summary>
        /// Exact meaning depends on type of item
        /// If UFO, number of points gained for destroying UFO
        /// If craft, number of points lost if craft is shot down
        /// Otherwise, it's  gain from recovering item from a battlescape
        /// </summary>
        private float score;

        /// <summary>
        /// Type of OutpostCapacity container used to hold this type of item in an outpost's inventory
        /// </summary>
        private String storageType;

        /// <summary>
        /// Amount of storage space an item of this type needs in an outpost's inventory
        /// </summary>
        private int storageUnits;

        /// <summary>
        /// How much it costs (in $) to buy a one of these items
        /// </summary>
        private int buyPrice;

        /// <summary>
        /// Income (in $) gained from selling one of these items
        /// </summary>
        private int sellPrice;

        /// <summary>
        /// What is required for X-Corp to build this item
        /// </summary>
        /// <remarks>If null, X-Corp can't build this item</remarks>
        private BuildInfo buildInfo;

        /// <summary>
        /// Information about item when on a battlescape
        /// </summary>
        /// <remarks>If null, item doesn't exist on battlescape</remarks>
        private BattlescapeInfo battlescapeInfo;

        /// <summary>
        /// Info related to ammo that this item can hold
        /// </summary>
        private AmmoInfo ammoInfo;

        /// <summary>damage item can do (if it's a weapon or ammo)</summary>
        private DamageInfo damageInfo;

        /// <summary>
        /// If this item is armor that X-Corp soldier can wear, index to the armors list
        /// </summary>
        private int armorIndex = NotArmor;

        /// <summary>
        /// If item is an X-Cap, then this is the X-Cap's chassis type
        /// </summary>
        /// <remarks>Ideally, this would be facored out into an XCapInfo class,
        /// but, as it has a single field, that seems overkill at the moment</remarks>
        private Race xCapChassis;

        /// <summary>
        /// Magic number, this item is not an armor (as opposed to "None" armor)
        /// </summary>
        public const int NotArmor = -1;

        /// <summary>Actions a combatant can do with this item</summary>
        private List<ActionInfo> actions = new List<ActionInfo>();

        #endregion
    }
}
