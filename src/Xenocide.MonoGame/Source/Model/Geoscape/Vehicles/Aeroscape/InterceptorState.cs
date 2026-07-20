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
* @file InterceptorState.cs
* @date Created: 2026/07/20
* @author File creator: Xenocide Agent
* @author Credits: none
*/
#endregion

using System;

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// Tracks per-interceptor state during an aeroscape dogfight.
    /// Each interceptor in a multi-interceptor engagement has its own
    /// weapon cooldowns and enable states.
    /// </summary>
    [Serializable]
    public class InterceptorState
    {
        /// <summary>
        /// The aircraft engaging in this dogfight.
        /// </summary>
        public Aircraft Aircraft { get; private set; }

        /// <summary>
        /// Whether weapon pod 1 is enabled (player can toggle during fight).
        /// </summary>
        public bool Weapon1Enabled { get; set; } = true;

        /// <summary>
        /// Whether weapon pod 2 is enabled (player can toggle during fight).
        /// </summary>
        public bool Weapon2Enabled { get; set; } = true;

        /// <summary>
        /// Cooldown timer for weapon pod 1 (seconds until next shot).
        /// </summary>
        public double W1FireCountdown { get; set; }

        /// <summary>
        /// Cooldown timer for weapon pod 2 (seconds until next shot).
        /// </summary>
        public double W2FireCountdown { get; set; }

        /// <summary>
        /// Current tactical mode for this interceptor.
        /// </summary>
        public TacticalMode Mode { get; set; } = TacticalMode.Standard;

        /// <summary>
        /// Whether this interceptor is still actively engaged.
        /// Set to false when it disengages, runs out of ammo, or is destroyed.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Whether this interceptor has any weapons that can still fire.
        /// </summary>
        public bool HasUsableWeapons
        {
            get
            {
                if (Aircraft.WeaponPods.Count == 0)
                    return false;

                bool w1Ready = Weapon1Enabled && Aircraft.WeaponPods.Count > 0 &&
                               Aircraft.WeaponPods[0] != null && Aircraft.WeaponPods[0].HasAmmo;
                bool w2Ready = Weapon2Enabled && Aircraft.WeaponPods.Count > 1 &&
                               Aircraft.WeaponPods[1] != null && Aircraft.WeaponPods[1].HasAmmo;
                return w1Ready || w2Ready;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public InterceptorState(Aircraft aircraft)
        {
            Aircraft = aircraft ?? throw new ArgumentNullException(nameof(aircraft));
        }

        /// <summary>
        /// Update weapon cooldown timers by the given elapsed time.
        /// </summary>
        public void UpdateCooldowns(double elapsedSeconds)
        {
            if (W1FireCountdown > 0)
                W1FireCountdown = Math.Max(0, W1FireCountdown - elapsedSeconds);

            if (W2FireCountdown > 0)
                W2FireCountdown = Math.Max(0, W2FireCountdown - elapsedSeconds);
        }
    }
}
