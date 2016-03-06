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
* @file City.cs
* @date Created: 2007/06/28
* @author File creator: darkside
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using ProjectXenocide.Utils;

namespace ProjectXenocide.Model.Geoscape.Geography
{
    /// <summary>
    /// City object to represent cities on the geosphere
    /// </summary>
    [Serializable]
    public class City : IGeoPosition
    {
        /// <summary>
        /// Creates a new city with the given parameters
        /// </summary>
        /// <param name="cityNode">XML node holding data to construct city</param>
        /// <param name="manager">Namespace used in planets.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if cityNode is null")]
        public City(XPathNavigator cityNode, XmlNamespaceManager manager)
        {
            this.name = Util.GetStringAttribute(cityNode, "name");
            this.position = new GeoPosition(cityNode.SelectSingleNode("p:geoposition", manager));
        }

        /// <summary>
        /// Override to return city name
        /// </summary>
        /// <returns>city name</returns>
        public override string ToString()
        {
            return name;
        }

        #region Fields

        /// <summary>
        /// the name of the city
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// the location of this city
        /// </summary>
        public GeoPosition Position { get { return position; } }

        /// <summary>
        /// IGeoPosition interface
        /// </summary>
        /// <returns>true</returns>
        public bool IsKnownToXCorp { get { return true; } }

        /// <summary>
        /// the name of the city
        /// </summary>
        private string name;

        /// <summary>
        /// the location of this city
        /// </summary>
        private GeoPosition position;

        # endregion
    }
}
