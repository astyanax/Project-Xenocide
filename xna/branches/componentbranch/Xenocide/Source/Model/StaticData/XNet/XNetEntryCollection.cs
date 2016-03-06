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
* @file XNetEntryCollection.cs
* @date Created: 2007/05/20
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections;
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
    /// List of all the available X-Net entries
    /// </summary>
    public sealed class XNetEntryCollection : IEnumerable<XNetEntry>
    {
        /// <summary>
        /// Load the list of X-Net entries from a file
        /// </summary>
        /// <param name="filename">Name of the XML file</param>
        public XNetEntryCollection(string filename)
        {
            entries = new List<XNetEntry>();

            XPathNavigator nav = (new XPathDocument(filename)).CreateNavigator();
            XmlNamespaceManager manager = new XmlNamespaceManager(nav.NameTable);
            manager.AddNamespace("x", "XNet");
            foreach (XPathNavigator category in nav.Select("/x:xnet/x:category", manager))
            {
                string categoryName = category.GetAttribute("name", String.Empty);
                foreach (XPathNavigator entry in category.Select("x:entry", manager))
                {
                    entries.Add(new XNetEntry(entry, manager, categoryName));
                }
            }
            ValidateList();
        }

        /// <summary>
        /// Retreive an X-Net entry by position
        /// </summary>
        /// <param name="pos">position (zero based)</param>
        /// <returns>the XNetEntry</returns>
        public XNetEntry this[int pos] { get { return entries[pos]; } }

        /// <summary>
        /// Fetch the X-Net entry for specified game item/facility/whatever
        /// </summary>
        /// <param name="objectName">Internal identifier for type of Obect we're checking</param>
        /// <returns>X-Net entry for object, if found, else null</returns>
        public XNetEntry FindEntryFor(String objectName)
        {
            foreach (XNetEntry e in entries)
            {
                if (e.IsEntryFor(objectName))
                {
                    return e;
                }
            }
            // if get here, not found
            return null;
        }
        
        /// <summary>
        /// Implement generics IEnumerable&lt;T&gt; interface
        /// </summary>
        /// <returns>The Enumerator</returns>
        IEnumerator<XNetEntry> IEnumerable<XNetEntry>.GetEnumerator()
        {
            return entries.GetEnumerator();
        }

        /// <summary>
        /// Implement IEnumerable interface
        /// </summary>
        /// <returns>The Enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return entries.GetEnumerator();
        }

        /// <summary>
        /// Check that the list of facilities we've loaded is good
        /// </summary>
        /// <remarks>Will throw if list isn't valid</remarks>
        private static void ValidateList()
        {
            // ToDo: implement
        }

        /// <summary>
        /// The list of X-Net entries
        /// </summary>
        private List<XNetEntry> entries;
    }
}
