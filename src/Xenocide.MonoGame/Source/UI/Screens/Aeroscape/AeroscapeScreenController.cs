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
* @file AeroscapeScreenController.cs
* @date Created: 2007/03/11
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

using System;

using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Geoscape.Vehicles;

namespace ProjectXenocide.UI.Screens
{
    public partial class AeroscapeScreen
    {
        /// <summary>
        /// Full real-time simulation engine for the aeroscape dogfight.
        /// Handles distance management, weapon cooldowns, attack resolution,
        /// UFO AI, and end-condition detection.
        /// </summary>
        /// <remarks>
        /// ARCHITECTURE: All game state mutations happen here. The Screen class
        /// calls Tick() each frame and reads AeroscapeState properties for display.
        ///
        /// SIMULATION LOOP (per tick, in simulated seconds):
        /// 1. Advance distance toward each interceptor's target distance
        /// 2. Advance weapon cooldowns for all interceptors
        /// 3. Each interceptor fires weapons that are off cooldown (if in range)
        /// 4. Advance UFO AI (distance adjustment, firing)
        /// 5. Check end conditions (destruction, escape, disengage, out of ammo)
        ///
        /// TIME MODEL: One simulated second per Tick(1.0). Screen multiplies by
        /// speed factor (1x or 2x) based on real elapsed time.
        /// </remarks>
        private class AeroscapeSimulation
        {
            /// <summary>
            /// Closing/retreat speed in meters per simulated second.
            /// Craft approach each other at a fraction of max speed.
            /// </summary>
            private const double ApproachSpeedFactor = 0.1;

            /// <summary>
            /// UFO AI closing speed factor.
            /// </summary>
            private const double UfoSpeedFactor = 0.08;

            /// <summary>
            /// Base UFO weapon cooldown in seconds.
            /// </summary>
            private const double UfoBaseFireCooldown = 4.0;

            /// <summary>
            /// Hull percentage threshold for UFO escape attempt.
            /// UFO starts fleeing when hull drops below this percentage.
            /// </summary>
            private const double UfoEscapeHullThreshold = 0.3;

            private readonly AeroscapeState state;

            /// <summary>
            /// Whether the dogfight has ended.
            /// </summary>
            public bool IsDogfightOver => state.Outcome != DogfightOutcome.InProgress;

            /// <summary>
            /// The simulation state (read by screen for display).
            /// </summary>
            public AeroscapeState State => state;

            /// <summary>
            /// Constructor.
            /// </summary>
            public AeroscapeSimulation(AeroscapeState state)
            {
                this.state = state;
            }

            /// <summary>
            /// Advance the simulation by the given number of simulated seconds.
            /// </summary>
            /// <param name="seconds">Simulated seconds to advance</param>
            public void Tick(double seconds)
            {
                if (IsDogfightOver)
                    return;

                state.ElapsedSeconds += seconds;

                // Step 1: Move interceptors toward their target distances
                MoveInterceptors(seconds);

                // Step 2: Update weapon cooldowns for all interceptors
                UpdateInterceptorCooldowns(seconds);

                // Step 3: Each active interceptor fires weapons
                FireInterceptorWeapons();

                // Step 4: UFO AI
                UpdateUfoAI(seconds);

                // Step 5: Check end conditions
                CheckEndConditions();
            }

            /// <summary>
            /// Set the tactical mode for a specific interceptor.
            /// </summary>
            public void SetTacticalMode(int interceptorIndex, TacticalMode mode)
            {
                if (interceptorIndex >= 0 && interceptorIndex < state.Interceptors.Count)
                {
                    state.Interceptors[interceptorIndex].Mode = mode;
                    state.Log.Record("Interceptor {0} set to {1}",
                        state.Interceptors[interceptorIndex].Aircraft.Name,
                        mode.ToString().ToUpper());
                }
            }

            /// <summary>
            /// Toggle a weapon pod on/off for an interceptor.
            /// </summary>
            public void ToggleWeapon(int interceptorIndex, int weaponIndex)
            {
                if (interceptorIndex < 0 || interceptorIndex >= state.Interceptors.Count)
                    return;

                var interceptor = state.Interceptors[interceptorIndex];
                if (weaponIndex == 0)
                {
                    interceptor.Weapon1Enabled = !interceptor.Weapon1Enabled;
                }
                else if (weaponIndex == 1)
                {
                    interceptor.Weapon2Enabled = !interceptor.Weapon2Enabled;
                }
            }

            /// <summary>
            /// Command an interceptor to disengage (retreat).
            /// </summary>
            public void DisengageInterceptor(int interceptorIndex)
            {
                if (interceptorIndex >= 0 && interceptorIndex < state.Interceptors.Count)
                {
                    var interceptor = state.Interceptors[interceptorIndex];
                    interceptor.IsActive = false;
                    state.Log.Record("{0} is disengaging", interceptor.Aircraft.Name);
                }
            }

