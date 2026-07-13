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
    /// <summary>
    /// Field for the Documents property.
    /// </summary>
    protected List<Document> fDocuments = new();
    /// <summary>
    /// Field for the Notes property.
    /// </summary>
    protected List<Note> fNotes = new();
    /// <summary>
    /// Field for the Components property.
    /// </summary>
    protected List<Component> fComponents = new();
    /// <summary>
    /// Field for the TempFileText property.
    /// </summary>
    protected string fTempFileText = string.Empty;
    /// <summary>
    /// Field for the QuickView property.
    /// </summary>
    protected QuickView fQuickView = new();

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
    /// <summary>
    /// Normalizes an image file name stem.
    /// </summary>
    /// <param name="Value">The image file name stem.</param>
    /// <returns>The normalized image file name stem.</returns>
    static string NormalizeImageFileName(string Value)
    {
        string Result = string.IsNullOrWhiteSpace(Value) ? "Image" : Value.Trim();
        foreach (char Char in System.IO.Path.GetInvalidFileNameChars())
            Result = Result.Replace(Char, '_');

        return string.IsNullOrWhiteSpace(Result) ? "Image" : Result;
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

        if (System.IO.File.Exists(ProjectPath))
            throw new InvalidOperationException($"The project storage path points to a file: {ProjectPath}");

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

            if (!ExistingInfo.Id.IsSameText(Id))
                throw new InvalidOperationException($"The project storage path belongs to a different project: {ProjectPath}");

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
            Title = DecodeTitle(System.IO.Path.GetFileName(ProjectPath));
        }
    }
    /// <summary>
    /// Checks whether the project item graph has duplicate ids.
    /// </summary>
    protected virtual void CheckDuplicateItemIds()
    {
        HashSet<string> Ids = new(StringComparer.OrdinalIgnoreCase);
        foreach (BaseItem Item in GetDescendantItems(true))
        {
            if (!Ids.Add(Item.Id))
                throw new InvalidOperationException($"Duplicate item id in project: {Item.Id}");
        }
    }
    /// <summary>
    /// Loads the temporary project markdown file.
    /// </summary>
    protected virtual void LoadTempFile()
    {
        TempFileText = LoadMarkdownFile(TempFilePath);
    }
    /// <summary>
    /// Loads project components from the components bucket.
    /// </summary>
    /// <returns>The loaded components.</returns>
    protected virtual List<Component> LoadComponents()
    {
        List<Component> Result = new();
        if (System.IO.Directory.Exists(ComponentsFolderPath))
        {
            string[] FilePaths = System.IO.Directory.GetFiles(ComponentsFolderPath);
            if (FilePaths.Length > 0)
                throw new InvalidOperationException($"Storage bucket contains files: {ComponentsFolderPath}");

            string[] FolderPaths = System.IO.Directory.GetDirectories(ComponentsFolderPath);
            foreach (string ItemFolderPath in FolderPaths)
            {
                Component Component = new Component();
                Component.Title = DecodeTitle(System.IO.Path.GetFileName(ItemFolderPath));
                Component.UpdateReferences(this);
                Component.Load();
                Result.Add(Component);
            }
        }

        Result = Result.OrderBy(Item => Item.Category).ThenBy(Item => Item.Title).ToList();
        CheckLoadedComponents(Result);
        return Result;
    }
    /// <summary>
    /// Checks loaded components for duplicate titles or ids.
    /// </summary>
    /// <param name="Items">The loaded components.</param>
    protected virtual void CheckLoadedComponents(List<Component> Items)
    {
        for (int Index = 0; Index < Items.Count; Index++)
        {
            Component Item = Items[Index];
            for (int OtherIndex = Index + 1; OtherIndex < Items.Count; OtherIndex++)
            {
                Component OtherItem = Items[OtherIndex];
                if (Item.Title.IsSameText(OtherItem.Title))
                    throw new InvalidOperationException($"Duplicate component title {Item.Title} in folder: {ComponentsFolderPath}");

                if (Item.Id.IsSameText(OtherItem.Id))
                    throw new InvalidOperationException($"Duplicate component id {Item.Id} in folder: {ComponentsFolderPath}");
            }
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
        if (System.IO.File.Exists(StoragePath))
            throw new InvalidOperationException($"A file already exists at the project storage path: {StoragePath}");

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
    /// <summary>
    /// Detaches a note from memory without deleting persistent storage.
    /// </summary>
    /// <param name="Note">The note to remove.</param>
    /// <returns>True if the note is removed; otherwise false.</returns>
    internal bool DetachNote(Note Note)
    {
        bool Result = Notes.Remove(Note);
        if (Result)
        {
            Note.ClearReferences();
            RenumberChildren();
            UpdateReferences(null);
        }

        return Result;
    }
    /// <summary>
    /// Detaches a component from memory without deleting persistent storage.
    /// </summary>
    /// <param name="Component">The component to remove.</param>
    /// <returns>True if the component is removed; otherwise false.</returns>
    internal bool DetachComponent(Component Component)
    {
        bool Result = Components.Remove(Component);
        if (Result)
        {
            Component.ClearReferences();
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
        CheckCanAddItem(Documents);
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
        CheckCanAddItem(Documents);
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
    /// Adds a note.
    /// </summary>
    /// <param name="Title">The note title.</param>
    /// <returns>The added note.</returns>
    public override Note AddNote(string Title)
    {
        if (!CanAddNote)
            throw new InvalidOperationException("This project cannot contain notes.");

        CheckCanAddItem(Notes);
        CheckDuplicateTitle(Notes, Title);

        Note Result = new Note();
        Result.Title = Title;
        Notes.Add(Result);
        Result.UpdateReferences(this);
        RenumberChildren();
        SaveItemIfStorageReady(Result);
        return Result;
    }
    /// <summary>
    /// Removes a note from memory and persistent storage.
    /// </summary>
    /// <param name="Note">The note to remove.</param>
    /// <returns>True if the note is removed; otherwise false.</returns>
    public bool RemoveNote(Note Note)
    {
        return RemoveChild(Note);
    }
    /// <summary>
    /// Adds a component.
    /// </summary>
    /// <param name="Component">The component to add.</param>
    /// <returns>The added component.</returns>
    public override Component AddComponent(Component Component)
    {
        if (!CanAddComponent)
            throw new InvalidOperationException("This project cannot contain components.");

        if (Component == null)
            throw new ArgumentNullException(nameof(Component));

        CheckCanAddItem(Components);
        CheckDuplicateTitle(Components, Component.Title);

        Components.Add(Component);
        Component.UpdateReferences(this);
        SaveItemIfStorageReady(Component);
        return Component;
    }
    /// <summary>
    /// Removes a component from memory and persistent storage.
    /// </summary>
    /// <param name="Component">The component to remove.</param>
    /// <returns>True if the component is removed; otherwise false.</returns>
    public bool RemoveComponent(Component Component)
    {
        return RemoveChild(Component);
    }
    /// <summary>
    /// Deletes a child item from memory and persistent storage.
    /// </summary>
    /// <param name="Item">The child item to delete.</param>
    /// <returns>True if the child item is deleted; otherwise false.</returns>
    public override bool RemoveChild(BaseItem Item)
    {
        Document Document = Item as Document;
        if (Document != null)
        {
            if (!Documents.Contains(Document))
                return false;

            Document.Delete();
            return true;
        }

        Note Note = Item as Note;
        if (Note != null)
        {
            if (!Notes.Contains(Note))
                return false;

            Note.Delete();
            return true;
        }

        Component Component = Item as Component;
        if (Component == null || !Components.Contains(Component))
            return false;

        Component.Delete();
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
    /// Moves a note inside the project notes list.
    /// </summary>
    /// <param name="Note">The note to move.</param>
    /// <param name="NewOrderIndex">The new one-based order index.</param>
    /// <returns>True if the note is moved; otherwise false.</returns>
    public bool MoveNote(Note Note, int NewOrderIndex)
    {
        if (!CanContainNotes)
            throw new InvalidOperationException("This project cannot contain notes.");

        bool Result = MoveItem(Notes, Note, NewOrderIndex);
        if (Result)
            UpdateReferences(null);

        return Result;
    }
    /// <summary>
    /// Copies an image file to the project images folder.
    /// </summary>
    /// <param name="SourceFilePath">The source image file path.</param>
    /// <returns>The markdown relative image path.</returns>
    public string AddImage(string SourceFilePath)
    {
        CheckProjectPath(false);

        if (string.IsNullOrWhiteSpace(SourceFilePath))
            throw new InvalidOperationException("No image file was selected.");

        if (!System.IO.File.Exists(SourceFilePath))
            throw new InvalidOperationException($"The image file does not exist: {SourceFilePath}");

        System.IO.Directory.CreateDirectory(ImagesFolderPath);

        string Extension = System.IO.Path.GetExtension(SourceFilePath);
        string FileName = NormalizeImageFileName(System.IO.Path.GetFileNameWithoutExtension(SourceFilePath));

        string DestFileName = FileName + Extension;
        string DestFilePath = System.IO.Path.Combine(ImagesFolderPath, DestFileName);
        int Index = 2;

        while (System.IO.File.Exists(DestFilePath))
        {
            DestFileName = $"{FileName}-{Index}{Extension}";
            DestFilePath = System.IO.Path.Combine(ImagesFolderPath, DestFileName);
            Index++;
        }

        System.IO.File.Copy(SourceFilePath, DestFilePath);
        return DestFileName;
    }
    /// <summary>
    /// Returns the sorted component list.
    /// </summary>
    /// <returns>The sorted component list.</returns>
    public List<Component> GetComponentList()
    {
        return Components.OrderBy(Item => Item.Category).ThenBy(Item => Item.Title).ToList();
    }
    /// <summary>
    /// Finds a component by id.
    /// </summary>
    /// <param name="Id">The component id.</param>
    /// <returns>The component, if found; otherwise null.</returns>
    public Component FindComponentById(string Id)
    {
        return Components.FirstOrDefault(Item => Item.Id.IsSameText(Id));
    }
    /// <summary>
    /// Counts components with the specified title.
    /// </summary>
    /// <param name="Title">The component title.</param>
    /// <returns>The component count.</returns>
    public int CountComponentTitle(string Title)
    {
        return Components.Count(Item => Item.Title.IsSameText(Title));
    }
    /// <summary>
    /// Returns the sorted project category list.
    /// </summary>
    /// <returns>The sorted category list.</returns>
    public List<string> GetCategoryList()
    {
        return Components
            .Select(Item => Item.Category)
            .Where(Item => !string.IsNullOrWhiteSpace(Item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(Item => Item)
            .ToList();
    }
    /// <summary>
    /// Returns the sorted project tag list.
    /// </summary>
    /// <returns>The sorted tag list.</returns>
    public List<string> GetTagList()
    {
        return Components
            .SelectMany(Item => Item.TagList)
            .Where(Item => !string.IsNullOrWhiteSpace(Item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(Item => Item)
            .ToList();
    }
    /// <summary>
    /// Executes a project-wide search.
    /// </summary>
    /// <param name="Term">The search term.</param>
    /// <returns>The search results.</returns>
    public LinkItemList GlobalSearch(string Term)
    {
        return ProjectGlobalSearch.Execute(this, Term);
    }
    /// <summary>
    /// Saves the temporary project markdown file.
    /// </summary>
    public void SaveTempFile()
    {
        CheckProjectPath(false);
        SaveMarkdownFile(TempFilePath, TempFileText);
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
        CheckDuplicateItemIds();
        base.Save();
        SaveTempFile();

        System.IO.Directory.CreateDirectory(DocumentsFolderPath);

        foreach (Document Document in Documents)
            Document.Save();

        System.IO.Directory.CreateDirectory(NotesFolderPath);

        foreach (Note Note in Notes)
            Note.Save();

        System.IO.Directory.CreateDirectory(ComponentsFolderPath);

        foreach (Component Component in Components)
            Component.Save();

        System.IO.Directory.CreateDirectory(ImagesFolderPath);
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
        LoadTempFile();

        Documents = LoadItems<Document>(DocumentsFolderPath);
        Notes = LoadItems<Note>(NotesFolderPath);
        Components = LoadComponents();
        UpdateReferences(null);
        CheckDuplicateItemIds();
        QuickView = QuickView.Load(this);
    }
    /// <summary>
    /// Renames the project.
    /// </summary>
    /// <param name="NewTitle">The new title.</param>
    public override void Rename(string NewTitle)
    {
        if (!CanRename())
            throw new InvalidOperationException("This project cannot be renamed.");

        string OldTitle = Title;
        try
        {
            Title = NewTitle;
            if (CanPersistStorage())
            {
                CheckProjectPath(false);
                CheckProjectSaveFolder();
                SaveInfo();
            }
        }
        catch
        {
            Title = OldTitle;
            throw;
        }
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

        foreach (Note Note in Notes)
            Note.PrepareInfo();

        foreach (Component Component in Components)
            Component.PrepareInfo();
    }
    /// <summary>
    /// Applies persisted item information after loading the project.
    /// </summary>
    public override void ApplyInfo()
    {
        base.ApplyInfo();

        foreach (Document Document in Documents)
            Document.ApplyInfo();

        foreach (Note Note in Notes)
            Note.ApplyInfo();

        foreach (Component Component in Components)
            Component.ApplyInfo();
    }
    /// <summary>
    /// Renumbers project child items.
    /// </summary>
    public override void RenumberChildren()
    {
        RenumberItems(Documents);
        RenumberItems(Notes);

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

        foreach (Note Note in Notes)
            Note.UpdateReferences(this);

        foreach (Component Component in Components)
            Component.UpdateReferences(this);
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

        foreach (Note Note in Notes)
            Result.Add(Note);

        foreach (Component Component in Components)
            Result.Add(Component);

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
    /// Gets the notes folder name.
    /// </summary>
    static public string NotesFolderName => "Notes";
    /// <summary>
    /// Gets the components folder name.
    /// </summary>
    static public string ComponentsFolderName => "Components";
    /// <summary>
    /// Gets the images folder name.
    /// </summary>
    static public string ImagesFolderName => "Images";
    /// <summary>
    /// Gets the temporary markdown file name.
    /// </summary>
    static public string TempFileName => "Temp.md";
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
    /// Gets a value indicating whether this item can contain notes.
    /// </summary>
    [JsonIgnore]
    public override bool CanContainNotes => true;
    /// <summary>
    /// Gets a value indicating whether this item can contain components.
    /// </summary>
    [JsonIgnore]
    public override bool CanContainComponents => true;
    /// <summary>
    /// Gets a value indicating whether a document can be added to this item.
    /// </summary>
    [JsonIgnore]
    public override bool CanAddDocument => CanAddItem(Documents);
    /// <summary>
    /// Gets a value indicating whether a structured document can be added to this item.
    /// </summary>
    [JsonIgnore]
    public override bool CanAddStructuredDocument => CanAddItem(Documents);
    /// <summary>
    /// Gets a value indicating whether a note can be added to this item.
    /// </summary>
    [JsonIgnore]
    public override bool CanAddNote => CanContainNotes && CanAddItem(Notes);
    /// <summary>
    /// Gets a value indicating whether a component can be added to this item.
    /// </summary>
    [JsonIgnore]
    public override bool CanAddComponent => CanContainComponents && CanAddItem(Components);
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
    /// Gets the parent folder path that contains the project root folder.
    /// </summary>
    [JsonIgnore]
    public string ParentFolderPath => string.IsNullOrWhiteSpace(ProjectPath) ? string.Empty : System.IO.Path.GetDirectoryName(ProjectPath);
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
    /// Gets the file-system folder path of the project notes bucket.
    /// </summary>
    [JsonIgnore]
    public string NotesFolderPath => System.IO.Path.Combine(FolderPath, NotesFolderName);
    /// <summary>
    /// Gets the file-system folder path of the project components bucket.
    /// </summary>
    [JsonIgnore]
    public string ComponentsFolderPath => System.IO.Path.Combine(FolderPath, ComponentsFolderName);
    /// <summary>
    /// Gets the file-system folder path of the project images bucket.
    /// </summary>
    [JsonIgnore]
    public string ImagesFolderPath => System.IO.Path.Combine(FolderPath, ImagesFolderName);
    /// <summary>
    /// Gets the file-system path of the temporary markdown file.
    /// </summary>
    [JsonIgnore]
    public string TempFilePath => System.IO.Path.Combine(FolderPath, TempFileName);
    /// <summary>
    /// Gets the quick-view json file path.
    /// </summary>
    [JsonIgnore]
    public string QuickViewFilePath => System.IO.Path.Combine(FolderPath, QuickView.FileName);
    /// <summary>
    /// Gets or sets the temporary markdown text.
    /// </summary>
    public string TempFileText
    {
        get => fTempFileText;
        set => fTempFileText = value ?? string.Empty;
    }
    /// <summary>
    /// Gets or sets the project quick-view list.
    /// </summary>
    [JsonIgnore]
    public QuickView QuickView
    {
        get => fQuickView;
        set => fQuickView = value ?? new QuickView();
    }
    /// <summary>
    /// Gets or sets the project notes.
    /// </summary>
    public List<Note> Notes
    {
        get => fNotes;
        set => fNotes = value ?? new List<Note>();
    }
    /// <summary>
    /// Gets or sets the project components.
    /// </summary>
    public List<Component> Components
    {
        get => fComponents;
        set => fComponents = value ?? new List<Component>();
    }
    /// <summary>
    /// Gets or sets the project documents.
    /// </summary>
    public List<Document> Documents
    {
        get => fDocuments;
        set => fDocuments = value ?? new List<Document>();
    }
}
