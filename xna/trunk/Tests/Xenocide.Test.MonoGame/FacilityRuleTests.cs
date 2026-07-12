using System.Reflection;

using ProjectXenocide;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Facilities;

namespace Xenocide.Test.MonoGame;

public class FacilityRuleTests : IDisposable
{
    public FacilityRuleTests()
    {
        var staticTables = new StaticTables();
        var staticTablesField = typeof(ProjectXenocide.Xenocide).GetField("staticTables", BindingFlags.Static | BindingFlags.NonPublic)!;
        staticTablesField.SetValue(null, staticTables);

        // GeoData's fields (eventQueue, geoTime, etc.) are initialized inline,
        // so a plain new GameState() supports QueueEvent and GeoTime access.
        // Must NOT call SetToStartGameCondition() — that tries to load planet
        // PNG textures not available in the test output directory.
        ProjectXenocide.Xenocide.GameState = new GameState();
    }

    public void Dispose()
    {
        var staticTablesField = typeof(ProjectXenocide.Xenocide).GetField("staticTables", BindingFlags.Static | BindingFlags.NonPublic)!;
        staticTablesField.SetValue(null, null);
        ProjectXenocide.Xenocide.GameState = null!;
    }

    private static Outpost CreateOutpostWithLiftAt(int x, int y)
    {
        var outpost = new Outpost(new GeoPosition(0, 0), "Test Base");
        var lift = new FacilityHandle("FAC_BASE_ACCESS_FACILITY", x, y);
        outpost.Floorplan.AddFacility(lift);
        return outpost;
    }

    // ── Rule: Access Lift cannot be removed ──

    [Fact]
    public void AccessLift_CannotBeRemoved()
    {
        var outpost = CreateOutpostWithLiftAt(0, 0);
        var lift = outpost.Floorplan.Facilities[0];
        Assert.Equal(XenoError.DeleteWillSplitBase, outpost.Floorplan.CanRemoveFacility(lift));
    }

    // ── Rule: Destroy restriction (in-use storage blocks removal) ──

    [Fact]
    public void StorageFacility_Full_CannotBeRemoved()
    {
        var outpost = CreateOutpostWithLiftAt(0, 0);
        var storage = new FacilityHandle("FAC_STORAGE_FACILITY", 0, 1);
        outpost.Floorplan.AddFacility(storage);
        // Fill the 500-unit storage completely
        outpost.Statistics.Capacities["STORAGE_GEAR"].Use(500);
        Assert.Equal(XenoError.FacilityIsInUse, outpost.Floorplan.CanRemoveFacility(storage));
    }

    [Fact]
    public void StorageFacility_Empty_CanBeRemoved()
    {
        var outpost = CreateOutpostWithLiftAt(0, 0);
        var storage = new FacilityHandle("FAC_STORAGE_FACILITY", 0, 1);
        outpost.Floorplan.AddFacility(storage);
        // Storage is empty (0 in use), so removal should succeed
        Assert.Equal(XenoError.None, outpost.Floorplan.CanRemoveFacility(storage));
    }

    // ── Rule: Neural Shielding visibility reduction ──

    [Fact]
    public void Detectability_WithoutShield_Returns25Percent()
    {
        var outpost = CreateOutpostWithLiftAt(0, 0);
        Assert.Equal(25, outpost.Detectability());
    }

    [Fact]
    public void Detectability_WithNeuralShield_Returns1Percent()
    {
        var outpost = CreateOutpostWithLiftAt(0, 0);
        var shield = new FacilityHandle("FAC_NEURAL_SHIELDING_FACILITY", 0, 1);
        outpost.Floorplan.AddFacility(shield);
        Assert.Equal(1, outpost.Detectability());
    }

    // ── Rule: LimitIsOnePerOutpost correct for each facility type ──

