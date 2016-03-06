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
* @file UnitTestResearchGraph.cs
* @date Created: 2007/09/30
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
using System.Xml;
using System.Xml.XPath;


using ProjectXenocide.Utils;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape.Outposts;

#endregion

namespace ProjectXenocide.Model.StaticData.Research
{
    /// <summary>
    /// Unit Tests of the ResearchGraph class
    /// </summary>
    public sealed partial class ResearchGraph : IEnumerable<ResearchTopic>
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

            //... Starting tech
            TechnologyManager mgr = new TechnologyManager(graph);
            graph.GiveStartingTech(mgr);

            // should be 3 topics we can initially research
            Debug.Assert(graph["RES_FIELD_MEDICAL_KIT"].CanResearch(mgr, outposts));
            Debug.Assert(graph["RES_LASER_PRINCIPLES"].CanResearch(mgr, outposts));
            Debug.Assert(graph["RES_MOTION_SENSOR"].CanResearch(mgr, outposts));
            Debug.Assert(3 == Util.SequenceLength(graph.StartableTopics(mgr, outposts)));

            // After granting medical kit, should no longer be researchable
            graph["RES_FIELD_MEDICAL_KIT"].GrantReward(mgr);
            Debug.Assert(!graph["RES_FIELD_MEDICAL_KIT"].CanResearch(mgr, outposts));

            // Add a navigator to outpost, should unlock navigator & alien topics
            Debug.Assert(!graph["RES_GREY"].CanResearch(mgr, outposts));
            Item greyNavigator = Xenocide.StaticTables.ItemList["ITEM_GREY_NAVIGATOR"].Manufacture();
            outposts[0].Inventory.Add(greyNavigator, false);

            //... check unlocked
            Debug.Assert(graph["RES_GREY"].CanResearch(mgr, outposts));
            Debug.Assert(graph["RES_NAVIGATOR_INTERROGATION"].CanResearch(mgr, outposts));
            Debug.Assert(graph["RES_ALIEN_ORIGINS"].CanResearch(mgr, outposts));

            // starting project should kill the navigator
            graph["RES_GREY"].ConsumeStartingArtefacts(mgr, outposts);
            Debug.Assert(0 == outposts[0].Inventory.NumberInInventory(greyNavigator.ItemInfo));
            Debug.Assert(!graph["RES_GREY"].CanResearch(mgr, outposts));
            Debug.Assert(!graph["RES_NAVIGATOR_INTERROGATION"].CanResearch(mgr, outposts));
            Debug.Assert(!graph["RES_ALIEN_ORIGINS"].CanResearch(mgr, outposts));

            // should be able to do navigator interrogation 8 times before run out of topics
            outposts[0].Inventory.Add(greyNavigator, false);
            for (int i = 0; i < 8; ++i)
            {
                Debug.Assert(graph["RES_NAVIGATOR_INTERROGATION"].CanResearch(mgr, outposts));
                graph["RES_NAVIGATOR_INTERROGATION"].GrantReward(mgr);
            }
            Debug.Assert(!graph["RES_NAVIGATOR_INTERROGATION"].CanResearch(mgr, outposts));

            // now test multi dependancy
            Debug.Assert(!graph["RES_PLASMA_PRINCIPLES"].CanResearch(mgr, outposts));
            graph["RES_HEAVY_PLASMA_RIFLE_CLIP"].GrantReward(mgr);
            graph["RES_HEAVY_PLASMA_RIFLE"].GrantReward(mgr);
            graph["RES_PLASMA_PISTOL"].GrantReward(mgr);
            graph["RES_PLASMA_RIFLE"].GrantReward(mgr);
            Debug.Assert(!graph["RES_PLASMA_PRINCIPLES"].CanResearch(mgr, outposts));
            graph["RES_PLASMA_PISTOL_CLIP"].GrantReward(mgr);
            Debug.Assert(graph["RES_PLASMA_PRINCIPLES"].CanResearch(mgr, outposts));
        }
    }
}
