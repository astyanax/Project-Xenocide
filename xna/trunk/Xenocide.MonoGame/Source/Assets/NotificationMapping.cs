using System;
using System.Collections.Generic;

using ProjectXenocide.Utils;

namespace ProjectXenocide.Assets
{
    /// <summary>
    /// Maps game event types to their notification behavior.
    /// Currently hardcoded; future settings screen will make this user-configurable.
    /// TODO: Add a "Notifications" section in the settings screen.
    /// </summary>
    public static class NotificationMapping
    {
        private static readonly Dictionary<string, MessageType> _map = new(StringComparer.OrdinalIgnoreCase)
        {
            // GeoEvents — non-blocking
            ["FuelLowGeoEvent"]              = MessageType.Warning,
            ["FacilityFinishedGeoEvent"]     = MessageType.Info,
            ["UfoAttackingOutpostGeoEvent"]  = MessageType.Warning,
            ["UfoVaporized"]                 = MessageType.Info,
            ["UfoCrashed"]                   = MessageType.Info,
            ["ItemArrived"]                  = MessageType.Info,
            ["TransferComplete"]             = MessageType.Info,

            // GeoEvents — require user action
            ["ResearchFinishedGeoEvent"]     = MessageType.Required,
            ["GameOverGeoEvent"]             = MessageType.Required,
            ["TrackingLostGeoEvent"]         = MessageType.Required,
            ["StartBattlescapeGeoEvent"]     = MessageType.Required,

            // Validation / user feedback — blocking (mapped for future configurability)
            ["InsufficientFunds"]            = MessageType.Error,
            ["DuplicateFilename"]            = MessageType.Error,
            ["BaseNeedsName"]                = MessageType.Error,
            ["NoProjectSelected"]            = MessageType.Error,
            ["NoSaleSelected"]               = MessageType.Error,
            ["CantFitItems"]                 = MessageType.Error,

            // Success confirmations
            ["GameSaved"]                    = MessageType.Info,
            ["GameLoaded"]                   = MessageType.Info,
            ["BaseNameChanged"]              = MessageType.Info,
        };

        public static MessageType GetType(string eventName, MessageType fallback = MessageType.Info)
        {
            return _map.TryGetValue(eventName, out var type) ? type : fallback;
        }
    }
}
