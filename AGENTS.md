# AGENTS.md — Project Xenocide Agent Guide

This document provides comprehensive context for AI agents working on the Project Xenocide codebase.

---

## Project Overview

**Xenocide** is a fan-made, open-source remake of *X-COM: UFO Defense* (1994). Originally built with C# and XNA Game Studio 3.0 (~2007–2010), it has been migrated to **MonoGame** for modern cross-platform support.

### Tech Stack

| Component | Technology |
|-----------|-----------|
| Language | C# (.NET 9.0) |
| Game Framework | MonoGame 3.8.x (DesktopGL — OpenGL + SDL2) |
| GUI | Gum (`Gum.MonoGame` NuGet) — WYSIWYG layouts via `.gusx` files |
| Audio | MonoGame `SoundEffect` via MGCB content pipeline |
| Logging | NLog (coloured console + rotating file) |
| Build | .NET SDK 9.0+, `dotnet` CLI, MGCB content pipeline |
| Testing | xUnit.net 2.9.2 |
| Content | MGCB Editor (compiles .fbx, .fx, .spritefont, textures, .ogg to .xnb) |
| Data | XML-driven game content (items, research, facilities, UFO behaviors) |

### Build & Run

```powershell
# Build
dotnet build src/Xenocide.MonoGame

# Run
dotnet run --project src/Xenocide.MonoGame

# Run tests
dotnet test src/Xenocide.Test.MonoGame

# Build content only
dotnet build src/Xenocide.MonoGame -t:BuildContent
```

### Prerequisites

- .NET SDK 9.0+
- MonoGame templates: `dotnet new install MonoGame.Templates.CSharp`
- MGCB tools: `dotnet tool install -g dotnet-mgcb` and `dotnet tool install -g dotnet-mgcb-editor`

---

## Project Structure

```
src/
  Xenocide.MonoGame/           — Main game project
    Source/
      Audio/                   — GameAudioComponent (MonoGame SoundEffect backend)
      Model/                   — Game state, geoscape, battlescape, static data, AI
      Services/                — Savegame service
      UI/
        Controls/              — Toast notifications, software cursor
        Dialogs/               — 13 modal dialogs (ModalDialog base class)
        Scenes/                — 3D rendering scenes
          Battlescape/         — 3D battlefield rendering
          Common/              — Shared 3D utilities (PolarScene, LineMesh)
          EquipSoldier/        — 3D soldier model viewer
          Facility/            — 3D base facility grid rendering
          Geoscape/            — 3D globe, Earth, skybox, HUD
          Statistics/          — 3D statistics scene
          XNet/                — 3D model viewer for encyclopedia
        Screens/               — 27 game screens (Gum-based)
          Battlescape/         — Battlescreen state machine files
          EquipSoldier/        — Controller + ItemSource strategy files
      Utils/                   — NLog, profiling, serialization, content cache
    Content/                   — MGCB assets
      Gum/                     — Gum .gumx project + .gusx screen layouts
  Xenocide.Test.MonoGame/      — xUnit test project
```

---

## Architecture: Screen Partitioning (3-Layer Pattern)

Every game screen follows a **3-layer architecture** separating GUI, game logic, and rendering:

```
┌─────────────────────────────────────────────────────┐
│  GUI Layer (Screen class, partial)                  │
│  - Gum controls, grid setup, event wiring           │
│  - Delegates button clicks to controller            │
│  - Updates GUI in response to controller results    │
├─────────────────────────────────────────────────────┤
│  Controller Layer (nested class, partial)           │
│  - All game state queries and mutations             │
│  - Business rules, validation, computation          │
│  - Zero Gum/MonoGame GUI references                 │
│  - Unit-testable without GUI framework              │
├─────────────────────────────────────────────────────┤
│  Scene Layer (separate class, for 3D screens only)  │
│  - 3D rendering, camera, projection                 │
│  - Mouse/keyboard input handling                    │
│  - No knowledge of GUI controls                     │
└─────────────────────────────────────────────────────┘
```

### Class Hierarchy

```
Frame (abstract)                         — lifecycle hooks, CeguiId
  └── Screen (abstract)                  — Update(), Draw(), BackgroundFilename
        └── GumScreen (abstract)         — Gum .gusx loading, CreateGumControls()
              └── PolarScreen (abstract) — 3D scene, viewport, mouse handling
```

### File Organization Rules

| Logic Size | Organization | Example |
|-----------|-------------|---------|
| < 150 lines | Nested class in same file | `MonthlyReportScreen` |
| 150-300 lines | Nested class in separate file via `partial class` | `ResearchScreen` + `Research/ResearchScreenController.cs` |
| 3+ files / 4+ modes | Subdirectory with partial class files | `Manufacture/` (4 files), `AssignToCraft/` (2 files) |

