// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Project wiki settings.
/// </summary>
public class ProjectWikiSettings
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the ProjectWikiSettings class.
    /// </summary>
    public ProjectWikiSettings()
    {
    }

    // ● properties
    /// <summary>
    /// Gets or sets the primary wiki output folder path.
    /// </summary>
    public string WikiFolderPath { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the secondary wiki output folder path.
    /// </summary>
    public string WikiFolderPath2 { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the home component title.
    /// </summary>
    public string HomeComponentTitle { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the about component title.
    /// </summary>
    public string AboutComponentTitle { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether tag pages are generated.
    /// </summary>
    public bool GenerateTagPages { get; set; } = true;
    /// <summary>
    /// Gets or sets the published site base URL.
    /// </summary>
    public string SiteBaseUrl { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the default social image URL.
    /// </summary>
    public string DefaultSocialImageUrl { get; set; } = string.Empty;
}
