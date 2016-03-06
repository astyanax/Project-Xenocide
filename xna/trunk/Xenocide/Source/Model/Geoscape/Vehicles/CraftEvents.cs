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
* @file CraftEvents.cs
* @date Created: 2007/02/17
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using ProjectXenocide.Model.Geoscape.AI;

#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// Set of Geoscape events that a craft may need to react to
    /// </summary>
    public interface ICraftEvents
    {
        /// <summary>
        /// Respond to this craft reaching a destination
        /// </summary>
        void OnDestinationReached();

        /// <summary>
        /// Respond to this craft reaching a destination point of interest
        /// </summary>
        void OnSubDestinationReached();

        /// <summary>
        /// Respond to a specified period of time elapsing
        /// </summary>
        void OnTimerFinished();

        /// <summary>
        /// Respond to a craft on the Geoscape being destroyed
        /// </summary>
        /// <param name="destroyedCraft">craft that's been destroyed</param>
        void OnCraftDestroyed(Craft destroyedCraft);

        /// <summary>
        /// Respond to this craft being destroyed
        /// </summary>
        void OnDestroyed();

        /// <summary>
        /// Respond to this craft finishing it's current mission
        /// </summary>
        void OnMissionFinished();

        /// <summary>
        /// Respond to this craft loosing sight of the craft it is hunting
        /// </summary>
        /// <remarks>Don't confuse this with prey escaping</remarks>
        void OnPreyTrackingLost();

        /// <summary>
        /// Respond to prey escaping
        /// </summary>
        void OnPreyGone();

        /// <summary>
        /// Respond to craft running low on fuel
        /// </summary>
        void OnFuelLow();

        /// <summary>
        /// Respond to this craft reaching position where it can attack another craft
        /// </summary>
        void OnInAttackRange();

        /// <summary>
        /// Respond to dogfight about to start between this craft and another
        /// </summary>
        void OnDogfightStart();

        /// <summary>
        /// Respond to this craft finishing a dogfight with another craft
        /// </summary>
        void OnDogfightFinished();

        /// <summary>
        /// Respond to AlienSite this craft is heading towards ceasing to exist
        /// </summary>
        /// <param name="site">site that no longer exists</param>
        void OnSiteGone(AlienSite site);
    }
}
