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
* @file ResearchReward.cs
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

#endregion

namespace ProjectXenocide.Model.StaticData.Research
{
    /// <summary>
    /// What a player recieves when a research topic is completed
    /// </summary>
    public class ResearchReward
    {
        /// <summary>
        /// Construct from an XML node
        /// </summary>
        /// <param name="node">XML node holding data to construct</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if node == null")]
        public ResearchReward(XPathNavigator node)
        {
            allOf = (Util.GetStringAttribute(node, "quantity") == "AllOf");

            // technologies
            foreach (XPathNavigator techNode in node.SelectChildren(XPathNodeType.Element))
            {
                technologies.Add(new Technology(techNode));
            }
        }

        /// <summary>
        /// Give the player the Reward
        /// </summary>
        /// <param name="manager">TechnologyManager to add the reward to</param>
        public void Grant(TechnologyManager manager)
        {
            if (allOf)
            {
                GrantAllOf(manager);
            }
            else
            {
                GrantOneOf(manager);
            }
        }

        /// <summary>
        /// Check if there's any technologies the player can still earn
        /// </summary>
        /// <param name="manager">manager holding technologies player has so far</param>
        /// <returns>true if there's still technologies the player still doesn't have</returns>
        public bool IsRewardLeft(TechnologyManager manager)
        {
            foreach (Technology tech in technologies)
            {
                if (!manager.IsAvailable(tech))
                {
                    return true;
                }
            }
            // if get here, all known
            return false;
        }

        /// <summary>
        /// Give player one technology that player doesn't already have
        /// </summary>
        /// <param name="manager">TechnologyManager to add the reward to</param>
        private void GrantOneOf(TechnologyManager manager)
        {
            List<Technology> choices = new List<Technology>();
            foreach (Technology tech in technologies)
            {
                if (!manager.IsAvailable(tech))
                {
                    choices.Add(tech);
                }
            }
            Debug.Assert(0 < choices.Count);
            manager.Add(choices[Xenocide.Rng.Next(choices.Count)]);
        }

        /// <summary>
        /// Give player all technologies that player doesn't already have
        /// </summary>
        /// <param name="manager">TechnologyManager to add the reward to</param>
        private void GrantAllOf(TechnologyManager manager)
        {
            foreach (Technology tech in technologies)
            {
                manager.Add(tech);
            }
        }

        #region Fields

        /// <summary>
        /// Grant all of the technologies, or just one?
        /// </summary>
        private bool allOf;

        /// <summary>
        /// List of technologies that may be gained
        /// </summary>
        private List<Technology> technologies = new List<Technology>();

        #endregion Fields
    }
}
