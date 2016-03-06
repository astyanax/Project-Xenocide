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
* @file InBaseState.cs
* @date Created: 2007/02/17
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Xenocide.Model.Geoscape.HumanBases;

#endregion

namespace Xenocide.Model.Geoscape.Craft
{
    /// <summary>
    /// State that represents Aircraft waiting in a base
    /// </summary>
    [Serializable]
    public class InBaseState : MissionState
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mission">Mission that owns this state</param>
        public InBaseState(Mission mission)
            :
            base(mission)
        {
        }

        /// <summary>
        /// Change the craft, based on time elapsed
        /// </summary>
        /// <param name="milliseconds">Time that has passed</param>
        protected override void UpdateState(double milliseconds)
        {
            Craft craft = Mission.Craft;
            craft.Refuel(milliseconds);
            craft.Reload(milliseconds);
            craft.Repair(milliseconds);
            craft.Rearm(milliseconds);
        }

        /// <summary>
        /// Anything that needs to be done when mission enters this state
        /// </summary>
        public override void OnEnterState() 
        {
            base.OnEnterState();
            Mission.Craft.InBase = true;
        }

        /// <summary>
        /// Anything that needs to be done when mission leaves this state
        /// </summary>
        public override void OnExitState() 
        {
            base.OnExitState();
            Mission.Craft.InBase = false;
        }
    }
}
