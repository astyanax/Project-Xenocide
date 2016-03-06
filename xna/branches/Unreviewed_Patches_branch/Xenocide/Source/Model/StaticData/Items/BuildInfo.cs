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
* @file BuildInfo.cs
* @date Created: 2007/10/06
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
using System.Diagnostics;

using ProjectXenocide.Utils;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// What is required for X-Corp to build a specific item
    /// </summary>
    /// <remarks>essentially, the construct element from item.xml
    /// &lt;construct&gt;
    ///     &lt;facility type="FAC_ENGINEERING_FACILITY" space="30" /&gt;
    ///     &lt;cost hours="1200" money="850000"&gt;
    ///         &lt;material type="ITEM_XENIUM_122" quantity="30" /&gt;
    ///         &lt;material type="ITEM_ALIEN_COMPOSITES" quantity="5" /&gt;
    ///     &lt;/cost&gt;
    /// &lt;/construct&gt;
    /// </remarks>
    [Serializable]
    public class BuildInfo
    {
        /// <summary>
        /// Construct BuildInfo from information in an XML element
        /// </summary>
        /// <param name="element">XML element holding data to construct BuildInfo</param>
        /// <param name="manager">Namespace used in item.xml</param>
        public BuildInfo(XPathNavigator element, XmlNamespaceManager manager)
        {
            // facility
            XPathNavigator facilityNode = element.SelectSingleNode("i:facility", manager);
            Debug.Assert("FAC_ENGINEERING_FACILITY" == Util.GetStringAttribute(facilityNode, "type"));
            space = Util.GetIntAttribute(facilityNode, "space");

            // cost (optional)
            XPathNavigator costNode = element.SelectSingleNode("i:cost", manager);
            if (null != costNode)
            {
                hours = Util.GetIntAttribute(costNode, "hours");
                dollars = Util.GetIntAttribute(costNode, "money");

                // materials (optional)
                foreach (XPathNavigator material in costNode.SelectChildren(XPathNodeType.Element))
                {
                    Debug.Assert("material" == material.LocalName);
                    materials.Add(new ItemLine(material));
                }
            }
        }

        /// <summary>
        /// Does outpost satisfy the BuildInfo requirements?
        /// </summary>
        /// <param name="outpost">outpost to examine</param>
        /// <returns>null if requirements satisifed, else failed requirement</returns>
        public string CanBuildHere(Outpost outpost)
        {
            if (GetCapacityInfo(outpost).Available < Space)
            {
                return Strings.ERROR_INSUFFICIENT_SPACE_FOR_BUILD;
            }
            foreach (ItemLine itemLine in Materials)
            {
                if (!itemLine.Contains(outpost))
                {
                    string materialName = Xenocide.StaticTables.ItemList[itemLine.ItemId].Name;
                    return Util.StringFormat(Strings.ERROR_INSUFFICIENT_MATERIAL_FOR_BUILD, materialName);
                }
            }

            // if get here, we're good to go
            return null;
        }

        /// <summary>
        /// Update outpost's state with changes due to build starting.
        /// Namely, remove materials required, and grab space
        /// </summary>
        /// <param name="outpost">outpost to adjust</param>
        public void StartBuild(Outpost outpost)
        {
            Debug.Assert(null == CanBuildHere(outpost));
            GetCapacityInfo(outpost).Use((uint)Space);
            foreach (ItemLine itemLine in Materials)
            {
                itemLine.Remove(outpost);
            }
        }

        /// <summary>
        /// Update outpost's state with changes due to build finishing.
        /// Namely, release the space consumed by the build
        /// </summary>
        /// <param name="outpost">outpost to adjust</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if outpost == null")]
        public void ReleaseBuildResources(Outpost outpost)
        {
            GetCapacityInfo(outpost).Release((uint)Space);
        }

        /// <summary>
        /// Details of space outpost has for building this type of item
        /// </summary>
        /// <param name="outpost">outpost to check</param>
        /// <returns>amount of available space</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if outpost is null")]
        public OutpostCapacityInfo GetCapacityInfo(Outpost outpost)
        {
            return outpost.Statistics.Capacities[SpaceType];
        }

        #region Fields

        /// <summary>
        /// Space required in facility to build this item
        /// </summary>
        public int Space { get { return space; } }

        /// <summary>
        /// Number of person hours item takes to build
        /// </summary>
        public int Hours { get { return hours; } }

        /// <summary>
        /// Number of dollars item takes to build
        /// </summary>
        public int Dollars { get { return dollars; } }

        /// <summary>
        /// Raw materials needed to construct item
        /// </summary>
        public IList<ItemLine> Materials { get { return materials; } }

        /// <summary>
        /// Space required in facility to build this item
        /// </summary>
        private int space;

        /// <summary>
        /// Number of person hours item takes to build
        /// </summary>
        private int hours;

        /// <summary>
        /// Number of dollars item takes to build
        /// </summary>
        private int dollars;

        /// <summary>
        /// Raw materials needed to construct item
        /// </summary>
        private List<ItemLine> materials = new List<ItemLine>();

        /// <summary>
        /// Type of space needed to build project
        /// </summary>
        /// <remarks>eventually, save the facility type from XML, and lookup storage space it provides</remarks>
        private string SpaceType { get { return "STORAGE_ENGINEER"; } }

        #endregion Fields
    }
}
