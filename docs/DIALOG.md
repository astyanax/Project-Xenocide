# Dialog & Message System

## Implementation Status (May 2026)

| Phase | Status |
|-------|--------|
| `MessageLog` static queue + GameState persistence | ✅ Implemented |
| `ModalDialog` base class (overlay + title bar + close X + centering) | ✅ Implemented |
| All 13 dialogs refactored to extend `ModalDialog` | ✅ Done |
| `GumDialog` deleted (fully replaced) | ✅ Done |
| `ToastNotification` component (auto-fading popups) | ✅ Implemented |
| `NotificationMapping` event→type map | ✅ Implemented |
| `ScreenManager.PostMessage()` | ✅ Implemented |
| GeoscapeScreen message log panel (ListBox) | ✅ Implemented |
| Envelope icon + `PendingActionsDialog` (email inbox) | Pending |
| GeoEvent integration (non-blocking PostMessage) | Pending |
| Settings screen "Notifications" section | TODO |

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
  │     └── GumDialog  (UI/Dialogs/GumDialog.cs)
  │           ├── GumMessageBoxDialog    (simple OK message)
  │           ├── GumYesNoDialog         (Yes/No or OK/Cancel)
  │           ├── GumOptionsDialog       (Load/Save/Sound/Abandon menu)
  │           ├── SoundOptionsDialog     (music/sound volume controls)
  │           ├── LaunchInterceptDialog  (select craft to intercept)
  │           ├── NameNewBaseDialog      (name input for new base)
  │           ├── AlienMissionDialog     (debug cheat: select alien mission)
  │           ├── StartBattlescapeDialog (confirm/auto-complete battlescape)
  │           ├── TrackingLostDialog     (what to do when tracking lost)
  │           ├── UfoInfoDialog          (UFO detail info display)
  │           ├── AircraftOrdersDialog   (give orders to aircraft)
  │           ├── BuildFacilityDialog    (select facility to build)
  │           └── PickActionDialog       (combat actions in battlescape)
  └── Screen  (UI/Screens/Screen.cs) — "full screens with background"
```

### Rendered Appearance

All 13 dialogs share the same rendering approach:
- **No window chrome** — no title bar, no close "X" button, no border, no background dim/overlay.
- **Default Gum StackPanel** — all content is added via `RootContainer.AddChild()`, which uses a plain `StackPanel` that auto-sizes.
- **No centering** — the `UiSize(0.5f, 0.3f)` passed to the constructor is never applied. The StackPanel renders at its parent's default position (top-left corner of the Gum root).
- **Same typeface** — Buttons use the default Gum Button font. No differentiated styling for title vs body text.

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

- **`ScreenManager.HandleEscapeKey()`** detects the ESC edge. If dialogs are showing, it unconditionally calls `CloseDialog(showingDialogs.Peek())` — dismissing the topmost dialog **without invoking any callbacks** (OkAction, YesAction, NoAction are skipped).
- **No dialog subclass overrides `HandleEscape()`** — none have custom ESC behavior.
- **For `GumYesNoDialog`**, ESC acts like "Cancel" (closes without side effects).
- **For `GumMessageBoxDialog`**, the `OkAction` callback is silently discarded on ESC.

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

### Known Gaps (Before MessageLog System)

| Gap | Detail |
|-----|--------|
| No message log | All messages are ephemeral popups. Zero historical persistence. |
| No dialog chrome | No title bar, close X button, background dim, or centering. |
| UiSize ignored | `new UiSize(0.5f, 0.3f)` passed to constructors but never applied to the StackPanel. |
| ESC discards callbacks | Closing a dialog via ESC skips any registered actions. |
| Game pauses on ALL dialogs | Even informational messages ("Fuel low") block gameplay. |
| No non-blocking channel | Every message forces a user interaction. |
| No dialog visual differentiation | All dialogs look the same — stacked labels and buttons on a plain StackPanel. |
| Geoscape bottom-left unused | ~930×540 pixels available below the time/funds labels and left of the 3D globe. |

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
