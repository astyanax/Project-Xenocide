# Architecture

## Screen Lifecycle

```
Program.cs              Xenocide.cs              ScreenManager           GumScreen
    │                       │                        │                      │
    ├─new Xenocide() ──────►│                        │                      │
    │                       ├─Initialize()           │                      │
    │                       │  ├─GumService.Init()   │                      │
    │                       │  └─ScheduleScreen ────►├─ScheduleScreen()     │
    │                       │                        │                      │
    │                       ├─LoadContent()          │                      │
    │                       │  ├─PreloadGeoscape     │                      │
    │                       │  └─PreloadXNetModels   │                      │
    │                       │                        │                      │
    │                       ├─Update() ─────────────►├─Update()             │
    │                       │                        │  └─SwapScreens() ───►├─Show()
    │                       │                        │                      │  ├─LoadGumxLayout()
    │                       │                        │                      │  ├─CreateGumControls()
    │                       │                        │                      │  └─WireButton()/AddChild()
    │                       │                        │                      │
    │                       └─Draw() ───────────────►├─Draw()               │
    │                                                │  └─ActiveScreen.Draw │
```

**Key classes:**
- `ScreenManager` — owns the screen stack, manages scheduling/push/pop, holds the dialog queue
- `GumScreen` — base for all Gum-based screens; loads `.gusx` layout from `Xenocide.GumProject`
- `PolarScreen` → `GeoscapeScreen` — 3D globe view with Gum overlay (date, funds, buttons)
- `GumDialog` → dialogs load from `.gusx` layouts; `ModalDialog` → programmatic fallback

## AI / Mission System

```
Overmind (monthly strategic planning)
  │
  ├─StartOfMonth() → TaskFactory.Create(type, overmind, region) → InvasionTask
  │
  └─ InvasionTask (manages a sequence of UFO launches)
       │
       ├─ BuildOutpostTask    → BuildOutpostMission    → UfoMission HFSM
       ├─ InfiltrationTask    → InfiltrationMission    → UfoMission HFSM
       ├─ ResearchTask        → ResearchMission        → UfoMission HFSM
       ├─ RetaliationTask     → RetaliationMission     → UfoMission HFSM
       ├─ SupplyOutpostTask   → SupplyOutpostMission   → UfoMission HFSM
       └─ TerrorTask          → TerrorMission          → UfoMission HFSM
```

**UFO Mission HFSM (Hierarchical Finite State Machine):**
```
UfoMission (abstract)
  └─ State (abstract)
       └─ MoveToTarget    → UFO flies to destination
       └─ PatrolMission   → UFO patrols area
       └─ LandMission     → UFO lands, creates battlescape
       └─ ReturnToBase    → UFO exits map
```

**Key classes:**
- `Overmind` — top-level alien AI, monthly planning, manages tasks/UFOs/sites
- `TaskFactory` — builds `InvasionTask` instances with `TaskPlan` (UFO launch schedules)
- `InvasionTask` — schedules UFO launches via `Appointment`, assigns missions to UFOs
- `AlienMission` enum — 8 types: Research, Harvest, Abduction, Infiltration, Outpost, Terror, Retaliation, Supply

## Game State

```
GameState (root object, serialized to save files)
  ├─ GeoData          — geoscape data (Planet, Outposts, Overmind, Ufos, XCorp)
  │    ├─ Planet      — globe geography (regions, cities, countries)
  │    ├─ Overmind    — alien AI state
  │    ├─ Outposts    — X-Corp and alien bases
  │    ├─ Ufos        — active UFO craft
  │    └─ XCorp       — player organization (bank, tech, personnel)
  ├─ Battlescape      — current tactical mission (null when on geoscape)
  └─ MessageLogEntries — in-game event messages (persisted)
```

**Save/Load:**
- `GameStateSerializer.Save(Stream, GameState, version)` → JSON
- `GameStateSerializer.Load(Stream, version, out error)` → deserialized GameState
- `ModelJsonConverter` — auto-discovers model types, handles polymorphism via `$type`
- `Vector3DictionaryConverter` — handles `Dictionary<Vector3, T>` keys

## UI System

```
GumService.Default (Singleton, initialized in Xenocide.Initialize)
  │
  ├─ Root (GraphicalUiElement) — top-level visual tree
  │    └─ screen.AddToRoot() appends screen root
  │
  ├─ Xenocide.gumx — Gum project file
  │    ├─ ScreenReference → Screen.gusx (XML layout)
  │    ├─ ComponentReference → reusable components (XenocideButton, DialogPanel, etc.)
  │    └─ BehaviorReference → interaction behaviors
  │
  └─ Gum.Forms — WPF-style controls
       ├─ Button      → maps to XenocideButton component
       ├─ Label       → plain text
       ├─ StackPanel  → vertical/horizontal stacking
       ├─ ListBox     → scrolling item list
       └─ ItemsControl → data-template items
```

**Screen → .gusx pattern:**
1. `.gusx` file defines instances (named elements with `BaseType`)
2. `.gumx` registers screen via `ScreenReference`
3. `Xenocide.GumProject.Screens.Find(name)` loads `ScreenSave`
4. `screenSave.ToGraphicalUiElement()` produces runtime visual tree
5. `WireButton(name, handler)` finds named Button and attaches Click handler

**Modal Dialog pattern (programmatic fallback):**
1. `ModalDialog.BuildPanel()` → creates StackPanel shell
2. `ModalDialog.BuildTitleBar()` → title label + close button
3. Subclass `CreateDialogWidgets()` → adds content-specific controls
