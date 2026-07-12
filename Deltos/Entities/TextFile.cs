// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents a markdown text item stored in its own folder.
/// </summary>
public class TextFile: BaseItem
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the TextFile class.
    /// </summary>
    public TextFile()
    {
    }

    // ● public
    /// <summary>
    /// Updates runtime references after loading the text file.
    /// </summary>
    /// <param name="ParentItem">The parent item.</param>
    public override void UpdateReferences(BaseItem ParentItem)
    {
        base.UpdateReferences(ParentItem);
    }
    
    // ● properties
    /// <summary>
    /// Gets the item type.
    /// </summary>
    public override ItemType Type => ItemType.TextFile;
    /// <summary>
    /// Gets the primary text file name.
    /// </summary>
    static public string TextFileName => "Text.md";
    /// <summary>
    /// Gets the secondary text file name.
    /// </summary>
    static public string Text2FileName => "Text2.md";
    /// <summary>
    /// Gets the abstraction text file name.
    /// </summary>
    static public string AbstractionFileName => "Abstraction.md";
    /// <summary>
    /// Gets the draft text file name.
    /// </summary>
    static public string DraftFileName => "Draft.md";
    /// <summary>
    /// Gets or sets the text file title.
    /// </summary>
    public override string Title
    {
        get => base.Title;
        set => base.Title = value;
    }
    /// <summary>
    /// Gets the text file display title.
    /// </summary>
    public override string DisplayTitle => base.DisplayTitle;
    /// <summary>
    /// Gets the owning folder.
    /// </summary>
    [JsonIgnore]
    public Folder Folder => Parent as Folder;
    /// <summary>
    /// Gets or sets the primary text.
    /// </summary>
    public string Text { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the secondary text.
    /// </summary>
    public string Text2 { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the abstraction text.
    /// </summary>
    public string Abstraction { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the draft text.
    /// </summary>
    public string Draft { get; set; } = string.Empty;
}
