// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Tests.Entities;

/// <summary>
/// Tests the document content mode contract.
/// </summary>
public class DocumentModeTests
{
    // ● private
    /// <summary>
    /// Creates a temporary project.
    /// </summary>
    /// <param name="ProjectPath">The temporary project path.</param>
    /// <returns>The created project.</returns>
    static Project CreateProject(out string ProjectPath)
    {
        string ParentFolderPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(ParentFolderPath);
        Project Result = Project.Create(ParentFolderPath, "Test Project");
        ProjectPath = Result.ProjectPath;
        return Result;
    }
    /// <summary>
    /// Deletes a folder if it exists.
    /// </summary>
    /// <param name="FolderPath">The folder path.</param>
    static void DeleteFolder(string FolderPath)
    {
        if (Directory.Exists(FolderPath))
            Directory.Delete(FolderPath, true);
    }
    /// <summary>
    /// Creates a two-level document folder structure.
    /// </summary>
    /// <returns>The folder structure.</returns>
    static FolderItem CreateChapterSceneStructure()
    {
        FolderItem Result = new FolderItem();
        Result.Title = "Chapter";
        Result.Child = new FolderItem();
        Result.Child.Title = "Scene";
        return Result;
    }
    /// <summary>
    /// Adds placeholder documents to a project.
    /// </summary>
    /// <param name="Project">The project.</param>
    /// <param name="Count">The item count.</param>
    static void AddPlaceholderDocuments(Project Project, int Count)
    {
        for (int Index = 0; Index < Count; Index++)
        {
            Document Document = new Document();
            Document.Title = $"Book {Index + 1}";
            Project.Documents.Add(Document);
        }
    }
    /// <summary>
    /// Adds placeholder folders to a document.
    /// </summary>
    /// <param name="Document">The document.</param>
    /// <param name="Count">The item count.</param>
    static void AddPlaceholderFolders(Document Document, int Count)
    {
        for (int Index = 0; Index < Count; Index++)
        {
            Folder Folder = new Folder();
            Folder.Title = $"Chapter {Index + 1}";
            Document.Folders.Add(Folder);
        }
    }
    /// <summary>
    /// Adds placeholder text files to a document.
    /// </summary>
    /// <param name="Document">The document.</param>
    /// <param name="Count">The item count.</param>
    static void AddPlaceholderTextFiles(Document Document, int Count)
    {
        for (int Index = 0; Index < Count; Index++)
        {
            TextFile TextFile = new TextFile();
            TextFile.Title = $"Scene {Index + 1}";
            Document.Files.Add(TextFile);
        }
    }

    // ● public
    /// <summary>
    /// Tests that only a project can contain and add documents.
    /// </summary>
    [Fact]
    public void OnlyProjectCanAddDocuments()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            BaseItem ProjectItem = Project;
            Document Document = ProjectItem.AddDocument("Book");
            Document.AddTextFile("Opening Scene");
            TextFile TextFileItem = Document.Files[0];

