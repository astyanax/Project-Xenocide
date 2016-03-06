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
* @file PatrolMission.cs
* @date Created: 2007/03/11
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Xenocide.Model.Geoscape.Craft
{
    /// <summary>
    /// Mission where
    /// 1. Craft heads towards position
    /// 2. Craft stays at position as long as possible
    /// 3. When craft runs low on fuel (during stage 1 or 2), it returns to base
    /// </summary>
    [Serializable]
    public class PatrolMission : Mission
    {
        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="craft">The craft this mission is for</param>
        /// <param name="target">Position the craft is to patrol</param>
        public PatrolMission(Craft craft, GeoPosition target)
            :
            base(craft)
        {
            SetState(new PatrolState(this, target));
        }

        /// <summary>
        /// We've returned to our home base
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
