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
* @file AeroscapeState.cs
* @date Created: 2026/07/20
* @author File creator: Xenocide Agent
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;

using ProjectXenocide.Model.Battlescape;

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// Core simulation state for an aeroscape dogfight between aircraft and UFO.
    /// Holds all per-interceptor states, distance, timing, and UFO AI.
    /// </summary>
    /// <remarks>
    /// ARCHITECTURE: This is the model layer for the aeroscape simulation.
    /// It contains zero UI/GUI references. The controller (AeroscapeSimulation)
    /// orchestrates ticks; the screen reads properties for display.
    ///
    /// DISTANCE MODEL:
    /// - Maximum standoff distance: 80,000 meters
    /// - Each interceptor closes toward a target distance based on its tactical mode
    /// - Distance is shared (UFO perspective): the UFO sees the nearest interceptor
    ///
    /// TACTICAL MODES (per interceptor):
    /// - Standoff: maintain 80,000m, no fire
    /// - Cautious: target = max weapon range, 1.5x cooldown
    /// - Standard: target = max weapon range (safe firing distance), 1.0x cooldown
    /// - Aggressive: target = 1,000m, 0.75x cooldown
    /// - Disengage: increase distance to 80,000m, then deactivate
    /// </remarks>
    public class AeroscapeState
    {
        /// <summary>
        /// Maximum distance on the radar (standoff range) in meters.
        /// Set to 60,000 so that long-range weapons (Titan 60km, GAIA 65km) can fire at standoff.
        /// </summary>
        public const double MaxDistance = 60000.0;

        /// <summary>
        /// Minimum safe distance (prevents collision/oscillation).
        /// </summary>
        public const double MinDistance = 100.0;

        /// <summary>
        /// The UFO being intercepted.
        /// </summary>
        public Ufo Ufo { get; private set; }

        /// <summary>
        /// All interceptors in this dogfight, with their per-craft state.
        /// </summary>
        public List<InterceptorState> Interceptors { get; private set; }

        /// <summary>
        /// Index of the currently selected interceptor (for UI display).
        /// </summary>
        public int SelectedInterceptorIndex { get; set; }

        /// <summary>
        /// The currently selected interceptor's state.
        /// </summary>
        public InterceptorState SelectedInterceptor
        {
            get
            {
                if (Interceptors.Count == 0)
                    return null;
                int idx = Math.Max(0, Math.Min(SelectedInterceptorIndex, Interceptors.Count - 1));
                return Interceptors[idx];
            }
        }

        /// <summary>
        /// Distance between the nearest interceptor and the UFO, in meters.
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// Previous frame's distance, for smooth display interpolation.
        /// </summary>
        public double PrevDistance { get; set; }

        /// <summary>
        /// Current dogfight outcome. Set when the fight ends.
        /// </summary>
        public DogfightOutcome Outcome { get; set; } = DogfightOutcome.InProgress;

        /// <summary>
        /// The battle log recording all combat events.
        /// </summary>
        public BattleLog Log { get; private set; }

        /// <summary>
        /// Elapsed dogfight time in seconds.
        /// </summary>
        public double ElapsedSeconds { get; set; }

        /// <summary>
        /// UFO AI state: escape countdown in seconds.
        /// Decreases over time (faster when damaged). When it reaches 0, UFO flees.
        /// </summary>
        public double UfoEscapeCountdown { get; set; }

        /// <summary>
        /// Whether the UFO is a large/aggressive type that actively hunts interceptors.
        /// Small UFOs (scouts, research, supply) try to escape instead.
        /// </summary>
        public bool UfoIsAggressive { get; private set; }

        /// <summary>
        /// UFO AI state: target distance the UFO wants to maintain (in meters).
        /// </summary>
        public double UfoPreferredDistance { get; set; }

        /// <summary>
        /// UFO AI state: cooldown timer until next UFO weapon fire.
        /// </summary>
        public double UfoFireCountdown { get; set; }

        /// <summary>
        /// Whether all active interceptors have exhausted their weapons.
        /// Unarmed interceptors are excluded (they were never "in ammo").
        /// </summary>
        public bool AllInterceptorsOutofAmmo
        {
            get
            {
                bool anyHasWeapons = false;
                foreach (var s in Interceptors)
                {
                    if (s.IsActive && s.HasWeapons)
                    {
                        anyHasWeapons = true;
                        if (s.HasUsableWeapons)
                            return false;
                    }
                }
                // If no interceptor has weapons at all, this isn't "out of ammo"
                return anyHasWeapons;
            }
        }

        /// <summary>
        /// Whether all interceptors are inactive (disengaged or destroyed).
        /// </summary>
        public bool AllInterceptorsInactive
        {
            get
            {
                foreach (var s in Interceptors)
                {
                    if (s.IsActive)
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AeroscapeState(Ufo ufo, IList<Aircraft> aircraft)
        {
            Ufo = ufo ?? throw new ArgumentNullException(nameof(ufo));
            Log = new BattleLog();
            Distance = MaxDistance;
            UfoPreferredDistance = 10000.0;
            UfoEscapeCountdown = 120.0;
            UfoFireCountdown = 3.0;

            // Large and very large UFOs actively hunt interceptors
            string size = ufo.UfoItemInfo.UfoSize;
            UfoIsAggressive = size.Contains("LARGE") || size.Contains("Large");

            Interceptors = new List<InterceptorState>();
            if (aircraft != null)
            {
                foreach (var ac in aircraft)
                {
                    Interceptors.Add(new InterceptorState(ac));
                }
            }
        }

        /// <summary>
        /// Get the maximum weapon range across all weapons of an interceptor (in meters).
        /// Returns 0 if the interceptor has no weapons.
        /// </summary>
        public static int GetMaxWeaponRange(InterceptorState interceptor)
        {
            int maxRange = 0;
            foreach (var pod in interceptor.Aircraft.WeaponPods)
            {
                if (pod != null && pod.WeaponRange > maxRange)
                    maxRange = pod.WeaponRange;
            }
            return maxRange;
        }

        /// <summary>
        /// Get the minimum weapon range across all weapons of an interceptor (in meters).
        /// Falls back to max range if no weapons (prevents 0-range suicide approach).
        /// </summary>
        public static int GetMinWeaponRange(InterceptorState interceptor)
        {
            int minRange = int.MaxValue;
            int maxRange = 0;
            foreach (var pod in interceptor.Aircraft.WeaponPods)
            {
                if (pod != null)
                {
                    if (pod.WeaponRange < minRange)
                        minRange = pod.WeaponRange;
                    if (pod.WeaponRange > maxRange)
                        maxRange = pod.WeaponRange;
                }
            }
            // If no weapons, return max range (which is 0, handled by caller)
            if (minRange == int.MaxValue)
                return maxRange;
            return minRange;
        }

        /// <summary>
        /// Get the UFO's maximum weapon range (in meters).
        /// </summary>
        public int GetUfoMaxWeaponRange()
        {
            int maxRange = 0;
            foreach (var pod in Ufo.WeaponPods)
            {
                if (pod != null && pod.WeaponRange > maxRange)
                    maxRange = pod.WeaponRange;
            }
            return maxRange;
        }

        /// <summary>
        /// Compute the cooldown speed multiplier for a given tactical mode.
        /// Lower multiplier = faster firing.
        /// </summary>
        public static double GetCooldownMultiplier(TacticalMode mode)
        {
            switch (mode)
            {
                // Matches original UFO:EU frame ratios (64:48:32 for launched weapons)
                case TacticalMode.Cautious: return 1.333;
                case TacticalMode.Standard: return 1.0;
                case TacticalMode.Aggressive: return 0.667;
                default: return 1.0;
            }
        }

        /// <summary>
        /// Get the target distance for a given tactical mode (in meters).
        /// </summary>
        public static double GetTargetDistance(TacticalMode mode, InterceptorState interceptor)
        {
            switch (mode)
            {
                case TacticalMode.Standoff:
                    // Stand at max weapon range so long-range weapons can fire
                    int standoffRange = GetMaxWeaponRange(interceptor);
                    return standoffRange > 0 ? standoffRange : MaxDistance;
                case TacticalMode.Cautious:
                    return GetMaxWeaponRange(interceptor);
                case TacticalMode.Standard:
                    // Use max range for safety; min range for Standard would be risky
                    return GetMaxWeaponRange(interceptor);
                case TacticalMode.Aggressive:
                    return 1000.0;
                case TacticalMode.Disengage:
                    return MaxDistance;
                default:
                    return MaxDistance;
            }
        }
    }
}
