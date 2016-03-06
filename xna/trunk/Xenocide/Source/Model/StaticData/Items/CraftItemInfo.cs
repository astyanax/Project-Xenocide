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
* @file CraftItemInfo.cs
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

using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// The non-changing information that is common between UFOs and X-Corp aircraft
    /// </summary>
    [Serializable]
    public abstract class CraftItemInfo : ItemInfo
    {
        /// <summary>
        /// Construct a CraftItemInfo from an XML file
        /// </summary>
        /// <param name="itemNode">XML node holding data to construct CraftItemInfo</param>
        /// <param name="manager">Namespace used in item.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if itemNode == null")]
        protected CraftItemInfo(XPathNavigator itemNode, XmlNamespaceManager manager)
            : base(itemNode, manager)
        {
        }

        /// <summary>
        /// Return number of craft of specified type owned by an an Outpost
        /// </summary>
        /// <remarks>This is part of a double dispatch lookup</remarks>
        /// <param name="inventory">Inventory of outpost to check</param>
        /// <returns>Number of craft</returns>
        public override int NumberInInventory(OutpostInventory inventory)
        {
            int count = 0;
            foreach (Craft c in inventory.Fleet)
            {
                if (c.ItemInfo.Id == Id)
                {
                    ++count;
                }
            }
            return count;
        }

        #region Methods

        /// <summary>
        /// Extract the maxSpeed attribute from the XML node
        /// </summary>
        /// <param name="node">XML node containing the maxSpeed attribute</param>
        protected void ExtractMaxSpeed(XPathNavigator node)
        {
            maxSpeed = Util.GetIntAttribute(node, "maxSpeed");
        }
        
        /// <summary>
        /// Extract the hull properties from the XML node
        /// </summary>
        /// <param name="node">XML node containing a "hull" node</param>
        /// <param name="manager">Namespace used in item.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if node == null")]
        protected void ExtractHullElement(XPathNavigator node, XmlNamespaceManager manager)
        {
            XPathNavigator hull = node.SelectSingleNode("i:hull", manager);
            hullHardness = Util.GetDoubleAttribute(hull, "hardness");
            maxDamage    = Util.GetIntAttribute(hull,    "maxDamage");
        }

        /// <summary>
        /// Add stats specific to this craft type to string collection for display on X-Net
        /// </summary>
        /// <param name="stats">string collection to append strings to</param>
        protected override void XNetStatisticsCore(StringCollection stats)
        {
            stats.Add(Util.StringFormat(Strings.ITEM_STATS_CRAFT_MAX_SPEED, MaxSpeed));
            stats.Add(Util.StringFormat(Strings.ITEM_STATS_CRAFT_DAMAGE_CAPACITY, MaxDamage));
        }

        #endregion

        #region Fields

        /// <summary>
        /// Maximum speed craft can travel at (in meters/second)
        /// </summary>
        public    int MaxSpeed { get { return maxSpeed; } }

        /// <summary>
        /// Hull's resistance to damage
        /// </summary>
        public double HullHardness { get { return hullHardness; } }

        /// <summary>
        /// Maximum damage craft can take before being destroyed
        /// </summary>
        public int MaxDamage { get { return maxDamage; } }

        /// <summary>
        /// Number of weapon pods craft can carry
        /// </summary>
        public int NumHardpoints { get { return numHardpoints; } protected set { numHardpoints = value; } }

        /// <summary>
        /// Are instances of this type of object distinct or interchangable?
        /// e.g. Each craft is unique, but a Plasma rifle identical to any other plasma rifle.
        /// </summary>
        public override bool IsUnique { get { return true; } }

        /// <summary>
        /// Maximum speed craft can travel at (in meters/second)
        /// </summary>
        private int maxSpeed;

        /// <summary>
        /// Hull's resistance to damage
        /// </summary>
        private double hullHardness;

        /// <summary>
        /// Maximum damage craft can take before being destroyed
        /// </summary>
        private int maxDamage;

        /// <summary>
        /// Number of weapon pods craft can carry
        /// </summary>
        private int numHardpoints;

        #endregion
    }
}
