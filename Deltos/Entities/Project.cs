// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents a Deltos project.
/// </summary>
public class Project: BaseItem
{
    // ● protected
    /// <summary>
    /// Field for the ProjectPath property.
    /// </summary>
    protected string fProjectPath = string.Empty;

    // ● construction
    /// <summary>
    /// Initializes a new instance of the Project class.
    /// </summary>
    public Project()
    {
    }

    // ● private
    /// <summary>
    /// Normalizes a project storage folder path.
    /// </summary>
    /// <param name="Value">The project storage folder path.</param>
    /// <returns>The normalized project storage folder path.</returns>
    static string NormalizeProjectPath(string Value)
    {
        if (string.IsNullOrWhiteSpace(Value))
            return string.Empty;

        string Result = Value.Trim();
        if (System.IO.Path.IsPathFullyQualified(Result))
            Result = System.IO.Path.GetFullPath(Result);

        return Result;
    }

    // ● protected
    /// <summary>
    /// Checks whether the project has a resolved storage folder path.
    /// </summary>
    /// <param name="ShouldExist">True when the folder path must already exist.</param>
    protected virtual void CheckProjectPath(bool ShouldExist)
    {
        if (string.IsNullOrWhiteSpace(ProjectPath))
            throw new InvalidOperationException("The project has no storage path.");

        if (!System.IO.Path.IsPathFullyQualified(ProjectPath))
            throw new InvalidOperationException($"The project storage path is not absolute: {ProjectPath}");

        if (ShouldExist && !System.IO.Directory.Exists(ProjectPath))
            throw new InvalidOperationException($"The project storage path does not exist: {ProjectPath}");
    }
    /// <summary>
    /// Checks whether the project information file exists.
    /// </summary>
    protected virtual void CheckProjectInfoFile()
    {
        if (!System.IO.File.Exists(InfoFilePath))
            throw new InvalidOperationException($"The project information file does not exist: {InfoFilePath}");
    }
    /// <summary>
    /// Checks whether the project storage folder can receive a first project save.
    /// </summary>
    protected virtual void CheckProjectSaveFolder()
    {
        if (!System.IO.Directory.Exists(ProjectPath))
            return;

        if (System.IO.File.Exists(InfoFilePath))
        {
            ItemInfo ExistingInfo = new ItemInfo();
            Json.LoadFromFile(ExistingInfo, InfoFilePath);
            if (string.IsNullOrWhiteSpace(ExistingInfo.Id))
                throw new InvalidOperationException($"The project information file has no project id: {InfoFilePath}");

            if (ExistingInfo.Type != ItemType.Project)
                throw new InvalidOperationException($"The project storage path contains a non-project information file: {InfoFilePath}");

            return;
        }

        if (System.IO.Directory.EnumerateFileSystemEntries(ProjectPath).Any())
            throw new InvalidOperationException($"The project storage path is not empty: {ProjectPath}");
    }
    /// <summary>
    /// Updates the project information before saving it.
    /// </summary>
    protected override void UpdateInfo()
    {
        base.UpdateInfo();

        Info.Title = Title;
    }
    /// <summary>
    /// Applies the project information after loading it.
    /// </summary>
    protected override void ApplyInfoCore()
    {
        base.ApplyInfoCore();

        if (Info.Type != ItemType.Project)
            throw new InvalidOperationException($"Invalid project info type. Expected {ItemType.Project}, found {Info.Type}.");

        if (!string.IsNullOrWhiteSpace(Info.Title))
        {
            Title = Info.Title;
        }
        else if (string.IsNullOrWhiteSpace(Title))
        {
            Title = System.IO.Path.GetFileName(ProjectPath);
        }
    }

