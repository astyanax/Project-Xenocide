using System.Reflection;

using ProjectXenocide.Model;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Battlescape;

using Xunit;

namespace Xenocide.Test.MonoGame;

/// <summary>
/// Resolver tests that need a real <see cref="Combatant"/> (and therefore the
/// static tables). Covers the injury carry-over behaviour: a soldier who enters
/// a mission already wounded must not be healed by resolution.
/// </summary>
[Collection("GameStateInit")]
public class EngagementApplyTests : IDisposable
{
    public EngagementApplyTests()
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

    private static Combatant NewSoldier(int health, int injury)
    {
        Combatant combatant = ProjectXenocide.Xenocide.StaticTables.CombatantFactory.MakeAlien(Race.Morlock, AlienRank.Soldier);
        combatant.Stats[Statistic.Health] = health;
        combatant.Stats[Statistic.InjuryDamage] = injury;
        return combatant;
    }

    private static CombatantProfile Profile(Combatant combatant, bool xcorp, int accuracy, int damage)
    {
        return new CombatantProfile
        {
            Combatant = combatant,
            Name = xcorp ? "soldier" : "alien",
            IsXCorp = xcorp,
            Accuracy = accuracy,
            Health = combatant.Stats[Statistic.Health],
            CurrentHealth = combatant.Stats[Statistic.Health] - combatant.Stats[Statistic.InjuryDamage],
            Reactions = 40,
            Damage = damage,
            ArmorFront = 0,
        };
    }

    [Fact]
    public void Build_CurrentHealthExcludesExistingInjury()
    {
        Combatant combatant = NewSoldier(40, 7);

        CombatantProfile profile = CombatantProfile.Build(combatant, true, EngagementResolver.DefaultUnarmedDamage);

        Assert.Equal(40, profile.Health);
        Assert.Equal(33, profile.CurrentHealth);
    }

    [Fact]
    public void Build_CurrentHealthIsAtLeastOneEvenWhenBadlyWounded()
    {
        Combatant combatant = NewSoldier(10, 10);

        CombatantProfile profile = CombatantProfile.Build(combatant, true, EngagementResolver.DefaultUnarmedDamage);

        Assert.Equal(1, profile.CurrentHealth);
    }

    [Fact]
    public void Apply_DoesNotHealPreExistingInjury()
    {
        Combatant soldier = NewSoldier(40, 5);
        var xcorp = new[] { Profile(soldier, true, 80, 50) };
        var aliens = new[] { CombatantProfile.Synthetic("alien", false, 0, 100, 40, 0, 0) };

        var result = EngagementResolver.Simulate(xcorp, aliens, new Random(1));
        EngagementResolver.Apply(result);

        // The soldier took no damage, so the 5 points of injury he started with remain.
        Assert.Equal(5, soldier.Stats[Statistic.InjuryDamage]);
    }

    [Fact]
    public void Apply_NeverReducesInjuryAcrossSeeds()
    {
        bool observedDamage = false;
        for (int seed = 1; seed <= 25; ++seed)
        {
            Combatant soldier = NewSoldier(40, 5);
            var xcorp = new[] { Profile(soldier, true, 60, 30) };
            var aliens = new[] { CombatantProfile.Synthetic("alien", false, 70, 1000, 40, 8, 0) };

            var result = EngagementResolver.Simulate(xcorp, aliens, new Random(seed));
            EngagementResolver.Apply(result);

            Assert.True(soldier.Stats[Statistic.InjuryDamage] >= 5,
                $"seed {seed}: injury dropped to {soldier.Stats[Statistic.InjuryDamage]}");
            observedDamage |= soldier.Stats[Statistic.InjuryDamage] > 5;
        }

        Assert.True(observedDamage, "expected at least one seed to add new injury");
    }
}
