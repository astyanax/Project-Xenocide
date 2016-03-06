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
* @file Battlescape/UfoSiteMission.cs
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
using Xenocide.Resources;


namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// Mission is X-Corp attacking a landed/crashed UFO
    /// </summary>
    [Serializable]
    public class UfoSiteMission : Mission
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="ufo">UFO being attacked</param>
        /// <param name="hunter">X-Corp Craft that is landing troops</param>
        public UfoSiteMission(Craft ufo, Craft hunter)
            : base(hunter as Aircraft)
        {
            this.ufo    = ufo as Ufo;
        }

        /// <summary>
        /// Text to show on the start mission dialog
        /// </summary>
        /// <returns>message to show</returns>
        public override string MakeStartMissionText()
        {
            return Util.StringFormat(Strings.MSGBOX_START_BATTLESCAPE, Aircraft.Name, ufo.Name);
        }

        /// <summary>
        /// Called if we're not going to start the battlescape at this point in time.
        /// </summary>
        public override void DontStart()
        {
            // Send aircraft on it's way home
            Aircraft.OnDogfightFinished();
        }

        /// <summary>
        /// Any mission ending handling that's specific to this type of mission goes here
        /// </summary>
        /// <param name="finishType">Who won the battle</param>
        protected override void OnFinishCore(BattleFinish finishType)
        {
            // Note results of mission
            Location = ufo.Position;
            Outpost = Aircraft.HomeBase;
            switch(finishType)
            {
                case BattleFinish.XCorpVictory:
                {
                    AddToSalvage(ufo.RecoveredItems());

                    // Tell UFO that it has been killed
                    ufo.OnDestroyed();

                    // Tell aircraft fight's over (it won)
                    Aircraft.OnDogfightFinished();
                }
                break;

                case BattleFinish.Aborted:
                    //ToDo: check this is all we need to do
                    Aircraft.OnDogfightFinished();
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

        /// <summary>
        /// Create the Alien force for the battlescape
        /// </summary>
        public override Team CreateAlienTeam()
        {
            return ufo.CreateCrew(Xenocide.StaticTables.StartSettings.Difficulty);
        }

        #region Fields

        /// <summary>
        /// UFO being attacked
        /// </summary>
        private Ufo ufo;

        #endregion
    }
}
