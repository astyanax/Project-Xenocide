using System;
using System.Collections.Generic;
using System.Globalization;

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>A single line in the engagement log.</summary>
    public sealed class EngagementLogEntry
    {
        /// <summary>Round the event happened in (0 = pre-battle).</summary>
        public int Round { get; init; }

        /// <summary>What happened.</summary>
        public string Text { get; init; }
    }

    /// <summary>Ordered log of what happened during an engagement.</summary>
    public sealed class EngagementLog
    {
        /// <summary>The events, in the order they occurred.</summary>
        public IReadOnlyList<EngagementLogEntry> Entries => entries;

        /// <summary>Append a literal line.</summary>
        public void Add(int round, string text)
        {
            entries.Add(new EngagementLogEntry { Round = round, Text = text });
        }

        /// <summary>Append a formatted line.</summary>
        public void Add(int round, string format, params object[] args)
        {
            entries.Add(new EngagementLogEntry { Round = round, Text = string.Format(CultureInfo.InvariantCulture, format, args) });
        }

        private readonly List<EngagementLogEntry> entries = new List<EngagementLogEntry>();
    }

    /// <summary>Final state of one unit in a resolved engagement.</summary>
    public sealed class UnitOutcome
    {
        /// <summary>The unit this outcome belongs to.</summary>
        public CombatantProfile Profile { get; init; }

        /// <summary>Hit points remaining when the engagement ended.</summary>
        public int RemainingHealth { get; init; }

        /// <summary>Whether the unit was killed.</summary>
        public bool Dead { get; init; }

        /// <summary>Whether the unit was knocked out (stunned) rather than killed.</summary>
        // TODO: set once the resolver models stun/knockout (for live captures).
        public bool Unconscious { get; init; }
    }

    /// <summary>Result of running the engagement simulation once.</summary>
    public sealed class EngagementResult
    {
        /// <summary>How the engagement ended.</summary>
        public BattleFinish Finish { get; init; }

        /// <summary>Number of rounds fought.</summary>
        public int Rounds { get; init; }

        /// <summary>Round-by-round log of the engagement.</summary>
        public EngagementLog Log { get; init; } = new EngagementLog();

        /// <summary>Final state of every unit that took part.</summary>
        public IReadOnlyList<UnitOutcome> Units { get; init; } = Array.Empty<UnitOutcome>();

        /// <summary>X-Corp soldiers killed.</summary>
        public int XCorpKia { get; init; }

        /// <summary>X-Corp soldiers wounded (lost health) but alive.</summary>
        public int XCorpWounded { get; init; }

        /// <summary>Aliens killed.</summary>
        public int AlienKills { get; init; }
    }

    /// <summary>Aggregate of many simulated engagements (the pre-battle prediction).</summary>
    public sealed class EngagementPrediction
    {
        /// <summary>Number of simulated engagements aggregated.</summary>
        // TODO: shown in the UI once the prediction screen exposes its confidence.
        public int Samples { get; init; }

        /// <summary>Fraction of simulations X-Corp won (0..1).</summary>
        public double WinProbability { get; init; }

        /// <summary>Average X-Corp soldiers killed per engagement.</summary>
        public double ExpectedXCorpKia { get; init; }

        /// <summary>Average X-Corp soldiers wounded per engagement.</summary>
        public double ExpectedXCorpWounded { get; init; }

        /// <summary>Average aliens killed per engagement.</summary>
        public double ExpectedAlienKills { get; init; }
    }
}
