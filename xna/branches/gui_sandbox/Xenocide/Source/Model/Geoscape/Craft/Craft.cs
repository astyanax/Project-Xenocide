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
* @file Craft.cs
* @date Created: 2007/02/05
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Xenocide.Resources;
using Xenocide.Model.Geoscape.HumanBases;

#endregion

namespace Xenocide.Model.Geoscape.Craft
{
    /// <summary>
    /// Base class that all Craft derive from
    /// (This represents a craft that can travel on the Geoscape)
    /// </summary>
    [Serializable]
    abstract public class Craft : ICraftEvents
    {
        /// <summary>
        /// This constructor is used by UFOs
        /// </summary>
        /// <param name="maxSpeed">Fastest speed craft is capable of</param>
        /// <param name="position">Initial location for UFO</param>
        protected Craft(double maxSpeed, GeoPosition position)
        {
            this.maxSpeed = maxSpeed;
            this.position = position;
        }

        /// <summary>
        /// This constructor is used by Aircraft
        /// </summary>
        /// <param name="maxSpeed">Fastest speed craft is capable of</param>
        /// <param name="homeBase">Craft's home base</param>
        protected Craft(double maxSpeed, HumanBase homeBase)
        {
            this.maxSpeed = maxSpeed;
            this.position = homeBase.Position;
            this.homeBase = homeBase;
        }

        /// <summary>
        /// Tell this craft about another craft that is hunting it 
        /// </summary>
        /// <param name="craft">craft hunting this one</param>
        public void AddHunter(Craft craft)
        {
            Hunters.Add(craft);
        }

        /// <summary>
        /// Tell this craft about another craft that was hunting it 
        /// </summary>
        /// <param name="craft">craft that stopped hunting this one</param>
        public void RemoveHunter(Craft craft)
        {
            Hunters.Remove(craft);
        }

        #region update state in response to passage of time

        /// <summary>
        /// Update the craft, due to passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        virtual public void Update(double milliseconds)
        {
            mission.Update(milliseconds);
        }

        /// <summary>
        /// Update internal fuel reserves, to reflect passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        /// <returns>false if craft is fully refueled</returns>
        /// <remarks>Default behaviour (used by UFOs) is do nothing</remarks>
        virtual public bool Refuel(double milliseconds)
        {
            return false;
        }

        /// <summary>
        /// Update internal ammunition reserves, to reflect passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        /// <returns>false if craft is fully reloaded</returns>
        /// <remarks>Default behaviour (used by UFOs) is do nothing</remarks>
        virtual public bool Reload(double milliseconds)
        {
            return false;
        }

        /// <summary>
        /// Update craft's "health", to reflect passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        /// <returns>false if craft is fully repaired</returns>
        /// <remarks>Default behaviour (used by UFOs) is do nothing</remarks>
        virtual public bool Repair(double milliseconds)
        {
            return false;
        }

        /// <summary>
        /// Update weapon pods installed in craft, to reflect passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        /// <returns>false if craft is fully rearmed</returns>
        /// <remarks>Default behaviour (used by UFOs) is do nothing</remarks>
        virtual public bool Rearm(double milliseconds)
        {
            return false;
        }

        /// <summary>
        /// Update internal fuel reserves, to reflect passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        /// <returns>false if Fuel at "return to base" level</returns>
        /// <remarks>Default behaviour (used by UFOs) is do nothing</remarks>
        virtual public bool ConsumeFuel(double milliseconds)
        {
            return true;
        }

        #endregion

        #region react to events on the geoscape

        /// <summary>
        /// Respond to this craft reaching a destination
        /// </summary>
        /// <remarks>default behaviour is delegate to mission</remarks>
        public virtual void OnDestinationReached()
        {
            mission.OnDestinationReached();
        }

        /// <summary>
        /// Respond to timer finishing counting down.
        /// </summary>
        /// <remarks>default behaviour is delegate to mission</remarks>
        public virtual void OnTimerFinished()
        {
            mission.OnTimerFinished();
        }

        /// <summary>
        /// Respond to a craft on the Geoscape being destroyed
        /// </summary>
        /// <param name="destroyedCraft">craft that's been destroyed</param>
        /// <remarks>default behaviour is delegate to mission</remarks>
        public virtual void OnCraftDestroyed(Craft destroyedCraft)
        {
            mission.OnCraftDestroyed(destroyedCraft);
        }

        /// <summary>
        /// Respond to this craft being destroyed
        /// </summary>
        public virtual void OnDestroyed()
        {
            // tell all hunters this craft no longer exists
            for (int i = Hunters.Count - 1; 0 <= i; --i)
            {
                Hunters[i].OnCraftDestroyed(this);
            }
            
            // pass on to mission for any other processing
            mission.OnDestroyed();
        }

        /// <summary>
        /// Respond to this craft finishing it's current mission
        /// </summary>
        /// <remarks>default behaviour is delegate to mission</remarks>
        public virtual void OnMissionFinished()
        {
            // tell all hunters this craft no longer exists
            for (int i = Mission.Craft.Hunters.Count - 1; 0 <= i; --i)
            {
                Mission.Craft.Hunters[i].OnPreyGone();
            }

            mission.OnMissionFinished();
        }

