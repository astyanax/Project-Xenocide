# Migration Plan: XNA 3.0 → MonoGame

The legacy XNA 3.0 codebase will be migrated to **MonoGame** for modern cross-platform support (Windows + Linux).

The XNA 3.0→4.0 API conversion (Phase 1) targets MonoGame's XNA 4.0-like API directly — no intermediate framework or test step is needed. MonoGame's API surface is close enough that the conversion and the port happen together.

See [README.md](README.md) for build prerequisites and quick-start instructions.

## 1. Current Dependencies (Legacy XNA 3.0 Stack)

### .NET Framework
| Dependency | Version | Usage | Files Affected |
|-----------|---------|-------|---------------|
| .NET Framework | 2.0 (trunk) / 3.5 (branch) | Runtime target | All 290 source files |
| `System` | BCL | Core types | ~290 |
| `System.Collections.Generic` | BCL | Collections | ~250 |
| `System.Text` | BCL | Strings | ~280 |
| `System.Diagnostics` | BCL | Debugging | ~193 |
| `System.Xml` / `System.Xml.XPath` | BCL | XML data loading | ~69 |
| `System.Drawing` | BCL | SizeF, PointF, Rectangle (CeGui interop) | ~40 |
| `System.Windows.Forms` | BCL | MessageBox, MouseButtons, Keys | ~8 (direct), ~54 (via CeGui) |
| `System.Runtime.Serialization.Formatters.Binary` | BCL | Save/load (`BinaryFormatter`) | 2 |
| `System.Threading` | BCL | Threading | ~19 |
| `System.IO` | BCL | File I/O | ~19 |
| `System.Globalization` | BCL | Culture | ~20 |

### XNA Framework (Microsoft.Xna.Framework.*)
| Dependency | Version | Usage | Files Affected |
|-----------|---------|-------|---------------|
| `Microsoft.Xna.Framework` | 3.0.0.0 | Vector2/3/4, Matrix, Color, GameTime, etc. | ~147 |
| `Microsoft.Xna.Framework.Game` | 3.0.0.0 | Game class, GameComponent | ~90 |
| `Microsoft.Xna.Framework.Graphics` | 3.0.0.0 | GraphicsDevice, SpriteBatch, Model, Texture2D, Effect | ~57 |
| `Microsoft.Xna.Framework.Content` | 3.0.0.0 | ContentManager.Load&lt;T&gt;() | ~45 |
| `Microsoft.Xna.Framework.Storage` | 3.0.0.0 | StorageDevice, StorageContainer | ~40 (indirect via FileUtil) |
| `Microsoft.Xna.Framework.Input` | 3.0.0.0 | KeyboardState, MouseState | ~15 |
| `Microsoft.Xna.Framework.GamerServices` | 3.0.0.0 | Guide, SignedInGamer | 2 |
| `Microsoft.Xna.Framework.Audio` | 3.0.0.0 | Audio namespace | 1 |

