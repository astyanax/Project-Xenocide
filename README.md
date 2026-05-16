# Project Xenocide

**Xenocide** was a fan-made, open-source remake of the classic strategy game *X-COM: UFO Defense* (also known as *UFO: Enemy Unknown*). Originally built with C# and Microsoft XNA Game Studio 3.0 (~2007–2010), this project is currently being **migrated to MonoGame** for modern cross-platform support (Windows + Linux).

> **Status:** This repository is an **archival snapshot** imported from the original Subversion repository, currently undergoing modernization. See [MIGRATION.md](MIGRATION.md) for the full plan and progress.

## Tech Stack

### Original (Legacy XNA 3.0)
| Component | Technology |
|-----------|------------|
| Language | C# (.NET 2.0) |
| Game Framework | Microsoft XNA Game Studio 3.0 |
| GUI | CeGui# (Crazy Eddie's GUI – .NET port) |
| Audio | FMOD Ex |
| Build | MSBuild (Visual Studio 2008) |
| Testing | NUnit |
| Installer | NSIS (current), Inno Setup (legacy) |

### Target (MonoGame)
| Component | Technology |
|-----------|------------|
| Language | C# (.NET 8.0+) |
| Game Framework | MonoGame 3.8.4+ (DesktopGL — OpenGL + SDL2) |
| GUI | Gum / MGUI / Myra *(to be decided)* |
| Audio | MonoGame SoundEffect/Song or FMOD *(to be decided)* |
| Build | .NET SDK 9.0+, `dotnet` CLI, MGCB content pipeline |
| Testing | NUnit (via NuGet) |
| Content | MGCB Editor (compiles .fbx, .fx, .spritefont, textures) |
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

## Building (Modern — MonoGame Target)

The migration to MonoGame is in progress. Once complete, the game will build on Windows and Linux with these steps.

### Prerequisites

**1. Install .NET SDK**

The .NET SDK is required. Use the official install script (recommended for cross-platform):

```powershell
# Windows (PowerShell)
Invoke-WebRequest -Uri https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.ps1 -OutFile dotnet-install.ps1
.\dotnet-install.ps1 -Channel 9.0
```

```bash
# Linux / macOS
curl -sSL https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0
```

Alternatively, download the installer from:
https://dotnet.microsoft.com/en-us/download/dotnet/9.0

**2. Install MonoGame templates**

```powershell
dotnet new install MonoGame.Templates.CSharp
```

**3. Install MGCB (content pipeline) tools**

```powershell
dotnet tool install -g dotnet-mgcb
dotnet tool install -g dotnet-mgcb-editor
```

### Build & Run

```powershell
# Restore dependencies
dotnet restore

# Build and run
dotnet run --project xna/trunk/Xenocide.MonoGame
```

> **Note:** The MGCB content pipeline compiles .fbx models, .fx shaders, textures, and spritefonts into .xnb format at build time via the `MonoGame.Content.Builder.Task` NuGet package.

---

## Building (Legacy — XNA 3.0)

The original XNA 3.0 build is preserved for reference but **requires deprecated tooling**:

- Visual C# Express 2008 or Visual Studio 2008
- Microsoft XNA Game Studio 3.0
- .NET Framework 2.0 SP1+
- DirectX 9.0c (August 2008+)

Open `xna/trunk/Xenocide.sln` in Visual Studio 2008, select Debug|x86 or Release|x86, and build.

> Modern Windows systems generally **cannot** build the original XNA 3.0 project without running the legacy toolchain in a VM or compatibility environment.

## Project Structure

```
MIGRATION.md             — MonoGame migration plan and roadmap
assets/                  — Artwork, design documents, sound, and pre-built installers
  ProgressReleases/      — Historical release installers (see LEGACY.md)
xna/                     — Main source code
  trunk/
    Xenocide/            — Game source (~93,650 lines of C#, 293 files) [LEGACY XNA 3.0]
    Xenocide.MonoGame/   — MonoGame migration target [NEW]
    Xenocide.Pipeline/   — XNA Content Pipeline extension [LEGACY]
    Tests/               — NUnit unit tests [LEGACY]
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
