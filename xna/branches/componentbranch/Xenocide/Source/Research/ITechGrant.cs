using System;
using System.Collections.Generic;

namespace Xenocide.Research
{
    /// <summary>
    /// Class that represents the "reward" for completing a research project
    /// </summary>
    public interface ITechGrant
    {
        /// <summary>
        /// Give player the reward
        /// </summary>
        void Grant();
    }
}