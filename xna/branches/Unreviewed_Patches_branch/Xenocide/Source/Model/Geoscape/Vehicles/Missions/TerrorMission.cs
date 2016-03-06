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
* @file TerrorMission.cs
* @date Created: 2007/08/20
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using ProjectXenocide.Model.Geoscape.Geography;
using ProjectXenocide.Model.Geoscape.GeoEvents;
using ProjectXenocide.Model.Geoscape.AI;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// Mission where UFO will drop an assault team on a city
    /// </summary>
    [Serializable]
    public class TerrorMission : UfoMission
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ufo">UFO that "owns" this mission</param>
        /// <param name="city">The city the UFO is going to Terrorise</param>
        /// <param name="attackCity">Will the UFO actually drop off an assult team?</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if city is null")]
        public TerrorMission(Ufo ufo, City city, bool attackCity)
            :
            base(ufo, city.Position.RandomLocationDistantBykm(500), numLandings)
        {
            this.city       = city;
            this.attackCity = attackCity;
            this.race       = ufo.Race;
        }

        /// <summary>
        /// Called when UFO reaches final landing site.
        /// </summary>
        protected override void OnFinalLandingSiteReached()
        {
            // Assault will start in 30 minutes. Give UFO time to get clear.
            if (attackCity)
            {
                Xenocide.GameState.GeoData.GeoTime.MakeAppointment(new TimeSpan(0, 30, 0), StartTerrorMission);
            }

            // and leave earth
            GeoPosition exit = city.Position.RandomLocation((float)Math.PI / 2.0f);
            SetState(new MoveToPositionState(this, exit));
        }

        /// <summary>
        /// Called when UFO decides it's time to go for the final landing site.  May be:
        /// </summary>
        protected override GeoPosition OnCalcFinalLandingSite()
        {
            // head for the city
            return city.Position;
        }

        /// <summary>
        /// Get the terror mission started
        /// </summary>
        private void StartTerrorMission()
        {
            Xenocide.GameState.GeoData.Overmind.AddSite(new TerrorMissionAlienSite(city, race));
            MessageBoxGeoEvent.Queue(Strings.MSGBOX_TERROR_MISSION_STARTED, city);
        }

        #region Fields

        /// <summary>
        /// The city the UFO is going to Terrorise
        /// </summary>
        private City city;

        /// <summary>
        /// A terror mission UFO will land 4 times before heading to the target city
        /// </summary>
        private const int numLandings = 5;

        /// <summary>
        /// Will the UFO actually drop off an assult team?
        /// </summary>
        private bool attackCity;

        /// <summary>
        /// Alien race that will be used to attack the city
        /// </summary>
        private Race race;

        #endregion
    }
}
