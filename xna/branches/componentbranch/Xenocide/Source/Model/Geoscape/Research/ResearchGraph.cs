using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using System.Xml;
using Microsoft.Xna.Framework;

namespace Xenocide.Model.Geoscape.Research
{
    /// <summary>
    /// Delegate definition for callback functions that are called when a topic has been researched.
    /// </summary>
    /// <param name="topic"></param>
    public delegate void TopicResearchedHandler(ResearchTopic topic);

    /// <summary>
    /// Data Structure for holding info about Research items
    /// </summary>
    [Serializable]
    public sealed class ResearchGraph : GameStateComponent
    {
        private Dictionary<string, ResearchTopic> researchTopics;

        /// <summary>
        /// Construct a new Research Graph from an XML file
        /// </summary>
        /// <param name="filename">The filename of the XML file to be parsed</param>
        public ResearchGraph(string filename)
        {
            researchTopics = new Dictionary<string, ResearchTopic>();
            XPathNavigator nav = (new XPathDocument(filename)).CreateNavigator();
            XmlNamespaceManager manager = new XmlNamespaceManager(nav.NameTable);
            manager.AddNamespace("r", "ResearchConfig");
            foreach (XPathNavigator topicElement in nav.Select("/r:researchtopics/r:topic", manager))
            {
                ResearchTopic topic = new ResearchTopic(topicElement, manager);
                researchTopics.Add(topic.Id, topic);
            }

            //TODO: replace by event scheduling when ready
            //Xenocide.GameState.GeoData.GeoTime.DayPassed += NextDay;
        }

        protected override void  OnGameSet()
        {
            ((IResearchService)Game.Services.GetService(typeof(IResearchService))).ResearchGraph = this;
            foreach (ResearchTopic topic in researchTopics.Values)
                topic.Game = Game;
        }

        /// <summary>
        /// Advance all research projects one day. This delegates to the Topics' NextDay() methods.
        /// </summary>
        public void NextDay()
        {
            /*
            foreach (ResearchTopic topic in researchTopics.Values)
            {
                if (topic.NextDay())
                    TopicResearched(topic);
            }
            */
        }

        /// <summary>
        /// Returns a research topic by symbolic id
        /// </summary>
        /// <param name="id">The id of the topic to retrieve</param>
        /// <returns>The topic with the requested id</returns>
        internal ResearchTopic this[string id]
        {
            get
            {
                return researchTopics[id];
            }
        }

        /// <summary>
        /// A list of all topics that can currently be researched.
        /// </summary>
        internal IEnumerable<ResearchTopic> ResearchableTopics
        {
            get
            {
                List<ResearchTopic> result = new List<ResearchTopic>();
                foreach (ResearchTopic topic in researchTopics.Values)
                {
                    if (topic.IsResearchable)
                        yield return topic;
                }
            }
        }

        /// <summary>
        /// A list of all topics that currently have scientists assigned and are being researched.
        /// </summary>
        internal IEnumerable<ResearchTopic> CurrentResearchProjects
        {
            get
            {
                List<ResearchTopic> result = new List<ResearchTopic>();
                foreach (ResearchTopic topic in researchTopics.Values)
                {
                    if (topic.Scientists > 0)
                        yield return topic;
                }
            }
        }

        /// <summary>
        /// Delegate that notifies listeners if a new topic has been researched.
        /// </summary>
        [NonSerialized]
        internal TopicResearchedHandler TopicResearched;
    }
}
