// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Program entry point.
/// </summary>
class Program
{
    // ● public
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    [STAThread]
    static public void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    /// <summary>
    /// Builds the Avalonia application.
    /// </summary>
    /// <returns>The application builder.</returns>
    static public AppBuilder BuildAvaloniaApp()
    {
        AppBuilder Result = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

        if (OperatingSystem.IsLinux())
        {
            Result.With(new X11PlatformOptions
            {
                UseDBusMenu = false
            });
        }

        return Result;
    }
}
