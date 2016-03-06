using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xenocide.Model.Geoscape.Research
{
    public interface IResearchService
    {
        IEnumerable<ResearchTopic> ResearchableTopics { get; }
        IEnumerable<ResearchTopic> CurrentResearchProjects { get; }

        ResearchGraph ResearchGraph { set; }
        TopicResearchedHandler TopicResearched { get; set; }
        ResearchTopic this[string id] { get; }
    }

    public class ResearchService : IResearchService
    {
        public ResearchService(Game game)
        {
            game.Services.AddService(typeof(IResearchService), this);   
        }

        #region IResearchService Member

        public IEnumerable<ResearchTopic> ResearchableTopics
        {
            get { return researchGraph.ResearchableTopics; }
        }

        public IEnumerable<ResearchTopic> CurrentResearchProjects
        {
            get { return researchGraph.CurrentResearchProjects; }
        }

        public ResearchGraph ResearchGraph 
        {
            set
            {
                if (researchGraph != null)
                    researchGraph.TopicResearched -= TopicResearched;
                researchGraph = value;
                if (researchGraph != null)
                    researchGraph.TopicResearched += TopicResearched;
            }
        }

        public TopicResearchedHandler TopicResearched 
        {
            get
            {
                return topicResearched;
            }
            set
            {
                if (researchGraph != null)
                    researchGraph.TopicResearched -= topicResearched;
                topicResearched = value;
                if (researchGraph != null)
                    researchGraph.TopicResearched += topicResearched;
            }
        }

        public ResearchTopic this[string id] 
        {
            get
            {
                return researchGraph[id];
            }
        }

        #endregion

        private ResearchGraph researchGraph;
        private TopicResearchedHandler topicResearched;
    }
}
