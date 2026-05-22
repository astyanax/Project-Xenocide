using System;

AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    Console.Error.WriteLine("=== UNHANDLED EXCEPTION ===");
    Console.Error.WriteLine(args.ExceptionObject);
};

try
{
    using var game = new ProjectXenocide.Xenocide();
    game.Run();
}
catch (Exception ex)
{
    Console.Error.WriteLine("=== GAME CRASHED ===");
    Console.Error.WriteLine(ex);
    throw;
}
