# Gum UI Conversion — Status & Implementation Plan

## Current Status (May 2026)

### Completed
| Item | Status |
|------|--------|
| `.gumx` project at `Content/Gum/Xenocide.gumx` | ✅ 1280×1024 canvas, multi-resolution previews |
| `XenocideButton` component | ✅ 3-slice button (Left/Middle/Right Sprites) + ButtonBehavior, state categories (Enabled/Pushed/Highlighted) with TaharezLook texture coordinates |
| `GumService.Initialize(this, "Gum/Xenocide.gumx")` | ✅ Loads project at startup, returns `GumProjectSave` |
| `GumScreen` dual-mode loading | ✅ `Show()` tries `ScreenSave.ToGraphicalUiElement()` by CeguiId; falls back to programmatic |
| `WireButton()` helper | ✅ Wires click sound + handler for named Gum buttons |
| `StartScreen` fully Gum-native | ✅ `.gusx` layout + `CreateGumControls()` wires events |
| `.csproj` content copy | ✅ All Gum file types + Textures copied to output |
| Button stretch fix | ✅ MiddleSlice = Sprite, X=35, Width=-38 (RelativeToContainer), RightSlice X=0 (right-anchored) |
| Button labels fixed | ✅ Set via `.gusx` state variables |
| Version centralized | ✅ `Xenocide.GameVersion = "0.4"` |
| CreditsScreen Escape fix | ✅ `HandleEscape()` override returns StartScreen |
| Large texture fallback | ✅ `EarthGlobe.TryLoadTexture()` falls back to `_LEGACY_*` files |

### Architecture
```
GumScreen.Show()
  ├── TryLoadScreenFromGumx(CeguiId)
  │     └── gumProject.Screens.Find(s => s.Name == screenName)?.ToGraphicalUiElement()
  ├── GumRoot ≠ null → AddToRoot(), CreateGumControls() wires named controls
  └── GumRoot == null → programmatic fallback (RootContainer StackPanel)
```

---

## Phase Overview

| Phase | Screens | Effort Per Screen |
|-------|---------|-------------------|
| **Phase 1** ✅ Running | AeroscapeScreen, BattlescapeScreen, CreditsScreen | 1-2 Gum sessions |
| **Phase 2** | StoresScreen, ShowTransfersScreen, MonthlyCostsScreen, MonthlyReportScreen, BattlescapeReportScreen | 1 Gum session each |
| **Phase 3** | ResearchScreen, PurchaseScreen, SellScreen, BaseInfoScreen, MakeTransferScreen | 2 Gum sessions each |
| **Phase 4** | BasesScreen, EquipCraftScreen, ManufactureScreen, SoldiersListScreen, LoadSaveGameScreen, EquipSoldierScreen, AssignToCraftScreen | 3-4 Gum sessions each |
| **Phase 5** | GeoscapeScreen, XNetScreen, StatisticsScreen | 3-5 Gum sessions each (3D scenes) |

---

## Phase 1: Simple Overlay Screens

### AeroscapeScreen
| Property | Value |
|----------|-------|
| **CeguiId** | `AeroscapeScreen` |
| **Background** | `GeoscapeScreenBackground.png` (default) |
| **Buttons (3)** | `realTimeBtn` ("Real Time"), `advanceTimeBtn` ("Advance Time"), `closeBtn` ("Close") |
| **Labels (5)** | `craftNameLabel`, `craftDamageLabel`, `pod1Label`, `pod2Label`, `logLabel` |
| **Layout** | ButtonPanel (right, TopToBottom) + InfoLabelContainer (left/center) |

### BattlescapeScreen
| Property | Value |
|----------|-------|
| **CeguiId** | `BattlescapeScreen` |
| **Background** | `BasesScreenBackground.png` |
| **Viewport** | Left 74.5% — UI on right 25.5% |
| **Buttons (5)** | `equipmentButton`, `rightHandButton`, `finishTurnButton`, `topLevelButton`, `abortButton` |
| **Labels (1)** | `combatantStatsTextWindow` (initially hidden) |

### CreditsScreen
| Property | Value |
|----------|-------|
| **CeguiId** | `CreditsScreen` |
| **Approach** | No Gum controls needed — uses SpriteBatch for scrolling text. Keep `CreateGumControls()` empty. |

---

## Phase 2: Single-Grid Screens (Template Pattern)

