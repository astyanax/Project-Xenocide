using System;
using System.Collections.Generic;
using System.Globalization;

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>A single line in the engagement log.</summary>
    public sealed class EngagementLogEntry
    {
        public int Round { get; init; }
        public string Text { get; init; }
    }

    /// <summary>Ordered log of what happened during an engagement.</summary>
    public sealed class EngagementLog
    {
        public IReadOnlyList<EngagementLogEntry> Entries => entries;

        public void Add(int round, string text)
        {
            entries.Add(new EngagementLogEntry { Round = round, Text = text });
        }

        public void Add(int round, string format, params object[] args)
        {
            entries.Add(new EngagementLogEntry { Round = round, Text = string.Format(CultureInfo.InvariantCulture, format, args) });
        }

        private readonly List<EngagementLogEntry> entries = new List<EngagementLogEntry>();
    }

    /// <summary>Final state of one unit in a resolved engagement.</summary>
    public sealed class UnitOutcome
    {
        public CombatantProfile Profile { get; init; }
        public int RemainingHealth { get; init; }
        public bool Dead { get; init; }
        public bool Unconscious { get; init; }
    }

    /// <summary>Result of running the engagement simulation once.</summary>
    public sealed class EngagementResult
    {
        public BattleFinish Finish { get; init; }
        public int Rounds { get; init; }
        public EngagementLog Log { get; init; } = new EngagementLog();

        public IReadOnlyList<UnitOutcome> Units { get; init; } = Array.Empty<UnitOutcome>();

        public int XCorpKia { get; init; }
        public int XCorpWounded { get; init; }
        public int AlienKills { get; init; }
    }

    /// <summary>Aggregate of many simulated engagements (the pre-battle prediction).</summary>
    public sealed class EngagementPrediction
    {
        public int Samples { get; init; }
        public double WinProbability { get; init; }
        public double ExpectedXCorpKia { get; init; }
        public double ExpectedXCorpWounded { get; init; }
        public double ExpectedAlienKills { get; init; }
    }
}
