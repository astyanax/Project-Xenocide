using System;
using System.Collections.Generic;
using System.Linq;

using ProjectXenocide.Assets;

namespace ProjectXenocide.Utils
{
    /// <summary>
    /// Type of a logged message, determining its visual treatment and whether
    /// it demands a user action.
    /// </summary>
    public enum MessageType
    {
        Info,
        Warning,
        Error,
        Required,
    }

    /// <summary>
    /// A single entry in the persistent message log.  Stored in GameState
    /// so it survives save/load and persists across screen transitions.
    /// </summary>
    public class MessageEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Identifier of the event that produced this entry (e.g. "FuelLowGeoEvent").
        /// Used for per-event settings, de-duplication and context actions.
        /// </summary>
        public string EventId { get; set; } = string.Empty;

        public string Text { get; set; }
        public MessageType Type { get; set; }
        public int GameDay { get; set; }
        public int GameHour { get; set; }
        public int GameMinute { get; set; }

        /// <summary>True once the player has resolved a Required entry.</summary>
        public bool IsActioned { get; set; }

        /// <summary>True once the player has acknowledged/hidden a Required entry.</summary>
        public bool IsDismissed { get; set; }

        /// <summary>Number of identical events grouped into this entry (de-duplication).</summary>
        public int RepeatCount { get; set; } = 1;

        /// <summary>Identifier of the context action offered for this entry (may be empty).</summary>
        public string ActionId { get; set; } = string.Empty;

        /// <summary>Argument for <see cref="ActionId"/> (craft/base name, coordinates, ...).</summary>
        public string TargetId { get; set; } = string.Empty;

        public string TimeString =>
            $"{GameDay:000}-{GameHour:D2}:{GameMinute:D2}";

        /// <summary>Text as shown in the log, including any de-duplication count.</summary>
        public string DisplayText =>
            RepeatCount > 1 ? $"{Text} (x{RepeatCount})" : Text;

        /// <summary>A Required entry that has not yet been resolved.</summary>
        public bool IsPendingRequired =>
            Type == MessageType.Required && !IsActioned;

        /// <summary>A Required entry the player still needs to be told about.</summary>
        public bool IsUnseenRequired =>
            IsPendingRequired && !IsDismissed;

