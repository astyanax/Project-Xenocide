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
* @file BuildProject.cs
* @date Created: 2007/10/07
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using ProjectXenocide.Utils;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape.GeoEvents;
using ProjectXenocide.Model.Geoscape.Outposts;

using Xenocide.Resources;

#endregion

namespace ProjectXenocide.Model.Geoscape
{
    /// <summary>
    /// Represents an ongoing project to build an item
    /// </summary>
    [Serializable]
    public class BuildProject : Project
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemId">Identifer of the item being built</param>
        /// <param name="techManager">the technologies the player has</param>
        /// <param name="outpost">where item is being built</param>
        /// <param name="bank">funds used to pay for the project</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if outpost is null")]
        public BuildProject(string itemId, TechnologyManager techManager, Outpost outpost, Bank bank)
            :
            base("ITEM_PERSON_ENGINEER", (GetItem(itemId).BuildInfo.Hours), outpost.BuildProjectManager)
        {
            this.itemId         = itemId;
            this.outpost        = outpost;
            this.techManager    = techManager;
            this.bank           = bank;
            Item.StartBuild(techManager, outpost, bank);
        }

        /// <summary>
        /// Called when finish building one item
        /// </summary>
        public override void OnFinish()
        {
            Item.ReleaseBuildResources(outpost);
            outpost.Inventory.Add(Item.Manufacture(), false);
            --buildCount;

            // now we're either done, or we have another item to build
            if (0 == buildCount)
            {
                // we're done
                Cleanup();
                MessageBoxGeoEvent.Queue(Strings.MSGBOX_BUILD_PROJECT_FINISHED, outpost.Name, Item.Name);
            }
            else
            {
                // another item to build
                string error = Item.CanStartManufacture(techManager, outpost, bank);
                if (null == error)
                {
                    HoursWorked = 0;
                    Item.StartBuild(techManager, outpost, bank);
                    OnNumWorkersChanged();
                }
                else
                {
                    // can't build another item
                    Cleanup();
                    MessageBoxGeoEvent.Queue(Strings.MSGBOX_BUILD_PROJECT_ABORTED, Item.Name, outpost.Name, error);
                }
            }
        }

        /// <summary>
        /// Abort current build and cancel project
        /// </summary>
        public void Cancel()
        {
            Item.ReleaseBuildResources(outpost);
            Cleanup();
        }

        /// <summary>
        /// Calculate number of calander days it will take to build all requested items
        /// </summary>
        /// <returns>Value formatted for display to user</returns>
        public String CalcTotalItemsEtaToShow()
        {
            return CalcEtaToShow(TotalManHours);
        }

        /// <summary>
        /// Get information on Item
        /// </summary>
        /// <param name="itemId">Identifer of the item being built</param>
        /// <returns>Item</returns>
        private static ItemInfo GetItem(string itemId)
        {
            return Xenocide.StaticTables.ItemList[itemId];
        }

        #region Fields

        /// <summary>
        /// The name of this project, to show to player
        /// </summary>
        public string Name { get { return Item.Name; } }

        /// <summary>
        /// Internal code used inside Xenocide to refer to this Research Project
        /// </summary>
        public override string Id { get { return itemId; } }

        /// <summary>
        /// Number of items project has left to build
        /// </summary>
        public int BuildCount
        {
            get { return buildCount; }
            set
            {
                // must use Cancel() to stop project once it has started
                if (0 < value)
                {
                    buildCount = value;
                }
            }
        }

        /// <summary>
        /// Man hours required to build ALL items in project
        /// </summary>
        public double TotalManHours { get { return buildCount * (double)Item.BuildInfo.Hours; } }

        /// <summary>
        /// Static data for type of item being constructed
        /// </summary>
        public ItemInfo Item { get { return GetItem(itemId); } }

        /// <summary>
        /// Identifer of the item being built
        /// </summary>
        private string itemId;

        /// <summary>
        /// where item is being built
        /// </summary>
        private Outpost outpost;

        /// <summary>
        /// Number of items project has left to build
        /// </summary>
        private int buildCount = 1;

        /// <summary>
        /// funds used to pay for the project
        /// </summary>
        private Bank bank;

        /// <summary>
        /// the technologies the player has
        /// </summary>
        private TechnologyManager techManager;

        #endregion Fields
    }
}
