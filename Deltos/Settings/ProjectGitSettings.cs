// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Project git settings.
/// </summary>
public class ProjectGitSettings
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the ProjectGitSettings class.
    /// </summary>
    public ProjectGitSettings()
    {
    }

    // ● properties
    /// <summary>
    /// Gets or sets the remote name.
    /// </summary>
    public string RemoteName { get; set; } = "origin";
    /// <summary>
    /// Gets or sets the branch to push.
    /// </summary>
    public string Branch { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the remote repository URL.
    /// </summary>
    public string RemoteUrl { get; set; } = string.Empty;
}
