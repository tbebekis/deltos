// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Application startup support.
/// </summary>
static public partial class AppHost
{
    // ● private
    /// <summary>
    /// Initializes global exception handling.
    /// </summary>
    static void InitializeGlobalExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (Sender, Args) =>
        {
            Exception Error = Args.ExceptionObject as Exception;
            HandleGlobalException(Error);
        };

        TaskScheduler.UnobservedTaskException += (Sender, Args) =>
        {
            HandleGlobalException(Args.Exception);
            Args.SetObserved();
        };
    }
    /// <summary>
    /// Handles a global exception.
    /// </summary>
    /// <param name="Error">The exception.</param>
    static void HandleGlobalException(Exception Error)
    {
        if (Error == null)
            return;

        Console.WriteLine(Error);
        LogBox.AppendLine(Error.ToString());
    }
    /// <summary>
    /// Returns the active owner window for owned dialogs.
    /// </summary>
    /// <returns>The active owner window, or null.</returns>
    static Window GetDialogOwner()
    {
        if (MainWindow != null && MainWindow.IsVisible)
            return MainWindow;

        if (StartupWindow != null && StartupWindow.IsVisible)
            return StartupWindow;

        return AvaloniaDesktop?.MainWindow;
    }

    // ● public
    /// <summary>
    /// Creates the startup window.
    /// </summary>
    /// <returns>The startup window.</returns>
    static public StartupWindow CreateStartupWindow()
    {
        StartupWindow = new StartupWindow();
        return StartupWindow;
    }
    /// <summary>
    /// Starts this application.
    /// </summary>
    /// <param name="AvaloniaDesktop">The Avalonia desktop lifetime.</param>
    static public async Task Start(IClassicDesktopStyleApplicationLifetime AvaloniaDesktop)
    {
        bool Success = true;
        AppHost.AvaloniaDesktop = AvaloniaDesktop;

        try
        {
            InitializeGlobalExceptionHandling();
            Initialize();

            MainWindow = new MainWindow();
            AvaloniaDesktop.MainWindow = MainWindow;
            MainWindow.Show();
        }
        catch (Exception e)
        {
            HandleGlobalException(e);
            Success = false;
        }

        if (StartupWindow != null)
            StartupWindow.CloseWindow();

        if (!Success)
            AvaloniaDesktop.Shutdown(1);

        await Task.CompletedTask;
    }
    /// <summary>
    /// Initializes this application.
    /// </summary>
    static public void Initialize()
    {
        SysConfig.ApplicationMode = ApplicationMode.Desktop;
        SysConfig.MainAssembly = typeof(AppHost).Assembly;
    }
    /// <summary>
    /// Shows the please-wait window.
    /// </summary>
    /// <param name="Message">The message to display.</param>
    /// <param name="Owner">The owner window.</param>
    /// <returns>The please-wait window.</returns>
    static public PleaseWaitWindow ShowPleaseWait(string Message = null, Window Owner = null)
    {
        if (PleaseWaitWindow == null)
        {
            PleaseWaitWindow = new PleaseWaitWindow();
            Owner ??= GetDialogOwner();

            if (Owner != null)
                PleaseWaitWindow.Show(Owner);
            else
                PleaseWaitWindow.Show();
        }

        if (!string.IsNullOrWhiteSpace(Message))
            PleaseWaitWindow.Message = Message;

        PleaseWaitWindow.Activate();

        return PleaseWaitWindow;
    }
    /// <summary>
    /// Hides the please-wait window.
    /// </summary>
    static public void HidePleaseWait()
    {
        if (PleaseWaitWindow == null)
            return;

        PleaseWaitWindow.CloseWindow();
        PleaseWaitWindow = null;
    }

    // ● properties
    /// <summary>
    /// Gets the startup window.
    /// </summary>
    static public StartupWindow StartupWindow { get; private set; }
    /// <summary>
    /// Gets the main window.
    /// </summary>
    static public MainWindow MainWindow { get; private set; }
    /// <summary>
    /// Gets the please-wait window.
    /// </summary>
    static public PleaseWaitWindow PleaseWaitWindow { get; private set; }
    /// <summary>
    /// Gets the Avalonia desktop lifetime.
    /// </summary>
    static public IClassicDesktopStyleApplicationLifetime AvaloniaDesktop { get; private set; }
}
