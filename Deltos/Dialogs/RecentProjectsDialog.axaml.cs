// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Provides input and result data for the RecentProjectsDialog.
/// </summary>
public class RecentProjectsDialogData
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the RecentProjectsDialogData class.
    /// </summary>
    public RecentProjectsDialogData()
    {
    }

    // ● properties
    /// <summary>
    /// Gets or sets the recent project folder paths.
    /// </summary>
    public List<string> RecentProjects { get; set; } = new();
    /// <summary>
    /// Gets or sets the selected project folder path.
    /// </summary>
    public string SelectedProjectPath { get; set; } = string.Empty;
}

/// <summary>
/// Lets the user select a recent project.
/// </summary>
public partial class RecentProjectsDialog: DialogWindow
{
    // ● private fields
    /// <summary>
    /// The edited dialog data.
    /// </summary>
    RecentProjectsDialogData fData;

    // ● private
    /// <summary>
    /// Creates a project list item.
    /// </summary>
    /// <param name="ProjectPath">The project path.</param>
    /// <returns>The created list box item.</returns>
    ListBoxItem CreateProjectListItem(string ProjectPath)
    {
        return new ListBoxItem
        {
            Content = ProjectPath,
            Tag = ProjectPath
        };
    }
    /// <summary>
    /// Reloads the project list.
    /// </summary>
    void ReloadProjectList()
    {
        lboProjects.Items.Clear();

        foreach (string ProjectPath in fData.RecentProjects)
            lboProjects.Items.Add(CreateProjectListItem(ProjectPath));

        if (lboProjects.Items.Count > 0)
            lboProjects.SelectedIndex = 0;

        UpdateButtonState();
    }
    /// <summary>
    /// Updates command button state.
    /// </summary>
    void UpdateButtonState()
    {
        bool HasSelection = lboProjects.SelectedItem is ListBoxItem;
        btnRemove.IsEnabled = HasSelection;
        btnClear.IsEnabled = lboProjects.Items.Count > 0;
    }
    /// <summary>
    /// Accepts the dialog changes.
    /// </summary>
    void AcceptDialog()
    {
        fData.SelectedProjectPath = string.Empty;

        if (lboProjects.SelectedItem is ListBoxItem Item && Item.Tag is string ProjectPath)
            fData.SelectedProjectPath = ProjectPath;

        ResultData = fData;
        ModalResult = ModalResult.Ok;
    }
    /// <summary>
    /// Handles project selection changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void ProjectsSelectionChanged(object Sender, SelectionChangedEventArgs Args)
    {
        UpdateButtonState();
    }
    /// <summary>
    /// Handles project double-tap.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void ProjectsDoubleTapped(object Sender, TappedEventArgs Args)
    {
        if (lboProjects.SelectedItem is ListBoxItem)
            AcceptDialog();
    }
    /// <summary>
    /// Handles the Remove from List button click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void Remove_Click(object Sender, RoutedEventArgs Args)
    {
        if (lboProjects.SelectedItem is ListBoxItem Item && Item.Tag is string ProjectPath)
        {
            fData.RecentProjects.RemoveAll(x => x.IsSameText(ProjectPath));
            ReloadProjectList();
        }
    }
    /// <summary>
    /// Handles the Clear All button click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void Clear_Click(object Sender, RoutedEventArgs Args)
    {
        fData.RecentProjects.Clear();
        ReloadProjectList();
    }
    /// <summary>
    /// Handles the OK button click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void OK_Click(object Sender, RoutedEventArgs Args)
    {
        AcceptDialog();
    }
    /// <summary>
    /// Handles the Cancel button click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void Cancel_Click(object Sender, RoutedEventArgs Args)
    {
        ModalResult = ModalResult.Cancel;
    }

    // ● protected
    /// <summary>
    /// Loads item values into the dialog controls.
    /// </summary>
    protected override async Task ItemToControls()
    {
        RecentProjectsDialogData Source = InputData as RecentProjectsDialogData ?? new RecentProjectsDialogData();
        fData = new RecentProjectsDialogData();
        fData.RecentProjects.AddRange(Source.RecentProjects ?? new List<string>());
        fData.SelectedProjectPath = string.Empty;

        ReloadProjectList();
        await Task.CompletedTask;
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the RecentProjectsDialog class.
    /// </summary>
    public RecentProjectsDialog()
    {
        InitializeComponent();
    }
}
