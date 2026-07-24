// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents a document inside a project.
/// </summary>
public class Document: BaseItem
{
    /// <summary>
    /// Describes an item position in document tree order.
    /// </summary>
    class MoveEntry
    {
        // ● properties
        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        public BaseItem Item { get; set; }
        /// <summary>
        /// Gets or sets the parent item.
        /// </summary>
        public BaseItem Parent { get; set; }
        /// <summary>
        /// Gets or sets the parent item list.
        /// </summary>
        public List<BaseItem> Items { get; set; }
        /// <summary>
        /// Gets or sets the item index in its parent list.
        /// </summary>
        public int Index { get; set; }
    }

    // ● protected
    /// <summary>
    /// Field for the Synopsis property.
    /// </summary>
    protected string fSynopsis = string.Empty;
    /// <summary>
    /// Field for the Structure property.
    /// </summary>
    protected FolderItem fStructure = new();
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

    // ● private
    /// <summary>
    /// Adds folders recursively to a container list.
    /// </summary>
    /// <param name="Containers">The container list.</param>
    /// <param name="Folders">The folders to add.</param>
    static void AddMoveContainers(List<BaseItem> Containers, List<Folder> Folders)
    {
        foreach (Folder Folder in Folders)
        {
            Containers.Add(Folder);
            AddMoveContainers(Containers, Folder.Folders);
        }
    }
    /// <summary>
    /// Adds folders of a specified level recursively to a container list.
    /// </summary>
    /// <param name="Containers">The container list.</param>
    /// <param name="Folders">The folders to check.</param>
    /// <param name="Level">The folder level.</param>
    static void AddMoveContainers(List<BaseItem> Containers, List<Folder> Folders, int Level)
    {
        foreach (Folder Folder in Folders)
        {
            if (Folder.Level == Level)
                Containers.Add(Folder);

            AddMoveContainers(Containers, Folder.Folders, Level);
        }
    }
    /// <summary>
    /// Adds move entries recursively in visible document tree order.
    /// </summary>
    /// <param name="Result">The move entries.</param>
    /// <param name="ParentItem">The parent item.</param>
    /// <param name="Items">The parent item list.</param>
    static void AddMoveEntries(List<MoveEntry> Result, BaseItem ParentItem, List<BaseItem> Items)
    {
        for (int Index = 0; Index < Items.Count; Index++)
        {
            BaseItem Item = Items[Index];
            Result.Add(new MoveEntry { Item = Item, Parent = ParentItem, Items = Items, Index = Index });

            Folder Folder = Item as Folder;
            if (Folder != null)
                AddMoveEntries(Result, Folder, Folder.Items);
        }
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the Document class.
    /// </summary>
    public Document()
    {
    }

    // ● protected
    /// <summary>
    /// Saves the document structure file.
    /// </summary>
    protected virtual void SaveStructure()
    {
        Json.SaveToFile(Structure, StructureFilePath);
    }
    /// <summary>
    /// Loads the document structure file.
    /// </summary>
    protected virtual void LoadStructure()
    {
        fStructure = new FolderItem();
        if (!System.IO.File.Exists(StructureFilePath))
            return;

        Json.LoadFromFile(Structure, StructureFilePath);
        Structure?.UpdateReferences(null);
        Structure?.CheckValid();
    }
    /// <summary>
    /// Deletes the document structure file.
    /// </summary>
    protected virtual void DeleteStructure()
    {
        if (System.IO.File.Exists(StructureFilePath))
            System.IO.File.Delete(StructureFilePath);
    }
    /// <summary>
    /// Checks whether the document can be renamed.
    /// </summary>
    /// <param name="NewTitle">The new title.</param>
    protected override void CheckRenameTitle(string NewTitle)
    {
        Project ProjectItem = Parent as Project;
        if (ProjectItem != null)
            CheckDuplicateTitle(ProjectItem.Documents, NewTitle, this);
    }
    /// <summary>
    /// Checks whether the document content model is valid.
    /// </summary>
    protected virtual void CheckContentModel()
    {
        foreach (BaseItem Item in Items)
        {
            if (!(Item is Folder) && !(Item is TextFile))
                throw new InvalidOperationException($"Invalid document child item type: {Item.Type}.");
        }
    }
    /// <summary>
    /// Returns the default child folder level title.
    /// </summary>
    /// <returns>The default child folder level title.</returns>
    protected virtual string GetDefaultFolderLevelTitle()
    {
        return string.IsNullOrWhiteSpace(Structure?.Title) ? Folder.DefaultLevelTitle : Structure.Title;
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
            throw new InvalidOperationException("The document has no folder structure.");

        CheckCanAddItem(Items);
        CheckDuplicateTitle(Items, Title);

        Folder Result = new Folder();
        Result.Title = Title;
        Result.LevelTitle = string.IsNullOrWhiteSpace(LevelTitle) ? GetDefaultFolderLevelTitle() : LevelTitle;
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
            throw new InvalidOperationException("This document cannot contain text files.");

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
    /// Sets the document folder structure.
    /// </summary>
    /// <param name="Value">The folder structure.</param>
    public override void SetStructure(FolderItem Value)
    {
        if (!CanSetStructure)
            throw new InvalidOperationException("Cannot set a folder structure while the document contains child items.");

        if (Value == null)
            throw new ArgumentNullException(nameof(Value));

        Value.CheckValid();
        fStructure = Value.Clone();
        Structure.UpdateReferences(null);
        CheckContentModel();
        if (CanPersistStorage())
            Save();
    }
    /// <summary>
    /// Clears the document folder structure and turns the document into a flat document.
    /// </summary>
    public override void ClearStructure()
    {
        if (!CanClearStructure)
            throw new InvalidOperationException("Cannot clear the folder structure while the document contains folders.");

        fStructure = new FolderItem();
        CheckContentModel();
        if (CanPersistStorage())
            Save();
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
        if (File != null)
        {
            if (!Items.Contains(File))
                return false;

            File.Delete();
            return true;
        }

        return false;
    }
    /// <summary>
    /// Moves a folder inside the document folders list.
    /// </summary>
    /// <param name="Folder">The folder to move.</param>
    /// <param name="NewOrderIndex">The new one-based order index.</param>
    /// <returns>True if the folder is moved; otherwise false.</returns>
    public bool MoveFolder(Folder Folder, int NewOrderIndex)
    {
        if (!CanContainFolders)
            throw new InvalidOperationException("This document cannot contain folders.");

        bool Result = MoveItem(Items, Folder, NewOrderIndex);
        if (Result)
            UpdateReferences(Parent);

        return Result;
    }
    /// <summary>
    /// Moves a text file inside the document text files list.
    /// </summary>
    /// <param name="File">The text file to move.</param>
    /// <param name="NewOrderIndex">The new one-based order index.</param>
    /// <returns>True if the text file is moved; otherwise false.</returns>
    public bool MoveTextFile(TextFile File, int NewOrderIndex)
    {
        if (!CanContainTextFiles)
            throw new InvalidOperationException("This document cannot contain text files.");

        bool Result = MoveItem(Items, File, NewOrderIndex);
        if (Result)
            UpdateReferences(Parent);

        return Result;
    }
    /// <summary>
    /// Returns the document and its folders as possible item containers.
    /// </summary>
    /// <returns>The possible item containers.</returns>
    public List<BaseItem> GetMoveContainers()
    {
        List<BaseItem> Result = new();
        Result.Add(this);
        AddMoveContainers(Result, Folders);
        return Result;
    }
    /// <summary>
    /// Returns containers that may contain folders of a specified level.
    /// </summary>
    /// <param name="FolderLevel">The folder level.</param>
    /// <returns>The possible folder containers.</returns>
    public List<BaseItem> GetFolderMoveContainers(int FolderLevel)
    {
        List<BaseItem> Result = new();

        if (FolderLevel == 0)
        {
            Result.Add(this);
        }
        else
        {
            AddMoveContainers(Result, Folders, FolderLevel - 1);
        }

        return Result;
    }
    /// <summary>
    /// Returns containers that may contain text files.
    /// </summary>
    /// <returns>The possible text file containers.</returns>
    public List<BaseItem> GetTextFileMoveContainers()
    {
        List<BaseItem> Result = new();
        Result.AddRange(GetMoveContainers());

        return Result;
    }
    /// <summary>
    /// Returns true if a document item can move in visible tree order.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <returns>True if the item can move; otherwise false.</returns>
    public bool CanMoveDocumentItem(BaseItem Item, bool Up)
    {
        return GetDocumentMoveTarget(Item, Up, out _, out _, out _);
    }
    /// <summary>
    /// Moves a document item in visible tree order.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <returns>True if the item is moved; otherwise false.</returns>
    public bool MoveDocumentItem(BaseItem Item, bool Up)
    {
        if (!GetDocumentMoveTarget(Item, Up, out MoveEntry SourceEntry, out BaseItem TargetParent, out int TargetIndex))
            return false;

        List<BaseItem> TargetItems = GetContainerItems(TargetParent);
        bool Result = MoveItem(SourceEntry.Items, TargetItems, Item, TargetParent, TargetIndex);
        if (Result)
            UpdateReferences(Parent);

        return Result;
    }
    /// <summary>
    /// Returns the document move target for an item.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <param name="SourceEntry">The source entry.</param>
    /// <param name="TargetParent">The target parent.</param>
    /// <param name="TargetIndex">The target index.</param>
    /// <returns>True if a target is found; otherwise false.</returns>
    bool GetDocumentMoveTarget(BaseItem Item, bool Up, out MoveEntry SourceEntry, out BaseItem TargetParent, out int TargetIndex)
    {
        SourceEntry = null;
        TargetParent = null;
        TargetIndex = -1;
        if (Item == null || !ReferenceEquals(Item.Document, this))
            return false;

        List<MoveEntry> Entries = GetMoveEntries();
        int SourceIndex = Entries.FindIndex(Entry => ReferenceEquals(Entry.Item, Item));
        if (SourceIndex < 0)
            return false;

        SourceEntry = Entries[SourceIndex];
        MoveEntry TargetEntry = Up ? GetPreviousMoveEntry(Entries, SourceIndex) : GetNextMoveEntry(Entries, SourceIndex, Item);
        if (TargetEntry == null)
        {
            if (Up || !(SourceEntry.Parent is Folder))
                return false;

            MoveEntry ParentEntry = FindMoveEntry(Entries, SourceEntry.Parent);
            if (ParentEntry == null)
                return false;

            TargetParent = ParentEntry.Parent;
            TargetIndex = ParentEntry.Index + 1;
        }
        else if (Up && ReferenceEquals(TargetEntry.Item, SourceEntry.Parent))
        {
            MoveEntry ParentEntry = FindMoveEntry(Entries, SourceEntry.Parent);
            if (ParentEntry == null)
                return false;

            TargetParent = ParentEntry.Parent;
            TargetIndex = ParentEntry.Index;
        }
        else if (!Up && SourceEntry.Parent is Folder && !ReferenceEquals(TargetEntry.Parent, SourceEntry.Parent) && !IsDescendantOf(TargetEntry.Item, SourceEntry.Parent))
        {
            MoveEntry ParentEntry = FindMoveEntry(Entries, SourceEntry.Parent);
            if (ParentEntry == null)
                return false;

            TargetParent = ParentEntry.Parent;
            TargetIndex = ParentEntry.Index + 1;
        }
        else if (Up && GetAncestorChildUnderParent(TargetEntry.Item, SourceEntry.Parent) is Folder PreviousFolder && !ReferenceEquals(PreviousFolder, Item))
        {
            TargetParent = PreviousFolder;
            TargetIndex = PreviousFolder.Items.Count;
        }
        else if (!Up && TargetEntry.Item is Folder NextFolder && ReferenceEquals(TargetEntry.Parent, SourceEntry.Parent))
        {
            TargetParent = NextFolder;
            TargetIndex = 0;
        }
        else if (ReferenceEquals(SourceEntry.Parent, TargetEntry.Parent))
        {
            TargetParent = SourceEntry.Parent;
            TargetIndex = Up ? TargetEntry.Index : TargetEntry.Index + 1;
        }
        else if (TargetEntry.Item is Folder TargetFolder)
        {
            TargetParent = TargetFolder;
            TargetIndex = Up ? TargetFolder.Items.Count : 0;
        }
        else
        {
            TargetParent = TargetEntry.Parent;
            TargetIndex = Up ? TargetEntry.Index : TargetEntry.Index + 1;
        }

        Folder MovingFolder = Item as Folder;
        Folder ParentFolder = TargetParent as Folder;
        if (MovingFolder != null && ParentFolder != null && (ReferenceEquals(MovingFolder, ParentFolder) || MovingFolder.ContainsFolder(ParentFolder)))
            return false;

        List<BaseItem> TargetItems = GetContainerItems(TargetParent);
        return TargetItems != null && (SourceEntry.Items == TargetItems || CanAddItem(TargetItems)) && (SourceEntry.Items == TargetItems || !ContainsTitle(TargetItems, Item.Title));
    }
    /// <summary>
    /// Finds a move entry by item.
    /// </summary>
    /// <param name="Entries">The move entries.</param>
    /// <param name="Item">The item.</param>
    /// <returns>The move entry, if any; otherwise null.</returns>
    MoveEntry FindMoveEntry(List<MoveEntry> Entries, BaseItem Item)
    {
        return Entries.FirstOrDefault(Entry => ReferenceEquals(Entry.Item, Item));
    }
    /// <summary>
    /// Returns the ancestor item directly under a parent item.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <param name="ParentItem">The parent item.</param>
    /// <returns>The direct child ancestor, if found; otherwise null.</returns>
    BaseItem GetAncestorChildUnderParent(BaseItem Item, BaseItem ParentItem)
    {
        BaseItem Result = Item;
        while (Result != null && Result.Parent != null && !ReferenceEquals(Result.Parent, ParentItem))
            Result = Result.Parent;

        return Result != null && ReferenceEquals(Result.Parent, ParentItem) ? Result : null;
    }
    /// <summary>
    /// Returns the previous move entry.
    /// </summary>
    /// <param name="Entries">The move entries.</param>
    /// <param name="SourceIndex">The source index.</param>
    /// <returns>The previous move entry, if any; otherwise null.</returns>
    MoveEntry GetPreviousMoveEntry(List<MoveEntry> Entries, int SourceIndex)
    {
        return SourceIndex > 0 ? Entries[SourceIndex - 1] : null;
    }
    /// <summary>
    /// Returns the next move entry after the item subtree.
    /// </summary>
    /// <param name="Entries">The move entries.</param>
    /// <param name="SourceIndex">The source index.</param>
    /// <param name="Item">The source item.</param>
    /// <returns>The next move entry, if any; otherwise null.</returns>
    MoveEntry GetNextMoveEntry(List<MoveEntry> Entries, int SourceIndex, BaseItem Item)
    {
        for (int Index = SourceIndex + 1; Index < Entries.Count; Index++)
        {
            if (!IsDescendantOf(Entries[Index].Item, Item))
                return Entries[Index];
        }

        return null;
    }
    /// <summary>
    /// Returns the mixed child item list for a container.
    /// </summary>
    /// <param name="Container">The container.</param>
    /// <returns>The mixed child item list.</returns>
    List<BaseItem> GetContainerItems(BaseItem Container)
    {
        Document DocumentItem = Container as Document;
        if (DocumentItem != null)
            return DocumentItem.Items;

        Folder Folder = Container as Folder;
        return Folder?.Items;
    }
    /// <summary>
    /// Returns the move entries.
    /// </summary>
    /// <returns>The move entries.</returns>
    List<MoveEntry> GetMoveEntries()
    {
        List<MoveEntry> Result = new();
        AddMoveEntries(Result, this, Items);
        return Result;
    }
    /// <summary>
    /// Returns true if an item is a descendant of a possible ancestor.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <param name="PossibleAncestor">The possible ancestor.</param>
    /// <returns>True if the item is a descendant; otherwise false.</returns>
    bool IsDescendantOf(BaseItem Item, BaseItem PossibleAncestor)
    {
        BaseItem ParentItem = Item?.Parent;
        while (ParentItem != null)
        {
            if (ReferenceEquals(ParentItem, PossibleAncestor))
                return true;

            ParentItem = ParentItem.Parent;
        }

        return false;
    }
    /// <summary>
    /// Returns the document child items.
    /// </summary>
    /// <returns>The document child items.</returns>
    public override List<BaseItem> GetChildItems()
    {
        List<BaseItem> Result = new();

        foreach (BaseItem Item in Items)
            Result.Add(Item);

        return Result;
    }
    /// <summary>
    /// Returns true if the document can move in the specified direction.
    /// </summary>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <returns>True if the document can move; otherwise false.</returns>
    public override bool CanMove(bool Up)
    {
        Project ProjectItem = Parent as Project;
        return ProjectItem != null && CanMoveItem(ProjectItem.Documents, this, Up);
    }
    /// <summary>
    /// Moves the document one step in the specified direction.
    /// </summary>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <returns>True if the document is moved; otherwise false.</returns>
    public override bool Move(bool Up)
    {
        Project ProjectItem = Parent as Project;
        if (ProjectItem == null || !CanMove(Up))
            return false;

        bool Result = MoveItem(ProjectItem.Documents, this, Up);
        if (Result)
            ProjectItem.UpdateReferences(null);

        return Result;
    }
    /// <summary>
    /// Deletes the document from persistent storage.
    /// </summary>
    public override void Delete()
    {
        if (!CanDelete())
            throw new InvalidOperationException("This document cannot be deleted.");

        string ItemFolderPath = FolderPath;
        DeleteStorage(ItemFolderPath);

        Project ProjectItem = Parent as Project;
        if (ProjectItem != null)
            ProjectItem.DetachDocument(this);
    }
    /// <summary>
    /// Saves the document to persistent storage.
    /// </summary>
    public override void Save()
    {
        CheckContentModel();
        RenumberChildren();
        UpdateReferences(Parent);
        base.Save();
        SaveMarkdownFile(SynopsisFilePath, Synopsis);

        if (HasFolderStructure)
        {
            SaveStructure();
        }
        else
        {
            DeleteStructure();
        }

        System.IO.Directory.CreateDirectory(ItemsFolderPath);
        foreach (BaseItem Item in Items)
            Item.Save();

        DeleteInternalMoveFolders(ItemsFolderPath);
        DeleteInternalMoveFolders(FolderPath);
        DeleteStorage(FoldersFolderPath);
        DeleteStorage(TextFilesFolderPath);

    }
    /// <summary>
    /// Loads the document from persistent storage.
    /// </summary>
    public override void Load()
    {
        base.Load();
        Synopsis = LoadMarkdownFile(SynopsisFilePath);
        LoadStructure();

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
    /// Prepares persisted item information before saving the document.
    /// </summary>
    public override void PrepareInfo()
    {
        base.PrepareInfo();

        foreach (BaseItem Item in Items)
            Item.PrepareInfo();

    }
    /// <summary>
    /// Applies persisted item information after loading the document.
    /// </summary>
    public override void ApplyInfo()
    {
        base.ApplyInfo();

        foreach (BaseItem Item in Items)
            Item.ApplyInfo();

    }
    /// <summary>
    /// Renumbers document child items.
    /// </summary>
    public override void RenumberChildren()
    {
        CheckContentModel();

        RenumberItems(Items);

        foreach (Folder Folder in Folders)
            Folder.RenumberChildren();
    }
    /// <summary>
    /// Updates runtime references after loading the document graph.
    /// </summary>
    /// <param name="ParentItem">The parent item.</param>
    public override void UpdateReferences(BaseItem ParentItem)
    {
        base.UpdateReferences(ParentItem);
        Structure?.UpdateReferences(null);

        foreach (BaseItem Item in Items)
            Item.UpdateReferences(this);

    }
    /// <summary>
    /// Clears runtime references when the document is detached from its parent.
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
    public override ItemType Type => ItemType.Document;
    /// <summary>
    /// Gets the document structure file name.
    /// </summary>
    static public string StructureFileName => "Structure.json";
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
    /// Gets the synopsis text file name.
    /// </summary>
    static public string SynopsisFileName => "Synopsis.md";
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
    /// Gets the file-system folder path of the document.
    /// </summary>
    [JsonIgnore]
    public override string FolderPath
    {
        get
        {
            Project ProjectItem = Parent as Project;
            if (ProjectItem != null)
                return System.IO.Path.Combine(ProjectItem.DocumentsFolderPath, StorageName);

            return base.FolderPath;
        }
    }
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
    /// Gets the file-system path of the document structure file.
    /// </summary>
    [JsonIgnore]
    public string StructureFilePath => System.IO.Path.Combine(FolderPath, StructureFileName);
    /// <summary>
    /// Gets the file-system path of the document synopsis file.
    /// </summary>
    [JsonIgnore]
    public string SynopsisFilePath => System.IO.Path.Combine(FolderPath, SynopsisFileName);
    /// <summary>
    /// Gets a value indicating whether this document has a folder structure.
    /// </summary>
    [JsonIgnore]
    public bool HasFolderStructure => Structure != null && !string.IsNullOrWhiteSpace(Structure.Title);
    /// <summary>
    /// Gets a value indicating whether this document is a flat document.
    /// </summary>
    [JsonIgnore]
    public bool IsFlatDocument => !HasFolderStructure;
    /// <summary>
    /// Gets a value indicating whether a folder structure can be set.
    /// </summary>
    [JsonIgnore]
    public override bool CanSetStructure => true;
    /// <summary>
    /// Gets a value indicating whether the folder structure can be cleared.
    /// </summary>
    [JsonIgnore]
    public override bool CanClearStructure => HasFolderStructure;
    /// <summary>
    /// Gets a value indicating whether this document can contain root folders.
    /// </summary>
    [JsonIgnore]
    public override bool CanContainFolders => true;
    /// <summary>
    /// Gets a value indicating whether this document can contain text files.
    /// </summary>
    [JsonIgnore]
    public override bool CanContainTextFiles => true;
    /// Gets a value indicating whether a root folder can be added.
    /// </summary>
    [JsonIgnore]
    public override bool CanAddFolder => CanContainFolders && CanAddItem(Items);
    /// <summary>
    /// Gets a value indicating whether a text file can be added.
    /// </summary>
    [JsonIgnore]
    public override bool CanAddTextFile => CanContainTextFiles && CanAddItem(Items);
    /// <summary>
    /// Gets or sets the document child items.
    /// </summary>
    public List<BaseItem> Items
    {
        get => fItems;
        set => fItems = value ?? new List<BaseItem>();
    }
    /// <summary>
    /// Gets or sets the document folders.
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
    /// Gets or sets the document text files.
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
    /// Gets or sets the document synopsis.
    /// </summary>
    public string Synopsis
    {
        get => fSynopsis;
        set => fSynopsis = value ?? string.Empty;
    }
    /// <summary>
    /// Gets or sets the document folder structure.
    /// </summary>
    public FolderItem Structure => fStructure;
}
