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
* @file StartOfMonthGeoEvent.cs
* @date Created: 2007/06/04
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
    /// Signal that we've started a new month
    /// </summary>
    [Serializable]
    public class StartOfMonthGeoEvent : GeoEvent
    {
        /// <summary>
        /// Called to get the event to do whatever processing is necessary
        /// </summary>
        public override void Process()
        {
            Xenocide.GameState.GeoData.GeoTime.StopTime(); 
            Xenocide.ScreenManager.ScheduleScreen(new MonthlyReportScreen(true));
        }

        #region Fields


        #endregion
    }
}
