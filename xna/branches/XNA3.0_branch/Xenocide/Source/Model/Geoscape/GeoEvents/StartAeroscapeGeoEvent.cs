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
* @file StartAeroscapeGeoEvent.cs
* @date Created: 2007/03/11
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.UI.Dialogs;

#endregion

namespace ProjectXenocide.Model.Geoscape.GeoEvents
{
    /// <summary>
    /// A craft (that carries weapons) has reached a UFO (that is in flight)
    /// </summary>
    [Serializable]
    public class StartAeroscapeGeoEvent : GeoEvent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hunter">Craft that is attacking the UFO</param>
        /// <param name="target">Craft that is being attacked</param>
        public StartAeroscapeGeoEvent(Craft hunter, Craft target)
        {
            this.hunter = hunter;
            this.target = target;
        }
        
        /// <summary>
        /// Called to get the event to do whatever processing is necessary
        /// </summary>
        public override void Process()
        {
            Xenocide.GameState.GeoData.GeoTime.StopTime();
            Xenocide.ScreenManager.ScheduleScreen(
                new AeroscapeScreen((Aircraft)hunter, (Ufo)target)
            );
        }

#region Fields

        /// <summary>
        /// UFO being attacked
        /// </summary>
        private Craft target;

        /// <summary>
        /// Craft that is attacking the UFO
        /// </summary>
        private Craft hunter;

#endregion
    }
}
