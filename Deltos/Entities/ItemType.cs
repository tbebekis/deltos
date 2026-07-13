// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Defines the supported project item types.
/// </summary>
[Flags]
public enum ItemType
{
    /// <summary>
    /// No item type.
    /// </summary>
    None = 0,
    /// <summary>
    /// A project item.
    /// </summary>
    Project = 1,
    /// <summary>
    /// A component item.
    /// </summary>
    Component = 2,
    /// <summary>
    /// A document item.
    /// </summary>
    Document = 4,
    /// <summary>
    /// A folder item.
    /// </summary>
    Folder = 8,
    /// <summary>
    /// A text file item.
    /// </summary>
    TextFile = 16,
    /// <summary>
    /// A note item.
    /// </summary>
    Note = 32
}
