using System;
using System.Collections.Generic;

namespace Xenocide.Research
{
    /// <summary>
    /// A precondition that must be satisfied before a research project can start
    /// </summary>
    public interface ITechPrerequisite
    {
        /// <summary>
        /// Check if precondition is satisfied
        /// </summary>
        /// <returns>true if it's satisfied</returns>
        bool Evaluate();
    }
}