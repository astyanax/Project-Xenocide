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
* @file PersonItemInfo.cs
* @date Created: 2007/09/10
* @author File creator: David Teviotdale
* @author Credits: nil
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

using ProjectXenocide.Utils;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;

#endregion

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// The non-changing information about a X-Corp employee (Engineer, Scientist, Soldier)
    /// </summary>
    [Serializable]
    public class PersonItemInfo : ItemInfo
    {
        /// <summary>
        /// Construct a PersonItemInfo from XML file
        /// </summary>
        /// <param name="itemNode">XML node holding data to construct PersonItemInfo</param>
        /// <param name="manager">Namespace used in item.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if itemNode == null")]
        public PersonItemInfo(XPathNavigator itemNode, XmlNamespaceManager manager)
            : base(itemNode, manager)
        {
            // statistics element
            XPathNavigator personNode = itemNode.SelectSingleNode("i:statistics", manager);
            monthlySalary = Util.GetIntAttribute(personNode,    "monthlySalary");
            worksIn       = Util.GetStringAttribute(personNode, "worksIn");
            skillLevel    = Util.GetIntAttribute(personNode,    "skillLevel");
        }

        #region Methods

        /// <summary>
        /// Return number of people of this type in an Outpost
        /// </summary>
        /// <remarks>This is part of a double dispatch lookup</remarks>
        /// <param name="inventory">Inventory of outpost to check</param>
        /// <returns>Number of people</returns>
        public override int NumberInInventory(OutpostInventory inventory)
        {
            return Util.SequenceLength(inventory.ListStaff(Id));
        }

        /// <summary>
        /// Construct an Item for a person of this type, in the state it
        /// would be if just purchased
        /// </summary>
        /// <returns>constructed Item</returns>
        public override Item Manufacture()
        {
            return new Person(this);
        }

        /// <summary>
        /// Add a person held in an Item to an outpost's storage
        /// </summary>
        /// <param name="inventory">inventory to put item into</param>
        /// <param name="item">person to add</param>
        /// <param name="spaceAlreadyRecorded">true if we've already recorded space item is using. e.g. We're finishing production/transfer</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if inventory == null")]
        public override void AddTo(OutpostInventory inventory, Item item, bool spaceAlreadyRecorded)
        {
            // put the item into the outpost, casting the Item to the correct type so
            // double dereference works
            inventory.Add(item as Person, spaceAlreadyRecorded);
        }

        /// <summary>
        /// Remove a specific person from inventory
        /// </summary>
        /// <param name="inventory">Inventory to remove person from</param>
        /// <param name="item">Details of person to remove</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if inventory == null")]
        public override void Remove(OutpostInventory inventory, Item item)
        {
            // Remove the person from the outpost, casting the Item to the correct type so
            // double dereference works
            inventory.Remove(item as Person);
        }

        /// <summary>
        /// Add stats specific to this person type to string collection for display on X-Net
        /// </summary>
        /// <param name="stats">string collection to append strings to</param>
        protected override void XNetStatisticsCore(StringCollection stats)
        {
        }

        #endregion

        #region Fields

        /// <summary>
        /// How productive is person when working?
        /// i.e. Units of "work" per person day of labour.
        /// </summary>
        public int SkillLevel { get { return skillLevel; } }

        /// <summary>
        /// Storage type used by person when working
        /// </summary>
        public String WorksIn { get { return worksIn; } }

        /// <summary>
        /// Are instances of this type of object distinct or interchangable?
        /// e.g. Each craft is unique, but a Plasma rifle identical to any other plasma rifle.
        /// </summary>
        public override bool IsUnique { get { return true; } }

        /// <summary>
        /// What it costs per month to keep an item of this type
        /// </summary>
        public override int MonthlyCharge { get { return monthlySalary; } }

        /// <summary>
        /// How much we pay person each month
        /// </summary>
        private int monthlySalary;

        /// <summary>
        /// How productive is person when working?
        /// i.e. Units of "work" per person day of labour.
        /// </summary>
        private int skillLevel;

        /// <summary>
        /// Storage type used by person when working
        /// </summary>
        private String worksIn;

        #endregion
    }
}
