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
* @file InterceptMission.cs
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
    /// Mission where
    /// 1. Craft heads towards prey
    /// 2. If craft reaches target, dogfight starts
    /// 3. At end of dogfight, if this craft survives, it returns to base
    /// 4. If craft fails to reach target, returns to base
    /// </summary>
    [Serializable]
    public class InterceptMission : Mission
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="craft">craft that "owns" this mission</param>
        /// <param name="target">The UFO the aircraft is to hunt</param>
        public InterceptMission(Craft craft, Craft target)
            :
            base(craft)
        {
            SetState(new InterceptCraftState(this, target));
        }

        /// <summary>
        /// We've reached a destination (our home base)
        /// </summary>
        public override void OnDestinationReached()
        {
            // we're back at home base
            SetState(new InBaseState(this));
        }

        #region Fields

        #endregion
    }
}
