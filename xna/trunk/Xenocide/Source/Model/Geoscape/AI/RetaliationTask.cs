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
* @file RetaliationTask.cs
* @date Created: 2007/09/01
* @author File creator: dteviot
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
using ProjectXenocide.Model.Geoscape.Outposts;

#endregion

namespace ProjectXenocide.Model.Geoscape.AI
{
    /// <summary>
    /// Alien Overmind sending a series of UFOs to destroy an X-Corp base
    /// </summary>
    [Serializable]
    public partial class RetaliationTask : InvasionTask
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="overmind">Overmind that owns this task</param>
        /// <param name="searchStart">Where Overmind will start it's search for X-Corp outposts</param>
        /// <param name="taskPlan">The Missions this task requires</param>
        public RetaliationTask(Overmind overmind, GeoPosition searchStart, TaskPlan taskPlan)
            :
            base(overmind, searchStart, taskPlan)
        {
        }

        /// <summary>
        /// Called when UFO has finished and survived the mission the task set it
        /// </summary>
        /// <param name="ufo">UFO that has finished the mission</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if ufo is null")]
        public override void OnMissionFinished(Ufo ufo)
        {
            CheckIfXCorpOutpostDestroyed(ufo);
            base.OnMissionFinished(ufo);
        }

        /// <summary>
        /// Called when a UFO has been destroyed (by enemy action)
        /// </summary>
        /// <param name="ufo">UFO that was destroyed</param>
        public override void OnUfoDestroyed(Ufo ufo)
        {
            CheckIfXCorpOutpostDestroyed(ufo);
            base.OnUfoDestroyed(ufo);
        }

        /// <summary>
        /// Choose first landing site for the UFO
        /// </summary>
        /// <returns>landing site's GeoPosition</returns>
        protected override GeoPosition SelectFirstLandingSite()
        {
            if (locatedOutpost)
            {
                // Outpost has been located, so just go for it.
                return outpost.Position;
            }
            else
            {
                // start where UFO was downed
                return Centroid;
            }
        }

        /// <summary>
        /// Give a UFO a mission
        /// </summary>
        /// <param name="ufo">Ufo to give mission to</param>
        /// <param name="numLandings">Number times the UFO will land</param>
        /// <param name="numSublandings">Number of points the UFO will investigate between landings</param>
        protected override void GiveMission(Ufo ufo, int numLandings, int numSubLandings)
        {
            if (locatedOutpost)
            {
                // Outpost has been located, so just go for it.
                ufo.Mission = new RetaliationMission(ufo, outpost.Position, outpost, numLandings,numLandings);
            }
            else
            {
                SelectOutpost();
                if ((null != outpost) && Xenocide.Rng.RollDice(outpost.Detectability()))
                {
                    // Given enough time, the UFO will find the outpost
                    ufo.Mission = new RetaliationMission(ufo, Centroid, outpost, numLandings,numSubLandings);
                }
                else
                {
                    // UFO isn't going to find the outpost (or there isn't one)
                    ufo.Mission = new ResearchMission(ufo, Centroid, numLandings, numSubLandings);
                }
            }
        }

        /// <summary>
        /// Select the outpost in the search area the Overmind might "locate"
        /// Pick closest to the attack site
        /// </summary>
        private void SelectOutpost()
        {
            locatedOutpost = false;
            outpost        = null;
            float minDistance = searchRadius;
            foreach (Outpost o in Xenocide.GameState.GeoData.Outposts)
            {
                float distance = Centroid.Distance(o.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    outpost     = o;
                }
            }
        }

        /// <summary>
        /// Check if the target Outpost still exists
        /// </summary>
        /// <param name="ufo">UFO sent to destroy outpost</param>
        private void CheckIfXCorpOutpostDestroyed(Ufo ufo)
        {
            RetaliationMission mission = ufo.Mission as RetaliationMission;
            // research missions mean UFO learned nothing.
            if (null != mission)
            {
                if (!Xenocide.GameState.GeoData.Outposts.Contains(outpost))
                {
                    // outpost no longer exists 
                    // ToDo: theoretically, UFO could have failed to destroy outpost, and player 
                    // disassembled outpost before UFO left Geoscape, so Overmind should think outpost
                    // still exists.  Someone might want to write logic to cover that case.
                    outpost        = null;
                    locatedOutpost = false;
                }
                else if (mission.LocatedOutpost)
                {
                    // UFO reached outpost so Overmind knows where Outpost is
                    locatedOutpost = true;
                }
            }
        }

        #region constants

        /// <summary>
        /// UFOs will search for outposts up to 3000km from site of destroyed UFO
        /// </summary>
        private static readonly float searchRadius = (float)GeoPosition.KilometersToRadians(3000);

        #endregion

        #region fields

        /// <summary>
        /// The outpost the overmind wants destroyed. (null if there isn't one.)
        /// </summary>
        private Outpost outpost;

        /// <summary>
        /// Has the overmind "located" the outpost yet?
        /// </summary>
        private bool locatedOutpost;

        #endregion fields
    }
}
