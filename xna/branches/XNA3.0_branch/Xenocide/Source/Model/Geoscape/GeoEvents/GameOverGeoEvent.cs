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
* @file GameOverGeoEvent.cs
* @date Created: 2007/11/12
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using ProjectXenocide.Utils;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.UI.Screens;

#endregion

namespace ProjectXenocide.Model.Geoscape.GeoEvents
{
    /// <summary>
    /// Geoevent that tells user Game is over
    /// </summary>
    [Serializable]
    public class GameOverGeoEvent : GeoEvent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="format">Message to show including formatting</param>
        /// <param name="args">values to inject into formatting string</param>
        public GameOverGeoEvent(string format, params Object[] args)
        {
            this.message = Util.StringFormat(format, args);
        }
        
        /// <summary>
        /// Called to get the event to do whatever processing is necessary
        /// </summary>
        public override void Process()
        {
            Xenocide.GameState.GeoData.GeoTime.StopTime();
            MessageBoxDialog dialog = new MessageBoxDialog(message);
            dialog.OkAction += delegate()
            {
                Xenocide.ScreenManager.ScheduleScreen(new StartScreen());
                // ToDo, may need to purge the Geoevent queue
            };
            Xenocide.ScreenManager.ShowDialog(dialog);
        }

#region Fields

        /// <summary>
        /// Message to show
        /// </summary>
        private String message;

#endregion
    }
}
