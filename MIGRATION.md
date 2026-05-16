# Migration Plan: XNA 3.0 → MonoGame

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
| `Microsoft.Xna.Framework.Content` | 3.0.0.0 | ContentManager.Load<T>() | ~45 |
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
| **NUnit** | 2.2.9.0 | Unit testing (test project only) | Tests only |

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
| .NET SDK | 8.0+ (target .NET 8.0 LTS) | Runtime target |
| `System.Text.Json` | Built-in | Replace BinaryFormatter for save/load |

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
| **Myra** | ~855 | Active (2026) | XML-based layouts (FNA-native, works with MG) |
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
- [ ] **Install .NET SDK 9.0** (or 10.0) — currently **NOT installed** (runtimes only)
- [ ] Install MonoGame templates: `dotnet new install MonoGame.Templates.CSharp`
- [ ] Install MGCB tools: `dotnet tool install -g dotnet-mgcb dotnet-mgcb-editor`
- [ ] Create new MonoGame DesktopGL project
- [ ] Set up MGCB content project (`.mgcb`) with all asset references
- [ ] Restore source file structure into new project

### Phase 1: XNA 3.0 → 4.0 API Conversion
- [ ] Replace `effect.Begin()/End()` → `Pass.Apply()` only
- [ ] Replace `GraphicsDevice.RenderState.*` → state objects (BlendState, DepthStencilState, RasterizerState)
- [ ] Replace `GraphicsDevice.VertexDeclaration` assignments → remove
- [ ] Replace `GraphicsDevice.Vertices[0].SetSource()` → `SetVertexBuffer()`
- [ ] Replace `SpriteBatch.Begin(blend, sort, save)` → `Begin(sort, blend)`
- [ ] Replace `ResolveTexture2D` → `RenderTarget2D`
- [ ] Replace point sprites → triangle-based rendering
- [ ] Move `Color` from `Microsoft.Xna.Framework.Graphics` → `Microsoft.Xna.Framework`
- [ ] Replace `StorageDevice`/`StorageContainer` → `System.IO` file operations
- [ ] Remove `GamerServices` references
- [ ] Update `VertexElement` constructors

### Phase 2: Platform-Specific Code Cleanup
- [ ] Replace `System.Windows.Forms.MessageBox` → in-game message dialog
- [ ] Replace `System.Windows.Forms.MouseButtons` → MonoGame Input helpers
- [ ] Replace `System.Drawing.SizeF`/`PointF` → custom structs or Vector2
- [ ] Replace `System.Drawing.Rectangle` → `Microsoft.Xna.Framework.Rectangle`
- [ ] Replace `ErrorDialogue` WinForms Form → in-game or console error display

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
- [ ] Update README.md with new build instructions
- [ ] Update CI/CD if applicable

---

## 4. What You Need to Install

### Required (not yet installed)

| Software | Why | Download |
|----------|-----|----------|
| **.NET SDK 9.0** (or 10.0) | Core build tool — only runtimes are present currently | https://dotnet.microsoft.com/en-us/download/dotnet/9.0 |
| **MonoGame templates** | `dotnet new mgdesktopgl` project template | `dotnet new install MonoGame.Templates.CSharp` |
| **MGCB tools** | Content pipeline (model/spritefont/shader compilation) | `dotnet tool install -g dotnet-mgcb` |

### Already Available

| Tool | Status |
|------|--------|
| **Git** 2.54.0 | ✅ Available |
| **VS Code** | ✅ Available at `C:\Program Files\Microsoft VS Code\bin\code.cmd` |
| **.NET Runtimes** | ✅ 6.0, 7.0, 8.0, 9.0, 10.0 installed (but no SDK) |

### Notes

- **No Visual Studio required** — the MonoGame DesktopGL template works entirely from the `dotnet` CLI and VS Code. The old XNA 3.0 solution (`Xenocide.sln`) required Visual Studio 2008 and XNA Game Studio, but those are only needed for the original codebase, not the migrated one.
- **XNA 3.0→4.0 API conversion will be done in code alone** — you don't need the old XNA toolchain installed unless you want to build the original project for reference. The conversion is a source-level mechanical change based on the well-documented API differences.
- **For content (.fx shaders, .fbx models):** MGCB handles these natively. If you need to reference the original compiled output, the `xna/trunk/Xenocide/Content/` folder has the source assets. The MGCB tool re-imports from source, not from old `.xnb` files.
