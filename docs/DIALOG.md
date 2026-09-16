# Dialog & Message System

## Implementation Status (May 2026)

| Phase | Status |
|-------|--------|
| `MessageLog` static queue + GameState persistence | ✅ Implemented |
| `ModalDialog` base class (overlay + title bar + close X + centering) | ✅ Implemented |
| All 11 dialogs extend `ModalDialog` | ✅ Done |
| `GumDialog` deleted (fully replaced) | ✅ Done |
| `ToastNotification` component (auto-fading popups) | ✅ Implemented |
| `NotificationMapping` specs (type + pause + toast + dedup + action) | ✅ Implemented |
| `ScreenManager.PostMessage()` | ✅ Implemented |
| GeoscapeScreen **situation log** panel (themed, header, auto-scroll) | ✅ Implemented |
| Envelope/INBOX badge + `PendingActionsDialog` (Dismiss / Dismiss All) | ✅ Implemented (context actions partial) |
| GeoEvent integration (non-blocking notifications) | ✅ Done for the 4 game-event GeoEvents; validation modals intentionally kept |
| Per-event toggles + pause-on-alert + toast switch (persisted) | ✅ Implemented (`SettingsScreen.ShowNotificationTab` → `GameOptions`) |
| De-duplication / grouping of repeats | ✅ Implemented (`MessageLog` dedup window) |
| Global "M" hotkey → situation report | ✅ Implemented (`Xenocide.Update`) |

## Overview

Xenocide's dialog system handles three categories of user interaction:

| Category | Behavior | Examples |
|----------|----------|----------|
| **Critical (Required)** | Blocks gameplay, demands user action. Can be temporarily dismissed but stays in a pending queue with an envelope indicator. Only removed when explicitly actioned or expired. | Tracking lost, Battlescape start confirmation, Game over |
| **Informational (Blocking)** | Blocks gameplay, user clicks OK to dismiss. Used for validation errors and transient confirmations. | Duplicate filename, insufficient funds, base needs name |
| **Notification (Non-blocking)** | Does NOT pause gameplay. Appears as a toast + persists in the message log. | Fuel low, research finished, facility built, item arrived |

## Current State (May 2026)

### Inheritance Hierarchy

```
Frame  (UI/Screens/Frame.cs)
  ├── Dialog  (UI/Dialogs/Dialog.cs) — "modal popup dialogs"
  │     └── ModalDialog  (UI/Dialogs/ModalDialog.cs, abstract)
  │           ├── GumMessageBoxDialog    (simple OK message)
  │           ├── GumYesNoDialog         (Yes/No or OK/Cancel)
  │           ├── GumOptionsDialog       (Load/Save/Sound/Abandon menu)
  │           ├── SoundOptionsDialog     (music/sound volume controls)
  │           ├── LaunchInterceptDialog  (select craft to intercept)
  │           ├── NameNewBaseDialog      (name input for new base)
  │           ├── AlienMissionDialog     (debug cheat: select alien mission)
  │           ├── StartBattlescapeDialog (confirm/auto-complete battlescape)
  │           ├── TrackingLostDialog     (what to do when tracking lost)
  │           ├── BuildFacilityDialog    (select facility to build)
  │           └── PickActionDialog       (combat actions in battlescape)
  └── Screen  (UI/Screens/Screen.cs) — "full screens with background"
```

### Rendered Appearance

All dialogs share the same rendering approach, implemented in `ModalDialog`:
- **Title bar** — a 28px bar (`ColorCategoryState = Primary`) with the dialog title on the left and a close "X" button on the right.
- **Programmatic panel** — `ModalDialog` builds a `StackPanel` (`_panel`) sized from `PanelWidth`/`PanelHeight` and centered using the live `GraphicsDevice.Viewport`.
- **Content area** — subclasses add controls via `ContentArea` in `CreateDialogWidgets()`; `AddButton()` wraps `ThemedButton`.
- **Background dim** — `ScreenManager.DrawDialogOverlay()` draws a semi-transparent black quad over the scene between the 3D scene and the Gum dialogs.

### How Dialogs Are Shown and Dismissed