            /// <summary>
            /// Cycle to the next active interceptor.
            /// </summary>
            public void SelectNextInterceptor()
            {
                if (state.Interceptors.Count <= 1)
                    return;

                int start = state.SelectedInterceptorIndex;
                for (int i = 1; i <= state.Interceptors.Count; i++)
                {
                    int idx = (start + i) % state.Interceptors.Count;
                    if (state.Interceptors[idx].IsActive)
                    {
                        state.SelectedInterceptorIndex = idx;
                        return;
                    }
                }
            }

            #region Distance Management

            private void MoveInterceptors(double seconds)
            {
                // Find the nearest active interceptor's speed
                double nearestSpeed = 0;
                foreach (var interceptor in state.Interceptors)
                {
                    if (!interceptor.IsActive)
                        continue;

                    double speed = interceptor.Aircraft.CraftItemInfo.MaxSpeed * ApproachSpeedFactor;
                    if (speed > nearestSpeed)
                        nearestSpeed = speed;
                }

                if (nearestSpeed <= 0)
                    return;

                // Determine the target distance from the nearest interceptor's tactical mode
                var nearest = GetNearestActiveInterceptor();
                if (nearest == null)
                    return;

                TacticalMode mode = GetEffectiveMode(nearest);
                double targetDistance = AeroscapeState.GetTargetDistance(mode, nearest);

                // Move distance toward target
                double delta = targetDistance - state.Distance;
                double maxMove = nearestSpeed * seconds;

                if (Math.Abs(delta) <= maxMove)
                {
                    state.Distance = targetDistance;
                }
                else
                {
                    state.Distance += Math.Sign(delta) * maxMove;
                }

                // Clamp distance
                state.Distance = Math.Max(100, Math.Min(AeroscapeState.MaxDistance, state.Distance));
            }

            private InterceptorState GetNearestActiveInterceptor()
            {
                InterceptorState nearest = null;
                double bestSpeed = 0;

                foreach (var interceptor in state.Interceptors)
                {
                    if (!interceptor.IsActive)
                        continue;

                    double speed = interceptor.Aircraft.CraftItemInfo.MaxSpeed;
                    if (speed > bestSpeed)
                    {
                        bestSpeed = speed;
                        nearest = interceptor;
                    }
                }
                return nearest;
            }

            private TacticalMode GetEffectiveMode(InterceptorState interceptor)
            {
                // Use the selected interceptor's mode for the shared distance
                var selected = state.SelectedInterceptor;
                if (selected != null)
                    return GetCurrentMode(selected);
                return TacticalMode.Standoff;
            }

            private TacticalMode GetCurrentMode(InterceptorState interceptor)
            {
                return interceptor.Mode;
            }

            #endregion

            #region Weapon Cooldowns

            private void UpdateInterceptorCooldowns(double seconds)
            {
                foreach (var interceptor in state.Interceptors)
                {
                    if (!interceptor.IsActive)
                        continue;

                    TacticalMode mode = GetCurrentMode(interceptor);
                    double multiplier = AeroscapeState.GetCooldownMultiplier(mode);
                    interceptor.UpdateCooldowns(seconds * multiplier);
                }
            }

            private void FireInterceptorWeapons()
            {
                foreach (var interceptor in state.Interceptors)
                {
                    if (!interceptor.IsActive)
                        continue;

                    int maxRange = AeroscapeState.GetMaxWeaponRange(interceptor);
                    if (state.Distance > maxRange)
                        continue;

                    // Fire weapon pod 1
                    if (interceptor.Weapon1Enabled && interceptor.W1FireCountdown <= 0)
                    {
                        FireWeaponPod(interceptor, 0);
                        double cooldown = GetWeaponCooldown(interceptor, 0);
                        double multiplier = AeroscapeState.GetCooldownMultiplier(GetCurrentMode(interceptor));
                        interceptor.W1FireCountdown = cooldown * multiplier;
                    }

                    // Fire weapon pod 2
                    if (interceptor.Weapon2Enabled && interceptor.W2FireCountdown <= 0)
                    {
                        FireWeaponPod(interceptor, 1);
                        double cooldown = GetWeaponCooldown(interceptor, 1);
                        double multiplier = AeroscapeState.GetCooldownMultiplier(GetCurrentMode(interceptor));
                        interceptor.W2FireCountdown = cooldown * multiplier;
                    }
                }
            }

            private void FireWeaponPod(InterceptorState interceptor, int podIndex)
            {
                if (podIndex >= interceptor.Aircraft.WeaponPods.Count)
                    return;

                var pod = interceptor.Aircraft.WeaponPods[podIndex];
                if (pod == null || !pod.HasAmmo)
                    return;

                AttackResult result = pod.Attack(state.Ufo, state.Log);

                switch (result)
                {
                    case AttackResult.OpponentCrashed:
                    case AttackResult.OpponentDestroyed:
                        state.Outcome = DogfightOutcome.AircraftVictory;
                        break;
                    case AttackResult.OutOfAmmo:
                        // Pod is out of ammo, but others might still fire
                        break;
                }
            }

