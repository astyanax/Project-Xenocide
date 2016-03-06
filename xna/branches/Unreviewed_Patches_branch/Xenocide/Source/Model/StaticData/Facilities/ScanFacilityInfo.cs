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
* @file ScanFacilityInfo.cs
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
    /// The non-changing information about a type of facility (in an X-Corp Outpost)
    /// used to search for UFOs
    /// </summary>
    public class ScanFacilityInfo : FacilityInfo
    {
        /// <summary>
        /// Construct a ScanFacilityInfo from an XML file
        /// </summary>
        /// <param name="facilityNode">XML node holding data to construct FacilityInfo</param>
        /// <param name="manager">Namespace used in facility.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if facilityNode == null")]
        public ScanFacilityInfo(XPathNavigator facilityNode, XmlNamespaceManager manager)
            : base(facilityNode, manager, true)
        {
            // scan element
            XPathNavigator scan = facilityNode.SelectSingleNode("f:scanning", manager);
            range = Util.GetIntAttribute(scan, "range");
            accuracy = Util.GetDoubleAttribute(scan, "accuracy");
            canDecodeTransmissions = Util.GetBoolAttribute(scan, "decode");
        }

        #region Methods

        /// <summary>
        /// Add stats specific to this facility type to string collection for display on X-Net
        /// </summary>
        /// <param name="stats">string collection to append strings to</param>
        protected override void XNetStatisticsCore(StringCollection stats)
        {
            stats.Add(Util.StringFormat(Strings.FACILITY_INFO_SCAN_RANGE, range));
            stats.Add(Util.StringFormat(Strings.FACILITY_INFO_SCAN_ACCURACY, accuracy));
            if (canDecodeTransmissions)
            {
                stats.Add(Strings.FACILITY_INFO_SCAN_DECODE);
            }
        }

        /// <summary>
        /// Update Outpost statistics to reflect finishing constuction of this facility
        /// </summary>
        /// <param name="statistics">statistics of outpost that building facility will modify</param>
        protected override void FinishedBuildingCore(OutpostStatistics statistics)
        {
            statistics.AddRadar(Id);
        }

        /// <summary>
        /// Update outpost statistics to reflect destroying this facility
        /// </summary>
        /// <param name="statistics">statistics of outpost that destroying facility will modify</param>
        /// <param name="finishedBuilding">was it built, or under construction capacity?</param>
        protected override void DestroyCore(OutpostStatistics statistics, bool finishedBuilding)
        {
            statistics.RemoveRadar(Id);
        }

        #endregion

        #region Fields

        /// <summary>
        /// Range (in km) that UFOs can be detected at
        /// </summary>
        public int Range { get { return range; } }

        /// <summary>
        /// probablity that once in range a UFO will be detected
        /// </summary>
        public double Accuracy { get { return accuracy; } }

        /// <summary>
        /// Can this facility tell what UFO is doing?
        /// </summary>
        public bool CanDecodeTransmissions { get { return canDecodeTransmissions; } }

        /// <summary>
        /// Range (in km) that UFOs can be detected at
        /// </summary>
        private int range;

        /// <summary>
        /// probablity that once in range a UFO will be detected
        /// </summary>
        private double accuracy;

        /// <summary>
        /// Can this facility tell what UFO is doing?
        /// </summary>
        private bool canDecodeTransmissions;

        #endregion
    }
}
