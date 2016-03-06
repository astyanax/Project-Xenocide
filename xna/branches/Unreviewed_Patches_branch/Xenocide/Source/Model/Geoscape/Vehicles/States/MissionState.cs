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
* @file MissionState.cs
* @date Created: 2007/02/10
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape.AI;

#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// The state machine that drives craft behaviour
    /// </summary>
    [Serializable]
    abstract public class MissionState : ICraftEvents
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mission">mission that owns this state</param>
        protected MissionState(Mission mission)
            :
            this(mission, 0.0)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mission">mission that owns this state</param>
        /// <param name="currentSpeed">Craft's current speed, in radians/second</param>
        protected MissionState(Mission mission, double currentSpeed)
        {
            if (mission == null)
                throw new ArgumentNullException("mission");
            this.mission = mission;
            this.currentSpeed = currentSpeed;
        }

        /// <summary>
        /// Update state, to reflect passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        public void Update(double milliseconds)
        {
            if (DoChecks(milliseconds))
            {
                UpdateState(milliseconds);
            }
        }

        /// <summary>
        /// Check for any events that may have occured since last update
        /// </summary>
        /// <param name="milliseconds">Time that has passed</param>
        /// <returns>false if event has happened that should cancel the update</returns>
        protected virtual bool DoChecks(double milliseconds)
        {
            return true;
        }

        /// <summary>
        /// Change the craft, based on time elapsed
        /// </summary>
        /// <param name="milliseconds">Time that has passed</param>
        protected abstract void UpdateState(double milliseconds);

        /// <summary>
        /// Anything that needs to be done when mission enters this state
        /// </summary>
        public virtual void OnEnterState() 
        {
            Util.GeoTimeDebugWriteLine("{0} Entering State {1}", Mission.Craft.Name, this.GetType().Name);
        }

        /// <summary>
        /// Anything that needs to be done when mission leaves this state
        /// </summary>
        public virtual void OnExitState() 
        {
            Util.GeoTimeDebugWriteLine("{0} Exiting State {1}", Mission.Craft.Name, this.GetType().Name);
        }

        /// <summary>
        /// Respond to this craft reaching a destination
        /// </summary>
        /// <remarks>default behaviour is to ignore</remarks>
        public virtual void OnDestinationReached() { }

        /// <summary>
        /// Respond to timer finishing counting down.
        /// </summary>
        /// <remarks>default behaviour is to ignore</remarks>
        public virtual void OnTimerFinished() { }

        /// <summary>
        /// Respond to a craft on the Geoscape being destroyed
        /// </summary>
        /// <param name="destroyedCraft">craft that's been destroyed</param>
        /// <remarks>default behaviour is to ignore</remarks>
        public virtual void OnCraftDestroyed(Craft destroyedCraft) { }

        /// <summary>
        /// Respond to this craft being destroyed
        /// </summary>
        /// <remarks>default behaviour is to ignore</remarks>
        public virtual void OnDestroyed() { }

        /// <summary>
        /// Respond to this craft finishing it's current mission
        /// </summary>
        /// <remarks>default behaviour is to ignore</remarks>
        public virtual void OnMissionFinished() { }

        /// <summary>
        /// Respond to this craft loosing sight of the craft it is hunting
        /// </summary>
        /// <remarks>default behaviour is to ignore</remarks>
        public virtual void OnPreyTrackingLost() { }

        /// <summary>
        /// Respond to prey escaping
        /// </summary>
        /// <remarks>default behaviour is to ignore</remarks>
        public virtual void OnPreyGone() { }

        /// <summary>
        /// Respond to craft running low on fuel
        /// </summary>
        /// <remarks>default behaviour is to ignore</remarks>
        public virtual void OnFuelLow() { }

        /// <summary>
        /// Respond to this craft reaching position where it can attack another craft
        /// </summary>
        /// <remarks>default behaviour is to ignore</remarks>
        public virtual void OnInAttackRange() { }

        /// <summary>
        /// Respond to dogfight about to start between this craft and another
        /// </summary>
        /// <remarks>default behaviour is to ignore</remarks>
        public virtual void OnDogfightStart() { }

        /// <summary>
        /// Respond to this craft finishing a dogfight with another craft
        /// </summary>
        /// <remarks>default behaviour is to ignore</remarks>
        public virtual void OnDogfightFinished() { }

        /// <summary>
        /// Respond to AlienSite this craft is heading towards ceasing to exist
        /// </summary>
        /// <param name="site">site that no longer exists</param>
        /// <remarks>default behaviour is to ignore</remarks>
        public virtual void OnSiteGone(AlienSite site) { }

        #region Fields

        /// <summary>
        /// Craft's current speed, in radians/second
        /// </summary>
        public double CurrentSpeed { get { return currentSpeed; } }

        /// <summary>
        /// The mission that owns this state
        /// </summary>
        protected Mission Mission { get { return mission; } }

        /// <summary>
        /// The mission that owns this state
        /// </summary>
        private Mission mission;

        /// <summary>
        /// Craft's current speed, in radians/second
        /// </summary>
        private double currentSpeed;

        #endregion
    }
}
