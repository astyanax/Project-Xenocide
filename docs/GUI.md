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
2. **Create** a new Gum project, save it as `Content/GumProject/GumProject.gumx`
3. **Add Forms components**: In Gum, select `Content → Add Forms Components`
4. **Design** screens visually by drag-dropping controls
5. **Load** in game:

```csharp
GumUI.Initialize(this, "GumProject/GumProject.gumx");
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

All 27 screens and CeGui# stubs have been converted to Gum. See `MIGRATION.md` items 2-3 and 10-14 for details.

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

2. **BuildFacilityDialog** (GumDialog subclass) populates `ContentPanel` with one `Button` per buildable facility. Each button embeds name, cost, build days, and monthly maintenance. A Cancel button is added last.

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

### GUSX Layout Notes (BuildFacilityDialog)

The `BuildFacilityDialog.gusx` file defines the dialog's visual container layout: a `DialogPanel` (500×450, centered on screen) containing a `TitleBar` (28px), `DialogBackground`, and `ScrollViewerInstance` (Y=28, Height=-28). The `ContentPanel` inside the ScrollViewer has Height=0 (fill remaining space).

Layout conventions in Gum for `DimensionUnitType.RelativeToParent`:
- `0` = fill remaining space (100% of parent minus fixed-size siblings)
- Negative values (e.g. `-28`) = parent dimension minus that many pixels (e.g. `Height = 100% - 28px`)

This GUSX file CAN be loaded in the Gum UI Tool for visual layout editing — the container structure (ScrollViewer, ContentPanel, TitleBar, etc.) is all designer-defined. However, the individual facility buttons inside ContentPanel are populated **programmatically** in `WireGumControls()` (C#), so the designer shows an empty ContentPanel at design time. The layout values (heights, Y offsets) are shared between designer and runtime, which is why incorrect values (e.g. a zero-area ContentPanel) silently break the dialog at runtime.

### Logging Levels

All facility placement classes use NLog with three levels:
- **INFO**: State transitions, facility placement/cancellation/demolition, affordability, important events
- **DEBUG**: Edge-detect resets, button counts, mouse handler lifecycle
- **TRACE**: Every mouse move, cell projection, ghost position update, IsPositionLegal checks

### Remaining Gum Backlog

- Dialog `.gusx` conversion — 9 dialogs currently programmatic (4 done: MessageBox, YesNo, Options, GumOptions)
- Software cursor polish — context-sensitive cursors (hand/arrow per element), HW/SW toggle via settings
- GridPanel XenocideButton styling — `RowButtonFactory` property added to GridPanel.cs; remaining: implement flat XenocideButton visual (NineSlice-based, avoiding hierarchical GUE limitation)

---

## References

- **Gum documentation**: https://docs.flatredball.com/gum/
- **GitHub repo**: https://github.com/vchelaru/Gum
- **NuGet**: https://www.nuget.org/packages/Gum.MonoGame
- **MonoGame tutorial**: https://docs.monogame.net/articles/tutorials/building_2d_games/20_implementing_ui_with_gum/
- **Legacy analysis**: `docs/legacy/GUI.md`
- **Migration plan**: `MIGRATION.md`
