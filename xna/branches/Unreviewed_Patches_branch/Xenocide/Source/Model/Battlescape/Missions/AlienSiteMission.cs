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
* @file Battlescape/AlienSiteMission.cs
* @date Created: 2007/12/30
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using ProjectXenocide.Utils;

using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Geoscape.AI;
using Xenocide.Resources;


namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// Mission is X-Corp attacking a Terror Site or an Alien Outpost
    /// </summary>
    [Serializable]
    public class AlienSiteMission : Mission
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="site">Alien activity (outpost or terror site)</param>
        /// <param name="hunter">X-Corp Craft that is landing troops</param>
        public AlienSiteMission(AlienSite site, Craft hunter)
            : base(hunter as Aircraft)
        {
            this.site   = site;
        }

        /// <summary>
        /// Text to show on the start mission dialog
        /// </summary>
        /// <returns>message to show</returns>
        public override string MakeStartMissionText()
        {
            return Util.StringFormat(Strings.MSGBOX_START_BATTLESCAPE, Aircraft.Name, site.Name);
        }

        /// <summary>
        /// Create the Alien force for the battlescape
        /// </summary>
        public override Team CreateAlienTeam()
        {
            return site.CreateAlienTeam();
        }

        /// <summary>
        /// Any mission ending handling that's specific to this type of mission goes here
        /// </summary>
        /// <param name="finishType">Who won the battle</param>
        protected override void OnFinishCore(BattleFinish finishType)
        {
            // Note results of mission
            Location = site.Position;
            Outpost = Aircraft.HomeBase;
            switch (finishType)
            {
                case BattleFinish.XCorpVictory:
                    {
                        AddToSalvage(site.RecoveredItems());

                        // Tell Site that it has been destroyed
                        site.OnSiteDestroyed();

                        // Aircraft is already heading home, so it doesn't need an update
                    }
                    break;

                case BattleFinish.Aborted:
                    //ToDo: check this is all we need to do
                    // Aircraft is already heading home, so it doesn't need an update
                    break;

                case BattleFinish.AlienVictory:
                    //ToDo: check this is all we need to do
                    Aircraft.OnDestroyed();
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
        }

        #region Fields

        /// <summary>
        /// Alien activity (outpost or terror site)
        /// </summary>
        private AlienSite site;

        #endregion
    }
}
