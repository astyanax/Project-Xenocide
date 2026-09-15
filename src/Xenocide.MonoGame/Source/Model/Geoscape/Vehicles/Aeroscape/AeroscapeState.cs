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
    /// - Maximum standoff distance: 60,000 meters (see <see cref="MaxDistance"/>)
    /// - Each interceptor tracks its own distance and closes toward a target
    ///   distance based on its tactical mode (<see cref="InterceptorState.Distance"/>)
    /// - The UFO perceives the nearest interceptor (<see cref="NearestDistance"/>),
    ///   so multiple interceptors already resolve correctly.
    ///
    /// TACTICAL MODES (per interceptor), matching the X-COM stance model:
    /// - Standoff: hold at max weapon range (observe, low risk)
    /// - Cautious: hold at ~90% of max weapon range (fire from range)
    /// - Standard: hold at ~75% of max weapon range (balanced)
    /// - Aggressive: close to ~1,000m (maximum damage, highest risk)
    /// - Disengage: retreat to max distance, then leave the fight
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
        /// Distance from the UFO to the nearest active interceptor (what the UFO
        /// "sees"). With a single interceptor this is simply its distance; the
        /// value is derived per tick so multi-interceptor engagements resolve
        /// without further refactoring.
        /// </summary>
        public double NearestDistance
        {
            get
            {
                double nearest = MaxDistance;
                bool anyActive = false;
                foreach (InterceptorState s in Interceptors)
                {
                    if (s.IsActive)
                    {
                        anyActive = true;
                        if (s.Distance < nearest)
                            nearest = s.Distance;
                    }
                }
                return anyActive ? nearest : Distance;
            }
        }

        /// <summary>The nearest active interceptor, or null if none are active.</summary>
        public InterceptorState NearestInterceptor
        {
            get
            {
                InterceptorState nearest = null;
                double best = double.MaxValue;
                foreach (InterceptorState s in Interceptors)
                {
                    if (s.IsActive && s.Distance < best)
                    {
                        best = s.Distance;
                        nearest = s;
                    }
                }
                return nearest;
            }
        }

        /// <summary>
        /// Current dogfight outcome. Set when the fight ends.
        /// </summary>
        public DogfightOutcome Outcome { get; set; } = DogfightOutcome.InProgress;

        /// <summary>
        /// The battle log recording all combat events.
        /// </summary>
        public BattleLog Log { get; private set; }

        /// <summary>
        /// A weapon-fire event, used by the view layer to draw a tracer/flash.
        /// </summary>
        public sealed class FireFlash
        {
            public double Time { get; set; }
            public bool FromInterceptor { get; set; }
            public bool Hit { get; set; }
        }

        /// <summary>
        /// Recent weapon fires (interceptor and UFO) with their simulation time.
        /// The view draws tracers for entries younger than a short window and
        /// prunes the rest.
        /// </summary>
        public List<FireFlash> Flashes { get; } = new List<FireFlash>();

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
            // Seed the interpolation baseline, otherwise the first running frame
            // interpolates from 0 (centre) and the craft visibly teleport.
            PrevDistance = MaxDistance;
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
            int maxRange = GetMaxWeaponRange(interceptor);

            switch (mode)
            {
                case TacticalMode.Standoff:
                    // Hold at the edge of weapon range: observe, minimal risk.
                    return maxRange > 0 ? maxRange : MaxDistance;

                case TacticalMode.Cautious:
                    // Fire from (near) maximum range.
                    return maxRange > 0 ? maxRange * 0.90 : MaxDistance;

                case TacticalMode.Standard:
                    // Balanced: close to three-quarters of maximum range.
                    return maxRange > 0 ? maxRange * 0.75 : MaxDistance;

                case TacticalMode.Aggressive:
                    // Close to point-blank for maximum damage output.
                    return 1000.0;

                case TacticalMode.Disengage:
                    return MaxDistance;

                default:
                    return MaxDistance;
            }
        }
    }
}
