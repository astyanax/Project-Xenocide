using System;
using System.Linq;
using System.Reflection;

using ProjectXenocide;
using ProjectXenocide.Model;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Utils;

using Xunit;

namespace Xenocide.Test.MonoGame
{
    /// <summary>
    /// Unit tests for the notification/message-log semantics: badge counting,
    /// dismissal vs. action, de-duplication, per-event toggles and pause-on-alert.
    /// </summary>
    [Collection("GameStateInit")]
    public class NotificationLogTests : IDisposable
    {
        public NotificationLogTests()
        {
            ProjectXenocide.Xenocide.Rng.ClearLoadedValues();

            var staticTables = new StaticTables();
            typeof(ProjectXenocide.Xenocide)
                .GetField("staticTables", BindingFlags.Static | BindingFlags.NonPublic)!
                .SetValue(null, staticTables);
            staticTables.Populate();

            typeof(ProjectXenocide.Xenocide)
                .GetField("gameBalance", BindingFlags.Static | BindingFlags.NonPublic)!
                .SetValue(null, new GameBalanceClass(Difficulty.Easy));

            ProjectXenocide.Xenocide.GameState = new GameState();
            ProjectXenocide.Xenocide.GameState.SetToStartGameCondition();

            NotificationSettings.Reset();
            MessageLog.Reset();
        }

        public void Dispose()
        {
            MessageLog.Reset();

            typeof(ProjectXenocide.Xenocide)
                .GetField("staticTables", BindingFlags.Static | BindingFlags.NonPublic)!
                .SetValue(null, null);

            ProjectXenocide.Xenocide.GameState = null!;
        }

        [Fact]
        public void RequiredEntry_ActionAndDismissAreTrackedSeparately()
        {
            var entry = MessageLog.PostNotification("ResearchFinishedGeoEvent", "Laser Weapons research complete");

            Assert.NotNull(entry);
            Assert.Equal(MessageType.Required, entry!.Type);
            Assert.Equal(1, MessageLog.RequiredCount);
            Assert.True(MessageLog.HasPending);

            // Dismissing clears the badge count but leaves the entry pending.
            MessageLog.Dismiss(entry.Id);
            Assert.Equal(0, MessageLog.RequiredCount);
            Assert.Single(MessageLog.PendingRequired);

            // Actioning resolves it.
            MessageLog.Action(entry.Id);
            Assert.Empty(MessageLog.PendingRequired);
            Assert.False(MessageLog.HasPending);
        }

        [Fact]
        public void DismissAll_ClearsTheBadgeButKeepsHistory()
        {
            MessageLog.PostNotification("ResearchFinishedGeoEvent", "A");
            MessageLog.PostNotification("ResearchFinishedGeoEvent", "B");
            Assert.Equal(2, MessageLog.RequiredCount);

            MessageLog.DismissAll();

            Assert.Equal(0, MessageLog.RequiredCount);
            Assert.Equal(2, MessageLog.PendingRequired.Count());
        }

        [Fact]
        public void RepeatedEvents_AreGroupedWithinTheDedupWindow()
        {
            MessageLog.PostNotification("FuelLowGeoEvent", "Craft low on fuel");
            MessageLog.PostNotification("FuelLowGeoEvent", "Craft low on fuel");

            Assert.Single(MessageLog.Entries);
            Assert.Equal(2, MessageLog.Entries[0].RepeatCount);
            Assert.Contains("x2", MessageLog.Entries[0].DisplayText);
        }

        [Fact]
        public void DisabledEvent_IsNotLogged()
        {
            NotificationSettings.SetEventEnabled("FuelLowGeoEvent", false);

            var entry = MessageLog.PostNotification("FuelLowGeoEvent", "Craft low on fuel");

            Assert.Null(entry);
            Assert.Empty(MessageLog.Entries);
        }

        [Fact]
        public void WarningEvent_PausesTimeWhenEnabled()
        {
            ProjectXenocide.Xenocide.GameState.GeoData.GeoTime.TimeRatio = 60f;

            MessageLog.PostNotification("FuelLowGeoEvent", "Craft low on fuel");

            Assert.Equal(0f, ProjectXenocide.Xenocide.GameState.GeoData.GeoTime.TimeRatio);
        }

        [Fact]
        public void WarningEvent_DoesNotPauseWhenPauseDisabled()
        {
            NotificationSettings.PauseOnAlerts = false;
            ProjectXenocide.Xenocide.GameState.GeoData.GeoTime.TimeRatio = 60f;

            MessageLog.PostNotification("FuelLowGeoEvent", "Craft low on fuel");

            Assert.Equal(60f, ProjectXenocide.Xenocide.GameState.GeoData.GeoTime.TimeRatio);
        }

        [Fact]
        public void InfoEvent_DoesNotPauseTime()
        {
            ProjectXenocide.Xenocide.GameState.GeoData.GeoTime.TimeRatio = 60f;

            MessageLog.PostNotification("FacilityFinishedGeoEvent", "Facility built");

            Assert.Equal(60f, ProjectXenocide.Xenocide.GameState.GeoData.GeoTime.TimeRatio);
        }
    }
}
