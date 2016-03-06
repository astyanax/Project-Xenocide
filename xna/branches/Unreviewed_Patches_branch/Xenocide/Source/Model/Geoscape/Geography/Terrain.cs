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
* @file Terrain.cs
* @date Created: 2007/07/13
* @author File creator: dteviot
* @author Credits: darkside
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;
using System.Threading;
using System.IO;


using ProjectXenocide.Utils;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;

using CeGui;

#endregion

namespace ProjectXenocide.Model.Geoscape.Geography
{

    /// <summary>
    /// The types of terrain on the geoscape.  e.g. Water, Mountain, swamp, etc.
    /// </summary>
    [Serializable]
    public class Terrain : IGeoBitmapProperty
    {
        /// <summary>
        /// Load a new terrain type from XML file
        /// </summary>
        /// <param name="entryNode">XML node holding data to construct a Terrain</param>
        /// <param name="manager">Namespace used in planets.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if entryNode == null")]
        public Terrain(XPathNavigator entryNode, XmlNamespaceManager manager)
        {
            name     = entryNode.GetAttribute("name", String.Empty);
            colorKey = Util.GetColorKey(entryNode, manager);
        }

        #region Fields

        /// <summary>
        /// Name of this type of terrain
        /// </summary>
        public String Name { get { return name; } }

        /// <summary>
        /// Name of this type of terrain
        /// </summary>
        private String name;

        /// <summary>
        /// The color which represents this type of terrain on the bitmap
        /// </summary>
        private uint colorKey;

        /// <summary>
        /// Number of pixels of this type of terrain on the bitmap
        /// </summary>
        private uint size;

        #endregion Fields

        #region IGeoBitmapMember Members

        /// <summary>
        /// The color which represents this type of terrain on the bitmap
        /// </summary>
        public uint ColorKey { get { return colorKey; } }

        /// <summary>
        /// Number of pixels of this type of terrain on the bitmap
        /// </summary>
        public uint Size { get { return size; } set { size = value; } }

        #endregion
    }
}
