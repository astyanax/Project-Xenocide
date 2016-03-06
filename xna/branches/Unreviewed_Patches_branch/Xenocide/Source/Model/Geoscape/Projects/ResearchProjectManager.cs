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
* @file ResearchProjectManager.cs
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

using Microsoft.Xna.Framework;
using ProjectXenocide.Model.StaticData.Research;
using ProjectXenocide.Model.Geoscape.Outposts;

#endregion

namespace ProjectXenocide.Model.Geoscape
{
    /// <summary>
    /// Keeps track of research projects in progress by a player
    /// </summary>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public sealed partial class ResearchProjectManager : ProjectManager
    {
        /// <summary>
        /// Create a research project
        /// </summary>
        /// <param name="topicId">topic the project will research</param>
        /// <param name="techManager">the technologies the player has</param>
        /// <param name="outposts">the outposts the player has (where artefacts are stored)</param>
        /// <returns>the created project</returns>
        public ResearchProject CreateProject(string topicId, TechnologyManager techManager, ICollection<Outpost> outposts)
        {
            ResearchProject project = new ResearchProject(topicId, techManager, outposts, this);
            Add(project);
            return project;
        }

        /// <summary>
        /// Remove a project from the list
        /// </summary>
        /// <param name="project">project to remove</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if project is null")]
        public override void Remove(Project project)
        {
            Debug.Assert(project.IsFinished, "Can only remove finished research projects");
            base.Remove(project);
        }

        /// <summary>
        /// Manager is informed that an X-Corp outpost was destroyed
        /// So any people in the outpost are dead and can't be working on project
        /// </summary>
        /// <param name="outpost">the destroyed outpost</param>
        public void OnOutpostDestroyed(Outpost outpost)
        {
            foreach (ResearchProject project in Projects)
            {
                project.OnOutpostDestroyed(outpost.Inventory.ListStaff("ITEM_PERSON_SCIENTIST", true));
            }
        }
    }
}