    // ● static public
    /// <summary>
    /// Returns the storage folder name for a project title.
    /// </summary>
    /// <param name="Title">The project title.</param>
    /// <returns>The project storage folder name.</returns>
    static public string GetProjectFolderName(string Title)
    {
        return EncodeTitle(Title);
    }
    /// <summary>
    /// Returns the project root folder path under a selected parent folder.
    /// </summary>
    /// <param name="ParentFolderPath">The parent folder selected by the user.</param>
    /// <param name="Title">The project title.</param>
    /// <returns>The project root folder path.</returns>
    static public string GetProjectFolderPath(string ParentFolderPath, string Title)
    {
        string ParentPath = NormalizeProjectPath(ParentFolderPath);
        if (string.IsNullOrWhiteSpace(ParentPath))
            throw new InvalidOperationException("The parent storage folder path is empty.");

        if (!System.IO.Path.IsPathFullyQualified(ParentPath))
            throw new InvalidOperationException($"The parent storage folder path is not absolute: {ParentPath}");

        if (!System.IO.Directory.Exists(ParentPath))
            throw new InvalidOperationException($"The parent storage folder path does not exist: {ParentPath}");

        return System.IO.Path.Combine(ParentPath, GetProjectFolderName(Title));
    }
    /// <summary>
    /// Creates a new project under a parent storage folder.
    /// </summary>
    /// <param name="ParentFolderPath">The parent folder selected by the user.</param>
    /// <param name="Title">The project title.</param>
    /// <returns>The created project.</returns>
    static public Project Create(string ParentFolderPath, string Title)
    {
        string StoragePath = GetProjectFolderPath(ParentFolderPath, Title);
        if (System.IO.Directory.Exists(StoragePath) && System.IO.Directory.EnumerateFileSystemEntries(StoragePath).Any())
            throw new InvalidOperationException($"The project storage path is not empty: {StoragePath}");

        Project Result = new Project();
        Result.ProjectPath = StoragePath;
        Result.Title = Title;
        Result.UpdateReferences(null);
        Result.Save();
        return Result;
    }
    /// <summary>
    /// Opens an existing project from a storage folder.
    /// </summary>
    /// <param name="ProjectPath">The project storage folder path.</param>
    /// <returns>The opened project.</returns>
    static public Project Open(string ProjectPath)
    {
        Project Result = new Project();
        Result.ProjectPath = NormalizeProjectPath(ProjectPath);
        Result.Load();
        return Result;
    }
    
    // ● internal
    /// <summary>
    /// Detaches a document from memory without deleting persistent storage.
    /// </summary>
    /// <param name="Document">The document to remove.</param>
    /// <returns>True if the document is removed; otherwise false.</returns>
    internal bool DetachDocument(Document Document)
    {
        bool Result = Documents.Remove(Document);
        if (Result)
        {
            Document.ClearReferences();
            RenumberChildren();
            UpdateReferences(null);
        }

        return Result;
    }

