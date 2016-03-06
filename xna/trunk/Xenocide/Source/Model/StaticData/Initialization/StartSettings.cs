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
* @file StartSettings.cs
* @date Created: 2007/08/05
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections;
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
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape.Vehicles;

#endregion

namespace ProjectXenocide.Model.StaticData
{
    /// <summary>
    /// Assorted configuration data for putting game into starting state
    /// </summary>
    public sealed class StartSettings
    {
        /// <summary>
        /// Load the list of settings from a file
        /// </summary>
        /// <param name="filename">Name of file holding the settings</param>
        public void Populate(string filename)
        {
            facilities = new List<StartingFacility>();
            stocks     = new List<Stock>();

            // Set up XPathNavigator
            const string xmlns = "StartSettingConfig";
            XPathNavigator nav = Util.MakeValidatingXPathNavigator(filename, xmlns);
            XmlNamespaceManager manager = new XmlNamespaceManager(nav.NameTable);
            manager.AddNamespace("s", xmlns);

            // Process the facilities
            foreach (XPathNavigator facilityElement in nav.Select("/s:startSettings/s:outpostLayout/s:facility", manager))
            {
                facilities.Add(new StartingFacility(facilityElement));
            }

            // process the inventory
            foreach (XPathNavigator stockElement in nav.Select("/s:startSettings/s:inventory/s:stock", manager))
            {
                stocks.Add(new Stock(stockElement));
            }

            // process the cheats
            cheats = new Cheats(nav.SelectSingleNode("/s:startSettings/s:cheats", manager));
        }

        /// <summary>
        /// A facility in the inital outpost
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible",
            Justification = "We are C# programmers, we can handle nested classes")]
        public class StartingFacility
        {
            /// <summary>
            /// Construct from XML file
            /// </summary>
            /// <param name="node">XML node holding data</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
                Justification = "will throw if itemNode == null")]
            public StartingFacility(XPathNavigator node)
            {
                facilityId = Util.GetStringAttribute(node, "type");
                left       = Util.GetIntAttribute(node,    "left");
                top        = Util.GetIntAttribute(node,    "top");
            }

            /// <summary>
            /// Construct facility handle used to place facility of this type in outpost
            /// </summary>
            /// <returns>the handle</returns>
            public FacilityHandle Construct()
            {
                FacilityHandle handle = new FacilityHandle(facilityId, left, top);
                return handle;
            }

            #region Fields

            /// <summary>
            /// Type of facility
            /// </summary>
            private String facilityId;

            /// <summary>
            /// Topmost column of facility
            /// </summary>
            private int top;

            /// <summary>
            /// leftmost row of facility
            /// </summary>
            private int left;

            #endregion Fields
        }

        /// <summary>
        /// A quantity of items in the initial outpost
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible",
            Justification = "We are C# programmers, we can handle nested classes")]
        public class Stock : ItemLine
        {
            /// <summary>
            /// Construct from XML file
            /// </summary>
            /// <param name="node">XML node holding data</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
                Justification = "will throw if itemNode == null")]
            public Stock(XPathNavigator node) :
                base(node)
            {
            }

            /// <summary>
            /// Construct item handle used to place item of this type in outpost
            /// </summary>
            /// <returns>the handle</returns>
            public override Item Construct()
            {
                Item handle   = base.Construct();
                Aircraft   aircraft = handle as Aircraft;
                if (aircraft != null) 
                {
                    aircraft.ArmWithDefaultWeapons();
                }
                return handle;
            }
        }

        #region assorted Constants (will probably move to XML file later)

        /// <summary>
        /// Number of points the Overmind gets each day an Alien Outpost remains
        /// </summary>
        public const int AlienOutpostDailyScore = 5;

        /// <summary>
        /// Points the Overmind gets if X-Corp don't attend a terror site
        /// </summary>
        public const int TerrorizeCityAlienScore = 1000;

        /// <summary>
        /// Points X-Corp and Overmind get for every 30 minutes a craft is in the air
        /// </summary>
        public const int CraftFlyingScore = 1;

        /// <summary>
        /// Percentange chance that if a UFO is shot down, the Overmind will retaliate.
        /// </summary>
        public const int RetaliationPercentage = 2;

        /// <summary>
        /// Percentange chance that a UFO can detect an unshielded XCorp Outpost
        /// </summary>
        public const int XCorpOutpostDetectability = 25;

        /// <summary>
        /// Percentange chance that a UFO can detect a shielded XCorp Outpost
        /// </summary>
        public const int ShieldedXCorpOutpostDetectability = 1;

        /// <summary>
        /// If X-Corp debit is bigger than this 2 months in a row, player looses
        /// </summary>
        public const int LooseGameMonthlyDebit = -1000000;

        /// <summary>
        /// If X-Corp net score is less than this 2 months in a row, player looses
        /// </summary>
        public const int LooseGameMonthlyScore = -1000;

        #endregion

        #region Fields

        /// <summary>
        /// The facilities to put in the inital outpost
        /// </summary>
        public ICollection<StartingFacility> Facilities { get { return facilities; } }

        /// <summary>
        /// The supplies to put in the inital outpost
        /// </summary>
        public IList<Stock> Stocks { get { return stocks; } }

        /// <summary>
        /// The cheat codes that are active
        /// </summary>
        public Cheats Cheats { get { return cheats; } }

        /// <summary>How hard is it to win?</summary>
        public Difficulty Difficulty { get { return difficulty; } }

        /// <summary>
        /// The facilities to put in the inital outpost
        /// </summary>
        private List<StartingFacility> facilities;

        /// <summary>
        /// The supplies to put in the inital outpost
        /// </summary>
        private List<Stock> stocks;

        /// <summary>
        /// The cheat codes that are active
        /// </summary>
        private Cheats cheats;

        /// <summary>How hard is it to win?</summary>
        /// <remarks>ToDo: Allow user to change</remarks>
        private const Difficulty difficulty = Difficulty.Easy;

        #endregion Fields
    }
}
