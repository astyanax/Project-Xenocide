using System;
using System.Collections.Generic;
using System.Linq;

using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.UI.Screens;

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// Glue between a geoscape ground mission and the Strategic Engagement
    /// resolver: builds the forces, resolves the engagement, applies the
    /// casualties, runs the existing mission result contract, and shows the
    /// report screen.
    /// </summary>
    /// <remarks>
    /// The dedicated Engagement screen (shown before committing, with the odds
    /// prediction and the log) will drive <see cref="EngagementResolver"/>
    /// directly; this headless path remains for auto-resolution.
    /// </remarks>
    public static class AutoResolveMission
    {
        public static EngagementResult Resolve(Mission mission)
        {
            var battle = new Battle(mission);

            List<CombatantProfile> xcorp = battle.Teams[Team.XCorp].Combatants
                .Select(c => CombatantProfile.Build(c, true, EngagementResolver.DefaultUnarmedDamage))
                .ToList();
            List<CombatantProfile> aliens = battle.Teams[Team.Aliens].Combatants
                .Select(c => CombatantProfile.Build(c, false, EngagementResolver.DefaultUnarmedDamage))
                .ToList();

            EngagementResult result = EngagementResolver.Simulate(xcorp, aliens, new Random());
            EngagementResolver.Apply(result);

            mission.OnFinish(battle, result.Finish);
            battle.PostMissionCleanup();
            Xenocide.GameState.Battlescape = null;

            Xenocide.ScreenManager.ScheduleScreen(new BattlescapeReportScreen(mission));
            return result;
        }
    }
}
