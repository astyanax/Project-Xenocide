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
* @file Armor.cs
* @date Created: 2007/12/08
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
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.StaticData.Items;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.StaticData.Battlescape
{
    /// <summary>
    /// The details of the armor "worn" by a combatant
    /// </summary>
    public partial class Armor
    {
        /// <summary>
        /// Construct an Armor from an XML file
        /// </summary>
        /// <param name="armorNode">XML node holding data to construct Armor</param>
        /// <param name="manager">Namespace used in combatant.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if armorNode is null")]
        public Armor(XPathNavigator armorNode, XmlNamespaceManager manager)
        {
            id    = Util.GetStringAttribute(armorNode, "id");
            flyer = Util.GetBoolAttribute(  armorNode, "flyer");
            plates[(int)Side.Front] = Util.GetIntAttribute(armorNode, "front");
            plates[(int)Side.Side]  = Util.GetIntAttribute(armorNode, "side");
            plates[(int)Side.Rear]  = Util.GetIntAttribute(armorNode, "rear");
            plates[(int)Side.Under] = Util.GetIntAttribute(armorNode, "under");

            // parse susceptibilities
            foreach (XPathNavigator node in armorNode.Select("c:susceptibility", manager))
            {
                string typeName = Util.GetStringAttribute(node, "damageType");
                susceptibilities[(int)Enum.Parse(typeof(DamageType), typeName)] = new Susceptibility(node);
            }
        }

        /// <summary>
        /// The directions an attack can come from
        /// </summary>
        public enum Side
        {
            /// <summary>
            /// The frontal armor
            /// </summary>
            Front,

            /// <summary>
            /// The left and right side armor
            /// </summary>
            Side,

            /// <summary>
            /// The back armor
            /// </summary>
            Rear,

            /// <summary>
            /// The underneath armor
            /// </summary>
            Under
        }

        #region Methods

        /// <summary>
        /// Add stats specific to armor for display on X-Net
        /// </summary>
        /// <param name="stats">string collection to append strings to</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if stats is null")]
        public void XNetStatistics(StringCollection stats)
        {
            stats.Add(Util.StringFormat(Strings.ITEM_STATS_ARMOR_FRONT, plates[(int)Side.Front]));
            stats.Add(Util.StringFormat(Strings.ITEM_STATS_ARMOR_SIDE,  plates[(int)Side.Side]));
            stats.Add(Util.StringFormat(Strings.ITEM_STATS_ARMOR_REAR,  plates[(int)Side.Rear]));
            stats.Add(Util.StringFormat(Strings.ITEM_STATS_ARMOR_UNDER, plates[(int)Side.Under]));
        }

        /// <summary>
        /// Calcluate the amount of damage inflicted by an attack
        /// </summary>
        /// <param name="damageInfo">details of weapon attacking</param>
        /// <param name="side">where attack hit armor</param>
        /// <returns>Damage inflicted.  X = points of physical damage, Y = stun</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if damageInfo is null")]
        public Vector2 DamageInflicted(DamageInfo damageInfo, Side side)
        {
            int points = damageInfo.Points;
            points -= plates[(int)side];
            if (points < 0)
            {
                points = 0;
            }
            return susceptibilities[(int)damageInfo.DamageType].DamageInflicted(points);
        }

        #endregion

        /// <summary>
        /// The number of points of physical and stun damage a point of a specific 
        /// damage type will inflict on a combatant
        /// </summary>
        private struct Susceptibility
        {
            /// <summary>
            /// ctor
            /// </summary>
            /// <param name="node">XML node holding data to construct Susceptibility</param>
            public Susceptibility(XPathNavigator node)
            {
                this.physical = Util.GetFloatAttribute(node, "physical");
                this.stun     = Util.GetFloatAttribute(node, "stun");
            }

            /// <summary>
            /// Calcluate the amount of damage inflicted by an attack
            /// </summary>
            /// <param name="points">strength of the attack</param>
            /// <returns>Damage inflicted.  X = points of physical damage, Y = stun</returns>
            public Vector2 DamageInflicted(int points)
            {
                return new Vector2((int)(physical * points), (int)(stun * points));
            }

            /// <summary>
            /// Number of points of physical damage inflicted by one point of this damage type
            /// </summary>
            private float physical;

            /// <summary>
            /// Number of points of stun damage inflicted by one point of this damage type
            /// </summary>
            private float stun;
        }

        #region Fields

        /// <summary>
        /// Internal name used by C# code to refer to this armor
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>Does armor give wearer ability to fly?</summary>
        public bool Flyer { get { return flyer; } }

        /// <summary>
        /// Internal name used by C# code to refer to this armor
        /// </summary>
        private string id;

        /// <summary>
        /// The armor that an attack can hit
        /// </summary>
        private int[] plates = new int[Enum.GetValues(typeof(Side)).Length];

        /// <summary>
        /// Table mapping damage type to points of damage inflicted
        /// </summary>
        private Susceptibility[] susceptibilities = new Susceptibility[Enum.GetValues(typeof(DamageType)).Length];

        /// <summary>Does armor give wearer ability to fly?</summary>
        private bool flyer;

        #endregion
    }
}
