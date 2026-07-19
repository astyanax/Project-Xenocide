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

---

## Screen Partitioning (3-Layer Architecture)

Every game screen is decomposed into three layers with distinct responsibilities. This separation enables unit testing of game logic without GUI dependencies, makes screens easier to navigate, and provides a clear pattern for adding new screens.

### The Three Layers

```
┌─────────────────────────────────────────────────────┐
│  GUI Layer (Screen class, partial)                  │
│  Responsibilities:                                  │
│  - Gum control creation and layout                  │
│  - Grid initialization and row population           │
│  - Button/event wiring to controller methods        │
│  - GUI refresh after controller returns results     │
│  References: Gum.Forms, Gum.Wireframe              │
├─────────────────────────────────────────────────────┤
│  Controller Layer (nested class, partial)           │
│  Responsibilities:                                  │
│  - All game state queries (reading model data)      │
│  - All game state mutations (modifying model data)  │
│  - Business rules and validation logic              │
│  - Data computation (costs, stats, availability)    │
│  References: ProjectXenocide.Model.* ONLY           │
├─────────────────────────────────────────────────────┤
│  Scene Layer (separate class, 3D screens only)      │
│  Responsibilities:                                  │
│  - 3D rendering (camera, projection, draw calls)    │
│  - Mouse/keyboard input within viewport             │
│  - Model loading and GPU resource management        │
│  References: Microsoft.Xna.Framework.Graphics ONLY  │
└─────────────────────────────────────────────────────┘
```

### Data Flow

```
User clicks button
       │
       ▼
Screen.OnAddButton()          ← GUI layer (event handler)
       │
       ▼
Controller.AddScientist(1)    ← Controller layer (business logic)
       │
       ├── Reads: Outpost.ListStaff(), ProjectMgr.ActiveProjects
       ├── Validates: idleScientists.Count > 0
       ├── Mutates: project.AddWorker(scientist)
       └── Returns: success/failure + updated data
       │
       ▼
Screen.RefreshGrid()          ← GUI layer (updates controls)
```

### File Organization Rules

| Complexity | Pattern | Example |
|-----------|---------|---------|
| **Small** (< 150 lines logic) | Nested class in same file | `MonthlyReportScreen` — `MakeScoreString()` stays inline |
| **Medium** (150-300 lines) | Nested class in separate file via `partial class` | `ResearchScreen.cs` + `Research/ResearchScreenController.cs` |
| **Large** (3+ files / 4+ modes) | Subdirectory with partial class files | `Manufacture/` (4 files: controller + 3 LineItem types) |

### Pattern A: Nested Controller (No 3D Scene)

Used by: ResearchScreen, ManufactureScreen, AssignToCraftScreen, PurchaseScreen, SellScreen, MakeTransferScreen, EquipCraftScreen, BaseInfoScreen, AeroscapeScreen, LoadSaveGameScreen, SoldiersListScreen, StartScreen

```csharp
// ResearchScreen.cs — GUI layer
public partial class ResearchScreen : GumScreen
{
    private Controller controller;

    protected override void CreateGumControls()
    {
        // Initialize controller with model references
        controller = new Controller(ProjectMgr, TechMgr, Outposts);

        // Wire GUI events to controller methods
        WireButton("addIdleScientistsButton", (s, e) =>
        {
            controller.AddIdleScientists(1);
            RefreshGrid();
        });
    }

    private void RefreshGrid()
    {
        // Query controller for display data
        var items = controller.GetResearchLineItems();
        // Update GUI controls with results
    }
}

// Research/ResearchScreenController.cs — Controller layer
public partial class ResearchScreen
{
    /// <summary>
    /// Handles all research game logic: scientist assignment,
    /// project management, and topic validation.
    /// </summary>
    private class Controller
    {
        private readonly IProjectManager projectMgr;
        private readonly TechManager techMgr;
        private readonly OutpostManager outposts;

        public Controller(IProjectManager pm, TechManager tm, OutpostManager om)
        {
            projectMgr = pm;
            techMgr = tm;
            outposts = om;
        }

        /// <summary>
        /// Assigns idle scientists to the selected research project.
        /// </summary>
        public void AddIdleScientists(int count) { /* ... */ }

        /// <summary>
        /// Returns topics available for research, excluding already-assigned ones.
        /// </summary>
        public List<ResearchLineItem> GetResearchLineItems() { /* ... */ }
    }
}
```

### Pattern B: State Machine (Complex Navigation)

Used by: GeoscapeScreen, BattlescapeScreen

```csharp
// GeoscapeScreen.cs — GUI layer
public partial class GeoscapeScreen : PolarScreen
{
    private ScreenState state;

    public override void Show()
    {
        state = new ViewGeoscapeScreenState(this);
        state.CreateGumControls();
    }

    // Delegates all button clicks to current state
    private void OnBasesButton() => state.OnBasesButton();
    private void OnResearchButton() => state.OnResearchButton();
}

// GeoscapeScreenState.cs — Controller layer (state machine)
public partial class GeoscapeScreen
{
    public abstract class ScreenState
    {
        protected GeoscapeScreen Screen { get; }
        protected GameState GameState => Xenocide.GameState;

        public virtual void OnBasesButton() { }
        public virtual void OnResearchButton() { }
    }

    public class ViewGeoscapeScreenState : ScreenState
    {
        public override void OnBasesButton()
            => Screen.ScreenManager.ScheduleScreen(new BasesScreen());
    }

    public class TargetingScreenState : ScreenState
    {
        public override void OnBasesButton() { /* blocked during targeting */ }
    }
}
```

