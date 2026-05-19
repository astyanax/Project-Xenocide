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
dotnet add xna/trunk/Xenocide.MonoGame/Xenocide.MonoGame.csproj package Gum.MonoGame
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

## Integration Plan

### Screen Migration Pattern

Each existing screen needs to be converted from CeGUI# to Gum:

1. **Create a ViewModel** class for screen state (e.g., `StartScreenViewModel`)
2. **Replace** `CreateCeguiWidgets()` with a Gum control tree
3. **Replace** CeGUI# event handlers (`Clicked +=`) with Gum Click events or data binding
4. **Remove** CeGUI# root widget references; Gum manages its own tree

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

### Screen Conversion Order (Recommended)

1. **StartScreen** — simplest; 6 buttons, 1 label → validate integration works end-to-end
2. **LoadSaveGameScreen** — adds list/grid and text input
3. **CreditsScreen** — static text display
4. **MonthlyReportScreen** — formatted text + buttons
5. **MessageBoxDialog** / **YesNoDialog** — dialog pattern
6. **OptionsDialog** — sliders, checkboxes, combo boxes
7. **ResearchScreen** — multi-column list + tree view
8. **BasesScreen** — tabs, lists, grids
9. **EquipSoldierScreen** — drag-drop (most complex)
10. **GeoscapeScreen** — 3D scene integration (lowest UI complexity)
11. **BattlescapeScreen** — 3D overlay UI
12. Remaining management screens

---

## References

- **Gum documentation**: https://docs.flatredball.com/gum/
- **GitHub repo**: https://github.com/vchelaru/Gum
- **NuGet**: https://www.nuget.org/packages/Gum.MonoGame
- **MonoGame tutorial**: https://docs.monogame.net/articles/tutorials/building_2d_games/20_implementing_ui_with_gum/
- **Legacy analysis**: `docs/legacy/GUI.md`
- **Migration plan**: `MIGRATION.md`
