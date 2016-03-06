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
* @file BattlescapeInfo.cs
* @date Created: 2007/11/12
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using ProjectXenocide.Utils;

#endregion

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// Information about an item on the battlescape
    /// </summary>
    [Serializable]
    public class BattlescapeInfo
    {
        /// <summary>
        /// Construct BattlescapeInfo from information in an XML element
        /// </summary>
        /// <param name="battlescapeNode">XML element holding data to construct BattlescapeInfo</param>
        /// <param name="manager">Namespace used in item.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if battlescapeNode is null")]
        public BattlescapeInfo(XPathNavigator battlescapeNode, XmlNamespaceManager manager)
        {
            armor = Util.GetIntAttribute(battlescapeNode, "armor");

            // "carry by soldier" details (optional)
            XPathNavigator carryInfoNode = battlescapeNode.SelectSingleNode("i:carryInfo", manager);
            if (null != carryInfoNode)
            {
                carryInfo = new CarryInfo(carryInfoNode);
            }

            // 3D model details (mandatory)
            graphic = new Graphic(battlescapeNode.SelectSingleNode("i:graphics", manager));
        }

        #region Fields

        /// <summary>
        /// Attacks less than or equal to this won't effect the item, over this level will destory it
        /// </summary>
        public int Armor { get { return armor; } }

        /// <summary>
        /// Information related to a soldier carring an item
        /// </summary>
        /// <remarks>If null, soldiers can't carry this item</remarks>
        public CarryInfo CarryInfo { get { return carryInfo; } }

        /// <summary>3D model to draw on battlescpe</summary>
        public Graphic Graphic { get { return graphic; } set { graphic = value; } }

        /// <summary>
        /// Attacks less than or equal to this won't effect the item, over this level will destory it
        /// </summary>
        private int armor;

        /// <summary>
        /// Information related to a soldier carring an item
        /// </summary>
        /// <remarks>If null, soldiers can't carry this item</remarks>
        private CarryInfo carryInfo;

        /// <summary>3D model to draw on battlescpe</summary>
        private Graphic graphic;

        #endregion Fields
    }

}
