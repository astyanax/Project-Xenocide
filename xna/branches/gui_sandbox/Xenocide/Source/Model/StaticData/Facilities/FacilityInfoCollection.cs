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
* @file FacilityInfoCollection.cs
* @date Created: 2007/04/09
* @author File creator: dteviot
* @author Credits: code is derived from previous humanfacilitytype.h
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

using Xenocide.Resources;
using Xenocide.Utils;

#endregion

namespace Xenocide.Model.StaticData.Facilities
{
    /// <summary>
    /// List of all the available facility types for X-Corp bases
    /// </summary>
    public sealed class FacilityInfoCollection : IEnumerable<FacilityInfo>
    {
        /// <summary>
        /// Load the list of facilities from a file
        /// </summary>
        /// <param name="filename">Name of the XML file</param>
        /// <param name="xnetEntryList">X-Net entries to check against</param>
        public FacilityInfoCollection(string filename, XNetEntryCollection xnetEntryList)
        {
            facilities = new SortedList<string, FacilityInfo>();
            XPathNavigator nav = (new XPathDocument(filename)).CreateNavigator();
            XmlNamespaceManager manager = new XmlNamespaceManager(nav.NameTable);
            manager.AddNamespace("f", "FacilityConfig");
            foreach (XPathNavigator facilityElement in nav.Select("/f:facilities/f:facility", manager))
            {
                FacilityInfo facility = Factory(facilityElement, manager);
                facilities.Add(facility.Id, facility);
            }
            ValidateList(xnetEntryList);
        }

        /// <summary>
        /// Retreive a FacilityInfo by position
        /// </summary>
        /// <param name="pos">position (zero based)</param>
        /// <returns>the FacilityInfo</returns>
        public FacilityInfo this[int pos] { get { return facilities.Values[pos]; } }

        /// <summary>
        /// Retreive a FacilityInfo by name
        /// </summary>
        /// <param name="id">internal name used to ID the facility type</param>
        /// <returns>the FacilityInfo</returns>
        public FacilityInfo this[string id] { get { return facilities[id]; } }

        /// <summary>
        /// Return the index to the facility with the specified id
        /// </summary>
        /// <param name="id">id to search for</param>
        /// <returns>the index</returns>
        public int IndexOf(string id)
        {
            return facilities.IndexOfKey(id);
        }
        
        /// <summary>
        /// Implement generics IEnumerable&lt;T&gt; interface
        /// </summary>
        /// <returns>The Enumerator</returns>
        IEnumerator<FacilityInfo> IEnumerable<FacilityInfo>.GetEnumerator()
        {
            return facilities.Values.GetEnumerator();
        }

        /// <summary>
        /// Implement IEnumerable interface
        /// </summary>
        /// <returns>The Enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return facilities.Values.GetEnumerator();
        }

        /// <summary>
        /// The list of facility types
        /// </summary>
        private SortedList<string, FacilityInfo> facilities;

        /// <summary>
        /// Check that the list of facilities we've loaded is good
        /// </summary>
        /// <param name="xnetEntryList">X-Net entries to check against</param>
        /// <remarks>Will throw if list isn't valid</remarks>
        private void ValidateList(XNetEntryCollection xnetEntryList)
        {
            // check there's an X-Net entry for each facility
            foreach (FacilityInfo f in facilities.Values)
            {
                if ((null == xnetEntryList.FindEntryFor(f.Id)))
                {
                    throw new Exception(Util.StringFormat(Strings.EXCEPTION_NO_XNET_ENTRY_FOUND, f.Name));
                }
            }
        }

        /// <summary>
        /// Construct correct type of FacilityInfo from an XML element
        /// </summary>
        /// <param name="element">Navigator to XML element</param>
        /// <param name="manager">Namespace used in facility.xml</param>
        /// <returns>the ScanFacilityInfo</returns>
        private static FacilityInfo Factory(XPathNavigator element, XmlNamespaceManager manager)
        {
            // get the type of XML node (which will tell use the type of facility to build)
            string type = type = element.GetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance");
            Debug.Assert(!String.IsNullOrEmpty(type));

            if ((type == "storageFacilityType") || (type == "projectFacilityType"))
            {
                return new StorageFacilityInfo(element, manager);
            }
            else if (type == "scanningFacilityType")
            {
                return new ScanFacilityInfo(element, manager);
            }
            else if (type == "defenseFacilityType")
            {
                return new DefenseFacilityInfo(element, manager);
            }
            else if ((type == "facilityType") || (type == "shieldFacilityType"))
            {
                // at current time, access lift, grav and neural shield 
                // facilities are each special types
                return new SpecialFacilityInfo(element, manager);
            }
            else
            {
                // someone's created a new facility type
                Debug.Assert(false);
                return (FacilityInfo)null;
            }
        }
    }
}