All share: Background + GridContainer (named `GridArea`) + BottomBar (buttons).

| Screen | CeguiId | Buttons | Labels | Grid Columns |
|---|---|---|---|---|
| StoresScreen | `StoresScreen` | `okButton` (1) | — | Item, Qty, Space Used |
| ShowTransfersScreen | `ShowTransfers` | `closeButton` (1) | — | Item, Qty, ETA |
| MonthlyCostsScreen | `MonthlyCosts` | `closeButton` (1) | — | Name, Per Unit, Qty, Total |
| MonthlyReportScreen | `MonthlyReportScreen` | `okButton` (1) | `monthText`, `scoreText` | Country, Attitude, Funds, Change |
| BattlescapeReportScreen | `BattlescapeReportScreen` | `okButton` (1) | `recoveredLabelText` | 2 grids |

**Gum template**: BGImage → TopBar → GridContainer → BottomBar

---

## Phase 3: Management Screens

### ResearchScreen
| Property | Value |
|----------|-------|
| **CeguiId** | `Research` |
| **Buttons (5)** | `addIdleScientistsButton`, `moreScientistsButton`, `lessScientistsButton`, `removeAllScientistsButton`, `closeButton` |
| **Labels (1)** | `availableText` (idle scientists) |
| **Grid** | Project, Scientists, ETA |

### BaseInfoScreen
| Property | Value |
|----------|-------|
| **CeguiId** | `BaseInfoScreen` |
| **Buttons (4)** | `transfersButton`, `storesButton`, `costsButton`, `okButton` |
| **Special** | ComboBox `outpostsListComboBox`, TextBox `nameEditBox` |
| **Grids (2)** | Staff, Facilities |

### PurchaseScreen / SellScreen
| Screen | CeguiId | Buttons (4) | Labels (2) |
|---|---|---|---|
| PurchaseScreen | `PurchaseScreen` | `buyMoreButton`, `buyLessButton`, `confirmButton`, `cancelButton` | `fundsText`, `totalCostText` |
| SellScreen | `SellScreen` | `sellMoreButton`, `sellLessButton`, `confirmButton`, `cancelButton` | `fundsText`, `totalValueText` |

### MakeTransferScreen
| Property | Value |
|----------|-------|
| **CeguiId** | `MakeTransferScreen` |
| **Buttons (4)** | `moveMoreButton`, `moveLessButton`, `confirmButton`, `cancelButton` |
| **Labels (2)** | `sourceText`, `totalCostText` |
| **Special** | ComboBox `outpostsListComboBox` |

---

## Phase 4: Heavy Screens

### BasesScreen
| Property | Value |
|----------|-------|
| **CeguiId** | `BasesScreen` |
| **Buttons (11)** | `newBaseButton`, `baseInfoButton`, `soldiersButton`, `equipCraftButton`, `buildFacButton`, `produceButton`, `transferButton`, `buyButton`, `sellButton`, `geoscapeButton` |
| **Labels (1)** | `fundsText` |
| **ComboBox (1)** | `basesListComboBox` |
| **3D Scene** | FacilityScene, viewport left 66% |

### ManufactureScreen
| Property | Value |
|----------|-------|
| **CeguiId** | `Manufacture` |
| **Buttons (8)** | `buildMoreButton`, `buildLessButton`, `cancelBuildButton`, `addIdleEngineersButton`, `moreEngineersButton`, `lessEngineersButton`, `removeAllEngineersButton`, `closeButton` |
| **Labels (1)** | `availableText` |
| **Grids (2)** | Project + Requirements |

### SoldiersListScreen
| Property | Value |
|----------|-------|
| **CeguiId** | `SoldiersListScreen` |
| **Buttons (4)** | `psiTrainButton` (conditional), `craftButton`, `equipButton`, `closeButton` |
| **Labels (1)** | `nameEditBox` (soldier name) |
| **Grids (2)** | Soldier list + Attributes |

### LoadSaveGameScreen
| Property | Value |
|----------|-------|
| **CeguiId** | `LoadSaveGameScreen` |
| **Buttons (3)** | `saveButton` (changes text per mode), `deleteButton`, `cancelButton` |
| **TextBox (1)** | `filenameEditBox` |
| **Grid (1)** | Saved games list |

