using System.Reflection;

using ProjectXenocide.Model;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Battlescape;

using Xunit;

namespace Xenocide.Test.MonoGame;

/// <summary>
/// Tests the Mission result contract and EngagementSession wiring using a fake
/// mission that carries synthetic teams. Shares the "GameStateInit" collection
/// because building combatants needs the static tables.
/// </summary>
[Collection("GameStateInit")]
public class EngagementMissionTests : IDisposable
{
    public EngagementMissionTests()
    {
        ProjectXenocide.Xenocide.Rng.ClearLoadedValues();

        var staticTables = new StaticTables();
        typeof(ProjectXenocide.Xenocide)
            .GetField("staticTables", BindingFlags.Static | BindingFlags.NonPublic)!
            .SetValue(null, staticTables);
        staticTables.Populate();

        typeof(ProjectXenocide.Xenocide)
            .GetField("gameBalance", BindingFlags.Static | BindingFlags.NonPublic)!
            .SetValue(null, new GameBalanceClass(Difficulty.Easy));

        ProjectXenocide.Xenocide.GameState = new GameState();
        ProjectXenocide.Xenocide.GameState.SetToStartGameCondition();
    }

    public void Dispose()
    {
        typeof(ProjectXenocide.Xenocide)
            .GetField("staticTables", BindingFlags.Static | BindingFlags.NonPublic)!
            .SetValue(null, null);
        ProjectXenocide.Xenocide.GameState = null!;
    }

    private static Combatant NewAlien()
    {
        return ProjectXenocide.Xenocide.StaticTables.CombatantFactory.MakeAlien(Race.Morlock, AlienRank.Soldier);
    }

    private static void Kill(Combatant combatant)
    {
        combatant.Stats[Statistic.Health] = 10;
        combatant.Stats[Statistic.InjuryDamage] = 11;
    }

    private static Team TeamOf(params Combatant[] combatants)
    {
        var team = new Team();
        foreach (Combatant combatant in combatants)
        {
            team.Combatants.Add(combatant);
        }
        return team;
    }

    [Fact]
    public void XCorpVictory_RecoversAndCapturesTheAlienForce()
    {
        Combatant survivor = NewAlien();
        Combatant dead = NewAlien();
        Kill(dead);
        Team aliens = TeamOf(survivor, dead);
        var mission = new FakeMission { AlienTeam = aliens };

        mission.OnFinish(BattleFinish.XCorpVictory, aliens);

        string capturedItem = ProjectXenocide.Xenocide.StaticTables.ItemList[survivor.CombatantInfo.StunItemId].Id;
        string deadItem = ProjectXenocide.Xenocide.StaticTables.ItemList[dead.CombatantInfo.DeadItemId].Id;
        Assert.Contains(mission.Salvage, line => line.ItemId == capturedItem);
        Assert.Contains(mission.Salvage, line => line.ItemId == deadItem);
        Assert.NotEmpty(mission.Scores);
    }

    [Fact]
    public void AlienVictory_RecoversNothing()
    {
        Team aliens = TeamOf(NewAlien());
        var mission = new FakeMission { AlienTeam = aliens };

        mission.OnFinish(BattleFinish.AlienVictory, aliens);

        Assert.Empty(mission.Salvage);
    }

    [Fact]
    public void Aborted_RecoversNothing()
    {
        Team aliens = TeamOf(NewAlien());
        var mission = new FakeMission { AlienTeam = aliens };

        mission.OnFinish(BattleFinish.Aborted, aliens);

        Assert.Empty(mission.Salvage);
    }

    [Fact]
    public void Engage_ResolvesOnceAndAppliesCasualties()
    {
        Combatant soldier = NewAlien();
        soldier.Stats[Statistic.FatalWoundsHead] = 3;
        var mission = new FakeMission
        {
            XcorpTeam = TeamOf(soldier),
            AlienTeam = TeamOf(NewAlien()),
        };
        var session = new EngagementSession(mission, predictionSamples: 5);

        EngagementResult first = session.Engage();
        EngagementResult second = session.Engage();

        Assert.NotNull(first);
        Assert.Same(first, second);
        Assert.Same(first, session.Result);
        Assert.Equal(1, mission.OnFinishCount);
        // Post-mission cleanup heals fatal wounds.
        Assert.Equal(0, soldier.Stats[Statistic.FatalWoundsHead]);
    }

    [Fact]
    public void Withdraw_DoesNotResolveTheMission()
    {
        var mission = new FakeMission
        {
            XcorpTeam = TeamOf(NewAlien()),
            AlienTeam = TeamOf(NewAlien()),
        };
        var session = new EngagementSession(mission, predictionSamples: 5);

        session.Withdraw();

        Assert.Null(session.Result);
        Assert.Equal(0, mission.OnFinishCount);
    }

    /// <summary>Minimal mission with synthetic teams and no aircraft/outpost.</summary>
    private sealed class FakeMission : Mission
    {
        public FakeMission()
            : base((Outpost)null!)
        {
        }

        public Team XcorpTeam { get; set; } = new Team();
        public Team AlienTeam { get; set; } = new Team();
        public int OnFinishCount { get; private set; }

        public override string MakeStartMissionText() => "test";

        public override Team CreateXCorpTeam() => XcorpTeam;
        public override Team CreateAlienTeam() => AlienTeam;

        protected override void OnFinishCore(BattleFinish finishType) => ++OnFinishCount;

        // The base loss hooks walk aircraft/outpost, which this fake does not have.
        protected override void CalcXCorpLossesOnAbort() { }
        protected override void CalcXCorpLossesOnAlienVictory() { }
        protected override void CalcXCorpLossesOnXCorpVictory() { }
    }
}
