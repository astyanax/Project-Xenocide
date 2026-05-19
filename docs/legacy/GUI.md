# GUI System: Legacy Analysis & Replacement Evaluation

## 1. Overview

This document analyzes the legacy **CeGUI#** (C# port of CEGUI) GUI system used by Project Xenocide, and evaluates five replacement candidates for the MonoGame port (Phase 4 of the migration plan). The primary goal is to select a GUI framework that is easy to work with visually, maintain, and extend for a complex turn-based strategy game.

---

## 2. Legacy CeGUI# System

### 2.1 Source

CeGUI# is a C# port of the CEGUI (Crazy Eddie's GUI) library, originally targeting XNA 3.0. The project's copy lives in:

- `xna/trunk/Dependancies/CeGui/` — full source of the core library
- `xna/trunk/Xenocide.MonoGame/Source/Stubs/CeGuiStubs.cs` — **170-line stub** covering all types the game consumes (the stub replaced the real CeGUI# DLL for the MonoGame port)

### 2.2 Architecture

CeGUI# follows a **retained-mode** widget-tree architecture:

- **GuiManager** (`CeGui.GuiManager`) — top-level component registered with the XNA `Game`; owns the root sheet, mouse cursor, tooltip, and all input routing
- **Window** (`CeGui.Window`) — base class for all UI elements; has position, size, visibility, event model
- **Widget** (`CeGui.Window` → `CeGui.Widget`) — derived class adding input interaction (click, hover, keyboard)
- **WindowSheet** — top-level sheet that handles mouse position and delegates to `GuiManager`
- **Renderer** (`CeGui.Renderers.Xna.XnaRenderer`) — singleton that renders the full widget tree each frame

### 2.3 Coordinate Model

CeGUI# uses a **three-mode metrics system**:

| Mode | Meaning | Example |
|------|---------|---------|
| `MetricsMode.Pixels` | Absolute pixel values | `XPosition = 100f` |
| `MetricsMode.Relative` | Fraction of parent (0.0–1.0) | `XPosition = 0.5f` (centered) |
| `MetricsMode.Absolute` | Absolute game units | Rarely used |

This is vaguely similar to CEGUI's UDim (now used in Roblox), though not as fully generalized.

### 2.4 Widget Inventory

The game uses **18 distinct widget types** across all screens:

| Widget Type | CeGui# Class | Usage |
|-------------|-------------|-------|
| StaticImage | `CeGui.Widgets.StaticImage` | Backgrounds, icons, full-screen images |
| StaticText | `CeGui.Widgets.StaticText` | Labels, headers, descriptions |
| PushButton | `CeGui.Widgets.PushButton` | All clickable buttons |
| EditBox | `CeGui.Widgets.EditBox` | Text input fields |
| ComboBox | `CeGui.Widgets.ComboBox` | Dropdown selections |
| Checkbox | `CeGui.Widgets.Checkbox` | Toggle options |
| MultiColumnList | `CeGui.Widgets.MultiColumnList` | Grid/tabular data (research tree, soldier stats) |
| FrameWindow | `CeGui.Widgets.FrameWindow` | Draggable/movable windows |
| Slider | `CeGui.Widgets.Slider` | Volume, scroll position |
| Listbox | `CeGui.Widgets.Listbox` | Item selection lists |
| Thumb | `CeGui.Widget` (generic) | Slider thumb |
| ScrollBar | `CeGui.Widget` (generic) | Scroll control |
| Menubar | `CeGui.Widget` (generic) | Menu bars |
| PopupMenu | `CeGui.Widget` (generic) | Context/popup menus |
| MenuItem | `CeGui.Widget` (generic) | Menu entries |
| Titlebar | `CeGui.Widget` (generic) | Window title bars |
| Tooltip | `CeGui.Tooltip` | Hover tooltips |
| DragContainer | `CeGui.DragContainer` | Drag-and-drop items |

### 2.5 Event Model

- **WindowEventHandler** — `(object sender, WindowEventArgs e)` for window-level events (close, activate)
- **MouseEventHandler** — `(object sender, MouseEventArgs e)` with position, button, wheel delta
- **GuiEventHandler** — `(object sender, GuiEventArgs e)` for general UI events (click, text change, selection change)

Events are subscribed with `+=` in C# code — no XML wiring.

### 2.6 Layout Construction

**All layouts are created in C# code** via `GuiBuilder`:

```csharp
var button = GuiBuilder.CreateButton("StartGameButton");
button.Text = "New Game";
button.Clicked += OnStartGame;
panel.AddChild(button);
```

There are **zero `.layout` XML files** in the active codebase. The only GUI assets are:

- `Xenocide/assets/cgui.png` — single 2150×2048 spritemap texture
- `Xenocide/assets/cgui.xml` — imageset XML defining named subrects
- `Xenocide/assets/VeraMono-10.font` — bitmap font definition (XML)
- `Xenocide/assets/TaharezScrollbarLook.scheme` / `Vanilla.scheme` — widget-look XML files

### 2.7 Screen Pattern

The base screen class is `Xenocide.UI.Screens.Frame`:

- `AddWidget(Widget)` — adds to internal list
- `CalculatePosition()` — positions relative to screen dimensions
- All 20+ screen classes (StartScreen, GeoscapeScreen, etc.) inherit from `Frame`

### 2.8 Stub Completeness

The 170-line stub at `CeGuiStubs.cs` covers every CeGUI# type, event, and method the game calls. Every method is a no-op returning default/null. This is sufficient for compilation but **nothing renders** — the game is a black screen at runtime.

---

## 3. GUI Replacement Candidates

### 3.1 Candidate Matrix

| Criteria | Gum (MonoGameGum) | MGUI | Myra | GeonBit.UI | ImGui.NET |
|----------|-------------------|------|------|------------|-----------|
| **GitHub Stars** | ~450 | ~100 | ~855 | ~508 | ~2,000 |
| **Last Update** | Mar 2026 | Mar 2026 | Mar 2026 | Apr 2024 | Jan 2026 |
| **License** | MIT | MIT | MIT | MIT | MIT |
| **.NET Target** | net6.0+ (works on net9.0) | net6.0-windows (modify for cross) | net6.0+ | net7.0 | netstandard2.0+ |
| **NuGet Package** | `Gum.MonoGame` (✅) | Source-only (❌) | `Myra` (✅) | `GeonBit.UI` (✅) | `ImGui.NET` (✅) |
| **Visual Designer** | **Gum UI Tool** (WYSIWYG) | MGXAMLDesigner (runtime XAML) | None | None | None |
| **Layout Engine** | Flexible anchor/dock/stack | WPF-style DockPanel/Grid/Stack | Vertical/horizontal stacks | Auto-anchor system | Manual positioning |
| **Data Binding** | MVVM via `SetBinding` + `INotifyPropertyChanged` | Full WPF-style binding engine | None | None | None |
| **Control Count** | ~20 Form controls | ~30+ controls | ~12 controls | ~15+ entities | Unlimited (ImGui draw calls) |
| **Cross-Platform** | ✅ (Windows, Linux, macOS, mobile) | ⚠️ (defaults to Windows; manual edit for Linux) | ✅ | ✅ | ✅ |
| **Documentation** | **Excellent** (dedicated docs site + MonoGame tutorial) | Minimal (README + wiki WIP) | Moderate | Good (API docs site) | Excellent (upstream Dear ImGui docs) |
| **Community** | Active Discord (Gum + FlatRedBall) | Small (1 primary contributor) | Moderate | Moderate | Large (Dear ImGui ecosystem) |
| **Suitability for Game UI** | ✅ Excellent | ✅ Good | ✅ Moderate | ✅ Moderate | ❌ (tools/debug UI, not game menus) |

### 3.2 Candidate Profiles

#### Gum (MonoGameGum) — ⭐ RECOMMENDED

**Repository**: [vchelaru/Gum](https://github.com/vchelaru/Gum) — 450+ stars, 40 contributors, MIT license  
**NuGet**: `Gum.MonoGame` (v2025.12.9.1+)  
**Docs**: https://docs.flatredball.com/gum/

Gum is a general-purpose UI layout tool and runtime created by Victor Chelaru (FlatRedBall engine author). It consists of two parts:

1. **Gum UI Tool** — a standalone WYSIWYG editor for visually creating UI layouts. Supports drag-and-drop, component inheritance, state machines, and live preview.
2. **Gum Runtime Library** (MonoGameGum) — loads `.gumx` project files or works purely in code. Renders via `GumService.Default`.

Gum was officially adopted by **MonoGame.Extended** in 2024 as their recommended GUI solution, replacing the older `MonoGame.Extended.Gui` module. It also has an official tutorial in the MonoGame documentation.

**Key strengths:**
- **WYSIWYG designer** — create screens visually, preview layouts, export to XML
- **Flexible layout engine** — anchoring, docking, stacking, percentage-based sizing
- **WPF-style MVVM data binding** — `SetBinding()`, `BindingContext`, `INotifyPropertyChanged`, `IValueConverter`, `DependsOn` attributes
- **Rich controls** — Button, CheckBox, ComboBox, Grid, ItemsControl, Label, ListBox, Menu, PasswordBox, RadioButton, ScrollBar, ScrollViewer, StackPanel, Slider, Splitter, TextBox, Window
- **State machines** — visual states (Enabled, Disabled, Highlighted, Pushed) auto-managed by controls
- **Cross-platform** — works on MonoGame DesktopGL, DirectX, Android, iOS; also works with FNA, Kni, Nez, SkiaSharp, Silk.NET, raylib
- **Active maintenance** — latest release March 2026, with V3 visuals update (Nov 2025) simplifying styling
- **Code-only or designer** — can use the tool for initial design and tweak in code, or go fully code-only

**Potential concerns:**
- Learning curve if you're unfamiliar with WPF/MVVM patterns (but the pattern is broadly useful)
- The layout system has its own conventions (not exactly CSS or WPF)
- Some advanced controls (TreeView, DataGrid) not available yet

---

#### MGUI — ⭐ STRONG CONTENDER

**Repository**: [Videogamers0/MGUI](https://github.com/Videogamers0/MGUI) — ~100 stars, 2 contributors, MIT license  
**NuGet**: Source-only (no NuGet package; must clone and add projects)  
**Docs**: README + Wiki (under construction)

MGUI is a WPF-inspired UI framework for MonoGame with a powerful layout engine and data binding system.

**Key strengths:**
- **Most WPF-like experience** — DockPanel, Grid, StackPanel, UniformGrid, TabControl, Expander, GroupBox, etc.
- **Full data binding engine** — `MGBinding` markup extension, `OneTime`/`OneWay`/`TwoWay`/`OneWayToSource` modes, converters, `StringFormat`, nested property paths, `ElementName` binding
- **Runtime XAML rendering** — `MGXAMLDesigner` control can parse and render XAML markup at runtime
- **Rich control set** — ~30+ controls including advanced ones like MGListView, MGTabControl, MGExpander, MGGroupBox, MGProgressButton, MGRatingControl, MGSeparator, MGContextMenu
- **Sophisticated input system** — `InputTracker` → `MouseTracker`/`KeyboardTracker` with `IsHandled` propagation pattern
- **Actively maintained** — last push March 2026

**Potential concerns:**
- **No NuGet package** — must clone the repo, add projects to your solution, reference them, and link `.mgcb` content files. This complicates CI/CD and dependency management.
- **Cross-platform requires manual edit** — targets `net6.0-windows` by default; must edit `.csproj` to `net6.0` for Linux/macOS, and the author notes things "probably™ work fine"
- **Very small community** — 100 stars, only 2 contributors, limited community support
- **Minimal documentation** — README covers getting started; wiki is "under construction"
- **Smaller widget set** for game-specific needs compared to Gum
- **No visual designer** — `MGXAMLDesigner` renders XAML at runtime but isn't a design tool
- **Target framework** — defaults to `net6.0-windows`, needs upgrading to net9.0

---

#### Myra

**Repository**: https://github.com/rds1983/Myra — ~855 stars, MIT license  
**NuGet**: `Myra` (available)  
**Docs**: README + wiki

XML-based UI library for MonoGame. Well-established but:
- No data binding
- No visual designer
- Controls are basic (Button, TextBox, ListBox, TreeView, Window, Menu, Grid, StackPanel)
- Layout is fixed (no WPF-style flexible layout)
- Last significant work: text rendering improvements in early 2026

Viable but offers less than Gum or MGUI for complex screen-based UIs.

#### GeonBit.UI

**Repository**: https://github.com/RonenNess/GeonBit.UI — ~508 stars, MIT license  
**NuGet**: `GeonBit.UI` (available)

Pre-styled UI for rapid prototyping. Good for simple menus but:
- No data binding
- Limited layout system (anchor-based only)
- No visual designer
- Last NuGet update: April 2024
- Author now recommends **Iguina** (successor, framework-agnostic)
- Too minimal for Xenocide's complex screens (grids, trees, multi-tab panels)

#### ImGui.NET

**Repository**: https://github.com/ImGuiNET/ImGui.NET — ~2,000 stars, MIT license  
**NuGet**: `ImGui.NET` (available)

Industry-standard immediate-mode GUI. Excellent for developer tools but:
- **Immediate mode** — UI state is rebuilt every frame; no retained widget tree
- **No data binding** — manual state synchronization
- **No visual designer**
- Best for debug overlays, inspector tools, and in-game consoles
- Poor fit for complex game menus (save/load screens, research trees, soldier equipment)

---

## 4. Deep-Dive: Gum vs MGUI

### 4.1 Layout Engine Comparison

| Aspect | Gum | MGUI |
|--------|-----|------|
| **Layout Types** | Anchoring, docking (`Fill`, `Left`, `Top`, `Right`, `Bottom`), `StackPanel` (horizontal/vertical), `Grid` | `MGDockPanel`, `MGGrid`, `MGStackPanel`, `MGOverlayPanel`, `MGUniformGrid` |
| **Sizing Model** | Absolute pixels, percentage of parent, `RelativeToChildren` (auto-size to content), wrapping | Absolute, stretch, auto, percentage via Grid `*` sizing |
| **Per-child Offsets** | `X`/`Y` offset relative to anchor position | `Margin` property |
| **Z-Order** | Children draw order (last = on top) | Children draw order + `MGResizeGrip` for z-order management |
| **Layout Passes** | Parent→child and child→parent passes (bidirectional) | Standard measure/arrange (WPF-like) |

**Verdict**: Both are powerful. Gum's layout is more game-oriented (anchors + percentage sizing for resolution independence). MGUI's layout is closer to WPF (Grid with star-sizing, DockPanel).

### 4.2 Control Set Comparison

| Control | Gum | MGUI |
|---------|-----|------|
| Button | ✅ `Button` | ✅ `MGButton`, `MGToggleButton`, `MGProgressButton` |
| CheckBox | ✅ `CheckBox` | ✅ `MGCheckBox` |
| ComboBox | ✅ `ComboBox` | ✅ `MGComboBox` |
| ListBox | ✅ `ListBox` | ✅ `MGListBox` |
| TextBox | ✅ `TextBox` | ✅ `MGTextBox`, `MGPasswordBox` |
| Slider | ✅ `Slider` | ✅ `MGSlider` |
| RadioButton | ✅ `RadioButton` | ✅ `MGRadioButton` |
| Label | ✅ `Label` | ✅ `MGTextBlock` |
| ScrollViewer | ✅ `ScrollViewer` | ✅ `MGScrollViewer` |
| StackPanel | ✅ `StackPanel` | ✅ `MGStackPanel` |
| Grid | ✅ `Grid` | ✅ `MGGrid`, `MGUniformGrid` |
| TabControl | ❌ | ✅ `MGTabControl` + `MGTabItem` |
| Expander | ❌ | ✅ `MGExpander` |
| GroupBox | ❌ | ✅ `MGGroupBox` |
| Menu | ✅ `Menu` + `MenuItem` | ✅ `MGContextMenu` + `MGContextMenuItem` |
| ListView | ❌ | ✅ `MGListView` |
| Window | ✅ `Window` | ✅ `MGWindow` |
| PasswordBox | ✅ `PasswordBox` | ✅ `MGPasswordBox` |
| ProgressBar | ❌ | ✅ `MGProgressBar` |
| Separator | ❌ | ✅ `MGSeparator` |
| Border | ❌ | ✅ `MGBorder` |
| Image | ❌ (use `SpriteRuntime`) | ✅ `MGImage` |
| ToolTip | ❌ | ✅ `MGToolTip` |
| Rating Control | ❌ | ✅ `MGRatingControl` |
| Splitter | ✅ `Splitter` | ✅ `MGGridSplitter` |
| ItemsControl | ✅ `ItemsControl` | ❌ |

**Verdict**: MGUI has a slightly richer built-in control set (TabControl, Expander, GroupBox, ProgressBar, ToolTip, RatingControl). Gum's coverage is solid but has fewer bells and whistles. However, Gum's lack of progress bars and tabs can be addressed by composing existing controls or custom drawing.

### 4.3 Data Binding Comparison

| Aspect | Gum | MGUI |
|--------|-----|------|
| **ViewModel base** | `Gum.Mvvm.ViewModel` or any `INotifyPropertyChanged` | Any `INotifyPropertyChanged` |
| **Binding Context** | `BindingContext` (inherits to children, like WPF `DataContext`) | `WindowDataContext` (window-level) or `DataContextOverride` (element-level) |
| **Binding Syntax** | `SetBinding(nameof(control.Property), nameof(vm.Property))` | `{MGBinding Path=Foo.Bar}` in XAML, or C# equivalents |
| **Binding Modes** | `OneWay`, `TwoWay`, `OneWayToSource` | `OneTime`, `OneWay`, `OneWayToSource`, `TwoWay` |
| **Converters** | `IValueConverter` (same interface as WPF) | `IValueConverter` |
| **StringFormat** | ✅ Via `Binding.StringFormat` | ✅ Via `StringFormat` parameter |
| **Fallback/Null** | `FallbackValue`, `TargetNullValue` | ✅ |
| **Nested Paths** | ✅ `nameof(vm.Player.Name)` or dotted string | ✅ `{MGBinding Path=Foo.Bar}` |
| **ElementName Binding** | ❌ | ✅ `{MGBinding ElementName=SomeCheckbox, Path=IsChecked}` |
| **Indexer Binding** | ✅ `Items[0].Text` | ❌ (not documented) |
| **Lambda Binding** | ✅ `SetBinding<VM>(...)` with expression | ❌ |
| **Collection Binding** | `INotifyCollectionChanged` / `ObservableCollection` | ✅ |
| **Event Binding** | ✅ `SetBinding(nameof(handler), nameof(VM.Event))` | ❌ |

**Verdict**: Both have strong data binding. Gum's API is slightly more modern (lambda expressions for compile-time safety, event binding). MGUI's XAML-based `MGBinding` markup is more familiar to WPF developers and supports `ElementName` binding.

### 4.4 Designer / Visual Tooling

| Aspect | Gum | MGUI |
|--------|-----|------|
| **WYSIWYG Editor** | ✅ **Gum UI Tool** — standalone, drag-drop, live preview, component inheritance, states | ❌ No visual designer |
| **Runtime Preview** | ✅ Layout changes visible in-editor | Only via runtime `MGXAMLDesigner` control |
| **Code Generation** | ✅ From `.gumx` project files (optional; code-only also supported) | ❌ |
| **XAML at Runtime** | ❌ | ✅ `MGXAMLDesigner` renders XAML markup live |
| **Designer Availability** | Standalone app (Windows) | N/A |
| **Designer Status** | Actively maintained (latest release Mar 2026) | N/A |

**Verdict**: Gum's **WYSIWYG designer is a decisive advantage** for visual UI development. It enables non-programmers (designers) to create and iterate on layouts. MGUI has no designer — everything is code or runtime XAML.

### 4.5 Community & Maintenance

| Aspect | Gum | MGUI |
|--------|-----|------|
| **Stars** | ~450 | ~100 |
| **Contributors** | 40 | 2 |
| **Latest Release** | March 2026 | N/A (source-only) |
| **Last Commit** | April 2026 | March 2026 |
| **Open Issues** | ~36 | ~7 |
| **Documentation** | Full docs site + MonoGame official tutorial | README + WIP wiki |
| **Support Channels** | Discord + GitHub Issues | GitHub Issues |
| **Adopted By** | MonoGame.Extended (official), FlatRedBall, Nez | None |
| **Release Cadence** | Monthly releases | Irregular |

**Verdict**: Gum has a **significantly larger community**, more contributors, better documentation, and an established release cadence. Its adoption by MonoGame.Extended as the official GUI solution ensures ongoing compatibility.

### 4.6 Cross-Platform Considerations

| Aspect | Gum | MGUI |
|--------|-----|------|
| **Windows** | ✅ Full support | ✅ Full support (primary target) |
| **Linux** | ✅ Tested (DesktopGL) | ⚠️ Requires manual `.csproj` edit; "probably works" |
| **macOS** | ✅ Tested | ⚠️ Same as Linux |
| **Mobile (Android/iOS)** | ✅ Supported | ❌ Not documented |
| **NuGet Package** | ✅ `Gum.MonoGame` (single package, all platforms) | ❌ Source clone + project reference |
| **Content Pipeline** | Standard `.mgcb` integration | Must link MGUI's `.mgcb` files as content |

**Verdict**: Gum is **fully cross-platform out of the box** with a single NuGet package. MGUI requires source integration and manual platform configuration.

### 4.7 Integration Complexity

#### Gum Integration
```csharp
// 1. Add NuGet: dotnet add package Gum.MonoGame
// 2. In Game1.cs:
protected override void Initialize()
{
    GumService.Default.Initialize(this);  // code-only
    // or: GumService.Default.Initialize(this, "GumProject/GumProject.gumx");
    base.Initialize();
}
protected override void Update(GameTime gameTime)
{
    GumService.Default.Update(gameTime);
    base.Update(gameTime);
}
protected override void Draw(GameTime gameTime)
{
    GraphicsDevice.Clear(Color.CornflowerBlue);
    GumService.Default.Draw();
    base.Draw(gameTime);
}
```

Total: 3 lines of setup, 1 NuGet package, works on all platforms.

#### MGUI Integration
```csharp
// 1. Clone repo, add MGUI.Shared.csproj + MGUI.Core.csproj to solution
// 2. Add project references
// 3. Link .mgcb content files from MGUI projects
// 4. Edit MGUI.Core.csproj target from net6.0-windows to net6.0 (for Linux)
// 5. In Game1.cs:
protected override void Initialize()
{
    this.MGUIRenderer = new MainRenderer(new GameRenderHost<Game1>(this));
    this.Desktop = new MGDesktop(MGUIRenderer);
    // ...create windows, controls...
    base.Initialize();
}
protected override void Update(GameTime gameTime)
{
    Desktop.Update(gameTime);
    base.Update(gameTime);
}
protected override void Draw(GameTime gameTime)
{
    GraphicsDevice.Clear(Color.CornflowerBlue);
    Desktop.Draw();
    base.Draw(gameTime);
}
```

Total: ~6 steps (clone, add projects, modify csproj, link content, 3 lines startup code), Windows-primary assumption, no NuGet.

---

## 5. Evaluation Summary

### 5.1 Criteria Weighting

| Criteria | Weight | Rationale |
|----------|--------|-----------|
| **Visual Designer** | 30% | Primary requirement: ease of visual creation and iteration |
| **Data Binding** | 20% | MVVM pattern reduces boilerplate and bugs in complex screens |
| **Cross-Platform** | 15% | Must work on Windows + Linux without per-platform hacks |
| **Documentation** | 15% | Good docs reduce learning curve and ongoing friction |
| **Community / Maintenance** | 10% | Active project ensures bug fixes and future compatibility |
| **Control Richness** | 5% | Need enough controls for all screen types |
| **Integration Ease** | 5% | NuGet package vs source integration |

### 5.2 Weighted Scores

| Candidate | Designer | Binding | X-Plat | Docs | Community | Controls | Integration | **Total** |
|-----------|:--------:|:-------:|:------:|:----:|:---------:|:--------:|:-----------:|:---------:|
| **Gum** | 10 (3.0) | 9 (1.8) | 10 (1.5) | 10 (1.5) | 8 (0.8) | 7 (0.35) | 9 (0.45) | **9.40** |
| **MGUI** | 0 (0.0) | 9 (1.8) | 5 (0.75) | 4 (0.6) | 3 (0.3) | 9 (0.45) | 3 (0.15) | **4.05** |
| Myra | 0 (0.0) | 0 (0.0) | 9 (1.35) | 6 (0.9) | 6 (0.6) | 5 (0.25) | 8 (0.40) | **3.50** |
| GeonBit.UI | 0 (0.0) | 0 (0.0) | 9 (1.35) | 7 (1.05) | 5 (0.5) | 5 (0.25) | 8 (0.40) | **3.55** |
| ImGui.NET | 0 (0.0) | 0 (0.0) | 9 (1.35) | 8 (1.2) | 9 (0.9) | 10 (0.5) | 9 (0.45) | **4.40** |

**ImGui.NET scores higher than expected on this matrix because it excels in community, docs, and integration — but its **immediate-mode paradigm** is fundamentally mismatched for retained-mode screen UIs. The numerical score doesn't capture this architectural concern.

### 5.3 Architectural Fit

Beyond the quantitative score, there are architectural considerations:

| Concern | Gum | MGUI |
|---------|-----|------|
| **Retained-mode** | ✅ Yes — widget tree persists between frames | ✅ Yes — widget tree persists |
| **Screen Composition** | ✅ `StackPanel`, `Grid`, `DockPanel` composition | ✅ Same (WPF-style) |
| **Dynamic Content** | ✅ `ItemsControl` + data binding for lists | ✅ `MGListBox` + `MGListView` with binding |
| **Styled/Skinned** | ✅ State machines (Enabled/Disabled/Highlighted/Pushed) + DefaultVisuals | ✅ Brushes system (solid, gradient, texture) + `MGGradientBrush` |
| **Custom Drawing** | ✅ `GraphicalUiElement` + `SpriteRuntime` for custom visuals | ✅ `MGImage` + custom element rendering |
| **Dialog/Modal** | ✅ Window control + modal support | ✅ `MGWindow` with modal windows |
| **Tooltip System** | ❌ Must build custom | ✅ `MGToolTip` built-in |

---

## 6. Recommendation

### Gum (MonoGameGum) is the recommended choice

**Rationale:**

1. **Visual Designer** — The Gum UI Tool is the only candidate with a proper WYSIWYG editor. This directly addresses the primary requirement of being "easy to work with visually." Designers can create and iterate on layouts without touching code.

2. **Data Binding** — MVVM support (`SetBinding`, `BindingContext`, `IValueConverter`, `DependsOn`) mirrors WPF patterns that the team may already know. It reduces boilerplate in complex screens like the research tree (grid of items) and soldier equipment (multi-panel drag-drop).

3. **Cross-Platform by Default** — Single NuGet package, works on Windows, Linux, macOS, Android, iOS with no configuration changes.

4. **Official MonoGame Integration** — Adopted by MonoGame.Extended, documented in the official MonoGame tutorial. This ensures long-term compatibility.

5. **Active Development** — Monthly releases, 40 contributors, responsive maintainer (vchelaru). The V3 visuals update (Nov 2025) shows active investment.

6. **Flexible Deployment** — Use the designer for complex screens and code-only for dynamic content. Or use it purely code-only. Both are fully supported.

### When MGUI Would Be Preferred

If a NuGet package existed, cross-platform was a first-class concern (not "edit csproj and hope"), and a visual designer was available, MGUI would be a stronger contender. Its WPF-parity control set (TabControl, Expander, GroupBox) and XAML runtime rendering are genuinely useful. However, the lack of these three things makes it a riskier choice for a long-lived project.

### Migration Path (Gum)

1. Add `Gum.MonoGame` NuGet package to the project
2. Initialize `GumService.Default` in the game class (3 lines)
3. Create a proof-of-concept screen (e.g., StartScreen) using Gum code-only approach
4. Evaluate the Gum UI Tool for visual layout design
5. Port screens progressively, starting with the simplest (StartScreen) and most complex (GeoscapeScreen)
6. For each screen:
   - Implement ViewModel with `INotifyPropertyChanged` (or inherit `Gum.Mvvm.ViewModel`)
   - Create controls via code or load from `.gumx` designer file
   - Wire binding with `SetBinding()`
   - Hook events (Click, TextChanged, etc.)
7. Remove CeGuiStubs.cs once all screens are converted

### Controls Mapping (CeGUI# → Gum)

| CeGUI# Widget | Gum Equivalent | Notes |
|---------------|----------------|-------|
| `StaticImage` | `SpriteRuntime` or `ColoredRectangleRuntime` | Use `GraphicalUiElement` for custom visuals |
| `StaticText` | `Label` | Direct equivalent |
| `PushButton` | `Button` | Direct equivalent |
| `EditBox` | `TextBox` | Direct equivalent |
| `ComboBox` | `ComboBox` | Direct equivalent |
| `Checkbox` | `CheckBox` | Direct equivalent |
| `MultiColumnList` | `ListBox` + custom columns or `Grid` | Most complex mapping; may need a custom `ItemsControl` with `Grid` layout |
| `FrameWindow` | `Window` | Direct equivalent |
| `Slider` | `Slider` | Direct equivalent |
| `Listbox` | `ListBox` | Direct equivalent |
| `Thumb` / `ScrollBar` | Built into `ScrollViewer` / `Slider` | No standalone equivalent needed |
| `Menubar` / `PopupMenu` / `MenuItem` | `Menu` + `MenuItem` | Direct equivalent |
| `Tooltip` | Built via `Window` or custom control | No built-in tooltip |
| `DragContainer` | Custom via `InteractiveGue` + mouse events | No built-in drag container |
