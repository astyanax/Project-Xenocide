using System;

using NLog;

var crashLogger = LogManager.GetLogger("Crash");

AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    crashLogger.Fatal("=== UNHANDLED EXCEPTION ===\n{0}", args.ExceptionObject);
};

try
{
    using var game = new ProjectXenocide.Xenocide();
    game.Run();
}
catch (Exception ex)
{
    crashLogger.Fatal(ex, "=== GAME CRASHED ===");
    LogManager.Shutdown();
    throw;
}
