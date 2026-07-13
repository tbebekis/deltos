// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Project settings stored in the project folder.
/// </summary>
public class ProjectSettings
{
    // ● public
    /// <summary>
    /// Loads project settings.
    /// </summary>
    /// <param name="Project">The project.</param>
    /// <returns>The loaded project settings.</returns>
    static public ProjectSettings Load(Project Project)
    {
        ProjectSettings Result = new ProjectSettings();
        string FilePath = GetFilePath(Project);
        if (System.IO.File.Exists(FilePath))
            Json.LoadFromFile(Result, FilePath);

        Result.EnsureDefaults();
        return Result;
    }
    /// <summary>
    /// Returns the project settings file path.
    /// </summary>
    /// <param name="Project">The project.</param>
    /// <returns>The project settings file path.</returns>
    static public string GetFilePath(Project Project)
    {
        if (Project == null || string.IsNullOrWhiteSpace(Project.ProjectPath))
            throw new InvalidOperationException("No project is open.");

        return System.IO.Path.Combine(Project.ProjectPath, FileName);
    }
    /// <summary>
    /// Saves the project settings.
    /// </summary>
    /// <param name="Project">The project.</param>
    public void Save(Project Project)
    {
        EnsureDefaults();
        Json.SaveToFile(this, GetFilePath(Project));
    }
    /// <summary>
    /// Ensures nested settings instances exist.
    /// </summary>
    public void EnsureDefaults()
    {
        Git ??= new ProjectGitSettings();
        Wiki ??= new ProjectWikiSettings();

        if (string.IsNullOrWhiteSpace(Git.RemoteName))
            Git.RemoteName = "origin";
    }

    // ● properties
    /// <summary>
    /// Gets or sets git settings.
    /// </summary>
    public ProjectGitSettings Git { get; set; } = new();
    /// <summary>
    /// Gets or sets wiki settings.
    /// </summary>
    public ProjectWikiSettings Wiki { get; set; } = new();
    /// <summary>
    /// Gets the project settings file name.
    /// </summary>
    static public string FileName => "ProjectSettings.json";
}
