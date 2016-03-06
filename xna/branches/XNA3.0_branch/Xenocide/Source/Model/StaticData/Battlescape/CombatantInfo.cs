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
* @file CombatantInfo.cs
* @date Created: 2007/12/26
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
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Battlescape.Combatants;

#endregion

namespace ProjectXenocide.Model.StaticData.Battlescape
{
        /// <summary>
        /// Information needed to build a specific type of combatant
        /// </summary>
    [Serializable]
    public partial class CombatantInfo
    {
        /// <summary>
        /// Load list of CombatantInfo from an XML file
        /// </summary>
        /// <param name="node">XML node holding data to construct CombatantInfo</param>
        /// <param name="manager">Namespace used in combatant.xml</param>
        public CombatantInfo(XPathNavigator node, XmlNamespaceManager manager)
        {
            this.race       = Util.ParseEnum<Race>(Util.GetStringAttribute(node, "race"));
            this.rank       = Util.ParseEnum<AlienRank>(Util.GetStringAttribute(node, "rank"));
            this.armorIndex = Xenocide.StaticTables.ArmorList.IndexOf(Util.GetStringAttribute(node, "armor"));
            this.ownerRace = race;
            if (Util.AttributePresent(node, "ownerRace"))
            {
                this.ownerRace = Util.ParseEnum<Race>(Util.GetStringAttribute(node, "ownerRace"));
            }

            XPathNavigator itemrefNode = node.SelectSingleNode("c:itemref", manager);
            if (null != itemrefNode)
            {
                this.deadItemId = Xenocide.StaticTables.ItemList.IndexOf(Util.GetStringAttribute(itemrefNode, "dead"));
                this.stunItemId = Xenocide.StaticTables.ItemList.IndexOf(Util.GetStringAttribute(itemrefNode, "stunned"));
            }

            XPathNavigator attributesNode = node.SelectSingleNode("c:attributes", manager);
            flyer = Util.GetBoolAttribute(attributesNode, "flyer");
            string carrier = Util.GetStringAttribute(attributesNode, "carrier");
            config = Util.ParseEnum<InventoryLayout.Config>(carrier);

            // parse statistics (called attributes)
            foreach (XPathNavigator attributeNode in attributesNode.SelectChildren(XPathNodeType.Element))
            {
                Statistic statistic = Util.ParseEnum<Statistic>(attributeNode.Name);
                string type = attributeNode.GetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance");
                Distribution distribution = null;
                if ("core" == type)
                {
                    distribution = new CoreDistribution(attributeNode);
                }
                else if ("normal" == type)
                {
                    distribution = new NormalDistribution(attributeNode);
                }
                else if ("uniform" == type)
                {
                    distribution = new UniformDistribution(attributeNode);
                }
                statistics.Add(statistic, distribution);
            }

            // graphic element
            XPathNavigator graphicNode = node.SelectSingleNode("c:graphics", manager);
            if (null != graphicNode)
            {
                graphic = new Graphic(graphicNode);
            }

            // Possible loadouts this combatant could have
            foreach (XPathNavigator equipementNode in node.Select("c:equipment", manager))
            {
                loadoutChoices.Add( new LoadoutChoice(equipementNode));
            }
        }

        /// <summary>Give combatant's statistics their starting values</summary>
        /// <param name="stats">where to put the statistics</param>
        public void GenerateStats(Stats stats)
        {
            foreach (KeyValuePair<Statistic, Distribution> pair in statistics)
            {
                stats.SetInitialValue(pair.Key, pair.Value.MakeValue());
            }
        }

        /// <summary>Pick a loadout to initially equip combatant with</summary>
        /// <returns>name of the loadout, or empty string if none</returns>
        public string PickInitialLoadout()
        {
            if (0 < loadoutChoices.Count)
            {
                return Util.SelectRandom(loadoutChoices).LoadoutName;
            }
            // if get here, no loadout
            return String.Empty;
        }

        #region Fields

        /// <summary>Combatant's Race</summary>
        public Race Race { get { return race; } }

        /// <summary>Race that uses this combatant (used for terrorists)</summary>
        public Race OwnerRace { get { return ownerRace; } }

        /// <summary>Combatant's Rank</summary>
        public AlienRank Rank { get { return rank; } }

        /// <summary>details of 3D model to show on battlescape</summary>
        public Graphic Graphic { get { return graphic; } }

        /// <summary>Can unit fly?</summary>
        public bool Flyer { get { return flyer; } }

        /// <summary>Type of InventoryLayout combatant has</summary>
        public InventoryLayout.Config Config { get { return config; } }

        /// <summary>Index to default armor the combatant "wears"</summary>
        public int ArmorIndex { get { return armorIndex; } }

        /// <summary>Index to Items for a corpse this alien in storage in a base</summary>
        public int DeadItemId { get { return deadItemId; } }

        /// <summary>Index to Items for a living alien in storage in a base</summary>
        public int StunItemId { get { return stunItemId; } }

        /// <summary>Points for killing this type of combatant</summary>
        public int VictoryPoints { get { return statistics[Statistic.VictoryPoints].MakeValue(); } }

        /// <summary>Combatant's Race</summary>
        private Race race;

        /// <summary>Race that uses this combatant (used for terrorists)</summary>
        private Race ownerRace;

        /// <summary>Combatant's Rank</summary>
        private AlienRank rank;

        /// <summary>details of 3D model to show on battlescape</summary>
        private Graphic graphic;

        /// <summary>Generators for the Statistic values</summary>
        [NonSerialized]
        private Dictionary<Statistic, Distribution> statistics = new Dictionary<Statistic, Distribution>();

        /// <summary>Can unit fly?</summary>
        private bool flyer;

        /// <summary>Type of InventoryLayout combatant has</summary>
        private InventoryLayout.Config config;

        /// <summary>Possible loadouts this combatant could be initially equiped with</summary>
        private List<LoadoutChoice> loadoutChoices = new List<LoadoutChoice>();

        /// <summary>Index to default armor the combatant "wears"</summary>
        private int armorIndex;

        /// <summary>Index to Items for a corpse this alien in storage in a base</summary>
        private int deadItemId;

        /// <summary>Index to Items for a living alien in storage in a base</summary>
        private int stunItemId;

        #endregion
    }
}
