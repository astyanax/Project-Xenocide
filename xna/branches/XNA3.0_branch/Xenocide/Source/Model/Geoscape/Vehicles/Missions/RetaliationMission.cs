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
* @file RetaliationMission.cs
* @date Created: 2007/09/01
* @author File creator: dteviot
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
using ProjectXenocide.Model.Geoscape.Outposts;

#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// Retaliation where UFO will search for and then attack an X-Corp outpost
    /// </summary>
    [Serializable]
    public class RetaliationMission : UfoMission
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="craft">craft that "owns" this mission</param>
        /// <param name="initialDestination">First landing site UFO will head for</param>
        /// <param name="outpost">The outpost the craft is going to attack</param>
        /// <param name="numLandings">Number of landings craft will perform (including attack on outpost)</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if outpost is null")]
        public RetaliationMission(Craft craft, GeoPosition initialDestination, Outpost outpost, int numLandings)
            :
            base(craft, initialDestination, numLandings)
        {
            this.outpost = outpost;
        }

        /// <summary>
        /// Does the target outpost still exist?
        /// </summary>
        /// <returns>true if outpost still exists</returns>
        public bool DoesOutpostExist()
        {
            return Xenocide.GameState.GeoData.Outposts.Contains(outpost);
        }

        /// <summary>
        /// Called when UFO reaches final landing site (the outpost)
        /// </summary>
        protected override void OnFinalLandingSiteReached()
        {
            // can only attack outpost if it still exists
            if (Xenocide.GameState.GeoData.Outposts.Contains(outpost))
            {
                locatedOutpost = true;
                AttackOutpost();
            }

            // leave earth
            GeoPosition exit = outpost.Position.RandomLocation((float)Math.PI / 2.0f);
            SetState(new MoveToPositionState(this, exit));
        }

        /// <summary>
        /// Called when UFO decides it's time to go for the final landing site.  May be:
        /// </summary>
        protected override GeoPosition OnCalcFinalLandingSite()
        {
            // head for the outpost
            return outpost.Position;
        }

        /// <summary>
        /// Attack the outpost
        /// </summary>
        private void AttackOutpost()
        {
            Xenocide.GameState.GeoData.QueueEvent(new UfoAttackingOutpostGeoEvent(Craft, outpost));
        }

        #region Fields

        /// <summary>
        /// Was UFO destroyed before it located the outpost?
        /// </summary>
        public bool LocatedOutpost { get { return locatedOutpost; } }

        /// <summary>
        /// The outpost the craft is going to attack
        /// </summary>
        private Outpost outpost;

        /// <summary>
        /// Was UFO destroyed before it located the outpost?
        /// </summary>
        private bool locatedOutpost;

        #endregion
    }
}
