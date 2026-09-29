using System;
using System.Collections.Generic;
using System.Reflection;

using ProjectXenocide;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.StaticData;

using Xunit;

namespace Xenocide.Test.MonoGame
{
    /// <summary>
    /// Tests for multi-waypoint patrol routes (craft visits each waypoint in turn).
    /// </summary>
    [Collection("GameStateInit")]
    public class PatrolRouteTests : IDisposable
    {
        public PatrolRouteTests()
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

        [Fact]
        public void PatrolMission_AdvancesThroughEachWaypoint()
        {
            var outpost = OutpostInventory.ConstructTestOutpost();
            var aircraft = (Aircraft)ProjectXenocide.Xenocide.StaticTables.ItemList["ITEM_XC-22_ECLIPSE"].Manufacture();
            outpost.Inventory.Add(aircraft, false);
            aircraft.Mission.Abort();

            var waypoints = new List<GeoPosition>
            {
                new GeoPosition(0.001f, 0.001f),
                new GeoPosition(0.002f, 0.002f),
            };

            aircraft.Mission = new PatrolMission(aircraft, waypoints);
            var patrol = (PatrolMission)aircraft.Mission;

            Assert.Equal(2, patrol.Waypoints.Count);
            Assert.Equal(0, patrol.CurrentWaypointIndex);

            // Give the craft enough time to reach each nearby waypoint.
            for (int i = 0; i < 5; ++i)
            {
                aircraft.Update(60000);
            }

            // It should have moved past the first waypoint and be loitering at the last.
            Assert.Equal(1, patrol.CurrentWaypointIndex);
            Assert.True(aircraft.Position.IsWithin(waypoints[1], 0.001f),
                "Craft should be at the final waypoint.");
        }
    }
}
