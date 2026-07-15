// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Application project lifecycle support.
/// </summary>
static public partial class AppHost
{
    // ● public
    /// <summary>
    /// Creates and opens a new project.
    /// </summary>
    /// <param name="ParentFolderPath">The parent folder path.</param>
    /// <param name="Title">The project title.</param>
    /// <returns>The created project.</returns>
    static public Project CreateProject(string ParentFolderPath, string Title)
    {
        CloseProject();

        Project Project = Project.Create(ParentFolderPath, Title);
        SetCurrentProject(Project, true);

        return Project;
    }

    /// <summary>
    /// Opens a project from a project folder path.
    /// </summary>
    /// <param name="ProjectPath">The project folder path.</param>
    /// <param name="SaveSettings">True to persist the project path as the last project.</param>
    /// <returns>The opened project.</returns>
    static public Project OpenProject(string ProjectPath, bool SaveSettings = true)
    {
        CloseProject();

        Project Project = Project.Open(ProjectPath);
        SetCurrentProject(Project, SaveSettings);

        return Project;
    }

    /// <summary>
    /// Opens the last project from settings.
    /// </summary>
    /// <returns>The opened project, or null.</returns>
    static public Project OpenLastProject()
    {
        if (Settings == null || !Settings.LoadLastProjectOnStartup)
            return null;

        string ProjectPath = Settings.LastProjectFolderPath;
        if (string.IsNullOrWhiteSpace(ProjectPath) || ProjectPath == "___")
            return null;

        if (!System.IO.Directory.Exists(ProjectPath))
            return null;

        try
        {
            return OpenProject(ProjectPath, false);
        }
        catch (Exception e)
        {
            LogBox.AppendLine(e);
            return null;
        }
    }

    /// <summary>
    /// Closes the current project and all project-owned UI.
    /// </summary>
    static public void CloseProject()
    {
        if (CurrentProject == null)
            return;

        string Title = CurrentProject.Title;

        ProjectClosed?.Invoke(null, EventArgs.Empty);
        ClearDirtyEditors();
        CloseAllUi();
        CurrentProject = null;

        LogBox.AppendLine($"Project closed: '{Title}'.");
    }

    /// <summary>
    /// Sets the current project.
    /// </summary>
    /// <param name="Project">The current project.</param>
    /// <param name="SaveSettings">True to persist the last project setting.</param>
    static public void SetCurrentProject(Project Project, bool SaveSettings)
    {
        CurrentProject = Project;

        if (Settings != null && Project != null)
        {
            if (SaveSettings)
                Settings.LastProjectFolderPath = Project.ProjectPath;

            Settings.AddRecentProject(Project.ProjectPath);
            Settings.Save();
        }

        if (Project != null)
        {
            ProjectOpened?.Invoke(null, EventArgs.Empty);
            ShowSideBarForms();
            LogBox.AppendLine($"Project opened: '{Project.Title}'.");
        }
    }

    // ● events
    /// <summary>
    /// Occurs after a project is opened.
    /// </summary>
    static public event EventHandler ProjectOpened;

    /// <summary>
    /// Occurs after a project is closed.
    /// </summary>
    static public event EventHandler ProjectClosed;
}
