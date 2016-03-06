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
* @file ReturnToBaseState.cs
* @date Created: 2007/02/18
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// State that represents Craft returning to it's base
    /// </summary>
    [Serializable]
    public class ReturnToBaseState : MoveToPositionState
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mission">mission that owns this state</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Is validated in base class")]
        public ReturnToBaseState(Mission mission)
            :
            base(mission, mission.Craft.HomeBase.Position)
        {
        }
    }
}
