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
* @file XNetStatisticsLink.cs
* @date Created: 2007/03/24
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

#endregion

namespace ProjectXenocide.Model.StaticData
{
    /// <summary>
    /// Base class for Link to other statistics information to provide with an XNet Entry.
    /// e.g. if X-Net is a weapon, this finds the properties of the weapon (ammo, cost, damage, range, etc.)
    /// </summary>
    internal class XNetStatisticsLink
    {
        /// <summary>
        /// Construct Link from information in an XML file
        /// </summary>
        /// <param name="node">node holding the info</param>
        public XNetStatisticsLink(XPathNavigator node)
        {
            objectName = (null == node) ? null : node.GetAttribute("object", String.Empty);
        }
        
        /// <summary>
        /// Get the statistics to show on X-Net
        /// </summary>
        /// <returns>The stats to show, as a string</returns>
        /// <remarks>Default is empty there are no stats to return</remarks>
        public virtual StringCollection getStatistics() 
        { 
            StringCollection collection = new StringCollection();
            return collection;
        }

        /// <summary>
        /// Factory method, constructs a XNetStatisticsLink from info in XML file
        /// </summary>
        /// <returns></returns>
        public static XNetStatisticsLink Factory(XPathNavigator node)
        {
            string objectType = (null == node) ? null : node.GetAttribute("objectType", String.Empty);
            if (objectType == "ITEM")
            {
                return new ItemXNetStatisticsLink(node);
            }
            else if (objectType == "FACILITY")
            {
                return new FacilityXNetStatisticsLink(node);
            }
            else
            {
                return new XNetStatisticsLink(node);
            }
        }

        public string ObjectName { get { return objectName; } }

        /// <summary>
        /// Name of the external object that link gets the stats for
        /// </summary>
        private string objectName;
    }

    /// <summary>
    /// Link to the statistics to show on X-Net for a base facility 
    /// </summary>
    internal class FacilityXNetStatisticsLink : XNetStatisticsLink
    {
        /// <summary>
        /// Construct Link from information in an XML file
        /// </summary>
        /// <param name="node">node holding the info</param>
        public FacilityXNetStatisticsLink(XPathNavigator node)
            : base(node)
        {
        }

        /// <summary>
        /// Get the statistics to show on X-Net
        /// </summary>
        /// <returns>The stats to show</returns>
        public override StringCollection getStatistics() 
        {
            return Xenocide.StaticTables.FacilityList[ObjectName].XNetStatistics;
        }
    }

    /// <summary>
    /// Link to the statistics to show on X-Net for an item
    /// </summary>
    internal class ItemXNetStatisticsLink : XNetStatisticsLink
    {
        /// <summary>
        /// Construct Link from information in an XML file
        /// </summary>
        /// <param name="node">node holding the info</param>
        public ItemXNetStatisticsLink(XPathNavigator node)
            : base(node)
        {
        }

        /// <summary>
        /// Get the statistics to show on X-Net
        /// </summary>
        /// <returns>The stats to show</returns>
        public override StringCollection getStatistics()
        { 
            return Xenocide.StaticTables.ItemList[ObjectName].XNetStatistics;
        }
    }
}