        /// <summary>
        /// Respond to this craft loosing sight of the craft it is hunting
        /// </summary>
        /// <remarks>Don't confuse this with prey escaping</remarks>
        /// <remarks>default behaviour is delegate to mission</remarks>
        public virtual void OnPreyTrackingLost()
        {
            mission.OnPreyTrackingLost();
        }

        /// <summary>
        /// Respond to prey escaping
        /// </summary>
        /// <remarks>default behaviour is delegate to mission</remarks>
        public virtual void OnPreyGone()
        {
            mission.OnPreyGone();
        }

        /// <summary>
        /// Respond to craft running low on fuel
        /// </summary>
        /// <remarks>default behaviour is delegate to mission</remarks>
        public virtual void OnFuelLow()
        {
            mission.OnFuelLow();
        }

        /// <summary>
        /// Respond to this craft reaching position where it can attack another craft
        /// </summary>
        /// <remarks>default behaviour is delegate to mission</remarks>
        public virtual void OnInAttackRange()
        {
            mission.OnInAttackRange();
        }

        /// <summary>
        /// Respond to this craft finishing a dogfight with another craft
        /// </summary>
        /// <remarks>default behaviour is delegate to mission</remarks>
        public virtual void OnDogfightFinished()
        {
            mission.OnDogfightFinished();
        }

        /// <summary>
        /// Respond to this craft crashing
        /// </summary>
        /// <remarks>default behaviour is do nothing</remarks>
        public virtual void OnCrashed()
        {
        }

        #endregion

        #region tests of craft's condition

        /// <summary>
        /// Does craft need to return to base for refueling?
        /// </summary>
        /// <returns>true if craft has just enough fuel to reach base</returns>
        /// <remarks>UFO's don't need fuel, so default is return false</remarks>
        public virtual bool IsFuelLow()
        {
            return false;
        }

        #endregion

        /// <summary>
        /// Find out if this craft should be drawn on the Geoscape
        /// </summary>
        /// <returns>true if this craft shold be drawn</returns>
        abstract public bool CanDrawOnGeoscape();

        #region Fields

        /// <summary>
        /// Location of craft on globe
        /// </summary>
        public GeoPosition Position { get { return position; } set { position = value; } }

        /// <summary>
        /// Maximum speed, in radians/second
        /// </summary>
        public double MaxSpeed { get { return maxSpeed; } set { maxSpeed = value; } }

        /// <summary>
        /// Mission that determines craft's current behaviour
        /// </summary>
        public Mission Mission 
        { 
            get { return mission; }
            set
            {
                // old mission needs to be stopped (by calling Mission.Abort() before changing it)
                Debug.Assert((null == mission) || (null == mission.State));

                mission = value;
            }
        }

        /// <summary>
        /// Is the craft currently in a base?
        /// </summary>
        /// <remarks>At current time, doesn't apply to UFOs, but may in future</remarks>
        public virtual bool InBase { get { return inBase; } set { inBase = value; } }

        /// <summary>
        /// The craft that this craft is hunting
        /// </summary>
        public Craft Prey { get { return prey; } set { prey = value; } }

        /// <summary>
        /// Craft that are "hunting" this craft
        /// </summary>
        public IList<Craft> Hunters { get { return hunters; } }

        /// <summary>
        /// Player readable identifier for this craft
        /// </summary>
        public virtual string Name { get { return "<ToDo>"; } }

        /// <summary>
        /// Base that "owns" the craft
        /// </summary>
        /// <remarks>At current time, doesn't apply to UFOs, but may in future</remarks>
        public virtual HumanBase HomeBase { get { return homeBase; } set { homeBase = value; } }

        /// <summary>
        /// Amount of fuel currently on board (in units)
        /// </summary>
        public virtual Double Fuel { get { throw new NotImplementedException(Strings.ResourceManager.GetString("EXCEPTION_UFOS_LACK_FUEL")); } }

        /// <summary>
        /// Location of craft on globe
        /// </summary>
        private GeoPosition position;

        /// <summary>
        /// Maximum speed, in radians/second
        /// </summary>
        private double maxSpeed;

        /// <summary>
        /// Mission that determines craft's current behaviour
        /// </summary>
        private Mission mission;

        /// <summary>
        /// Base that "owns" the craft
        /// </summary>
        /// <remarks>At current time, doesn't apply to UFOs, but may in future</remarks>
        private HumanBase homeBase;

        /// <summary>
        /// Is the craft currently in a base?
        /// </summary>
        /// <remarks>At current time, doesn't apply to UFOs, but may in future</remarks>
        private bool inBase;

        /// <summary>
        /// Craft that are "hunting" this craft
        /// </summary>
        private List<Craft> hunters = new List<Craft>();

        /// <summary>
        /// The craft that this craft is hunting
        /// </summary>
        private Craft prey;

        #endregion
    }
}
