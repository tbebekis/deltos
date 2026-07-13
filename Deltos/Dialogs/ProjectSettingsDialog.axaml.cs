// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Edits project settings.
/// </summary>
public partial class ProjectSettingsDialog: DialogWindow
{
    // ● private fields
    /// <summary>
    /// The edited settings.
    /// </summary>
    ProjectSettings fSettings;

    // ● private
    /// <summary>
    /// Creates a settings copy.
    /// </summary>
    /// <param name="Source">The source settings.</param>
    /// <returns>The settings copy.</returns>
    ProjectSettings CreateSettingsCopy(ProjectSettings Source)
    {
        ProjectSettings Result = new ProjectSettings();
        if (Source != null)
        {
            Source.EnsureDefaults();
            Result.Git.RemoteName = Source.Git.RemoteName;
            Result.Git.Branch = Source.Git.Branch;
            Result.Git.RemoteUrl = Source.Git.RemoteUrl;
        }

        Result.EnsureDefaults();
        return Result;
    }
    /// <summary>
    /// Loads settings into controls.
    /// </summary>
    void SettingsToControls()
    {
        edtRemoteName.Text = fSettings.Git.RemoteName;
        edtBranch.Text = fSettings.Git.Branch;
        edtRemoteUrl.Text = fSettings.Git.RemoteUrl;
    }
    /// <summary>
    /// Saves controls into settings.
    /// </summary>
    void ControlsToSettings()
    {
        fSettings.Git.RemoteName = string.IsNullOrWhiteSpace(edtRemoteName.Text) ? "origin" : edtRemoteName.Text.Trim();
        fSettings.Git.Branch = string.IsNullOrWhiteSpace(edtBranch.Text) ? string.Empty : edtBranch.Text.Trim();
        fSettings.Git.RemoteUrl = string.IsNullOrWhiteSpace(edtRemoteUrl.Text) ? string.Empty : edtRemoteUrl.Text.Trim();
        fSettings.EnsureDefaults();
    }
    /// <summary>
    /// Handles OK click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void OK_Click(object Sender, RoutedEventArgs Args)
    {
        ControlsToSettings();
        ResultData = fSettings;
        ModalResult = ModalResult.Ok;
    }
    /// <summary>
    /// Handles Cancel click.
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
        fSettings = CreateSettingsCopy(InputData as ProjectSettings);
        SettingsToControls();
        await Task.CompletedTask;
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the ProjectSettingsDialog class.
    /// </summary>
    public ProjectSettingsDialog()
    {
        InitializeComponent();
    }
}