            private double GetWeaponCooldown(InterceptorState interceptor, int podIndex)
            {
                if (podIndex >= interceptor.Aircraft.WeaponPods.Count)
                    return 5.0;

                var pod = interceptor.Aircraft.WeaponPods[podIndex];
                if (pod == null)
                    return 5.0;

                return pod.TimeToShoot;
            }

            #endregion

            #region UFO AI

            private void UfoFires()
            {
                if (!state.Ufo.IsArmed)
                    return;

                // Check if any interceptor is in weapon range
                bool anyInRange = false;
                foreach (var interceptor in state.Interceptors)
                {
                    if (!interceptor.IsActive)
                        continue;

                    int maxRange = state.GetUfoMaxWeaponRange();
                    if (state.Distance <= maxRange)
                    {
                        anyInRange = true;
                        break;
                    }
                }

                if (!anyInRange)
                    return;

                // Fire at the nearest interceptor
                var target = GetNearestActiveInterceptor();
                if (target == null)
                    return;

                AttackResult result = state.Ufo.Attack(target.Aircraft, state.Log);
                switch (result)
                {
                    case AttackResult.OpponentCrashed:
                    case AttackResult.OpponentDestroyed:
                        target.IsActive = false;
                        state.Log.Record("{0} has been {1}!",
                            target.Aircraft.Name,
                            result == AttackResult.OpponentDestroyed ? "destroyed" : "crashed");
                        break;
                }
            }

            private void UpdateUfoAI(double seconds)
            {
                // UFO tries to maintain optimal firing distance
                double ufoSpeed = state.Ufo.CraftItemInfo.MaxSpeed * UfoSpeedFactor;
                double targetDelta = state.UfoPreferredDistance - state.Distance;
                double maxMove = ufoSpeed * seconds;

                if (Math.Abs(targetDelta) <= maxMove)
                {
                    state.Distance += targetDelta;
                }
                else
                {
                    state.Distance += Math.Sign(targetDelta) * maxMove;
                }

                state.Distance = Math.Max(100, Math.Min(AeroscapeState.MaxDistance, state.Distance));

                // Update fire cooldown
                state.UfoFireCountdown -= seconds;
                if (state.UfoFireCountdown <= 0)
                {
                    UfoFires();
                    state.UfoFireCountdown = UfoBaseFireCooldown;
                }

                // Update escape countdown based on hull damage
                double hullPercent = (double)(state.Ufo.MaxDamage - state.Ufo.HullDamage) / state.Ufo.MaxDamage;
                if (hullPercent < UfoEscapeHullThreshold)
                {
                    state.UfoEscapeCountdown -= seconds;
                }
            }

            #endregion

            #region End Conditions

            private void CheckEndConditions()
            {
                // UFO destroyed
                if (state.Ufo.IsDestroyed)
                {
                    state.Outcome = DogfightOutcome.AircraftVictory;
                    return;
                }

                // UFO crashed
                if (state.Ufo.IsCrashed)
                {
                    state.Outcome = DogfightOutcome.AircraftVictory;
                    return;
                }

                // UFO escaped (countdown reached 0)
                if (state.UfoEscapeCountdown <= 0)
                {
                    state.Outcome = DogfightOutcome.UFOEscaped;
                    state.Log.Record("{0} has fled the engagement", state.Ufo.Name);
                    return;
                }

                // UFO outruns interceptors (distance exceeds max and no interceptor can catch up)
                if (state.Distance >= AeroscapeState.MaxDistance)
                {
                    // Check if any interceptor can still close
                    bool anyCanCatch = false;
                    foreach (var interceptor in state.Interceptors)
                    {
                        if (interceptor.IsActive)
                        {
                            TacticalMode mode = GetCurrentMode(interceptor);
                            if (mode != TacticalMode.Disengage && mode != TacticalMode.Standoff)
                            {
                                anyCanCatch = true;
                                break;
                            }
                        }
                    }
                    if (!anyCanCatch)
                    {
                        state.Outcome = DogfightOutcome.UFOEscaped;
                        state.Log.Record("{0} has escaped", state.Ufo.Name);
                        return;
                    }
                }

                // All interceptors inactive (disengaged or destroyed)
                if (state.AllInterceptorsInactive)
                {
                    state.Outcome = DogfightOutcome.AircraftRetreated;
                    state.Log.Record("All interceptors have disengaged");
                    return;
                }

                // All interceptors out of ammo
                if (state.AllInterceptorsOutofAmmo)
                {
                    state.Outcome = DogfightOutcome.AircraftRetreated;
                    state.Log.Record("All interceptors are out of ammunition");
                    return;
                }

                // All active interceptors disengaged
                bool anyActive = false;
                foreach (var interceptor in state.Interceptors)
                {
                    if (interceptor.IsActive)
                    {
                        anyActive = true;
                        break;
                    }
                }
                if (!anyActive)
                {
                    state.Outcome = DogfightOutcome.AircraftRetreated;
                    return;
                }
            }

            #endregion
        }
    }
}
