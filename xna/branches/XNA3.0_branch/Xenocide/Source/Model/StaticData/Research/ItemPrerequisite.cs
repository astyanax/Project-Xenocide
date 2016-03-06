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
* @file ItemPrerequisite.cs
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
using ProjectXenocide.Model.StaticData.Items;

#endregion

namespace ProjectXenocide.Model.StaticData.Research
{
    /// <summary>
    /// Precondition that player must have a type of artefact on hand before a ResearchTopic can be researched
    /// </summary>
    public class ItemPrerequisite : Prerequisite
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="element">Navigator to XML element</param>
        public ItemPrerequisite(XPathNavigator element)
        {
            artefact = Xenocide.StaticTables.ItemList[Util.GetStringAttribute(element, "name")];
        }

        /// <summary>
        /// Does the player have all the technologies needed to start research
        /// </summary>
        /// <param name="manager">the technologies the player has</param>
        /// <returns>true if player has all needed technologies</returns>
        /// <remarks>This should ONLY be used for validating the ResearchGraph</remarks>
        public override bool IsSatisfied(TechnologyManager manager)
        {
            // As this requirement is looking for an artefact, we don't care about available tech
            return true;
        }

        /// <summary>
        /// Does the player have all the technologies and sample artefacts needed to start research?
        /// </summary>
        /// <param name="manager">the technologies the player has</param>
        /// <param name="outposts">the outposts the player has (where artefacts are stored)</param>
        /// <returns>true if player has all needed technologies and sample artefacts</returns>
        public override bool IsSatisfied(TechnologyManager manager, ICollection<Outpost> outposts)
        {
            foreach (Outpost outpost in outposts)
            {
                if (0 < outpost.Inventory.NumberInInventory(artefact))
                {
                    return true;
                }
            }

            // if get here, artefact not available
            return false;
        }

        /// <summary>
        /// Consume any artefact(s) that are needed to begin research
        /// </summary>
        /// <param name="manager">the technologies the player has</param>
        /// <param name="outposts">the outposts the player has (where artefacts are stored)</param>
        public override void ConsumeStartingArtefacts(TechnologyManager manager, ICollection<Outpost> outposts)
        {
            foreach (Outpost outpost in outposts)
            {
                if (0 < outpost.Inventory.NumberInInventory(artefact))
                {
                    outpost.Inventory.Remove(artefact.FromOutpost(outpost.Inventory));
                    return;
                }
            }

            // if get here, something's wrong
            Debug.Assert(false);
        }

        #region Fields

        /// <summary>
        /// artefact player needs to have to satisfy prerequisite
        /// </summary>
        private ItemInfo artefact;

        #endregion Fields
    }
}
