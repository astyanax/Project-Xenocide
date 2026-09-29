# Strategic Engagement (Ground Combat)

Ground missions (attacking a landed/crashed UFO, an alien site, or defending a
base) are resolved by an abstract, round-based **Strategic Engagement** model.
There is no tactical map and no persistent battle state: a mission is turned
into snapshots of its two forces, those are simulated, and the casualties are
written back onto the roster.

This replaced the original 3D tactical battlescape (tile terrain, pathfinding,
move/shoot orders, per-unit AI). See `MIGRATION.md` for that history.

## Components

| Type | File | Responsibility |
|------|------|----------------|
| `CombatantProfile` | `Model/Battlescape/Resolution/CombatantProfile.cs` | Read-only snapshot of a unit (accuracy, health, reactions, damage, front armor) |
| `EngagementResolver` | `Model/Battlescape/Resolution/EngagementResolver.cs` | The simulation (`Simulate`), the odds estimate (`Predict`) and the write-back (`Apply`) |
| `EngagementResult` / `EngagementLog` / `EngagementPrediction` | `Model/Battlescape/Resolution/EngagementResult.cs` | Result data, round log and prediction aggregate |
| `EngagementText` | `Model/Battlescape/Resolution/EngagementText.cs` | Shared player-facing labels |
| `EngagementSession` | `Model/Battlescape/Resolution/EngagementSession.cs` | Builds the teams, runs `Predict`, drives `Engage`/`Withdraw` |
| `EngagementScreen` | `UI/Screens/EngagementScreen.cs` | Odds + force lists, Engage/Withdraw, staged combat log |
| `BattlescapeReportScreen` | `UI/Screens/BattlescapeReportScreen.cs` | Score, salvage, engagement summary and combat log |

### Flow

```
StartBattlescapeGeoEvent.Process()
   ├─ GeoTime.StopTime()
   └─ ScheduleScreen(new EngagementScreen(mission))
          └─ EngagementSession(mission)          // build teams + Monte-Carlo prediction
               ├─ mission.CreateXCorpTeam()
               ├─ mission.CreateAlienTeam()      // exposed as AlienTeam
               └─ EngagementResolver.Predict(...)

   Engage  → EngagementResolver.Simulate(...)    // deterministic given the RNG
           → EngagementResolver.Apply(result)    // write casualties back
           → mission.OnFinish(result.Finish, session.AlienTeam)
           → Combatant.PostMissionCleanup()      // heal fatal wounds
           → BattlescapeReportScreen(mission, result)
   Withdraw → mission.DontStart()                // no resolution
```

`Engage()` is idempotent — a second call returns the already-resolved result
rather than re-running the mission contract.

## The simulation

1. Each round, every living unit gets an initiative of `Reactions + rng(0..9)`;
   units act in descending order.
2. A shot rolls `clamp(Accuracy * AccuracyFactor, HitFloor, HitCeiling)` percent
   to hit.
3. A hit does `round(Damage * (1 ± DamageVariance))` damage, reduced by the
   target's **front** armor (never below 0). The shooter focuses the enemy with
   the highest damage.
4. The fight ends when a side is wiped out, or at `MaxRounds`. At the cap the
   side with the higher remaining **health fraction** wins; within
   `StalemateBand` it is a draw (`Aborted`).

Outcomes map to `BattleFinish`:

| `BattleFinish` | Meaning |
|----------------|---------|
| `XCorpVictory` | Aliens wiped out (or X-Corp clearly ahead at the cap) |
| `AlienVictory` | Squad wiped out (or aliens clearly ahead at the cap) |
| `Aborted` | Draw at the cap, or the player withdrew |

### Tuning constants (`EngagementResolver`)

| Constant | Value | Visibility | Meaning |
|----------|-------|-----------|---------|
| `MaxRounds` | 20 | public | Round cap before a decision is forced |
| `DefaultUnarmedDamage` | 15 | public | Damage for units with no weapon |
| `DefaultSamples` | 200 | public | Monte-Carlo sample count |
| `HitFloor` / `HitCeiling` | 5 / 95 | private | Clamp on hit chance (%) |
| `AccuracyFactor` | 0.5 | private | Scales raw accuracy into hit chance |
| `DamageVariance` | 0.2 | private | ±20% damage spread |
| `StalemateBand` | 0.05 | private | Health-fraction margin treated as a draw |

## Monte-Carlo prediction

`Predict` runs `Simulate` N times with its **own** `Random`, so it never disturbs
the game's RNG stream. It reports `WinProbability`, `ExpectedXCorpKia`,
`ExpectedXCorpWounded` and `ExpectedAlienKills`. `samples <= 0` is clamped to 1;
a fixed seed makes the prediction deterministic (used by tests).

