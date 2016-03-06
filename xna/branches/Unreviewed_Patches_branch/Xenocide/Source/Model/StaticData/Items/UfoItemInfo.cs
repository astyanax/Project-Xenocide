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
* @file UfoItemInfo.cs
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

using ProjectXenocide.Model.Battlescape;

#endregion

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// The non-changing information about a UFO Type
    /// </summary>
    [Serializable]
    public class UfoItemInfo : CraftItemInfo
    {
        /// <summary>
        /// Construct a UfoItemInfo from an XML file
        /// </summary>
        /// <param name="itemNode">XML node holding data to construct UfoItemInfo</param>
        /// <param name="manager">Namespace used in item.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if itemNode == null")]
        public UfoItemInfo(XPathNavigator itemNode, XmlNamespaceManager manager)
            : base(itemNode, manager)
        {
            // ufoInfo element
            XPathNavigator ufoInfo = itemNode.SelectSingleNode("i:ufoInfo", manager);
            ExtractMaxSpeed(ufoInfo);

            //... size
            ufoSize = XmlSizeStringToDisplayString(Util.GetStringAttribute(ufoInfo, "size"));

            //... and now the hull
            ExtractHullElement(ufoInfo, manager);

            // UFOs have either one or zero weapons
            XPathNavigator weaponNode = ufoInfo.SelectSingleNode("i:weapon", manager);
            if (null != weaponNode)
            {
                string weaponName = Util.GetStringAttribute(weaponNode, "name");
                weapon = (CraftWeaponItemInfo)Xenocide.StaticTables.ItemList[weaponName];
                NumHardpoints = 1;
            }

            // salvage
            salvage = new List<ItemLine>();
            foreach (XPathNavigator salvageNode in ufoInfo.Select("i:salvage/i:material", manager))
            {
                salvage.Add(new ItemLine(salvageNode));
            }

            // crewlist
            crewBuilder = new CrewBuilder(ufoInfo, manager);
        }

        /// <summary>
        /// Create the crew, for use on the battlescape
        /// </summary>
        /// <param name="race">race for the aliens</param>
        /// <param name="difficulty">game difficulty</param>
        /// <param name="ufoHealth">How shot up the UFO was, 100 == undamaged, 0 = completely destroyed</param>
        /// <returns>The aliens to put on the battlescape</returns>
        public Team CreateCrew(Race race, Difficulty difficulty, int ufoHealth)
        {
            return crewBuilder.CreateCrew(race, difficulty, ufoHealth);
        }

        #region Methods

        /// <summary>
        /// Add stats specific to this UFO to string collection for display on X-Net
        /// </summary>
        /// <param name="stats">string collection to append strings to</param>
        protected override void XNetStatisticsCore(StringCollection stats) 
        {
            // Stats common to all craft (Speed & Damage Capacity)
            base.XNetStatisticsCore(stats);

            // if UFO has a weapon, include it's stats
            if (null != Weapon)
            {
                String[] weaponStats = new String[Weapon.XNetStatistics.Count];
                Weapon.XNetStatistics.CopyTo(weaponStats, 0);
                stats.AddRange(weaponStats);
            }
        }

        /// <summary>
        /// Convert the UFO size string stored in the XML file into string that can be shown to user
        /// </summary>
        /// <param name="ufoSizeString">string from XML file to convert</param>
        /// <returns>String that can be shown to user</returns>
        private static String XmlSizeStringToDisplayString(String ufoSizeString)
        {
            // Nasty hack, but easy to implement
            return Util.LoadString("UFO_SIZE_" + ufoSizeString);
        }

        #endregion

        #region Fields

        /// <summary>
        /// Weapon craft is armed with (may be null)
        /// </summary>
        public CraftWeaponItemInfo Weapon { get { return weapon; } }

        /// <summary>
        /// size of the UFO, as user visible display string
        /// </summary>
        public String UfoSize { get { return ufoSize; } }

        /// <summary>
        /// Materials obtained if UFO is recovered
        /// </summary>
        public IList<ItemLine> Salvage { get { return salvage; } }

        /// <summary>
        /// Weapon craft is armed with (may be null)
        /// </summary>
        private CraftWeaponItemInfo weapon;

        /// <summary>
        /// size of the UFO, as user visible display string
        /// <remarks>Stored as a string, because all we do with it is show it to the user</remarks>
        /// </summary>
        private String ufoSize;

        /// <summary>
        /// Materials obtained if UFO is recovered
        /// </summary>
        private List<ItemLine> salvage;

        /// <summary>Used to construct crew, if UFO has a battlescape mission</summary>
        private CrewBuilder crewBuilder;

        #endregion
    }
}
