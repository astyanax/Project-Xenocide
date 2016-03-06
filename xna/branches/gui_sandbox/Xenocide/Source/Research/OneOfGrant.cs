using System;
using System.Collections.Generic;
using System.Text;

namespace Xenocide.Research
{
    /// <summary>
    /// Class that represents a set of grants which the Player will recieve ONE of 
    /// (chosen at random) when a research project is finished
    /// </summary>
    public class OneOfGrant : ITechGrant
    {
        /// <summary>
        /// set we pick an element to grant from
        /// </summary>
        private IList<ITechGrant> grants;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="grants">set we pick an element to grant from</param>
        public OneOfGrant(IList<ITechGrant> grants)
        {
            if (grants == null)
                throw new ArgumentNullException("Tech Grant list cannot be null.");

            if (grants.Count <= 1)
                throw new ArgumentException("Tech Grant list cannot be a single element or zero elements.");

            this.grants = grants;
        }

        /// <summary>
        /// Randomly (with no memory making it harder for harder technologies ;) ) select a Grant object and
        /// granting it.
        /// </summary>
        public void Grant()
        {
            Random rnd = new Random();
            int i = rnd.Next(grants.Count - 1);

            grants[i].Grant();
        }
    }
}
