#region Copyright
/**
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

/**
* @file CraftState.cs
* @date Created: 2007/02/05
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Xenocide.Model.Geoscape.Craft
{
    /// <summary>
    /// The state machine that drives craft behaviour
    /// </summary>
    abstract public class CraftState
    {
        public CraftState(Craft craft)
        {
            this.craft = craft;
        }

        /// <summary>
        /// Update state, to reflect passage of time
        /// </summary>
        /// <param name="Milliseconds">The amount of time that has passed</param>
        public void Update(double milliseconds)
        {
            if (preUpdateChecks(milliseconds))
            {
                updateState(milliseconds);
                postUpdateChecks(milliseconds);
            }
        }

        /// <summary>
        /// Check for any events that may have occured since last update
        /// </summary>
        /// <param name="milliseconds">Time that has passed</param>
        /// <returns>false if event has happened that should cancel the update</returns>
        protected virtual bool preUpdateChecks(double milliseconds)
        {
            return true;
        }

        /// <summary>
        /// Change the craft, based on time elapsed
        /// </summary>
        /// <param name="milliseconds">Time that has passed</param>
        protected abstract void updateState(double milliseconds);

        /// <summary>
        /// Check for any events that may have occured due to the update
        /// </summary>
        /// <param name="milliseconds">Time that has passed</param>
        protected virtual void postUpdateChecks(double milliseconds)
        {
        }

        #region Fields

        /// <summary>
        /// The craft this state machine will control
        /// </summary>
        protected Craft craft;

        #endregion
    }
}
