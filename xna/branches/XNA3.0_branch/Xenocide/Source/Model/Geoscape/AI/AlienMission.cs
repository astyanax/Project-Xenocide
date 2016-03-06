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
* @file AlienMission.cs
* @date Created: 2007/08/11
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace ProjectXenocide.Model.Geoscape.AI
{
    /// <summary>
    /// The different types of missions the Overmind can select
    /// </summary>
    public enum AlienMission
    {
        /// <summary>
        /// UFOs sent off on research
        /// </summary>
        Research,

        /// <summary>
        /// UFOs sent to get food supplies
        /// </summary>
        Harvest,

        /// <summary>
        /// UFOs sent to anal probe Eric Cartman
        /// </summary>
        Abduction,

        /// <summary>
        /// Try to convert country's leaders
        /// </summary>
        Infiltration,

        /// <summary>
        /// Build and outpost
        /// </summary>
        Outpost,

        /// <summary>
        /// Terrorise human city.
        /// </summary>
        Terror,

        /// <summary>
        /// Destroy and X-Corp outpost
        /// </summary>
        Retaliation,

        /// <summary>
        /// Provide outpost with supplies
        /// </summary>
        Supply,

    }
}
