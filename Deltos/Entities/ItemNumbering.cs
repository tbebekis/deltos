// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Specifies item numbering behavior for document output.
/// </summary>
public enum ItemNumbering
{
    /// <summary>
    /// Use automatic numbering.
    /// </summary>
    Automatic = 0,
    /// <summary>
    /// Do not number the item.
    /// </summary>
    None = 1,
    /// <summary>
    /// Use custom numbering text.
    /// </summary>
    Custom = 2
}
