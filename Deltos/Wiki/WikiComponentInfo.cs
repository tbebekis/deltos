// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Wiki;

/// <summary>
/// Provides wiki component information.
/// </summary>
public class WikiComponentInfo
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the WikiComponentInfo class.
    /// </summary>
    public WikiComponentInfo()
    {
    }

    // ● properties
    /// <summary>
    /// Gets or sets the component.
    /// </summary>
    public Component Component { get; set; }
    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the category.
    /// </summary>
    public string Category { get; set; } = string.Empty;
    /// <summary>
    /// Gets aliases.
    /// </summary>
    public List<string> AliasList { get; } = new();
    /// <summary>
    /// Gets tags.
    /// </summary>
    public List<string> TagList { get; } = new();
    /// <summary>
    /// Gets or sets the source markdown text.
    /// </summary>
    public string Text { get; set; } = string.Empty;
}
