using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace Xenocide.Test.MonoGame;

public class BugRegressionTests
{
    [Fact]
    public void Scheduler_TestAddRemove_WithFutureDates()
    {
        var schedulerType = typeof(ProjectXenocide.Model.Scheduler);
        var scheduler = Activator.CreateInstance(schedulerType, true)!;
        var addMethod = schedulerType.GetMethod("Add")!;
        var removeMethod = schedulerType.GetMethod("Remove")!;
        var updateMethod = schedulerType.GetMethod("Update")!;
        var appointmentsField = schedulerType.GetField("appointments", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var countProperty = appointmentsField.FieldType.GetProperty("Count")!;

        var appointmentType = schedulerType.GetNestedType("TestAppointment", BindingFlags.NonPublic)!;
        var ctor = appointmentType.GetConstructor(new[] { typeof(DateTime) })!;

        var a1 = ctor.Invoke(new object[] { new DateTime(2099, 1, 1) });
        var a2 = ctor.Invoke(new object[] { new DateTime(2098, 1, 1) });
        var a3 = ctor.Invoke(new object[] { new DateTime(2100, 1, 1) });
        var now = new DateTime(2097, 1, 1);

        addMethod.Invoke(scheduler, new[] { a1 });
        addMethod.Invoke(scheduler, new[] { a2 });
        addMethod.Invoke(scheduler, new[] { a3 });
        var list = appointmentsField.GetValue(scheduler)!;
        Assert.Equal(3, (int)countProperty.GetValue(list)!);

        removeMethod.Invoke(scheduler, new[] { a3 });
        Assert.Equal(2, (int)countProperty.GetValue(list)!);

        addMethod.Invoke(scheduler, new[] { a3 });
        Assert.Equal(3, (int)countProperty.GetValue(list)!);

        updateMethod.Invoke(scheduler, new object[] { now });
        Assert.Equal(3, (int)countProperty.GetValue(list)!);

        updateMethod.Invoke(scheduler, new object[] { new DateTime(2098, 1, 2) });
        Assert.Equal(2, (int)countProperty.GetValue(list)!);

        updateMethod.Invoke(scheduler, new object[] { new DateTime(2100, 1, 2) });
        Assert.Equal(0, (int)countProperty.GetValue(list)!);
    }

    [Fact]
    public void MorlockModel_XnbExists()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Models", "Characters", "Alien", "Morlock.xnb");
        Assert.True(File.Exists(path), $"Expected Morlock.xnb at {path}");
    }

    [Fact]
    public void XenocideResourceManager_ReturnsButtonStrings()
    {
        var rmType = typeof(ProjectXenocide.Model.Scheduler).Assembly.GetType("Xenocide.Resources.XenocideResourceManager")!;
        var getMethod = rmType.GetMethod("Get", BindingFlags.Public | BindingFlags.Static)!;
        var result = (string)getMethod.Invoke(null, new object[] { "BUTTON_TIME_STOP" })!;
        Assert.NotNull(result);
        Assert.Equal("Stop", result);
    }

    [Fact]
    public void GumX_AllScreenReferencesHaveGusxFiles()
    {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string gumxPath = null!;
        for (int i = 0; i < 10; i++) { dir = Path.GetDirectoryName(dir)!; var p = Path.Combine(dir, "Xenocide.MonoGame", "Content", "Gum", "Xenocide.gumx"); if (File.Exists(p)) { gumxPath = p; break; } }
        Assert.True(gumxPath != null, "Could not find .gumx by searching up from assembly");

        var screensDir = Path.Combine(Path.GetDirectoryName(gumxPath)!, "Screens");
        Assert.True(Directory.Exists(screensDir), $"Screens directory not found: {screensDir}");

        var xml = XDocument.Load(gumxPath);
        XNamespace ns = xml.Root!.GetDefaultNamespace();
        var references = xml.Root.Descendants(ns + "ScreenReference").Select(x => x.Attribute("Name")?.Value).Where(n => n != null).ToList();

        var files = Directory.GetFiles(screensDir, "*.gusx").Select(Path.GetFileNameWithoutExtension).ToHashSet();

        foreach (var name in references)
            Assert.True(files.Contains(name!), $"ScreenReference '{name}' has no matching .gusx file in {screensDir}");
    }

    [Fact]
    public void GumX_AllGusxFilesHaveScreenReferences()
    {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string gumxPath = null!;
        for (int i = 0; i < 10; i++) { dir = Path.GetDirectoryName(dir)!; var p = Path.Combine(dir, "Xenocide.MonoGame", "Content", "Gum", "Xenocide.gumx"); if (File.Exists(p)) { gumxPath = p; break; } }
        Assert.True(gumxPath != null, "Could not find .gumx by searching up from assembly");

        var screensDir = Path.Combine(Path.GetDirectoryName(gumxPath)!, "Screens");
        Assert.True(Directory.Exists(screensDir), $"Screens directory not found: {screensDir}");

        var xml = XDocument.Load(gumxPath);
        XNamespace ns = xml.Root!.GetDefaultNamespace();
        var referenceNames = xml.Root.Descendants(ns + "ScreenReference").Select(x => x.Attribute("Name")?.Value).Where(n => n != null).ToHashSet();

        var files = Directory.GetFiles(screensDir, "*.gusx").Select(Path.GetFileNameWithoutExtension);

        foreach (var file in files)
            Assert.True(referenceNames.Contains(file!), $".gusx file '{file}' has no matching ScreenReference in .gumx");
    }
}
