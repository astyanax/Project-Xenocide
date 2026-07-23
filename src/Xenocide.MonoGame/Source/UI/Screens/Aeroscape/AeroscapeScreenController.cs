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

using NLog;

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
        /// 2. Update weapon cooldowns for all interceptors
        /// 3. Each interceptor fires weapons that are off cooldown (if in range)
        /// 4. Advance UFO AI (distance adjustment, firing)
        /// 5. Check end conditions (destruction, escape, disengage, out of ammo)
        ///
        /// TIME MODEL: One simulated second per Tick(1.0). Screen multiplies by
        /// speed factor (1x or 2x) based on real elapsed time.
        ///
        /// COOLDOWN FIX: We manage our own per-interceptor cooldowns in
        /// InterceptorState and bypass WeaponPod.IsCycling() by calling
        /// Shoot() directly when the simulation cooldown expires. We also
        /// advance BattleLog.Now to stay in sync with simulation time.
        /// </remarks>
        private class AeroscapeSimulation
        {
            private static readonly Logger Log = LogManager.GetLogger("Aeroscape");

            /// <summary>
            /// Closing/retreat speed factor for interceptors (fraction of max speed).
            /// </summary>
            private const double ApproachSpeedFactor = 0.5;

            /// <summary>
            /// Speed factor for disengaging interceptors (full speed retreat).
            /// </summary>
            private const double DisengageSpeedFactor = 1.0;

            /// <summary>
            /// UFO AI closing speed factor.
            /// </summary>
            private const double UfoSpeedFactor = 0.4;

            /// <summary>
            /// Base UFO weapon cooldown in seconds.
            /// </summary>
            private const double UfoBaseFireCooldown = 4.0;

            /// <summary>
            /// Hull percentage threshold for UFO escape attempt.
            /// Non-aggressive UFOs start fleeing when hull drops below this percentage.
            /// </summary>
            private const double UfoEscapeHullThreshold = 0.5;

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

                Log.Debug("Tick {0:F1}s, distance={1:F0}m, interceptors={2}",
                    state.ElapsedSeconds + seconds, state.Distance, ActiveInterceptorCount);

                state.ElapsedSeconds += seconds;

                // Keep BattleLog time in sync with simulation time
                state.Log.UpdateTime(seconds);

                // Store previous distance before updating
                state.PrevDistance = state.Distance;

                // Step 1: Move interceptors toward their target distances
                MoveInterceptors(seconds);

                // Step 2: Update weapon cooldowns for all interceptors
                UpdateInterceptorCooldowns(seconds);

                // Step 3: Each active interceptor fires weapons
                FireInterceptorWeapons();

                // Step 4: Consume fuel for all active interceptors
                ConsumeFuel(seconds);

                // Step 5: UFO AI
                UpdateUfoAI(seconds);

                // Step 6: Check end conditions
                CheckEndConditions();
            }

            private int ActiveInterceptorCount
            {
                get
                {
                    int count = 0;
                    foreach (var s in state.Interceptors)
                        if (s.IsActive) count++;
                    return count;
                }
            }

            /// <summary>
            /// Set the tactical mode for a specific interceptor.
            /// </summary>
            public void SetTacticalMode(int interceptorIndex, TacticalMode mode)
            {
                if (interceptorIndex >= 0 && interceptorIndex < state.Interceptors.Count)
                {
                    var interceptor = state.Interceptors[interceptorIndex];
                    interceptor.Mode = mode;
                    Log.Info("{0} set to {1}", interceptor.Aircraft.Name, mode.ToString().ToUpperInvariant());
                    state.Log.Record("Interceptor {0} set to {1}",
                        interceptor.Aircraft.Name, mode.ToString().ToUpperInvariant());
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
                    interceptor.Weapon1Enabled = !interceptor.Weapon1Enabled;
                else if (weaponIndex == 1)
                    interceptor.Weapon2Enabled = !interceptor.Weapon2Enabled;
            }

            /// <summary>
            /// Command an interceptor to disengage (retreat then deactivate).
            /// </summary>
            public void DisengageInterceptor(int interceptorIndex)
            {
                if (interceptorIndex >= 0 && interceptorIndex < state.Interceptors.Count)
                {
                    var interceptor = state.Interceptors[interceptorIndex];
                    interceptor.Mode = TacticalMode.Disengage;
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
                // Process each active interceptor's distance movement
                foreach (var interceptor in state.Interceptors)
                {
                    if (!interceptor.IsActive)
                        continue;

                    TacticalMode mode = interceptor.Mode;

                    if (mode == TacticalMode.Disengage)
                    {
                        // Disengaging interceptor: full speed retreat
                        double speed = interceptor.Aircraft.CraftItemInfo.MaxSpeed * DisengageSpeedFactor;
                        double targetDistance = AeroscapeState.MaxDistance;
                        double delta = targetDistance - state.Distance;
                        double maxMove = speed * seconds;

                        // Complete disengage when beyond UFO weapon range
                        int ufoRange = state.GetUfoMaxWeaponRange();
                        double escapeDistance = Math.Max(ufoRange * 1.5, AeroscapeState.MaxDistance * 0.5);

                        if (state.Distance >= escapeDistance || Math.Abs(delta) <= maxMove)
                        {
                            state.Distance = Math.Min(targetDistance, state.Distance + maxMove);
                            interceptor.IsActive = false;
                            Log.Debug("{0} disengaged at distance {1:F0}m", interceptor.Aircraft.Name, state.Distance);
                            state.Log.Record("{0} has disengaged", interceptor.Aircraft.Name);
                        }
                        else
                        {
                            state.Distance += maxMove;
                            Log.Debug("{0} retreating, distance +{1:F0}m = {2:F0}m",
                                interceptor.Aircraft.Name, maxMove, state.Distance);
                        }
                    }
                    else
                    {
                        // Active interceptor: move toward target distance
                        double speed = interceptor.Aircraft.CraftItemInfo.MaxSpeed * ApproachSpeedFactor;
                        double targetDistance = AeroscapeState.GetTargetDistance(mode, interceptor);
                        double delta = targetDistance - state.Distance;
                        double maxMove = speed * seconds;

                        if (Math.Abs(delta) <= maxMove)
                        {
                            state.Distance = targetDistance;
                            Log.Debug("{0} reached target distance {1:F0}m ({2})",
                                interceptor.Aircraft.Name, targetDistance, mode);
                        }
                        else
                        {
                            state.Distance += Math.Sign(delta) * maxMove;
                            Log.Debug("{0} moving {1} target, dist {2:F0}m -> {3:F0}m ({4}, target {5:F0}m)",
                                interceptor.Aircraft.Name,
                                Math.Sign(delta) > 0 ? "toward" : "away from",
                                state.Distance - Math.Sign(delta) * maxMove,
                                state.Distance, mode, targetDistance);
                        }
                    }
                }

                // Clamp distance
                double beforeClamp = state.Distance;
                state.Distance = Math.Max(100, Math.Min(AeroscapeState.MaxDistance, state.Distance));
                if (state.Distance != beforeClamp)
                    Log.Debug("Distance clamped: {0:F0}m -> {1:F0}m", beforeClamp, state.Distance);
            }

            #endregion

            #region Weapon Cooldowns and Firing

            private void UpdateInterceptorCooldowns(double seconds)
            {
                foreach (var interceptor in state.Interceptors)
                {
                    if (!interceptor.IsActive)
                        continue;

                    TacticalMode mode = interceptor.Mode;
                    double multiplier = AeroscapeState.GetCooldownMultiplier(mode);
                    interceptor.UpdateCooldowns(seconds * multiplier);
                }
            }

            private void FireInterceptorWeapons()
            {
                foreach (var interceptor in state.Interceptors)
                {
                    if (!interceptor.IsActive || interceptor.Mode == TacticalMode.Disengage)
                        continue;

                    int maxRange = AeroscapeState.GetMaxWeaponRange(interceptor);
                    if (maxRange <= 0)
                    {
                        Log.Debug("{0}: no weapons, skipping fire", interceptor.Aircraft.Name);
                        continue;
                    }
                    if (state.Distance > maxRange)
                    {
                        Log.Debug("{0}: distance {1:F0}m > max range {2}m, cannot fire",
                            interceptor.Aircraft.Name, state.Distance, maxRange);
                        continue;
                    }

                    Log.Debug("{0}: distance {1:F0}m <= max range {2}m, checking weapons",
                        interceptor.Aircraft.Name, state.Distance, maxRange);

                    // Fire weapon pod 1 if off cooldown
                    if (interceptor.Weapon1Enabled && interceptor.W1FireCountdown <= 0)
                    {
                        FireWeaponPod(interceptor, 0);
                        double cooldown = GetWeaponCooldown(interceptor, 0);
                        double multiplier = AeroscapeState.GetCooldownMultiplier(interceptor.Mode);
                        interceptor.W1FireCountdown = cooldown * multiplier;
                        Log.Debug("{0}: W1 fired, cooldown {1:F1}s (x{2:F1}) = {3:F1}s",
                            interceptor.Aircraft.Name, cooldown, multiplier, interceptor.W1FireCountdown);
                    }
                    else if (interceptor.Weapon1Enabled)
                    {
                        Log.Debug("{0}: W1 on cooldown ({1:F1}s remaining)",
                            interceptor.Aircraft.Name, interceptor.W1FireCountdown);
                    }

                    // Fire weapon pod 2 if off cooldown
                    if (interceptor.Weapon2Enabled && interceptor.W2FireCountdown <= 0)
                    {
                        FireWeaponPod(interceptor, 1);
                        double cooldown = GetWeaponCooldown(interceptor, 1);
                        double multiplier = AeroscapeState.GetCooldownMultiplier(interceptor.Mode);
                        interceptor.W2FireCountdown = cooldown * multiplier;
                        Log.Debug("{0}: W2 fired, cooldown {1:F1}s (x{2:F1}) = {3:F1}s",
                            interceptor.Aircraft.Name, cooldown, multiplier, interceptor.W2FireCountdown);
                    }
                    else if (interceptor.Weapon2Enabled)
                    {
                        Log.Debug("{0}: W2 on cooldown ({1:F1}s remaining)",
                            interceptor.Aircraft.Name, interceptor.W2FireCountdown);
                    }
                }
            }

            private void FireWeaponPod(InterceptorState interceptor, int podIndex)
            {
                if (podIndex >= interceptor.Aircraft.WeaponPods.Count)
                    return;

                var pod = interceptor.Aircraft.WeaponPods[podIndex];
                if (pod == null || !pod.HasAmmo)
                {
                    Log.Debug("{0}: W{1} has no ammo", interceptor.Aircraft.Name, podIndex + 1);
                    return;
                }

                // Log accuracy check details
                double accuracy = pod.Accuracy;
                int damage = pod.WeaponDamage;
                Log.Debug("{0}: W{1} ({2}) firing, acc={3:F1}%, dmg={4}, dist={5:F0}m",
                    interceptor.Aircraft.Name, podIndex + 1, pod.Name, accuracy * 100, damage, state.Distance);

                // Call Shoot() directly to bypass IsCycling() check.
                AttackResult result = pod.Shoot(state.Ufo, state.Log);

                Log.Debug("{0}: W{1} result={2}", interceptor.Aircraft.Name, podIndex + 1, result);

                switch (result)
                {
                    case AttackResult.OpponentCrashed:
                        Log.Info("{0} CRASHED {1} with {2}!", interceptor.Aircraft.Name, state.Ufo.Name, pod.Name);
                        state.Outcome = DogfightOutcome.AircraftVictory;
                        break;
                    case AttackResult.OpponentDestroyed:
                        Log.Info("{0} DESTROYED {1} with {2}!", interceptor.Aircraft.Name, state.Ufo.Name, pod.Name);
                        state.Outcome = DogfightOutcome.AircraftVictory;
                        break;
                    case AttackResult.Nothing:
                        Log.Debug("{0}: W{1} hit but no kill, UFO hull {2:F0}% (health {3:F0}/{4:F0})",
                            interceptor.Aircraft.Name, podIndex + 1,
                            state.Ufo.HullPercent, state.Ufo.HullCapacity - state.Ufo.HullDamage, state.Ufo.HullCapacity);
                        break;
                }
            }

            private static double GetWeaponCooldown(InterceptorState interceptor, int podIndex)
            {
                if (podIndex >= interceptor.Aircraft.WeaponPods.Count)
                    return 5.0;

                var pod = interceptor.Aircraft.WeaponPods[podIndex];
                if (pod == null)
                    return 5.0;

                return pod.TimeToShoot;
            }

            #endregion

            #region Fuel Consumption

            private void ConsumeFuel(double seconds)
            {
                foreach (var interceptor in state.Interceptors)
                {
                    if (!interceptor.IsActive)
                        continue;

                    // Fuel consumption in units/hour, convert seconds to hours
                    double hours = seconds / 3600.0;
                    interceptor.Aircraft.ConsumeFuel(hours * 3600.0 * 1000.0);
                }
            }

            #endregion

            #region UFO AI

            private void UfoFires()
            {
                if (!state.Ufo.IsArmed)
                {
                    Log.Debug("UFO {0}: not armed, skipping fire", state.Ufo.Name);
                    return;
                }

                // Check if any interceptor is in weapon range
                int ufoMaxRange = state.GetUfoMaxWeaponRange();
                if (ufoMaxRange <= 0 || state.Distance > ufoMaxRange)
                {
                    Log.Debug("UFO {0}: distance {1:F0}m > weapon range {2}m, cannot fire",
                        state.Ufo.Name, state.Distance, ufoMaxRange);
                    return;
                }

                // Find the fastest active interceptor as target (closest proxy)
                var target = GetFastestActiveInterceptor();
                if (target == null)
                {
                    Log.Debug("UFO {0}: no active interceptor targets", state.Ufo.Name);
                    return;
                }

                Log.Debug("UFO {0} firing at {1} (dist={2:F0}m, range={3}m)",
                    state.Ufo.Name, target.Aircraft.Name, state.Distance, ufoMaxRange);

                AttackResult result = state.Ufo.Attack(target.Aircraft, state.Log);
                Log.Debug("UFO {0} attack on {1}: {2}", state.Ufo.Name, target.Aircraft.Name, result);

                switch (result)
                {
                    case AttackResult.OpponentCrashed:
                        Log.Info("{0} CRASHED {1}!", state.Ufo.Name, target.Aircraft.Name);
                        target.IsActive = false;
                        break;
                    case AttackResult.OpponentDestroyed:
                        Log.Info("{0} DESTROYED {1}!", state.Ufo.Name, target.Aircraft.Name);
                        target.IsActive = false;
                        break;
                    case AttackResult.Nothing:
                        Log.Debug("UFO {0} hit {1}, hull {2:F0}% (health {3:F0}/{4:F0})",
                            state.Ufo.Name, target.Aircraft.Name,
                            target.Aircraft.HullPercent, target.Aircraft.HullCapacity - target.Aircraft.HullDamage, target.Aircraft.HullCapacity);
                        break;
                }
            }

            private void UpdateUfoAI(double seconds)
            {
                double previousDistance = state.Distance;

                // Check if all remaining active interceptors are disengaging
                bool allDisengaging = true;
                bool anyActive = false;
                foreach (var s in state.Interceptors)
                {
                    if (s.IsActive)
                    {
                        anyActive = true;
                        if (s.Mode != TacticalMode.Disengage)
                        {
                            allDisengaging = false;
                            break;
                        }
                    }
                }

                // If all interceptors are disengaging, UFO doesn't pursue
                if (anyActive && allDisengaging)
                {
                    Log.Debug("UFO AI: all interceptors disengaging, not adjusting distance");
                }
                else if (state.UfoIsAggressive)
                {
                    // Aggressive UFO: actively close distance to hunt interceptors
                    double ufoSpeed = state.Ufo.CraftItemInfo.MaxSpeed * UfoSpeedFactor;
                    double ufoWeaponRange = state.GetUfoMaxWeaponRange();
                    double huntDistance = ufoWeaponRange > 0 ? ufoWeaponRange * 0.8 : 5000.0;
                    double targetDelta = huntDistance - state.Distance;
                    double maxMove = ufoSpeed * seconds;

                    if (Math.Abs(targetDelta) <= maxMove)
                        state.Distance += targetDelta;
                    else
                        state.Distance += Math.Sign(targetDelta) * maxMove;

                    Log.Debug("UFO AI (aggressive): wants {0:F0}m, delta={1:F0}m, maxMove={2:F0}m, dist {3:F0}m -> {4:F0}m",
                        huntDistance, targetDelta, maxMove, previousDistance, state.Distance);
                }
                else
                {
                    // Non-aggressive UFO: try to maintain preferred distance
                    double ufoSpeed = state.Ufo.CraftItemInfo.MaxSpeed * UfoSpeedFactor;
                    double targetDelta = state.UfoPreferredDistance - state.Distance;
                    double maxMove = ufoSpeed * seconds;

                    if (Math.Abs(targetDelta) <= maxMove)
                        state.Distance += targetDelta;
                    else
                        state.Distance += Math.Sign(targetDelta) * maxMove;

                    Log.Debug("UFO AI (passive): wants {0:F0}m, delta={1:F0}m, maxMove={2:F0}m, dist {3:F0}m -> {4:F0}m",
                        state.UfoPreferredDistance, targetDelta, maxMove, previousDistance, state.Distance);
                }

                state.Distance = Math.Max(100, Math.Min(AeroscapeState.MaxDistance, state.Distance));

                // Update fire cooldown
                state.UfoFireCountdown -= seconds;
                if (state.UfoFireCountdown <= 0)
                {
                    Log.Debug("UFO {0}: fire cooldown expired, firing", state.Ufo.Name);
                    UfoFires();
                    state.UfoFireCountdown = UfoBaseFireCooldown;
                    Log.Debug("UFO {0}: fire cooldown reset to {1:F1}s", state.Ufo.Name, UfoBaseFireCooldown);
                }

                // Update escape countdown
                double hullPercent = (double)(state.Ufo.HullCapacity - state.Ufo.HullDamage) / state.Ufo.HullCapacity;
                double escapeDecay = 0;
                if (state.UfoIsAggressive)
                {
                    if (hullPercent < 0.15)
                    {
                        escapeDecay = seconds * 0.5;
                        state.UfoEscapeCountdown -= escapeDecay;
                    }
                    else
                    {
                        escapeDecay = seconds * 0.01;
                        state.UfoEscapeCountdown -= escapeDecay;
                    }
                }
                else
                {
                    if (hullPercent < UfoEscapeHullThreshold)
                    {
                        escapeDecay = seconds * 2.0;
                        state.UfoEscapeCountdown -= escapeDecay;
                    }
                    else
                    {
                        escapeDecay = seconds * 0.05;
                        state.UfoEscapeCountdown -= escapeDecay;
                    }
                }

                if (state.UfoEscapeCountdown <= 30 && state.UfoEscapeCountdown > 0)
                {
                    Log.Debug("UFO {0}: escape countdown {1:F1}s (hull {2:F0}%, decay {3:F2}/s)",
                        state.Ufo.Name, state.UfoEscapeCountdown, hullPercent * 100, escapeDecay / seconds);
                }
            }

            private InterceptorState GetFastestActiveInterceptor()
            {
                InterceptorState fastest = null;
                double bestSpeed = 0;

                foreach (var interceptor in state.Interceptors)
                {
                    if (!interceptor.IsActive)
                        continue;

                    double speed = interceptor.Aircraft.CraftItemInfo.MaxSpeed;
                    if (speed > bestSpeed)
                    {
                        bestSpeed = speed;
                        fastest = interceptor;
                    }
                }
                return fastest;
            }

            #endregion

            #region End Conditions

            private void CheckEndConditions()
            {
                // UFO destroyed
                if (state.Ufo.IsDestroyed)
                {
                    Log.Info("DOGFIGHT ENDED: {0} DESTROYED by {1}. Elapsed={2:F0}s",
                        state.Ufo.Name, GetActiveInterceptorNames(), state.ElapsedSeconds);
                    state.Outcome = DogfightOutcome.AircraftVictory;
                    return;
                }

                // UFO crashed
                if (state.Ufo.IsCrashed)
                {
                    Log.Info("DOGFIGHT ENDED: {0} CRASHED by {1}. Elapsed={2:F0}s",
                        state.Ufo.Name, GetActiveInterceptorNames(), state.ElapsedSeconds);
                    state.Outcome = DogfightOutcome.AircraftVictory;
                    return;
                }

                // UFO escaped (countdown reached 0)
                if (state.UfoEscapeCountdown <= 0)
                {
                    Log.Info("DOGFIGHT ENDED: {0} ESCAPED. Elapsed={1:F0}s",
                        state.Ufo.Name, state.ElapsedSeconds);
                    state.Outcome = DogfightOutcome.UFOEscaped;
                    state.Log.Record("{0} has fled the engagement", state.Ufo.Name);
                    return;
                }

                // Any interceptor destroyed by UFO -> AircraftDestroyed
                bool anyDestroyed = false;
                foreach (var interceptor in state.Interceptors)
                {
                    if (!interceptor.IsActive && interceptor.Aircraft.IsDestroyed)
                    {
                        anyDestroyed = true;
                        break;
                    }
                }

                // All interceptors inactive (disengaged or destroyed)
                if (state.AllInterceptorsInactive)
                {
                    if (anyDestroyed)
                    {
                        Log.Info("DOGFIGHT ENDED: {0} destroyed by {1}. Elapsed={2:F0}s",
                            GetActiveInterceptorNames(), state.Ufo.Name, state.ElapsedSeconds);
                    }
                    else
                    {
                        Log.Info("DOGFIGHT ENDED: all interceptors disengaged from {0}. Elapsed={1:F0}s",
                            state.Ufo.Name, state.ElapsedSeconds);
                    }
                    state.Outcome = anyDestroyed
                        ? DogfightOutcome.AircraftDestroyed
                        : DogfightOutcome.AircraftRetreated;
                    state.Log.Record(anyDestroyed
                        ? "All interceptors have been destroyed"
                        : "All interceptors have disengaged");
                    return;
                }

                // All active interceptors out of usable weapons
                if (state.AllInterceptorsOutofAmmo)
                {
                    Log.Info("DOGFIGHT ENDED: all interceptors out of ammo vs {0}. Elapsed={1:F0}s",
                        state.Ufo.Name, state.ElapsedSeconds);
                    state.Outcome = DogfightOutcome.AircraftRetreated;
                    state.Log.Record("All interceptors are out of ammunition");
                    return;
                }
            }

            private string GetActiveInterceptorNames()
            {
                string names = "";
                foreach (var s in state.Interceptors)
                {
                    if (s.IsActive)
                    {
                        if (names.Length > 0) names += ", ";
                        names += s.Aircraft.Name;
                    }
                }
                return names.Length > 0 ? names : "(none)";
            }

            #endregion
        }
    }
}
