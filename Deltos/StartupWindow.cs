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
    /// Creates the startup logo image.
    /// </summary>
    /// <returns>The startup logo image.</returns>
    Image CreateLogoImage()
    {
        Image Result = new Image();
        Result.Width = 42;
        Result.Height = 42;
        Result.Stretch = Avalonia.Media.Stretch.Uniform;

        Uri Uri = AvaloniaAssets.FindUri("Resources/Images", "Deltos.png");
        AvaloniaAssets.SetImage(Result, Uri);

        return Result;
    }
    /// <summary>
    /// Creates the startup window header.
    /// </summary>
    /// <returns>The startup window header.</returns>
    Control CreateHeader()
    {
        Grid Result = new Grid();
        Result.ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto");
        Result.Margin = new Thickness(18, 14, 18, 8);

        Image LogoImage = CreateLogoImage();
        LogoImage.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        Grid.SetColumn(LogoImage, 0);
        Result.Children.Add(LogoImage);

        TextBlock TitleText = new TextBlock();
        TitleText.Text = "Deltos";
        TitleText.FontSize = 24;
        TitleText.FontWeight = Avalonia.Media.FontWeight.SemiBold;
        TitleText.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        TitleText.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        Grid.SetColumn(TitleText, 1);
        Result.Children.Add(TitleText);

        Border BalanceBox = new Border();
        BalanceBox.Width = 42;
        Grid.SetColumn(BalanceBox, 2);
        Result.Children.Add(BalanceBox);

        return Result;
    }
    /// <summary>
    /// Creates the startup please-wait panel.
    /// </summary>
    /// <returns>The startup please-wait panel.</returns>
    Control CreatePleaseWaitPanel()
    {
        Border Result = new Border();
        Result.Width = 420;
        Result.Padding = new Thickness(20);
        Result.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        Result.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        Result.BorderBrush = Avalonia.Media.Brushes.LightGray;
        Result.BorderThickness = new Thickness(1);
        Result.CornerRadius = new CornerRadius(4);

        Grid Panel = new Grid();
        Panel.RowDefinitions = new RowDefinitions("Auto,8,Auto,20,Auto");

        TextBlock TitleText = new TextBlock();
        TitleText.Text = "Please wait...";
        TitleText.FontWeight = Avalonia.Media.FontWeight.SemiBold;
        TitleText.FontSize = 15;
        Grid.SetRow(TitleText, 0);
        Panel.Children.Add(TitleText);

        TextBlock Message = new TextBlock();
        Message.Text = "Initializing application...";
        Message.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        Message.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        Grid.SetRow(Message, 2);
        Panel.Children.Add(Message);

        ProgressBar Progress = new ProgressBar();
        Progress.IsIndeterminate = true;
        Progress.Height = 18;
        Grid.SetRow(Progress, 4);
        Panel.Children.Add(Progress);

        Result.Child = Panel;

        return Result;
    }
    /// <summary>
    /// Creates the startup window content.
    /// </summary>
    /// <returns>The startup window content.</returns>
    Control CreateContent()
    {
        Grid Result = new Grid();
        Result.RowDefinitions = new RowDefinitions("Auto,*");
        Result.Background = Avalonia.Media.Brushes.White;

        Control Header = CreateHeader();
        Grid.SetRow(Header, 0);
        Result.Children.Add(Header);

        Control PleaseWaitPanel = CreatePleaseWaitPanel();
        Grid.SetRow(PleaseWaitPanel, 1);
        Result.Children.Add(PleaseWaitPanel);

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
        Width = 560;
        Height = 300;
        MinWidth = 560;
        MinHeight = 300;
        CanResize = false;
        WindowDecorations = WindowDecorations.None;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        ShowInTaskbar = true;

        Content = CreateContent();
    }
}
