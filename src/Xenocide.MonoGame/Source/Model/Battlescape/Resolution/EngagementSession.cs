using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// Holds one ground engagement: the forces involved, the pre-battle odds
    /// prediction, and (once committed) the resolved result.  This is the model
    /// behind the Engagement screen.
    /// </summary>
    public sealed class EngagementSession
    {
        public Mission Mission { get; }

        /// <summary>The alien force, kept so the mission's result contract can read it.</summary>
        public Team AlienTeam { get; }

        public IReadOnlyList<CombatantProfile> XCorp { get; }
        public IReadOnlyList<CombatantProfile> Aliens { get; }

        /// <summary>Monte-Carlo estimate of the outcome, computed when the session opens.</summary>
        public EngagementPrediction Prediction { get; }

        /// <summary>Set once <see cref="Engage"/> has been called.</summary>
        public EngagementResult Result { get; private set; }

        private readonly Random rng = new Random();

        public EngagementSession(Mission mission, int predictionSamples = EngagementResolver.DefaultSamples)
        {
            Mission = mission;
            AlienTeam = mission.CreateAlienTeam();
            Team xcorpTeam = mission.CreateXCorpTeam();

            XCorp = xcorpTeam.Combatants
                .Select(c => CombatantProfile.Build(c, true, EngagementResolver.DefaultUnarmedDamage))
                .ToList();
            Aliens = AlienTeam.Combatants
                .Select(c => CombatantProfile.Build(c, false, EngagementResolver.DefaultUnarmedDamage))
                .ToList();

            Prediction = EngagementResolver.Predict(XCorp, Aliens, predictionSamples, seed: Environment.TickCount);
        }

        /// <summary>Resolve the engagement for real and apply the casualties.</summary>
        /// <remarks>Idempotent: a second call returns the already-resolved result.</remarks>
        public EngagementResult Engage()
        {
            if (Result != null)
            {
                return Result;
            }

            Result = EngagementResolver.Simulate(XCorp, Aliens, rng);
            EngagementResolver.Apply(Result);
            Mission.OnFinish(Result.Finish, AlienTeam);

            // fatal wounds are healed after the mission (formerly done by Battle)
            foreach (CombatantProfile profile in XCorp.Concat(Aliens))
            {
                profile.Combatant?.PostMissionCleanup();
            }

            return Result;
        }

        /// <summary>Abandon the engagement without fighting.</summary>
        public void Withdraw()
        {
            Mission.DontStart();
        }
    }
}
