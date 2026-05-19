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

### GUI (CeGui# Replacement — To Be Decided)
| Option | Stars | Status | Approach |
|--------|-------|--------|----------|
| **Gum** (MonoGameGum) | ~500 | Active (2026) | WYSIWYG layout editor, retained-mode UI |
| **MGUI** | ~400 | Active | WPF-style XAML layouts, rich widgets |
| **GeonBit.UI** | ~350 | Active | Pre-styled, rapid setup |
| **Myra** | ~855 | Active (2026) | XML-based layouts, also works with MonoGame |
| **ImGui.NET** | ~1k | Active | Immediate-mode (less ideal for complex game menus) |

### Audio (FMOD Ex Replacement — To Be Decided)
| Option | Status | Notes |
|--------|--------|-------|
| **Built-in SoundEffect/Song** | Part of MonoGame | OGG/WAV for SFX, OGG/MP3 for music. Simplest path. |
| **FmodForFoxes** (NuGet) | Active | FMOD Studio wrapper for .NET, works with MonoGame |
| **Keep FMOD Ex via P/Invoke** | Legacy | FMOD Ex is old; will need updated bindings |

---

## 3. Migration Phases & Roadmap

### Phase 0: Tooling & Setup
- [x] **Install .NET SDK 9.0** — ✅ Done (detected on system)
- [x] Install MonoGame templates: `dotnet new install MonoGame.Templates.CSharp` — ✅ Done
- [x] Install MGCB tools: `dotnet tool install -g dotnet-mgcb dotnet-mgcb-editor` — ✅ Done
- [x] Create new MonoGame DesktopGL project: `dotnet new mgdesktopgl -o xna/trunk/Xenocide.MonoGame` — ✅ Done
- [x] Add NuGet packages: `MonoGame.Framework.DesktopGL`, `MonoGame.Content.Builder.Task` — ✅ Done
- [ ] Set up MGCB content project (`.mgcb`) with all asset references
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

### Phase 3: Save/Load System
- [ ] Remove `BinaryFormatter` (deprecated, Windows-only)
- [ ] Implement `System.Text.Json` serialization for `GameState`
- [ ] Add `[JsonPropertyName]` attributes to serialized classes
- [ ] Test save/load round-trip

### Phase 4: GUI Migration (CeGui# Replacement)
- [ ] Evaluate and select replacement (propose: Gum or MGUI)
- [ ] Build prototype of StartScreen in chosen framework
- [ ] Port **core screens**: StartScreen, GeoscapeScreen, BattlescapeScreen, BasesScreen, ResearchScreen
- [ ] Port **management screens**: PurchaseScreen, SellScreen, StoresScreen, EquipSoldierScreen, ManufactureScreen
- [ ] Port **auxiliary screens**: CreditsScreen, MonthlyReportScreen, StatisticsScreen, XNetScreen, LoadSaveGameScreen
- [ ] Port **dialog boxes**: MessageBoxDialog, YesNoDialog, OptionsDialog, etc.
- [ ] Convert `.layout` XML files to new format

### Phase 5: Audio Migration
- [ ] Audit FMOD Ex usage in AudioSystem (categorize: simple playback vs advanced features)
- [ ] Implement `IAudioSystem` interface
- [ ] If simple playback: implement MonoGame `SoundEffect`/`Song` backend
- [ ] If advanced features needed: implement FmodForFoxes backend
- [ ] Wire audio into game loop

### Phase 6: Content Pipeline Rebuild
- [ ] Import 3D models (.fbx, .x) into MGCB
- [ ] Update shaders (.fx) for D3D11 profile → compile via MGFXC
- [ ] Set up spritefont in MGCB (verify font availability)
- [ ] Import all textures → verify correct processing
- [ ] Test all `Content.Load<T>()` calls

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
1. **Content Pipeline Setup** — Create `.mgcb` project, import all .fbx/.fx/.spritefont/.jpg/.png assets, verify `Content.Load<T>()` works at runtime
2. **Replace CeGui# stubs with real implementation** — Evaluate Gum/MGUI/Myra for the GUI replacement, or flesh out stubs enough to render basic screens
3. **Replace audio stubs** — Implement the MonoGame `SoundEffect`/`Song` backend for `IAudioSystem`
4. **Runtime test** — Launch the game, fix any `NullReferenceException` / `MissingMethodException` from stub methods

### Short-term (functionality)
5. **Save/Load System** — Replace `BinaryFormatter` with `System.Text.Json`
6. **Remove remaining `System.Windows.Forms` references** — Replace message boxes with in-game dialogs
7. **Add xUnit.net test project** — Port critical tests from NUnit 2.2.9

### Medium-term (stability)
8. **Cross-platform validation** — Build and run on Linux; fix path casing, separator issues
9. **Performance profiling** — OpenGL backend optimization
10. **Cleanup** — Remove old XNA 3.0 project files, `Dependencies/`, `Lib/`, `Installers/` directories
