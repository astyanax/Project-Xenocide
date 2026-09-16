using System;
using System.Collections.Generic;
using System.Linq;

using ProjectXenocide.Model;

namespace ProjectXenocide.Utils
{
    /// <summary>
    /// User preferences for the notification system: global toggles plus a set of
    /// individually disabled events.  Persisted through <see cref="GameOptions"/>.
    /// </summary>
    public static class NotificationSettings
    {
        /// <summary>Master switch for on-screen toast notifications.</summary>
        public static bool ToastsEnabled { get; set; } = true;

        /// <summary>Pause geoscape time when an alert (Warning/Required) arrives.</summary>
        public static bool PauseOnAlerts { get; set; } = true;

        private static readonly HashSet<string> _disabled =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Event identifiers the player has switched off.</summary>
        public static IReadOnlyCollection<string> DisabledEvents => _disabled;

        /// <summary>True if the given event is allowed to notify.</summary>
        public static bool IsEventEnabled(string eventId)
        {
            return string.IsNullOrEmpty(eventId) || !_disabled.Contains(eventId);
        }

        /// <summary>Enable or disable an individual event.</summary>
        public static void SetEventEnabled(string eventId, bool enabled)
        {
            if (string.IsNullOrEmpty(eventId))
                return;

            if (enabled)
                _disabled.Remove(eventId);
            else
                _disabled.Add(eventId);
        }

        /// <summary>Apply a full set of preferences at once (from the Settings screen).</summary>
        public static void Apply(bool toastsEnabled, bool pauseOnAlerts, IEnumerable<string> disabledEvents)
        {
            ToastsEnabled = toastsEnabled;
            PauseOnAlerts = pauseOnAlerts;
            _disabled.Clear();
            if (disabledEvents != null)
            {
                foreach (var id in disabledEvents)
                    _disabled.Add(id);
            }
        }

        /// <summary>Restore defaults.</summary>
        public static void Reset()
        {
            ToastsEnabled = true;
            PauseOnAlerts = true;
            _disabled.Clear();
        }

        /// <summary>Copy settings out of the persisted options.</summary>
        public static void Load(GameOptions options)
        {
            Reset();
            if (options == null)
                return;

            ToastsEnabled = options.ToastNotifications;
            PauseOnAlerts = options.PauseOnAlerts;
            foreach (var id in options.DisabledNotifications)
            {
                _disabled.Add(id);
            }
        }

        /// <summary>Copy settings into the persisted options.</summary>
        public static void Save(GameOptions options)
        {
            if (options == null)
                return;

            options.ToastNotifications = ToastsEnabled;
            options.PauseOnAlerts = PauseOnAlerts;
            options.DisabledNotifications = _disabled.ToList();
        }
    }
}
