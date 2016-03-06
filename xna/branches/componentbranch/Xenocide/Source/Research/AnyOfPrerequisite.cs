using System;
using System.Collections.Generic;

namespace Xenocide.Research
{
    /// <summary>
    /// A set of prerequisites that requires any one of the set to be satisfied 
    /// for the prerequisite as a whole to be satisfied
    /// </summary>
    public class AnyOfPrerequisite : ITechPrerequisite
    {
        /// <summary>
        /// set of prerequisites to check
        /// </summary>
        private IList<ITechPrerequisite> prerequisites;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="prerequisites">set of prerequisites to check</param>
        public AnyOfPrerequisite(IList<ITechPrerequisite> prerequisites)
        {
            if (prerequisites == null)
                throw new ArgumentNullException("AnyOf Prerequisites list cannot be null");
            
            if (prerequisites.Count <= 1)
                throw new ArgumentException("AnyOf Prerequisites list cannot be empty");

            this.prerequisites = prerequisites;
        }

        /// <summary>
        /// Check if any precondition is satisfied
        /// </summary>
        /// <returns>true if at least one is satisfied</returns>
        public bool Evaluate()
        {
            foreach ( ITechPrerequisite prerequisite in this.prerequisites )
            {
                if ( prerequisite.Evaluate() )
                    return true;
            }
            
            return false;
        }
    }
}