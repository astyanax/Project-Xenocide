# Prompt for Next Session: Gum UI Layout & Theming

## Goal
Replace the current programmatic Gum UI (`CreateGumControls()` in every screen) with a proper `.gumx` project file that provides visual layout, button theming from the TaharezLook spritesheet, and typographic styling. Also finish removing CeGui stubs so the entire UI layer is Gum-native.

---

## 1. Set Up Gum UI Tool

Install the Gum WYSIWYG editor:
```
dotnet tool install -g GumUiTool
```
Then invoke `gum` in the project directory to create a `.gumx` file. The `.gumx` should live at:
```
xna/trunk/Xenocide.MonoGame/Content/Gum/Xenocide.gumx
```

In the Gum tool, create a project targeting a resolution of **1280×1024**. Load it at runtime via `GumService.Default.Initialize(this, "Content/Gum/Xenocide.gumx")` instead of the current parameterless `Initialize(this)`.

---

## 2. Create Styled Button Components from TaharezLook Spritesheet

The spritesheet is at `Content/Textures/UI/XenoNew.png`. The button segments defined in `Resources/TaharezLook.imageset` (line 94–185):

| State | Left | Middle | Right |
|-------|------|--------|-------|
| Normal | `ButtonLeftNormal` (261,130, 35×25) | `ButtonMiddleNormal` (297,130, 145×25) | `ButtonRightNormal` (442,130, 3×25) |
| Pushed | `ButtonLeftPushed` (261,195, 35×25) | `ButtonMiddlePushed` (297,195, 145×25) | `ButtonRightPushed` (442,195, 3×25) |
| Highlight | `ButtonLeftHighlight` (261,163, 35×25) | `ButtonMiddleHighlight` (297,163, 145×25) | `ButtonRightHighlight` (442,163, 3×25) |

In Gum, create a **Component** called `XenocideButton` using `NineSlice` for each of the 3 horizontal segments (or a single `NineSlice` for the full button), with state categories for Normal/Pushed/Highlighted. Assign the `Xeno` spritefont (Arial 8) as the default text font.

---

## 3. Define Global Styles / Typography

Available spritefonts from `Content/SpriteFonts/`:

| Style Name | Font | Size | Use |
|-----------|------|------|-----|
| `Xeno` | Arial | 8pt | Default UI text, button labels |
| `XenoBig` | Arial | 10pt Bold | Section headers |
| `LargeBaseName` | Arial | 24pt Bold | Base/screen titles |
| `GeoTime` | Consolas | 10pt Bold | Geoscape time/date display |
| `GeoTimeBig` | Consolas | 18pt Bold | Large geoscape readouts |
| `SpriteFont1` | Arial | 10pt | Legacy fallback |

Create a **Standard Element** or component-level font assignments so all `Button`, `Label`, and `TextBox` controls use the correct font automatically. Target: no `.Text` property assignments should need manual font overrides.

---

## 4. Convert Screens from Programmatic to .gumx Layouts

Currently every screen (27 total) builds Gum controls in C# via `protected override void CreateGumControls()`. The goal is to replace these with Gum editor-designed screens loaded at runtime.

**Priority screens to convert first (most visible, set the pattern):**

| Screen | Background | Key Controls |
|--------|-----------|--------------|
| `StartScreen` | `StartScreenBackground.png` | 5 buttons (New Game, Load, Credits, Quit, +DEBUG), version label |
| `BasesScreen` | `BasesScreenBackground.png` | Base list (ListBox), facility grid, funds label, build/transfer buttons |
| `GeoscapeScreen` | `GeoscapeScreenBackground.png` | Time/date labels, speed buttons, base/intercept controls, 3D viewport |
| `BattlescapeScreen` | `BasesScreenBackground.png` | Equipment/right-hand/finish-turn/abort buttons, combatant stats label, 3D viewport |

**Pattern for conversion:**
```csharp
protected override void CreateGumControls()
{
    // Load Gum screen from .gumx instead of building programmatically
    RootContainer = GumService.Default.Root.GetFrameworkElementByName<StackPanel>("StartScreenRoot");
    WireClickSounds(RootContainer);
    // Wire event handlers to named controls from .gumx
    var newGameBtn = RootContainer.GetChild<Button>("NewGameButton");
    newGameBtn.Click += OnNewGameClicked;
}
```

---

## 5. Replace CeGui Stubs in Frame and BattlescapeScreen

After all screens load from `.gumx`, the CeGui stubs in `Frame.cs` and `BattlescapeScreen.cs` can be removed:

| File | CeGui Dependency | Replacement |
|------|-----------------|-------------|
| `Frame.cs:65` | `CeGui.Size` in constructor | `Microsoft.Xna.Framework.Vector2` or Gum-native sizing |
| `Frame.cs:77-89` | `GuiSheet.AddChild(rootWidget)`, `CeGui.MetricsMode` | Gum `AddToRoot()` (already done in GumScreen) |
| `Frame.cs:122` | `CeGui.WindowManager.Instance.DestroyWindow()` | Gum `Root.Children.Remove()` (already done in GumScreen) |
| `Frame.cs:164-167` | `CeGui.GuiSheet` property | Remove — no longer needed |
| `BattlescapeScreen.cs:135-136` | `CeGui.MouseEventHandler` on sceneWindow | MonoGame `Mouse.GetState()` polling in Update |
| `BattlescapeScreen.cs:142` | `CeGui.Widgets.StaticImage sceneWindow` | Gum-native viewport or Rectangle |
| `Screen.cs:100-134` | `LoadBackgroundImage()` via CeGui ImagesetManager | Already handled by GumScreen SpriteBatch (Phase 4.7) |
| `Xenocide.cs:62,95,130` | `GuiManager`, `InitializeCegui()` | Remove — dead code since CeGui is stubbed |
| `ScreenManager.cs:362-364,375` | `RootGuiSheet`, `Taharez.TLGuiBuilder` | Remove after Frame no longer uses GuiSheet |

File `WinFormsStubs.cs` can also be removed — only used by `CeGui.MouseEventArgs.Button` which is a stub.

---

## 6. Content Pipeline — Add .gumx

Register the `.gumx` and any Gum-exported assets in `Content.mgcb`:
```
#begin Content/Gum/Xenocide.gumx
/importer:GumImporter
/processor:GumProcessor
/build:Content/Gum/Xenocide.gumx
```

Also add the `.gumx` to the `.csproj` content includes so it's copied to output.

---

## 7. Software Cursor Polish (Optional)

The `SoftwareCursor` component at `Source/UI/SoftwareCursor.cs` works but could be improved:
- Add hotspot offset (currently top-left of sprite = click point, typical arrow hotspot is ~(2,2))
- Add context-sensitive cursors: `MouseTarget` (4,146, 19×19) for interactable targets, `MouseTextBar` (29,167, 7×12) for text entry
- Add a `GameOptions` toggle to switch between HW and SW cursor
- Currently disables HW cursor when loaded; consider making this configurable

---

## Current File Locations

| What | Path |
|------|------|
| Spritesheet | `Content/Textures/UI/XenoNew.png` |
| Imageset definitions | `Resources/TaharezLook.imageset` (250 lines of sprite coordinates) |
| Software cursor | `Source/UI/SoftwareCursor.cs` |
| GumScreen base | `Source/UI/Screens/GumScreen.cs` |
| CeGui stubs | `Source/Stubs/CeGuiStubs.cs` (173 lines) |
| MGCB project | `Content.mgcb` (project root) |
| Build: 0 errors, 346 warnings (CA1852) | |
| Tests: 5/5 pass (xUnit) | |
