// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Wiki;

/// <summary>
/// Provides wiki build input information.
/// </summary>
public class WikiBuildInfo
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the WikiBuildInfo class.
    /// </summary>
    /// <param name="UseSecondaryText">True to use secondary component text.</param>
    public WikiBuildInfo(bool UseSecondaryText)
    {
        this.UseSecondaryText = UseSecondaryText;
    }

    // ● properties
    /// <summary>
    /// Gets or sets a value indicating whether secondary component text is used.
    /// </summary>
    public bool UseSecondaryText { get; set; }
    /// <summary>
    /// Gets or sets the source project.
    /// </summary>
    public Project Project { get; set; }
    /// <summary>
    /// Gets or sets the output folder path.
    /// </summary>
    public string OutputFolderPath { get; set; } = string.Empty;
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
    /// Gets or sets the site base URL.
    /// </summary>
    public string SiteBaseUrl { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the default social image URL.
    /// </summary>
    public string DefaultSocialImageUrl { get; set; } = string.Empty;
}
