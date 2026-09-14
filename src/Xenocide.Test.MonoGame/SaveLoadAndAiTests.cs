using System.Reflection;

using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Utils;

using Xenocide.Utils;

namespace Xenocide.Test.MonoGame;

/// <summary>
/// Tests for the save/load serializer (GameStateSerializer + ModelJsonConverter)
/// and the alien AI task factory. These exercise global state, so they share the
/// "GameStateInit" collection with <see cref="InGameUnitTests"/>.
/// </summary>
[Collection("GameStateInit")]
public class SaveLoadAndAiTests : IDisposable
{
    public SaveLoadAndAiTests()
    {
        ProjectXenocide.Xenocide.Rng.ClearLoadedValues();

        var staticTables = new StaticTables();
        var staticTablesField = typeof(ProjectXenocide.Xenocide)
            .GetField("staticTables", BindingFlags.Static | BindingFlags.NonPublic)!;
        staticTablesField.SetValue(null, staticTables);
        staticTables.Populate();

        var gameBalanceField = typeof(ProjectXenocide.Xenocide)
            .GetField("gameBalance", BindingFlags.Static | BindingFlags.NonPublic)!;
        gameBalanceField.SetValue(null, new GameBalanceClass(Difficulty.Easy));

        ProjectXenocide.Xenocide.GameState = new GameState();
        ProjectXenocide.Xenocide.GameState.SetToStartGameCondition();
    }

    public void Dispose()
    {
        var staticTablesField = typeof(ProjectXenocide.Xenocide)
            .GetField("staticTables", BindingFlags.Static | BindingFlags.NonPublic)!;
        staticTablesField.SetValue(null, null);
        ProjectXenocide.Xenocide.GameState = null!;
    }

    [Fact]
    public void GameStateSerializer_RoundTripsFullGameState()
    {
        var original = ProjectXenocide.Xenocide.GameState;
        original.MessageLogEntries.Add(new MessageEntry
        {
            Text = "UFO detected over Europe",
            Type = MessageType.Warning,
            GameDay = 3,
            GameHour = 4,
            GameMinute = 5
        });

        using var stream = new MemoryStream();
        GameStateSerializer.Save(stream, original, "test-version");
        Assert.True(stream.Length > 0, "Serializer produced an empty save file.");

        stream.Position = 0;
        var loaded = GameStateSerializer.Load(stream, "test-version", out var error);

        Assert.Null(error);
        Assert.NotNull(loaded);
        Assert.NotNull(loaded!.GeoData);
        Assert.NotNull(loaded.GeoData.Planet);

        var entry = Assert.Single(loaded.MessageLogEntries);
        Assert.Equal("UFO detected over Europe", entry.Text);
        Assert.Equal(MessageType.Warning, entry.Type);
        Assert.Equal(3, entry.GameDay);
    }

    [Fact]
    public void GameStateSerializer_ReadHeader_ReturnsSavedMetadata()
    {
        using var stream = new MemoryStream();
        GameStateSerializer.Save(stream, ProjectXenocide.Xenocide.GameState, "1.2.3");
        stream.Position = 0;

        var header = GameStateSerializer.ReadHeader(stream);

        Assert.NotNull(header);
        Assert.Equal("1.2.3", header!.AssemblyVersion);
        Assert.False(string.IsNullOrEmpty(header.GameTime));
    }

    [Fact]
    public void GameStateSerializer_Load_RejectsNewerFormatVersion()
    {
        using var stream = new MemoryStream();
        GameStateSerializer.Save(stream, ProjectXenocide.Xenocide.GameState, "test-version");

        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());

        // SaveFileWrapper is [Serializable], so ModelJsonConverter writes the
        // auto-property backing field (e.g. "<FormatVersion>k__BackingField").
        var tampered = System.Text.RegularExpressions.Regex.Replace(
            json, "(FormatVersion[^:]*:\\s*)\\d+", "${1}9999");
        Assert.NotEqual(json, tampered);

        using var tamperedStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(tampered));
        var loaded = GameStateSerializer.Load(tamperedStream, "test-version", out var error);

        Assert.Null(loaded);
        Assert.NotNull(error);
        Assert.Contains("newer version", error);
    }

    [Fact]
    public void TaskFactory_Create_MapsMissionTypesToTasks()
    {
        var overmind = ProjectXenocide.Xenocide.GameState.GeoData.Overmind;
        var region = ProjectXenocide.Xenocide.GameState.GeoData.Planet.AllRegions[0];
        var factory = new ProjectXenocide.Model.Geoscape.AI.TaskFactory();

        Assert.IsType<BuildOutpostTask>(factory.Create(AlienMission.Outpost, overmind, region));
        Assert.IsType<ResearchTask>(factory.Create(AlienMission.Research, overmind, region));
        Assert.IsType<ResearchTask>(factory.Create(AlienMission.Abduction, overmind, region));
        Assert.IsType<InfiltrationTask>(factory.Create(AlienMission.Infiltration, overmind, region));
    }
}