        /// <summary>Minutes since the start of the game year, for time-based tests.</summary>
        public int MinuteOfYear => ((GameDay - 1) * 24 * 60) + (GameHour * 60) + GameMinute;
    }

    /// <summary>
    /// Central message queue.  GeoEvents and game objects post messages here.
    /// Subscribers (ToastNotification, GeoscapeScreen log panel) listen to
    /// the MessagePosted event; <see cref="Changed"/> fires for any mutation.
    /// Entries are persisted in GameState.MessageLogEntries.
    /// </summary>
    public static class MessageLog
    {
        public const int DefaultMaxEntries = 200;

        private static readonly List<MessageEntry> _entries = new();
        private static int _maxEntries = DefaultMaxEntries;

        /// <summary>Raised when a new entry is added (drives toasts and the log panel).</summary>
        public static event Action<MessageEntry> MessagePosted;

        /// <summary>Raised when entries are added, dismissed, actioned or grouped.</summary>
        public static event Action Changed;

        public static int MaxEntries
        {
            get => _maxEntries;
            set { _maxEntries = Math.Max(10, value); TrimExcess(); }
        }

        public static IReadOnlyList<MessageEntry> Entries => _entries.AsReadOnly();

        /// <summary>All Required entries that the player has not yet resolved.</summary>
        public static IEnumerable<MessageEntry> PendingRequired =>
            _entries.Where(e => e.IsPendingRequired);

        /// <summary>Required entries still demanding the player's attention (badge count).</summary>
        public static int RequiredCount => _entries.Count(e => e.IsUnseenRequired);

        /// <summary>True if any Required entry is still demanding attention.</summary>
        public static bool HasPending => RequiredCount > 0;

        /// <summary>
        /// Post a message with an explicit type (no spec lookup, no pause/dedup).
        /// Prefer <see cref="PostNotification"/> for game events.
        /// </summary>
        public static MessageEntry Post(string text, MessageType type = MessageType.Info)
        {
            return AddEntry(string.Empty, text, type, string.Empty, string.Empty);
        }

        /// <summary>
        /// Post a game event.  The event's <see cref="NotificationSpec"/> decides the
        /// message type, whether time pauses, whether repeats are grouped and what
        /// context action (if any) is offered.
        /// </summary>
        /// <returns>The new or merged entry, or null if the event is disabled.</returns>
        public static MessageEntry PostNotification(string eventId, string text,
            string targetId = "", MessageType? typeOverride = null)
        {
            if (!NotificationSettings.IsEventEnabled(eventId))
            {
                return null;
            }

            NotificationSpec spec = NotificationMapping.Get(eventId);
            MessageType type = typeOverride ?? spec.Type;

            if (spec.Dedup && TryMergeDuplicate(eventId, spec.DedupMinutes))
            {
                return null;
            }

            MessageEntry entry = AddEntry(eventId, text, type, spec.ActionId, targetId);

            if (type == MessageType.Warning || type == MessageType.Required)
            {
                PlayAlertSound(type);
            }

            if (spec.Pause && NotificationSettings.PauseOnAlerts)
            {
                PauseTime();
            }

            return entry;
        }

        /// <summary>
        /// Play a short alert tone for a Warning/Required notification (OpenXCOM
        /// plays a sound when such events occur).  No-op in headless tests.
        /// </summary>
        private static void PlayAlertSound(MessageType type)
        {
            // Instance is null in headless tests; AudioSystem is only reachable
            // once the game is running.
            if (Xenocide.Instance == null)
            {
                return;
            }

            var audio = Xenocide.AudioSystem;
            if (audio == null)
            {
                return;
            }

            audio.PlaySound(type == MessageType.Required ? SoundId.Error : SoundId.PlanetViewClickObject);
        }

        /// <summary>
        /// Mark a Required entry as permanently resolved (user took the action).
        /// </summary>
        public static void Action(string id)
        {
            var entry = _entries.FirstOrDefault(e => e.Id == id);
            if (entry != null)
            {
                entry.IsActioned = true;
                Changed?.Invoke();
            }
        }

        /// <summary>
        /// Mark a Required entry as temporarily dismissed (stays in PendingRequired
        /// but no longer counts towards the envelope badge).
        /// </summary>
        public static void Dismiss(string id)
        {
            var entry = _entries.FirstOrDefault(e => e.Id == id);
            if (entry != null)
            {
                entry.IsDismissed = true;
                Changed?.Invoke();
            }
        }

        /// <summary>
        /// Dismiss all pending Required entries at once.
        /// </summary>
        public static void DismissAll()
        {
            bool any = false;
            foreach (var entry in _entries.Where(e => e.IsUnseenRequired))
            {
                entry.IsDismissed = true;
                any = true;
            }
            if (any)
            {
                Changed?.Invoke();
            }
        }

        /// <summary>Clear the log (used when starting a new game).</summary>
        public static void Reset()
        {
            _entries.Clear();
            Changed?.Invoke();
        }

        /// <summary>
        /// Save current entries to the active GameState so they survive save/load.
        /// </summary>
        public static void SaveToGameState()
        {
            var state = Xenocide.GameState;
            if (state != null)
                state.MessageLogEntries = new List<MessageEntry>(_entries);
        }

        /// <summary>
        /// Restore entries from a previously saved GameState.  Clears the log when
        /// the state has no entries (e.g. a brand new game).
        /// </summary>
        public static void LoadFromGameState()
        {
            _entries.Clear();
            var saved = Xenocide.GameState?.MessageLogEntries;
            if (saved != null)
            {
                _entries.AddRange(saved);
                TrimExcess();
            }
            Changed?.Invoke();
        }

        private static MessageEntry AddEntry(string eventId, string text, MessageType type,
            string actionId, string targetId)
        {
            var (day, hour, minute) = CurrentGameTime();
            var entry = new MessageEntry
            {
                EventId = eventId ?? string.Empty,
                Text = text,
                Type = type,
                ActionId = actionId ?? string.Empty,
                TargetId = targetId ?? string.Empty,
                GameDay = day,
                GameHour = hour,
                GameMinute = minute,
            };

            _entries.Add(entry);
            TrimExcess();
            MessagePosted?.Invoke(entry);
            Changed?.Invoke();
            return entry;
        }

        /// <summary>
        /// If an identical event was logged recently, bump its repeat count instead
        /// of adding a duplicate row.  Returns true if an existing entry was merged.
        /// </summary>
        private static bool TryMergeDuplicate(string eventId, int windowMinutes)
        {
            var geoTime = Xenocide.GameState?.GeoData?.GeoTime;
            if (geoTime == null || windowMinutes <= 0)
            {
                return false;
            }

            int now = MinuteOfYear(geoTime.Time);
            var duplicate = _entries.LastOrDefault(e => e.EventId == eventId);

            if (duplicate != null && Math.Abs(now - duplicate.MinuteOfYear) <= windowMinutes)
            {
                ++duplicate.RepeatCount;

                // keep the entry's timestamp current so the window slides
                var (day, hour, minute) = CurrentGameTime();
                duplicate.GameDay = day;
                duplicate.GameHour = hour;
                duplicate.GameMinute = minute;

                Changed?.Invoke();
                return true;
            }
            return false;
        }

        private static void PauseTime()
        {
            var geoTime = Xenocide.GameState?.GeoData?.GeoTime;
            if (geoTime != null && geoTime.TimeRatio > 0)
            {
                geoTime.StopTime();
            }
        }

        private static (int day, int hour, int minute) CurrentGameTime()
        {
            var geoTime = Xenocide.GameState?.GeoData?.GeoTime;
            return geoTime == null
                ? (1, 0, 0)
                : (geoTime.Time.DayOfYear, geoTime.Time.Hour, geoTime.Time.Minute);
        }

        private static int MinuteOfYear(DateTime time)
        {
            return ((time.DayOfYear - 1) * 24 * 60) + (time.Hour * 60) + time.Minute;
        }

        private static void TrimExcess()
        {
            while (_entries.Count > _maxEntries)
            {
                var oldestNonRequired = _entries
                    .FirstOrDefault(e => e.Type != MessageType.Required);
                if (oldestNonRequired != null)
                    _entries.Remove(oldestNonRequired);
                else
                    _entries.RemoveAt(0);
            }
        }
    }
}
