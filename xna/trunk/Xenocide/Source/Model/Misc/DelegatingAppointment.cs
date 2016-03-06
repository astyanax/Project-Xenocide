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
* @file DelegatingAppointment.cs
* @date Created: 2007/08/20
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

#endregion

namespace ProjectXenocide.Model
{
    /// <summary>
    /// An appointment that calls a delegate when the event comes due
    /// </summary>
    [Serializable]
    public class DelegatingAppointment : Appointment
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="occurs">When the appointment will occur</param>
        /// <param name="action">Action to take when appoinment occurs</param>
        public DelegatingAppointment(DateTime occurs, ProcessAction action)
            : base(occurs)
        {
            if (null == action)
            {
                throw new ArgumentNullException("action");
            }
            this.action = action;
        }

        /// <summary>
        /// delegate that is hooked up to yes or no button
        /// </summary>
        public delegate void ProcessAction();

        /// <summary>
        /// Automatically called when time reaches "occurs"
        /// This is were the event processing is done
        /// </summary>
        public override void Process()
        {
            action();
        }

        #region Fields

        /// <summary>
        /// Action to take when appoinment occurs
        /// </summary>
        private ProcessAction action;

        #endregion Fields
    }
}
