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
* @file SpecialFacilityInfo.cs
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

#endregion

namespace ProjectXenocide.Model.StaticData.Facilities
{
    /// <summary>
    /// The non-changing information about a type of facility (in an X-Corp Outpost)
    /// </summary>
    public class SpecialFacilityInfo : FacilityInfo
    {
        /// <summary>
        /// Construct a SpecialFacilityInfo from an XML file
        /// </summary>
        /// <param name="facilityNode">XML node holding data to construct FacilityInfo</param>
        /// <param name="manager">Namespace used in facility.xml</param>
        public SpecialFacilityInfo(XPathNavigator facilityNode, XmlNamespaceManager manager)
            : base(facilityNode, manager, true)
        {
        }

        #region Methods

        /// <summary>
        /// Add stats specific to this facility type to string collection for display on X-Net
        /// </summary>
        /// <param name="stats">string collection to append strings to</param>
        protected override void XNetStatisticsCore(StringCollection stats) 
        {
        }

        #endregion

        #region Fields

        #endregion
    }
}
