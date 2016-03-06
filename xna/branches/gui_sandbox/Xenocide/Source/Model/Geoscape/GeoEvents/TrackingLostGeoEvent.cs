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
* @file TrackingLostGeoEvent.cs
* @date Created: 2007/03/11
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using Xenocide.Model.Geoscape;
using Xenocide.Model.Geoscape.Craft;
using Xenocide.UI.Screens;
using Xenocide.UI.Dialogs;

#endregion

namespace Xenocide.Model.Geoscape.GeoEvents
{
    /// <summary>
    /// A craft has lost track of the UFO it was pursuing
    /// </summary>
    [Serializable]
    public class TrackingLostGeoEvent : GeoEvent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hunter">Last known position of craft (UFO) that was being tracked</param>
        /// <param name="target">Craft that was tracking the UFO</param>
        public TrackingLostGeoEvent(GeoPosition target, Craft.Craft hunter)
        {
            this.target = target;
            this.hunter = hunter;
        }
        
        /// <summary>
        /// Called to get the event to do whatever processing is necessary
        /// </summary>
        public override void Process()
        {
            Xenocide.ScreenManager.ShowDialog(
                new TrackingLostDialog(target, hunter)
            );
        }

#region Fields

        /// <summary>
        /// Last known position of craft (UFO) that was being tracked
        /// </summary>
        private GeoPosition target;

        /// <summary>
        /// Craft that was tracking the UFO
        /// </summary>
        private Craft.Craft hunter;

#endregion
    }
}
