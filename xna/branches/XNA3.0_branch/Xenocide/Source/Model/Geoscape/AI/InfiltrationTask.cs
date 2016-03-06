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
* @file InfiltrationTask.cs
* @date Created: 2007/08/27
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
using ProjectXenocide.Model.Geoscape.Geography;

#endregion

namespace ProjectXenocide.Model.Geoscape.AI
{
    /// <summary>
    /// Alien Overmind sending a series of UFOs to persuade country to stop supporting X-Corp
    /// </summary>
    [Serializable]
    public partial class InfiltrationTask : InvasionTask
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="overmind">Overmind that owns this task</param>
        /// <param name="centroid">Position on Geoscape that will be the center of the UFOs' activity</param>
        /// <param name="taskPlan">The Missions this task requires</param>
        /// <param name="country">country Alien is trying to infiltrate</param>
        public InfiltrationTask(Overmind overmind, GeoPosition centroid, TaskPlan taskPlan, Country country)
            :
            base(overmind, centroid, taskPlan)
        {
            this.country = country;
        }

        /// <summary>
        /// The last UFO involved in the task has finished or been destroyed
        /// </summary>
        /// <param name="ufo">the last UFO</param>
        public override void OnTaskFinished(Ufo ufo)
        {
            // figure out if Aliens infiltrated the country or not.
            // each UFO that's successful has a 10% chance of success.
            for (int i = 0; i < successfulMissions; ++i)
            {
                if (Xenocide.Rng.RollDice(10))
                {
                    InfiltrationSuccessful();
                    break;
                }
            }

            base.OnTaskFinished(ufo);
        }

        /// <summary>
        /// Called when UFO has finished and survived the mission the task set it
        /// </summary>
        /// <param name="ufo">UFO that has finished the mission</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if ufo is null")]
        public override void OnMissionFinished(Ufo ufo)
        {
            // keep track of the number of successful missions
            if (ufo.Mission.Success)
            {
                ++successfulMissions;
            }
            base.OnMissionFinished(ufo);
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

        /// <summary>
        /// Handle the infiltration succeeding
        /// </summary>
        private void InfiltrationSuccessful()
        {
            country.OnInfiltrated();

            // if there's no more countries to infiltrate, remove from available missions
            Planet planet = Xenocide.GameState.GeoData.Planet;
            if (null == planet.SelectCountryToInfiltrate())
            {
                planet.ClearInfiltrationMissions();
            }
        }

        #region fields

        /// <summary>
        /// country Alien is trying to infiltrate
        /// </summary>
        private Country country;

        /// <summary>
        /// How many UFOs managed to infiltrate the country
        /// </summary>
        private int successfulMissions;

        #endregion fields
    }
}
