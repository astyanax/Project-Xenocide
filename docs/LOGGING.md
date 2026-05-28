# Logging Architecture

## Overview

Project Xenocide uses **NLog** as its unified logging framework. All diagnostic output
(console, debug, and file) flows through NLog, replacing the previous ad-hoc mix of
`Console.WriteLine`, `Debug.WriteLine`, and custom file logging.

## Library Selection

### Comparison

| Feature | NLog (chosen) | Serilog | log4net | MS Extensions Logging |
|---|---|---|---|---|
| NuGet Downloads | 564M+ | 1.1B+ (sinks) | ~50M | Built into .NET |
| GitHub Stars | 6.5k | 8k | 927 | N/A |
| Latest Version | 6.1.3 | 4.3.1 | 3.3.1 | .NET 9 |
| License | BSD-3-Clause | Apache 2.0 | Apache 2.0 | MIT |
| Structured Logging | Yes (since v4.5) | First-class | No | Yes |
| Coloured Console | Built-in, ANSI | Built-in themes | Limited | 3rd party |
| File Sink + Rotation | Built-in | Separate sink | Built-in | No built-in |
| Dependencies | **Zero** | Multiple sinks | Minimal | Many sub-packages |
| AOT Support | Full (v6.0+) | Partial | No | Yes |
| Config Formats | XML, JSON, code | Code-first, JSON | XML only | Code/JSON |
| Game Usage | s&box (Facepunch), Playnite | Some indie games | Rare | N/A |

### Why NLog Wins for This Project

1. **Zero dependencies** — single NuGet package provides console, file, rotation,
   async logging, and coloured output. Serilog needs 3-4 separate packages.
2. **AOT compatible** — future-proof for potential NativeAOT builds.
3. **Battle-tested in games** — Facepunch's s&box engine and Playnite game launcher
   both use NLog.
4. **XML config** — easy to swap log levels per environment without recompilation;
   `autoReload="true"` means you can edit the config live.
5. **Very low overhead when a log level is disabled** — important for game loops.

## Configuration

### NLog.config

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true"
      throwConfigExceptions="false"
      internalLogLevel="Off">

  <targets>
    <!-- Coloured console output -->
    <target name="console" xsi:type="ColoredConsole"
            layout="${longdate} ${pad:padding=5:inner=${level:uppercase=true}} | ${logger:shortName=true} | ${message}${onexception:inner=${newline}${exception:format=tostring}}" />

    <!-- Rotating file log (7-day retention) -->
    <target name="file" xsi:type="File"
            fileName="${specialfolder:folder=LocalApplicationData}/Xenocide/logs/xenocide.log"
            layout="${longdate} | ${level:uppercase=true} | ${logger} | ${message}${onexception:inner=${newline}${exception:format=tostring}}"
            archiveFileName="${specialfolder:folder=LocalApplicationData}/Xenocide/logs/xenocide.{#}.log"
            archiveEvery="Day"
            maxArchiveFiles="7"
            archiveNumbering="Date"
            concurrentWrites="false"
            keepFileOpen="false" />
  </targets>

  <rules>
    <logger name="*" minlevel="Trace" writeTo="console" />
    <logger name="*" minlevel="Info"  writeTo="file" />
  </rules>
</nlog>
```

### Log Locations

| Output | Location |
|---|---|
| Console | `stdout` (coloured, timestamps, level) |
| File | `%LocalAppData%\Xenocide\logs\xenocide.log` |
| Archives | `%LocalAppData%\Xenocide\logs\xenocide.{date}.log` (7-day rolling) |

## Log Level Guidelines

| Level | Use Case |
|---|---|
| `Fatal` | Unhandled exceptions, crashes (Program.cs crash handler) |
| `Error` | Save failures, asset load failures with exceptions |
| `Warn` | Missing files, fallbacks triggered, validation warnings |
| `Info` | Game startup info, max texture size, version info |
| `Debug` | Screen transitions, dialog lifecycle, click events, state entry, GeoTime debug, profile timings, grid dumps |
| `Trace` | Very verbose per-frame details (not currently used) |

## Getting a Logger

Each class gets its own logger via:

```csharp
private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
```

Use it like:

```csharp
Logger.Info("MaxTextureSize: {0}", profileMax);
Logger.Warn("EarthGlobe: {0} not found, falling back to {1}", primary, fallback);
Logger.Error(ex, "Save failed");
Logger.Debug("ScreenManager: ScheduleScreen {0}", name);
```

## What Was Replaced

### Previously: Ad-Hoc Logging (42 call sites, 18 files)

| Pattern | Files | Replaced With |
|---|---|---|
| `Console.WriteLine(...)` | 15 files | `Logger.Debug/Info/Warn(...)` |
| `Console.Error.WriteLine(...)` | 3 files | `Logger.Error/Fatal(...)` |
| `Debug.WriteLine(...)` | 5 files | `Logger.Debug(...)` |
| Custom file `audio_debug.log` | `GameAudioComponent.cs` | NLog file target |
| `Util.GeoTimeDebugWriteLine(...)` | `Util.cs` + ~11 call sites | NLog `Logger.Debug(...)` |
| `Profile.Log(...)` | `ProfileTimer.cs` | NLog `Logger.Debug(...)` |

### NOT Changed (Not Logging)

- **`Debug.Assert`** (~833 calls) — development-time invariant checks, not logging.
- **`MessageLog.cs`** — in-game player-facing event queue (serialized to save files, drives UI).
- **`ScoreLog.cs`**, **`BattleLog.cs`**, **`MonthlyLog.cs`** — in-game data models.

## Migration Status

- [x] NLog package added
- [x] NLog.config created
- [x] Program.cs crash handler
- [x] Xenocide.cs
- [x] Util.cs (GeoTimeDebugWriteLine)
- [x] ProfileTimer.cs
- [x] ContentCache.cs
- [x] StartScreen.cs
- [x] ScreenManager.cs
- [x] ModalDialog.cs
- [x] GeoscapeScreen.cs
- [x] LoadSaveGameScreen.cs
- [x] EquipSoldierScreen.cs
- [x] BattlescapeScreen.cs
- [x] EarthGlobe.cs
- [x] CombatantMeshes.cs
- [x] ProjectileMesh.cs
- [x] FacilityMesh.cs
- [x] GeoBitmap.cs
- [x] UnitTestOutpostInventory.cs
- [x] GameAudioComponent.cs
- [x] System.Diagnostics cleanup (41 files)
- [x] SettingsScreen with NLog debug logging
- [x] GumDialog NLog crash fix (Text vs Label)
