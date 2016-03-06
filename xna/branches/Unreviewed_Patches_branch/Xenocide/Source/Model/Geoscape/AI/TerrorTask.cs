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
* @file TerrorTask.cs
* @date Created: 2007/08/20
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
    /// Alien Overmind sending a series of UFOs to terrorize human cities
    /// </summary>
    [Serializable]
    public partial class TerrorTask : InvasionTask
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="overmind">Overmind that owns this task</param>
        /// <param name="taskPlan">The Missions this task requires</param>
        public TerrorTask(Overmind overmind, TaskPlan taskPlan)
            :
            base(overmind, new GeoPosition(), taskPlan)
        {
        }

        /// <summary>
        /// The last UFO involved in the task has finished or been destroyed
        /// </summary>
        /// <param name="ufo">the last UFO</param>
        public override void OnTaskFinished(Ufo ufo)
        {
            // We just go around again
            ResetLaunchCounter();
            MakeLaunchAppointment();

            // DO NOT CALL base.OnTaskFinished() as that would call overmind.OnTaskFinished()
            // which would result in the destruction of this task.
        }

        /// <summary>
        /// Choose first landing site for the UFO
        /// </summary>
        /// <returns>landing site's GeoPosition</returns>
        protected override GeoPosition SelectFirstLandingSite()
        {
            city = Xenocide.GameState.GeoData.Planet.SelectRandomCity();
            Centroid = city.Position;
            return Centroid;
        }

        /// <summary>
        /// Give a UFO a mission
        /// </summary>
        /// <param name="ufo">Ufo to give mission to</param>
        protected override void GiveMission(Ufo ufo)
        {
            ufo.Mission = new TerrorMission(ufo, city, !StillUfosToLaunch);
        }

        #region fields

        /// <summary>
        /// City currently selected to terrorise
        /// </summary>
        private City city;

        #endregion fields
    }
}
