// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents a document inside a project.
/// </summary>
public class Document: BaseItem
{
    // ● protected
    /// <summary>
    /// Field for the Structure property.
    /// </summary>
    protected FolderItem fStructure = new();

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
        if (HasFolderStructure && Files.Count > 0)
            throw new InvalidOperationException("A structured document cannot contain document-level text files.");

        if (!HasFolderStructure && Folders.Count > 0)
            throw new InvalidOperationException("A flat document cannot contain folders.");
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
            throw new InvalidOperationException("The document has no folder structure.");

        CheckDuplicateTitle(Folders, Title);

        Folder Result = new Folder();
        Result.Title = Title;
        Result.LevelTitle = string.IsNullOrWhiteSpace(LevelTitle) ? Structure.Title : LevelTitle;
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
            throw new InvalidOperationException("This document cannot contain text files.");

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
    /// Sets the document folder structure.
    /// </summary>
    /// <param name="Value">The folder structure.</param>
    public void SetStructure(FolderItem Value)
    {
        if (!CanSetStructure)
            throw new InvalidOperationException("Cannot set a folder structure while the document contains child items.");

        if (Value == null)
            throw new ArgumentNullException(nameof(Value));

        Value.CheckValid();
        fStructure = Value;
        Structure.UpdateReferences(null);
        CheckContentModel();
        if (CanPersistStorage())
            Save();
    }
    /// <summary>
    /// Clears the document folder structure and turns the document into a flat document.
    /// </summary>
    public void ClearStructure()
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
    /// Moves a folder inside the document folders list.
    /// </summary>
    /// <param name="Folder">The folder to move.</param>
    /// <param name="NewOrderIndex">The new one-based order index.</param>
    /// <returns>True if the folder is moved; otherwise false.</returns>
    public bool MoveFolder(Folder Folder, int NewOrderIndex)
    {
        if (!CanContainFolders)
            throw new InvalidOperationException("This document cannot contain folders.");

        bool Result = MoveItem(Folders, Folder, NewOrderIndex);
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

        bool Result = MoveItem(Files, File, NewOrderIndex);
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
        if (CanContainTextFiles)
            Result.Add(this);

        foreach (BaseItem Container in GetMoveContainers())
        {
            Folder Folder = Container as Folder;
            if (Folder != null && Folder.CanContainTextFiles)
                Result.Add(Folder);
        }

        return Result;
    }
    /// <summary>
    /// Returns the document child items.
    /// </summary>
    /// <returns>The document child items.</returns>
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

        Project ProjectItem = Parent as Project;
        if (ProjectItem != null)
            ProjectItem.DetachDocument(this);

        DeleteStorage(ItemFolderPath);
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
        if (HasFolderStructure)
        {
            SaveStructure();
        }
        else
        {
            DeleteStructure();
        }

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
    /// Loads the document from persistent storage.
    /// </summary>
    public override void Load()
    {
        base.Load();
        LoadStructure();

        Folders = new List<Folder>();
        Files = new List<TextFile>();
        Folders = CanContainFolders ? LoadItems<Folder>(FoldersFolderPath) : new List<Folder>();
        Files = CanContainTextFiles ? LoadItems<TextFile>(TextFilesFolderPath) : new List<TextFile>();
        UpdateReferences(Parent);
    }
    /// <summary>
    /// Prepares persisted item information before saving the document.
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
    /// Applies persisted item information after loading the document.
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
    /// Renumbers document child items.
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
    /// Updates runtime references after loading the document graph.
    /// </summary>
    /// <param name="ParentItem">The parent item.</param>
    public override void UpdateReferences(BaseItem ParentItem)
    {
        base.UpdateReferences(ParentItem);
        Structure?.UpdateReferences(null);

        foreach (Folder Folder in Folders)
            Folder.UpdateReferences(this);

        foreach (TextFile File in Files)
            File.UpdateReferences(this);
    }
    /// <summary>
    /// Clears runtime references when the document is detached from its parent.
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
    public override ItemType Type => ItemType.Document;
    /// <summary>
    /// Gets the document structure file name.
    /// </summary>
    static public string StructureFileName => "Structure.json";
    /// <summary>
    /// Gets the folders bucket folder name.
    /// </summary>
    static public string FoldersFolderName => "Folders";
    /// <summary>
    /// Gets the text files bucket folder name.
    /// </summary>
    static public string TextFilesFolderName => "TextFiles";
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
    /// Gets the file-system path of the document structure file.
    /// </summary>
    [JsonIgnore]
    public string StructureFilePath => System.IO.Path.Combine(FolderPath, StructureFileName);
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
    public bool CanSetStructure => Files.Count == 0 && Folders.Count == 0;
    /// <summary>
    /// Gets a value indicating whether the folder structure can be cleared.
    /// </summary>
    [JsonIgnore]
    public bool CanClearStructure => Folders.Count == 0;
    /// <summary>
    /// Gets a value indicating whether this document can contain root folders.
    /// </summary>
    [JsonIgnore]
    public override bool CanContainFolders => HasFolderStructure && Files.Count == 0;
    /// <summary>
    /// Gets a value indicating whether this document can contain text files.
    /// </summary>
    [JsonIgnore]
    public override bool CanContainTextFiles => !HasFolderStructure && Folders.Count == 0;
    /// <summary>
    /// Gets a value indicating whether a root folder can be added.
    /// </summary>
    [JsonIgnore]
    public override bool CanAddFolder => CanContainFolders;
    /// <summary>
    /// Gets a value indicating whether a text file can be added.
    /// </summary>
    [JsonIgnore]
    public override bool CanAddTextFile => CanContainTextFiles;
    /// <summary>
    /// Gets or sets the document folders.
    /// </summary>
    public List<Folder> Folders { get; set; } = new();
    /// <summary>
    /// Gets or sets the document text files.
    /// </summary>
    public List<TextFile> Files { get; set; } = new();
    /// <summary>
    /// Gets or sets the document folder structure.
    /// </summary>
    public FolderItem Structure => fStructure;
}
