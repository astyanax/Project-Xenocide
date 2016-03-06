#region Copyright
/*
--------------------------------------------------------------------------------
This source file is part of Xenocide
  by  Project Xenocide Team

For the latest info on Xenocide, see http://www.projectxenocide.com/

This work is licensed under the Creative Commons
Attribution-NonCommercial-ShareAlike 2.5 License.

To view a copy of this license, visit
http://creativecommons.org/licenses/by-nc-sa/2.5/
or send a letter to Creative Commons, 543 Howard Street, 5th Floor,
San Francisco, California, 94105, USA.
--------------------------------------------------------------------------------
*/

/*
* @file BaseStatistics.cs
* @date Created: 2007/05/13
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using Xenocide.Model.StaticData.Facilities;
using Xenocide.UI.Dialogs;

#endregion

namespace Xenocide.Model.Geoscape.HumanBases
{
    /// <summary>
    /// Represents the capabilities of a human base
    /// </summary>
    [Serializable]
    public class BaseStatistics
    {
        #region Fields

        /// <summary>
        /// The storage limits of the base
        /// </summary>
        public BaseCapacities Capacities { get { return capacities; } }

        // private bool hasShortRangeNeudar;

        // private bool hasLongRangeNeudar;

        // private bool haseTachyonDetector;

        /// <summary>
        /// The storage limits of the base
        /// </summary>
        private BaseCapacities capacities = new BaseCapacities();

        #endregion Fields

        #region UnitTests

        #endregion UnitTests
    }
}
