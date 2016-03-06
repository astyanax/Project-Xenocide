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
* @file Technology.cs
* @date Created: 2007/09/29
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml.XPath;

using Microsoft.Xna.Framework;

using ProjectXenocide.Utils;

#endregion

namespace ProjectXenocide.Model
{
    /// <summary>
    /// The different types of technologies in the game
    /// </summary>
    public enum TechnologyType
    {
        /// <summary>
        /// Placeholder for "not a valid technology type"
        /// </summary>
        Invalid,

        /// <summary>
        /// A Research Topic that has been completed
        /// </summary>
        Research,

        /// <summary>
        /// An X-Net entry that is available for viewing
        /// </summary>
        XNet,

        /// <summary>
        /// A type of facility can be consturcted in an outpost
        /// </summary>
        Facility,

        /// <summary>
        /// An type of item that can be used (and possibly manufactured)
        /// </summary>
        Item
    }

    /// <summary>
    /// Represents a technology
    /// </summary>
    [Serializable]
    public class Technology
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">name of the technology</param>
        /// <param name="type">type of technology</param>
        public Technology(string id, TechnologyType type)
        {
            this.id   = id;
            this.type = type;
        }

        /// <summary>
        /// Construct a Technology from an XML element
        /// </summary>
        /// <param name="element">Navigator to XML element</param>
        /// <returns>the FacilityInfo</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if element is null")]
        public Technology(XPathNavigator element)
        {
            this.id   = Util.GetStringAttribute(element, "name");
            this.type = XmlToType(element.Name);
        }

        /// <summary>
        /// Convert an XML node's local name into into TechnologyType
        /// </summary>
        /// <param name="nodeName">Local name of the node</param>
        /// <returns>Matching TechnologyType</returns>
        static TechnologyType XmlToType(string nodeName)
        {
            TechnologyType type = TechnologyType.Invalid;
            if (nodeName == "facilityref")
            {
                type = TechnologyType.Facility;
            }
            else if (nodeName == "itemref")
            {
                type = TechnologyType.Item;
            }
            else if (nodeName == "topicref")
            {
                type = TechnologyType.Research;
            }
            else if (nodeName == "xnetref")
            {
                type = TechnologyType.XNet;
            }
            Debug.Assert(TechnologyType.Invalid != type);
            return type;
        }

        #region Fields

        /// <summary>
        /// Internal code to ID the technology
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// Type of technology this is
        /// </summary>
        public TechnologyType Type { get { return type; } }

        /// <summary>
        /// Internal code to ID the technology
        /// </summary>
        private string id;

        /// <summary>
        /// Type of technology this is
        /// </summary>
        private TechnologyType type;

        #endregion Fields
    }
}