### Third-Party Dependencies
| Dependency | Version | Purpose | Files Affected |
|-----------|---------|---------|---------------|
| **CeGui#** (Custom C# GUI) | Custom fork (last updated 2013) | UI widgets, layout, event handling | **~70 source files + 12+ .layout files + 150+ dep files** |
| **CeGui.Renderers.Xna** | Custom fork | XNA GraphicsDevice rendering backend | ~6 |
| **CeGui.WidgetSets.Taharez** | Custom fork | TaharezLook widget theme | ~3 |
| **FMOD Ex** (via custom C# wrapper) | Legacy (pre-FMOD Studio) | Audio playback | ~15 + 7 dependency files |
| **NUnit** | 2.2.9.0 | Unit testing (test project only) | Tests only — to be migrated to **xUnit.net** |

### Content Pipeline (XNA Game Studio 3.0)
| Type | Assets | Format | Pipeline Importer |
|------|--------|--------|-------------------|
| 3D Models | ~40 (characters, facilities, craft, weapons) | .X (DirectX) + .FBX | FBXImporter, XImporter |
| Shaders | 2 (GeoscapeShader, skybox) | .fx (HLSL) | EffectImporter |
| Fonts | 1 (SpriteFont1) | .spritefont (XML) | SpriteFontImporter |
| Textures | ~20+ (Earth, UI backgrounds, icons, atlas) | .jpg, .png | TextureImporter |
| Audio | Music + SFX | .ogg | AudioImporter (passthrough for FMOD) |
| XML Data | ~15 schema files + data | .xml, .xsd | Manual load via System.Xml |

---

## 2. Target Dependencies (MonoGame Stack)

### .NET
| Dependency | Version | Purpose |
|-----------|---------|---------|
| .NET SDK | 9.0+ | Runtime target |
| `System.Text.Json` | Built-in | Replace BinaryFormatter for save/load |
| **xUnit.net** | 2.9+ | Replace NUnit 2.2.9 for unit testing |

### MonoGame Framework (NuGet)
| Package | Version | Purpose | Notes |
|---------|---------|---------|-------|
| `MonoGame.Framework.DesktopGL` | 3.8.4+ | Core game framework (OpenGL + SDL2) | Cross-platform: Windows, Linux, macOS |
| `MonoGame.Content.Builder.Task` | 3.8.4+ | MSBuild content build integration | Compiles assets to .xnb at build time |
| `dotnet-mgcb` (CLI tool) | 3.8.4+ | Content pipeline CLI | `dotnet tool install -g dotnet-mgcb` |
| `dotnet-mgcb-editor` (CLI tool) | 3.8.4+ | GUI content project editor | `dotnet tool install -g dotnet-mgcb-editor` |

### GUI (CeGui# Replacement — DECIDED: Gum)
| Option | Stars | Status | Approach | Analysis |
|--------|-------|--------|----------|----------|
| **Gum** ✅ SELECTED | ~450 | Active (Mar 2026) | WYSIWYG layout editor + retained-mode UI + MVVM binding | **Winner (9.40/10).** Visual designer (Gum UI Tool), cross-platform NuGet, WPF-style data binding, adopted by MonoGame.Extended, 40 contributors, monthly releases. Full docs + official MonoGame tutorial. |
| **MGUI** | ~100 | Active (Mar 2026) | WPF-style XAML layouts, rich widgets | **Runner-up (4.05/10).** Richer controls (TabControl, Expander), WPF-parity binding engine, runtime XAML. **No NuGet** (source-clone only), **no visual designer**, cross-platform requires manual .csproj edit, very small community (2 contributors). |
| **GeonBit.UI** | ~508 | Stale (Apr 2024) | Pre-styled, rapid setup | **Third (3.55/10).** Simple to use but too minimal for complex screens. No binding, no designer, limited layout. Author now recommends successor (Iguina). |
| **Myra** | ~855 | Active (Mar 2026) | XML-based layouts | **Fourth (3.50/10).** Well-established but no data binding, no visual designer, basic controls only. Good for simple menus but lacks layout flexibility. |
| **ImGui.NET** | ~2k | Active (Jan 2026) | Immediate-mode | **Architectural mismatch (4.40/10 weighted).** Excellent for debug/tools UI but immediate-mode is fundamentally wrong for retained-mode game screens. No data binding, no widget tree persistence, every frame rebuilds state. |

### GUI (CeGui# Replacement — Decided)
| Package | Version | Purpose | Notes |
|---------|---------|---------|-------|
| `Gum.MonoGame` | Latest | UI layout engine + Forms controls + MVVM binding | NuGet, cross-platform, WYSIWYG designer available |

### Audio (FMOD Ex Replacement — DECIDED: Built-in SoundEffect)
| Option | Status | Notes |
|--------|--------|-------|
| **Built-in SoundEffect** ✅ SELECTED | MonoGame 3.8+ | `OggImporter` + `SoundEffectProcessor` via MGCB. `SongProcessor` is broken on DesktopGL (produces 128-byte stubs). All audio (SFX + music) uses `Content.Load<SoundEffect>()` with MGCB-compiled `.xnb`. Music uses `SoundEffectInstance` with `IsLooped` + auto-advance via `Update()` polling. Button click sounds auto-wired in `GumScreen.Show()`. |
| **FmodForFoxes** (NuGet) | Active | FMOD Studio wrapper for .NET, works with MonoGame. Not needed — simple playback only. |
| **Keep FMOD Ex via P/Invoke** | Legacy | FMOD Ex is old; will need updated bindings. Not needed. |

---

## 3. Migration Phases & Roadmap

### Phase 0: Tooling & Setup
- [x] **Install .NET SDK 9.0** — ✅ Done (detected on system)
- [x] Install MonoGame templates: `dotnet new install MonoGame.Templates.CSharp` — ✅ Done
- [x] Install MGCB tools: `dotnet tool install -g dotnet-mgcb dotnet-mgcb-editor` — ✅ Done
- [x] Create new MonoGame DesktopGL project: `dotnet new mgdesktopgl -o xna/trunk/Xenocide.MonoGame` — ✅ Done
- [x] Add NuGet packages: `MonoGame.Framework.DesktopGL`, `MonoGame.Content.Builder.Task` — ✅ Done
- [x] Set up MGCB content project (`.mgcb`) with all asset references — ✅ Models, shaders, textures, fonts, audio registered
- [ ] Replace NUnit with xUnit.net in test project

### Phase 1: XNA 3.0 → 4.0 API Conversion
- [x] Replace `effect.Begin()/End()` → `Pass.Apply()` only — ✅ Done
- [x] Replace `GraphicsDevice.RenderState.*` → state objects — ✅ Done
- [x] Replace `GraphicsDevice.VertexDeclaration` assignments → remove — ✅ Done
- [x] Replace `GraphicsDevice.Vertices[0].SetSource()` → `SetVertexBuffer()` — ✅ Done
- [x] Replace `SpriteBatch.Begin(blend, sort, save)` → `Begin(sort, blend)` — ✅ Done
- [x] Replace `ResolveTexture2D` → `RenderTarget2D` — ✅ Done
- [x] Replace point sprites → triangle-based rendering — ✅ Done
- [x] Move `Color` from `Graphics` → `Framework` namespace — ✅ Done
- [x] Replace `StorageDevice`/`StorageContainer` → `System.IO` — ✅ Done
- [x] Remove `GamerServices` references — ✅ Done
- [x] Update `VertexElement` constructors — ✅ Done

### Phase 1.5: Stub Dependencies & Compilation Fix
- [x] Add CeGui# stubs (70+ source files' worth of types, events, widgets) — ✅ Done
- [x] Add FMOD Ex audio stubs (`IAudioSystem` interface) — ✅ Done
- [x] Add `System.Windows.Forms.MouseButtons` stub — ✅ Done
- [x] Add `CeGui.WidgetSets.Taharez` namespace stub — ✅ Done
- [x] Fix all type mismatches between stubs and consuming code — ✅ Done
- [x] MonoGame project compiles from `dotnet build` with **0 errors** — ✅ Done

### Phase 2: Platform-Specific Code Cleanup
- [x] Replace `System.Windows.Forms.MouseButtons` → stub provided (WinFormsStubs.cs) — ✅ Done
- [x] Replace `System.Windows.Forms.MessageBox` → already using custom in-game `MessageBoxDialog` — ✅ Not needed
- [x] Replace `System.Drawing.SizeF` → `CeGui.Size` (struct with float Width/Height) — ✅ Done
- [x] Replace `System.Drawing.PointF` → `CeGui.Point` (struct with float X/Y) — ✅ Done
- [x] Replace `System.Drawing.Rectangle` → not used in codebase — ✅ Not needed
- [x] Replace `ErrorDialogue` WinForms Form → not present in codebase — ✅ Not needed
- [x] Remove dead `using System.Drawing;` from all 36 game code files — ✅ Done
- [x] Add missing `using System.Diagnostics;` (was masked by transitive System.Drawing dependency) — ✅ Done

**System.Drawing removal complete.** Only remaining reference is in `Source/Stubs/CeGuiStubs.cs` where it provides implicit conversion operators (`Point↔PointF`, `Size↔SizeF`) for backward compatibility during the transitional period. 

**Plan to remove System.Drawing from CeGuiStubs:**
1. Replace all `PointF`/`SizeF` implicit operators in CeGuiStubs with direct `CeGui.Point`/`CeGui.Size` usage in callers — already done
2. Remove `using System.Drawing;` and the two implicit operator pairs from CeGuiStubs
3. This happens naturally when the real GUI library replaces the stub OR can be done earlier as a standalone cleanup
4. After removal, the entire codebase will have zero `System.Drawing` dependency, improving cross-platform compatibility and reducing assembly load

### Phase 3: Save/Load System
- [x] Replace `BinaryFormatter` with `System.Text.Json` — ✅ Done
- [x] Implement generalized polymorphic converter (`ModelJsonConverter`) — ✅ Done
- [x] Implement `Vector3` dictionary key converter (`Vector3DictionaryConverterFactory`) — ✅ Done
- [x] Create `GameStateSerializer` service with versioned file format — ✅ Done
- [x] Migrate `[NonSerialized]` → `[JsonIgnore]` (8 fields across 5 files) — ✅ Done
- [x] Remove `SaveGameHeader` inner class (replaced by `GameStateSerializer`) — ✅ Done
- [x] Clean up unused imports (`BinaryFormatter`, `XmlSerializer`) — ✅ Done
- [x] Build produces **0 errors** — ✅ Done

**Architecture:**
- `GameStateSerializer` — static service in `Source/Utils/GameStateSerializer.cs`
  - `Save(Stream, GameState, gameVersion)` — writes JSON with format version + metadata
  - `Load(Stream, gameVersion, out error)` — reads JSON, validates version, deserializes
  - `ReadHeader(Stream)` — reads metadata without full deserialization (for grid display)
- `ModelJsonConverter` — custom `JsonConverter<object>` in `Source/Utils/ModelJsonConverter.cs`
  - Auto-discovers all `ProjectXenocide.Model` types via assembly scan at startup
  - Writes `$type` discriminator + `$id`/`$ref` for every model object (handles polymorphism + circular refs)
  - Reads `$type` → resolves type from cache → creates instance via `FormatterServices.GetUninitializedObject` (avoids constructor requirements)
  - Calls `[OnDeserialized]` methods if present (handles `Planet.OnDeserializedMethod`)
  - Thread-static state management ensures no cross-serialization leaks
- `Vector3DictionaryConverterFactory` — handles `Dictionary<Vector3, TValue>` (serializes Vector3 keys as "X,Y,Z" strings)
- Save file format: JSON with `{ formatVersion, savedAt, gameTime, gameVersion, gameState }` wrapper
- No `[JsonDerivedType]` attributes needed — type discovery is fully automatic via assembly scanning
- No parameterless constructors needed — instances created via `FormatterServices.GetUninitializedObject`
- No `ReferenceHandler.Preserve` needed — manual `$id`/`$ref` tracking in the converter

**Key design decisions:**
- Generalized by design: any new type added to `ProjectXenocide.Model` namespace is automatically discovered and serialized
- Versions are forward-compatible: `formatVersion` allows migration in `Load()` method
- `[NonSerialized]` honored as `[JsonIgnore]` — ensures transient runtime state isn't persisted
- `Vector3` dictionary keys (in `Battle.groundContents`) stored as comma-separated floats
- `BinaryFormatter` dependency completely removed from codebase
- `SaveGameHeader` inner class removed (replaced by `GameStateSerializer` wrapper)

### Phase 4: GUI Migration (CeGui# → Gum)

**Decision:** Gum (MonoGameGum) selected — see Section 2 GUI table for full analysis.

**Required NuGet:** `Gum.MonoGame` (NuGet package, one command install)

#### Phase 4.0: Setup & Proof of Concept (Automated + Manual)
- [x] Evaluate and select replacement (Gum selected over MGUI/Myra/GeonBit/ImGui — see docs/legacy/GUI.md) — ✅ Done
- [ ] Add `Gum.MonoGame` NuGet package to project (`dotnet add package Gum.MonoGame`)
- [ ] Initialize `GumService.Default` in `Xenocide.cs` (Initialize, Update, Draw — 3 lines)
- [ ] Create proof-of-concept: render a Gum `StackPanel` with `Button` + `Label` alongside the existing CeGUI# stubs
- [ ] Verify Gum renders on top of existing screen manager output
- [ ] **Manual: You** — verify the Gum tooltip/overlay appears correctly on the 3D scene

#### Phase 4.1: Convert StartScreen (Template / Reference Implementation)
- [ ] Create `StartScreenViewModel` with: version text, debug button visibility flag
- [ ] Replace `CreateCeguiWidgets()` with Gum `StackPanel` + `Button` controls + data binding
- [ ] Attach event handlers to Gum buttons (New Game, Load, Credits, Quit, Debug tests)
- [ ] **Manual: You** — verify all 5 StartScreen buttons work end-to-end (schedule different screens, quit)
- [ ] **Manual: You** — verify version text renders in bottom-right corner
- [ ] **Manual: You** — verify debug-only buttons (Run Tests, Battlescape) only appear in DEBUG builds

#### Phase 4.2: Convert Dialog Base + Simple Dialogs
- [ ] Create Gum-based `Dialog` base class (replacing CeGUI# dialog pattern)
- [ ] Convert `MessageBoxDialog` — Label + Button with Ok
- [ ] Convert `YesNoDialog` — Label + two Buttons
- [ ] Convert `OptionsDialog` — Sliders (volume), Checkboxes (options), ComboBoxes (selections)
- [ ] **Manual: You** — verify dialogs open, close, return correct results
- [ ] **Manual: You** — verify dialog queue and modal behavior (topmost dialog blocks input to screen below)

#### Phase 4.3: Convert Management Screens (Simple → Medium)
- [ ] Convert `CreditsScreen` — static formatted text (Labels + StackPanel)
- [ ] Convert `MonthlyReportScreen` — formatted text + navigation buttons
- [ ] Convert `LoadSaveGameScreen` — ListBox (save slots) + TextBox (rename) + Buttons
- [ ] Convert `StatisticsScreen` — formatted data display
- [ ] Convert `PurchaseScreen` / `SellScreen` — ListBox items + quantity controls + confirm
- [ ] Convert `StoresScreen` — ListBox + filters (ComboBox)
- [ ] Convert `ManufactureScreen` — project list + progress display
- [ ] **Manual: You** — verify each screen displays data correctly and navigation works

#### Phase 4.4: Convert Core Screens (Complex)
- [ ] Convert `ResearchScreen` — ResearchTree (custom ItemsControl + Grid) + description panel + buttons
- [ ] Convert `BasesScreen` — base list (ListBox) + base view (details) + facility grid
- [ ] Convert `EquipSoldierScreen` — soldier list + inventory grid + drag-drop between soldiers and stores
- [ ] **Manual: You** — verify research tree renders hierarchy correctly
- [ ] **Manual: You** — verify drag-and-drop soldier equipment works
- [ ] **Manual: You** — verify base facility grid layout

#### Phase 4.5: Convert 3D-Integrated Screens
- [ ] Convert `GeoscapeScreen` — add Gum overlay (date/time, funds, buttons) on top of 3D globe view
- [ ] Convert `BattlescapeScreen` — add Gum overlay (soldier stats, action buttons, turn info) on top of 3D battlefield
- [ ] Convert `AeroscapeScreen` / `BattlescapeReportScreen` — post-mission summary with Gum
- [ ] **Manual: You** — verify Gum overlay renders correctly on top of 3D (depth/stencil issues)
- [ ] **Manual: You** — verify no input conflicts between Gum and 3D scene (click-through, focus)

#### Phase 4.6: Cleanup
- [ ] Remove `CeGuiStubs.cs` — no longer needed
- [ ] Remove `using CeGui;` and `using CeGui.Renderers.Xna;` from all game files
- [ ] Remove `InitializeCegui()` from `Xenocide.cs`
- [ ] Remove `System.Drawing` using from CeGuiStubs.cs (file will be deleted)
- [ ] **Manual: You** — verify full build with 0 errors
- [ ] **Manual: You** — run game and verify all screens render and function

#### Key Design Decisions for Gum Screen Pattern

```csharp
// New screen pattern:
public class GumStartScreen : Screen
{
    private MainMenuViewModel viewModel;
    private GumService GumUI => GumService.Default;

    public override void Show()
    {
        viewModel = new MainMenuViewModel();
        viewModel.VersionText = Xenocide.CurrentVersion;

        var panel = new StackPanel();
        panel.AddToRoot();
        panel.BindingContext = viewModel;

        // ... add controls ...
    }
}
```

- Screens can share a single `GumService.Default` instance, or each screen can manage its own subtree via `AddToRoot()` / `Root.Children.Clear()`
- Data binding via `SetBinding` replaces manual text/state updates
- Event handlers use Gum's `Click` events (same pattern as CeGUI# `Clicked +=`)

#### What You Need to Do Manually

| Task | Reason |
|------|--------|
| **Verify proof-of-concept renders** | Need visual confirmation that Gum overlay renders correctly on 3D scene |
| **End-to-end button testing** | Each screen's buttons schedule other screens/dialogs — need to verify navigation chain |
| **Verify dialog modality** | Modal dialogs must block input to underlying screen — requires manual testing |
| **Drag-and-drop testing** | EquipSoldierScreen drag-drop is complex interaction — needs human validation |
| **Research tree visual check** | Tree hierarchy rendering correctness |
| **3D overlay testing** | Ensure Gum renders on top of 3D without depth conflicts |
| **Input conflict testing** | Ensure clicks don't pass through Gum to 3D scene (or vice versa) |
| **Final build verification** | Confirm 0 errors, game launches, all screens display correctly |

Everything else (NuGet addition, code changes, control wiring, data binding, event handlers) can be done by automation/tooling.

### Phase 5: Audio Migration
- [x] Audit FMOD Ex usage in AudioSystem (categorize: simple playback vs advanced features) — ✅ Simple playback only; no DSP/3D audio used
- [x] Implement `IAudioSystem` interface — ✅ `GameAudioComponent` in `Source/Audio/GameAudioComponent.cs`
- [x] Implement MonoGame `SoundEffect` backend (replacing `FmodGameComponent` stub) — ✅ All audio via `OggImporter` + `SoundEffectProcessor` in MGCB
- [x] Wire audio into game loop (music per-screen, button click sounds, SFX) — ✅ Done
- [x] Register all 16 `.ogg` audio files in `Content.mgcb` (10 SFX + 6 music) — ✅ Done
- [x] Add `Content.RootDirectory = "Content"` in `Xenocide.cs` constructor — ✅ Required for ContentManager resolution
- [x] Set resolution to 1280×1024 with Alt+Enter fullscreen toggle — ✅ Done
- [x] Fix `Frame.Visible` null-ref crash (`rootWidget` null guard) — ✅ Done
- [x] Auto-wire button click sounds via `GumScreen.WireClickSounds()` — ✅ All ~108 Gum buttons across 25 screens
- [x] Wire CeGui button click sounds in `GeoscapeScreenState.cs` (4 buttons) — ✅ Done
- [x] Stop previous music before playing new (fix overlap when switching screens) — ✅ `StopMusicInternal()` before every `PlayRandomMusic()`

**Architecture:**
- `GameAudioComponent` — `GameComponent` implementing `IAudioSystem` in `Source/Audio/GameAudioComponent.cs`
  - SFX: preloaded via `LoadSound()` into `Dictionary<string, SoundEffect>`; playback via `SoundEffectInstance.Play()`
  - Music: loaded on-demand via `Content.Load<SoundEffect>()`; one active `SoundEffectInstance` at a time
  - Auto-advance: polls `_currentMusic.State == SoundState.Stopped` in `Update()` to play next random track
  - Categories: music tracks tagged by screen (`MainMenu`, `PlanetView`, `BaseView`, `XNet`); `PlayRandomMusic(category)` filters by tag
  - Volume: `MusicVolume`/`SoundVolume` per-instance; `SetMasterVolume()` → `SoundEffect.MasterVolume`
  - Log: writes to `%LOCALAPPDATA%\Xenocide\audio_debug.log`
- `GumScreen.WireClickSounds()` — recursively walks Gum `StackPanel.Children`, finds all `Button` controls, wires `Click +=` to play `Menu\buttonclick1_ok.ogg`
- `GeoscapeScreenState.cs` — 4 CeGui buttons manually wired with click sound lambdas

**Key findings:**
- `SoundEffect.FromStream()` only supports WAV, not OGG — NVorbis is only used by MGCB pipeline
- `SongProcessor` produces broken 128-byte `.xnb` stubs on DesktopGL — must use `SoundEffectProcessor` for everything
- Music `.xnb` files are large (decompressed PCM: 7–52 MB each) but functional
- `ContentManager` requires explicit `Content.RootDirectory = "Content"` — otherwise Gum or CeGui stubs may modify it
- `IAudioSystem` interface kept in `Source/Stubs/AudioSystemStubs.cs`; `GameAudioComponent` lives in `Source/Audio/`
- FMOD Ex wrapper files (`fmod.cs`, `fmod_dsp.cs`, `fmodex.dll`) remain in legacy branches only — not carried forward

### Phase 6: Content Pipeline Rebuild
- [x] Import 3D models (.fbx, .x) into MGCB — ✅ 37 model entries registered (all .X files + most .FBX)
- [x] Update shaders (.fx) for D3D11 profile — ✅ Already in .mgcb (GeoscapeShader, skybox)
- [x] Set up spritefont in MGCB — ✅ SpriteFont1 registered
- [x] Import all textures — ✅ 3 textures in .mgcb; others loaded via direct file (Texture2D.FromFile)
- [x] Register all 16 audio .ogg files — ✅ Done in Phase 5
- [x] Add missing facility models (17 .x files) — ✅ generalstorage through neural_shield
- [x] Add missing XCorps.X default model — ✅ Used by XNet screen
- [x] Fix `Content\` double-prefix path bugs — ✅ XNetScene.cs + CreditsScreen.cs
- [ ] 3 FBX models too old for AssImp (pre-2011): `FemaleShirt.FBX`, `Viper.FBX`, `Laser Rifle.FBX` — runtime try-catch handles gracefully
- [ ] Content pipeline: additional missing textures pending (EarthDiffuseMap, screen backgrounds, etc.)

**Key findings:**
- MGCB's `FbxImporter` only supports FBX 2011–2013; legacy FBX files (pre-2011) fail with `FBX-DOM unsupported`
- 17 facility models + XCorps.X + Barracks.FBX were never registered — caused `ContentLoadException` at runtime
- `Content.Load<T>()` paths must NOT include the `Content\` prefix — ContentManager prepends it automatically
- `Texture2D.FromFile()` and `TextureAtlas.LoadContent()` load textures directly — do not need MGCB

### Phase 7: Cross-Platform Validation
- [ ] Build and run on **Windows**
- [ ] Build and run on **Linux** (test basic gameplay flow)
- [ ] Fix file path casing issues (Linux is case-sensitive)
- [ ] Fix path separator issues (`\` → `/` or `Path.Combine()`)
- [ ] Performance profile on OpenGL backend
- [ ] Test save/load cross-platform compatibility

### Phase 8: Cleanup & Polish
- [ ] Remove old XNA 3.0 project files from active tree (archive if needed)
- [ ] Remove Dependancies/ directory (CeGui, FMOD source no longer needed)
- [ ] Remove old Lib/ directory (NUnit goes into NuGet)
- [ ] Remove old Installers/ directory (NSIS + Inno Setup scripts)
- [ ] Update README.md with final build instructions
- [ ] Update CI/CD if applicable

---

## 4. What You Need to Install

### Required (not yet installed)

| Software | Why | Download / Command |
|----------|-----|-------------------|
| **.NET SDK 9.0** (or 10.0) | Core build tool — only runtimes are present currently | [dotnet.microsoft.com](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) or install script |
| **MonoGame templates** | `dotnet new mgdesktopgl` | `dotnet new install MonoGame.Templates.CSharp` |
| **MGCB tools** | Content pipeline (models, shaders, fonts) | `dotnet tool install -g dotnet-mgcb dotnet-mgcb-editor` |

### Already Available

| Tool | Status |
|------|--------|
| **Git** 2.54.0 | ✅ Available |
| **VS Code** | ✅ Available at `C:\Program Files\Microsoft VS Code\bin\code.cmd` |
| **.NET Runtimes** | ✅ 6.0, 7.0, 8.0, 9.0, 10.0 installed (but no SDK) |

### Notes

- **No Visual Studio required** — MonoGame works from the `dotnet` CLI and VS Code. The old XNA 3.0 solution required Visual Studio 2008 and XNA Game Studio, but those are only needed for the original codebase.
- **XNA 3.0→4.0 API conversion targets MonoGame directly** — we don't need an intermediate XNA 4.0 test step. The conversion is a source-level mechanical change applied while porting to the new MonoGame project.
- **MonoGame content pipeline** (MGCB) handles .fbx, .fx, .spritefont, and textures natively into .xnb format via the `MonoGame.Content.Builder.Task` NuGet package.

---

## 5. Next Steps (Recommended Order)

### Immediate (get the game running)
1. ~~**Content Pipeline Setup**~~ ✅ Done — 37 models, 2 shaders, 1 font, 3 textures, 16 audio files registered in MGCB
2. ~~**Replace audio stubs**~~ ✅ Done — Phase 5 complete
3. **Remaining Content Gaps** — Register missing textures (EarthDiffuseMap, screen backgrounds) in MGCB; evaluate converting old-format FBX files
4. **Replace CeGui# stubs with Gum** — Phase 4.2–4.6 pending (dialogs, management screens, core screens, 3D overlays)
5. **Replace NUnit with xUnit.net** — Phase 0 remaining item
6. **Cross-platform validation** — Phase 7 (build on Linux, fix path case issues)
7. **Cleanup** — Phase 8 (remove old projects, dependencies, installers)

**GUI decision made:** Gum (MonoGameGum) selected. Full analysis in `docs/legacy/GUI.md` and `docs/GUI.md`.
