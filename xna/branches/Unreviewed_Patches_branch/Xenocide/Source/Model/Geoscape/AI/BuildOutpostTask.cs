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
* @file BuildOutpostTask.cs
* @date Created: 2007/08/27
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Geoscape.Geography;

#endregion

namespace ProjectXenocide.Model.Geoscape.AI
{
    /// <summary>
    /// Alien Overmind sending a series of UFOs to build an outpost
    /// </summary>
    [Serializable]
    public partial class BuildOutpostTask : InvasionTask
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="overmind">Overmind that owns this task</param>
        /// <param name="centroid">Position on Geoscape where base will be built</param>
        /// <param name="taskPlan">The Missions this task requires</param>
        public BuildOutpostTask(Overmind overmind, GeoPosition centroid, TaskPlan taskPlan)
            :
            base(overmind, centroid, taskPlan)
        {
            Debug.Assert(ufosInMission == taskPlan.Launches.Count);
            Debug.Assert("ITEM_UFO_JUGGERNAUT" == taskPlan.Launches[ufosInMission - 1].UfoType);
        }

        /// <summary>
        /// Called when UFO has finished and survived the mission the task set it
        /// </summary>
        /// <param name="ufo">UFO that has finished the mission</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if ufo is null")]
        public override void OnMissionFinished(Ufo ufo)
        {
            CheckIfOutpostBuilt(ufo);
            base.OnMissionFinished(ufo);
        }

        /// <summary>
        /// Called when a UFO has been destroyed (by enemy action)
        /// </summary>
        /// <param name="ufo">UFO that was destroyed</param>
        public override void OnUfoDestroyed(Ufo ufo)
        {
            CheckIfOutpostBuilt(ufo);
            base.OnUfoDestroyed(ufo);
        }

        /// <summary>
        /// Note, a UFO could have been destroyed AFTER the outpost was built
        /// </summary>
        /// <param name="ufo">UFO engaged in building the outpost</param>
        private void CheckIfOutpostBuilt(Ufo ufo)
        {
            // Final UFO is only one that counts
            if ((TaskPlan.Launches[ufosInMission - 1].UfoType == ufo.ItemInfo.Id) && ufo.Mission.Success)
            {
                Xenocide.GameState.GeoData.Overmind.AddSite(new OutpostAlienSite(Centroid, ufo.Race));
            }
        }

        /// <summary>
        /// Give a UFO a mission
        /// </summary>
        /// <param name="ufo">Ufo to give mission to</param>
        protected override void GiveMission(Ufo ufo)
        {
            // First half of UFOs are "scouting", other half do the "building"
            if (NextUfoToLaunch <= ufosInMission / 2)
            {
                const int numLandings = 6;
                ufo.Mission = new ResearchMission(ufo, Centroid, numLandings);
            }
            else
            {
                // Supply Outpost is same X-Corp visible behaviour as Build Outpost
                ufo.Mission = new SupplyOutpostMission(ufo, Centroid);
            }
        }

        private const int ufosInMission = 6;
    }
}