### Screen Inventory

| Screen | Controller Pattern | Scene? | Files |
|--------|-------------------|--------|-------|
| `GeoscapeScreen` | Nested `ScreenState` (state machine) | `GeoscapeScene` | 3 |
| `BattlescapeScreen` | Nested `ScreenState` (state machine) | `BattlescapeScene` | 7 |
| `BasesScreen` | Nested controller + `BasesScreenController` | `FacilityScene` | 3 |
| `EquipSoldierScreen` | Nested `Controller` (strategy pattern) | `EquipSoldierScene` | 6 |
| `XNetScreen` | None needed | `XNetScene` | 2 |
| `StatisticsScreen` | None needed | `StatisticsScene` | 2 |
| `ResearchScreen` | Nested `ResearchController` | None | 2 |
| `ManufactureScreen` | Nested `Controller` + 3 LineItem files | None | 5 |
| `AssignToCraftScreen` | Nested `Controller` | None | 2 |
| `EquipCraftScreen` | Nested `Controller` | None | 1 |
| `BaseInfoScreen` | Nested `Controller` | None | 2 |
| `AeroscapeScreen` | Nested `DogfightController` | None | 2 |
| `LoadSaveGameScreen` | Nested `SaveFileManager` | None | 2 |
| `PurchaseScreen` | Nested `Controller` | None | 1 |
| `SellScreen` | Nested `Controller` | None | 1 |
| `MakeTransferScreen` | Nested `Controller` | None | 1 |
| `SoldiersListScreen` | Nested `Controller` | None | 2 |
| `StartScreen` | Nested `DebugHelper` | None | 1 |
| `MonthlyReportScreen` | Doc comments only (small) | None | 1 |
| `MonthlyCostsScreen` | Doc comments only (small) | None | 1 |
| `BattlescapeReportScreen` | Doc comments only (small) | None | 1 |
| `SettingsScreen` | Doc comments only (small) | None | 1 |
| `CreditsScreen` | Custom 2D rendering | None | 1 |
| `ShowTransfersScreen` | Display only | None | 1 |
| `StoresScreen` | Display only | None | 1 |

### How to Add a New Screen

1. Create `Source/UI/Screens/MyNewScreen.cs` extending `GumScreen`
2. Implement `CreateGumControls()` — wire Gum buttons via `WireButton(name, handler)`
3. Add a `.gusx` layout in `Content/Gum/` matching the `CeguiId` string
4. Register the screen in `Content/Gum/GumProject.gumx`
5. If game logic > 150 lines, extract a nested `Controller` class via `partial class`
6. If 3D rendering is needed, create a `Scene` class in `Source/UI/Scenes/`
7. Schedule via `ScreenManager.ScheduleScreen(new MyNewScreen())`

### How to Extract a Controller

```csharp
// BEFORE: everything in one class
public class MyScreen : GumScreen
{
    protected override void CreateGumControls()
    {
        FindIdleScientists();       // game logic
        WireButton("addBtn", OnAdd); // GUI
    }
    private void FindIdleScientists() { /* business logic */ }
    private void OnAdd(object s, EventArgs e) { FindIdleScientists(); UpdateGrid(); }
}

// AFTER: GUI + Controller separated
// MyScreen.cs (GUI layer)
public partial class MyScreen : GumScreen
{
    private Controller controller;
    protected override void CreateGumControls()
    {
        controller = new Controller(Xenocide.GameState);
        controller.FindIdleScientists();
        WireButton("addBtn", (s, e) => { controller.AddScientist(); RefreshGrid(); });
    }
}

// MyScreen/Controller.cs (Logic layer)
public partial class MyScreen
{
    private class Controller
    {
        private readonly GameState gameState;
        public Controller(GameState gs) { gameState = gs; }
        public void FindIdleScientists() { /* pure business logic */ }
        public void AddScientist() { /* game state mutation */ }
    }
}
```

---

## Game State Architecture

```
GameState (root, serialized to save files)
  ├── GeoData          — geoscape data
  │    ├── Planet      — globe geography (regions, cities, countries)
  │    ├── Overmind    — alien AI state
  │    ├── Outposts    — X-Corp and alien bases
  │    ├── Ufos        — active UFO craft
  │    └── XCorp       — player organization (bank, tech, personnel)
  ├── Battlescape      — current tactical mission (null on geoscape)
  └── MessageLogEntries — in-game event messages
```

### Save/Load

- `GameStateSerializer.Save(Stream, GameState, version)` → JSON
- `GameStateSerializer.Load(Stream, version, out error)` → deserialized GameState
- `ModelJsonConverter` — auto-discovers model types, handles polymorphism via `$type`
- Format: `{ formatVersion, savedAt, gameTime, gameVersion, gameState }`

