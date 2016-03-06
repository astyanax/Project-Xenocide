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
* @file ResearchProject.cs
* @date Created: 2007/09/30
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
using ProjectXenocide.Model.StaticData.Research;
using ProjectXenocide.Model.Geoscape.GeoEvents;
using ProjectXenocide.Model.Geoscape.Outposts;

#endregion

namespace ProjectXenocide.Model.Geoscape
{
    /// <summary>
    /// Represents an ongoing project to research a ResearchTopic
    /// </summary>
    [Serializable]
    public class ResearchProject : Project
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="topicId">Identifer of the topic being researched</param>
        /// <param name="techManager">the technologies the player has</param>
        /// <param name="outposts">the outposts the player has (where artefacts are stored)</param>
        /// <param name="projectManager">the owner of this project</param>
        public ResearchProject(string topicId, TechnologyManager techManager, ICollection<Outpost> outposts,
            ProjectManager projectManager)
            :
            base("ITEM_PERSON_SCIENTIST", (GetTopic(topicId).Days * 24), projectManager)
        {
            this.topicId        = topicId;
            this.techManager    = techManager;
            Debug.Assert(topic.CanResearch(techManager, outposts));
            topic.ConsumeStartingArtefacts(techManager, outposts);
        }

        /// <summary>
        /// Called when project finishes
        /// </summary>
        public override void OnFinish()
        {
            topic.GrantReward(techManager);
            Xenocide.GameState.GeoData.QueueEvent(new ResearchFinishedGeoEvent(topic.Id));

            // check to see if we aquired any automatically granted projects
            // repeat until no new topics granted
            while (GrantAutomaticTopics()) { }
            Cleanup();
        }

        /// <summary>
        /// Check if there's any topics that can be automatically granted, and if so, grant them
        /// </summary>
        /// <returns>true if any topics were found</returns>
        private bool GrantAutomaticTopics()
        {
            bool extraTopics = false;
            foreach (ResearchTopic topic in Xenocide.StaticTables.ResearchGraph)
            {
                if ((0 == topic.Days) && topic.IsSatisfied(techManager) && topic.IsRewardLeft(techManager))
                {
                    topic.GrantReward(techManager);
                    extraTopics = true;
                    Xenocide.GameState.GeoData.QueueEvent(new ResearchFinishedGeoEvent(topic.Id));
                }
            }
            return extraTopics;
        }

        /// <summary>
        /// Get specified ResearchTopic
        /// </summary>
        /// <param name="topicId">id of topic to get</param>
        /// <returns>the topic</returns>
        private static ResearchTopic GetTopic(string topicId)
        {
            return Xenocide.StaticTables.ResearchGraph[topicId];
        }

        #region Fields

        /// <summary>
        /// The name of this project, to show to player
        /// </summary>
        public string Name { get { return topic.Name; } }

        /// <summary>
        /// Internal code used inside Xenocide to refer to this Research Project
        /// </summary>
        public override string Id { get { return topicId; } }

        /// <summary>
        /// The topic being researched
        /// </summary>
        private ResearchTopic topic { get { return GetTopic(topicId); } }

        /// <summary>
        /// Identifer of the topic being researched
        /// </summary>
        private string topicId;

        /// <summary>
        /// The technologies the player has
        /// </summary>
        private TechnologyManager techManager;

        #endregion Fields
    }
}
