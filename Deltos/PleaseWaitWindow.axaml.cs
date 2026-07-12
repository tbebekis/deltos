// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Displays a non-closable please-wait window while a UI-owned operation is running.
/// </summary>
public partial class PleaseWaitWindow: Window
{
    // ● private fields
    bool fCanClose;

    // ● protected
    /// <summary>
    /// Prevents user-initiated closing while the operation is running.
    /// </summary>
    /// <param name="e">The closing event arguments.</param>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!fCanClose)
            e.Cancel = true;

        base.OnClosing(e);
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the PleaseWaitWindow class.
    /// </summary>
    public PleaseWaitWindow()
    {
        InitializeComponent();
    }
    /// <summary>
    /// Initializes a new instance of the PleaseWaitWindow class.
    /// </summary>
    /// <param name="Message">The message to display.</param>
    public PleaseWaitWindow(string Message)
        : this()
    {
        this.Message = Message;
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

    // ● properties
    /// <summary>
    /// Gets or sets the displayed message.
    /// </summary>
    public string Message
    {
        get => lblMessage.Text;
        set => lblMessage.Text = value;
    }
}
