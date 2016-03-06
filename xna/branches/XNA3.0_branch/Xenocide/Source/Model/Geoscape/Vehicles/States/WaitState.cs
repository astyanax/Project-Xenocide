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
* @file WaitState.cs
* @date Created: 2007/02/11
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
    /// State that represents Craft moving to somewhere
    /// </summary>
    [Serializable]
    public class WaitState : MissionState
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mission">mission that owns this state</param>
        /// <param name="secondsToWait">Time craft is to wait</param>
        public WaitState(Mission mission, double secondsToWait)
            :
            base(mission)
        {
            this.secondsToWait = secondsToWait;
        }

        /// <summary>
        /// Change the craft, based on time elapsed
        /// </summary>
        /// <param name="milliseconds">Time that has passed</param>
        protected override void UpdateState(double milliseconds)
        {
            secondsToWait -= (milliseconds / 1000.0);
            if (secondsToWait <= 0.0)
            {
                Mission.OnTimerFinished();
            }
        }

        /// <summary>
        /// Time craft is to wait
        /// </summary>
        private double secondsToWait;
    }
}
