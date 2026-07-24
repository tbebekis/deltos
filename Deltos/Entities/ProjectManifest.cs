// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Describes the project storage contract version.
/// </summary>
public class ProjectManifest
{
    // ● static public
    /// <summary>
    /// Loads a project manifest from a project folder.
    /// </summary>
    /// <param name="ProjectPath">The project folder path.</param>
    /// <returns>The loaded project manifest.</returns>
    static public ProjectManifest Load(string ProjectPath)
    {
        ProjectManifest Result = new ProjectManifest();
        string FilePath = GetFilePath(ProjectPath);
        if (System.IO.File.Exists(FilePath))
            Json.LoadFromFile(Result, FilePath);

        return Result;
    }
    /// <summary>
    /// Saves a project manifest to a project folder.
    /// </summary>
    /// <param name="ProjectPath">The project folder path.</param>
    public void Save(string ProjectPath)
    {
        if (StorageVersion == 0)
            StorageVersion = CurrentStorageVersion;

        Json.SaveToFile(this, GetFilePath(ProjectPath));
    }
    /// <summary>
    /// Returns the manifest file path for a project folder.
    /// </summary>
    /// <param name="ProjectPath">The project folder path.</param>
    /// <returns>The manifest file path.</returns>
    static public string GetFilePath(string ProjectPath)
    {
        if (string.IsNullOrWhiteSpace(ProjectPath))
            throw new InvalidOperationException("The project storage path is empty.");

        return System.IO.Path.Combine(ProjectPath, FileName);
    }

    // ● properties
    /// <summary>
    /// Gets the current project storage version.
    /// </summary>
    static public int CurrentStorageVersion => 1;
    /// <summary>
    /// Gets the manifest file name.
    /// </summary>
    static public string FileName => "ProjectManifest.json";
    /// <summary>
    /// Gets or sets the project storage version.
    /// </summary>
    public int StorageVersion { get; set; }
}
