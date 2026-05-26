# Gum UI Conversion — Status & Implementation Plan

## Current Status (May 2026)

### Completed
| Item | Status |
|------|--------|
| `.gumx` project at `Content/Gum/Xenocide.gumx` | 1280x1024 canvas, 23 screen references |
| `XenocideButton` component | 3-slice button (Left/Middle/Right Sprites) + ButtonBehavior, state categories (Enabled/Pushed/Highlighted) with TaharezLook texture coordinates |
| `GumService.Initialize(this, "Gum/Xenocide.gumx")` | Loads project at startup, returns GumProjectSave |
| `GumScreen` dual-mode loading | `Show()` loads `.gusx` by CeguiId; programmatic fallback gated by `EnableProgrammaticFallback` (default false) |
| `WireButton()` helper | Wires click sound + handler for named Gum buttons |
| All 27 screens converted to Gum | `.gusx` layouts + `CreateGumControls()` wire events |
| All CeGui# removed | Stubs deleted, usings cleaned, Frame/Screen/Dialog rewritten |
| Programmatic control positioning | 14 screens fixed (Labels, GridPanels, ListBoxes, ComboBoxes, TextBoxes had no X/Y in GumX path) |
| Content preloading | `ContentCache` preloads Earth textures + skybox + all XNet models at startup (~4s one-time) |
| ProfileTimer | Scoped `using`-based profiling utility with console output |
| XNet text performance | `PopulateEntryText` 88x speedup (2660ms → 30ms) by removing CeGui-era per-line word wrapping |
| XNet scaling matrix cached | `CachedModel` stores model + precomputed scaling matrix |
| Save/load UX | Success messages, mode-dependent button text, path fixes, IntPtr serialization fix |
| BasesScreen NRE fix | WireButton results saved to class fields |
| Content cache | `ContentCache` stores Texture2D and `CachedModel` (Model + Matrix) |

### Architecture
```
GumScreen.Show()
  ├── TryLoadScreenFromGumx(CeguiId)
  │     └── gumProject.Screens.Find(s => s.Name == screenName)?.ToGraphicalUiElement()
  ├── GumRoot ≠ null → AddToRoot(), CreateGumControls() wires named controls
  └── GumRoot == null → throws (EnableProgrammaticFallback defaults to false)
```

### CeGui Teardown Summary
| File | Action |
|------|--------|
| `CeGuiStubs.cs` | Deleted |
| `IScreenManager.cs` | Deleted |
| `Frame.cs` | Rewritten — removed all CeGui methods/fields; kept core infrastructure |
| `Screen.cs` | Rewritten — removed CeGui background loading |
| `Dialog.cs` | Rewritten — removed CeGui.Window dependency |
| `GumDialog.cs` | Cleaned — removed CeGui overrides |
| `ScreenManager.cs` | Cleaned — removed RootGuiSheet, GuiBuilder |
| `Xenocide.cs` | Cleaned — removed InitializeCegui, GuiManager, CeGui usings |
| `GeoscapeScreen.cs` | Cleaned — removed AddButtonSound calls |
| `BugRegressionTests.cs` | Cleaned — removed 2 CeGui-dependent tests |

---

## Per-Screen Positioning Fixes

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

## CeguiId → .gumx Name Mapping

| CeguiId | .gumx Screen Name | Status |
|---------|-------------------|--------|
| `StartScreen` | `StartScreen` | ✅ |
| `AeroscapeScreen` | `AeroscapeScreen` | ✅ |
| `BattlescapeScreen` | `BattlescapeScreen` | ✅ |
| `GeoscapeScreen` | `GeoscapeScreen` | ✅ |
| `XNetScreen` | `XNetScreen` | ✅ |
| `BasesScreen` | `BasesScreen` | ✅ |
| All others | Must match CeguiId | ✅ All 23 match |

---

## Backlog
- [ ] Dialog `.gusx` conversion — 13 dialogs currently programmatic
- [ ] Software cursor polish — hotspot, context-sensitive cursors, HW/SW toggle
- [ ] GridPanel XenocideButton styling — `RowButtonFactory` property exists but styling approach needs rework (InteractiveGue can't wrap hierarchical GUEs)
- [ ] Cross-platform validation — build on Linux
- [ ] Remaining FBX model textures — add to MGCB
- [ ] Update README with final build instructions
