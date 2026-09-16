# Gum GUI Framework

## Overview

**Gum** is a general-purpose UI layout engine and framework for MonoGame and other .NET game runtimes. It was created by Victor Chelaru (FlatRedBall engine author) and consists of two parts:

1. **Gum UI Tool** — standalone WYSIWYG editor for visually designing UI layouts (drag-drop, live preview, component inheritance, state machines)
2. **Gum Runtime Library** (`Gum.MonoGame` NuGet package) — loads `.gumx` project files or works purely in code; renders via `GumService.Default`

Gum was officially adopted by **MonoGame.Extended** in 2024 as their recommended GUI solution, replacing the older `MonoGame.Extended.Gui` module. The official MonoGame documentation includes a dedicated Gum tutorial (`Chapter 20: Implementing UI with Gum`).

### Why Gum Was Chosen for Project Xenocide

After evaluating five GUI candidates — Gum, MGUI, Myra, GeonBit.UI, and ImGui.NET — Gum scored decisively highest (9.40/10 weighted) based on:

| Factor | Weight | Gum Score |
|--------|--------|-----------|
| **Visual designer** | 30% | 10 — Gum UI Tool is the only candidate with a WYSIWYG editor |
| **Data binding** | 20% | 9 — MVVM via `SetBinding`, `BindingContext`, `INotifyPropertyChanged` |
| **Cross-platform** | 15% | 10 — Single NuGet, works on Windows, Linux, macOS, mobile |
| **Documentation** | 15% | 10 — Full docs site + official MonoGame tutorial |
| **Community** | 10% | 8 — 40 contributors, monthly releases, MonoGame.Extended official |
| **Control richness** | 5% | 7 — ~20 controls, covering all needed widget types |
| **Integration ease** | 5% | 9 — One NuGet package, 3 lines of setup code |

The runner-up (MGUI, 4.05/10) has a richer control set (TabControl, Expander, GroupBox) and WPF-parity XAML, but lacks a visual designer, has no NuGet package, requires manual cross-platform configuration, and has only 2 contributors.

### Key Features

- **WYSIWYG editor** — create screens by drag-dropping, moving, and resizing objects with the mouse
- **Component inheritance** — reusable components with base/derived relationships
- **State machines** — visual states (Enabled, Disabled, Highlighted, Pushed) auto-managed by controls
- **Flexible layout engine** — anchoring, docking (`Fill`, `Left`, `Top`, `Right`, `Bottom`), `StackPanel` (horizontal/vertical), `Grid`, percentage-based sizing, `RelativeToChildren` (auto-size)
- **MVVM data binding** — `SetBinding()`, `BindingContext` inheritance, `INotifyPropertyChanged`, `IValueConverter`, `DependsOn` attributes, lambda-based binding for compile-time safety
- **20+ Forms controls** — Button, CheckBox, ComboBox, Grid, ItemsControl, Label, ListBox, Menu, PasswordBox, RadioButton, ScrollBar, ScrollViewer, StackPanel, Slider, Splitter, TextBox, Window
- **Code-only or designer** — use the Gum UI Tool for initial design, tweak in code, or go fully code-only
- **Cross-platform** — works on MonoGame DesktopGL (Windows, Linux, macOS), DirectX, Android, iOS; also with FNA, Kni, Nez, SkiaSharp, Silk.NET, raylib
- **V3 visuals** (November 2025) — simplified styling with new color properties, better consistency between code-only and designer

### Controls

Gum Forms controls are in the `MonoGameGum.Forms.Controls` namespace. Every control inherits from `FrameworkElement` and provides:

- `IsEnabled` — disable input
- `IsFocused` — keyboard focus tracking
- Layout shortcuts (`X`, `Y`, `Width`, `Height`, `Anchor`, `Dock`)
- Data binding via `SetBinding` and `BindingContext`

Available controls:

