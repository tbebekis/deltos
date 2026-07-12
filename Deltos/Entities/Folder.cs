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
    /// <summary>
    /// Checks whether the folder can be renamed.
    /// </summary>
    /// <param name="NewTitle">The new title.</param>
    protected override void CheckRenameTitle(string NewTitle)
    {
        Document DocumentItem = Parent as Document;
        if (DocumentItem != null)
            CheckDuplicateTitle(DocumentItem.Folders, NewTitle, this);

        Folder FolderItem = Parent as Folder;
        if (FolderItem != null)
            CheckDuplicateTitle(FolderItem.Folders, NewTitle, this);
    }
    /// <summary>
    /// Checks whether the folder content model is valid.
    /// </summary>
    protected virtual void CheckContentModel()
    {
        if (CanContainFolders && Files.Count > 0)
            throw new InvalidOperationException("A non-leaf folder cannot contain text files.");

        if (CanContainTextFiles && Folders.Count > 0)
            throw new InvalidOperationException("A leaf folder cannot contain child folders.");

        if (!CanContainFolders && !CanContainTextFiles && (Folders.Count > 0 || Files.Count > 0))
            throw new InvalidOperationException("The folder does not match the document structure.");
    }

    // ● internal
    /// <summary>
    /// Detaches a folder from memory without deleting persistent storage.
    /// </summary>
    /// <param name="Folder">The folder to remove.</param>
    /// <returns>True if the folder is removed; otherwise false.</returns>
    internal bool DetachFolder(Folder Folder)
    {
        bool Result = Folders.Remove(Folder);
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
        bool Result = Files.Remove(File);
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

        FolderItem ChildStructureItem = StructureItem.Child;
        CheckDuplicateTitle(Folders, Title);

        Folder Result = new Folder();
        Result.Title = Title;
        Result.LevelTitle = string.IsNullOrWhiteSpace(LevelTitle) ? ChildStructureItem.Title : LevelTitle;
        Folders.Add(Result);
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

        CheckDuplicateTitle(Files, Title);

        TextFile Result = new TextFile();
        Result.Title = Title;
        Files.Add(Result);
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
            if (!Folders.Contains(Folder))
                return false;

            Folder.Delete();
            return true;
        }

        TextFile File = Item as TextFile;
        if (File == null || !Files.Contains(File))
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

        bool Result = MoveItem(Folders, Folder, NewOrderIndex);
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

        bool Result = MoveItem(Files, File, NewOrderIndex);
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

        if (CanContainFolders)
        {
            foreach (Folder Folder in Folders)
                Result.Add(Folder);
        }

        if (CanContainTextFiles)
        {
            foreach (TextFile File in Files)
                Result.Add(File);
        }

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
        {
            if (CanMoveItem(DocumentItem.Folders, this, Up))
                return true;

            BaseItem TargetContainer = GetAdjacentFolderMoveContainer(Document, Parent, Level, Up, this);
            Document TargetDocument = TargetContainer as Document;
            if (TargetDocument != null)
                return !ContainsTitle(TargetDocument.Folders, Title);

            Folder TargetFolder = TargetContainer as Folder;
            return TargetFolder != null && !ContainsTitle(TargetFolder.Folders, Title);
        }

        Folder FolderItem = Parent as Folder;
        if (FolderItem != null)
        {
            if (CanMoveItem(FolderItem.Folders, this, Up))
                return true;

            BaseItem TargetContainer = GetAdjacentFolderMoveContainer(Document, Parent, Level, Up, this);
            Document TargetDocument = TargetContainer as Document;
            if (TargetDocument != null)
                return !ContainsTitle(TargetDocument.Folders, Title);

            Folder TargetFolder = TargetContainer as Folder;
            return TargetFolder != null && !ContainsTitle(TargetFolder.Folders, Title);
        }

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
            bool Result = false;
            if (CanMoveItem(DocumentItem.Folders, this, Up))
            {
                Result = MoveItem(DocumentItem.Folders, this, Up);
            }
            else
            {
                BaseItem TargetContainer = GetAdjacentFolderMoveContainer(Document, Parent, Level, Up, this);
                Document TargetDocument = TargetContainer as Document;
                if (TargetDocument != null)
                    Result = MoveItem(DocumentItem.Folders, TargetDocument.Folders, this, TargetDocument, Up);

                Folder TargetFolder = TargetContainer as Folder;
                if (TargetFolder != null)
                    Result = MoveItem(DocumentItem.Folders, TargetFolder.Folders, this, TargetFolder, Up);
            }

            if (Result)
                DocumentItem.UpdateReferences(DocumentItem.Parent);

            return Result;
        }

        Folder FolderItem = Parent as Folder;
        if (FolderItem != null)
        {
            bool Result = false;
            if (CanMoveItem(FolderItem.Folders, this, Up))
            {
                Result = MoveItem(FolderItem.Folders, this, Up);
            }
            else
            {
                BaseItem TargetContainer = GetAdjacentFolderMoveContainer(Document, Parent, Level, Up, this);
                Document TargetDocument = TargetContainer as Document;
                if (TargetDocument != null)
                    Result = MoveItem(FolderItem.Folders, TargetDocument.Folders, this, TargetDocument, Up);

                Folder TargetFolder = TargetContainer as Folder;
                if (TargetFolder != null)
                    Result = MoveItem(FolderItem.Folders, TargetFolder.Folders, this, TargetFolder, Up);
            }

            if (Result)
                FolderItem.UpdateReferences(FolderItem.Parent);

            return Result;
        }

        return false;
    }
    /// <summary>
    /// Deletes the folder from persistent storage.
    /// </summary>
    public override void Delete()
    {
        if (!CanDelete())
            throw new InvalidOperationException("This folder cannot be deleted.");

        string ItemFolderPath = FolderPath;

        Document DocumentItem = Parent as Document;
        if (DocumentItem != null)
            DocumentItem.DetachFolder(this);

        Folder FolderItem = Parent as Folder;
        if (FolderItem != null)
            FolderItem.DetachFolder(this);

        DeleteStorage(ItemFolderPath);
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

        if (CanContainFolders)
        {
            System.IO.Directory.CreateDirectory(FoldersFolderPath);
            DeleteStorage(TextFilesFolderPath);
        }

        if (CanContainTextFiles)
        {
            System.IO.Directory.CreateDirectory(TextFilesFolderPath);
            DeleteStorage(FoldersFolderPath);
        }

        if (CanContainFolders)
        {
            foreach (Folder Folder in Folders)
                Folder.Save();
        }

        if (CanContainTextFiles)
        {
            foreach (TextFile File in Files)
                File.Save();
        }
    }
    /// <summary>
    /// Loads the folder from persistent storage.
    /// </summary>
    public override void Load()
    {
        base.Load();

        Folders = new List<Folder>();
        Files = new List<TextFile>();
        Folders = CanContainFolders ? LoadItems<Folder>(FoldersFolderPath) : new List<Folder>();
        Files = CanContainTextFiles ? LoadItems<TextFile>(TextFilesFolderPath) : new List<TextFile>();
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
        CheckContentModel();

        if (CanContainFolders)
            RenumberItems(Folders);

        if (CanContainTextFiles)
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
    /// <summary>
    /// Clears runtime references when the folder is detached from its parent.
    /// </summary>
    public override void ClearReferences()
    {
        base.ClearReferences();

        foreach (Folder Folder in Folders)
            Folder.ClearReferences();

        foreach (TextFile File in Files)
            File.ClearReferences();
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
    public override bool CanContainFolders => StructureItem != null && !StructureItem.IsLeaf && Files.Count == 0;
    /// <summary>
    /// Gets a value indicating whether this folder can contain text files.
    /// </summary>
    [JsonIgnore]
    public override bool CanContainTextFiles => IsLeafLevel && Folders.Count == 0;
    /// <summary>
    /// Gets a value indicating whether a child folder can be added.
    /// </summary>
    [JsonIgnore]
    public override bool CanAddFolder => CanContainFolders;
    /// <summary>
    /// Gets a value indicating whether a text file can be added.
    /// </summary>
    [JsonIgnore]
    public override bool CanAddTextFile => CanContainTextFiles;
    /// <summary>
    /// Gets or sets the child folders.
    /// </summary>
    public List<Folder> Folders { get; set; } = new();
    /// <summary>
    /// Gets or sets the child text files.
    /// </summary>
    public List<TextFile> Files { get; set; } = new();
}