            Assert.True(Project.CanContainDocuments);
            Assert.True(Project.CanAddDocument);
            Assert.True(Project.CanAddStructuredDocument);
            Assert.False(Document.CanContainDocuments);
            Assert.False(Document.CanAddDocument);
            Assert.False(Document.CanAddStructuredDocument);
            Assert.False(TextFileItem.CanContainDocuments);
            Assert.False(TextFileItem.CanAddDocument);
            Assert.False(TextFileItem.CanAddStructuredDocument);
            Assert.Throws<InvalidOperationException>(() => Document.AddDocument("Nested Book"));
            Assert.True(Directory.Exists(Document.FolderPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that add command capability checks respect the maximum storage order.
    /// </summary>
    [Fact]
    public void AddCommandCapabilitiesRespectMaximumStorageOrder()
    {
        Project Project = new Project();
        Project.Title = "Test Project";
        Project.UpdateReferences(null);
        AddPlaceholderDocuments(Project, BaseItem.MaxOrderIndex);

        Document StructuredDocument = new Document();
        StructuredDocument.Title = "Structured Book";
        StructuredDocument.SetStructure(CreateChapterSceneStructure());
        AddPlaceholderFolders(StructuredDocument, BaseItem.MaxOrderIndex);

        Document FlatDocument = new Document();
        FlatDocument.Title = "Flat Book";
        AddPlaceholderTextFiles(FlatDocument, BaseItem.MaxOrderIndex);

        Assert.False(Project.CanAddDocument);
        Assert.False(Project.CanAddStructuredDocument);
        Assert.Throws<InvalidOperationException>(() => Project.AddDocument("Overflow Book"));
        Assert.Equal(BaseItem.MaxOrderIndex, Project.Documents.Count);

        Assert.False(StructuredDocument.CanAddFolder);
        Assert.Throws<InvalidOperationException>(() => StructuredDocument.AddFolder("Overflow Chapter", string.Empty));
        Assert.Equal(BaseItem.MaxOrderIndex, StructuredDocument.Folders.Count);

        Assert.False(FlatDocument.CanAddTextFile);
        Assert.Throws<InvalidOperationException>(() => FlatDocument.AddTextFile("Overflow Scene"));
        Assert.Equal(BaseItem.MaxOrderIndex, FlatDocument.Files.Count);
    }
    /// <summary>
    /// Tests that null collection setters are normalized to empty collections.
    /// </summary>
    [Fact]
    public void NullCollectionSettersNormalizeToEmptyCollections()
    {
        Project Project = new Project();
        Project.Title = "Test Project";
        Project.Documents = null;

        Document Document = new Document();
        Document.Title = "Structured Book";
        Document.SetStructure(CreateChapterSceneStructure());
        Document.Folders = null;
        Document.Files = null;

        Folder Folder = new Folder();
        Folder.Title = "Chapter One";
        Folder.Folders = null;
        Folder.Files = null;

        Assert.Empty(Project.Documents);
        Assert.True(Project.CanAddDocument);
        Assert.Empty(Document.Folders);
        Assert.Empty(Document.Files);
        Assert.True(Document.CanAddFolder);
        Assert.Empty(Folder.Folders);
        Assert.Empty(Folder.Files);
    }
    /// <summary>
    /// Tests that assigning a folder structure child updates runtime parent references.
    /// </summary>
    [Fact]
    public void FolderItemChildSetterUpdatesParentReferences()
    {
        FolderItem Parent = new FolderItem();
        Parent.Title = "Chapter";
        FolderItem FirstChild = new FolderItem();
        FirstChild.Title = "Scene";
        FolderItem SecondChild = new FolderItem();
        SecondChild.Title = "Beat";

        Parent.Child = FirstChild;
        Parent.Child = SecondChild;
        Parent.Child = null;

        Assert.Null(FirstChild.Parent);
        Assert.Null(SecondChild.Parent);
        Assert.Null(Parent.Child);
    }
    /// <summary>
    /// Tests that cloning a folder item graph creates independent runtime references.
    /// </summary>
    [Fact]
    public void FolderItemCloneCreatesIndependentReferences()
    {
        FolderItem Structure = CreateChapterSceneStructure();

        FolderItem Clone = Structure.Clone();
        Structure.Child.Title = "Beat";

        Assert.NotSame(Structure, Clone);
        Assert.NotSame(Structure.Child, Clone.Child);
        Assert.Null(Clone.Parent);
        Assert.Same(Clone, Clone.Child.Parent);
        Assert.Equal("Scene", Clone.Child.Title);
    }
    /// <summary>
    /// Tests that a document cannot receive a folder structure while it contains text files.
    /// </summary>
    [Fact]
    public void DocumentWithTextFilesCannotSetStructure()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            Document.AddTextFile("Opening Scene");

            Assert.True(Document.IsFlatDocument);
            Assert.False(Document.CanSetStructure);
            Assert.Throws<InvalidOperationException>(() => Document.SetStructure(CreateChapterSceneStructure()));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that a document cannot change its folder structure while it contains folders.
    /// </summary>
    [Fact]
    public void DocumentWithFoldersCannotSetStructure()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book", CreateChapterSceneStructure());
            Document.AddFolder("Chapter One", string.Empty);

            Assert.False(Document.CanSetStructure);
            Assert.Throws<InvalidOperationException>(() => Document.SetStructure(CreateChapterSceneStructure()));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that setting a document structure requires a valid folder item graph.
    /// </summary>
    [Fact]
    public void SetStructureRequiresValidFolderItemGraph()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book");
            FolderItem EmptyStructure = new FolderItem();

            Assert.Throws<ArgumentNullException>(() => Document.SetStructure(null));
            Assert.Throws<Exception>(() => Document.SetStructure(EmptyStructure));
            Assert.Throws<Exception>(() => new FolderItem { Title = "Chapter-Level" });
            Assert.True(Document.IsFlatDocument);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that setting a document structure rejects cyclic folder item graphs.
    /// </summary>
    [Fact]
    public void SetStructureRejectsCyclicFolderItemGraph()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book");
            FolderItem Structure = new FolderItem();
            Structure.Title = "Chapter";
            Structure.Child = Structure;

            Assert.Throws<InvalidOperationException>(() => Document.SetStructure(Structure));
            Assert.False(Structure.IsValid());
            Assert.True(Document.IsFlatDocument);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that setting a document structure clones the supplied folder item graph.
    /// </summary>
    [Fact]
    public void SetStructureClonesSuppliedFolderItemGraph()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book");
            FolderItem Structure = CreateChapterSceneStructure();

            Document.SetStructure(Structure);
            Structure.Title = "Part";
            Structure.Child.Title = "Chapter";

            Assert.NotSame(Structure, Document.Structure);
            Assert.Equal("Chapter", Document.Structure.Title);
            Assert.Equal("Scene", Document.Structure.Child.Title);
            Assert.Null(Structure.Parent);
            Assert.Same(Document.Structure, Document.Structure.Child.Parent);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that a structured document cannot be changed to flat while it contains folders.
    /// </summary>
    [Fact]
    public void StructuredDocumentWithFoldersCannotClearStructure()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book");
            Document.SetStructure(CreateChapterSceneStructure());
            Document.AddFolder("Chapter One", string.Empty);

            Assert.True(Document.HasFolderStructure);
            Assert.False(Document.CanClearStructure);
            Assert.Throws<InvalidOperationException>(() => Document.ClearStructure());
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that document add commands respect the active content mode.
    /// </summary>
    [Fact]
    public void DocumentAddCommandsRespectContentMode()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document FlatDocument = Project.AddDocument("Flat Book");
            Assert.True(FlatDocument.CanAddTextFile);
            Assert.False(FlatDocument.CanAddFolder);
            Assert.Throws<InvalidOperationException>(() => FlatDocument.AddFolder("Chapter One", string.Empty));

            Document StructuredDocument = Project.AddDocument("Structured Book");
            StructuredDocument.SetStructure(CreateChapterSceneStructure());
            Assert.True(StructuredDocument.CanAddFolder);
            Assert.False(StructuredDocument.CanAddTextFile);
            Assert.Throws<InvalidOperationException>(() => StructuredDocument.AddTextFile("Opening Scene"));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that a project can add a structured document in one command.
    /// </summary>
    [Fact]
    public void ProjectAddStructuredDocumentCreatesStructuredStorage()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            BaseItem ProjectItem = Project;

            Document Document = ProjectItem.AddDocument("Structured Book", CreateChapterSceneStructure());

            Assert.True(Document.HasFolderStructure);
            Assert.True(Document.CanAddFolder);
            Assert.False(Document.CanAddTextFile);
            Assert.True(File.Exists(Document.StructureFilePath));
            Assert.True(Directory.Exists(Document.FoldersFolderPath));
            Assert.False(Directory.Exists(Document.TextFilesFolderPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that adding a structured document clones the supplied folder item graph.
    /// </summary>
    [Fact]
    public void ProjectAddStructuredDocumentClonesSuppliedFolderItemGraph()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            FolderItem Structure = CreateChapterSceneStructure();

            Document Document = Project.AddDocument("Structured Book", Structure);
            Structure.Title = "Part";
            Structure.Child.Title = "Chapter";

            Assert.NotSame(Structure, Document.Structure);
            Assert.Equal("Chapter", Document.Structure.Title);
            Assert.Equal("Scene", Document.Structure.Child.Title);
            Assert.Null(Structure.Parent);
            Assert.Same(Document.Structure, Document.Structure.Child.Parent);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that document structure commands are available through BaseItem.
    /// </summary>
    [Fact]
    public void BaseItemCanSetAndClearDocumentStructure()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            BaseItem Item = Project.AddDocument("Structured Book");

            Assert.True(Item.CanSetStructure);
            Assert.False(Item.CanClearStructure);
            Item.SetStructure(CreateChapterSceneStructure());

            Assert.True(((Document)Item).HasFolderStructure);
            Assert.True(Item.CanClearStructure);

            Item.ClearStructure();

            Assert.True(((Document)Item).IsFlatDocument);
            Assert.False(Item.CanClearStructure);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that adding a structured document requires a valid folder item graph.
    /// </summary>
    [Fact]
    public void ProjectAddStructuredDocumentRequiresValidStructure()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            FolderItem InvalidStructure = new FolderItem();

            Assert.Throws<ArgumentNullException>(() => Project.AddDocument("Structured Book", null));
            Assert.Throws<Exception>(() => Project.AddDocument("Structured Book", InvalidStructure));
            Assert.Empty(Project.Documents);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that setting a document structure immediately updates persistent buckets.
    /// </summary>
    [Fact]
    public void SetStructureUpdatesPersistentBuckets()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book");
            Assert.True(Directory.Exists(Document.TextFilesFolderPath));

            Document.SetStructure(CreateChapterSceneStructure());

            Assert.True(File.Exists(Document.StructureFilePath));
            Assert.True(Directory.Exists(Document.FoldersFolderPath));
            Assert.False(Directory.Exists(Document.TextFilesFolderPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that clearing a document structure immediately updates persistent buckets.
    /// </summary>
    [Fact]
    public void ClearStructureUpdatesPersistentBuckets()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book");
            Document.SetStructure(CreateChapterSceneStructure());
            Assert.True(Directory.Exists(Document.FoldersFolderPath));

            Document.ClearStructure();

            Assert.False(File.Exists(Document.StructureFilePath));
            Assert.True(Directory.Exists(Document.TextFilesFolderPath));
            Assert.False(Directory.Exists(Document.FoldersFolderPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that folder add commands respect leaf and non-leaf levels.
    /// </summary>
    [Fact]
    public void FolderAddCommandsRespectStructureLevel()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book");
            Document.SetStructure(CreateChapterSceneStructure());
            Folder Chapter = Document.AddFolder("Chapter One", string.Empty);
            Folder Scene = Chapter.AddFolder("Scene Group", string.Empty);

            Assert.True(Chapter.CanAddFolder);
            Assert.False(Chapter.CanAddTextFile);
            Assert.Throws<InvalidOperationException>(() => Chapter.AddTextFile("Opening Scene"));

            Assert.True(Scene.CanAddTextFile);
            Assert.False(Scene.CanAddFolder);
            Assert.Throws<InvalidOperationException>(() => Scene.AddFolder("Nested Scene", string.Empty));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that descendant traversal returns the visible item subtree.
    /// </summary>
    [Fact]
    public void GetDescendantItemsReturnsVisibleItemSubtree()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book", CreateChapterSceneStructure());
            Folder Chapter = Document.AddFolder("Chapter One", string.Empty);
            Folder Scene = Chapter.AddFolder("Scene Group", string.Empty);
            TextFile TextFileItem = Scene.AddTextFile("Opening Scene");

            List<BaseItem> ProjectItems = Project.GetDescendantItems(true);
            List<BaseItem> DocumentItems = Document.GetDescendantItems();

            Assert.Equal(5, ProjectItems.Count);
            Assert.Contains(Project, ProjectItems);
            Assert.Contains(Document, ProjectItems);
            Assert.Contains(Chapter, ProjectItems);
            Assert.Contains(Scene, ProjectItems);
            Assert.Contains(TextFileItem, ProjectItems);
            Assert.Equal(3, DocumentItems.Count);
            Assert.DoesNotContain(Document, DocumentItems);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that folder level titles are validated and trimmed.
    /// </summary>
    [Fact]
    public void FolderLevelTitleIsValidatedAndTrimmed()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book", CreateChapterSceneStructure());
            Folder Folder = Document.AddFolder("Chapter One", "  Chapter  ");

            Assert.Equal("Chapter", Folder.LevelTitle);
            Assert.Throws<Exception>(() => Document.AddFolder("Chapter Two", "Chapter-Level"));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
}
