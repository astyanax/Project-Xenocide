# Project Xenocide

**Xenocide** was a fan-made, open-source remake of the classic strategy game *X-COM: UFO Defense* (also known as *UFO: Enemy Unknown*). It was built with C# and Microsoft XNA Game Studio 3.0.

> **Status:** This repository is an **archival snapshot** imported from the original Subversion repository. The project was actively developed between the early 2000s and ~2010. It is preserved here for historical, educational and nostalgic reasons.

## Tech Stack

| Component | Technology |
|-----------|------------|
| Language | C# (.NET 2.0) |
| Game Framework | Microsoft XNA Game Studio 3.0 |
| GUI | CeGui# (Crazy Eddie's GUI – .NET port) |
| Audio | FMOD Ex |
| Build | MSBuild (Visual Studio 2008) |
| Testing | NUnit |
| Installer | NSIS (current), Inno Setup (legacy) |
| Data | XML-driven game content |

## Features

### Geoscape (Global Strategy)
- Real-time 3D globe with time acceleration controls
- Base management — 17 facility types (storage, scan, defense, special) on a grid floorplan
- Aircraft interception — launch fighters with weapon pods to engage UFOs
- Research tree — XML-defined prerequisites and unlocks (items, facilities, X-Net entries)
- Manufacturing — build equipment in workshops with engineers
- X-Net encyclopedia — in-game database of researched technologies
- Funding model — monthly country-by-country support based on performance
- **Alien AI** — 3-layer strategic system: Overmind (monthly planning), InvasionTask (mission coordination), Craft (HFSM behaviors)

### Battlescape (Tactical Combat)
- Turn-based squad combat on tile-based 3D maps
- Teams: X-Corp, Aliens, Civilians
- Cell-based terrain with line-of-sight, cover, and height mechanics
- 6 hit locations (head, body, arms, legs) with armor front/side/rear values
- Fatal wounds system with medkit healing
- Experience and stat advancement for soldiers
- A* pathfinding with flyer/non-flyer support
- Combatant AI and team-level tactical AI
- Procedural terrain generation

### Aeroscape (Air Combat)
- Interceptor-vs-UFO air combat (partially implemented)

## Building

**Prerequisites:**
- Windows 2000 or newer (XP recommended)
- Visual C# Express 2008 or Visual Studio 2008
- Microsoft XNA Game Studio 3.0
- .NET Framework 2.0 SP1+
- DirectX 9.0c (August 2008+)

**Build:**
Open `xna/trunk/Xenocide.sln` in Visual Studio 2008, select Debug|x86 or Release|x86, and build.

> **Note:** Modern Windows systems may require workarounds to run XNA 3.0 applications. This codebase targets legacy APIs and is not expected to build or run on contemporary toolchains without significant effort.

## Project Structure

```
assets/                  — Artwork, design documents, sound, and pre-built installers
  ProgressReleases/      — Historical release installers (see LEGACY.md)
xna/                     — Main source code
  trunk/
    Xenocide/            — Game source (~93,650 lines of C#, 293 files)
    Xenocide.Pipeline/   — XNA Content Pipeline extension
    Tests/               — NUnit unit tests
    docs/                — Design documents and developer guides
  Installers/            — Installer build scripts (NSIS and legacy Inno Setup)
  branches/              — Feature branches (XNA 3.0, component system, GUI sandbox)
LICENSE                  — MIT License
```

## Media

- [YT: Project Xenocide 2007 Progress Update, by SupSuper](https://www.youtube.com/watch?v=0WZ4k61o0Kc)
- [StrategyCore: Home > Databank > Projects > Xenocide](https://www.strategycore.co.uk/databank/projects/xenocide/)

## License

Licensed under the [MIT License](LICENSE). Copyright © 2010 Project Xenocide team.

## Legacy Releases

Pre-built installers tracking the project's development history are archived in `assets/ProgressReleases/` and `xna/Installers/`. See [LEGACY.md](LEGACY.md) for details.
