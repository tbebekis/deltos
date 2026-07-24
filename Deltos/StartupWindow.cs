// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Startup window shown while the application initializes.
/// </summary>
public class StartupWindow: Window
{
    // ● private fields
    bool fCanClose;

    // ● protected
    /// <summary>
    /// Prevents user-initiated closing while the application initializes.
    /// </summary>
    /// <param name="e">The closing event arguments.</param>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!fCanClose)
            e.Cancel = true;

        base.OnClosing(e);
    }

    // ● private
    /// <summary>
    /// Creates the startup image.
    /// </summary>
    /// <returns>The startup image.</returns>
    Image CreateStartupImage()
    {
        Image Result = new Image();
        Result.Width = 540;
        Result.Height = 300;
        Result.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        Result.Margin = new Thickness(0, 14, 0, 34);
        Result.Stretch = Avalonia.Media.Stretch.Uniform;

        Uri Uri = AvaloniaAssets.FindUri("Assets", "Deltos-Startup.png");
        AvaloniaAssets.SetImage(Result, Uri);

        return Result;
    }
    /// <summary>
    /// Creates the startup title.
    /// </summary>
    /// <returns>The startup title.</returns>
    Control CreateTitle()
    {
        TextBlock Result = new TextBlock();
        Result.Text = "Deltos";
        Result.FontSize = 34;
        Result.FontWeight = Avalonia.Media.FontWeight.SemiBold;
        Result.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        return Result;
    }
    /// <summary>
    /// Creates the startup please-wait panel.
    /// </summary>
    /// <returns>The startup please-wait panel.</returns>
    Control CreatePleaseWaitPanel()
    {
        StackPanel Result = new StackPanel();
        Result.Width = 320;
        Result.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        Result.Margin = new Thickness(0, 0, 0, 0);
        Result.Spacing = 12;

        TextBlock TitleText = new TextBlock();
        TitleText.Text = "Starting...";
        TitleText.FontWeight = Avalonia.Media.FontWeight.SemiBold;
        TitleText.FontSize = 16;
        TitleText.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        Result.Children.Add(TitleText);

        ProgressBar Progress = new ProgressBar();
        Progress.IsIndeterminate = true;
        Progress.Height = 18;
        Result.Children.Add(Progress);

        return Result;
    }
    /// <summary>
    /// Creates the startup window content.
    /// </summary>
    /// <returns>The startup window content.</returns>
    Control CreateContent()
    {
        Grid Result = new Grid();
        Result.Background = Avalonia.Media.Brushes.White;

        StackPanel Panel = new StackPanel();
        Panel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        Panel.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
        Panel.Margin = new Thickness(0, 42, 0, 0);
        Panel.Spacing = 0;

        Panel.Children.Add(CreateTitle());
        Panel.Children.Add(CreateStartupImage());
        Panel.Children.Add(CreatePleaseWaitPanel());

        Result.Children.Add(Panel);

        return Result;
    }

    // ● public
    /// <summary>
    /// Closes this window from application code.
    /// </summary>
    public void CloseWindow()
    {
        fCanClose = true;
        Close();
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the StartupWindow class.
    /// </summary>
    public StartupWindow()
    {
        Title = "Deltos";
        WindowState = WindowState.Maximized;
        CanResize = false;
        WindowDecorations = WindowDecorations.None;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        ShowInTaskbar = true;

        Content = CreateContent();
    }
}
