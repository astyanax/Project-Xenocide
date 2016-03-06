using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using System.Xml;
using Xenocide.Utils;

namespace Xenocide.Model.Geoscape.Research
{
    /// <summary>
    /// Class representing the static info about a research topic
    /// </summary>
    [Serializable]
    public sealed class ResearchTopic : GameStateComponent
    {
        /// <summary>
        /// Construct a new ResearchTopic from XMLNode data
        /// </summary>
        /// <param name="topicNode"></param>
        /// <param name="manager"></param>
        public ResearchTopic(XPathNavigator topicNode, XmlNamespaceManager manager)
        {
            Util.GetAttribute(topicNode, "name", ref id);
            Util.GetAttribute(topicNode, "time", ref researchTime);
            
            totalResearchTime = researchTime;

            ReadPrerequites(topicNode.SelectSingleNode("r:prerequisite", manager), manager);
            ReadGrants(topicNode.SelectSingleNode("r:grants", manager), manager);

            timeEvent = new ScheduledTimeEvent();
            timeEvent.EventCompleted = new TimeEventCompletedHandler(Apply);
            timeEvent.Duration = new TimeSpan(researchTime, 0,0,0);
            timeEvent.Multiplier = 0.0;


            if (researchTime == 0)
                Apply(null);
        }

        private void ReadPrerequites(XPathNavigator prereqsNode, XmlNamespaceManager manager)
        {
            prereqs = new LinkedList<IResearchPreRequisite>();

            foreach (XPathNavigator prereqElement in prereqsNode.SelectChildren(XPathNodeType.Element))
            {
                if (prereqElement.Name == "xnetref")
                    prereqs.AddLast(new ResearchXNETPreRequisite(prereqElement.GetAttribute("name", String.Empty)));
                else
                    System.Console.WriteLine("Unsupported ResearchPrereq: " + prereqElement.Name);
            }
        }

        private void ReadGrants(XPathNavigator grantsNode, XmlNamespaceManager manager)
        {
            grants = new LinkedList<IResearchGrant>();
            
            string quantityStr = "";
            Util.GetAttribute(grantsNode, "quantity", ref quantityStr);
            grantType = (quantityStr == "AllOf") ? GrantType.ALL_OF : GrantType.ONE_OF;

            foreach (XPathNavigator grantElement in grantsNode.SelectChildren(XPathNodeType.Element))
            {
                string name = "";
                Util.GetAttribute(grantElement, "name", ref name);
                if(grantElement.Name == "xnetref")
                    grants.AddLast(new ResearchXNETGrant(name));
                else
                    System.Console.WriteLine("Unsupported ResearchGrant: " + grantElement.Name);
            }
        }

        /// <summary>
        /// The number of scientists assigned to this project.
        /// </summary>
        public int Scientists
        {
            get
            {
                return numScientists;
            }

            set
            {
                numScientists = value;
            }
        }

        /// <summary>
        /// The number of days of research left for this project using the currently assigned amount of
        /// scientists. If 0 scientists are assigned, this property has the value 0
        /// </summary>
        public uint DaysLeft
        {
            get
            {
                if (numScientists == 0)
                    return 0;
                else
                {
                    int result = researchTime / (int)numScientists;
                    if (researchTime % numScientists > 0)
                        ++result;

                    return (uint)((result > 0) ? result : 0);
                }
            }
        }

        /// <summary>
        /// Checks if this topic has already been fully researched
        /// </summary>
        public bool IsResearched
        {
            get
            {
                return researchTime <= 0;
            }
        }

        /// <summary>
        /// Checks if this topic is available for research
        /// </summary>
        public bool IsResearchable
        {
            get
            {
                if (IsResearched)
                    return false;

                foreach (IResearchPreRequisite prereq in prereqs)
                {
                    if (!prereq.IsSatisfied())
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Apply all grants of this researchtopic. All scientists are removed from the project.
        /// If its a project that grants one of several benefits it is again made available for research.
        /// </summary>
        private void Apply(ScheduledTimeEvent e)
        {
            if (IsResearched)
            {
                numScientists = 0;

                if (grantType == GrantType.ALL_OF)
                {
                    foreach (IResearchGrant grant in grants)
                    {
                        grant.Apply(Game);
                    }
                }
                else //ONE_OF
                {
                    researchTime = totalResearchTime;
                    
                    List<IResearchGrant> candidates = new List<IResearchGrant>(grants);

                    //shuffle the candidates
                    for (int idx = candidates.Count - 1; idx > 0; idx--)
                    {
                        int position = Xenocide.Rng.Next(idx + 1);
                        IResearchGrant temp = candidates[idx];

                        //try to apply the grant, and if it can still be applied, we are done
                        if (temp.Apply(Game))
                            break;

                        //if not, we move it to the back and then try a different one
                        //this is basicly the algorithm for creating an inplace permutation
                        //but we can skip once we have found a working candidate
                        candidates[idx] = candidates[position];
                        candidates[position] = temp;
                    }
                }
            }
        }

        #region Fields
        /// <summary>
        /// The symbolic id of this topic
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// The time of research left in man-days
        /// </summary>
        public int ResearchTime { get { return researchTime; } }

        private string id;
        private int researchTime;
        private int totalResearchTime;
        private int numScientists;

        private LinkedList<IResearchPreRequisite> prereqs;
        private LinkedList<IResearchGrant> grants;
        private enum GrantType { ONE_OF, ALL_OF };
        private GrantType grantType;
        private ScheduledTimeEvent timeEvent;

        private const double MILLIS_PER_DAY = 86400000.0;
        
        #endregion
    }
}