### EquipCraftScreen
| Property | Value |
|----------|-------|
| **CeguiId** | `EquipCraftScreen` |
| **Buttons (5)** | `emptyPod1Button`, `emptyPod2Button`, `setPod1Button`, `setPod2Button`, `closeButton` |
| **Labels (3)** | `baseNameText`, `pod1Text`, `pod2Text` |
| **Grids (2)** | Craft list + Weapons |

### AssignToCraftScreen
| Property | Value |
|----------|-------|
| **CeguiId** | `AssignToCraftScreen` |
| **Buttons (7)** | `addXcapButton`, `removeXcapButton`, `addSoldierButton`, `removeSoldierButton`, `soldierUpButton`, `soldierDownButton`, `closeButton` |
| **Grids (3)** | Craft, Soldiers, XCaps |

### EquipSoldierScreen
| Property | Value |
|----------|-------|
| **CeguiId** | `EquipSoldierScreen` |
| **Buttons (3)** | `closeButton`, `leftButton`, `rightButton` |
| **Labels (9)** | 8 static equipment zone labels + `ammoText` |
| **3D Scene** | EquipSoldierScene |
| **Note** | Skip for now — drag-drop and 3D scene make this the most complex screen |

---

## Phase 5: 3D Scene + Stateful Screens

### GeoscapeScreen
| Property | Value |
|----------|-------|
| **CeguiId** | `GeoscapeScreen` |
| **Background** | `GeoscapeScreenBackground.png` |
| **Viewport** | Left 74.5%, UI right 25.5% |
| **Buttons (16)** | 4 time speed + 6 navigation + 6 camera |
| **Labels (7)** | Time display, funds, tooltip |
| **States** | 5 ScreenStates add/remove buttons dynamically |
| **Note** | Most complex — time panel, speed buttons, nav buttons, camera d-pad need careful Gum layout |

### XNetScreen
| Property | Value |
|----------|-------|
| **CeguiId** | `XNetScreen` |
| **Buttons (1)** | `closeButton` |
| **ListBoxes (2)** | `entriesTree`, `textWindow` |
| **3D Model** | XNetScene |

### StatisticsScreen
| Property | Value |
|----------|-------|
| **CeguiId** | `StatisticsScreen` |
| **Buttons (6)** | `ufoByRegionButton`, `ufoByCountryButton`, `xcomByRegionButton`, `xcomByCountryButton`, `fundsButton`, `geoscapeButton` |
| **ListBox (1)** | `seriesList` |
| **3D Scene** | StatisticsScene graph |

---

## CeguiId → .gumx Name Mapping

| CeguiId | .gumx Screen Name | Status |
|---------|-------------------|--------|
| `StartScreen` | `StartScreen` | ✅ Match |
| `AeroscapeScreen` | `AeroscapeScreen` | ✅ New |
| `BattlescapeScreen` | `BattlescapeScreen` | ✅ Renamed from BattleScape |
| `GeoscapeScreen` | `GeoscapeScreen` | ⚠️ Rename from GeoScape |
| `XNetScreen` | `XNetScreen` | ⚠️ Rename from XNet |
| `BasesScreen` | `BasesScreen` | Created (empty) |
| All others | Must match CeguiId | Create when needed |

---

## Per-Screen Gum Editor Checklist

1. **Create screen** at 1280×1024
2. **Background Sprite**: `BGImage`, SourceFile=appropriate PNG, Width=100% Height=100%
3. **Button Panel**: `ButtonPanel` Container, ChildrenLayout=TopToBottom, positioned on side/bottom
4. **Add XenocideButton** instances — name matches C# variable, set `ButtonText.Text` in state
5. **Add Text instances** for dynamic labels — name matches C# variable
6. **Add GridContainer** — empty Container named `GridArea` for programmatic grid population
7. **Add ComboBox/TextBox** from Controls if needed
8. **In C#**: `WireButton("Name", handler)` for buttons, `GetFrameworkElementByName<Label>("Name")` for labels
9. **Set visibility**: elements that start hidden need `Visible = false` in `.gusx` state

---

## Backlog
- [ ] CeGui stubs teardown (`Frame.cs`, `Screen.cs`, `Xenocide.cs` InitializeCegui, `ScreenManager.cs` RootGuiSheet)
- [ ] Software cursor polish (hotspot, context-sensitive cursors, HW/SW toggle)
- [ ] Dialogs GumX conversion (13 dialogs)
