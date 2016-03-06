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
* @file StaticTables.cs
* @date Created: 2007/03/24
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using Microsoft.Xna.Framework.Storage;

using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Facilities;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.StaticData.Research;
using ProjectXenocide.Model.StaticData.Battlescape;

#endregion

namespace ProjectXenocide.Model
{
    /// <summary>
    /// The root class that holds all "model" (as in model-view) data that is fixed, but loaded from XML files
    /// </summary>
    /// <remarks>That is, things like weapon and craft stats, facilities, etc.</remarks>
    public class StaticTables
    {
        /// <summary>
        /// Read the static data from the files it's stored in
        /// </summary>
        public StaticTables()
        {
            xnetEntryList = new XNetEntryCollection(DataDirectory + "xnet.xml");
            facilityList  = new FacilityInfoCollection(DataDirectory + "facility.xml", xnetEntryList);
            peopleNames   = new PeopleNames(DataDirectory + "peopleNames.xml");
            armorList     = new ArmorCollection(DataDirectory + "combatant.xml");
        }

        /// <summary>
        /// Populate the tables
        /// <remarks>This function is needed because some items refer to Xenocide.StaticTables in their constructors
        /// So we have to have created the StaticTables object before we create them</remarks>
        /// </summary>
        public void Populate()
        {
            itemList.Populate(DataDirectory + "item.xml", xnetEntryList);
            startSettings.Populate(DataDirectory + "startsettings.xml");
            researchGraph.Populate(DataDirectory + "research.xml");
            combatantFactory.Populate(DataDirectory + "combatant.xml");

            // additional validation checks
            researchGraph.Validate(xnetEntryList, facilityList, itemList);
        }

        /// <summary>
        /// The X-Net entries
        /// </summary>
        public XNetEntryCollection XNetEntryList { get { return xnetEntryList; } }

        /// <summary>
        /// Stats for all the different types of Facilities
        /// </summary>
        public FacilityInfoCollection FacilityList { get { return facilityList; } }

        /// <summary>
        /// Stats for the different items
        /// </summary>
        public ItemCollection ItemList { get { return itemList; } }

        /// <summary>
        /// The directory holding all the XML data files.
        /// </summary>
        public string DataDirectory { get { return StorageContainer.TitleLocation + "/Content/Schema/"; } }

        /// <summary>
        /// Assorted configuration data for putting game into starting state
        /// </summary>
        public StartSettings StartSettings { get { return startSettings; } }

         /// <summary>
        /// Object holding lists of given and family names for all personnel
        /// </summary>
        public PeopleNames PeopleNames { get { return peopleNames; } }

        /// <summary>
        /// List of topics a player can (eventually) research
        /// </summary>
        public ResearchGraph ResearchGraph { get { return researchGraph; } }

        /// <summary>
        /// List of all the available armors that combatants can have
        /// </summary>
        public ArmorCollection ArmorList { get { return armorList; } }

        /// <summary>Builds combatants</summary>
        public CombatantFactory CombatantFactory { get { return combatantFactory; } }

        /// <summary>
        /// The X-Net entries
        /// </summary>
        private XNetEntryCollection xnetEntryList;

        /// <summary>
        /// Stats for all the different types of Facilities
        /// </summary>
        private FacilityInfoCollection facilityList;

        /// <summary>
        /// Stats for the different items
        /// </summary>
        private ItemCollection itemList = new ItemCollection();

        /// <summary>
        /// Assorted configuration data for putting game into starting state
        /// </summary>
        private StartSettings startSettings = new StartSettings();

        /// <summary>
        /// Object holding lists of given and family names for all personnel
        /// </summary>
        private PeopleNames peopleNames;

        /// <summary>
        /// List of topics a player can (eventually) research
        /// </summary>
        private ResearchGraph researchGraph = new ResearchGraph();

        /// <summary>
        /// List of all the available armors that combatants can have
        /// </summary>
        private ArmorCollection armorList;

        /// <summary>Builds combatants</summary>
        private CombatantFactory combatantFactory = new CombatantFactory();
    }
}

