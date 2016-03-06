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
* @file StartBattlescapeGeoEvent.cs
* @date Created: 2007/03/11
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.UI.Dialogs;

#endregion

namespace ProjectXenocide.Model.Geoscape.GeoEvents
{
    /// <summary>
    /// A troop carrying craft has reached a UFO on the ground (landed or crashed)
    /// </summary>
    [Serializable]
    public class StartBattlescapeGeoEvent : GeoEvent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mission">Details for battlescape mission</param>
        public StartBattlescapeGeoEvent(Mission mission)
        {
            this.mission = mission;
        }

        /// <summary>
        /// Called to get the event to do whatever processing is necessary
        /// </summary>
        public override void Process()
        {
            Xenocide.GameState.GeoData.GeoTime.StopTime(); 
            Xenocide.ScreenManager.ShowDialog(new StartBattlescapeDialog(mission));
        }

#region Fields

        /// <summary>
        /// Details for battlescape mission
        /// </summary>
        private Mission mission;

#endregion
    }
}
