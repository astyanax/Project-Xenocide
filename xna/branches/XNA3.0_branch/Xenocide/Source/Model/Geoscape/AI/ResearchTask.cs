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
* @file ResearchTask.cs
* @date Created: 2007/02/11
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;

#endregion

namespace ProjectXenocide.Model.Geoscape.AI
{
    /// <summary>
    /// Alien Overmind sending a series of UFOs to explore a region of the earth
    /// (Usually involves multiple UFOs)
    /// </summary>
    [Serializable]
    public class ResearchTask : InvasionTask
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="overmind">Overmind that owns this task</param>
        /// <param name="centroid">Position on Geoscape that will be the center of the UFOs' activity</param>
        /// <param name="taskPlan">The Missions this task requires</param>
        public ResearchTask(Overmind overmind, GeoPosition centroid, TaskPlan taskPlan)
            :
            base(overmind, centroid, taskPlan)
        {
        }

        /// <summary>
        /// Give a UFO a mission
        /// </summary>
        /// <param name="ufo">Ufo to give mission to</param>
        protected override void GiveMission(Ufo ufo)
        {
            const int numLandings = 3;
            ufo.Mission = new ResearchMission(ufo, Centroid, numLandings);
        }
    }
}
