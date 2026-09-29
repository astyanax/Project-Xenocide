namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>Human-readable text for engagement concepts, shared by the UI.</summary>
    public static class EngagementText
    {
        /// <summary>Player-facing label for an engagement outcome.</summary>
        public static string Finish(BattleFinish finish)
        {
            switch (finish)
            {
                case BattleFinish.XCorpVictory: return "X-Corp Victory";
                case BattleFinish.AlienVictory: return "Alien Victory";
                case BattleFinish.Aborted: return "Disengaged";
                default: return "Unknown";
            }
        }
    }
}
