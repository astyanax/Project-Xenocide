using System;
using System.Collections.Generic;
using System.Text;

namespace Xenocide.Research
{
    /// <summary>
    /// Class that represents a set of grants that will all be gained when 
    /// a research project is finished
    /// </summary>
    public class AllOfGrant : ITechGrant
    {
        /// <summary>
        /// The set of things that will be granted
        /// </summary>
        private IList<ITechGrant> grants;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="grants">The set of things that will be granted</param>
        public AllOfGrant(IList<ITechGrant> grants)
        {
            if (grants == null)
                throw new ArgumentNullException("Tech Grant list cannot be null");

            this.grants = grants;
        }

        /// <summary>
        /// Give player the reward
        /// </summary>
        public void Grant()
        {
            foreach (ITechGrant grant in grants)
                grant.Grant();
        }
    }
}
