using System;
using System.Collections.Generic;
using System.Linq;

using ProjectXenocide.Model.Battlescape.Combatants;

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// Strategic Engagement resolver: a headless, round-based combat simulation
    /// used in place of a tactical battlescape.
    /// </summary>
    /// <remarks>
    /// DESIGN (inspired by Master of Orion II's abstract ground combat and
    /// EU4's dice-plus-modifiers model):
    ///  - Units act in <b>initiative order</b> each round (Reactions + random).
    ///  - Each action rolls to hit: <c>clamp(Accuracy * AccuracyFactor, 5, 95)</c>.
    ///  - A hit does weapon damage (±variance) reduced by the target's front armor.
    ///  - The fight ends when a side is wiped out, or at <see cref="MaxRounds"/>
    ///    (stalemate -> the attacker withdraws, reported as Aborted).
    ///
    /// The simulation works on <see cref="CombatantProfile"/> snapshots, so the
    /// Monte-Carlo prediction (<see cref="Predict"/>) can run many times without
    /// touching the real game state; <see cref="Apply"/> writes the chosen outcome
    /// back onto the real combatants before the mission contract runs.
    /// </remarks>
    public static class EngagementResolver
    {
        /// <summary>Round limit; beyond this the attacker withdraws.</summary>
        public const int MaxRounds = 20;

        /// <summary>Damage used for unarmed units.</summary>
        public const int DefaultUnarmedDamage = 15;

        /// <summary>Default number of samples for the Monte-Carlo prediction.</summary>
        public const int DefaultSamples = 200;

        private const int HitFloor = 5;
        private const int HitCeiling = 95;
        private const double AccuracyFactor = 0.5;
        private const double DamageVariance = 0.2;
        private const double StalemateBand = 0.05;

        private sealed class Unit
        {
            public CombatantProfile P;
            public int Hp;
            public int Initiative;
            public bool Dead;
        }

        /// <summary>Run the simulation once.</summary>
        public static EngagementResult Simulate(
            IList<CombatantProfile> xcorp, IList<CombatantProfile> aliens, Random rng)
        {
            var xcorpUnits = MakeUnits(xcorp);
            var alienUnits = MakeUnits(aliens);
            var log = new EngagementLog();
            int round = 0;

            while ((round < MaxRounds)
                && (CountAlive(xcorpUnits) > 0)
                && (CountAlive(alienUnits) > 0))
            {
                ++round;
                log.Add(round, "Round " + round);

                List<Unit> order = xcorpUnits.Concat(alienUnits).Where(u => !u.Dead).ToList();
                foreach (Unit unit in order)
                {
                    unit.Initiative = unit.P.Reactions + rng.Next(0, 10);
                }
                order.Sort((a, b) => b.Initiative.CompareTo(a.Initiative));

                foreach (Unit shooter in order)
                {
                    if (shooter.Dead)
                    {
                        continue;
                    }

                    List<Unit> enemies = shooter.P.IsXCorp ? alienUnits : xcorpUnits;
                    Unit target = enemies.Where(e => !e.Dead)
                                         .OrderByDescending(e => e.P.Damage)
                                         .FirstOrDefault();
                    if (target == null)
                    {
                        break;
                    }
                    Fire(shooter, target, round, rng, log);
                }
            }

            int xAlive = CountAlive(xcorpUnits);
            int aAlive = CountAlive(alienUnits);
            BattleFinish finish = DetermineFinish(xcorpUnits, alienUnits, xAlive, aAlive, log, round);

            var units = new List<UnitOutcome>();
            foreach (Unit unit in xcorpUnits.Concat(alienUnits))
            {
                units.Add(new UnitOutcome
                {
                    Profile = unit.P,
                    RemainingHealth = Math.Max(0, unit.Hp),
                    Dead = unit.Dead,
                });
            }

            return new EngagementResult
            {
                Finish = finish,
                Rounds = round,
                Log = log,
                Units = units,
                XCorpKia = xcorpUnits.Count(u => u.Dead),
                XCorpWounded = xcorpUnits.Count(u => !u.Dead && (u.Hp < u.P.Health)),
                AlienKills = alienUnits.Count(u => u.Dead),
            };
        }

        /// <summary>
        /// Monte-Carlo estimate of the outcome, from the player's point of view.
        /// Uses its own RNG so it never disturbs the game's random stream.
        /// </summary>
        public static EngagementPrediction Predict(
            IList<CombatantProfile> xcorp, IList<CombatantProfile> aliens,
            int samples = DefaultSamples, int seed = 0)
        {
            if (samples <= 0)
            {
                samples = 1;
            }

            var rng = new Random(seed);
            int wins = 0;
            double kia = 0;
            double wounded = 0;
            double kills = 0;

            for (int i = 0; i < samples; ++i)
            {
                EngagementResult result = Simulate(xcorp, aliens, rng);
                if (result.Finish == BattleFinish.XCorpVictory)
                {
                    ++wins;
                }
                kia += result.XCorpKia;
                wounded += result.XCorpWounded;
                kills += result.AlienKills;
            }

            return new EngagementPrediction
            {
                Samples = samples,
                WinProbability = (double)wins / samples,
                ExpectedXCorpKia = kia / samples,
                ExpectedXCorpWounded = wounded / samples,
                ExpectedAlienKills = kills / samples,
            };
        }

        /// <summary>Write a resolved outcome back onto the real combatants.</summary>
        public static void Apply(EngagementResult result)
        {
            if (result?.Units == null)
            {
                return;
            }

            foreach (UnitOutcome outcome in result.Units)
            {
                Combatant combatant = outcome.Profile.Combatant;
                if (combatant == null)
                {
                    continue;
                }

                int health = combatant.Stats[Statistic.Health];
                if (outcome.Dead)
                {
                    combatant.Stats[Statistic.InjuryDamage] = health + 1;
                }
                else if (outcome.RemainingHealth < health)
                {
                    combatant.Stats[Statistic.InjuryDamage] = Math.Max(0, health - outcome.RemainingHealth);
                }
            }
        }

        private static void Fire(Unit shooter, Unit target, int round, Random rng, EngagementLog log)
        {
            int chance = Clamp((int)(shooter.P.Accuracy * AccuracyFactor), HitFloor, HitCeiling);
            if (rng.Next(100) >= chance)
            {
                return;
            }

            double variance = 1.0 + (((rng.NextDouble() * 2.0) - 1.0) * DamageVariance);
            int damage = ((int)Math.Round(shooter.P.Damage * variance)) - target.P.ArmorFront;
            if (damage < 0)
            {
                damage = 0;
            }

            target.Hp -= damage;

            if (target.Hp <= 0)
            {
                target.Dead = true;
                log.Add(round, "{0} kills {1}.", shooter.P.Name, target.P.Name);
            }
            else if (damage > 0)
            {
                log.Add(round, "{0} hits {1} for {2}.", shooter.P.Name, target.P.Name, damage);
            }
        }

        private static BattleFinish DetermineFinish(
            List<Unit> xcorpUnits, List<Unit> alienUnits, int xAlive, int aAlive,
            EngagementLog log, int round)
        {
            if ((xAlive == 0) && (aAlive == 0))
            {
                return BattleFinish.Aborted;
            }
            if (aAlive == 0)
            {
                log.Add(round, "All aliens neutralised.");
                return BattleFinish.XCorpVictory;
            }
            if (xAlive == 0)
            {
                log.Add(round, "X-Corp squad defeated.");
                return BattleFinish.AlienVictory;
            }

            // Round cap reached: the attacker withdraws unless clearly winning.
            double xFraction = HealthFraction(xcorpUnits);
            double aFraction = HealthFraction(alienUnits);
            log.Add(round, "Stalemate at round cap; engagement broken off.");
            if (Math.Abs(xFraction - aFraction) <= StalemateBand)
            {
                return BattleFinish.Aborted;
            }
            return (xFraction > aFraction) ? BattleFinish.XCorpVictory : BattleFinish.AlienVictory;
        }

        private static List<Unit> MakeUnits(IList<CombatantProfile> profiles)
        {
            var units = new List<Unit>();
            if (profiles == null)
            {
                return units;
            }
            foreach (CombatantProfile profile in profiles)
            {
                units.Add(new Unit { P = profile, Hp = profile.Health });
            }
            return units;
        }

        private static int CountAlive(List<Unit> units) => units.Count(u => !u.Dead);

        private static double HealthFraction(List<Unit> units)
        {
            int max = units.Sum(u => u.P.Health);
            if (max <= 0)
            {
                return 0;
            }
            return (double)units.Sum(u => Math.Max(0, u.Hp)) / max;
        }

        private static int Clamp(int value, int min, int max) => Math.Max(min, Math.Min(max, value));
    }
}
