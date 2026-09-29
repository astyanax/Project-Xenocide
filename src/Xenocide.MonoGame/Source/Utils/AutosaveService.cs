using System;

using NLog;

using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Services;

namespace ProjectXenocide.Utils
{
    /// <summary>
    /// OpenXcom-style autosave: periodically writes a rotating autosave slot
    /// while the geoscape clock runs.  Defaults: enabled, every 10 geoscape
    /// days, 5 slots.
    /// </summary>
    public static class AutosaveService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>Whether autosave is active (persisted).</summary>
        public static bool Enabled { get; set; } = true;

        /// <summary>Geoscape days between autosaves (persisted).</summary>
        public static int IntervalDays { get; set; } = 10;

        /// <summary>Number of rotating autosave slots (persisted).</summary>
        public static int Slots { get; set; } = 5;

        private static bool initialized;
        private static int lastAutosaveDay;
        private static int nextSlot = 1;

        /// <summary>Re-arm the timer (start a new game / load a game).</summary>
        public static void Reset()
        {
            initialized = false;
            nextSlot = 1;
        }

        /// <summary>Called from the geoscape time loop each step.</summary>
        public static void Update()
        {
            if (!Enabled || Slots <= 0)
            {
                return;
            }

            GeoTime geoTime = Xenocide.GameState?.GeoData?.GeoTime;
            if (geoTime == null)
            {
                return;
            }

            int today = geoTime.DayNumber();
            if (!initialized)
            {
                initialized = true;
                lastAutosaveDay = today;
                return;
            }

            if (today - lastAutosaveDay < IntervalDays)
            {
                return;
            }

            lastAutosaveDay = today;
            string name = SavegameService.AutosaveName(nextSlot);
            nextSlot = (nextSlot % Slots) + 1;

            if (SavegameService.Save(Xenocide.GameState, name))
            {
                Logger.Info("Autosave written: {0}", name);
            }
        }

        /// <summary>Load persisted preferences.</summary>
        public static void Load(GameOptions options)
        {
            if (options == null)
            {
                return;
            }
            Enabled = options.AutosaveEnabled;
            IntervalDays = Math.Max(1, options.AutosaveIntervalDays);
            Slots = Math.Max(1, options.AutosaveSlots);
        }

        /// <summary>Write preferences into the options to be persisted.</summary>
        public static void Save(GameOptions options)
        {
            if (options == null)
            {
                return;
            }
            options.AutosaveEnabled = Enabled;
            options.AutosaveIntervalDays = IntervalDays;
            options.AutosaveSlots = Slots;
        }
    }
}
