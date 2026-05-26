using System;
using System.Collections.Generic;
using System.Linq;

using ProjectXenocide.Model;

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
        public string Text { get; set; }
        public MessageType Type { get; set; }
        public int GameDay { get; set; }
        public int GameHour { get; set; }
        public int GameMinute { get; set; }
        public bool IsActioned { get; set; }
        public bool IsDismissed { get; set; }

        public string TimeString =>
            $"{GameDay:000}-{GameHour:D2}:{GameMinute:D2}";

        public bool IsPendingRequired =>
            Type == MessageType.Required && !IsActioned;
    }

    /// <summary>
    /// Central message queue.  GeoEvents and game objects post messages here.
    /// Subscribers (ToastNotification, GeoscapeScreen log panel) listen to
    /// the MessagePosted event.  Entries are persisted in GameState.MessageLogEntries.
    /// </summary>
    public static class MessageLog
    {
        public const int DefaultMaxEntries = 200;

        private static readonly List<MessageEntry> _entries = new();
        private static int _maxEntries = DefaultMaxEntries;

        public static event Action<MessageEntry> MessagePosted;

        public static int MaxEntries
        {
            get => _maxEntries;
            set { _maxEntries = Math.Max(10, value); TrimExcess(); }
        }

        public static IReadOnlyList<MessageEntry> Entries => _entries.AsReadOnly();

        public static IEnumerable<MessageEntry> PendingRequired =>
            _entries.Where(e => e.IsPendingRequired);

        public static int RequiredCount => _entries.Count(e => e.IsPendingRequired);

        public static void Post(string text, MessageType type = MessageType.Info)
        {
            var geoTime = Xenocide.GameState?.GeoData?.GeoTime;
            var entry = new MessageEntry
            {
                Text = text,
                Type = type,
                GameDay = geoTime != null ? geoTime.Time.DayOfYear : 1,
                GameHour = geoTime != null ? geoTime.Time.Hour : 0,
                GameMinute = geoTime != null ? geoTime.Time.Minute : 0,
            };

            _entries.Add(entry);
            TrimExcess();
            MessagePosted?.Invoke(entry);
        }

        /// <summary>
        /// Mark a Required entry as permanently resolved (user took the action).
        /// </summary>
        public static void Action(string id)
        {
            var entry = _entries.FirstOrDefault(e => e.Id == id);
            if (entry != null)
                entry.IsActioned = true;
        }

        /// <summary>
        /// Mark a Required entry as temporarily dismissed (stays in PendingRequired
        /// but won't be shown again until the envelope icon is clicked).
        /// </summary>
        public static void Dismiss(string id)
        {
            var entry = _entries.FirstOrDefault(e => e.Id == id);
            if (entry != null)
                entry.IsDismissed = true;
        }

        /// <summary>
        /// Dismiss all pending Required entries at once.
        /// </summary>
        public static void DismissAll()
        {
            foreach (var entry in _entries.Where(e => e.IsPendingRequired))
                entry.IsDismissed = true;
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
        /// Restore entries from a previously saved GameState.
        /// </summary>
        public static void LoadFromGameState()
        {
            var state = Xenocide.GameState;
            if (state?.MessageLogEntries != null)
            {
                _entries.Clear();
                _entries.AddRange(state.MessageLogEntries);
                TrimExcess();
            }
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
