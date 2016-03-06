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
* @file SupplyOutpostTask.cs
* @date Created: 2007/08/27
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Geoscape.Geography;

#endregion

namespace ProjectXenocide.Model.Geoscape.AI
{
    /// <summary>
    /// Alien Overmind sending a UFO to supply an outpost
    /// Rules
    /// 1. If a UFO fails its mission, send a bigger UFO.
    /// 2. If 3 UFOs in a row are shot down, outpost is destroyed (starvation)
    /// </summary>
    [Serializable]
    public partial class SupplyOutpostTask : InvasionTask
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="overmind">Overmind that owns this task</param>
        /// <param name="outpost">Outpost that is being supplied</param>
        /// <param name="taskPlan">The Missions this task requires</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if outpost is null")]
        public SupplyOutpostTask(Overmind overmind, OutpostAlienSite outpost, TaskPlan taskPlan)
            :
            base(overmind, outpost.Position, taskPlan)
        {
            this.outpost = outpost;
        }

        /// <summary>
        /// Called when UFO has finished and survived the mission the task set it
        /// </summary>
        /// <param name="ufo">UFO that has finished the mission</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if ufo is null")]
        public override void OnMissionFinished(Ufo ufo)
        {
            CheckIfOutpostResuplied(ufo);
            base.OnMissionFinished(ufo);
        }

        /// <summary>
        /// Called when a UFO has been destroyed (by enemy action)
        /// </summary>
        /// <param name="ufo">UFO that was destroyed</param>
        public override void OnUfoDestroyed(Ufo ufo)
        {
            CheckIfOutpostResuplied(ufo);
            base.OnUfoDestroyed(ufo);
        }

        /// <summary>
        /// The last UFO involved in the task has finished or been destroyed
        /// </summary>
        /// <param name="ufo">the last UFO</param>
        public override void OnTaskFinished(Ufo ufo)
        {
            // if we get here, 3 resupply missions have failed.
            // so outpost has starved to death.
            outpost.OnSiteDestroyed();

            base.OnTaskFinished(ufo);
        }

        /// <summary>
        /// Give a UFO a mission
        /// </summary>
        /// <param name="ufo">Ufo to give mission to</param>
        /// <param name="numLandings">Number times the UFO will land</param>
        /// <param name="numSublandings">Number of points the UFO will investigate between landings</param>
        protected override void GiveMission(Ufo ufo, int landings, int subLandings)
        {
            ufo.Mission = new SupplyOutpostMission(ufo, outpost.Position);

            // For supply missions, UFO crew is same species as outpost
            ufo.Race = outpost.Race;
        }

        /// <summary>
        /// Check if UFO managed to resupply outpost, and if so, update plan
        /// </summary>
        /// <param name="ufo">UFO attempting to resupply outpost</param>
        private void CheckIfOutpostResuplied(Ufo ufo)
        {
            // if managed to resupply outpost, send a freighter next time.
            if (ufo.Mission.Success)
            {
                ResetLaunchCounter();

                // also if we sent the last UFO in the plan, there's no appointment 
                // for a launch, so make one.
                if (null == Appointment)
                {
                    MakeLaunchAppointment();
                }
            }
        }

        #region fields

        /// <summary>
        /// Outpost that is being supplied
        /// </summary>
        private OutpostAlienSite outpost;

        #endregion fields
    }
}
