using System.Reflection;

using CeGui;

namespace Xenocide.Test.MonoGame;

public class BugRegressionTests
{
    [Fact]
    public void WindowSheet_CanCastToGuiSheet()
    {
        var window = WindowManager.Instance.GetWindow("Root");
        var sheet = (GuiSheet)window;
        Assert.NotNull(sheet);
    }

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
    public void CreateButton_SetsName()
    {
        var button = GuiBuilder.CreateButton("test_button_id");
        Assert.Equal("test_button_id", button.Name);
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
}
