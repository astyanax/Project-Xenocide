using System;

using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.UI.Screens;

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// TEMPORARY headless battle resolver.
    /// </summary>
    /// <remarks>
    /// Phase 0 of the Strategic Engagement refactor: the 3D battlescape has been
    /// removed, so ground missions are resolved headlessly.  This simply builds
    /// the mission's forces, picks a winner by numbers, and hands the result to
    /// the mission's existing <see cref="Mission.OnFinish"/> contract so the
    /// report/geoscape side effects keep working.  It is replaced by the
    /// Strategic Engagement resolver in the next phase.
    /// </remarks>
    public static class AutoResolveMission
    {
        public static void Resolve(Mission mission)
        {
            var battle = new Battle(mission);

            int xcorpCount = battle.Teams[Team.XCorp].Combatants.Count;
            int alienCount = battle.Teams[Team.Aliens].Combatants.Count;
            BattleFinish finish = (xcorpCount >= alienCount)
                ? BattleFinish.XCorpVictory
                : BattleFinish.AlienVictory;

            mission.OnFinish(battle, finish);
            battle.PostMissionCleanup();
            Xenocide.GameState.Battlescape = null;

            Xenocide.ScreenManager.ScheduleScreen(new BattlescapeReportScreen(mission));
        }
    }
}
