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
* @file ArmorCollection.cs
* @date Created: 2007/12/08
* @author File creator: David Teviotdale
* @author Credits: nil
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


using ProjectXenocide.Utils;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.StaticData.Battlescape
{
    /// <summary>
    /// List of all the available armors that combatants can have
    /// </summary>
    public class ArmorCollection : IEnumerable<Armor>
    {
        /// <summary>
        /// Load the list of armors from a file
        /// </summary>
        /// <param name="filename">Name of the XML file</param>
        public ArmorCollection(string filename)
        {
            // Set up XPathNavigator
            const string xmlns = "CombatantConfig";
            XPathNavigator nav = Util.MakeValidatingXPathNavigator(filename, xmlns);
            XmlNamespaceManager manager = new XmlNamespaceManager(nav.NameTable);
            manager.AddNamespace("c", xmlns);

            // Process the XML file
            foreach (XPathNavigator armorNode in nav.Select("/c:combatantdata/c:armor", manager))
            {
                Armor armor = new Armor(armorNode, manager);
                armors.Add(armor.Id, armor);
            }
            ValidateList();
        }

        /// <summary>
        /// Retreive a Armor by position
        /// </summary>
        /// <param name="pos">position (zero based)</param>
        /// <returns>the Armor</returns>
        public Armor this[int pos] { get { return armors.Values[pos]; } }

        /// <summary>
        /// Retreive a Armor by name
        /// </summary>
        /// <param name="id">internal name used to ID the armor</param>
        /// <returns>the Armor</returns>
        public Armor this[string id] { get { return armors[id]; } }

        /// <summary>
        /// Return the index to the armor with the specified id
        /// </summary>
        /// <param name="id">id to search for</param>
        /// <returns>the index</returns>
        public int IndexOf(string id)
        {
            return armors.IndexOfKey(id);
        }

        /// <summary>
        /// Implement generics IEnumerable&lt;T&gt; interface
        /// </summary>
        /// <returns>The Enumerator</returns>
        IEnumerator<Armor> IEnumerable<Armor>.GetEnumerator()
        {
            return armors.Values.GetEnumerator();
        }

        /// <summary>
        /// Implement IEnumerable interface
        /// </summary>
        /// <returns>The Enumerator</returns>
        public IEnumerator GetEnumerator()
        {
            return armors.Values.GetEnumerator();
        }

        /// <summary>
        /// Check that the list of armors we've loaded is good
        /// </summary>
        /// <remarks>Will throw if list isn't valid</remarks>
        private void ValidateList()
        {
            // There must be an armor with an ID of "None"
            if (-1 == NoArmorIndex)
            {
                throw new XPathException(Strings.EXCEPTION_NONE_ARMOR_MISSING);
            }
        }

        #region Fields

        /// <summary>
        /// Return the "No armor" armor
        /// </summary>
        public Armor NoArmor { get { return armors.Values[NoArmorIndex]; } }

        /// <summary>
        /// Return index of the "No armor" armor
        /// </summary>
        public int NoArmorIndex { get { return IndexOf("None"); } }

        /// <summary>
        /// The list of armors types
        /// </summary>
        private SortedList<string, Armor> armors = new SortedList<string, Armor>();

        #endregion
    }
}
