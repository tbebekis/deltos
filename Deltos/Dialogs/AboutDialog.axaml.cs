// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Displays application information.
/// </summary>
public partial class AboutDialog: DialogWindow
{
    // ● private
    /// <summary>
    /// Handles the Close button click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    private void Close_Click(object Sender, RoutedEventArgs Args)
    {
        ModalResult = ModalResult.Ok;
    }
    /// <summary>
    /// Opens the Amazon author/book page.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void AmazonLink_PointerPressed(object Sender, PointerPressedEventArgs Args)
    {
        Process.Start(new ProcessStartInfo("https://www.amazon.com/dp/B0DJH77BDJ") { UseShellExecute = true });
    }

    // ● protected
    /// <summary>
    /// Handles the window opened event.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            btnClose.Focus(Avalonia.Input.NavigationMethod.Tab, Avalonia.Input.KeyModifiers.None);
        }, Avalonia.Threading.DispatcherPriority.Input);
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the AboutDialog class.
    /// </summary>
    public AboutDialog()
    {
        InitializeComponent();
    }
}
