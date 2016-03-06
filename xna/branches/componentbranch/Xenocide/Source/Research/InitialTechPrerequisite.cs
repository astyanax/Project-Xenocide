using System;
using System.Collections.Generic;
using System.Text;

namespace Xenocide.Research
{
    /// <summary>
    /// The prerequiste for a technology that Player automatically gets when game begins
    /// </summary>
    public class InitialTechPrerequisite : ITechPrerequisite
    {
        /// <summary>
        /// Check if precondition is satisfied
        /// </summary>
        /// <returns>true if it's satisfied</returns>
        public bool Evaluate()
        {
            return true;
        }
    }
}
