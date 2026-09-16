using System;
using System.Collections.Generic;
using System.Linq;

using ProjectXenocide.Utils;

namespace ProjectXenocide.Assets
{
    /// <summary>
    /// Behaviour of a single notification event: how it is presented, whether it
    /// pauses the geoscape, whether repeats are grouped, and what context action
    /// (if any) it offers.
    /// </summary>
    public sealed class NotificationSpec
    {
        /// <summary>Stable identifier of the event (matches the GeoEvent type name).</summary>
        public string EventId { get; set; }

        /// <summary>Human-readable name shown in the Settings > Notifications list.</summary>
        public string DisplayName { get; set; }

        /// <summary>How the message is treated (Info/Warning/Error/Required).</summary>
        public MessageType Type { get; set; } = MessageType.Info;

        /// <summary>Show an on-screen toast when this event fires.</summary>
        public bool Toast { get; set; } = true;

        /// <summary>Pause geoscape time when this event fires.</summary>
        public bool Pause { get; set; }

        /// <summary>Group repeats of this event within <see cref="DedupMinutes"/>.</summary>
        public bool Dedup { get; set; }

        /// <summary>De-duplication window, in game minutes.</summary>
        public int DedupMinutes { get; set; }

        /// <summary>Context action offered for this event (empty for none).</summary>
        public string ActionId { get; set; } = string.Empty;

        /// <summary>
        /// True once the event is actually posted through <see cref="MessageLog.PostNotification"/>.
        /// Unwired specs are retained so later migrations can simply flip this on.
        /// </summary>
        public bool Wired { get; set; }
    }

    /// <summary>
    /// Maps game event identifiers to their notification behaviour.  This is the
    /// single source of truth used by <see cref="MessageLog.PostNotification"/>.
    /// </summary>
    /// <remarks>
    /// NOTE: Not every entry is wired yet.  Validation/feedback messages still use
    /// blocking <c>Util.ShowMessageBox</c> modals; only the game-event notifications
    /// are routed through here for now.  See docs/DIALOG.md for the migration list.
    /// </remarks>
    public static class NotificationMapping
    {
        // Context action identifiers.
        public const string ActionGoToAircraft = "GoToAircraft";
        public const string ActionGoToResearch = "GoToResearch";
        public const string ActionGoToManufacture = "GoToManufacture";
        public const string ActionGoToBase = "GoToBase";
        public const string ActionGoToGlobe = "GoToGlobe";

        private static readonly List<NotificationSpec> _specs = new()
        {
            // --- Wired events (routed through MessageLog.PostNotification) ---
            new NotificationSpec
            {
                EventId = "ResearchFinishedGeoEvent",
                DisplayName = "Research completed",
                Type = MessageType.Required,
                Toast = true,
                Pause = true,
                Dedup = false,
                ActionId = ActionGoToResearch,
                Wired = true,
            },
            new NotificationSpec
            {
                EventId = "FuelLowGeoEvent",
                DisplayName = "Craft low on fuel",
                Type = MessageType.Warning,
                Toast = true,
                Pause = true,
                Dedup = true,
                DedupMinutes = 360,
                ActionId = ActionGoToAircraft,
                Wired = true,
            },
            new NotificationSpec
            {
                EventId = "FacilityFinishedGeoEvent",
                DisplayName = "Facility construction finished",
                Type = MessageType.Info,
                Toast = true,
                Pause = false,
                Dedup = true,
                DedupMinutes = 60,
                ActionId = ActionGoToBase,
                Wired = true,
            },
            new NotificationSpec
            {
                EventId = "UfoAttackingOutpostGeoEvent",
                DisplayName = "UFO attacking a base",
                Type = MessageType.Warning,
                Toast = true,
                Pause = true,
                Dedup = false,
                ActionId = ActionGoToBase,
                Wired = true,
            },

            // --- Planned events (kept so future migrations only flip Wired = true) ---
            new NotificationSpec { EventId = "TrackingLostGeoEvent", DisplayName = "Tracking lost", Type = MessageType.Required, Pause = true },
            new NotificationSpec { EventId = "StartBattlescapeGeoEvent", DisplayName = "Battlescape ready", Type = MessageType.Required, Pause = true },
            new NotificationSpec { EventId = "GameOverGeoEvent", DisplayName = "Game over", Type = MessageType.Required, Pause = true },
            new NotificationSpec { EventId = "CraftDestroyedGeoEvent", DisplayName = "Craft destroyed", Type = MessageType.Warning, Pause = true },
            new NotificationSpec { EventId = "UfoDetectedGeoEvent", DisplayName = "UFO detected", Type = MessageType.Info, Dedup = true, DedupMinutes = 120 },
            new NotificationSpec { EventId = "ItemArrivedGeoEvent", DisplayName = "Items arrived at base", Type = MessageType.Info },
            new NotificationSpec { EventId = "TransferCompleteGeoEvent", DisplayName = "Transfer complete", Type = MessageType.Info },
            new NotificationSpec { EventId = "BaseOutOfSupplies", DisplayName = "Base out of craft supplies", Type = MessageType.Warning, Dedup = true, DedupMinutes = 360 },
        };

        private static readonly Dictionary<string, NotificationSpec> _map =
            _specs.ToDictionary(s => s.EventId, StringComparer.OrdinalIgnoreCase);

        /// <summary>All known notification specs, in display order.</summary>
        public static IReadOnlyList<NotificationSpec> All => _specs;

        /// <summary>Specs that are actually wired into the game (used by Settings).</summary>
        public static IEnumerable<NotificationSpec> Wired => _specs.Where(s => s.Wired);

        /// <summary>
        /// Get the spec for an event, or a neutral Info spec if unknown.
        /// </summary>
        public static NotificationSpec Get(string eventId)
        {
            if (!string.IsNullOrEmpty(eventId) && _map.TryGetValue(eventId, out var spec))
            {
                return spec;
            }
            return new NotificationSpec { EventId = eventId, DisplayName = eventId };
        }
    }
}
