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
* @file MessageBoxGeoEvent.cs
* @date Created: 2007/07/21
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using ProjectXenocide.Utils;

#endregion

namespace ProjectXenocide.Model.Geoscape.GeoEvents
{
    /// <summary>
    /// Geoevent that does nothing more than show a MessageBox
    /// </summary>
    /// <remarks>Will probably replace this later, with others that have more
    /// knowledge about what it is they're showing</remarks>
    [Serializable]
    public class MessageBoxGeoEvent : GeoEvent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="format">Message to show including formatting</param>
        /// <param name="args">values to inject into formatting string</param>
        public MessageBoxGeoEvent(string format, params Object[] args)
        {
            this.message = Util.StringFormat(format, args);
        }
        
        /// <summary>
        /// Called to get the event to do whatever processing is necessary
        /// </summary>
        public override void Process()
        {
            Xenocide.GameState.GeoData.GeoTime.StopTime();
            Util.ShowMessageBox(message);
        }

        /// <summary>
        /// Construct and queue a messagebox GeoEvent
        /// </summary>
        /// <param name="format">Message to show including formatting</param>
        /// <param name="args">values to inject into formatting string</param>
        public static void Queue(string format, params Object[] args)
        {
            Xenocide.GameState.GeoData.QueueEvent(new MessageBoxGeoEvent(format, args));
        }

#region Fields

        /// <summary>
        /// Message to show
        /// </summary>
        private String message;

#endregion
    }
}
