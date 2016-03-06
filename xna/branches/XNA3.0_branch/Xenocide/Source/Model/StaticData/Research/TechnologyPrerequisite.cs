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
* @file TechnologyPrerequisite.cs
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
    /// Precondition that player must have a Technology before a ResearchTopic can be researched
    /// </summary>
    public class TechnologyPrerequisite : Prerequisite
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="element">Navigator to XML element</param>
        public TechnologyPrerequisite(XPathNavigator element)
        {
            requiredTech = new Technology(element);
        }

        /// <summary>
        /// Does the player have all the technologies needed to start research
        /// </summary>
        /// <param name="manager">the technologies the player has</param>
        /// <returns>true if player has all needed technologies</returns>
        /// <remarks>This should ONLY be used for validating the ResearchGraph</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if manager is null")]
        public override bool IsSatisfied(TechnologyManager manager)
        {
            return manager.IsAvailable(requiredTech);
        }

        /// <summary>
        /// Does the player have all the technologies and sample artefact needed to start research?
        /// </summary>
        /// <param name="manager">the technologies the player has</param>
        /// <param name="outposts">the outposts the player has (where artefact are stored)</param>
        /// <returns>true if player has all needed technologies and sample artefact</returns>
        public override bool IsSatisfied(TechnologyManager manager, ICollection<Outpost> outposts)
        {
            // As this requirement is looking for a technology, we don't care about available artefact
            return IsSatisfied(manager);
        }

        #region Fields

        /// <summary>
        /// Technology player needs to have to satisfy prerequisite
        /// </summary>
        private Technology requiredTech;

        #endregion Fields
    }
}
