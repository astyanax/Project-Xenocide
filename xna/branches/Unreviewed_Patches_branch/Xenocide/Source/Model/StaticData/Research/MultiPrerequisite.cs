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
* @file MultiPrerequisite.cs
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
    /// Player has multiple conditions that must be satisfied before a ResearchTopic can be researched
    /// </summary>
    public class MultiPrerequisite : Prerequisite
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="element">Navigator to XML element</param>
        /// <param name="allOf">Namespace used in item.xml</param>
        public MultiPrerequisite(XPathNavigator element, bool allOf)
        {
            this.allOf = allOf;
            foreach (XPathNavigator condition in element.SelectChildren(XPathNodeType.Element))
            {
                preconditions.Add(Prerequisite.Factory(condition));
            }
        }

        /// <summary>
        /// Does the player have all the technologies needed to start research
        /// </summary>
        /// <param name="manager">the technologies the player has</param>
        /// <returns>true if player has all needed technologies</returns>
        /// <remarks>This should ONLY be used for validating the ResearchGraph</remarks>
        public override bool IsSatisfied(TechnologyManager manager)
        {
            int count = 0;
            foreach (Prerequisite p in preconditions)
            {
                if (p.IsSatisfied(manager))
                {
                    ++count;
                }
            }
            return allOf ? (preconditions.Count == count) : (0 < count);
        }

        /// <summary>
        /// Does the player have all the technologies and sample artefact needed to start research?
        /// </summary>
        /// <param name="manager">the technologies the player has</param>
        /// <param name="outposts">the outposts the player has (where artefact are stored)</param>
        /// <returns>true if player has all needed technologies and sample artefact</returns>
        public override bool IsSatisfied(TechnologyManager manager, ICollection<Outpost> outposts)
        {
            int count = 0;
            foreach (Prerequisite p in preconditions)
            {
                if (p.IsSatisfied(manager, outposts))
                {
                    ++count;
                }
            }
            return allOf ? (preconditions.Count == count) : (0 < count);
        }

        /// <summary>
        /// Consume any artefact(s) that are needed to begin research
        /// </summary>
        /// <param name="manager">the technologies the player has</param>
        /// <param name="outposts">the outposts the player has (where artefacts are stored)</param>
        public override void ConsumeStartingArtefacts(TechnologyManager manager, ICollection<Outpost> outposts)
        {
            foreach (Prerequisite p in preconditions)
            {
                if (p.IsSatisfied(manager, outposts))
                {
                    p.ConsumeStartingArtefacts(manager, outposts);

                    // if we only needed to satisfy one, we're done
                    if (!allOf)
                    {
                        return;
                    }
                }
            }
        }

        #region Fields

        /// <summary>
        /// Do all conditions need to be satified, or is it "any one"
        /// </summary>
        private bool allOf;

        /// <summary>
        /// Conditions player needs to have to satisfy prerequisite
        /// </summary>
        private List<Prerequisite> preconditions = new List<Prerequisite>();

        #endregion Fields
    }
}
