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
* @file PlanetParser.cs
* @date Created: 2007/06/21
* @author File creator: Darkside
* @author Credits: none
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
    /// Handles the parsing of planet data from planets.xml
    /// </summary>
    public static class PlanetParser
    {
        /// <summary>
        ///   Parses planet data from passed filename
        /// </summary>
        /// <param name="filename">source planets.xml to parse</param>
        /// <returns>the first planet parsed (default planet)</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if entryNode == null")]
        public static Planet Parse(string filename)
        {
            List<Planet> planets = new List<Planet>();

            // Set up XPathNavigator
            const string xmlns = "PlanetConfig";
            XPathNavigator nav = Util.MakeValidatingXPathNavigator(filename, xmlns);
            XmlNamespaceManager manager = new XmlNamespaceManager(nav.NameTable);
            manager.AddNamespace("p", xmlns);

            foreach (XPathNavigator planetNode in nav.Select("/p:planets/p:planet", manager))
            {
                planets.Add(new Planet(planetNode, manager));
            }

            return planets[0];
        }
    }
}
