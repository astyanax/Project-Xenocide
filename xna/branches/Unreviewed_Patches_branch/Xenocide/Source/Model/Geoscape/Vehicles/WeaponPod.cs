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
* @file WeaponPod.cs
* @date Created: 2007/06/18
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
using ProjectXenocide.Model.Geoscape.GeoEvents;
using ProjectXenocide.Model.Battlescape;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// Base class that represents a weapon that is carried by an Aircraft or UFO
    /// essentially, it wraps the CraftWeaponItemInfo, and tracks the available ammo
    /// </summary>
    [Serializable]
    public class WeaponPod : Item
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="weapon">weapon in the pod</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "none")]
        public WeaponPod(CraftWeaponItemInfo weapon)
            :
            base(weapon, weapon.Clip, 0)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="weaponName">name of weapon (from item.xml) in the pod</param>
        public WeaponPod(String weaponName) :
            this((CraftWeaponItemInfo)Xenocide.StaticTables.ItemList[weaponName])
        {
        }

        /// <summary>
        /// Try to attack another craft
        /// </summary>
        /// <param name="target">craft to attack</param>
        /// <param name="log">record of combat</param>
        /// <returns>result of attempt</returns>
        public AttackResult Attack(Craft target, BattleLog log)
        {
            if (!HasAmmo)
            {
                return AttackResult.OutOfAmmo;
            }

            if (IsCycling(log))
            {
                return AttackResult.Nothing;
            }

            return Shoot(target, log);
        }

        /// <summary>
        /// Is weapon still cycling before it can fire again?
        /// </summary>
        /// <param name="log">record of battle,  Also time</param>
        /// <returns>true if weapon needs to finish cycling before it can fire</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Want it to throw if log is null")]
        public bool IsCycling(BattleLog log)
        {
            if (log.Now - timeLastFired < Weapon.TimeToShoot)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Shoot the weapon  (consuming a unit of ammo, if weapon uses ammo)
        /// </summary>
        /// <param name="target">craft to attack</param>
        /// <param name="log">record of combat</param>
        /// <returns>result of attempt</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Want it to throw if log is null")]
        public AttackResult Shoot(Craft target, BattleLog log)
        {
            timeLastFired = log.Now;
            log.Record(Strings.SCREEN_AEROSCAPE_WEAPON_FIRED, Weapon.Name);

            // reduce ammo
            if (UsesAmmo)
            {
                Debug.Assert(HasAmmo);
                --ShotsLeft;
            }

            // see if we missed
            if (!Xenocide.Rng.RollDice(Weapon.Accuracy))
            {
                log.Record(Strings.SCREEN_AEROSCAPE_WEAPON_MISSED, Weapon.Name);
                return AttackResult.Nothing;
            }

            // inflict damage on opponent
            log.Record(Strings.SCREEN_AEROSCAPE_WEAPON_HIT, Weapon.Name);
            return target.Hit(Weapon.WeaponDamage, log);
        }

        /// <summary>
        /// Tell Pod that owning craft is back in base, so it can start reloading
        /// </summary>
        public void PrepareForReload()
        {
            surplusAmmo = 0.0;
            outpostOutOfAmmo = false;
        }

        /// <summary>
        /// Any initialization that needs to be done before 
        /// </summary>
        public void PrepareForDogfight()
        {
            // Set this to long ago, so that we can fire immediately.
            timeLastFired = -100.0;
        }

        /// <summary>
        /// Update internal ammunition reserves, to reflect passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        /// <param name="outpost">Outpost to get ammunition from</param>
        /// <returns>false if pod is fully loaded</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if outpost == null")]
        public bool Reload(double milliseconds, Outpost outpost)
        {
            // if pod doesn't use ammo or is fully loaded, then we're done
            if (!UsesAmmo || (ShotsLeft == ClipSize))
            {
                return false;
            }

            // figure out how much ammo should have been put in craft, based on elapsed time
            double increment = Math.Min((ReloadRate * milliseconds) + surplusAmmo, (ClipSize - ShotsLeft));

            // allow for time not being integer multiple of reload rate
            int unitsToRemove = (int)increment;
            surplusAmmo = increment - unitsToRemove;

            // pull rounds from outpost.
            // fn returns min(stored in base, rounds wanted)
            int available = outpost.Inventory.DecreaseAmmoRoundsInArmory(Clip, unitsToRemove);

            // if we managed some rounds, then outpost wasn't empty when we started
            if (0 < available)
            {
                outpostOutOfAmmo = false;
                ShotsLeft += available;
            }

            // handle case of insufficient ammo
            if (available < unitsToRemove)
            {
                surplusAmmo = 0.0;

                // tell user (if we haven't already)
                if (!outpostOutOfAmmo)
                {
                    outpostOutOfAmmo = true;
                    MessageBoxGeoEvent.Queue(
                        Strings.MGSBOX_BASE_OUT_OF_CRAFT_SUPPLIES, outpost.Name, Clip.Name, Name
                    );
                }
            }

            return (ShotsLeft < ClipSize);
        }

        /// <summary>
        /// Get string describing the Pod's state, for display to user
        /// </summary>
        /// <returns>the description</returns>
        public String PodInformationString()
        {
            StringBuilder info = new StringBuilder();
            // case of pod holding a weapon
            info.Append(Util.StringFormat(Strings.SCREEN_EQUIP_CRAFT_POD_WEAPON, Name));
            info.Append(Util.Linefeed);

            if (UsesAmmo)
            {
                // case of weapon requiring ammo
                info.Append(Util.StringFormat(Strings.SCREEN_EQUIP_CRAFT_POD_CLIP_SIZE, Weapon.ClipSizeString()));
                info.Append(Util.Linefeed);
                info.Append(Util.StringFormat(Strings.SCREEN_EQUIP_CRAFT_POD_AMMO, ShotsLeft));
            }
            else
            {
                // case of weapon not requiring ammo
                info.Append(Strings.SCREEN_EQUIP_CRAFT_POD_NO_CLIP);
            }
            return info.ToString();
        }

        /// <summary>
        /// Get string describing the Pod's state, for display to user
        /// </summary>
        /// <param name="pod">pod to get information for (may be null)</param>
        /// <returns>the description</returns>
        public static String PodInformationString(WeaponPod pod)
        {
            if (null == pod)
            {
                return Strings.SCREEN_EQUIP_CRAFT_POD_EMPTY;
            }
            else
            {
                return pod.PodInformationString();
            }
        }

        #region Fields

        /// <summary>
        /// item that has the static properties of the weapon
        /// </summary>
        public CraftWeaponItemInfo Weapon { get { return ItemInfo as CraftWeaponItemInfo; } }

        /// <summary>
        /// item that has the static properties of the weapon's ammo
        /// </summary>
        public ClipItemInfo Clip { get { return Weapon.Clip; } }

        /// <summary>
        /// Does weapon use ammo?
        /// </summary>
        public bool UsesAmmo { get { return (Clip != null); } }

        /// <summary>
        /// Can the weapon be fired?
        /// </summary>
        /// <returns>True if weapon has ammo (or doesn't need it)</returns>
        public bool HasAmmo { get { return (!UsesAmmo || (0 < ShotsLeft)); } }

        /// <summary>
        /// Time (in TUs) to take a shot with this weapon
        /// </summary>
        public int TimeToShoot { get { return Weapon.TimeToShoot; } }

        /// <summary>
        /// Probability shot will hit target
        /// </summary>
        public double Accuracy { get { return Weapon.Accuracy; } }

        /// <summary>
        /// range (in meters) of the weapon
        /// </summary>
        public int WeaponRange { get { return Weapon.WeaponRange; } }

        /// <summary>
        /// damage (in hull points) inflicted by weapon
        /// </summary>
        public int WeaponDamage { get { return Weapon.WeaponDamage; } }

        /// <summary>
        /// Ammo left in pod, as % of total capacity
        /// </summary>
        public int AmmoStatus
        {
            get { return UsesAmmo ? Util.ToPercent(ShotsLeft, ClipSize) : 100; }
        }

        /// <summary>
        /// Maximum number of rounds that can fit in the pod (if weapon takes ammo)
        /// </summary>
        public int ClipSize { get { return Weapon.ClipSize; } }

        /// <summary>
        /// How quickly pod can reload, in rounds/millisecond.  Default is 3 hours to fully reload, regardless of ammo type
        /// </summary>
        public double ReloadRate { get { return ClipSize / (3.0 * 3600.0 * 1000.0); } }

        /// <summary>
        /// any ammo units left over increment left over from previous Reload() call
        /// </summary>
        private double surplusAmmo;

        /// <summary>
        /// Record that outpost ran out of ammo, and we've informed the user
        /// </summary>
        private bool outpostOutOfAmmo;

        /// <summary>
        /// When did this pod last fire in the current Dogfight?
        /// Used to figure out if it's had enough time to cycle.
        /// </summary>
        private double timeLastFired;

        #endregion
    }
}
