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
* @file FacilityFinishedGeoEvent.cs
* @date Created: 2007/05/21
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using Xenocide.Resources;
using Xenocide.Utils;
using Xenocide.Model.Geoscape;
using Xenocide.Model.Geoscape.HumanBases;
using Xenocide.UI.Screens;
using Xenocide.UI.Dialogs;

#endregion

namespace Xenocide.Model.Geoscape.GeoEvents
{
    /// <summary>
    /// Construction of a facility has been completed
    /// </summary>
    [Serializable]
    public class FacilityFinishedGeoEvent : GeoEvent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="facility">Facility in the base</param>
        public FacilityFinishedGeoEvent(FacilityHandle facility)
        {
            this.facility  = facility;
        }
        
        /// <summary>
        /// Called to get the event to do whatever processing is necessary
        /// </summary>
        public override void Process()
        {
            //TODO: Reenable once Game reference available
            /*
            Xenocide.GameState.GeoData.GeoTime.StopTime();
            Xenocide.ScreenManager.ShowDialog(new MessageBoxDialog(
                null,
                Util.StringFormat(
                    Strings.MSGBOX_FINISHED_BUILDING_FACILITY,
                    facility.FacilityInfo.Name
            )));
             */
        }

#region Fields

        /// <summary>
        /// Facility in the base
        /// </summary>
        private FacilityHandle facility;

#endregion
    }
}
