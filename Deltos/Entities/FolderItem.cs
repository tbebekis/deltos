// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents a folder level in a document structure.
/// </summary>
public class FolderItem
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the FolderItem class.
    /// </summary>
    public FolderItem()
    {
    }

    // ● public
    /// <summary>
    /// Updates runtime references after loading the folder item graph.
    /// </summary>
    /// <param name="ParentItem">The parent folder item.</param>
    public void UpdateReferences(FolderItem ParentItem)
    {
        Parent = ParentItem;
        Child?.UpdateReferences(this);
    }

    // ● properties
    /// <summary>
    /// Gets or sets the parent folder item.
    /// </summary>
    [JsonIgnore]
    public FolderItem Parent { get; set; }
    /// <summary>
    /// Gets a value indicating whether this is the top folder item.
    /// </summary>
    public bool IsTop => Parent == null;
    /// <summary>
    /// Gets a value indicating whether this is a leaf folder item.
    /// </summary>
    public bool IsLeaf => Child == null;
    /// <summary>
    /// Gets the zero-based folder item level.
    /// </summary>
    public int Level => IsTop ? 0 : Parent.Level + 1;
    /// <summary>
    /// Gets or sets the child folder item.
    /// </summary>
    public FolderItem Child { get; set; }
    /// <summary>
    /// Gets or sets the display title of a folder item, such as Part, Chapter, or Section.
    /// </summary>
    public string Title { get; set; } = string.Empty;
}
