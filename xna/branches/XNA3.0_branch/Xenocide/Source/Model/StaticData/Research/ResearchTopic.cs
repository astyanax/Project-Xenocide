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
* @file ResearchTopic.cs
* @date Created: 2007/09/29
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;

using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape.Outposts;

#endregion

namespace ProjectXenocide.Model.StaticData.Research
{
    /// <summary>
    /// Something the player can research
    /// </summary>
    public class ResearchTopic
    {
        /// <summary>
        /// Construct from an XML node
        /// </summary>
        /// <param name="node">XML node holding data to construct</param>
        /// <param name="manager">Namespace used in research.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if node == null")]
        public ResearchTopic(XPathNavigator node, XmlNamespaceManager manager)
        {
            id             = Util.GetStringAttribute(node, "id");
            name           = Util.GetStringAttribute(node, "name");
            days           = Util.GetIntAttribute(   node, "time");
            prerequisite   = new MultiPrerequisite(node.SelectSingleNode("r:prerequisite", manager), true);
            researchReward = new ResearchReward(   node.SelectSingleNode("r:grants",       manager));
        }

        /// <summary>
        /// Can the player research this topic?
        /// </summary>
        /// <param name="manager">the technologies the player has</param>
        /// <param name="outposts">the outposts the player has (where artefacts are stored)</param>
        /// <returns>true if prerequisites are satisfied and project will give some benefit</returns>
        public bool CanResearch(TechnologyManager manager, ICollection<Outpost> outposts)
        {
            return (IsSatisfied(manager, outposts) && IsRewardLeft(manager));
        }

        /// <summary>
        /// Consume any artefact(s) that are needed to begin research
        /// </summary>
        /// <param name="manager">the technologies the player has</param>
        /// <param name="outposts">the outposts the player has (where artefacts are stored)</param>
        public void ConsumeStartingArtefacts(TechnologyManager manager, ICollection<Outpost> outposts)
        {
            prerequisite.ConsumeStartingArtefacts(manager, outposts);
        }

        /// <summary>
        /// Give the player the Reward
        /// </summary>
        /// <param name="manager">TechnologyManager to add the reward to</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if manager is null")]
        public void GrantReward(TechnologyManager manager)
        {
            // note, player also gets this topic as an available technology
            manager.Add(new Technology(id, TechnologyType.Research));
            researchReward.Grant(manager);
        }

        /// <summary>
        /// Check if there's any technologies the player can still earn
        /// </summary>
        /// <param name="manager">manager holding technologies player has so far</param>
        /// <returns>true if there's still technologies the player still doesn't have</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if manager is null")]
        public bool IsRewardLeft(TechnologyManager manager)
        {
            // the ResearchTopic is also a reward
            return (!manager.IsAvailable(Id) || researchReward.IsRewardLeft(manager));
        }

        /// <summary>
        /// Does the player have all the technologies needed to start research
        /// </summary>
        /// <param name="manager">the technologies the player has</param>
        /// <returns>true if player has all needed technologies</returns>
        /// <remarks>This should ONLY be used for validating the ResearchGraph</remarks>
        public bool IsSatisfied(TechnologyManager manager)
        {
            return prerequisite.IsSatisfied(manager);
        }

        /// <summary>
        /// Does the player have all the technologies and sample artefacts needed to start research?
        /// </summary>
        /// <param name="manager">the technologies the player has</param>
        /// <param name="outposts">the outposts the player has (where artefacts are stored)</param>
        /// <returns>true if player has all needed technologies and sample artefacts</returns>
        public bool IsSatisfied(TechnologyManager manager, ICollection<Outpost> outposts)
        {
            return prerequisite.IsSatisfied(manager, outposts);
        }

        #region Fields

        /// <summary>
        /// Name of the topic to show to player
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Identifier for the topic
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// Number of person days it takes to research this topic
        /// </summary>
        public int Days { get { return days; } }

        /// <summary>
        /// Name of the topic to show to player
        /// </summary>
        private string name;

        /// <summary>
        /// Identifier for the topic
        /// </summary>
        private string id;

        /// <summary>
        /// Conditions that must be satisfied before research can start on this topic
        /// </summary>
        private Prerequisite prerequisite;

        /// <summary>
        /// Reward player recieves when a research topic is completed
        /// </summary>
        private ResearchReward researchReward;

        /// <summary>
        /// Number of person days it takes to research this topic
        /// </summary>
        private int days;

        #endregion Fields
    }
}
