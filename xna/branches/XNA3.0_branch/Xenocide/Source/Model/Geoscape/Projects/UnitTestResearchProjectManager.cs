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
* @file UnitTestResearchProjectManager.cs
* @date Created: 2007/09/31
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

#endregion

namespace ProjectXenocide.Model.Geoscape
{
    /// <summary>
    /// Unit Tests for ResearchProjectManager
    /// </summary>
    public sealed partial class ResearchProjectManager : ProjectManager
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
            ResearchGraph graph = Xenocide.StaticTables.ResearchGraph;
            Person scientist1 = Xenocide.StaticTables.ItemList["ITEM_PERSON_SCIENTIST"].Manufacture() as Person;
            Person scientist2 = Xenocide.StaticTables.ItemList["ITEM_PERSON_SCIENTIST"].Manufacture() as Person;
            outposts[0].Inventory.Add(scientist1, false);
            outposts[0].Inventory.Add(scientist2, false);
            ResearchProjectManager projectMgr = new ResearchProjectManager();

            //... Starting tech
            TechnologyManager techMgr = new TechnologyManager(graph);
            graph.GiveStartingTech(techMgr);
            Debug.Assert(!techMgr.IsAvailable("RES_FIELD_MEDICAL_KIT"));

            // Start project
            Debug.Assert(!projectMgr.IsInProgress("RES_FIELD_MEDICAL_KIT"));
            ResearchProject medKitProject = projectMgr.CreateProject("RES_FIELD_MEDICAL_KIT", techMgr, outposts);
            Debug.Assert(projectMgr.IsInProgress(medKitProject));
            Debug.Assert(projectMgr.IsInProgress("RES_FIELD_MEDICAL_KIT"));

            // update with no scientist should have no effect
            const double day = 24.0 * 3600.0 *1000.0;
            Debug.Assert(0 == medKitProject.HoursWorked);
            Xenocide.GameState.GeoData.GeoTime.AddMilliseconds(210 * day);
            projectMgr.Update();
            Debug.Assert(0 == medKitProject.HoursWorked);
            Debug.Assert("Never" == medKitProject.CalcEtaToShow());

            // assign scientist
            medKitProject.Add(scientist1);
            Debug.Assert(210 * 24 == medKitProject.CalcEta().TotalHours);
            Debug.Assert("210" == medKitProject.CalcEtaToShow());

            // assign second
            medKitProject.Add(scientist2);
            Debug.Assert(105 * 24 == medKitProject.CalcEta().TotalHours);
            Debug.Assert("105" == medKitProject.CalcEtaToShow());

            // remove a scientist
            Debug.Assert(scientist2 == medKitProject.RemoveWorker());

            // run to completion
            medKitProject.Add(scientist2);
            Xenocide.GameState.GeoData.GeoTime.AddMilliseconds(105 * day);
            Debug.Assert(210 * 24 == medKitProject.HoursWorked);
            Debug.Assert(medKitProject.IsFinished);
            Debug.Assert(!projectMgr.IsInProgress(medKitProject));
            Debug.Assert(techMgr.IsAvailable("RES_FIELD_MEDICAL_KIT"));
        }
    }
}
