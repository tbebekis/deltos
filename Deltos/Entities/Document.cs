// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents a document inside a project.
/// </summary>
public class Document: BaseItem
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the Document class.
    /// </summary>
    public Document()
    {
    }

    // ● public
    /// <summary>
    /// Updates runtime references after loading the document graph.
    /// </summary>
    /// <param name="ParentItem">The parent item.</param>
    public override void UpdateReferences(BaseItem ParentItem)
    {
        base.UpdateReferences(ParentItem);

        foreach (Folder Folder in Folders)
            Folder.UpdateReferences(this);
    }
    
    // ● properties
    /// <summary>
    /// Gets the item type.
    /// </summary>
    public override ItemType Type => ItemType.Document;
    /// <summary>
    /// Gets the document structure file name.
    /// </summary>
    static public string StructureFileName => "Structure.json";
    /// <summary>
    /// Gets or sets the document title.
    /// </summary>
    public override string Title
    {
        get => base.Title;
        set => base.Title = value;
    }
    /// <summary>
    /// Gets the document display title.
    /// </summary>
    public override string DisplayTitle => base.DisplayTitle;
    /// <summary>
    /// Gets or sets the document folders.
    /// </summary>
    public List<Folder> Folders { get; set; } = new();
    /// <summary>
    /// Gets or sets the document folder structure.
    /// </summary>
    public FolderItem Structure { get; set; } = new();
}
