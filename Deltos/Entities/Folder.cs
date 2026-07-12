// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents a document folder item.
/// </summary>
public class Folder: BaseItem
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the Folder class.
    /// </summary>
    public Folder()
    {
    }

    // ● public
    /// <summary>
    /// Updates runtime references after loading the folder graph.
    /// </summary>
    /// <param name="ParentItem">The parent item.</param>
    public override void UpdateReferences(BaseItem ParentItem)
    {
        base.UpdateReferences(ParentItem);

        foreach (Folder Folder in Folders)
            Folder.UpdateReferences(this);

        foreach (TextFile File in Files)
            File.UpdateReferences(this);
    }
    
    // ● properties
    /// <summary>
    /// Gets the item type.
    /// </summary>
    public override ItemType Type => ItemType.Folder;
    /// <summary>
    /// Gets or sets the folder title.
    /// </summary>
    public override string Title
    {
        get => base.Title;
        set => base.Title = value;
    }
    /// <summary>
    /// Gets the folder display title.
    /// </summary>
    public override string DisplayTitle => base.DisplayTitle;
    /// <summary>
    /// Gets or sets the child folders.
    /// </summary>
    public List<Folder> Folders { get; set; } = new();
    /// <summary>
    /// Gets or sets the child text files.
    /// </summary>
    public List<TextFile> Files { get; set; } = new();
}
