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

using ProjectXenocide.Utils;

using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Geoscape.AI;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// Base class that all Craft derive from
    /// (This represents a craft that can travel on the Geoscape)
    /// </summary>
    [Serializable]
    abstract public class Craft : Item, ICraftEvents, IGeoPosition
    {
        /// <summary>
        /// The "core" constructor.  All constructors ultimately call down to here
        /// </summary>
        /// <param name="craftType">type of craft (from StaticTables.ItemList)</param>
        /// <param name="name">Unique, player visible name for this craft</param>
        /// <param name="position">Initial location for craft</param>
        protected Craft(ItemInfo craftType, String name, GeoPosition position)
            :
            base(craftType)
        {
            this.position = position;
            this.name = name;

            // Figure out maximum speed (in Radians/sec) this craft is capable of
            maxSpeed = GeoPosition.KilometersToRadians(CraftItemInfo.MaxSpeed / 1000.0);

            // allocate storage for weapons
            weaponPods = new WeaponPod[NumHardpoints];
        }

        /// <summary>
        /// This constructor is used by UFOs
        /// </summary>
        /// <param name="craftType">type of craft (from item.xml)</param>
        /// <param name="name">Unique, player visible name for this craft</param>
        /// <param name="position">Initial location for UFO</param>
        protected Craft(String craftType, String name, GeoPosition position)
            :
            this(Xenocide.StaticTables.ItemList[craftType], name, position)
        {
        }

        /// <summary>
        /// This constructor is used by Aircraft
        /// </summary>
        /// <param name="craftType">type of craft (from StaticTables.ItemList)</param>
        /// <param name="name">Unique, player visible name for this craft</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
          Justification = "will throw if homeBase == null")]
        protected Craft(ItemInfo craftType, String name)
            :
            this(craftType, name, new GeoPosition())
        {
        }

        /// <summary>
        /// Called when craft is sold.  Do any necessary processing here
        /// </summary>
        public override void OnSell()
        {
            // strip craft of all weapons. (Player may want them elsewhere)
            for (int i = WeaponPods.Count - 1; 0 <= i; --i)
            {
                if (null != WeaponPods[i])
                {
                    HomeBase.Inventory.Add(WeaponPods[i], false);
                    WeaponPods[i] = null;
                }
            }

            // remove any soldiers and X-Caps assigned to craft
            OnTransfer();
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
        /// Is a craft showing on this craft's radar?
        /// </summary>
        /// <param name="otherPosition">Position of other craft</param>
        /// <returns>true if other craft is showing</returns>
        public virtual bool IsOnRadar(GeoPosition otherPosition)
        {
            return false;
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
        /// <remarks>
        /// 1. Default behaviour (used by UFOs) is do nothing
        /// 2. Currently there is no extra cost to rearm a craft, it's included in the reload time
        /// </remarks>
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

        /// <summary>
        /// Craft has entered an Outpost
        /// </summary>
        public virtual void EnterOutpost()
        {
            InBase = true;
        }

        /// <summary>
        /// Craft has departed an Outpost
        /// </summary>
        public virtual void ExitOutpost()
        {
            InBase = false;
        }

        /// <summary>
        /// Try to attack another craft
        /// </summary>
        /// <param name="target">craft to attack</param>
        /// <param name="log">record of combat</param>
        /// <returns>result of attempt</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Want it to throw if log is null")]
        public AttackResult Attack(Craft target, BattleLog log)
        {
            // if craft can't fight, we're done
            if (!IsArmed)
            {
                log.Record(Strings.SCREEN_AEROSCAPE_CRAFT_CANT_FIGHT, Name);
                return AttackResult.OutOfAmmo;
            }

            AttackResult result = AttackResult.Nothing;
            foreach (WeaponPod pod in weaponPods)
            {
                if (pod != null)
                {
                    result = pod.Attack(target, log);
                    if ((result == AttackResult.OpponentCrashed) ||
                        (result == AttackResult.OpponentDestroyed) ||
                        (result == AttackResult.OpponentFled))
                    {
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Craft has been hit by an attack
        /// </summary>
        /// <param name="strength">strength of the attack</param>
        /// <param name="log">record of combat</param>
        /// <returns>Result of attack</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Want it to throw if log is null")]
        public AttackResult Hit(double strength, BattleLog log)
        {
            // see if armour absorbed the damage
            if (Xenocide.Rng.RollDice(HullHardness))
            {
                log.Record(Strings.SCREEN_AEROSCAPE_ARMOUR_STOPPED_IT, Name);
                return AttackResult.Nothing;
            }

            hullDamage += strength;

            // see if destroyed, crashed, or just damaged
            if (IsDestroyed)
            {
                return HitDestroysCraft(log);
            }
            else if (IsCrashed)
            {
                log.Record(Strings.SCREEN_AEROSCAPE_CRAFT_CRASHED, Name);
                // if we're over water, craft is destroyed
                if (Xenocide.GameState.GeoData.Planet.IsPositionOverWater(Position))
                {
                    return HitDestroysCraft(log);
                }
                else
                {
                    OnCrashed();
                    return AttackResult.OpponentCrashed;
                }
            }
            else
            {
                log.Record(Strings.SCREEN_AEROSCAPE_HULL_DAMAGED, Name);
                return AttackResult.Nothing;
            }
        }

        /// <summary>
        /// Process being destroyed by enemy action
        /// </summary>
        /// <param name="log">record of combat</param>
        /// <returns>AttackResult.OpponentDestroyed</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Want it to throw if log is null")]
        private AttackResult HitDestroysCraft(BattleLog log)
        {
            log.Record(Strings.SCREEN_AEROSCAPE_CRAFT_DESTROYED, Name);
            OnDestroyed();
            return AttackResult.OpponentDestroyed;
        }

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

            // if craft is hunting this one, remove it from hunters list
            hunters.Remove(destroyedCraft);
        }

        /// <summary>
        /// Respond to this craft being destroyed
        /// </summary>
        public virtual void OnDestroyed()
        {
            // flag as destroyed
            hullDamage = MaxDamage;

            // tell all hunters this craft no longer exists
            for (int i = Hunters.Count - 1; 0 <= i; --i)
            {
                Hunters[i].OnCraftDestroyed(this);
            }

            // tell any prey this craft no longer exists
            if (null != Prey)
            {
                Prey.OnCraftDestroyed(this);
                Prey = null;
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
        /// Respond to dogfight about to start between this craft and another
        /// </summary>
        public virtual void OnDogfightStart()
        {
            // prep weapon pods
            foreach (WeaponPod pod in weaponPods)
            {
                if (null != pod)
                {
                    pod.PrepareForDogfight();
                }
            }

            mission.OnDogfightStart();
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
        /// Respond to AlienSite this craft is heading towards ceasing to exist
        /// </summary>
        /// <param name="site">site that no longer exists</param>
        /// <remarks>default behaviour is delegate to mission</remarks>
        public virtual void OnSiteGone(AlienSite site)
        {
            mission.OnSiteGone(site);
        }

        /// <summary>
        /// Respond to this craft crashing
        /// </summary>
        /// <remarks>default behaviour is delegate to mission</remarks>
        public virtual void OnCrashed()
        {
            mission.OnCrashed();
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

        /// <summary>
        /// Does X-Corp know the position of this craft?
        /// </summary>
        /// <returns>true if this craft shold be drawn</returns>
        public virtual bool IsKnownToXCorp { get { return true; } }

        #endregion

        #region Fields

        /// <summary>
        /// Location of craft on globe
        /// </summary>
        public GeoPosition Position { get { return position; } set { position = value; } }

        /// <summary>
        /// Maximum speed, in radians/second
        /// </summary>
        public double MaxSpeed { get { return maxSpeed; } }

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
        /// An alien site this craft is flying towards
        /// </summary>
        public AlienSite AlienSite { get { return alienSite; } set { alienSite = value; } }

        /// <summary>
        /// Craft that are "hunting" this craft
        /// </summary>
        public IList<Craft> Hunters { get { return hunters; } }

        /// <summary>
        /// Player readable identifier for this craft
        /// </summary>
        public override string Name { get { return name; } protected set { name = value; } }

        /// <summary>
        /// Outpost that "owns" the craft
        /// </summary>
        /// <remarks>At current time, doesn't apply to UFOs, but may in future</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if value == null")]
        public virtual Outpost HomeBase
        {
            get { return homeBase; }
            set
            {
                homeBase = value;
                if (null != homeBase)
                {
                    position = homeBase.Position;
                }
            }
        }

        /// <summary>
        /// Amount of fuel currently on board (in units)
        /// </summary>
        public virtual Double Fuel { get { throw new NotImplementedException(Strings.EXCEPTION_UFOS_LACK_FUEL); } }

        /// <summary>
        /// Hull's resistance to damage
        /// </summary>
        public double HullHardness { get { return CraftItemInfo.HullHardness; } }

        /// <summary>
        /// Maximum damage craft can take before being destroyed
        /// </summary>
        public int MaxDamage { get { return CraftItemInfo.MaxDamage; } }

        /// <summary>
        /// Number of points damage that have been inflicated on craft
        /// </summary>
        public double HullDamage { get { return hullDamage; } set { hullDamage = value; } }

        /// <summary>
        /// Has craft sustained sufficient damage to destroy it?
        /// </summary>
        public bool IsDestroyed { get { return (MaxDamage <= hullDamage); } }

        /// <summary>
        /// Has craft sustained sufficient damage to force it to crash land?
        /// </summary>
        public virtual bool IsCrashed
        {
            get { return ((MaxDamage / 2) < hullDamage) && !IsDestroyed; }
        }

        /// <summary>
        /// The weapons being carried by this craft
        /// </summary>
        public IList<WeaponPod> WeaponPods { get { return weaponPods; } }

        /// <summary>
        /// Craft is carring at least one loaded weapon
        /// </summary>
        public bool IsArmed
        {
            get
            {
                foreach (WeaponPod pod in WeaponPods)
                {
                    if ((null != pod) && pod.HasAmmo)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Can this craft carry troops?
        /// </summary>
        public virtual bool CanCarrySoldiers { get { return false; } }

        /// <summary>
        /// Is this craft carrying soldiers?
        /// </summary>
        public virtual bool IsCarryingSoldiers { get { return false; } }

        /// <summary>
        /// Number of weapon pods craft can carry
        /// </summary>
        public int NumHardpoints { get { return CraftItemInfo.NumHardpoints; } }

        /// <summary>
        /// The Item object holding the static properties of this type of aircraft
        /// </summary>
        public CraftItemInfo CraftItemInfo { get { return ItemInfo as CraftItemInfo; } }

        /// <summary>
        /// Location of craft on globe
        /// </summary>
        private GeoPosition position;

        /// <summary>
        /// State of craft's hull, to show on LaunchIntercept dialog
        /// </summary>
        public int HullPercent { get { return Util.ToPercent(MaxDamage - HullDamage, MaxDamage); } }

        /// <summary>
        /// Build up string of form [x]/[y] where [x] is number of pods and [y] is max pods possible 
        /// </summary>
        public string PodCountStatus
        {
            get
            {
                int count = 0;
                for (int i = 0; i < WeaponPods.Count; ++i)
                {
                    count += ((null == WeaponPods[i]) ? 0 : 1);
                }
                return Util.StringFormat(Strings.POD_COUNT_STATUS, count, WeaponPods.Count);
            }
        }

        /// <summary>
        /// Return string indicating status of ammo in this craft's pod(s) 
        /// </summary>
        public string AmmoStatus
        {
            get
            {
                StringBuilder status = new StringBuilder();
                for (int i = 0; i < WeaponPods.Count; ++i)
                {
                    if (null != WeaponPods[i])
                    {
                        // add '/' separator, if there's 2 pods
                        if (!String.IsNullOrEmpty(status.ToString()))
                        {
                            status.Append("/");
                        }
                        status.Append(Util.StringFormat("{0}", WeaponPods[i].AmmoStatus));
                    }
                }
                return status.ToString();
            }
        }

        /// <summary>
        /// Craft's current speed, in meters/sec, for display to user
        /// </summary>
        public double MetersPerSecond
        {
            get
            {
                return GeoPosition.RadiansToKilometers(Mission.State.CurrentSpeed) * 1000.0;
            }
        }

        /// <summary>
        /// Maximum speed, in radians/second
        /// </summary>
        private double maxSpeed;

        /// <summary>
        /// Mission that determines craft's current behaviour
        /// </summary>
        private Mission mission;

        /// <summary>
        /// Outpost that "owns" the craft
        /// </summary>
        /// <remarks>At current time, doesn't apply to UFOs, but may in future</remarks>
        private Outpost homeBase;

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

        /// <summary>
        /// An alien site this craft is flying towards
        /// </summary>
        private AlienSite alienSite;

        /// <summary>
        /// Number of points damage that have been inflicated on craft
        /// </summary>
        private double hullDamage;

        /// <summary>
        /// The weapons being carried by this craft
        /// </summary>
        private WeaponPod[] weaponPods;

        /// <summary>
        /// Player readable identifier for this craft
        /// </summary>
        private string name;

        #endregion
    }
}
