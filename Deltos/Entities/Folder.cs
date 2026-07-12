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

    // ● protected
    /// <summary>
    /// Updates the item information before saving it.
    /// </summary>
    protected override void UpdateInfo()
    {
        base.UpdateInfo();

        Info.IsFolder = true;
        Info.LevelTitle = LevelTitle;
    }
    /// <summary>
    /// Applies the item information after loading it.
    /// </summary>
    protected override void ApplyInfoCore()
    {
        base.ApplyInfoCore();

        LevelTitle = Info.LevelTitle;
    }

    // ● public
    /// <summary>
    /// Adds a folder.
    /// </summary>
    /// <param name="Title">The folder title.</param>
    /// <param name="LevelTitle">The document level title.</param>
    /// <returns>The added folder.</returns>
    public Folder AddFolder(string Title, string LevelTitle)
    {
        AppHost.CheckValidFileName(Title);

        Folder Result = new Folder();
        Result.Title = Title;
        Result.LevelTitle = LevelTitle;
        Folders.Add(Result);
        Result.UpdateReferences(this);
        RenumberChildren();
        return Result;
    }
    /// <summary>
    /// Adds a text file.
    /// </summary>
    /// <param name="Title">The text file title.</param>
    /// <returns>The added text file.</returns>
    public TextFile AddTextFile(string Title)
    {
        AppHost.CheckValidFileName(Title);

        TextFile Result = new TextFile();
        Result.Title = Title;
        Files.Add(Result);
        Result.UpdateReferences(this);
        RenumberChildren();
        return Result;
    }
    /// <summary>
    /// Removes a folder.
    /// </summary>
    /// <param name="Folder">The folder to remove.</param>
    /// <returns>True if the folder is removed; otherwise false.</returns>
    public bool RemoveFolder(Folder Folder)
    {
        bool Result = Folders.Remove(Folder);
        if (Result)
            RenumberChildren();

        return Result;
    }
    /// <summary>
    /// Removes a text file.
    /// </summary>
    /// <param name="File">The text file to remove.</param>
    /// <returns>True if the text file is removed; otherwise false.</returns>
    public bool RemoveTextFile(TextFile File)
    {
        bool Result = Files.Remove(File);
        if (Result)
            RenumberChildren();

        return Result;
    }
    /// <summary>
    /// Deletes the folder from persistent storage.
    /// </summary>
    public override void Delete()
    {
        string ItemFolderPath = FolderPath;

        Document DocumentItem = Parent as Document;
        if (DocumentItem != null)
            DocumentItem.RemoveFolder(this);

        Folder FolderItem = Parent as Folder;
        if (FolderItem != null)
            FolderItem.RemoveFolder(this);

        DeleteStorage(ItemFolderPath);
    }
    /// <summary>
    /// Saves the folder to persistent storage.
    /// </summary>
    public override void Save()
    {
        RenumberChildren();
        base.Save();

        System.IO.Directory.CreateDirectory(FoldersFolderPath);
        System.IO.Directory.CreateDirectory(TextFilesFolderPath);

        foreach (Folder Folder in Folders)
            Folder.Save();

        foreach (TextFile File in Files)
            File.Save();
    }
    /// <summary>
    /// Loads the folder from persistent storage.
    /// </summary>
    public override void Load()
    {
        base.Load();

        Folders = LoadItems<Folder>(FoldersFolderPath);
        Files = LoadItems<TextFile>(TextFilesFolderPath);
        UpdateReferences(Parent);
    }
    /// <summary>
    /// Prepares persisted item information before saving the folder.
    /// </summary>
    public override void PrepareInfo()
    {
        base.PrepareInfo();

        foreach (Folder Folder in Folders)
            Folder.PrepareInfo();

        foreach (TextFile File in Files)
            File.PrepareInfo();
    }
    /// <summary>
    /// Applies persisted item information after loading the folder.
    /// </summary>
    public override void ApplyInfo()
    {
        base.ApplyInfo();

        foreach (Folder Folder in Folders)
            Folder.ApplyInfo();

        foreach (TextFile File in Files)
            File.ApplyInfo();
    }
    /// <summary>
    /// Renumbers folder child items.
    /// </summary>
    public override void RenumberChildren()
    {
        RenumberItems(Folders);
        RenumberItems(Files);

        foreach (Folder Folder in Folders)
            Folder.RenumberChildren();
    }
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
    /// Gets the file-system folder path of the folder.
    /// </summary>
    [JsonIgnore]
    public override string FolderPath
    {
        get
        {
            Document DocumentItem = Parent as Document;
            if (DocumentItem != null)
                return System.IO.Path.Combine(DocumentItem.FoldersFolderPath, StorageName);

            Folder FolderItem = Parent as Folder;
            if (FolderItem != null)
                return System.IO.Path.Combine(FolderItem.FoldersFolderPath, StorageName);

            return base.FolderPath;
        }
    }
    /// <summary>
    /// Gets the folders bucket folder name.
    /// </summary>
    static public string FoldersFolderName => "Folders";
    /// <summary>
    /// Gets the text files bucket folder name.
    /// </summary>
    static public string TextFilesFolderName => "TextFiles";
    /// <summary>
    /// Gets the file-system folder path of the child folders bucket.
    /// </summary>
    [JsonIgnore]
    public string FoldersFolderPath => System.IO.Path.Combine(FolderPath, FoldersFolderName);
    /// <summary>
    /// Gets the file-system folder path of the child text files bucket.
    /// </summary>
    [JsonIgnore]
    public string TextFilesFolderPath => System.IO.Path.Combine(FolderPath, TextFilesFolderName);
    /// <summary>
    /// Gets or sets the document level title, such as Part, Chapter, or Section.
    /// </summary>
    public string LevelTitle { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the child folders.
    /// </summary>
    public List<Folder> Folders { get; set; } = new();
    /// <summary>
    /// Gets or sets the child text files.
    /// </summary>
    public List<TextFile> Files { get; set; } = new();
}
