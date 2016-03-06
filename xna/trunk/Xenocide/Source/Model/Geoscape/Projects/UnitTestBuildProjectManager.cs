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
* @file UnitTestBuildProjectManager.cs
* @date Created: 2007/10/08
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;


using Microsoft.Xna.Framework;
using ProjectXenocide.Model.StaticData.Research;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Items;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.Geoscape
{
    /// <summary>
    /// Unit Tests for BuildProjectManager
    /// </summary>
    public sealed partial class BuildProjectManager : ProjectManager
    {
        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            // setup
            List<Outpost> outposts = new List<Outpost>();
            outposts.Add(OutpostInventory.ConstructTestOutpost());
            ResearchGraph     graph     = Xenocide.StaticTables.ResearchGraph;
            Person            engineer1 = Xenocide.StaticTables.ItemList["ITEM_PERSON_ENGINEER"].Manufacture() as Person;
            Bank              bank      = Xenocide.GameState.GeoData.XCorp.Bank;
            ItemInfo          item      = Xenocide.StaticTables.ItemList["ITEM_XC-2_STARFIRE"];
            OutpostInventory  inventory = outposts[0].Inventory;
            TechnologyManager techMgr   = new TechnologyManager(graph);
            graph.GiveStartingTech(techMgr);
            inventory.Add(engineer1, false);

            // can't start, no tech
            Debug.Assert(Strings.ERROR_INSUFFICIENT_RESEARCH_FOR_BUILD == item.CanStartManufacture(techMgr, outposts[0], bank));

            // can't start, insuffient funds
            bank.Debit(bank.CurrentBalance);
            Debug.Assert(0 == bank.CurrentBalance);
            techMgr.Add(new Technology("ITEM_XC-2_STARFIRE", TechnologyType.Item));
            Debug.Assert(Strings.ERROR_INSUFFICIENT_FUNDS_FOR_BUILD == item.CanStartManufacture(techMgr, outposts[0], bank));

            // no space to put finished item
            bank.Credit(1200000);
            Item gryphon1 = Xenocide.StaticTables.ItemList["ITEM_XC-1_GRYPHON"].Manufacture();
            inventory.Add(gryphon1, false);
            Item gryphon2 = Xenocide.StaticTables.ItemList["ITEM_XC-1_GRYPHON"].Manufacture();
            inventory.Add(gryphon2, false);
            Debug.Assert(Strings.ERROR_INSUFFICIENT_SPACE_FOR_BUILT_ITEM == item.CanStartManufacture(techMgr, outposts[0], bank));

            // no workshop space
            inventory.Remove(gryphon2);
            inventory.Remove(gryphon1);
            Debug.Assert(Strings.ERROR_INSUFFICIENT_SPACE_FOR_BUILD == item.CanStartManufacture(techMgr, outposts[0], bank));

            // Missing materials
            outposts[0].Floorplan.AddFacility(new FacilityHandle("FAC_ENGINEERING_FACILITY", 1, 4));
            Debug.Assert(null != item.CanStartManufacture(techMgr, outposts[0], bank));

            // add materials not quite enough to make 3 starfires
            for (int i = 0; i < 3; ++i)
            {
                Shipment shipment = new Shipment(outposts[0], new TimeSpan(0));
                shipment.Add(item.BuildInfo.Materials);

                // really should call shipment.Ship(), but that sets an appointment I don't want
                inventory.Add(shipment);
                shipment.OnShipmentArrive();
            }
            inventory.Remove(Xenocide.StaticTables.ItemList["ITEM_ALIEN_COMPOSITES"].Manufacture());
            Debug.Assert(null == item.CanStartManufacture(techMgr, outposts[0], bank));

            // start build
            Debug.Assert(2 == outposts[0].Statistics.Capacities[item.StorageType].Available);
            BuildProjectManager projectMgr = outposts[0].BuildProjectManager;
            BuildProject        project    = projectMgr.CreateProject(item.Id, techMgr, outposts[0], bank);

            // check consumption of requirements
            Debug.Assert(  2 == inventory.NumberInInventory(Xenocide.StaticTables.ItemList["ITEM_ALIEN_NAVIGATION_SYSTEMS"]));
            Debug.Assert(  2 == inventory.NumberInInventory(Xenocide.StaticTables.ItemList["ITEM_XENIUM_REACTOR"]));
            Debug.Assert(129 == inventory.NumberInInventory(Xenocide.StaticTables.ItemList["ITEM_ALIEN_COMPOSITES"]));
            Debug.Assert(800000 == bank.CurrentBalance);
            Debug.Assert(1 == outposts[0].Statistics.Capacities[item.StorageType].Available);
            Debug.Assert(20 == item.BuildInfo.GetCapacityInfo(outposts[0]).Available);

            Debug.Assert(1 == project.BuildCount);
            project.BuildCount = 3;
            Debug.Assert(42000 == project.TotalManHours);
            Debug.Assert(Strings.ETA_NEVER == project.CalcTotalItemsEtaToShow());
            project.Add(engineer1);
            Debug.Assert("1750" == project.CalcTotalItemsEtaToShow());

            // run to build time for one project
            const double day = 24.0 * 3600.0 * 1000.0;
            for (int i = 0; i < 584; ++i)
            {
                Xenocide.GameState.GeoData.GeoTime.AddMilliseconds(day);
            }
            Debug.Assert(2 == project.BuildCount);

            // second is built, but project ends because can't start 3rd
            Debug.Assert(projectMgr.IsInProgress(project));
            for (int i = 0; i < 584; ++i)
            {
                Xenocide.GameState.GeoData.GeoTime.AddMilliseconds(day);
            }
            Debug.Assert(1 == project.BuildCount);
            Debug.Assert(2 == inventory.NumberInInventory(item));
            Debug.Assert(1 == inventory.NumberInInventory(Xenocide.StaticTables.ItemList["ITEM_ALIEN_NAVIGATION_SYSTEMS"]));
            Debug.Assert(!projectMgr.IsInProgress(project));
        }
    }
}
