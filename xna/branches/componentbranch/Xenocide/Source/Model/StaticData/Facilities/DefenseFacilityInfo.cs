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

#endregion

namespace Xenocide.Model.StaticData.Facilities
{
    /// <summary>
    /// The non-changing information about a type of facility (in an X-Corp Base)
    /// used to defend the base from attack by UFOs
    /// </summary>
    public class DefenseFacilityInfo : FacilityInfo
    {
        /// <summary>
        /// Construct a DefenseFacilityInfo from an XML file
        /// </summary>
        /// <param name="facilityNode">XML node holding data to construct FacilityInfo</param>
        /// <param name="manager">Namespace used in facility.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if facilityNode == null")]
        public DefenseFacilityInfo(XPathNavigator facilityNode, XmlNamespaceManager manager)
            : base(facilityNode, manager, false)
        {
            // defense element
            XPathNavigator defense = facilityNode.SelectSingleNode("f:defense", manager);
            Util.GetAttribute(defense, "range",    ref range);
            Util.GetAttribute(defense, "accuracy", ref accuracy);
            Util.GetAttribute(defense, "damage",   ref damage);
        }

        #region Methods

        /// <summary>
        /// Add stats specific to this facility type to string collection for display on X-Net
        /// </summary>
        /// <param name="stats">string collection to append strings to</param>
        protected override void XNetStatisticsCore(StringCollection stats) 
        {
            stats.Add(Util.StringFormat(Strings.FACILITY_INFO_DEFENSE_RANGE,    range));
            stats.Add(Util.StringFormat(Strings.FACILITY_INFO_DEFENSE_ACCURACY, accuracy));
            stats.Add(Util.StringFormat(Strings.FACILITY_INFO_DEFENSE_DAMAGE,   damage));
        }

        #endregion

        #region Fields

        /// <summary>
        /// maximum range (in km) that we can shoot at UFOs
        /// </summary>
        public int Range { get { return range; } }

        /// <summary>
        /// probablity that shot will hit UFO
        /// </summary>
        public double Accuracy { get { return accuracy; } }

        /// <summary>
        /// damage shot will infict on UFO (if it hits)
        /// </summary>
        public int Damage { get { return damage; } }

        /// <summary>
        /// maximum range (in km) that we can shoot at UFOs
        /// </summary>
        private int range;

        /// <summary>
        /// probablity that shot will hit UFO
        /// </summary>
        private double accuracy;

        /// <summary>
        /// damage shot will infict on UFO (if it hits)
        /// </summary>
        private int damage;


        #endregion
    }
}
