// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Base class for all project entities.
/// </summary>
public class BaseItem
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
    /// Field for the DisplayTitle property.
    /// </summary>
    protected string fDisplayTitle = string.Empty;
    /// <summary>
    /// Field for the OrderIndex property.
    /// </summary>
    protected int fOrderIndex;
    
    // ● construction
    /// <summary>
    /// Initializes a new instance of the BaseItem class.
    /// </summary>
    public BaseItem()
    {
    }
    
    // ● static public
    /// <summary>
    /// Converts an item title to a file-system title segment.
    /// </summary>
    /// <param name="Title">The item title.</param>
    /// <returns>The file-system title segment.</returns>
    static public string EncodeTitle(string Title)
    {
        AppHost.CheckValidFileName(Title);
        return Title.Trim().Replace(' ', '_');
    }
    /// <summary>
    /// Converts a file-system title segment to an item title.
    /// </summary>
    /// <param name="Title">The file-system title segment.</param>
    /// <returns>The item title.</returns>
    static public string DecodeTitle(string Title)
    {
        return Title.Replace('_', ' ');
    }
    /// <summary>
    /// Returns the storage name for an ordered item title.
    /// </summary>
    /// <param name="OrderIndex">The order index.</param>
    /// <param name="Title">The item title.</param>
    /// <returns>The storage name.</returns>
    static public string GetStorageName(int OrderIndex, string Title)
    {
        if (OrderIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(OrderIndex));

        return $"{OrderIndex:000}._{EncodeTitle(Title)}";
    }
    /// <summary>
    /// Returns the display title for an ordered item title.
    /// </summary>
    /// <param name="OrderIndex">The order index.</param>
    /// <param name="Title">The item title.</param>
    /// <returns>The display title.</returns>
    static public string GetDisplayTitle(int OrderIndex, string Title)
    {
        return DecodeTitle(GetStorageName(OrderIndex, Title));
    }
    /// <summary>
    /// Tries to parse a storage name.
    /// </summary>
    /// <param name="StorageName">The storage name.</param>
    /// <param name="OrderIndex">The parsed order index.</param>
    /// <param name="Title">The parsed title.</param>
    /// <param name="DisplayTitle">The parsed display title.</param>
    /// <returns>True if the storage name is parsed successfully; otherwise false.</returns>
    static public bool TryParseStorageName(string StorageName, out int OrderIndex, out string Title, out string DisplayTitle)
    {
        OrderIndex = 0;
        Title = string.Empty;
        DisplayTitle = string.Empty;

        if (string.IsNullOrWhiteSpace(StorageName) || StorageName.Length < 5)
            return false;

        if (StorageName[3] != '.')
            return false;

        if (!int.TryParse(StorageName.Substring(0, 3), out OrderIndex))
            return false;

        string EncodedTitle = StorageName.Substring(4);
        if (EncodedTitle.StartsWith("_"))
            EncodedTitle = EncodedTitle.Substring(1);

        Title = DecodeTitle(EncodedTitle);
        DisplayTitle = DecodeTitle(StorageName);

        if (!AppHost.IsValidFileName(Title, false))
            return false;

        return !string.IsNullOrWhiteSpace(Title);
    }

    // ● public
    /// <summary>
    /// Sets the item title fields from a storage name.
    /// </summary>
    /// <param name="StorageName">The storage name.</param>
    public virtual void SetStorageName(string StorageName)
    {
        if (!TryParseStorageName(StorageName, out int OrderIndex, out string Title, out string DisplayTitle))
            throw new ArgumentException($"Invalid storage name: {StorageName}", nameof(StorageName));

        fOrderIndex = OrderIndex;
        fTitle = Title;
        fDisplayTitle = DisplayTitle;
    }
    /// <summary>
    /// Updates runtime references after loading the item graph.
    /// </summary>
    /// <param name="ParentItem">The parent item.</param>
    public virtual void UpdateReferences(BaseItem ParentItem)
    {
        Parent = ParentItem;
        Project = ParentItem == null ? Project : ParentItem.Project;
    }
    /// <summary>
    /// Saves the item to persistent storage.
    /// </summary>
    public virtual void Save()
    {
    }
    /// <summary>
    /// Loads the item from persistent storage.
    /// </summary>
    public virtual void Load()
    {
    }
    
    // ● properties
    /// <summary>
    /// Gets the parent item.
    /// </summary>
    [JsonIgnore]
    public virtual BaseItem Parent { get; protected set; }
    /// <summary>
    /// Gets the owning project.
    /// </summary>
    [JsonIgnore]
    public virtual Project Project { get; protected set; }
    /// <summary>
    /// Gets the owning document.
    /// </summary>
    [JsonIgnore]
    public virtual Document Document
    {
        get
        {
            Document Result = this as Document;
            if (Result != null)
                return Result;

            return Parent == null ? null : Parent.Document;
        }
    }
    /// <summary>
    /// Gets the item type.
    /// </summary>
    public virtual ItemType Type => ItemType.None;
    /// <summary>
    /// Gets a value indicating whether this item is a project.
    /// </summary>
    [JsonIgnore]
    public bool IsProject => Type == ItemType.Project;
    /// <summary>
    /// Gets a value indicating whether this item is a document.
    /// </summary>
    [JsonIgnore]
    public bool IsDocument => Type == ItemType.Document;
    /// <summary>
    /// Gets a value indicating whether this item is a folder.
    /// </summary>
    [JsonIgnore]
    public bool IsFolder => Type == ItemType.Folder;
    /// <summary>
    /// Gets a value indicating whether this item is a text file.
    /// </summary>
    [JsonIgnore]
    public bool IsTextFile => Type == ItemType.TextFile;
    /// <summary>
    /// Gets or sets the unique item identifier.
    /// </summary>
    public string Id
    {
        get
        {
            if (string.IsNullOrWhiteSpace(fId))
                fId = Sys.GenId(UseBrackets: false);
            return fId;
        }
        set => fId = value;
    }
    /// <summary>
    /// Gets or sets the item title.
    /// </summary>
    public virtual string Title
    {
        get => fTitle;
        set => fTitle = value;
    }
    /// <summary>
    /// Gets the display title of the item.
    /// </summary>
    public virtual string DisplayTitle => fDisplayTitle;
    /// <summary>
    /// Gets the item order index among its siblings.
    /// </summary>
    public virtual int OrderIndex => fOrderIndex;
    /// <summary>
    /// Gets or sets the persisted item information.
    /// </summary>
    public virtual ItemInfo Info { get; set; } = new();
    /// <summary>
    /// Gets the item information file name.
    /// </summary>
    static public string InfoFileName => "Info.json";
}
