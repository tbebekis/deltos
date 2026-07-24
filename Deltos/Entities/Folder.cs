// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents a document folder item.
/// </summary>
public class Folder: BaseItem
{
    // ● protected
    /// <summary>
    /// Field for the Synopsis property.
    /// </summary>
    protected string fSynopsis = string.Empty;
    /// <summary>
    /// Field for the LevelTitle property.
    /// </summary>
    protected string fLevelTitle = string.Empty;
    /// <summary>
    /// Field for the Items property.
    /// </summary>
    protected List<BaseItem> fItems = new();
    /// <summary>
    /// Field for the Folders property.
    /// </summary>
    protected List<Folder> fFolders = new();
    /// <summary>
    /// Field for the Files property.
    /// </summary>
    protected List<TextFile> fFiles = new();

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

        CheckLevelTitle();
        Info.IsFolder = true;
        Info.LevelTitle = LevelTitle;
    }
    /// <summary>
    /// Checks whether the folder level title is valid.
    /// </summary>
    protected virtual void CheckLevelTitle()
    {
        if (string.IsNullOrWhiteSpace(LevelTitle))
            return;

        AppHost.CheckValidFolderLevelTitle(LevelTitle);
    }
    /// <summary>
    /// Applies the item information after loading it.
    /// </summary>
    protected override void ApplyInfoCore()
    {
        base.ApplyInfoCore();

        LevelTitle = Info.LevelTitle;
    }
    /// <summary>
    /// Checks whether the folder can be renamed.
    /// </summary>
    /// <param name="NewTitle">The new title.</param>
    protected override void CheckRenameTitle(string NewTitle)
    {
        Document DocumentItem = Parent as Document;
        if (DocumentItem != null)
            CheckDuplicateTitle(DocumentItem.Items, NewTitle, this);

        Folder FolderItem = Parent as Folder;
        if (FolderItem != null)
            CheckDuplicateTitle(FolderItem.Items, NewTitle, this);
    }
    /// <summary>
    /// Checks whether the folder content model is valid.
    /// </summary>
    protected virtual void CheckContentModel()
    {
        foreach (BaseItem Item in Items)
        {
            if (!(Item is Folder) && !(Item is TextFile))
                throw new InvalidOperationException($"Invalid folder child item type: {Item.Type}.");
        }
    }
    /// <summary>
    /// Returns the source folders list.
    /// </summary>
    /// <returns>The source folders list.</returns>
    protected virtual List<BaseItem> GetSourceItems()
    {
        Document DocumentItem = Parent as Document;
        if (DocumentItem != null)
            return DocumentItem.Items;

        Folder FolderItem = Parent as Folder;
        if (FolderItem != null)
            return FolderItem.Items;

        return null;
    }
    /// <summary>
    /// Returns the target folders list.
    /// </summary>
    /// <param name="TargetParent">The target parent item.</param>
    /// <returns>The target folders list.</returns>
    protected virtual List<BaseItem> GetTargetItems(BaseItem TargetParent)
    {
        Document DocumentItem = TargetParent as Document;
        if (DocumentItem != null)
            return DocumentItem.Items;

        Folder FolderItem = TargetParent as Folder;
        if (FolderItem != null)
            return FolderItem.Items;

        return null;
    }
    /// <summary>
    /// Returns a document structure item at a specified level.
    /// </summary>
    /// <param name="DocumentItem">The document.</param>
    /// <param name="LevelIndex">The zero-based level index.</param>
    /// <returns>The structure item, if found; otherwise null.</returns>
    protected virtual FolderItem GetStructureItem(Document DocumentItem, int LevelIndex)
    {
        FolderItem Result = DocumentItem?.Structure;
        for (int Index = 0; Result != null && Index < LevelIndex; Index++)
            Result = Result.Child;

        return Result;
    }
    /// <summary>
    /// Returns true if this folder subtree can match a target document structure.
    /// </summary>
    /// <param name="TargetDocument">The target document.</param>
    /// <param name="TargetLevel">The target folder level.</param>
    /// <returns>True if this folder subtree can match the target document structure; otherwise false.</returns>
    protected virtual bool MatchesTargetStructure(Document TargetDocument, int TargetLevel)
    {
        FolderItem TargetStructureItem = GetStructureItem(TargetDocument, TargetLevel);
        if (TargetStructureItem == null)
            return false;

        if (!string.Equals(TargetStructureItem.Title, LevelTitle, StringComparison.OrdinalIgnoreCase))
            return false;

        if (Files.Count > 0 && !TargetStructureItem.IsLeaf)
            return false;

        if (Folders.Count > 0 && TargetStructureItem.IsLeaf)
            return false;

        foreach (Folder Folder in Folders)
        {
            if (!Folder.MatchesTargetStructure(TargetDocument, TargetLevel + 1))
                return false;
        }

        return true;
    }
    /// <summary>
    /// Returns true if this folder can change to a document parent.
    /// </summary>
    /// <param name="TargetDocument">The target document.</param>
    /// <returns>True if this folder can change to the target document parent; otherwise false.</returns>
    protected virtual bool CanChangeToDocument(Document TargetDocument)
    {
        if (TargetDocument == null || ReferenceEquals(Parent, TargetDocument))
            return false;

        if (Project == null || !ReferenceEquals(Project, TargetDocument.Project))
            return false;

        if (!TargetDocument.CanAddFolder)
            return false;

        if (ContainsTitle(TargetDocument.Items, Title))
            return false;

        return true;
    }
    /// <summary>
    /// Returns true if this folder can change to a folder parent.
    /// </summary>
    /// <param name="TargetFolder">The target folder.</param>
    /// <returns>True if this folder can change to the target folder parent; otherwise false.</returns>
    protected virtual bool CanChangeToFolder(Folder TargetFolder)
    {
        if (TargetFolder == null || ReferenceEquals(Parent, TargetFolder))
            return false;

        if (Project == null || !ReferenceEquals(Project, TargetFolder.Project))
            return false;

        if (ReferenceEquals(TargetFolder, this) || ContainsFolder(TargetFolder))
            return false;

        if (!TargetFolder.CanAddFolder)
            return false;

        if (ContainsTitle(TargetFolder.Items, Title))
            return false;

        return true;
    }
    /// <summary>
    /// Returns the default child folder level title.
    /// </summary>
    /// <returns>The default child folder level title.</returns>
    protected virtual string GetDefaultChildFolderLevelTitle()
    {
        string Result = StructureItem?.Child?.Title;
        return string.IsNullOrWhiteSpace(Result) ? DefaultLevelTitle : Result;
    }

    // ● internal
    /// <summary>
    /// Detaches a folder from memory without deleting persistent storage.
    /// </summary>
    /// <param name="Folder">The folder to remove.</param>
    /// <returns>True if the folder is removed; otherwise false.</returns>
    internal bool DetachFolder(Folder Folder)
    {
        bool Result = Items.Remove(Folder);
        if (Result)
        {
            Folder.ClearReferences();
            RenumberChildren();
            UpdateReferences(Parent);
        }

        return Result;
    }
    /// <summary>
    /// Detaches a text file from memory without deleting persistent storage.
    /// </summary>
    /// <param name="File">The text file to remove.</param>
    /// <returns>True if the text file is removed; otherwise false.</returns>
    internal bool DetachTextFile(TextFile File)
    {
        bool Result = Items.Remove(File);
        if (Result)
        {
            File.ClearReferences();
            RenumberChildren();
            UpdateReferences(Parent);
        }

        return Result;
    }

    // ● public
    /// <summary>
    /// Adds a folder.
    /// </summary>
    /// <param name="Title">The folder title.</param>
    /// <param name="LevelTitle">The document level title.</param>
    /// <returns>The added folder.</returns>
    public override Folder AddFolder(string Title, string LevelTitle)
    {
        if (!CanAddFolder)
            throw new InvalidOperationException("This folder cannot contain child folders.");

        CheckCanAddItem(Items);
        CheckDuplicateTitle(Items, Title);

        Folder Result = new Folder();
        Result.Title = Title;
        Result.LevelTitle = string.IsNullOrWhiteSpace(LevelTitle) ? GetDefaultChildFolderLevelTitle() : LevelTitle;
        Items.Add(Result);
        Result.UpdateReferences(this);
        RenumberChildren();
        SaveItemIfStorageReady(Result);
        return Result;
    }
    /// <summary>
    /// Adds a text file.
    /// </summary>
    /// <param name="Title">The text file title.</param>
    /// <returns>The added text file.</returns>
    public override TextFile AddTextFile(string Title)
    {
        if (!CanAddTextFile)
            throw new InvalidOperationException("This folder cannot contain text files.");

        CheckCanAddItem(Items);
        CheckDuplicateTitle(Items, Title);

        TextFile Result = new TextFile();
        Result.Title = Title;
        Items.Add(Result);
        Result.UpdateReferences(this);
        RenumberChildren();
        SaveItemIfStorageReady(Result);
        return Result;
    }
    /// <summary>
    /// Removes a folder from memory and persistent storage.
    /// </summary>
    /// <param name="Folder">The folder to remove.</param>
    /// <returns>True if the folder is removed; otherwise false.</returns>
    public bool RemoveFolder(Folder Folder)
    {
        return RemoveChild(Folder);
    }
    /// <summary>
    /// Removes a text file from memory and persistent storage.
    /// </summary>
    /// <param name="File">The text file to remove.</param>
    /// <returns>True if the text file is removed; otherwise false.</returns>
    public bool RemoveTextFile(TextFile File)
    {
        return RemoveChild(File);
    }
    /// <summary>
    /// Deletes a child item from memory and persistent storage.
    /// </summary>
    /// <param name="Item">The child item to delete.</param>
    /// <returns>True if the child item is deleted; otherwise false.</returns>
    public override bool RemoveChild(BaseItem Item)
    {
        Folder Folder = Item as Folder;
        if (Folder != null)
        {
            if (!Items.Contains(Folder))
                return false;

            Folder.Delete();
            return true;
        }

        TextFile File = Item as TextFile;
        if (File == null || !Items.Contains(File))
            return false;

        File.Delete();
        return true;
    }
    /// <summary>
    /// Moves a folder inside the child folders list.
    /// </summary>
    /// <param name="Folder">The folder to move.</param>
    /// <param name="NewOrderIndex">The new one-based order index.</param>
    /// <returns>True if the folder is moved; otherwise false.</returns>
    public bool MoveFolder(Folder Folder, int NewOrderIndex)
    {
        if (!CanContainFolders)
            throw new InvalidOperationException("This folder cannot contain child folders.");

        bool Result = MoveItem(Items, Folder, NewOrderIndex);
        if (Result)
            UpdateReferences(Parent);

        return Result;
    }
    /// <summary>
    /// Moves a text file inside the child text files list.
    /// </summary>
    /// <param name="File">The text file to move.</param>
    /// <param name="NewOrderIndex">The new one-based order index.</param>
    /// <returns>True if the text file is moved; otherwise false.</returns>
    public bool MoveTextFile(TextFile File, int NewOrderIndex)
    {
        if (!CanContainTextFiles)
            throw new InvalidOperationException("This folder cannot contain text files.");

        bool Result = MoveItem(Items, File, NewOrderIndex);
        if (Result)
            UpdateReferences(Parent);

        return Result;
    }
    /// <summary>
    /// Returns true if this folder contains another folder.
    /// </summary>
    /// <param name="Folder">The folder to check.</param>
    /// <returns>True if the folder is contained; otherwise false.</returns>
    public bool ContainsFolder(Folder Folder)
    {
        foreach (Folder ChildFolder in Folders)
        {
            if (ReferenceEquals(ChildFolder, Folder) || ChildFolder.ContainsFolder(Folder))
                return true;
        }

        return false;
    }
    /// <summary>
    /// Returns the folder child items.
    /// </summary>
    /// <returns>The folder child items.</returns>
    public override List<BaseItem> GetChildItems()
    {
        List<BaseItem> Result = new();

        foreach (BaseItem Item in Items)
            Result.Add(Item);

        return Result;
    }
    /// <summary>
    /// Returns true if the folder can move in the specified direction.
    /// </summary>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <returns>True if the folder can move; otherwise false.</returns>
    public override bool CanMove(bool Up)
    {
        Document DocumentItem = Parent as Document;
        if (DocumentItem != null)
            return CanMoveItem(DocumentItem.Items, this, Up);

        Folder FolderItem = Parent as Folder;
        if (FolderItem != null)
            return CanMoveItem(FolderItem.Items, this, Up);

        return false;
    }
    /// <summary>
    /// Moves the folder one step in the specified direction.
    /// </summary>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <returns>True if the folder is moved; otherwise false.</returns>
    public override bool Move(bool Up)
    {
        if (!CanMove(Up))
            return false;

        Document DocumentItem = Parent as Document;
        if (DocumentItem != null)
        {
            bool Result = MoveItem(DocumentItem.Items, this, Up);
            if (Result)
                DocumentItem.UpdateReferences(DocumentItem.Parent);

            return Result;
        }

        Folder FolderItem = Parent as Folder;
        if (FolderItem != null)
        {
            bool Result = MoveItem(FolderItem.Items, this, Up);
            if (Result)
                FolderItem.UpdateReferences(FolderItem.Parent);

            return Result;
        }

        return false;
    }
    /// <summary>
    /// Returns true if the folder can change parent.
    /// </summary>
    /// <param name="TargetParent">The target parent item.</param>
    /// <returns>True if the folder can change parent; otherwise false.</returns>
    public override bool CanChangeParent(BaseItem TargetParent)
    {
        Document TargetDocument = TargetParent as Document;
        if (TargetDocument != null)
            return CanChangeToDocument(TargetDocument);

        Folder TargetFolder = TargetParent as Folder;
        return CanChangeToFolder(TargetFolder);
    }
    /// <summary>
    /// Changes the folder parent.
    /// </summary>
    /// <param name="TargetParent">The target parent item.</param>
    /// <returns>True if the folder parent is changed; otherwise false.</returns>
    public override bool ChangeParent(BaseItem TargetParent)
    {
        if (!CanChangeParent(TargetParent))
            return false;

        BaseItem SourceParent = Parent;
        List<BaseItem> SourceItems = GetSourceItems();
        List<BaseItem> TargetItems = GetTargetItems(TargetParent);
        bool Result = MoveItem(SourceItems, TargetItems, this, TargetParent);
        if (Result)
        {
            SourceParent.UpdateReferences(SourceParent.Parent);
            TargetParent.UpdateReferences(TargetParent.Parent);
        }

        return Result;
    }
    /// <summary>
    /// Deletes the folder from persistent storage.
    /// </summary>
    public override void Delete()
    {
        if (!CanDelete())
            throw new InvalidOperationException("This folder cannot be deleted.");

        string ItemFolderPath = FolderPath;
        DeleteStorage(ItemFolderPath);

        Document DocumentItem = Parent as Document;
        if (DocumentItem != null)
            DocumentItem.DetachFolder(this);

        Folder FolderItem = Parent as Folder;
        if (FolderItem != null)
            FolderItem.DetachFolder(this);
    }
    /// <summary>
    /// Saves the folder to persistent storage.
    /// </summary>
    public override void Save()
    {
        CheckContentModel();
        RenumberChildren();
        UpdateReferences(Parent);
        base.Save();
        SaveMarkdownFile(SynopsisFilePath, Synopsis);

        System.IO.Directory.CreateDirectory(ItemsFolderPath);
        foreach (BaseItem Item in Items)
            Item.Save();

        DeleteInternalMoveFolders(ItemsFolderPath);
        DeleteInternalMoveFolders(FolderPath);
        DeleteStorage(FoldersFolderPath);
        DeleteStorage(TextFilesFolderPath);
    }
    /// <summary>
    /// Loads the folder from persistent storage.
    /// </summary>
    public override void Load()
    {
        base.Load();
        Synopsis = LoadMarkdownFile(SynopsisFilePath);

        Items = new List<BaseItem>();
        if (System.IO.Directory.Exists(ItemsFolderPath))
        {
            Items = LoadChildItems(ItemsFolderPath);
            CheckUnusedStorageBucket(FoldersFolderPath);
            CheckUnusedStorageBucket(TextFilesFolderPath);
        }
        else
        {
            Items.AddRange(LoadItems<Folder>(FoldersFolderPath));
            Items.AddRange(LoadItems<TextFile>(TextFilesFolderPath));
            Items.Sort((A, B) => A.OrderIndex.CompareTo(B.OrderIndex));
            CheckLoadedItems(Items, FolderPath);
        }

        UpdateReferences(Parent);
    }
    /// <summary>
    /// Prepares persisted item information before saving the folder.
    /// </summary>
    public override void PrepareInfo()
    {
        base.PrepareInfo();

        foreach (BaseItem Item in Items)
            Item.PrepareInfo();
    }
    /// <summary>
    /// Applies persisted item information after loading the folder.
    /// </summary>
    public override void ApplyInfo()
    {
        base.ApplyInfo();

        foreach (BaseItem Item in Items)
            Item.ApplyInfo();
    }
    /// <summary>
    /// Renumbers folder child items.
    /// </summary>
    public override void RenumberChildren()
    {
        CheckContentModel();

        RenumberItems(Items);

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

        foreach (BaseItem Item in Items)
            Item.UpdateReferences(this);
    }
    /// <summary>
    /// Clears runtime references when the folder is detached from its parent.
    /// </summary>
    public override void ClearReferences()
    {
        base.ClearReferences();

        foreach (BaseItem Item in Items)
            Item.ClearReferences();
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
    public override string DisplayTitle
    {
        get
        {
            string Result = base.DisplayTitle;
            if (Document?.Structure?.IsLeaf == true)
                return Result;
            if (AppHost.Settings?.ShowFolderLevelTitleInTree != true)
                return Result;

            return string.IsNullOrWhiteSpace(LevelTitle) ? Result : $"{LevelTitle} - {Result}";
        }
    }
    /// <summary>
    /// Gets the secondary folder display title.
    /// </summary>
    public override string DisplayTitle2
    {
        get
        {
            string Result = base.DisplayTitle2;
            if (Document?.Structure?.IsLeaf == true)
                return Result;
            if (AppHost.Settings?.ShowFolderLevelTitleInTree != true)
                return Result;

            return string.IsNullOrWhiteSpace(LevelTitle) ? Result : $"{LevelTitle} - {Result}";
        }
    }
    /// <summary>
    /// Gets the file-system folder path of the folder.
    /// </summary>
    [JsonIgnore]
    public override string FolderPath
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(fStorageFolderPathOverride))
                return fStorageFolderPathOverride;

            Document DocumentItem = Parent as Document;
            if (DocumentItem != null)
                return System.IO.Path.Combine(DocumentItem.ItemsFolderPath, StorageName);

            Folder FolderItem = Parent as Folder;
            if (FolderItem != null)
                return System.IO.Path.Combine(FolderItem.ItemsFolderPath, StorageName);

            return base.FolderPath;
        }
    }
    /// <summary>
    /// Gets the default folder level title.
    /// </summary>
    static public string DefaultLevelTitle => "Folder";
    /// <summary>
    /// Gets the mixed child items bucket folder name.
    /// </summary>
    static public string ItemsFolderName => "Items";
    /// <summary>
    /// Gets the folders bucket folder name.
    /// </summary>
    static public string FoldersFolderName => "Folders";
    /// <summary>
    /// Gets the text files bucket folder name.
    /// </summary>
    static public string TextFilesFolderName => "TextFiles";
    /// <summary>
    /// Gets the synopsis text file name.
    /// </summary>
    static public string SynopsisFileName => "Synopsis.md";
    /// <summary>
    /// Gets the file-system folder path of the child items bucket.
    /// </summary>
    [JsonIgnore]
    public string ItemsFolderPath => System.IO.Path.Combine(FolderPath, ItemsFolderName);
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
    /// Gets the file-system path of the folder synopsis file.
    /// </summary>
    [JsonIgnore]
    public string SynopsisFilePath => System.IO.Path.Combine(FolderPath, SynopsisFileName);
    /// <summary>
    /// Gets or sets the document level title, such as Part, Chapter, or Section.
    /// </summary>
    public string LevelTitle
    {
        get => fLevelTitle;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                fLevelTitle = string.Empty;
                return;
            }

            AppHost.CheckValidFolderLevelTitle(value);
            fLevelTitle = value.Trim();
        }
    }
    /// <summary>
    /// Gets the zero-based folder level inside the document.
    /// </summary>
    [JsonIgnore]
    public int Level
    {
        get
        {
            Folder FolderItem = Parent as Folder;
            if (FolderItem != null)
                return FolderItem.Level + 1;

            return Parent is Document ? 0 : -1;
        }
    }
    /// <summary>
    /// Gets the matching document structure item.
    /// </summary>
    [JsonIgnore]
    public FolderItem StructureItem
    {
        get
        {
            FolderItem Result = Document == null ? null : Document.Structure;
            for (int Index = 0; Result != null && Index < Level; Index++)
                Result = Result.Child;

            return Result;
        }
    }
    /// <summary>
    /// Gets a value indicating whether this folder is at the document leaf level.
    /// </summary>
    [JsonIgnore]
    public bool IsLeafLevel => StructureItem != null && StructureItem.IsLeaf;
    /// <summary>
    /// Gets a value indicating whether this folder can contain child folders.
    /// </summary>
    [JsonIgnore]
    public override bool CanContainFolders => true;
    /// <summary>
    /// Gets a value indicating whether this folder can contain text files.
    /// </summary>
    [JsonIgnore]
    public override bool CanContainTextFiles => true;
    /// <summary>
    /// Gets a value indicating whether a child folder can be added.
    /// </summary>
    [JsonIgnore]
    public override bool CanAddFolder => CanContainFolders && CanAddItem(Items);
    /// <summary>
    /// Gets a value indicating whether a text file can be added.
    /// </summary>
    [JsonIgnore]
    public override bool CanAddTextFile => CanContainTextFiles && CanAddItem(Items);
    /// <summary>
    /// Gets or sets the child items.
    /// </summary>
    public List<BaseItem> Items
    {
        get => fItems;
        set => fItems = value ?? new List<BaseItem>();
    }
    /// <summary>
    /// Gets or sets the child folders.
    /// </summary>
    public List<Folder> Folders
    {
        get => Items.OfType<Folder>().ToList();
        set
        {
            Items.RemoveAll(Item => Item is Folder);
            if (value != null)
                Items.AddRange(value);
        }
    }
    /// <summary>
    /// Gets or sets the child text files.
    /// </summary>
    public List<TextFile> Files
    {
        get => Items.OfType<TextFile>().ToList();
        set
        {
            Items.RemoveAll(Item => Item is TextFile);
            if (value != null)
                Items.AddRange(value);
        }
    }
    /// <summary>
    /// Gets or sets the folder synopsis.
    /// </summary>
    public string Synopsis
    {
        get => fSynopsis;
        set => fSynopsis = value ?? string.Empty;
    }
}
