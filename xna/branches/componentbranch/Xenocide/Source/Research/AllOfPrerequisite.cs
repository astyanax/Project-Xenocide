using System;
using System.Collections.Generic;

namespace Xenocide.Research
{
    /// <summary>
    /// A set of prerequisites that all must be satisfied for the prerequisite as
    /// a whole to be satisfied
    /// </summary>
    public class AllOfPrerequisite : ITechPrerequisite
    {
        /// <summary>
        /// set of prerequisites that must be satisfied
        /// </summary>
        private IList<ITechPrerequisite> prerequisites;
      
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="prerequisites">set of prerequisites that must be satisfied</param>
        public AllOfPrerequisite ( IList<ITechPrerequisite> prerequisites )
        {
            if (prerequisites == null)
                throw new ArgumentNullException("AllOf Prerequisites list cannot be null");

            if (prerequisites.Count == 0)
                throw new ArgumentException("AllOf Prerequisites list cannot be empty");

            this.prerequisites = prerequisites;
        }

        /// <summary>
        /// Check if all preconditions are satisfied
        /// </summary>
        /// <returns>true if they're satisfied</returns>
        public bool Evaluate()
        {
            foreach (ITechPrerequisite prerequisite in this.prerequisites)
            {
                if ( !prerequisite.Evaluate() )
                    return false;
            }
            
            return true;
        }
    }
}