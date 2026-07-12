// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Main application window.
/// </summary>
public partial class MainWindow : Window
{
    // ● private fields
    private Tripous.Desktop.ToolBar fToolBar;

    // ● private
    /// <summary>
    /// Initializes the window controls.
    /// </summary>
    private void WindowInitialize()
    {
        CreateToolBar();
        UpdateStatusBar("Ready", "No project open");
    }

    /// <summary>
    /// Creates the main toolbar.
    /// </summary>
    private void CreateToolBar()
    {
        fToolBar = new();
        fToolBar.Panel = pnlToolBar;

        Button Button = fToolBar.AddButton("book_add.png", "New Project", AnyToolBarClick);
        Button.Tag = "NewProject";

        Button = fToolBar.AddButton("book_open.png", "Open Project", AnyToolBarClick);
        Button.Tag = "OpenProject";

        Button = fToolBar.AddButton("disk.png", "Save Project", AnyToolBarClick);
        Button.Tag = "SaveProject";

        fToolBar.AddSeparator();

        Button = fToolBar.AddButton("document_torn.png", "New Document", AnyToolBarClick);
        Button.Tag = "NewDocument";

        Button = fToolBar.AddButton("folder.png", "New Folder", AnyToolBarClick);
        Button.Tag = "NewFolder";

        Button = fToolBar.AddButton("file_extension_txt.png", "New Text File", AnyToolBarClick);
        Button.Tag = "NewTextFile";

        fToolBar.AddSeparator();

        Button = fToolBar.AddButton("arrow_up.png", "Move Up", AnyToolBarClick);
        Button.Tag = "MoveUp";

        Button = fToolBar.AddButton("arrow_down.png", "Move Down", AnyToolBarClick);
        Button.Tag = "MoveDown";

        Button = fToolBar.AddButton("bullet_edit.png", "Rename", AnyToolBarClick);
        Button.Tag = "Rename";

        Button = fToolBar.AddButton("bin.png", "Delete", AnyToolBarClick);
        Button.Tag = "Delete";
    }

    /// <summary>
    /// Handles toolbar button clicks.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    private void AnyToolBarClick(object Sender, RoutedEventArgs Args)
    {
        if (Sender is Control Control && Control.Tag is string CommandName)
            UpdateStatusBar($"Command: {CommandName}", "No project open");
    }

    /// <summary>
    /// Updates the status bar.
    /// </summary>
    /// <param name="StatusText">The main status text.</param>
    /// <param name="ProjectText">The project status text.</param>
    private void UpdateStatusBar(string StatusText, string ProjectText)
    {
        lblStatus.Text = StatusText;
        lblProjectStatus.Text = ProjectText;
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the MainWindow class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        WindowInitialize();
    }
}
