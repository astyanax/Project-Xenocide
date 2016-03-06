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
* @file StorageFacilityInfo.cs
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

using Xenocide.Resources;
using Xenocide.Utils;
using Xenocide.Model.Geoscape.HumanBases;


#endregion

namespace Xenocide.Model.StaticData.Facilities
{
    /// <summary>
    /// The non-changing information about a type of facility (in an X-Corp Base)
    /// used to "store" things
    /// </summary>
    public class StorageFacilityInfo : FacilityInfo
    {
        /// <summary>
        /// Construct a StorageFacilityInfo from an XML file
        /// </summary>
        /// <param name="facilityNode">XML node holding data to construct FacilityInfo</param>
        /// <param name="manager">Namespace used in facility.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if facilityNode == null")]
        public StorageFacilityInfo(XPathNavigator facilityNode, XmlNamespaceManager manager)
            : base(facilityNode, manager, false)
        {
            // storage element
            XPathNavigator storage = facilityNode.SelectSingleNode("f:storage", manager);
            Util.GetAttribute(storage, "type",     ref storageType);
            Util.GetAttribute(storage, "capacity", ref capacity);
        }

        #region Methods

        /// <summary>
        /// Is facility being used in this base?
        /// </summary>
        /// <param name="statistics">base facility is in</param>
        /// <returns>true if facility is being used</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
          Justification = "will throw if statistics == null")]
        public bool IsFacilityInUse(BaseStatistics statistics)
        {
            // facility is in use if base has less free capacity than this facility provides.
            BaseCapacityInfo info = statistics.Capacities[storageType];
            return ((0 < info.InUse) && (info.Available <= capacity));
        }

        /// <summary>
        /// Add stats specific to this facility type to string collection for display on X-Net
        /// </summary>
        /// <param name="stats">string collection to append strings to</param>
        protected override void XNetStatisticsCore(StringCollection stats) 
        {
            stats.Add(Util.StringFormat(Strings.FACILITY_INFO_CAPACITY, Capacity));
        }

        /// <summary>
        /// Update base statistics to reflect begining constuction of this facility
        /// </summary>
        /// <param name="statistics">statistics of base that building facility will modify</param>
        protected override void StartBuildingCore(BaseStatistics statistics)
        {
            statistics.Capacities[storageType].StartBuilding((uint)capacity);
        }

        /// <summary>
        /// Update base statistics to reflect finishing constuction of this facility
        /// </summary>
        /// <param name="statistics">statistics of base that building facility will modify</param>
        protected override void FinishedBuildingCore(BaseStatistics statistics)
        {
            statistics.Capacities[storageType].FinishedBuilding((uint)capacity);
        }

        /// <summary>
        /// Update base statistics to reflect destroying this facility
        /// </summary>
        /// <param name="statistics">statistics of base that destroying facility will modify</param>
        /// <param name="finishedBuilding">was it built, or under construction capacity?</param>
        protected override void DestroyCore(BaseStatistics statistics, bool finishedBuilding)
        {
            statistics.Capacities[storageType].Destroy((uint)capacity, finishedBuilding);
        }

        #endregion

        #region Fields

        /// <summary>
        /// Number of units of capacity facility provides
        /// </summary>
        public int Capacity { get { return capacity; } }

        /// <summary>
        /// type of item facility "stores"
        /// </summary>
        /// <remarks>ToDo: This may be replaced with an emum later</remarks>
        public string StorageType { get { return storageType; } }

        /// <summary>
        /// Number of units of capacity facility provides
        /// </summary>
        private int capacity;

        /// <summary>
        /// Type of item facility "stores"
        /// </summary>
        /// <remarks>ToDo: This may be replaced with an emum later</remarks>
        private string storageType;

        #endregion
    }
}
