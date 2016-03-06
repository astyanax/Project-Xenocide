using System;
using System.Collections.Generic;

namespace Xenocide.Research
{
    /// <summary>
    /// Class representing the projects that can be researched
    /// </summary>
    public class ResearchTree
    {
        #region Research Node Class

        /// <summary>
        /// A project that can be researched
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible",
            Justification="We are C# programmers, we can handle nested classes")]
        public class ResearchNode
        {
            private IList<ResearchNode> parents;
            private IList<ResearchNode> children;
            private string topic;
            
            /// <summary>
            /// Identifier for the X-Net entry that describes this Research Node
            /// </summary>
            private string xnetId;
            private IList<ITechPrerequisite> prerequisites;
            private IList<ITechGrant> grants;
            
            private bool persistent;
            private bool unlocked;
            private int researchCost;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="topic">name of this project</param>
            /// <param name="parents"></param>
            /// <param name="children"></param>
            /// <param name="prerequisites">condition(s) that must be satisfied before project can start</param>
            /// <param name="grants">reward for completing the project</param>
            /// <param name="xnetId">Identifier for the X-Net entry that describes this project</param>
            /// <param name="researchCost">person days project will take to complete</param>
            /// <param name="persistent"></param>
            public ResearchNode(string topic, IList<ResearchNode> parents, IList<ResearchNode> children, IList<ITechPrerequisite> prerequisites, IList<ITechGrant> grants, string xnetId, int researchCost, bool persistent)
            {
                if (String.IsNullOrEmpty(topic))
                    throw new ArgumentException("Topic cannot be empty.");

                if (parents == null || children == null || prerequisites == null || grants == null ) 
                    throw new ArgumentNullException("Lists provided cannot be null.");

                if (String.IsNullOrEmpty(xnetId))
                    throw new ArgumentNullException("Xnet ID must exist.");

                this.parents = parents;
                this.children = children;
                this.topic = topic;
                this.xnetId = xnetId;
                this.prerequisites = prerequisites;
                this.grants = grants;
                this.researchCost = researchCost;
                this.persistent = persistent;
            }

            /// <summary>
            /// dteviot: I have no idea what this is for (it replciates prerequisites)
            /// </summary>
            public IList<ResearchNode> Parents { get { return this.parents; } }

            /// <summary>
            /// dteviot: I have no idea what this is for (it replciates grants)
            /// </summary>
            public IList<ResearchNode> Children { get { return this.children; } }

            /// <summary>
            /// name of this project
            /// </summary>
            public string Topic { get { return topic; } }

            /// <summary>
            /// Identifier for the X-Net entry that describes this Research Node
            /// </summary>
            public string XNetId { get { return xnetId; } }

            /// <summary>
            /// Condition(s) that must be satisfied before project can start
            /// </summary>
            public IList<ITechPrerequisite> Prerequisites { get { return prerequisites; } }

            /// <summary>
            /// Reward for completing the project
            /// </summary>
            public IList<ITechGrant> Grants { get { return grants; } }

            /// <summary>
            /// Person days project will take to complete
            /// </summary>
            public int ResearchCost
            {
                get { return this.researchCost; }
            }

            /// <summary>
            /// At this point in time, is player able to start this project?
            /// </summary>
            public bool Researcheable
            {
                get
                {
                    if (unlocked && !persistent)
                        return false;
                    else
                    {
                        foreach (ITechPrerequisite prerequisite in prerequisites)
                        {
                            if (!prerequisite.Evaluate())
                                return false;
                        }
                        return false;
                    }
                }
            }

            /// <summary>
            /// dteviot: I'm not sure what this does
            /// </summary>
            public bool Unlocked
            {
                get { return this.unlocked && !persistent; }
            }

            /// <summary>
            /// dteviot: I'm not sure what this does
            /// </summary>
            public void Unlock()
            {
                this.unlocked = true;

                foreach (ITechGrant grant in grants)
                    grant.Grant();
            }
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields",
            Justification="Code is under development")]
        private ResearchNode root;

        /// <summary>
        /// Constructor
        /// </summary>
        public ResearchTree(ResearchNode root)
        {
            if (root == null)
                throw new ArgumentNullException("root");

            this.root = root;
        }


    }
}