### Pattern C: Strategy Pattern (Multiple Modes)

Used by: EquipSoldierScreen

```csharp
// EquipSoldierScreen.cs — GUI layer
public partial class EquipSoldierScreen : PolarScreen
{
    private Controller controller;

    // Mode determined at construction
    public EquipSoldierScreen(Combatant combatant, bool inOutpost)
        : base("EquipSoldier")
    {
        controller = inOutpost
            ? new InOutpostController(this, combatant)
            : new BattlescapeController(this, combatant);
    }
}

// EquipSoldier/EquipSoldierScreenController.cs — Abstract controller
public partial class EquipSoldierScreen
{
    private abstract class Controller
    {
        public abstract void CreateGumControls();
        public abstract void OnCloseButton();
        public abstract Combatant Combatant { get; }
    }
}

// EquipSoldier/EquipSoldierScreenInOutpostController.cs — Concrete
public partial class EquipSoldierScreen
{
    private class InOutpostController : Controller
    {
        public override void OnCloseButton()
            => EquipSoldierScreen.ScreenManager.ScheduleScreen(new BasesScreen());
    }
}
```

### Pattern D: Separate Scene (3D Rendering)

Used by: GeoscapeScreen, BattlescapeScreen, BasesScreen, XNetScreen, StatisticsScreen, EquipSoldierScreen

```csharp
// GeoscapeScene.cs — Scene layer (pure rendering)
public class GeoscapeScene : PolarScene, IDisposable
{
    private EarthGlobe globe;
    private SkyBox skybox;
    private GeoHud hud;

    public void LoadContent(ContentManager content, GraphicsDevice device) { /* ... */ }
    public void Draw(GameTime gameTime, GraphicsDevice device, UiRect viewport) { /* ... */ }

    // No knowledge of Gum controls, buttons, or screen state
}
```

### How to Extract a Controller (Step-by-Step)

1. **Identify game logic methods** — find methods that query or mutate `GameState`, `Outpost`, `ProjectMgr`, `TechManager`, etc.
2. **Create the controller file** — `Source/UI/Screens/{ScreenName}/{ScreenName}Controller.cs`
3. **Make the screen partial** — add `partial` keyword to the screen class declaration
4. **Move logic methods** — cut/paste into a nested `Controller` class
5. **Add constructor** — controller receives model references (never GUI references)
6. **Update screen** — replace direct calls with `controller.MethodName()`
7. **Verify** — `dotnet build` (0 errors), `dotnet test` (all pass)

### Controller Design Rules

- **No GUI references** — controller `using` statements must NOT include `Gum.Forms`, `Gum.Wireframe`, or `MonoGameGum`
- **No static state** — controller receives dependencies via constructor, never accesses `Xenocide.Instance` directly
- **Pure returns** — controllers return data (lists, booleans, strings); screens update GUI based on results
- **Mutable game state** — controllers modify `GameState` through model APIs (`project.AddWorker()`, `outpost.Inventory.Remove()`, etc.)
- **XML doc comments** — every public/important method gets `<summary>` + `<remarks>` explaining game mechanics

### Screen Inventory (Partitioning Status)

| Screen | Pattern | Controller File | Lines (Screen/Controller) |
|--------|---------|----------------|--------------------------|
| GeoscapeScreen | B (State) | `GeoscapeScreenState.cs` | 700 / 615 |
| BattlescapeScreen | B (State) | `Battlescape/` (6 files) | 272 / 609 |
| BasesScreen | D (Scene) + enum state | `Bases/BasesScreenController.cs` | 939 / extracted |
| EquipSoldierScreen | C (Strategy) + D | `EquipSoldier/` (5 files) | 518 / 718 |
| XNetScreen | D (Scene) | None needed | 482 / — |
| StatisticsScreen | D (Scene) | None needed | 410 / — |
| ResearchScreen | A (Controller) | `Research/ResearchScreenController.cs` | ~300 / ~250 |
| ManufactureScreen | A (Controller) | `Manufacture/` (4 files) | ~280 / ~300 |
| AssignToCraftScreen | A (Controller) | `AssignToCraft/AssignToCraftScreenController.cs` | ~300 / ~250 |
| EquipCraftScreen | A (Controller) | Nested in partial class | ~300 / ~120 |
| BaseInfoScreen | A (Controller) | Nested in partial class | ~280 / ~150 |
| AeroscapeScreen | A (Controller) | Nested `DogfightController` | ~150 / ~100 |
| LoadSaveGameScreen | A (Controller) | Nested `SaveFileManager` | ~350 / ~120 |
| PurchaseScreen | A (Controller) | Nested in partial class | ~300 / ~80 |
| SellScreen | A (Controller) | Nested in partial class | ~250 / ~80 |
| MakeTransferScreen | A (Controller) | Nested in partial class | ~250 / ~80 |
| SoldiersListScreen | A (Controller) | Nested in partial class | ~350 / ~60 |
| StartScreen | A (Helper) | Nested `DebugHelper` | ~300 / ~80 |
| MonthlyReportScreen | Doc comments only | Too small to split | 214 / — |
| MonthlyCostsScreen | Doc comments only | Too small to split | 201 / — |
| BattlescapeReportScreen | Doc comments only | Too small to split | 238 / — |
| SettingsScreen | Doc comments only | Too small to split | 245 / — |
| CreditsScreen | Custom 2D rendering | No game logic | 264 / — |
| ShowTransfersScreen | Display only | No game logic | 138 / — |
| StoresScreen | Display only | No game logic | 133 / — |
