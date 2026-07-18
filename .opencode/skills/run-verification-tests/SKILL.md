---
name: run-verification-tests
description: "Run ALL project tests (in-game debug tests + xUnit test project) to verify codebase, check regressions, or validate changes. Use when the user says 'run tests', 'verify', 'check regressions', 'review codebase', or 'make sure everything works'."
---

# Run Verification Tests

Runs both test suites: the **xUnit unit tests** (`dotnet test`) and the **in-game debug tests** (Run Tests button). Tracks and reports any errors.

## Step 1: xUnit Unit Tests

Build and run the Xenocide.Test.MonoGame project:

```powershell
dotnet build .\src\Xenocide.Test.MonoGame\Xenocide.Test.MonoGame.csproj -c Debug
dotnet test .\src\Xenocide.Test.MonoGame\Xenocide.Test.MonoGame.csproj --no-build -v normal
```

Capture the **Passed / Failed / Total** counts and any failure messages.

## Step 2: In-Game Debug Tests

These are `[Conditional("DEBUG")]` static methods invoked from the main menu's "Run Tests" button (`StartScreen.OnRunTestsClicked`). To run them:

1. Build Debug: `dotnet build .\src\Xenocide.MonoGame\Xenocide.MonoGame.csproj -c Debug`
2. Launch: `.\src\Xenocide.MonoGame\bin\Debug\net9.0\Xenocide.MonoGame.exe`
3. On the main menu, click **Run Tests** (only visible in Debug builds)
4. Watch for any assertion failures (logged to console as `Process terminated. Assertion failed.` followed by the callstack)
5. Alternatively monitor the log output for `DEBUG | StartScreen | Run Tests clicked` to confirm tests started

The in-game tests are defined in these `RunTests()` methods across the codebase (all found via grep for `static void RunTests`):

| Class | File |
|---|---|
| `Geography.Planet.RunTests()` | `Source/Model/Geoscape/Geography/UnitTestPlanet.cs` |
| `Mission.RunTests()` | `Source/Model/Battlescape/Missions/UnitTestMission.cs` |
| `Combatant.RunTests()` | `Source/Model/Battlescape/Combatants/UnitTestCombatant.cs` |
| `Trajectory.RunTests()` | `Source/Model/Battlescape/Projectiles/UnitTestTrajectory.cs` |
| `Terrain.RunTests()` | `Source/Model/Battlescape/Terrain/UnitTestTerrainPathfinding.cs` |
| `ShootOrder.RunTests()` | `Source/Model/Battlescape/Combatants/Orders/UnitTestShootOrder.cs` |
| `MoveOrder.RunTests()` | `Source/Model/Battlescape/Combatants/Orders/UnitTestMoveOrder.cs` |
| `CrewBuilder.RunTests()` | `Source/Model/Battlescape/Missions/UnitTestCrewBuilder.cs` |
| `Pathfinder.RunTests()` | `Source/Model/Battlescape/Terrain/UnitTestPathfinder.cs` |
| `CombatantFactory.RunTests()` | .../StaticData/Battlescape/UnitTestCombatantFactory.cs |
| `Armor.RunTests()` | `Source/Model/StaticData/Battlescape/UnitTestArmor.cs` |
| `Item.RunItemTests()` | `Source/Model/StaticData/Items/UnitTestItemHandle.cs` |
| `CombatantInventory.RunTests()` | .../Combatants/UnitTestCombatantInventory.cs |
| `ResearchGraph.RunTests()` | `Source/Model/StaticData/Research/UnitTestResearchGraph.cs` |
| `BuildProjectManager.RunTests()` | `Source/Model/Geoscape/Projects/UnitTestBuildProjectManager.cs` |
| `ResearchProjectManager.RunTests()` | .../Projects/UnitTestResearchProjectManager.cs |
| `RetaliationTask.RunTests()` | `Source/Model/Geoscape/AI/UnitTestRetaliationTask.cs` |
| `BuildOutpostTask.RunTests()` | `Source/Model/Geoscape/AI/UnitTestBuildOutpostTask.cs` |
| `SupplyOutpostTask.RunTests()` | `Source/Model/Geoscape/AI/UnitTestSupplyOutpostTask.cs` |
| `TerrorTask.RunTests()` | `Source/Model/Geoscape/AI/UnitTestTerrorTask.cs` |
| `InfiltrationTask.RunTests()` | `Source/Model/Geoscape/AI/UnitTestInfiltrationTask.cs` |
| `GeoBitmap.RunTests()` | `Source/Model/Geoscape/Geography/UnitTestPlanet.cs` |
| `GeoPosition.RunTests()` | `Source/Model/Geoscape/Geography/UnitTestPlanet.cs` |
| `Scheduler.RunTests()` | `Source/Model/Scheduler.cs` |
| `AttackAlienSiteMission.RunTests()` | .../Missions/UnitTestAttackAlienSiteMission.cs |
| `OutpostStatistics.RunTests()` | `Source/Model/Geoscape/Outposts/UnitTestOutpostStatistics.cs` |
| `Floorplan.RunTests()` | `Source/Model/Geoscape/Outposts/UnitTestFloorplan.cs` |
| `OutpostInventory.RunTests()` | `Source/Model/Geoscape/Outposts/UnitTestOutpostInventory.cs` |
| `Ufo.RunTests()` | `Source/Model/Geoscape/Vehicles/UnitTestUfo.cs` |
| `Aircraft.RunTests()` | `Source/Model/Geoscape/Vehicles/UnitTestAircraft.cs` |
| `ScoreLog.RunTests()` | `Source/Model/Battlescape/ScoreLog.cs` |

## Error Tracking

Both suites use assertions (`Debug.Assert` for in-game tests, `Assert.*` / `Debug.Assert` for xUnit tests). Assertion failures manifest as:

- **xUnit tests**: Structured error output with file, line, expected vs actual
- **In-game tests**: `Process terminated. Assertion failed.` followed by a stack trace written to stderr

When reporting errors, include:
1. Which test failed (class name + method name)
2. The assertion expression and line number
3. The expected vs actual values
4. The callstack to understand what led to the failure

## Build Verification

Always rebuild both projects before running tests:

```powershell
dotnet build .\src\Xenocide.MonoGame\Xenocide.MonoGame.csproj -c Debug
dotnet build .\src\Xenocide.Test.MonoGame\Xenocide.Test.MonoGame.csproj -c Debug
```
