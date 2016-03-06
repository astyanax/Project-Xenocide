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
* @file UfoAttackingOutpostGeoEvent.cs
* @date Created: 2007/09/02
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;


using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.UI.Dialogs;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.Geoscape.GeoEvents
{
    /// <summary>
    /// A Ufo is attacking an X-Corp Outpost
    /// </summary>
    [Serializable]
    public class UfoAttackingOutpostGeoEvent : GeoEvent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="craft">UFO that is attacking</param>
        /// <param name="outpost">Outpost that is being attacked</param>
        public UfoAttackingOutpostGeoEvent(Craft craft, Outpost outpost)
        {
            this.craft = craft;
            this.outpost = outpost;
        }

        /// <summary>
        /// Called to get the event to do whatever processing is necessary
        /// </summary>
        public override void Process()
        {
            // Have outpost attack UFO.see what happens
            BattleLog log = new BattleLog();
            switch (outpost.Attack(craft, log))
            {
                case AttackResult.OpponentDestroyed:
                    UfoVapourized();
                    break;

                case AttackResult.OpponentCrashed:
                    UfoCrashed();
                    break;

                default:
                    UfoLandedTroops();
                    break;
            }
        }

        /// <summary>
        /// Outpost's defenses atomized the UFO. There's nothing to salvage
        /// </summary>
        private void UfoVapourized()
        {
            Util.ShowMessageBox(Strings.MSGBOX_OUTPOST_ATTACKING_UFO_VAPOURIZED, outpost.Name, craft.Name);
        }

        /// <summary>
        /// Outpost's defenses shot down the UFO. There may be salvage
        /// </summary>
        private void UfoCrashed()
        {
            craft.OnDogfightFinished();
            Xenocide.GameState.GeoData.GeoTime.StopTime();
            Util.ShowMessageBox(Strings.MSGBOX_OUTPOST_ATTACKING_UFO_CRASHED, outpost.Name, craft.Name);
        }

        /// <summary>
        /// UFO got through, we've now got a battlescape (outpost defense mission)
        /// </summary>
        public void UfoLandedTroops()
        {
            XCorpOutpostMission battlescapeMission = new XCorpOutpostMission(craft, outpost);
            Xenocide.GameState.GeoData.QueueEvent(new StartBattlescapeGeoEvent(battlescapeMission));
        }

        #region Fields

        /// <summary>
        /// UFO that is attacking
        /// </summary>
        private Craft craft;

        /// <summary>
        /// Outpost that is being attacked
        /// </summary>
        private Outpost outpost;

        #endregion
    }
}
