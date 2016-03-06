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
* @file XNetEntry.cs
* @date Created: 2007/03/24
* @author File creator: dteviot
* @author Credits: none
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

using Xenocide.Utils;

#endregion

namespace Xenocide.Model.StaticData
{
    /// <summary>
    /// An entry that is shown on X-Net
    /// </summary>
    public class XNetEntry
    {
        /// <summary>
        /// Construct an XNetEntry from an XML file
        /// </summary>
        /// <param name="entryNode">XML node holding data to construct an XNetEntry</param>
        /// <param name="manager">Namespace used in xnet.xml</param>
        /// <param name="category">Name of node on X-NET tree to put this entry in</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if entryNode == null")]
        public XNetEntry(XPathNavigator entryNode, XmlNamespaceManager manager, string category)
        {
            id            = entryNode.GetAttribute("id", String.Empty);
            name          = entryNode.GetAttribute("name", String.Empty);
            this.category = category;
            link          = XNetStatisticsLink.Factory(entryNode.SelectSingleNode("x:statisticsLink", manager));
            shortEntry    = ExtractXNetText(entryNode, manager, "x:shortentry");
            body          = ExtractXNetText(entryNode, manager, "x:body");
            fluff         = ExtractXNetText(entryNode, manager, "x:fluff");
            model         = ExtractModel(entryNode.SelectSingleNode("x:graphics", manager));
        }

        #region Methods

        /// <summary>
        /// Extract a one of an X-Net entries "text" sections from the XML file
        /// </summary>
        /// <param name="entryNode">XML data for entry</param>
        /// <param name="manager">Namespace used in xnet.xml</param>
        /// <param name="elementName">name of sub element holding the section</param>
        /// <returns>the text</returns>
        private static StringCollection ExtractXNetText(
            XPathNavigator      entryNode, 
            XmlNamespaceManager manager, 
            string              elementName )
        {
            XPathNavigator   element = entryNode.SelectSingleNode(elementName, manager); 
            StringCollection text    = new StringCollection();
            foreach (XPathNavigator child in element.SelectChildren(XPathNodeType.Element))
            {
                if (child.Name == "bullet")
                {
                    text.Add("- " + child.Value);
                }
                else if (child.Name == "paragraph")
                {
                    text.Add(child.Value);
                }
            }
            return text;
        }

        /// <summary>
        /// Extract name of 3D model to show on X-Net when this entry is selected
        /// </summary>
        /// <param name="graphicsNode">XML element to extract model's name from</param>
        /// <returns>The model's name, or empty string if there isn't one</returns>
        private static string ExtractModel(XPathNavigator graphicsNode)
        {
            string modelName = String.Empty;
            
            // not all X-Net entries have a model (at the moment)
            if (null != graphicsNode)
            {
                modelName = graphicsNode.GetAttribute("model", String.Empty);

                // strip ".mesh" from end of string
                if (modelName.EndsWith(".mesh"))
                {
                    modelName = modelName.Substring(0, modelName.Length - 5);
                }
            }

            // at current time, the stun launcher is the only model we have
            if (!String.IsNullOrEmpty(modelName) && ("Stun Launcher" != modelName))
            {
                modelName = String.Empty;
            }

            return modelName;
        }

        /// <summary>
        /// Is this the X-Net entry for the specified game item/facility/whatever?
        /// </summary>
        /// <param name="objectName">Internal identifier for type of Obect we're checking</param>
        /// <returns>true if this IS the entry for the object</returns>
        public bool IsEntryFor(String objectName)
        {
            return (link.ObjectName == objectName);
        }

        #endregion

        #region Fields

        /// <summary>
        /// Localized Name (of Entry) to show to player
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Internal name used by C# code to refer to this entry
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// "Node" on tree the entry goes under
        /// </summary>
        public string Category { get { return category; } }

        /// <summary>
        /// "Head" part of XNet entry's text
        /// </summary>
        public StringCollection ShortEntry { get { return shortEntry; } }

        /// <summary>
        /// Main part of XNet entry's text
        /// </summary>
        public StringCollection Body { get { return body; } }

        /// <summary>
        /// fluff part of XNet entry's text
        /// </summary>
        public StringCollection Fluff { get { return fluff; } }

        /// <summary>
        /// Actual numeric stats to show for X-Net entry
        /// </summary>
        public StringCollection Stats { get { return link.getStatistics(); } }

        /// <summary>
        /// Name of 3D model to show on X-Net when this entry is selected
        /// </summary>
        public string Model { get { return model; } }

        /// <summary>
        /// Internal name used by C# code to refer to this entry
        /// </summary>
        private string id;

        /// <summary>
        /// Localized Name (of Entry) to show to player
        /// </summary>
        private string name;

        /// <summary>
        /// "Node" on tree the entry goes under
        /// </summary>
        private string category;

        /// <summary>
        /// "Head" part of XNet entry's text
        /// </summary>
        private StringCollection shortEntry;

        /// <summary>
        /// Main part of XNet entry's text
        /// </summary>
        private StringCollection body;

        /// <summary>
        /// fluff part of XNet entry's text
        /// </summary>
        private StringCollection fluff;

        /// <summary>
        /// Actual numeric stats to show for X-Net entry
        /// </summary>
        private XNetStatisticsLink link;

        /// <summary>
        /// Name of 3D model to show on X-Net when this entry is selected
        /// </summary>
        private string model;

        #endregion
    }
}
