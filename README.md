# Project Xenocide

**Xenocide** is a fan-made, open-source remake of the classic strategy game *X-COM: UFO Defense* (also known as *UFO: Enemy Unknown*). Originally built with C# and Microsoft XNA Game Studio 3.0 (~2007–2010), the project has been **migrated to MonoGame** for modern cross-platform support (Windows + Linux).

## Tech Stack

| Component | Technology |
|---|---|
| Language | C# (.NET 9.0) |
| Game Framework | MonoGame 3.8.x (DesktopGL — OpenGL + SDL2) |
| GUI | **Gum** (Gum.MonoGame) — WYSIWYG layouts via `.gusx` files |
| Audio | MonoGame `SoundEffect` via MGCB content pipeline |
| Logging | **NLog** — coloured console + rotating file output |
| Build | .NET SDK 9.0+, `dotnet` CLI, MGCB content pipeline |
| Testing | **xUnit.net** 2.9.2 |
| Content | MGCB Editor (compiles .fbx, .fx, .spritefont, textures, .ogg) |
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

### Settings
- Display options (resolution, fullscreen, software/hardware cursor)
- Sound options (music/SFX volume)
- Notification and gameplay settings

## Building

### Prerequisites

- **.NET SDK 9.0+** — [download](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- **MonoGame templates and MGCB tools:**
  ```powershell
  dotnet new install MonoGame.Templates.CSharp
  dotnet tool install -g dotnet-mgcb
  dotnet tool install -g dotnet-mgcb-editor
  ```

### Build & Run

```powershell
dotnet run --project src/Xenocide.MonoGame
```

The MGCB content pipeline compiles .fbx models, .fx shaders, textures, spritefonts, and .ogg audio into .xnb format at build time.

## Documentation

| Document | Description |
|---|---|
| [MIGRATION.md](MIGRATION.md) | Full migration plan and progress (XNA 3.0 → MonoGame) |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | System architecture (screens, AI/missions, game state, UI) |
| [docs/GUI.md](docs/GUI.md) | Gum GUI framework and Xenocide UI component architecture |
| [docs/DIALOG.md](docs/DIALOG.md) | Dialog and message/notification system |
| [docs/LOGGING.md](docs/LOGGING.md) | Logging architecture (NLog setup, log levels, configuration) |
| [AGENTS.md](AGENTS.md) | Contributor/agent guide (architecture conventions) |
| [LEGACY.md](LEGACY.md) | Historical context and legacy releases |

## Project Structure

```
Xenocide.sln             — Solution file
AGENTS.md                — Contributor/agent guide
MIGRATION.md             — Migration plan and roadmap
docs/                    — Architecture, GUI, dialog and logging documentation
assets/                  — Artwork, design documents, sounds
src/
  Xenocide.MonoGame/     — Game project (active development)
    Source/
      Audio/             — GameAudioComponent (MonoGame SoundEffect backend)
      Model/             — Game state, geoscape, battlescape, static data
      Services/          — Savegame service
      UI/
        Controls/        — Themed controls (ScreenLayout, ThemedLabel/Button, StyledGrid)
        Dialogs/         — Modal dialogs (programmatic ModalDialog subclasses)
        Scenes/          — 3D scenes (Geoscape, Battlescape, XNet, Facilities, ...)
        Screens/         — 24 game screens (Gum-based)
      Utils/             — NLog logging, profiling, serialization, content cache
    Content/             — MGCB assets (models, shaders, textures, fonts, audio)
      Gum/               — Gum project (Xenocide.gumx) + .gusx/.gucx layouts
    Content.mgcb         — MGCB content manifest
  Xenocide.Test.MonoGame/ — xUnit.net unit tests
```

## Legacy Build (XNA 3.0)

The original XNA 3.0 build required deprecated tooling (Visual Studio 2008, XNA Game Studio 3.0, .NET Framework 2.0). It has been superseded by the MonoGame port.

## Media

- [YT: Project Xenocide 2007 Progress Update, by SupSuper](https://www.youtube.com/watch?v=0WZ4k61o0Kc)
- [StrategyCore: Home > Databank > Projects > Xenocide](https://www.strategycore.co.uk/databank/projects/xenocide/)

## License

Licensed under the [MIT License](LICENSE). Copyright © 2010 Project Xenocide team.