---

## AI / Mission System

```
Overmind (monthly strategic planning)
  └─ InvasionTask (manages a sequence of UFO launches)
       ├── BuildOutpostTask    → BuildOutpostMission
       ├── InfiltrationTask    → InfiltrationMission
       ├── ResearchTask        → ResearchMission
       ├── RetaliationTask     → RetaliationMission
       ├── SupplyOutpostTask   → SupplyOutpostMission
       └── TerrorTask          → TerrorMission
```

Each mission type uses a UFO Mission HFSM (Hierarchical Finite State Machine):
`MoveToTarget → PatrolMission → LandMission → ReturnToBase`

---

## GUI System (Gum)

### Initialization

```csharp
// In Xenocide.cs
GumService.Default.Initialize(this);        // Initialize
GumService.Default.Update(gameTime);        // Update (each frame)
GumService.Default.Draw();                  // Draw (after screen)
```

### Screen → .gusx Pattern

1. `.gusx` file defines instances (named elements with `BaseType`)
2. `.gumx` registers screen via `ScreenReference`
3. `Xenocide.GumProject.Screens.Find(name)` loads `ScreenSave`
4. `screenSave.ToGraphicalUiElement()` produces runtime visual tree
5. `WireButton(name, handler)` finds named Button and attaches Click handler

### Controls

| Control | Usage |
|---------|-------|
| `Button` | Clickable button with text |
| `Label` | Read-only text display |
| `StackPanel` | Vertical/horizontal stacking |
| `Grid` / `GridPanel` | Table-based layout |
| `ListBox` / `ComboBox` | Selection lists |
| `TextBox` | Editable text input |
| `CheckBox` / `RadioButton` | Toggle/mutual exclusion |
| `Slider` | Numeric range selection |

---

## Key Conventions

### Naming

- Screen classes: `XxxScreen` (e.g., `ResearchScreen`)
- Controller classes: nested `Controller` or `XxxController` (e.g., `ResearchController`)
- Scene classes: `XxxScene` (e.g., `GeoscapeScene`)
- State classes: nested `ScreenState` (e.g., `GeoscapeScreen.ScreenState`)
- Dialog classes: `XxxDialog` (e.g., `BuildFacilityDialog`)

### File Organization

- Screens go in `Source/UI/Screens/`
- Complex screens get a subdirectory (e.g., `Source/UI/Screens/Research/`)
- Scenes go in `Source/UI/Scenes/` with a subdirectory per screen
- Dialogs go in `Source/UI/Dialogs/`
- Model classes go in `Source/Model/` with subdirectories per domain

### Code Style

- Use `#region` for major sections (copyright, using statements, etc.)
- XML doc comments (`/// <summary>`) on public APIs and key methods
- Architectural comments in `<remarks>` blocks explaining design decisions
- `private` fields with camelCase, no underscore prefix (unless existing convention)
- `protected` properties with PascalCase
- `static readonly` for constants

### Testing

- xUnit.net 2.9.2 with `[Fact]` and `[Theory]` attributes
- Test project: `src/Xenocide.Test.MonoGame/`
- Model-layer tests are the primary focus (game logic validation)
- Run: `dotnet test src/Xenocide.Test.MonoGame`

### Content Pipeline

- Assets registered in `Content/Content.mgcb`
- Models: `.fbx` / `.x` → MGCB → `.xnb` (via `FbxImporter` / `XImporter`)
- Shaders: `.fx` → MGCB → `.xnb` (via `EffectImporter`, D3D11 profile)
- Textures: `.jpg` / `.png` → MGCB → `.xnb` (via `TextureImporter`)
- Fonts: `.spritefont` → MGCB → `.xnb` (via `SpriteFontImporter`)
- Audio: `.ogg` → MGCB → `.xnb` (via `OggImporter` + `SoundEffectProcessor`)
- Content loaded at runtime: `Content.Load<Texture2D>("path/without/Content/prefix")`

---

## Documentation Map

| Document | Location | Purpose |
|----------|----------|---------|
| `AGENTS.md` | Root | This file — comprehensive agent guide |
| `MIGRATION.md` | Root | Full migration plan and progress (XNA → MonoGame) |
| `README.md` | Root | Project overview, build instructions |
| `docs/ARCHITECTURE.md` | docs/ | System architecture (screens, AI, state, UI) |
| `docs/GUI.md` | docs/ | Gum GUI framework documentation |
| `docs/DIALOG.md` | docs/ | Dialog & message system architecture |
| `docs/LOGGING.md` | docs/ | NLog logging architecture |
