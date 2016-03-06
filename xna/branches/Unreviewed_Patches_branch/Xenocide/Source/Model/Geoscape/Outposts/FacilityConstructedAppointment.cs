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
* @file FacilityConstructedAppointment.cs
* @date Created: 2007/08/06
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using CeGui;
using ProjectXenocide.Model.Geoscape.Outposts;

#endregion

namespace ProjectXenocide.Model
{
    /// <summary>
    /// An event that records when a facility should be constructed
    /// </summary>
    [Serializable]
    public class FacilityConstructedAppointment : Appointment
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="occurs">When the facility will finish construction</param>
        /// <param name="facility">Facility that is being constructed</param>
        public FacilityConstructedAppointment(DateTime occurs, FacilityHandle facility) 
            : 
            base(occurs) 
        {
            this.facility = facility;
        }

        /// <summary>
        /// Automatically called when time reaches "occurs"
        /// This is were the event processing is done
        /// </summary>
        public override void Process()
        {
            facility.FinishedBuilding(statistics);
        }

        #region Fields

        /// <summary>
        /// OutpostStatistics to update when facility is completed
        /// </summary>
        public OutpostStatistics Statistics { get { return statistics; } set { statistics = value; } }

        /// <summary>
        /// OutpostStatistics to update when facility is completed
        /// </summary>
        private OutpostStatistics statistics;

        /// <summary>
        /// Facility that is being constructed
        /// </summary>
        private FacilityHandle facility;

        #endregion Fields
    }
}
