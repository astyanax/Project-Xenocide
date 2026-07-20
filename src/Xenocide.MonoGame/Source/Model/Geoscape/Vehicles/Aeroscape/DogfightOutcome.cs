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
* @file DogfightOutcome.cs
* @date Created: 2026/07/20
* @author File creator: Xenocide Agent
* @author Credits: none
*/
#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// The possible outcomes of an aeroscape dogfight engagement.
    /// </summary>
    public enum DogfightOutcome
    {
        /// <summary>
        /// The dogfight is still in progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// The aircraft won: UFO was destroyed or crashed.
        /// Results in score bonus and potentially a crash site for recovery.
        /// </summary>
        AircraftVictory,

        /// <summary>
        /// The aircraft successfully disengaged and retreated.
        /// No damage inflicted on UFO, no score change.
        /// </summary>
        AircraftRetreated,

        /// <summary>
        /// The aircraft was shot down by the UFO.
        /// Craft is lost, crew may be recoverable later.
        /// </summary>
        AircraftDestroyed,

        /// <summary>
        /// The UFO escaped by outrunning the interceptors.
        /// No damage inflicted, UFO continues its mission.
        /// </summary>
        UFOEscaped,
    }
}