| Control | Description |
|---------|-------------|
| `Button` | Clickable button with text, supports Click event |
| `CheckBox` | Toggleable true/false state |
| `ComboBox` | Collapsible option selector |
| `Grid` | Table-based layout with rows/columns |
| `ItemsControl` | Data-bound collection display |
| `Label` | Read-only text display |
| `ListBox` | Scrollable item selection list |
| `Menu` / `MenuItem` | Menu bars and entries |
| `PasswordBox` | Masked text input |
| `RadioButton` | Mutually exclusive option selector |
| `ScrollBar` / `ScrollViewer` | Scrollable content areas |
| `Slider` | Numeric value selection from a range |
| `Splitter` | Resizable panel splitter |
| `StackPanel` | Vertical or horizontal sequential layout |
| `TextBox` | Editable text input (single/multi-line) |
| `Window` | Framed, draggable window container |

### Layout System

Gum uses an anchoring/docking system similar to WPF but designed for game rendering:

- **Anchoring** — position an element relative to parent edges (`Anchor.TopLeft`, `Anchor.Center`, `Anchor.BottomRight`, etc.)
- **Docking** — fill available space (`Dock.Fill`, `Dock.Left`, `Dock.Top`, `Dock.Right`, `Dock.Bottom`)
- **StackPanel** — horizontal or vertical stacking with `Spacing` between children
- **Grid** — row/column-based layout with absolute or star (`*`) sizing
- **Percentage sizing** — `Width = 50` (pixels) or percentage via `WidthUnits = DimensionUnitType.Percentage`
- **Auto-sizing** — `RelativeToChildren` makes parent wrap to fit children

### Data Binding (MVVM)

Gum uses WPF-style MVVM:

```csharp
// ViewModel (any INotifyPropertyChanged works; Gum.Mvvm.ViewModel provides convenience)
class SettingsViewModel : ViewModel
{
    public int SfxVolume
    {
        get => Get<int>();
        set => Set(value);
    }
}

// Binding in screen code
viewModel = new SettingsViewModel();
panel.BindingContext = viewModel;

sfxSlider.SetBinding(nameof(Slider.Value), nameof(SettingsViewModel.SfxVolume));
```

Features:
- `BindingContext` inherited by children (like WPF `DataContext`)
- `IValueConverter` support
- `BindingMode.OneWay`, `TwoWay`, `OneWayToSource`
- `StringFormat`, `FallbackValue`, `TargetNullValue`
- Nested property paths (`Player.Name`)
- `DependsOn` attribute for computed properties
- Lambda binding for compile-time safety: `SetBinding<VM>(nameof(ctrl.Prop), vm => vm.Property)`

---

## Quick-Start Tutorial: Integrating Gum into Xenocide

### Step 1: Add the NuGet Package

```bash
dotnet add src/Xenocide.MonoGame/Xenocide.MonoGame.csproj package Gum.MonoGame
```

This installs `Gum.MonoGame` and its dependency `GumCore`.

### Step 2: Initialize Gum in Xenocide.cs

Add these three calls to the game class:

```csharp
using MonoGameGum;
using Gum.Forms;

namespace ProjectXenocide
{
    public class Xenocide : Game
    {
        private GumService GumUI => GumService.Default;

        protected override void Initialize()
        {
            // ... existing init code ...

            GumUI.Initialize(this);  // code-only mode (no .gumx file)

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            GumUI.Update(gameTime);  // must come before or after screenManager.Update
            // ... existing update code ...
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            // ... existing draw code (screenManager.Draw) ...
            GumUI.Draw();  // draw Gum UI on top of everything
        }
    }
}
```

> **Note on render order**: `GumUI.Draw()` should be called after `screenManager.Draw()` so Gum renders on top of the 3D scene. Alternatively, call it inside the active screen's `Draw()` method for per-screen Gum rendering.

### Step 3: Create a Simple Screen (Code-Only)

```csharp
using MonoGameGum.Forms.Controls;

public class GumStartScreen
{
    private StackPanel rootPanel;
    private Button startButton;

    public void Initialize()
    {
        rootPanel = new StackPanel();
        rootPanel.AddToRoot();  // attaches to Gum's root

        var title = new Label();
        title.Text = "Project Xenocide";
        title.FontScale = 2.0f;
        rootPanel.AddChild(title);

        startButton = new Button();
        startButton.Text = "New Game";
        startButton.Click += OnNewGameClicked;
        rootPanel.AddChild(startButton);

        var loadButton = new Button();
        loadButton.Text = "Load Game";
        loadButton.Click += OnLoadGameClicked;
        rootPanel.AddChild(loadButton);
    }

    private void OnNewGameClicked(object sender, EventArgs e)
    {
        // Start new game logic
    }

    private void OnLoadGameClicked(object sender, EventArgs e)
    {
        // Show load game screen
    }

    public void Destroy()
    {
        GumService.Default.Root.Children.Clear();
    }
}
```