    // ● public
    /// <summary>
    /// Adds a document.
    /// </summary>
    /// <param name="Title">The document title.</param>
    /// <returns>The added document.</returns>
    public override Document AddDocument(string Title)
    {
        CheckDuplicateTitle(Documents, Title);

        Document Result = new Document();
        Result.Title = Title;
        Documents.Add(Result);
        Result.UpdateReferences(this);
        RenumberChildren();
        SaveItemIfStorageReady(Result);
        return Result;
    }
    /// <summary>
    /// Adds a structured document.
    /// </summary>
    /// <param name="Title">The document title.</param>
    /// <param name="Structure">The document folder structure.</param>
    /// <returns>The added document.</returns>
    public override Document AddDocument(string Title, FolderItem Structure)
    {
        if (Structure == null)
            throw new ArgumentNullException(nameof(Structure));

        Structure.CheckValid();
        CheckDuplicateTitle(Documents, Title);

        Document Result = new Document();
        Result.Title = Title;
        Result.SetStructure(Structure);
        Documents.Add(Result);
        Result.UpdateReferences(this);
        RenumberChildren();
        SaveItemIfStorageReady(Result);
        return Result;
    }
    /// <summary>
    /// Removes a document from memory and persistent storage.
    /// </summary>
    /// <param name="Document">The document to remove.</param>
    /// <returns>True if the document is removed; otherwise false.</returns>
    public bool RemoveDocument(Document Document)
    {
        return RemoveChild(Document);
    }
    /// <summary>
    /// Deletes a child item from memory and persistent storage.
    /// </summary>
    /// <param name="Item">The child item to delete.</param>
    /// <returns>True if the child item is deleted; otherwise false.</returns>
    public override bool RemoveChild(BaseItem Item)
    {
        Document Document = Item as Document;
        if (Document == null || !Documents.Contains(Document))
            return false;

        Document.Delete();
        return true;
    }
    /// <summary>
    /// Moves a document inside the project documents list.
    /// </summary>
    /// <param name="Document">The document to move.</param>
    /// <param name="NewOrderIndex">The new one-based order index.</param>
    /// <returns>True if the document is moved; otherwise false.</returns>
    public bool MoveDocument(Document Document, int NewOrderIndex)
    {
        bool Result = MoveItem(Documents, Document, NewOrderIndex);
        if (Result)
            UpdateReferences(null);

        return Result;
    }
    /// <summary>
    /// Saves the project to persistent storage.
    /// </summary>
    public override void Save()
    {
        CheckProjectPath(false);
        CheckProjectSaveFolder();
        System.IO.Directory.CreateDirectory(FolderPath);
        RenumberChildren();
        UpdateReferences(null);
        base.Save();

        System.IO.Directory.CreateDirectory(DocumentsFolderPath);

        foreach (Document Document in Documents)
            Document.Save();
    }
    /// <summary>
    /// Loads the project from persistent storage.
    /// </summary>
    public override void Load()
    {
        CheckProjectPath(true);
        CheckProjectInfoFile();
        UpdateReferences(null);
        base.Load();

        Documents = LoadItems<Document>(DocumentsFolderPath);
        UpdateReferences(null);
    }
    /// <summary>
    /// Renames the project.
    /// </summary>
    /// <param name="NewTitle">The new title.</param>
    public override void Rename(string NewTitle)
    {
        if (!CanRename())
            throw new InvalidOperationException("This project cannot be renamed.");

        Title = NewTitle;
        if (CanPersistStorage())
            SaveInfo();
    }
    /// <summary>
    /// Returns true if the project can be deleted from its parent.
    /// </summary>
    /// <returns>Always false for the project root.</returns>
    public override bool CanDelete()
    {
        return false;
    }
    /// <summary>
    /// Deletes the project from persistent storage.
    /// </summary>
    public override void Delete()
    {
        throw new InvalidOperationException("A project root cannot be deleted as a child item.");
    }
    /// <summary>
    /// Prepares persisted item information before saving the project.
    /// </summary>
    public override void PrepareInfo()
    {
        base.PrepareInfo();

        foreach (Document Document in Documents)
            Document.PrepareInfo();
    }
    /// <summary>
    /// Applies persisted item information after loading the project.
    /// </summary>
    public override void ApplyInfo()
    {
        base.ApplyInfo();

        foreach (Document Document in Documents)
            Document.ApplyInfo();
    }
    /// <summary>
    /// Renumbers project child items.
    /// </summary>
    public override void RenumberChildren()
    {
        RenumberItems(Documents);

        foreach (Document Document in Documents)
            Document.RenumberChildren();
    }
    /// <summary>
    /// Updates runtime references after loading the project graph.
    /// </summary>
    /// <param name="ParentItem">The parent item.</param>
    public override void UpdateReferences(BaseItem ParentItem)
    {
        Parent = ParentItem;
        Project = this;

        foreach (Document Document in Documents)
            Document.UpdateReferences(this);
    }
    /// <summary>
    /// Returns the project child items.
    /// </summary>
    /// <returns>The project child items.</returns>
    public override List<BaseItem> GetChildItems()
    {
        List<BaseItem> Result = new();
        foreach (Document Document in Documents)
            Result.Add(Document);

        return Result;
    }

    // ● properties
    /// <summary>
    /// Gets the item type.
    /// </summary>
    public override ItemType Type => ItemType.Project;
    /// <summary>
    /// Gets the documents folder name.
    /// </summary>
    static public string DocumentsFolderName => "Documents";
    /// <summary>
    /// Gets the owning document.
    /// </summary>
    [JsonIgnore]
    public override Document Document => null;
    /// <summary>
    /// Gets a value indicating whether this item can contain documents.
    /// </summary>
    [JsonIgnore]
    public override bool CanContainDocuments => true;
    /// <summary>
    /// Gets a value indicating whether a document can be added to this item.
    /// </summary>
    [JsonIgnore]
    public override bool CanAddDocument => true;
    /// <summary>
    /// Gets or sets the project root folder path.
    /// </summary>
    [JsonIgnore]
    public string ProjectPath
    {
        get => fProjectPath;
        set => fProjectPath = NormalizeProjectPath(value);
    }
    /// <summary>
    /// Gets the file-system folder path of the project.
    /// </summary>
    [JsonIgnore]
    public override string FolderPath => ProjectPath;
    /// <summary>
    /// Gets the file-system storage name of the project folder.
    /// </summary>
    [JsonIgnore]
    public override string StorageName => string.Empty;
    /// <summary>
    /// Gets the file-system folder path of the project documents bucket.
    /// </summary>
    [JsonIgnore]
    public string DocumentsFolderPath => System.IO.Path.Combine(FolderPath, DocumentsFolderName);
    /// <summary>
    /// Gets or sets the project documents.
    /// </summary>
    public List<Document> Documents { get; set; } = new();
}
