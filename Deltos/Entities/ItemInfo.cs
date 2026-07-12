// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents the persisted information stored in the Info.json file of a BaseItem.
/// </summary>
public class ItemInfo
{
    // ● protected
    /// <summary>
    /// Field for the Id property.
    /// </summary>
    protected string fId = string.Empty;
    /// <summary>
    /// Field for the Title property.
    /// </summary>
    protected string fTitle = string.Empty;
    /// <summary>
    /// Field for the Category property.
    /// </summary>
    protected string fCategory = string.Empty;
    /// <summary>
    /// Field for the TagList property.
    /// </summary>
    protected string fTagList = string.Empty;
    /// <summary>
    /// Field for the LevelTitle property.
    /// </summary>
    protected string fLevelTitle = string.Empty;

    // ● construction
    /// <summary>
    /// Initializes a new instance of the ItemInfo class.
    /// </summary>
    public ItemInfo()
    {
    }

    // ● properties
    /// <summary>
    /// Gets or sets the identifier of the owning item.
    /// </summary>
    public string Id
    {
        get => fId;
        set => fId = value ?? string.Empty;
    }
    /// <summary>
    /// Gets or sets the title of the owning project item.
    /// </summary>
    public string Title
    {
        get => fTitle;
        set => fTitle = value ?? string.Empty;
    }
    /// <summary>
    /// Gets or sets the type of the owning item.
    /// </summary>
    public ItemType Type { get; set; }
    /// <summary>
    /// Gets or sets the category of a component item.
    /// </summary>
    public string Category
    {
        get => fCategory;
        set => fCategory = value ?? string.Empty;
    }
    /// <summary>
    /// Gets or sets the semicolon-separated tag list of a component item.
    /// </summary>
    public string TagList
    {
        get => fTagList;
        set => fTagList = value ?? string.Empty;
    }
    /// <summary>
    /// Gets or sets a value indicating whether the item represents a folder.
    /// </summary>
    public bool IsFolder { get; set; }
    /// <summary>
    /// Gets or sets the document level title, such as Part, Chapter, or Section.
    /// </summary>
    public string LevelTitle
    {
        get => fLevelTitle;
        set => fLevelTitle = value ?? string.Empty;
    }
}
