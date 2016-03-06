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
* @file ResearchGraph.cs
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
using System.Xml;
using System.Xml.XPath;


using ProjectXenocide.Utils;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Facilities;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape.Outposts;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.StaticData.Research
{
    /// <summary>
    /// The list of topics a player can (eventually) research
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public sealed partial class ResearchGraph : IEnumerable<ResearchTopic>
    {
        /// <summary>
        /// Load the list of topics from a file
        /// </summary>
        /// <param name="filename">Name of the XML file</param>
        public void Populate(string filename)
        {
            // Set up XPathNavigator
            const string xmlns = "ResearchConfig";
            XPathNavigator nav = Util.MakeValidatingXPathNavigator(filename, xmlns);
            XmlNamespaceManager manager = new XmlNamespaceManager(nav.NameTable);
            manager.AddNamespace("r", xmlns);

            // Process the XML file
            foreach (XPathNavigator topicElement in nav.Select("/r:researchtopics/r:topic", manager))
            {
                ResearchTopic topic = new ResearchTopic(topicElement, manager);

                // Starting tech doesn't go in tree, it's a special case
                if (topic.Id == "RES_STARTING_TECHNOLOGY")
                {
                    startingTech = topic;
                }
                else
                {
                    topics.Add(topic.Id, topic);
                }
            }
        }

        /// <summary>
        /// Give player the starting technologies
        /// </summary>
        /// <param name="manager">player's manager to give techs to</param>
        public void GiveStartingTech(TechnologyManager manager)
        {
            startingTech.GrantReward(manager);
        }

        /// <summary>
        /// Return list of topics that a player can start researching
        /// </summary>
        /// <param name="manager">the technologies the player has</param>
        /// <param name="outposts">the outposts the player has (where artefacts are stored)</param>
        /// <returns></returns>
        public IEnumerable<ResearchTopic> StartableTopics(TechnologyManager manager, ICollection<Outpost> outposts)
        {
            return Util.FilterColection(topics.Values,
                delegate(ResearchTopic topic) { return topic.CanResearch(manager, outposts); }
            );
        }

        /// <summary>
        /// Retreive a ResearchTopic by name
        /// </summary>
        /// <param name="id">internal name used to ID the ResearchTopic</param>
        /// <returns>the ResearchTopic</returns>
        public ResearchTopic this[string id] { get { return topics[id]; } }

        /// <summary>
        /// Implement generics IEnumerable&lt;T&gt; interface
        /// </summary>
        /// <returns>The Enumerator</returns>
        IEnumerator<ResearchTopic> IEnumerable<ResearchTopic>.GetEnumerator()
        {
            return topics.Values.GetEnumerator();
        }

        /// <summary>
        /// Implement IEnumerable interface
        /// </summary>
        /// <returns>The Enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return topics.Values.GetEnumerator();
        }

        /// <summary>
        /// Check that the loaded research tree is valid
        /// </summary>
        /// <param name="xnetEntryList">X-Net entries, to check against research tree</param>
        /// <param name="facilities">Facilities, to check against research tree</param>
        /// <param name="items">Items, to check against research tree</param>
        public void Validate(XNetEntryCollection xnetEntryList, FacilityInfoCollection facilities,
            ItemCollection items)
        {
            TechnologyManager mgr = LookForInaccessableTopics();
            ValidateXNetAgainstTech(mgr, xnetEntryList);
            ValidateFacilityAgainstTech(mgr, facilities);
            ValidateItemsAgainstTech(mgr, items);
        }

        /// <summary>
        /// Look for ResearchTopics that can't be researched
        /// </summary>
        /// <returns>List of all available technologies</returns>
        private TechnologyManager LookForInaccessableTopics()
        {
            Dictionary<string, bool> solved = new Dictionary<string, bool>();
            TechnologyManager mgr = new TechnologyManager(this);

            // cycle through, adding techs until we can't add any more
            GiveStartingTech(mgr);
            for (bool added = true; added; )
            {
                added = false;
                foreach (ResearchTopic topic in topics.Values)
                {
                    if (topic.IsSatisfied(mgr))
                    {
                        solved[topic.Id] = true;
                        while (topic.IsRewardLeft(mgr))
                        {
                            topic.GrantReward(mgr);
                            added = true;
                        }
                    }
                }
            }

            // did we miss any?
            foreach (ResearchTopic topic in topics.Values)
            {
                if (!solved.ContainsKey(topic.Id))
                {
                    throw new XPathException(Util.StringFormat(Strings.EXCEPTION_UNREACHABLE_RESEARCH_TOPIC, topic.Id));
                }
            }
            return mgr;
        }

        /// <summary>
        /// Check that every X-Net entry has a tech, and every X-Net tech has an entry
        /// </summary>
        /// <param name="mgr">List of all avaiable techs</param>
        /// <param name="xnetEntryList">List of all X-Net entries</param>
        private static void ValidateXNetAgainstTech(TechnologyManager mgr, XNetEntryCollection xnetEntryList)
        {
            foreach (XNetEntry entry in xnetEntryList)
            {
                if (!mgr.IsAvailable(entry.Id))
                {
                    throw new XPathException(Util.StringFormat(Strings.EXCEPTION_UNREACHABLE_XNET_ENTRY, entry.Name));
                }
            }

            foreach (Technology tech in mgr)
            {
                if ((TechnologyType.XNet == tech.Type) && (null == xnetEntryList.FindById(tech.Id)))
                {
                    throw new XPathException(Util.StringFormat(Strings.EXCEPTION_MISSING_XNET_ENTRY, tech.Id));
                }
            }
        }

        /// <summary>
        /// Check that every Facility has a tech, and every Facility tech has a facility
        /// </summary>
        /// <param name="mgr">List of all avaiable techs</param>
        /// <param name="facilities">List of all facilities</param>
        public static void ValidateFacilityAgainstTech(TechnologyManager mgr, FacilityInfoCollection facilities)
        {
            foreach (FacilityInfo facility in facilities)
            {
                if (!mgr.IsAvailable(facility.Id))
                {
                    throw new XPathException(Util.StringFormat(Strings.EXCEPTION_UNREACHABLE_FACILITY, facility.Name));
                }
            }

            foreach (Technology tech in mgr)
            {
                if ((TechnologyType.Facility == tech.Type) && (null == facilities[tech.Id]))
                {
                    throw new XPathException(Util.StringFormat(Strings.EXCEPTION_MISSING_FACILITY, tech.Id));
                }
            }
        }

        /// <summary>
        /// Check that every Item has a tech, and every Item tech has an item
        /// </summary>
        /// <param name="mgr">List of all avaiable techs</param>
        /// <param name="items">List of all items</param>
        public static void ValidateItemsAgainstTech(TechnologyManager mgr, ItemCollection items)
        {
            foreach (ItemInfo item in items)
            {
                if (!mgr.IsAvailable(item.Id))
                {
                    // if X-Corp can buy or build the item, then we need a technology to access it
                    if (item.CanPurchase || (null != item.BuildInfo))
                    {
                        throw new XPathException(Util.StringFormat(Strings.EXCEPTION_UNREACHABLE_ITEM, item.Name));
                    }
                }
            }

            foreach (Technology tech in mgr)
            {
                if ((TechnologyType.Item == tech.Type) && (null == items[tech.Id]))
                {
                    throw new XPathException(Util.StringFormat(Strings.EXCEPTION_MISSING_ITEM, tech.Id));
                }
            }
        }

        #region Fields

        /// <summary>
        /// The actual list of topics
        /// </summary>
        private Dictionary<string, ResearchTopic> topics = new Dictionary<string, ResearchTopic>();

        /// <summary>
        /// Tech that player gets at start of game
        /// </summary>
        private ResearchTopic startingTech;

        #endregion Fields
    }
}
