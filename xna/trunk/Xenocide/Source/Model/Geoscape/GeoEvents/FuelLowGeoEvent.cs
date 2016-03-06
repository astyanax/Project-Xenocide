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
* @file FuelLowGeoEvent.cs
* @date Created: 2007/05/27
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
using ProjectXenocide.UI.Screens;
using ProjectXenocide.UI.Dialogs;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.Geoscape.GeoEvents
{
    /// <summary>
    /// A craft has run low on fuel and needs to return to base
    /// </summary>
    [Serializable]
    public class FuelLowGeoEvent : GeoEvent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="craft">Craft that has run low on fuel</param>
        public FuelLowGeoEvent(Craft craft)
        {
            this.craft = craft;
        }

        /// <summary>
        /// Called to get the event to do whatever processing is necessary
        /// </summary>
        public override void Process()
        {
            Xenocide.GameState.GeoData.GeoTime.StopTime();
            Util.ShowMessageBox(Strings.MSGBOX_FUEL_LOW, craft.Name);
        }

        #region Fields

        /// <summary>
        /// Craft that has run low on fuel
        /// </summary>
        private Craft craft;

        #endregion
    }
}
