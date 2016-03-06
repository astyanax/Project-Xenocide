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
* @file FacilityInfo.cs
* @date Created: 2007/04/09
* @author File creator: dteviot
* @author Credits: code is derived from previous humanfacilitytype.h
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

namespace ProjectXenocide.Model.StaticData.Facilities
{
    /// <summary>
    /// Base class holding the static attributes (loaded from XML file) for every 
    /// type of facility in an outpost.
    /// </summary>
    /// <remarks>This is part of a flyweight pattern.</remarks>
    public abstract class FacilityInfo
    {
        /// <summary>
        /// Construct a FacilityInfo from an XML file
        /// </summary>
        /// <param name="facilityNode">XML node holding data to construct FacilityInfo</param>
        /// <param name="manager">Namespace used in facility.xml</param>
        /// <param name="limitIsOnePerOutpost">Are outposts limited to having no more than one instance of this facility type each?</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if facilityNode == null")]
        protected FacilityInfo(XPathNavigator facilityNode, XmlNamespaceManager manager, bool limitIsOnePerOutpost)
        {
            id = Util.GetStringAttribute(facilityNode, "id");
            name = Util.GetStringAttribute(facilityNode, "name");

            // size element
            XPathNavigator size = facilityNode.SelectSingleNode("f:size", manager);
            xSize = Util.GetIntAttribute(size, "xSize");
            ySize = Util.GetIntAttribute(size, "ySize");

            // build element
            XPathNavigator build = facilityNode.SelectSingleNode("f:build", manager);
            buildDays = Util.GetIntAttribute(build, "time");
            buildCost = Util.GetIntAttribute(build, "cost");
            scrapRevenue = Util.GetIntAttribute(build, "scrapRevenue");
            monthlyMaintenance = Util.GetIntAttribute(build, "maintenance");

            // graphics element
            XPathNavigator graphics = facilityNode.SelectSingleNode("f:graphics", manager);
            modelName = Util.GetStringAttribute(graphics, "model");

            this.limitIsOnePerOutpost = limitIsOnePerOutpost;
        }

        #region Methods

        /// <summary>
        /// Add stats specific to this facility type to string collection for display on X-Net
        /// </summary>
        /// <param name="stats">string collection to append strings to</param>
        protected abstract void XNetStatisticsCore(StringCollection stats);

        /// <summary>
        /// Update outpost statistics to reflect begining constuction of this facility
        /// </summary>
        /// <param name="statistics">statistics of outpost that building facility will modify</param>
        public void StartBuilding(OutpostStatistics statistics)
        {
            StartBuildingCore(statistics);
        }

        /// <summary>
        /// Update outpost statistics to reflect finishing constuction of this facility
        /// </summary>
        /// <param name="statistics">statistics of outpost that building facility will modify</param>
        public void FinishedBuilding(OutpostStatistics statistics)
        {
            FinishedBuildingCore(statistics);
        }

        /// <summary>
        /// Update outpost statistics to reflect destroying this facility
        /// </summary>
        /// <param name="statistics">statistics of outpost that destroying facility will modify</param>
        /// <param name="finishedBuilding">was it built, or under construction capacity?</param>
        public void Destroy(OutpostStatistics statistics, bool finishedBuilding)
        {
            DestroyCore(statistics, finishedBuilding);
        }

        /// <summary>
        /// Update outpost statistics to reflect begining constuction of this facility
        /// </summary>
        /// <param name="statistics">statistics of outpost that building facility will modify</param>
        protected virtual void StartBuildingCore(OutpostStatistics statistics)
        {
        }

        /// <summary>
        /// Update outpost statistics to reflect finishing constuction of this facility
        /// </summary>
        /// <param name="statistics">statistics of outpost that building facility will modify</param>
        protected virtual void FinishedBuildingCore(OutpostStatistics statistics)
        {
        }

        /// <summary>
        /// Update outpost statistics to reflect destroying this facility
        /// </summary>
        /// <param name="statistics">statistics of outpost that destroying facility will modify</param>
        /// <param name="finishedBuilding">was it built, or under construction capacity?</param>
        protected virtual void DestroyCore(OutpostStatistics statistics, bool finishedBuilding)
        {
        }

        #endregion

        #region Fields

        /// <summary>
        /// Localized Name (of facility) to show to player
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Internal name used by C# code to refer to this entry
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// Width of facility, in units
        /// </summary>
        public int XSize { get { return xSize; } }

        /// <summary>
        /// Height of facility, in units
        /// </summary>
        public int YSize { get { return ySize; } }

        /// <summary>
        /// What the facility costs in maintenance each month
        /// </summary>
        public int MonthlyMaintenance { get { return monthlyMaintenance; } }

        /// <summary>
        /// Cost to build the facility
        /// </summary>
        public int BuildCost { get { return buildCost; } }

        /// <summary>
        /// Number of days it takes to build the facility
        /// </summary>
        public int BuildDays { get { return buildDays; } }

        /// <summary>
        /// Income player will get if facility destroyed
        /// </summary>
        public int ScrapRevenue { get { return scrapRevenue; } }

        /// <summary>
        /// Can the outpost have no more than one
        /// </summary>
        public bool LimitIsOnePerOutpost { get { return limitIsOnePerOutpost; } }

        /// <summary>
        /// Name of the facility's 3D model
        /// </summary>
        public String ModelName { get { return modelName; } }

        /// <summary>
        /// Return string collection describing facility statistics for display in X-Net
        /// </summary>
        public StringCollection XNetStatistics
        {
            get
            {
                StringCollection stats = new StringCollection();
                // size
                const int unitsToMeters = 25;
                int width = xSize * unitsToMeters;
                int height = ySize * unitsToMeters;
                stats.Add(Util.StringFormat(Strings.FACILITY_INFO_SIZE, width, height));

                // rest of basic stats
                stats.Add(Util.StringFormat(Strings.FACILITY_INFO_BUILD_COST, buildCost));
                stats.Add(Util.StringFormat(Strings.FACILITY_INFO_MAINTENANCE_COST, monthlyMaintenance));
                stats.Add(Util.StringFormat(Strings.FACILITY_INFO_BUILD_TIME, buildDays));
                stats.Add(Util.StringFormat(Strings.FACILITY_INFO_SCRAP_VALUE, scrapRevenue));

                // get type specific stats
                XNetStatisticsCore(stats);

                return stats;
            }
        }

        /// <summary>
        /// Localized Name (of facility) to show to player
        /// </summary>
        private string name;

        /// <summary>
        /// Internal name used by C# code to refer to this entry
        /// </summary>
        private string id;

        /// <summary>
        /// Width of facility, in units
        /// </summary>
        private int xSize;

        /// <summary>
        /// Height of facility, in units
        /// </summary>
        private int ySize;

        /// <summary>
        /// What the facility costs in maintenance each month
        /// </summary>
        private int monthlyMaintenance;

        /// <summary>
        /// Cost to build the facility
        /// </summary>
        private int buildCost;

        /// <summary>
        /// Number of days it takes to build the facility
        /// </summary>
        private int buildDays;

        /// <summary>
        /// Income player will get if facility destroyed
        /// </summary>
        private int scrapRevenue;

        /// <summary>
        /// Can the outpost have no more than one
        /// </summary>
        private bool limitIsOnePerOutpost;

        /// <summary>
        /// Name of the facility's 3D model
        /// </summary>
        private String modelName;

        #endregion
    }
}
