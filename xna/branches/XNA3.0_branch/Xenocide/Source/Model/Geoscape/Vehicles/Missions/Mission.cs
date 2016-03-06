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
* @file Mission.cs
* @date Created: 2007/02/10
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using ProjectXenocide.Model.Geoscape.AI;

#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// Base class for the Hierachial finite state machine that drives a craft's 
    /// behaviour while on a mission.  The different Missions derive from this class
    /// </summary>
    [Serializable]
    abstract public class Mission : ICraftEvents
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="craft">craft that "owns" this mission</param>
        protected Mission(Craft craft)
        {
            this.craft = craft;
        }

        /// <summary>
        /// Update state, to reflect passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        virtual public void Update(double milliseconds)
        {
            if (DoChecks(milliseconds))
            {
                State.Update(milliseconds);
            }
        }

        /// <summary>
        /// Change the state
        /// </summary>
        /// <param name="newState">state that will replace the current state</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if newState == null")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "method performs a time-consuming operation")]
        public void SetState(MissionState newState)
        {
            if (state != null)
            {
                state.OnExitState();
            }
            state = newState;
            state.OnEnterState();
        }

        /// <summary>
        /// usually, player is about to give craft a new mission
        /// </summary>
        public virtual void Abort()
        {
            // give the current state a chance to clean itself up
            if (state != null)
            {
                state.OnExitState();
                state = null;
            }
        }

        #region react to events on the geoscape

        /// <summary>
        /// Respond to this craft reaching a destination
        /// </summary>
        /// <remarks>default behaviour is delegate to state</remarks>
        public virtual void OnDestinationReached() 
        {
            State.OnDestinationReached();
        }

        /// <summary>
        /// Respond to timer finishing counting down.
        /// </summary>
        /// <remarks>default behaviour is delegate to state</remarks>
        public virtual void OnTimerFinished()
        {
            State.OnTimerFinished();
        }

        /// <summary>
        /// Respond to a craft on the Geoscape being destroyed
        /// </summary>
        /// <param name="destroyedCraft">The craft that has been destroyed</param>
        /// <remarks>default behaviour is delegate to state</remarks>
        public virtual void OnCraftDestroyed(Craft destroyedCraft)
        {
            State.OnCraftDestroyed(destroyedCraft);
        }

        /// <summary>
        /// Respond to this craft being destroyed
        /// </summary>
        /// <remarks>default behaviour is delegate to state</remarks>
        public virtual void OnDestroyed()
        {
            State.OnDestroyed();
        }

        /// <summary>
        /// Respond to this craft finishing it's current mission
        /// </summary>
        /// <remarks>default behaviour is delegate to state</remarks>
        public virtual void OnMissionFinished()
        {
            State.OnMissionFinished();
        }

        /// <summary>
        /// Respond to this craft loosing sight of the craft it is hunting
        /// </summary>
        /// <remarks>Don't confuse this with prey escaping</remarks>
        /// <remarks>default behaviour is delegate to state</remarks>
        public virtual void OnPreyTrackingLost()
        {
            State.OnPreyTrackingLost();
        }

        /// <summary>
        /// Respond to prey escaping
        /// </summary>
        /// <remarks>default behaviour is delegate to state</remarks>
        public virtual void OnPreyGone()
        {
            State.OnPreyGone();
        }

        /// <summary>
        /// Respond to craft running low on fuel
        /// </summary>
        /// <remarks>default behaviour is delegate to state</remarks>
        public virtual void OnFuelLow()
        {
            State.OnFuelLow();
        }

        /// <summary>
        /// Respond to this craft reaching position where it can attack another craft
        /// </summary>
        /// <remarks>default behaviour is delegate to state</remarks>
        public virtual void OnInAttackRange()
        {
            State.OnInAttackRange();
        }

        /// <summary>
        /// Respond to dogfight about to start between this craft and another
        /// </summary>
        /// <remarks>default behaviour is delegate to state</remarks>
        public virtual void OnDogfightStart()
        {
            State.OnDogfightStart();
        }

        /// <summary>
        /// Respond to this craft finishing a dogfight with another craft
        /// </summary>
        /// <remarks>default behaviour is delegate to state</remarks>
        public virtual void OnDogfightFinished()
        {
            State.OnDogfightFinished();
        }

        /// <summary>
        /// Respond to AlienSite this craft is heading towards ceasing to exist
        /// </summary>
        /// <param name="site">site that no longer exists</param>
        /// <remarks>default behaviour is delegate to state</remarks>
        public virtual void OnSiteGone(AlienSite site)
        {
            State.OnSiteGone(site);
        }

        /// <summary>
        /// Respond to this craft crashing
        /// </summary>
        /// <remarks>default behaviour is tag mission as a failure</remarks>
        public virtual void OnCrashed()
        {
        }

        #endregion

        /// <summary>
        /// Check for any events that may have occured since last update
        /// </summary>
        /// <param name="milliseconds">Time that has passed</param>
        /// <returns>false if event has happened that should cancel the update</returns>
        protected virtual bool DoChecks(double milliseconds)
        {
            return true;
        }

        #region Fields

        /// <summary>
        /// Current state
        /// </summary>
        public MissionState State { get { return state; } }

        /// <summary>
        /// Was the UFO successful in its part of the mission?
        /// </summary>
        /// <remarks>Except for Supply and Outpost construction, UFO suceeds, unless it's shot down</remarks>
        public virtual bool Success { get { return Craft.IsCrashed; } }

        /// <summary>
        /// Current state
        /// </summary>
        private MissionState state;

        /// <summary>
        /// craft that "owns" this mission
        /// </summary>
        public Craft Craft { get { return craft; } }

        /// <summary>
        /// craft that "owns" this mission
        /// </summary>
        private Craft craft;

        #endregion
    }
}
