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
    /// - Standard: target = min weapon range, 1.0x cooldown
    /// - Aggressive: target = 1,000m, 0.75x cooldown
    /// - Disengage: increase distance to 80,000m, then end
    ///
    /// UFO AI:
    /// - Maintains optimal firing distance (5,000-15,000m)
    /// - Fires when in weapon range
    /// - Has an escape countdown that triggers if hull drops below threshold
    /// </remarks>
    public class AeroscapeState
    {
        /// <summary>
        /// Maximum distance on the radar (standoff range) in meters.
        /// </summary>
        public const double MaxDistance = 80000.0;

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
        /// Decreases when UFO hull is damaged. When it reaches 0, UFO flees.
        /// </summary>
        public double UfoEscapeCountdown { get; set; }

        /// <summary>
        /// UFO AI state: target distance the UFO wants to maintain (in meters).
        /// </summary>
        public double UfoPreferredDistance { get; set; }

        /// <summary>
        /// UFO AI state: cooldown timer until next UFO weapon fire.
        /// </summary>
        public double UfoFireCountdown { get; set; }

        /// <summary>
        /// Whether the nearest interceptor has any weapons left.
        /// </summary>
        public bool AllInterceptorsOutofAmmo
        {
            get
            {
                foreach (var state in Interceptors)
                {
                    if (state.IsActive && state.HasUsableWeapons)
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Whether all interceptors are inactive (disengaged or destroyed).
        /// </summary>
        public bool AllInterceptorsInactive
        {
            get
            {
                foreach (var state in Interceptors)
                {
                    if (state.IsActive)
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// The active interceptor nearest to the UFO.
        /// </summary>
        public InterceptorState NearestInterceptor
        {
            get
            {
                InterceptorState nearest = null;
                foreach (var state in Interceptors)
                {
                    if (state.IsActive)
                    {
                        if (nearest == null ||
                            GetInterceptorSpeed(state) > GetInterceptorSpeed(nearest))
                        {
                            nearest = state;
                        }
                    }
                }
                return nearest;
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
            UfoEscapeCountdown = 30.0;
            UfoFireCountdown = 3.0;

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
        /// </summary>
        public static int GetMinWeaponRange(InterceptorState interceptor)
        {
            int minRange = int.MaxValue;
            foreach (var pod in interceptor.Aircraft.WeaponPods)
            {
                if (pod != null && pod.WeaponRange < minRange)
                    minRange = pod.WeaponRange;
            }
            return minRange == int.MaxValue ? 0 : minRange;
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
        /// Compute the speed multiplier for a given tactical mode.
        /// </summary>
        public static double GetCooldownMultiplier(TacticalMode mode)
        {
            switch (mode)
            {
                case TacticalMode.Cautious: return 1.5;
                case TacticalMode.Standard: return 1.0;
                case TacticalMode.Aggressive: return 0.75;
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
                    return MaxDistance;
                case TacticalMode.Cautious:
                    return GetMaxWeaponRange(interceptor);
                case TacticalMode.Standard:
                    return GetMinWeaponRange(interceptor);
                case TacticalMode.Aggressive:
                    return 1000.0;
                case TacticalMode.Disengage:
                    return MaxDistance;
                default:
                    return MaxDistance;
            }
        }

        private static double GetInterceptorSpeed(InterceptorState interceptor)
        {
            return interceptor.Aircraft.CraftItemInfo.MaxSpeed;
        }
    }
}