### Step 4: Using the Gum UI Tool (Designer Mode)

1. **Download** the Gum UI Tool from [GitHub releases](https://github.com/vchelaru/Gum/releases)
2. **Create** a new Gum project, save it as `Content/Gum/Xenocide.gumx`
3. **Add Forms components**: In Gum, select `Content → Add Forms Components`
4. **Design** screens visually by drag-dropping controls
5. **Load** in game:

```csharp
GumUI.Initialize(this, "Gum/Xenocide.gumx");
```

6. **Access controls** in code:

```csharp
var screenRuntime = GumService.Default.Root;
var button = screenRuntime.GetFrameworkElementByName<Button>("StartButton");
button.Click += (s, e) => { /* ... */ };
```

### Step 5: Data-Bound Example

```csharp
// ViewModel
class MainMenuViewModel : ViewModel
{
    public string VersionText
    {
        get => Get<string>();
        set => Set(value);
    }
    public bool IsDebugBuild
    {
        get => Get<bool>();
        set => Set(value);
    }
}

// Screen code
var vm = new MainMenuViewModel();
vm.VersionText = Xenocide.CurrentVersion;
rootPanel.BindingContext = vm;

versionLabel.SetBinding(nameof(Label.Text), nameof(MainMenuViewModel.VersionText));
debugPanel.SetBinding(nameof(StackPanel.Visible), nameof(MainMenuViewModel.IsDebugBuild));
```

---

## Migration Status (Completed)

All 24 game screens and CeGui# stubs have been converted to Gum. See `MIGRATION.md` items 2-3 and 10-14 for details.

### Architecture (GumScreen.Show pipeline)

```
GumScreen.Show()
  ├── TryLoadScreenFromGumx(CeguiId)
  │     └── gumProject.Screens.Find(s => s.Name == screenName)?.ToGraphicalUiElement()
  ├── GumRoot ≠ null → AddToRoot(), CreateGumControls() wires named controls
  └── GumRoot == null → throws (EnableProgrammaticFallback defaults to false)
```

### CeGUI# to Gum Mapping

| CeGUI# Widget | Gum Equivalent | Notes |
|---------------|----------------|-------|
| `PushButton` | `Button` | Click event directly maps |
| `StaticText` | `Label` | Text property maps directly |
| `StaticImage` | `SpriteRuntime` or `ColoredRectangleRuntime` | Use as `Visual` property |
| `EditBox` | `TextBox` | Text/Accepted events map |
| `Checkbox` | `CheckBox` | Checked/IsChecked map |
| `ComboBox` | `ComboBox` | Items + SelectedItem |
| `Listbox` | `ListBox` | Items + SelectedObject |
| `MultiColumnList` | `ListBox` + `Grid` or custom `ItemsControl` | Most complex mapping |
| `Slider` | `Slider` | Value/Minimum/Maximum |
| `FrameWindow` | `Window` | Draggable window |
| `Menubar`/`PopupMenu`/`MenuItem` | `Menu` + `MenuItem` | Same concepts |
| `Tooltip` | Custom (use `Window` or `Label`) | No built-in tooltip in Gum |

### CeguiId → .gumx Name Mapping

| CeguiId | .gumx Screen Name |
|---------|-------------------|
| `StartScreen` | `StartScreen` |
| `AeroscapeScreen` | `AeroscapeScreen` |
| `BattlescapeScreen` | `BattlescapeScreen` |
| `GeoscapeScreen` | `GeoscapeScreen` |
| `XNetScreen` | `XNetScreen` |
| `BasesScreen` | `BasesScreen` |
| All others | Must match CeguiId — all 23 match |

### Per-Screen Positioning Fixes

Programmatic controls in the GumX path had no X/Y positioning, causing overlap at (0,0). Fixed in:

| Screen | Controls Positioned |
|--------|-------------------|
| ManufactureScreen | availableText, projectGrid, requirementsGrid |
| SellScreen | fundsText, totalValueText, grid |
| PurchaseScreen | fundsText, totalCostText, grid |
| StoresScreen | grid |
| MakeTransferScreen | sourceText, totalCostText, outpostsListComboBox, grid |
| ShowTransfersScreen | grid |
| MonthlyCostsScreen | grid |
| BaseInfoScreen | outpostsListComboBox, nameEditBox, staffGrid, facilitiesGrid |
| EquipCraftScreen | baseNameText, pod1Text, pod2Text, craftGrid, weaponsGrid |
| AssignToCraftScreen | baseNameText, craftGrid, soldierGrid, xcapGrid |
| StatisticsScreen | seriesList |
| BattlescapeReportScreen | recoveredLabelText, scoreGrid, recoveredGrid |
| SoldiersListScreen | psiTrainButton, nameEditBox, attributesGrid, soldiersListGrid |
| EquipSoldierScreen | ammoText, 8 static labels |
| LoadSaveGameScreen | filenameEditBox, savesgrid |

### Gum API Pitfalls & Practical Learnings

These lessons were learned the hard way during the AeroscapeScreen implementation.

#### .gusx `BaseType="Text"` Is NOT a Forms Label

Every `.gusx` file in the project defines text elements as `<Instance Name="foo" BaseType="Text" />`. This creates a **visual-only** `GraphicalUiElement` that renders text — it is NOT a `Gum.Forms.Controls.Label` (Forms control).

**What crashes:**
```csharp
// CRASHES — "Text" visual has no FormsControlAsObject
var label = GumRoot.GetFrameworkElementByName<Label>("statusLabel");
label.Text = "hello";
```

**What works — use the raw visual API instead:**
```csharp
var elem = GumRoot.GetGraphicalUiElementByName("statusLabel");
elem.SetProperty("Text", "hello");
```

The `GetFrameworkElementByName<T>()` method requires `FormsControlAsObject` to be set on the element, which only happens for elements created via the Gum.Forms API (`new Label()`, `new Button()`, etc.), not for designer elements with `BaseType="Text"`.

**Why `GetFrameworkElementByName<Button>()` works but `<Label>()` doesn't:** Buttons use `BaseType="XenocideButton"` (a custom Forms component) or the Gum V3 `Button` visual, which registers a Forms control. Text labels use `BaseType="Text"` with no Forms control registration.

**Two patterns for reading text from .gusx:**

| Pattern | Code | Used By |
|---------|------|---------|
| `GetGraphicalUiElementByName` + `SetProperty` | `elem.SetProperty("Text", value)` | AeroscapeScreen |
| Create programmatically | `var label = new Label(); ... label.Text = value;` | GeoscapeScreen, BattlescapeScreen |

#### Button `Color` Is Not a Direct Property

`Gum.Forms.Controls.Button` (and its `.Visual` which is `InteractiveGue`) does not have a `Color` property. Button styling can be done through:

1. **Text-based indication** — Change `button.Text` to show active state (e.g., `[STANDARD]` vs `STANDARD`). This is what AeroscapeScreen uses.
2. **`SetProperty("ColorCategoryState", "Primary")`** — Apply a predefined Gum color style category. Used by `GridPanel` and `ModalDialog` for title bars.
3. **V3 `ButtonVisual.BackgroundColor`** — The V3 default visual exposes `BackgroundColor`, `ForegroundColor`, and `FocusedIndicatorColor`. Access via the V3 visual type, not the Forms `Button` wrapper.

#### Programmatic vs Designer Controls

The codebase uses two distinct patterns for text elements in screens:

1. **Designer-defined** — Text elements in `.gusx` (e.g., `BaseType="Text"`). Accessed via `GetGraphicalUiElementByName()` + `SetProperty("Text", ...)`. Layout is designer-controlled.
2. **Programmatically created** — Labels created as `new Label()` in C# code. Accessed via `.Text` property directly. Must be manually positioned and added to a parent container.

Most screens use designer-defined text for layout labels and programmatic labels for dynamic content. The AeroscapeScreen uses exclusively designer-defined text for its HUD.

---

## Base Screen & Facility System Architecture

### Overview

The `BasesScreen` (a `GumScreen` subclass) manages the layout of facilities in an X-Corp Outpost. It owns a 3D `FacilityScene`, a `SceneMouseHandler` for input, and a `FacilityTooltip` for hover information.

### Key Classes

| Class | File | Responsibility |
|-------|------|----------------|
| `BasesScreen` | `UI/Screens/BasesScreen.cs` | State machine, placement logic, Gum button wiring |
| `FacilityScene` | `UI/Scenes/Facility/FacilityScene.cs` | 3D rendering (grid, facilities, ghost, adjacency lines) |
| `SceneMouseHandler` | `UI/SceneMouseHandler.cs` | Polls mouse state, fires viewport-relative events |
| `Floorplan` | `Model/Geoscape/Outposts/Floorplan.cs` | 6×6 grid data model (facility positions, validation) |
| `FacilityHandle` | `Model/Geoscape/Outposts/FacilityHandle.cs` | Placed facility instance (X, Y, FacilityInfo, IsUnderConstruction) |
| `BuildFacilityDialog` | `UI/Dialogs/BuildFacilityDialog.cs` | Lists buildable facilities, triggers placement |
| `FacilityTooltip` | `UI/Screens/FacilityTooltip.cs` | Gum StackPanel with 5 labels showing facility info |
| `AdjacencyLines` | `UI/Scenes/Facility/AdjacencyLines.cs` | Builds colored lines (green=connected, red=overlapping) |
| `LineMesh` | `UI/Scenes/Common/LineMesh.cs` | GPU vertex/index buffer wrapper for LineList rendering |

### State Machine

```
BasesScreenState enum (defined in BasesScreen.cs):
  NotAdding      → Normal mode. Right-click demolishes. "Build Facilities" button opens BuildFacilityDialog.
  AddAccessLift  → Auto-entered when building into an empty base. Creates a ghost FacilityHandle.
  AddFacility    → User selected a facility from BuildFacilityDialog. Ghost follows cursor.
```

State transitions:

```
"Build Facilities" click (Or B key)
  ├── Base empty? → State = AddAccessLift → ghost = "FAC_BASE_ACCESS_FACILITY"
  └── Has facilities? → Show BuildFacilityDialog
                           └── User selects facility → BuildFacility(handle) → State = AddFacility

Right-click / Escape / Backspace → CancelFacility → State = NotAdding, ghost = null
```

### Placement Flow

1. **"Build Facilities" button** → `OnBuildFacilitiesButton()`
   - Base empty → `State = AddAccessLift` (auto-creates access lift ghost)
   - Has facilities → `ShowDialog(new BuildFacilityDialog(this))`

2. **BuildFacilityDialog** (ModalDialog subclass) populates `ContentArea` with one `Button` per buildable facility. Each button embeds name, cost, build days, and monthly maintenance. A Cancel button is added last.

3. **User clicks a facility** → `OnFacilitySelected(idx)` → checks `CanAfford` → checks `LimitIsOnePerOutpost` → `basesScreen.BuildFacility(handle)` → sets `scene.NewFacility` and `State = AddFacility`.

4. **Each frame**: `SceneMouseHandler.Update()` polls `Mouse.GetState()`, hit-tests the viewport rect, and fires `MouseMoved(relX, relY)` → `OnSceneMouseMoved` → `RelToCell` → `UpdateNewFacilityPosition(cell)` → sets ghost X/Y → ghost `HasPosition` becomes true.

5. **FacilityScene.Draw()** checks `null != newFacility` → calls `RebuildAdjacencyLines()` (builds green/red line indicators to neighbours) → calls `Draw(newFacility)` which renders the ghost model tinted green (valid) or red (invalid) via `IsPositionLegal`.

6. **Left-click** → `OnSceneLeftClicked` → `AddFacility(cell)`:
   - Checks `CanAfford` (guard for AddAccessLift path)
   - Validates via `Floorplan.IsPositionLegal`
   - If legal → `Bank.Debit(cost)` → `Floorplan.AddFacility(handle)` → `ScheduleScreen(new BasesScreen())` (full reload)
   - If illegal → `Util.ShowMessageBox(error)`

### Demolition Flow

1. **Right-click** → `OnSceneRightClicked`:
   - If in add mode → `CancelFacility()` first
   - Then `RemoveFacility(cell)` → validates via `CanRemoveFacility` → shows `GumYesNoDialog`
   - On yes → stores for Ctrl+Z undo, credits scrap revenue, removes from floorplan, schedules reload

### Initialization Order & The sceneWindowRect Bug

When `ScreenManager.SwapScreens()` schedules a new `BasesScreen`, the call order is:

```
ScreenManager.SwapScreens() (line 144–150):
  1. screen.LoadContent(content, device)   ← LoadContent runs FIRST
  2. screen.Show()                          ← Show runs SECOND
```

`BasesScreen.LoadContent()` creates the `SceneMouseHandler`, which receives `sceneWindowRect` **by value** (UiRect is a struct). But `sceneWindowRect` was historically initialized only in `CreateGumControls()`, which is called from `Show()` — which hadn't run yet!

**The fix**: `sceneWindowRect` is initialized at field declaration in `BasesScreen.cs`:
```csharp
private UiRect sceneWindowRect = new UiRect(0.02f, 0.073f, 0.661f, 0.9264f);
```
This ensures the mouse handler is created with the correct non-zero viewport. The assignment in `CreateGumControls` is kept as a documentation safety net.

**Without this fix**: The mouse handler has a zero-area viewport (Left=Top=Right=Bottom=0), so `inViewport` is always false, and no `MouseMoved`/`LeftClicked`/`RightClicked` events ever fire. The facility ghost never appears and placement clicks are silently swallowed.

### Dialog Layout Notes

Dialogs are **programmatic** — there are no dialog `.gusx` files. Every dialog derives
from `ModalDialog` (`Source/UI/Dialogs/ModalDialog.cs`), which builds its panel, title bar
and content area in code. `BuildFacilityDialog` therefore has no `.gusx` layout; its buttons
are created in `CreateDialogWidgets()`.

Layout conventions in Gum for `DimensionUnitType.RelativeToParent`:
- `0` = fill remaining space (100% of parent minus fixed-size siblings)
- Negative values (e.g. `-28`) = parent dimension minus that many pixels (e.g. `Height = 100% - 28px`)

### Logging Levels

All facility placement classes use NLog with three levels:
- **INFO**: State transitions, facility placement/cancellation/demolition, affordability, important events
- **DEBUG**: Edge-detect resets, button counts, mouse handler lifecycle
- **TRACE**: Every mouse move, cell projection, ghost position update, IsPositionLegal checks

### Gum Runtime Type Hierarchy (Reflection Findings)

Understanding Gum's internal type hierarchy is critical for programmatic UI construction. The two layers — **Wireframe** (low-level visual tree) and **Forms** (high-level controls) — have different rules for creating containers.

#### The Core Problem: Bare `GraphicalUiElement` Has No Runtime

```csharp
var gue = new GraphicalUiElement();  // NO runtime
gue.Children.Add(child);             // THROWS: read-only collection
```

`new GraphicalUiElement()` creates a visual element without a `RuntimeObject`. Without a runtime, the `Children` collection (`GraphicalUiElementCollection`) is **read-only** and throws `InvalidOperationException` on any mutation attempt. Adding the bare GUE to `GumService.Default.Root.Children` does NOT fix this — the parent-child relationship is established but the child's own `Children` remains read-only.

#### Container Types Comparison

| Type | Namespace | Has Runtime | Children Writable | Auto-Layout | Use Case |
|------|-----------|:-----------:|:-----------------:|:-----------:|----------|
| `GraphicalUiElement` | `Gum.Wireframe` | **No** | **No** | No | Base class only — never instantiate directly |
| `ContainerRuntime` | `MonoGameGum.GueDeriving` | Yes | **Yes** | **No** | **Recommended** — invisible container, manual positioning |
| `InteractiveGue` | `MonoGameGum.GueDeriving` | Yes | Yes | No | Interactive container (receives mouse/keyboard input) |
| `StackPanel` | `Gum.Forms.Controls` | Yes | Yes (via `AddChild`) | **Yes** (vertical/horizontal) | Sequential layout of Form controls |
| `Panel` | `Gum.Forms.Controls` | Yes | Yes (via `AddChild`) | Yes | Base Forms panel (StackPanel inherits from this) |

#### Correct Pattern: `ContainerRuntime`

```csharp
using MonoGameGum.GueDeriving;

var container = new ContainerRuntime();
container.Width = 100;
container.WidthUnits = Gum.DataTypes.DimensionUnitType.PercentageOfParent;
container.Height = 100;
container.HeightUnits = Gum.DataTypes.DimensionUnitType.PercentageOfParent;

// Children are fully writable — manual positioning
container.Children.Add(childGue);
// OR
container.AddChild(childGue);

// Add to root
GumService.Default.Root.Children.Add(container);
```

`ContainerRuntime` is the concrete type that backs the `Container` BaseType in `.gusx` files. It creates a proper invisible container renderable with manual (absolute) positioning — no auto-layout engine processes its children.

#### Incorrect Patterns

```csharp
// WRONG: bare GUE — Children is read-only
var root = new GraphicalUiElement();
root.Children.Add(child);  // InvalidOperationException

// WRONG: StackPanel auto-arranges children vertically
var root = new StackPanel();
GumService.Default.Root.Children.Add(root.Visual);
root.Visual.Children.Add(child);  // child position may be overridden by layout

// WRONG: ScreenSave on a programmatic element — needs project context
var screenSave = new ScreenSave();
screenSave.Instances.Add(...);
var gue = screenSave.ToGraphicalUiElement();  // may fail without GumProject
```

#### When to Use Each Type

| Scenario | Type | Why |
|----------|------|-----|
| Root container for a programmatic screen layout | `ContainerRuntime` | Writable Children, no auto-layout, matches .gusx Container behavior |
| Vertical/horizontal list of controls | `StackPanel` | Built-in stacking, spacing, orientation |
| Adding a Forms control (Button, Label, etc.) | Call `.AddChild(control)` on parent | FrameworkElement.AddChild handles Visual tree integration |
| Adding a raw GUE to a parent | `parent.Children.Add(gue.Visual)` | Direct Visual tree manipulation (parent must have runtime) |

#### The Runtime Rule

A `GraphicalUiElement` has writable `Children` **if and only if** it was created through a type that initializes a `RuntimeObject` in its constructor. All `FrameworkElement` subclasses (Button, Label, StackPanel, etc.) and `ContainerRuntime`/`InteractiveGue` do this. The bare `GraphicalUiElement()` constructor does not.

### Remaining Gum Backlog

- Software cursor polish — the `Select` bracket is shown over clickable controls; a dedicated hand-cursor frame could replace it later (the atlas has unassigned cursor frames at x=118/214/238).
- ViewportMode `FullScene` — ✅ Done. `ScreenLayout` supports `Standard`/`SplitViewport`/`FullScene`; EquipSoldierScreen adopts `FullScene`, and the Aeroscape HUD is anchored responsively via `ResponsiveHud` (design resolution 1280×1024).

---

## UI Component Architecture (Phase 4.7)

### Overview

The project provides a set of reusable UI components that standardize screen layout, typography, theming, and data grid presentation. These components eliminate hardcoded pixel positioning and ensure visual consistency across all screens.

### Components

| Component | File | Purpose |
|-----------|------|---------|
| `ScreenLayout` (.cs) | `Source/UI/Controls/ScreenLayout.cs` | Standard screen structure: scrollable content area (75%), button bar (190px, top-right anchored), status bar (bottom). Also provides `ViewportMode` (Standard/SplitViewport) for 3D/2D scene screens |
| `ContentArea` (.cs) | `Source/UI/Controls/ContentArea.cs` | Manages dynamic content: AddHeader, AddLabel, AddGrid, AddSpacer, Clear |
| `ThemedLabel` (.cs) | `Source/UI/Controls/ThemedLabel.cs` | Factory for pre-styled Labels: `Create(text, TextStyle)` with Title/H1/H2/H3/Normal/Strong/Emphasis/Small/Tiny sizes, plus `CreateTitle`/`CreateSection`/`CreateBody`/`CreateCaption` helpers |
| `ThemedButton` (.cs) | `Source/UI/Controls/ThemedButton.cs` | Single source of truth for button creation: `Create()` (textured XenocideButton 3-slice from XenoNew.png) + `CreateFlat()` (ButtonStandard for ColorCategoryState striping); auto-wires ButtonClick1 |
| `StyledGrid` (.cs) | `Source/UI/Controls/StyledGrid.cs` | GridPanel subclass with alternating row colors, header styling, 25px rows |
| `ModalDialog` (.cs) | `Source/UI/Dialogs/ModalDialog.cs` | Dialog base class (replaces the deleted GumDialog): title bar, centered panel, `CreateDialogWidgets()`, `AddButton()` helper |

### ScreenLayout Structure

```
┌──────────────────────────────────────────────────┐
│  ScreenLayout (100% x 100%)                      │
│  ┌──────────────────────────┐ ┌────────────────┐ │
│  │ ContentScroll            │ │ ButtonBar      │ │
│  │ (ScrollViewer, 75%)      │ │ (StackPanel,   │ │
│  │ ┌──────────────────────┐ │ │  190px)        │ │
│  │ │ ContentStack         │ │ │                │ │
│  │ │ (StackPanel,         │ │ │ [Button1]      │ │
│  │ │  vertical stack)     │ │ │ [Button2]      │ │
│  │ │                      │ │ │ [Button3]      │ │
│  │ └──────────────────────┘ │ │                │ │
│  └──────────────────────────┘ └────────────────┘ │
│  ┌──────────────────────────────────────────────┐│
│  │ StatusBar (StackPanel, 30px, bottom)         ││
│  └──────────────────────────────────────────────┘│
└──────────────────────────────────────────────────┘
```

### Usage Pattern

```csharp
// In CreateGumControls():
private ScreenLayout layout;
private ContentArea content;

protected override void CreateGumControls()
{
    layout = new ScreenLayout();
    layout.AddToRoot();
    content = new ContentArea(layout.ContentPanel);

    // Buttons go to the right panel
    layout.AddButton("Research", OnResearch);
    layout.AddButton("Cancel", OnCancel);

    // Content auto-stacks vertically in the scrollable area
    content.AddHeader("Research Projects");
    fundsText = ThemedLabel.CreateBody("");
    content.AddLabel(fundsText);

    grid = new StyledGrid();
    grid.AddColumn("Name", 400);
    grid.AddColumn("Cost", 200);
    content.AddGrid(grid);
}
```

### TextStyle Enum

| Style | Size | Weight | Usage |
|-------|------|--------|-------|
| `Title` | 28px | Bold | Screen titles |
| `H1` | 22px | Bold | Major headers |
| `H2` | 18px | Bold | Section headers |
| `H3` | 16px | Bold | Sub-section headers |
| `Strong` | 14px | Bold | Button text, emphasis |
| `Normal` | 14px | Regular | Body text |
| `Emphasis` | 14px | Italic | Emphasis text |
| `Small` | 12px | Regular | Captions, secondary |
| `Tiny` | 10px | Regular | Micro labels |

### StyledGrid Features

- Alternating row colors (PrimaryLight tint for even rows)
- Header row uses DarkGray background
- Row height: 25px
- Selection highlight: Primary color
- Column width specified in pixels

### Migration Path

Each screen migration follows this template:

**Before:**
```csharp
fundsText = new Label();
fundsText.Visual.X = 20;
fundsText.Visual.Y = 20;
AddChild(fundsText);

grid = new GridPanel();
grid.Visual.X = 20;
grid.Visual.Y = 60;
grid.Visual.Width = 750;
AddChild(grid);
```

**After:**
```csharp
layout = new ScreenLayout();
layout.AddToRoot();
content = new ContentArea(layout.ContentPanel);

fundsText = ThemedLabel.CreateBody("");
content.AddLabel(fundsText);

grid = new StyledGrid();
grid.AddColumn("Name", 400);
content.AddGrid(grid);
```

---

## References

- **Gum documentation**: https://docs.flatredball.com/gum/
- **GitHub repo**: https://github.com/vchelaru/Gum
- **NuGet**: https://www.nuget.org/packages/Gum.MonoGame
- **MonoGame tutorial**: https://docs.monogame.net/articles/tutorials/building_2d_games/20_implementing_ui_with_gum/
- **Legacy analysis**: `docs/legacy/GUI.md`
- **Migration plan**: `MIGRATION.md`
