// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = AppHost.CreateStartupWindow();
            desktop.MainWindow.Opened += async (Sender, Args) =>
            {
                await AppHost.Start(desktop);
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
