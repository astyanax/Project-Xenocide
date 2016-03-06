using System;
using System.Collections.Generic;
using System.Text;

namespace Xenocide.Model.Geoscape.Research
{
    /// <summary>
    /// Interface for different prerequisite for a ResearchTopic to become researchable.
    /// </summary>
    public interface IResearchPreRequisite
    {
        /// <summary>
        /// Check if this prerequisite is fullfilled.
        /// </summary>
        /// <returns>true if it is fullfilled, false otherwise</returns>
        bool IsSatisfied();
    }

    /// <summary>
    /// IResearchPreRequisite implementation that checks for the exitance of a particular XNET-Entry.
    /// </summary>
    [Serializable]
    public class ResearchXNETPreRequisite : IResearchPreRequisite
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The symbolic name of the XNetEntry that is to be checked.</param>
        public ResearchXNETPreRequisite(string id)
        {
            this.id = id;
        }

        /// <summary>
        /// Returns true if the particular XNETEntry is available.
        /// </summary>
        /// <returns></returns>
        public bool IsSatisfied()
        {
            return (Xenocide.GameState.XNetState[id]);
        }

        private string id;
    }
}
