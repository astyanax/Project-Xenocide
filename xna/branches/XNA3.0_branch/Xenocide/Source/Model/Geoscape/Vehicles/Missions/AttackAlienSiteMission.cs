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
* @file AttackAlienSiteMission.cs
* @date Created: 2007/08/26
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.Model.Geoscape.GeoEvents;
using ProjectXenocide.Model.Battlescape;

#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// Mission where
    /// 1. Craft heads towards alien site
    /// 2. If craft reaches target, ground mission starts
    /// 3. At end of ground mission, craft either returns to base, or is destroyed
    /// 4. If craft fails to reach target, returns to base
    /// </summary>
    [Serializable]
    public partial class AttackAlienSiteMission : Mission
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="craft">craft that "owns" this mission</param>
        /// <param name="target">The Alien Site the craft is to attack</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if target is null")]
        public AttackAlienSiteMission(Craft craft, AlienSite target)
            :
            base(craft)
        {
            this.target = target;
            SetState(new MoveToPositionState(this, target.Position));
            target.AddInbound(Craft);
        }

        /// <summary>
        /// usually, player is about to give craft a new mission
        /// </summary>
        public override void Abort()
        {
            TellSiteNoLongerInbound();
            base.Abort();
        }

        /// <summary>
        /// We've reached a destination (either alien site, or home base)
        /// </summary>
        public override void OnDestinationReached()
        {
            if (null != target)
            {
                // we're at the AlienSite, so set state to return home
                OnArriveAtAlienSite();
            }
            else
            {
                // we're back at home base
                SetState(new InBaseState(this));
            }
        }

        /// <summary>
        /// Respond to AlienSite this craft is heading towards ceasing to exist
        /// </summary>
        public override void OnSiteGone(AlienSite site)
        {
            GoHome();
        }

        /// <summary>
        /// Do what we need to do, when we get to the alien site
        /// </summary>
        private void OnArriveAtAlienSite()
        {
            // start up ground battle
            AlienSiteMission battlescapeMission = new AlienSiteMission(target, Craft);
            Xenocide.GameState.GeoData.QueueEvent(new StartBattlescapeGeoEvent(battlescapeMission));

            // set craft on it's way home.
            GoHome();
        }

        /// <summary>
        /// set craft on its way home.
        /// </summary>
        private void GoHome()
        {
            TellSiteNoLongerInbound();
            SetState(new ReturnToBaseState(this));
        }

        /// <summary>
        /// Tell the alien site this craft is no longer inbound to it
        /// </summary>
        private void TellSiteNoLongerInbound()
        {
            if (null != target)
            {
                target.RemoveInbound(Craft);
                target = null;
            }
        }

        #region Fields

        /// <summary>
        /// The Alien Site the craft is to attack
        /// </summary>
        private AlienSite target;

        #endregion
    }
}
