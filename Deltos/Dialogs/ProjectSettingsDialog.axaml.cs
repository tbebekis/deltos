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
            Result.Wiki.WikiFolderPath = Source.Wiki.WikiFolderPath;
            Result.Wiki.WikiFolderPath2 = Source.Wiki.WikiFolderPath2;
            Result.Wiki.HomeComponentTitle = Source.Wiki.HomeComponentTitle;
            Result.Wiki.AboutComponentTitle = Source.Wiki.AboutComponentTitle;
            Result.Wiki.GenerateTagPages = Source.Wiki.GenerateTagPages;
            Result.Wiki.SiteBaseUrl = Source.Wiki.SiteBaseUrl;
            Result.Wiki.DefaultSocialImageUrl = Source.Wiki.DefaultSocialImageUrl;
        }

        Result.EnsureDefaults();
        ApplyDefaultWikiFolders(Result);
        return Result;
    }
    /// <summary>
    /// Applies default wiki folder paths to empty settings.
    /// </summary>
    /// <param name="Settings">The settings.</param>
    void ApplyDefaultWikiFolders(ProjectSettings Settings)
    {
        Project Project = AppHost.CurrentProject;
        if (Project == null || string.IsNullOrWhiteSpace(Project.ProjectPath))
            return;

        string ProjectFolderName = System.IO.Path.GetFileName(Project.ProjectPath);
        if (string.IsNullOrWhiteSpace(ProjectFolderName))
            return;

        if (string.IsNullOrWhiteSpace(Settings.Wiki.WikiFolderPath))
            Settings.Wiki.WikiFolderPath = System.IO.Path.Combine(Project.ProjectPath, $"{ProjectFolderName}_Wiki");

        if (string.IsNullOrWhiteSpace(Settings.Wiki.WikiFolderPath2))
            Settings.Wiki.WikiFolderPath2 = System.IO.Path.Combine(Project.ProjectPath, $"{ProjectFolderName}_Wiki2");
    }
    /// <summary>
    /// Loads settings into controls.
    /// </summary>
    void SettingsToControls()
    {
        edtRemoteName.Text = fSettings.Git.RemoteName;
        edtBranch.Text = fSettings.Git.Branch;
        edtRemoteUrl.Text = fSettings.Git.RemoteUrl;
        edtWikiFolderPath.Text = fSettings.Wiki.WikiFolderPath;
        edtWikiFolderPath2.Text = fSettings.Wiki.WikiFolderPath2;
        edtHomeComponentTitle.Text = fSettings.Wiki.HomeComponentTitle;
        edtAboutComponentTitle.Text = fSettings.Wiki.AboutComponentTitle;
        chkGenerateTagPages.IsChecked = fSettings.Wiki.GenerateTagPages;
        edtSiteBaseUrl.Text = fSettings.Wiki.SiteBaseUrl;
        edtDefaultSocialImageUrl.Text = fSettings.Wiki.DefaultSocialImageUrl;
    }
    /// <summary>
    /// Saves controls into settings.
    /// </summary>
    void ControlsToSettings()
    {
        fSettings.Git.RemoteName = string.IsNullOrWhiteSpace(edtRemoteName.Text) ? "origin" : edtRemoteName.Text.Trim();
        fSettings.Git.Branch = string.IsNullOrWhiteSpace(edtBranch.Text) ? string.Empty : edtBranch.Text.Trim();
        fSettings.Git.RemoteUrl = string.IsNullOrWhiteSpace(edtRemoteUrl.Text) ? string.Empty : edtRemoteUrl.Text.Trim();
        fSettings.Wiki.WikiFolderPath = string.IsNullOrWhiteSpace(edtWikiFolderPath.Text) ? string.Empty : edtWikiFolderPath.Text.Trim();
        fSettings.Wiki.WikiFolderPath2 = string.IsNullOrWhiteSpace(edtWikiFolderPath2.Text) ? string.Empty : edtWikiFolderPath2.Text.Trim();
        fSettings.Wiki.HomeComponentTitle = string.IsNullOrWhiteSpace(edtHomeComponentTitle.Text) ? string.Empty : edtHomeComponentTitle.Text.Trim();
        fSettings.Wiki.AboutComponentTitle = string.IsNullOrWhiteSpace(edtAboutComponentTitle.Text) ? string.Empty : edtAboutComponentTitle.Text.Trim();
        fSettings.Wiki.GenerateTagPages = chkGenerateTagPages.IsChecked == true;
        fSettings.Wiki.SiteBaseUrl = string.IsNullOrWhiteSpace(edtSiteBaseUrl.Text) ? string.Empty : edtSiteBaseUrl.Text.Trim();
        fSettings.Wiki.DefaultSocialImageUrl = string.IsNullOrWhiteSpace(edtDefaultSocialImageUrl.Text) ? string.Empty : edtDefaultSocialImageUrl.Text.Trim();
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
