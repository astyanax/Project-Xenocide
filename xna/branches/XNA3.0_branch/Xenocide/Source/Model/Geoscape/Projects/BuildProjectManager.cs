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
* @file BuildProjectManager.cs
* @date Created: 2007/10/07
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
    /// Keeps track of items being constructed in an outpost
    /// </summary>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public sealed partial class BuildProjectManager : ProjectManager
    {
        /// <summary>
        /// Create a manufacturing project
        /// </summary>
        /// <param name="itemId">item the project will build</param>
        /// <param name="techManager">the technologies the player has</param>
        /// <param name="outpost">where item is being built</param>
        /// <param name="bank">funds used to pay for the project</param>
        /// <returns>the created project</returns>
        public BuildProject CreateProject(string itemId, TechnologyManager techManager, Outpost outpost,
            Bank bank)
        {
            BuildProject project = new BuildProject(itemId, techManager, outpost, bank);
            Add(project);
            return project;
        }

        /// <summary>
        /// Manager is informed that its X-Corp outpost was destroyed
        /// So cancel all projects in outpost (so we don't get appointments that they're done.)
        /// </summary>
        public void OnOutpostDestroyed()
        {
            foreach (BuildProject project in Projects)
            {
                project.Cancel();
            }
        }
    }
}