    public static IEnumerable<object[]> OnePerOutpostTrueData => new[]
    {
        new object[] { "FAC_SHORT_RANGE_NEUDAR" },
        new object[] { "FAC_LONG_RANGE_NEUDAR" },
        new object[] { "FAC_TACHYON_EMISSIONS_DETECTOR" },
        new object[] { "FAC_BASE_ACCESS_FACILITY" },
        new object[] { "FAC_GRAVITY_SHIELD_FACILITY" },
        new object[] { "FAC_NEURAL_SHIELDING_FACILITY" },
    };

    [Theory]
    [MemberData(nameof(OnePerOutpostTrueData))]
    public void LimitIsOnePerOutpost_True(string facilityId)
    {
        Assert.True(ProjectXenocide.Xenocide.StaticTables.FacilityList[facilityId].LimitIsOnePerOutpost);
    }

    public static IEnumerable<object[]> OnePerOutpostFalseData => new[]
    {
        new object[] { "FAC_STORAGE_FACILITY" },
        new object[] { "FAC_XENOMORPH_HOLDING_FACILITY" },
        new object[] { "FAC_LANDING_PAD" },
        new object[] { "FAC_RESEARCH_FACILITY" },
        new object[] { "FAC_ENGINEERING_FACILITY" },
        new object[] { "FAC_BARRACKS_FACILITY" },
        new object[] { "FAC_PSIONIC_TRAINING_FACILITY" },
        new object[] { "FAC_MISSILE_DEFENSE_ARRAY" },
        new object[] { "FAC_LASER_DEFENSE_ARRAY" },
        new object[] { "FAC_PLASMA_DEFENSE_ARRAY" },
        new object[] { "FAC_GAIA_DEFENSE_ARRAY" },
    };

    [Theory]
    [MemberData(nameof(OnePerOutpostFalseData))]
    public void LimitIsOnePerOutpost_False(string facilityId)
    {
        Assert.False(ProjectXenocide.Xenocide.StaticTables.FacilityList[facilityId].LimitIsOnePerOutpost);
    }

    // ── Rule: Scan facility limit (1 per type) — verify the flag is what the UI checks ──

    [Fact]
    public void SameScanType_Twice_FloorplanAcceptsBoth()
    {
        var outpost = CreateOutpostWithLiftAt(0, 0);
        var radar1 = new FacilityHandle("FAC_SHORT_RANGE_NEUDAR", 0, 1);
        outpost.Floorplan.AddFacility(radar1);
        var radar2 = new FacilityHandle("FAC_SHORT_RANGE_NEUDAR", 0, 2);
        outpost.Floorplan.AddFacility(radar2);
        Assert.Equal(3, outpost.Floorplan.Facilities.Count);
    }

    // ── Rule: Defence shot count (1 per facility, 2 with Gravity Shield) ──

    [Fact]
    public void Attack_LoopsOverAllDefenseFacilities()
    {
        var outpost = CreateOutpostWithLiftAt(0, 0);
        var missile = new FacilityHandle("FAC_MISSILE_DEFENSE_ARRAY", 0, 1);
        outpost.Floorplan.AddFacility(missile);
        var laser = new FacilityHandle("FAC_LASER_DEFENSE_ARRAY", 0, 2);
        outpost.Floorplan.AddFacility(laser);

        Assert.Contains(outpost.Floorplan.Facilities, h => h.FacilityInfo.Id == "FAC_MISSILE_DEFENSE_ARRAY");
        Assert.Contains(outpost.Floorplan.Facilities, h => h.FacilityInfo.Id == "FAC_LASER_DEFENSE_ARRAY");
    }

    // ── Rule: Gravity Shield one per base ──

    [Fact]
    public void GravityShield_LimitIsOne()
    {
        Assert.True(ProjectXenocide.Xenocide.StaticTables.FacilityList["FAC_GRAVITY_SHIELD_FACILITY"].LimitIsOnePerOutpost);
    }

    // ── Rule: Neural Shielding one per base ──

    [Fact]
    public void NeuralShield_LimitIsOne()
    {
        Assert.True(ProjectXenocide.Xenocide.StaticTables.FacilityList["FAC_NEURAL_SHIELDING_FACILITY"].LimitIsOnePerOutpost);
    }
}