**ScreenManager manages two collections:**

```csharp
queuedDialogs: List<Dialog>      // FIFO queue — items wait here until no dialog is showing
showingDialogs: Stack<Dialog>    // LIFO stack — currently visible dialogs
```

**Lifecycle:**

1. **ShowDialog(dialog)** — saves screen state, disables current screen, pushes onto `showingDialogs`, calls `dialog.Show()`.
2. **CloseDialog(dialog)** — pops from `showingDialogs`, calls `dialog.Dispose()`, re-enables next frame.
3. **QueueDialog(dialog)** — adds to `queuedDialogs`. Not widely used — most callers use `ShowDialog()` directly.
4. **Update() loop** — if no dialogs showing AND queue has items, pops from queue and shows. If dialogs are showing, the entire screen update is **skipped** — game time stops entirely.

### Blocking Behavior

When ANY dialog is visible (even a simple "OK" message box), the `ScreenManager.Update()` method skips the screen's update. This means:
- Game time pauses (GeoTime.StopTime() is not called, but the update loop simply doesn't run screen.Update())
- No input is processed by the game — only the dialog receives input
- All geoscape events (UFO movement, craft movement, time progression) freeze

This was the default behavior in the CeGui system because dialogs were truly modal OS-level windows. In Gum, we can choose whether to block or not.

### ESC Handling

- **`ScreenManager.HandleEscapeKey()`** detects the ESC edge. If dialogs are showing, it calls `showingDialogs.Peek().HandleEscape()` and only falls back to `CloseDialog` if the dialog returns false.
- **`ModalDialog.HandleEscape()`** returns true and calls `Dismiss()`, which invokes `DismissAction` (cancel/cleanup) before closing.
- **For `GumYesNoDialog`**, ESC triggers the registered `NoAction`.
- **For `SoundOptionsDialog`**, ESC restores the previous volumes via `DismissAction`.

### Message Display Flow

**`Util.ShowMessageBox()`** (~59 call sites across 17 files):
```csharp
public static void ShowMessageBox(string format, params Object[] args)
{
    Xenocide.ScreenManager.ShowDialog(
        new GumMessageBoxDialog(Util.StringFormat(format, args)));
}
```
Creates a blocking `GumMessageBoxDialog` with the formatted message text.

**`ScreenManager.ShowDialog()`** (~16 direct call sites):
- Used for dialogs that need custom behavior (LaunchInterceptDialog, BuildFacilityDialog, SoundOptionsDialog, etc.)
- Also used for YesNo prompts and confirmation dialogs.

**All messages are blocking.** There is no non-blocking notification channel, no message history, and no log.

### Resolved Gaps

| Gap | Status |
|-----|--------|
| Message log | ✅ `MessageLog` static queue + `GameState.MessageLogEntries` persistence + Geoscape `ListBox` panel |
| Dialog chrome | ✅ Title bar + close "X" + centered panel + background dim (`ScreenManager.DrawDialogOverlay`) |
| ESC discards callbacks | ✅ ESC now routes through `ModalDialog.HandleEscape()` → `Dismiss()` → `DismissAction` |
| Dialog titles dropped | ✅ Fixed: `ModalDialog` stores the title in a backing field |
| Non-blocking channel | ✅ `ScreenManager.PostMessage()` + `ToastNotification` |

### Remaining Gaps

| Gap | Detail |
|-----|--------|
| `UiSize` unused | `Dialog(UiSize)` still stores no size; `ModalDialog` uses `PanelWidth`/`PanelHeight` instead. |
| Validation messages still block | The ~60 `Util.ShowMessageBox` **validation/feedback** calls (no selection, insufficient funds, base needs a name, ...) are deliberately still modal. Only game-event notifications were migrated (see list below). |
| Context actions partial | `GoToResearch` / `GoToManufacture` / `GoToBase` / `GoToAircraft` / `GoToGlobe` exist; globe centring and aircraft selection are still TODO. |
| "Home" zoom-to-last-event | Not implemented (OpenXCOM-style shortcut). |
| Notification sound | No dedicated alert SFX yet; no sound is played on notifications. |

### Remaining message modals to migrate (tracked, not yet done)

These GeoEvents/messages still use blocking `Util.ShowMessageBox` and are candidates
for conversion to `MessageLog.PostNotification`:

| Source | Suggested type |
|--------|----------------|
| `MessageBoxGeoEvent` | dynamic (Warning/Info) |
| `GameOverGeoEvent` | Required (keep modal until a dedicated screen exists) |
| `TrackingLostGeoEvent` | Required |
| `StartBattlescapeGeoEvent` | Required (confirmation — likely stays modal) |
| `Bank` insufficient funds, save/load errors, screen validation | Error (stay modal) |

`NotificationMapping` already contains placeholder specs for the un-wired events —
flip `Wired = true` and route the call site through `MessageLog.PostNotification`.

---

## Planned Architecture

### New Class Hierarchy

```
Frame
  └── Dialog
        └── ModalDialog     ← NEW base (replaces GumDialog)
              ├── GumMessageBoxDialog    (refactored)
              ├── GumYesNoDialog         (refactored)
              ├── GumOptionsDialog       (refactored)
              ├── SoundOptionsDialog     (refactored)
              ├── ... (8 more dialogs)   (refactored)
              └── PendingActionsDialog   ← NEW (email inbox viewer)
```

### Visual Layout of ModalDialog

```
┌──────────────────────────────────────────────────────────┐
│ ████████████████████████████████████████████████████████ │  ← OverlayContainer
│ ███████████ semi-transparent dark overlay ██████████████ │     (full screen, ~50% black)
│ ████████████████████████████████████████████████████████ │
│                                                          │
│          ┌──────────────────────────────────┐            │
│          │  Title Text                  [X] │  ← TitleBar (28px, dark bg)
│          ├──────────────────────────────────┤            │
│          │                                  │            │
│          │  (subclass content goes here)    │  ← ContentArea Container
│          │                                  │            │
│          │                                  │            │
│          └──────────────────────────────────┘            │
│                                                          │
└──────────────────────────────────────────────────────────┘
       DialogPanel is centered on screen, sized from UiSize
```

### Message Flow

```
GeoEvents / Game Logic
       │
       ├── Required action needed?
       │        │
       │        ▼
       │   ScreenManager.ShowCriticalDialog(modalDialog)
       │        │
       │        ▼
       │   [blocks game, user MUST act or dismiss]
       │   [dismiss → stays in PendingRequired list]
       │   [action → removed from list]
       │
       ├── Validation / blocking confirmation?
       │        │
       │        ▼
       │   Util.ShowMessageBox(text)  [blocks game, user clicks OK]
       │
       └── Informational only?
                │
                ▼
           MessageLog.Post(text, type)
                │
                ├──→ GeoscapeScreen message log panel (persistent)
                ├──→ ToastNotification (auto-fading popup, ~4s)
                └──→ GameState save file (persists across sessions)
```

### Message Type Differentiation

| Type | Icon | Color | Toast Color | Behavior |
|------|------|-------|-------------|----------|
| `Info` | (none) | White/gray | Dark blue | Purely informational |
| `Warning` | ⚠ | Yellow | Orange | Needs attention but not critical |
| `Error` | ✕ | Red | Red | Something went wrong |
| `Required` | ⚠/● | Cyan highlight | N/A | Must be actioned. Can be dismissed, stays pending. |

### MessageLog Static Queue

```
MessageLog (static class, in Source/Utils/)
  _entries: List<MessageEntry>              ← persisted in GameState
  Post(text, type)                          ← fires MessagePosted event
  MaxEntries (default 200, settable)        ← trim limit
  PendingRequired                           ← Required entries not yet actioned
  RequiredCount                             ← for envelope badge
  Dismiss(id)                               ← marks Required as dismissed (stays pending)
  Action(id)                                ← marks Required as actioned (removes from pending)
  MessagePosted event                       ← subscribers: Toast, Geoscape log panel
```

### GeoscapeScreen Message Log Panel

Position: X=20, Y=160, Width=400, Height=350 (bottom-left)
- Semi-transparent dark background (~80% opacity)
- Envelope icon in upper-left with pending count badge
- Scrollable list (ListBox or ScrollViewer) showing messages newest-at-bottom
- Color-coded by message type
- Auto-scrolls to latest on new message
- Click envelope → opens `PendingActionsDialog`

### PendingActionsDialog ("Email Inbox")

Opened by clicking the envelope icon on the geoscape message log.
- Lists all `PendingRequired` entries with timestamp, message text, and action button(s)
- Each entry has a "Dismiss" button (stays in pending list, but hidden from view until envelope clicked again)
- Each entry has its context-specific action button (e.g., "Go To Aircraft", "View Research", "Target On Globe")
- "Dismiss All" button at the bottom
- If all entries are dismissed or actioned, the envelope icon disappears

### Toast Notifications

Component: `ToastNotification : DrawableGameComponent`
- Subscribes to `MessageLog.MessagePosted`
- Renders at top-center of screen as a small colored bar
- Auto-fades after ~4 seconds
- Multiple toasts stack vertically
- Color-coded by message type

### Notification Mapping

Currently hardcoded mapping (future: user-configurable settings screen):

| Event | Type | Method |
|-------|------|--------|
| FuelLowGeoEvent | Warning | PostMessage (non-blocking) |
| ResearchFinishedGeoEvent | Required | ShowCriticalDialog |
| FacilityFinishedGeoEvent | Info | PostMessage (non-blocking) |
| UfoAttackingOutpost | Warning | PostMessage (non-blocking) |
| GameOverGeoEvent | Required | ShowCriticalDialog |
| TrackingLostGeoEvent | Required | ShowCriticalDialog |
| StartBattlescapeGeoEvent | Required | ShowCriticalDialog |
| MessageBoxGeoEvent | depends on content | Dynamic |
| DuplicateFilename | (blocking) | ShowMessageBox |
| InsufficientFunds | (blocking) | ShowMessageBox |
| BaseNeedsName | (blocking) | ShowMessageBox |
| Game Saved/Loaded | Info | PostMessage (non-blocking) |

### Save/Load Integration

MessageLog entries are stored as a `List<MessageEntry>` field on `GameState`:
```csharp
// GameState.cs
public List<MessageEntry> MessageLogEntries { get; set; } = new();
```

The `ModelJsonConverter` auto-discovers `MessageEntry` since it's in the `ProjectXenocide.Utils` namespace. No converter changes needed.

On game load, `MessageLog.Load()` restores entries from `Xenocide.GameState.MessageLogEntries`. Entries older than `MaxEntries` are trimmed. `Required` entries that have been actioned are preserved (so the player can see their action history). Entries that were dismissed but not actioned remain in `PendingRequired`.

### Files Changed

| File | Action | Purpose |
|------|--------|---------|
| `Source/Utils/MessageLog.cs` | Create | Static message queue, event, save/load |
| `Source/UI/Dialogs/ModalDialog.cs` | Create | Centered overlay dialog with title bar + close button |
| `Source/UI/Controls/ToastNotification.cs` | Create | DrawableGameComponent for fade-away notifications |
| `Source/UI/Screens/GeoscapeScreen.cs` | Modify | Add message log panel, envelope icon, toast subscription |
| `Source/UI/Dialogs/GumMessageBoxDialog.cs` | Modify | Extend ModalDialog instead of GumDialog |
| `Source/UI/Dialogs/GumYesNoDialog.cs` | Modify | Extend ModalDialog |
| `Source/UI/Dialogs/GumOptionsDialog.cs` | Modify | Extend ModalDialog |
| `Source/UI/Dialogs/GumDialog.cs` | Delete | Replaced by ModalDialog |
| `Source/UI/Dialogs/*` (9 files) | Modify | Extend ModalDialog |
| `Source/UI/Screens/ScreenManager.cs` | Modify | Add PostMessage, ShowCriticalDialog |
| `Source/Model/GameState.cs` | Modify | Add MessageLogEntries field |
| `Source/Assets/NotificationMapping.cs` | Create | Hardcoded event→type mapping |
