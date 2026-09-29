using System;
using System.Linq;
using System.Reflection;

using ProjectXenocide;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.Model.StaticData;

using Xunit;

namespace Xenocide.Test.MonoGame
{
    /// <summary>
    /// Tests for the alien Overmind's debug mission creation: every mission type
    /// either creates a task (targeting/near the clicked position, or a random
    /// valid fallback) or is refused when no valid target exists.
    /// </summary>
    [Collection("GameStateInit")]
    public class OvermindTests : IDisposable
    {
        public OvermindTests()
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
        }

        public void Dispose()
        {
            typeof(ProjectXenocide.Xenocide)
                .GetField("staticTables", BindingFlags.Static | BindingFlags.NonPublic)!
                .SetValue(null, null);

            ProjectXenocide.Xenocide.GameState = null!;
        }

        private static Overmind Overmind => ProjectXenocide.Xenocide.GameState.GeoData.Overmind;

        [Theory]
        [InlineData(AlienMission.Research)]
        [InlineData(AlienMission.Harvest)]
        [InlineData(AlienMission.Abduction)]
        [InlineData(AlienMission.Terror)]
        [InlineData(AlienMission.Outpost)]
        public void DebugCreateMission_CreatesTaskForValidTypes(AlienMission type)
        {
            int before = Overmind.Tasks.Count;

            bool created = Overmind.DebugCreateMission(type, new GeoPosition());

            Assert.True(created, $"{type} should create a task (a valid target exists).");
            Assert.Equal(before + 1, Overmind.Tasks.Count);
        }

        [Fact]
        public void DebugCreateMission_Terror_TargetsACity()
        {
            Assert.NotEmpty(ProjectXenocide.Xenocide.GameState.GeoData.Planet.AllCities);

            bool created = Overmind.DebugCreateMission(AlienMission.Terror, new GeoPosition());

            Assert.True(created);
            Assert.Contains(Overmind.Tasks, task => task is TerrorTask);
        }

        [Fact]
        public void DebugCreateMission_Supply_RefusedWithNoAlienOutposts()
        {
            // A fresh game has no alien outposts, so there is nothing to resupply.
            Assert.DoesNotContain(Overmind.Sites, site => site is OutpostAlienSite);

            bool created = Overmind.DebugCreateMission(AlienMission.Supply, new GeoPosition());

            Assert.False(created);
        }
    }
}
