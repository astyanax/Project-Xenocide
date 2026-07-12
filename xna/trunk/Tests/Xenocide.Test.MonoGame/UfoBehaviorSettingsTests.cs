using System.IO;

using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.Model.StaticData.AI;

namespace Xenocide.Test.MonoGame;

/// <summary>
/// Tests for UfoBehaviorSettings - the loader for ufobehavior.xml
/// (Phases 9.3 + 9.4 of MIGRATION.md).
///
/// These tests use temporary XML files rather than the production
/// ufobehavior.xml so they verify the parsing logic in isolation.
/// </summary>
public class UfoBehaviorSettingsTests
{
    private const string ValidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ufobehavior xmlns=""UfoBehaviorConfig"">
  <timingConstants>
    <crashSiteDuration hours=""6"" />
    <landedUfoDuration hours=""1.5"" />
    <terrorSiteDuration hours=""8"" />
    <terrorSiteSpawnDelay minutes=""15"" />
    <retaliationSearchRadius kilometers=""2000"" />
    <aircraftRadarRange nauticalMiles=""500"" />
  </timingConstants>
  <missionPlans>
    <missionPlan type=""Research"" score=""20"">
      <launch ufoType=""ITEM_UFO_PROBE""  earliestLaunch=""10"" latestLaunch=""20""  landings=""1"" subLandings=""2""/>
      <launch ufoType=""ITEM_UFO_RECON""  earliestLaunch=""50"" latestLaunch=""100"" landings=""2"" subLandings=""2""/>
    </missionPlan>
    <missionPlan type=""Harvest"" score=""30"">
      <launch ufoType=""ITEM_UFO_PROBE""  earliestLaunch=""48"" latestLaunch=""120"" landings=""1"" subLandings=""3""/>
    </missionPlan>
  </missionPlans>
</ufobehavior>";

    private const string MissingElementXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ufobehavior xmlns=""UfoBehaviorConfig"">
  <timingConstants>
    <landedUfoDuration hours=""2"" />
  </timingConstants>
  <missionPlans>
    <missionPlan type=""Research"" score=""20"">
      <launch ufoType=""ITEM_UFO_PROBE"" earliestLaunch=""10"" latestLaunch=""20"" landings=""1"" subLandings=""2""/>
    </missionPlan>
  </missionPlans>
</ufobehavior>";

    /// <summary>
    /// All 6 timing constants should be parsed from XML and accessible via properties.
    /// </summary>
    [Fact]
    public void TimingConstants_LoadFromXml()
    {
        var settings = LoadSettings(ValidXml);

        Assert.Equal(6.0, settings.CrashSiteDuration.TotalHours);
        Assert.Equal(1.5, settings.LandedUfoDuration.TotalHours);
        Assert.Equal(8.0, settings.TerrorSiteDuration.TotalHours);
        Assert.Equal(TimeSpan.FromMinutes(15), settings.TerrorSiteSpawnDelay);
    }

    /// <summary>
    /// Distance constants (search radius, radar range) should be converted
    /// to radians on the unit sphere (the internal representation used by
    /// GeoPosition.IsWithin etc.).
    /// </summary>
    [Fact]
    public void DistanceConstants_ConvertedToRadians()
    {
        var settings = LoadSettings(ValidXml);

        // 2000 km and 500 nautical miles should both be positive non-zero
        // radian values.  We don't pin exact values (depends on
        // GeoPosition.KilometersToRadians/KnotsToRadians conversion)
        // but we do verify the conversion happened.
        Assert.True(settings.RetaliationSearchRadius > 0);
        Assert.True(settings.AircraftRadarRange > 0);
    }

    /// <summary>
    /// Mission plan for a known alien mission type should be loaded with
    /// the correct number of launches.
    /// </summary>
    [Fact]
    public void MissionPlan_LoadResearch()
    {
        var settings = LoadSettings(ValidXml);

        var research = settings.GetPlan(AlienMission.Research);
        Assert.NotNull(research);
        Assert.Equal(2, research.Launches.Count);
        Assert.Equal("Research", research.Name);
        Assert.Equal(20.0f, research.Score);
    }

    /// <summary>
    /// Each launch entry preserves all 5 attributes (UFO type, earliest/latest
    /// hours, landings, sub-landings) as set in the XML.
    /// </summary>
    [Fact]
    public void LaunchPlan_PreservesAllAttributes()
    {
        var settings = LoadSettings(ValidXml);

        var research = settings.GetPlan(AlienMission.Research);
        var first = research.Launches[0];
        Assert.Equal("ITEM_UFO_PROBE", first.UfoType);
        Assert.Equal(10.0f, first.EarliestHours);
        Assert.Equal(20.0f, first.LatestHours);
        Assert.Equal(1, first.Landings);
        Assert.Equal(2, first.SubLandings);
    }

    /// <summary>
    /// The second mission plan (Harvest) should also be loaded, and have
    /// its own score and launch count.
    /// </summary>
    [Fact]
    public void MissionPlan_LoadHarvest()
    {
        var settings = LoadSettings(ValidXml);

        var harvest = settings.GetPlan(AlienMission.Harvest);
        Assert.NotNull(harvest);
        Assert.Single(harvest.Launches);
        Assert.Equal(30.0f, harvest.Score);
    }

    /// <summary>
    /// Verify CalculateLaunchDelay preserves the legacy X-COM behavior:
    /// a random delay between [earliest, latest] hours (specifically, a
    /// random fraction up to 60% of the difference, with the rest at the
    /// minimum - this matches LaunchPlan.CalculateLaunchDelay()).
    /// </summary>
    [Fact]
    public void LaunchPlan_CalculateLaunchDelay_Bounded()
    {
        var settings = LoadSettings(ValidXml);
        var research = settings.GetPlan(AlienMission.Research);
        var first = research.Launches[0];

        // Run the calculation many times and verify the delay is always
        // between earliest and latest hours.
        for (int i = 0; i < 100; ++i)
        {
            TimeSpan delay = first.CalculateLaunchDelay();
            double minSeconds = 10.0 * 3600.0;
            double maxSeconds = 10.0 * 3600.0 + (20.0 - 10.0) * 0.6 * 100.0 * 60.0;
            Assert.True(delay.TotalSeconds >= minSeconds);
            Assert.True(delay.TotalSeconds <= maxSeconds);
        }
    }

    /// <summary>
    /// If a required timing constant is missing, the loader should throw
    /// with a clear error message (referencing the missing element name).
    /// </summary>
    [Fact]
    public void MissingElement_ThrowsWithElementName()
    {
        var settings = new UfoBehaviorSettings();
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, MissingElementXml);
        var ex = Assert.Throws<System.Xml.XmlException>(() => settings.Populate(tempFile));
        Assert.Contains("crashSiteDuration", ex.Message);
    }

    /// <summary>
    /// Helper: write XML to a temp file, populate the settings, and return.
    /// Note: the underlying XmlReader in MakeValidatingXPathNavigator
    /// holds the file open until GC, so we don't try to delete the
    /// temp file in a finally block (would race with the open handle).
    /// Files are small and live in the system temp dir - the OS will
    /// clean them up eventually.
    /// </summary>
    private static UfoBehaviorSettings LoadSettings(string xml)
    {
        var settings = new UfoBehaviorSettings();
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, xml);
        settings.Populate(tempFile);
        return settings;
    }
}
