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
* @file TerrorMissionAlienSite.cs
* @date Created: 2007/08/20
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Geography;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.Battlescape;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.Geoscape.AI
{
    /// <summary>
    /// A Terror site on earth
    /// </summary>
    [Serializable]
    public class TerrorMissionAlienSite : AlienSite
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="city">City that's being terrorized</param>
        /// <param name="race">The race that is present at this site</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if city is null")]
        public TerrorMissionAlienSite(City city, Race race)
            :
            base(Util.StringFormat(Strings.TERROR_SITE_NAME, city.Name), city.Position, race)
        {
            this.city = city;

            // X-Corp have 12 hours to get on site
            TimeSpan delay = new TimeSpan(12, 0, 0);
            appointment = Xenocide.GameState.GeoData.GeoTime.MakeAppointment(delay, OnAppointment);
        }

        /// <summary>
        /// Handle X-Corp destroying the terror mission
        /// </summary>
        /// <remarks>template method pattern</remarks>
        protected override void OnSiteDestroyedCore()
        {
            Xenocide.GameState.GeoData.GeoTime.Scheduler.Remove(appointment);
        }

        /// <summary>Create Alien force for a battlescape mission at this site</summary>
        /// <returns>The alien force</returns>
        public override Team CreateAlienTeam()
        {
            return CreateAlienTeam("ITEM_UFO_INTIMIDATOR");
        }

        /// <summary>
        /// If X-Corp don't get here in time, it's 1000 points to the aliens
        /// </summary>
        public void OnAppointment()
        {
            Xenocide.GameState.GeoData.AddScore(Participant.Alien, StartSettings.TerrorizeCityAlienScore, city.Position);
            OnSiteDestroyed();
        }

        #region fields

        /// <summary>
        /// Is the position known to X-Corp?
        /// </summary>
        public override bool IsKnownToXCorp { get { return true; } set { } }

        /// <summary>
        /// The city being terrorized
        /// </summary>
        private City city;

        /// <summary>
        /// Appointment scheduled for this site.  (Supply, or Terror mission finsihed.)
        /// </summary>
        private DelegatingAppointment appointment;

        #endregion fields
    }
}
