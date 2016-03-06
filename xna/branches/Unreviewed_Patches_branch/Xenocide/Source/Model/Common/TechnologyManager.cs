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
* @file TechnologyManager.cs
* @date Created: 2007/09/29
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

#endregion

namespace ProjectXenocide.Model
{
    /// <summary>
    /// The technologies aquired by X-Corp
    /// </summary>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public sealed class TechnologyManager : IEnumerable<Technology>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graph">to load starting tech from</param>
        public TechnologyManager(ResearchGraph graph)
        {
            graph.GiveStartingTech(this);
        }

        /// <summary>
        /// Add a technology to the list of technologies known
        /// </summary>
        /// <param name="tech">technology to add</param>
        public void Add(Technology tech)
        {
            technologies[tech.Id] = tech;
        }

        /// <summary>
        /// Does X-Corp have access to a technology?
        /// </summary>
        /// <param name="tech">technology to check for</param>
        /// <returns>true if X-Corp has access</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if tech is null")]
        public bool IsAvailable(Technology tech)
        {
            return IsAvailable(tech.Id);
        }

        /// <summary>
        /// Does X-Corp have access to a technology?
        /// </summary>
        /// <param name="tech">name of technology to check for</param>
        /// <returns>true if X-Corp has access</returns>
        public bool IsAvailable(string tech)
        {
            return technologies.ContainsKey(tech);
        }

        /// <summary>
        /// Implement generics IEnumerable&lt;T&gt; interface
        /// </summary>
        /// <returns>The Enumerator</returns>
        IEnumerator<Technology> IEnumerable<Technology>.GetEnumerator()
        {
            return technologies.Values.GetEnumerator();
        }

        /// <summary>
        /// Implement IEnumerable interface
        /// </summary>
        /// <returns>The Enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return technologies.Values.GetEnumerator();
        }

        #region Fields

        /// <summary>
        /// Internal code to ID the technology
        /// </summary>
        private Dictionary<string, Technology> technologies = new Dictionary<string, Technology>();

        #endregion Fields
    }
}
