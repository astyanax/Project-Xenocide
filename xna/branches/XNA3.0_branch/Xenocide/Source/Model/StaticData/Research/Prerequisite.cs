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
* @file Prerequisite.cs
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
    /// Precondition that must be satisfied before a ResearchTopic can be researched
    /// </summary>
    public abstract class Prerequisite
    {
        /// <summary>
        /// Constructor
        /// </summary>
        protected Prerequisite() {}

        /// <summary>
        /// Does the player have all the technologies needed to start research
        /// </summary>
        /// <param name="manager">the technologies the player has</param>
        /// <returns>true if player has all needed technologies</returns>
        /// <remarks>This should ONLY be used for validating the ResearchGraph</remarks>
        public abstract bool IsSatisfied(TechnologyManager manager);

        /// <summary>
        /// Does the player have all the technologies and sample artefacts needed to start research?
        /// </summary>
        /// <param name="manager">the technologies the player has</param>
        /// <param name="outposts">the outposts the player has (where artefacts are stored)</param>
        /// <returns>true if player has all needed technologies and sample artefacts</returns>
        public abstract bool IsSatisfied(TechnologyManager manager, ICollection<Outpost> outposts);

        /// <summary>
        /// Consume any artefact(s) that are needed to begin research
        /// </summary>
        /// <param name="manager">the technologies the player has</param>
        /// <param name="outposts">the outposts the player has (where artefacts are stored)</param>
        public virtual void ConsumeStartingArtefacts(TechnologyManager manager, ICollection<Outpost> outposts) { }

        /// <summary>
        /// Construct correct type of Prerequisite from an XML element
        /// </summary>
        /// <param name="element">Navigator to XML element</param>
        /// <returns>the Prerequisite</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if element is null")]
        public static Prerequisite Factory(XPathNavigator element)
        {
            if ((element.Name == "AnyOneOf") || (element.Name == "AllOf"))
            {
                return new MultiPrerequisite(element, (element.Name == "AllOf"));
            }
            else if ((element.Name == "xnetref") || (element.Name == "topicref"))
            {
                return new TechnologyPrerequisite(element);
            }
            else if (element.Name == "itemref")
            {
                return new ItemPrerequisite(element);
            }
            else
            {
                Debug.Assert(false);
                return null;
            }
        }
    }
}
