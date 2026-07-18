using System.Diagnostics;
using System.Reflection;

using ProjectXenocide;
using ProjectXenocide.Model;
using ProjectXenocide.Model.StaticData;

namespace Xenocide.Test.MonoGame;

[Collection("GameStateInit")]
public class InGameUnitTests : IDisposable
{
    public InGameUnitTests()
    {
        var staticTables = new StaticTables();
        var staticTablesField = typeof(ProjectXenocide.Xenocide).GetField("staticTables", BindingFlags.Static | BindingFlags.NonPublic)!;
        staticTablesField.SetValue(null, staticTables);
        staticTables.Populate();

        var gameBalanceField = typeof(ProjectXenocide.Xenocide).GetField("gameBalance", BindingFlags.Static | BindingFlags.NonPublic)!;
        gameBalanceField.SetValue(null, new GameBalanceClass(Difficulty.Easy));

        ProjectXenocide.Xenocide.GameState = new GameState();
    }

    public void Dispose()
    {
        var staticTablesField = typeof(ProjectXenocide.Xenocide).GetField("staticTables", BindingFlags.Static | BindingFlags.NonPublic)!;
        staticTablesField.SetValue(null, null);
        ProjectXenocide.Xenocide.GameState = null!;
    }

    [Fact] public void Planet_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Geoscape.Geography.Planet.RunTests());
    [Fact] public void Mission_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Battlescape.Mission.RunTests());
    [Fact] public void Combatant_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Battlescape.Combatants.Combatant.RunTests());
    [Fact] public void Trajectory_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Battlescape.Trajectory.RunTests());
    [Fact] public void Terrain_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Battlescape.Terrain.RunTests());
    [Fact] public void ShootOrder_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Battlescape.Combatants.ShootOrder.RunTests());
    [Fact] public void MoveOrder_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Battlescape.Combatants.MoveOrder.RunTests());
    [Fact] public void CrewBuilder_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Battlescape.CrewBuilder.RunTests());
    [Fact] public void Pathfinder_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Battlescape.Pathfinder.RunTests());
    [Fact] public void CombatantFactory_RunTests() => RunInGameTest(() => ProjectXenocide.Model.StaticData.Battlescape.CombatantFactory.RunTests());
    [Fact] public void Armor_RunTests() => RunInGameTest(() => ProjectXenocide.Model.StaticData.Battlescape.Armor.RunTests());
    [Fact] public void Item_RunItemTests() => RunInGameTest(() => ProjectXenocide.Model.StaticData.Items.Item.RunItemTests());
    [Fact] public void CombatantInventory_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Battlescape.Combatants.CombatantInventory.RunTests());
    [Fact] public void ResearchGraph_RunTests() => RunInGameTest(() => ProjectXenocide.Model.StaticData.Research.ResearchGraph.RunTests());
    [Fact] public void BuildProjectManager_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Geoscape.BuildProjectManager.RunTests());
    [Fact] public void ResearchProjectManager_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Geoscape.ResearchProjectManager.RunTests());
    [Fact] public void RetaliationTask_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Geoscape.AI.RetaliationTask.RunTests());
    [Fact] public void BuildOutpostTask_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Geoscape.AI.BuildOutpostTask.RunTests());
    [Fact] public void SupplyOutpostTask_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Geoscape.AI.SupplyOutpostTask.RunTests());
    [Fact] public void TerrorTask_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Geoscape.AI.TerrorTask.RunTests());
    [Fact] public void InfiltrationTask_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Geoscape.AI.InfiltrationTask.RunTests());
    [Fact] public void GeoBitmap_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Geoscape.Geography.GeoBitmap.RunTests());
    [Fact] public void GeoPosition_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Geoscape.GeoPosition.RunTests());
    [Fact] public void Scheduler_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Scheduler.RunTests());
    [Fact] public void AttackAlienSiteMission_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Geoscape.Vehicles.AttackAlienSiteMission.RunTests());
    [Fact] public void OutpostStatistics_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Geoscape.Outposts.OutpostStatistics.RunTests());
    [Fact] public void Floorplan_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Geoscape.Outposts.Floorplan.RunTests());
    [Fact] public void OutpostInventory_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Geoscape.Outposts.OutpostInventory.RunTests());
    [Fact] public void Ufo_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Geoscape.Vehicles.Ufo.RunTests());
    [Fact] public void Aircraft_RunTests() => RunInGameTest(() => ProjectXenocide.Model.Geoscape.Vehicles.Aircraft.RunTests());
    [Fact] public void ScoreLog_RunTests() => RunInGameTest(() => ProjectXenocide.Model.ScoreLog.RunTests());

    private static void RunInGameTest(Action test)
    {
        ProjectXenocide.Xenocide.DebugTesting = true;

        var failures = new List<string>();

        var savedListeners = new TraceListener[Trace.Listeners.Count];
        Trace.Listeners.CopyTo(savedListeners, 0);
        Trace.Listeners.Clear();
        Trace.Listeners.Add(new AssertCaptureListener(failures));

        try
        {
            test();
        }
        catch (Exception ex)
        {
            failures.Add($"Exception: {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            ProjectXenocide.Xenocide.DebugTesting = false;

            Trace.Listeners.Clear();
            foreach (var l in savedListeners)
                Trace.Listeners.Add(l);
        }

        if (failures.Count > 0)
        {
            Assert.Fail("In-game test failed:\n" + string.Join("\n", failures.Select(f => "  " + f)));
        }
    }

    private sealed class AssertCaptureListener : TraceListener
    {
        private readonly List<string> _failures;

        public AssertCaptureListener(List<string> failures)
        {
            _failures = failures;
        }

        public override void Write(string? message) { }
        public override void WriteLine(string? message) { }

        public override void Fail(string? message)
        {
            _failures.Add("Debug.Assert: " + message);
        }

        public override void Fail(string? message, string? detailMessage)
        {
            _failures.Add("Debug.Assert: " + message + "\n    " + detailMessage);
        }
    }
}