## Applying results

`Apply` writes only casualties:

- Dead → `InjuryDamage = Health + 1`.
- Alive → `InjuryDamage = Health - RemainingHealth`.

Because each unit starts the simulation from `CombatantProfile.CurrentHealth`
(`Health - InjuryDamage`, floor 1), this is monotonic: a soldier who entered a
mission already wounded is never healed by fighting. `XCorpWounded` counts only
units that actually lost health this mission.

## Mission result contract

`Mission.OnFinish(BattleFinish finishType, Team alienTeam)` calls `CalcLosses`,
then the mission-specific `OnFinishCore(finishType)`.

- `ScoreKilledAliens` records dead aliens; on `XCorpVictory` surviving aliens are
  recorded as **captures** and recovered (with their kit) into salvage.
- Each finish type has an X-Corp-loss hook (`CalcXCorpLossesOnAbort`,
  `...OnAlienVictory`, `...OnXCorpVictory`) and an alien-loss hook.

Per mission type:

| Mission | `XCorpVictory` | `AlienVictory` | `Aborted` |
|---------|----------------|----------------|-----------|
| Base `Mission` | captures + salvage | no recovery | survivors recovered, no salvage |
| `UfoSiteMission` | UFO destroyed, salvage, craft home | craft destroyed | craft goes home, UFO survives |
| `AlienSiteMission` | site destroyed, salvage | craft destroyed | site survives |
| `XCorpOutpostMission` | attacking UFO destroyed | outpost destroyed | outpost holds; only casualties lost |

## The UI

`EngagementScreen` builds its controls programmatically (`HasGumxLayout => false`).
It shows the prediction, both force grids, and Engage / Withdraw. After engaging
it reveals the round log one round every 0.35 s, with **Skip to End**, then
**Continue** opens `BattlescapeReportScreen`, whose OK button ships the salvage,
applies the score and returns to the geoscape.

## Testing

Model-layer tests (xUnit):

- `EngagementResolverTests` — determinism, decisive/aborted outcomes, empty and
  null forces, armor absorption, prediction aggregates and sample clamping, log
  ordering. Pure: no game state needed.
- `EngagementApplyTests` — `CombatantProfile.Build` and the injury carry-over
  rules (needs the static tables; part of the `GameStateInit` collection).
- `EngagementMissionTests` — `Mission.OnFinish` salvage/captures per finish type,
  `EngagementSession` resolve-once/withdraw, and post-mission fatal-wound cleanup.

The screen text logic is kept in the model (`EngagementText`) so it stays
testable without Gum.

## How to add or differentiate a mission type

1. Subclass `Mission` (see `UfoSiteMission` for a template).
2. Implement `MakeStartMissionText` and override `CreateAlienTeam` /
   `CreateXCorpTeam` (and `CreateCivilianTeam` for terror missions).
3. Override the loss hooks and `OnFinishCore` for the mission's consequences.
4. Raise `StartBattlescapeGeoEvent(mission)` when the craft reaches the target —
   the rest of the pipeline is shared.

Mission types currently differ only in force composition, terrain-free setup and
the result consequences. Deliberate future work: doctrine/roles and squad
training, and per-mission rules (see below).

## Reserved for the roadmap

These members are intentionally unused today and tagged with `TODO` comments in
code; they exist to keep the data model ready for planned features:

| Member | Planned use |
|--------|-------------|
| `CombatantProfile.Bravery` | Morale / panic / surrender |
| `UnitOutcome.Unconscious` | Stun/knockout outcomes (live captures) |
| `EngagementPrediction.Samples` | Prediction confidence shown in the UI |
| `Combatant.OnStartTurn`, `Team.OnStartTurn` | Turn-based resolution model |
| `Combatant.Heal`, `TotalFatalWounds`, `fatalWoundsStat`, `GameBalanceClass.GenerateFatalWounds`/`HealFatalWounds`/`HealInjuryDamage` | Wound model (fatal wounds, medkits) |
| `Combatant.RecordAchievement`, `Experience` | Engagement XP / stat advancement |
| `Combatant.Kneeling` | Stance/cover accuracy bonus (already read by `Accuracy`) |
| `Combatant.Position`/`Heading`/`HeadingVector` | Positional model (flanking, side/rear armor) |
| `Combatant.PlaceInTeam` | Stable unit ordering in the engagement UI/report |
| `Mission.CreateCivilianTeam` | Terror-mission civilians |
