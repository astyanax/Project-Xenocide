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
* @file Appointment.cs
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

#endregion

namespace ProjectXenocide.Model
{
    /// <summary>
    /// An event that will occur at some known time in the future
    /// </summary>
    [Serializable]
    public abstract class Appointment
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="occurs">When the appointment will occur</param>
        protected Appointment(DateTime occurs)
        {
            this.occurs = occurs;
        }

        /// <summary>
        /// Automatically called when time reaches "occurs"
        /// This is were the event processing is done
        /// </summary>
        public abstract void Process();

        /// <summary>
        /// Number of GameTime seconds before event occurs
        /// </summary>
        /// <param name="now">The current game time</param>
        public double SecondsLeft(DateTime now)
        {
            return (occurs - now).TotalSeconds;
        }

        #region Fields

        /// <summary>
        /// When the event will occur
        /// </summary>
        public DateTime Occurs { get { return occurs; } protected set { occurs = value; }}

        /// <summary>
        /// When the event will occur
        /// </summary>
        private DateTime occurs;

        #endregion Fields
    }
}
