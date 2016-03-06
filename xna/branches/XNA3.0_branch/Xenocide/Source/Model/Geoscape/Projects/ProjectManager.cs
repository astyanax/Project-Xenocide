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
* @file ProjectManager.cs
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

#endregion

namespace ProjectXenocide.Model.Geoscape
{
    /// <summary>
    /// Keeps track of projects in progress
    /// </summary>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class ProjectManager : IEnumerable<Project>
    {
        /// <summary>
        /// Add project to the list
        /// </summary>
        /// <param name="project">project to add to list</param>
        protected void Add(Project project)
        {
            projects.Add(project.Id, project);
        }

        /// <summary>
        /// Remove a project from the list
        /// </summary>
        /// <param name="project">project to remove</param>
        public virtual void Remove(Project project)
        {
            Debug.Assert(projects.ContainsKey(project.Id), "Project is not in progress");
            projects.Remove(project.Id);
        }

        /// <summary>
        /// Is player working on this project
        /// </summary>
        /// <param name="project">project to check for</param>
        /// <returns>true if project is in progress</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if project is null")]
        public bool IsInProgress(Project project)
        {
            return IsInProgress(project.Id);
        }

        /// <summary>
        /// Is player working on this project
        /// </summary>
        /// <param name="projectId">Id of project to check for</param>
        /// <returns>true if project is in progress</returns>
        public bool IsInProgress(string projectId)
        {
            return projects.ContainsKey(projectId);
        }

        /// <summary>
        /// Tell all projects to bring their state up to date
        /// </summary>
        public void Update()
        {
            for (int i = projects.Values.Count - 1; 0 <= i; --i)
            {
                projects.Values[i].Update();
            }
        }

        /// <summary>
        /// Implement generics IEnumerable&lt;T&gt; interface
        /// </summary>
        /// <returns>The Enumerator</returns>
        IEnumerator<Project> IEnumerable<Project>.GetEnumerator()
        {
            return projects.Values.GetEnumerator();
        }

        /// <summary>
        /// Implement IEnumerable interface
        /// </summary>
        /// <returns>The Enumerator</returns>
        public IEnumerator GetEnumerator()
        {
            return projects.Values.GetEnumerator();
        }

        #region Fields

        /// <summary>
        /// The projects currently in progress
        /// </summary>
        protected ICollection<Project> Projects { get { return projects.Values; } }

        /// <summary>
        /// The projects currently in progress
        /// </summary>
        private SortedList<string, Project> projects = new SortedList<string, Project>();

        #endregion Fields
    }
}
