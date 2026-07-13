// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Provides find and replace options.
/// </summary>
public class FindReplaceOptions
{
    // ● public
    /// <summary>
    /// Clears the options.
    /// </summary>
    public void Clear()
    {
        TextToFind = string.Empty;
        ReplaceWith = string.Empty;
        MatchCase = false;
        WholeWord = false;
        Replace = false;
        ReplaceAll = false;
    }

    // ● properties
    /// <summary>
    /// Gets or sets the text to find.
    /// </summary>
    public string TextToFind { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the replacement text.
    /// </summary>
    public string ReplaceWith { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether matching is case-sensitive.
    /// </summary>
    public bool MatchCase { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether matching is whole-word only.
    /// </summary>
    public bool WholeWord { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the current finding should be replaced.
    /// </summary>
    public bool Replace { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether all findings should be replaced.
    /// </summary>
    public bool ReplaceAll { get; set; }
}
