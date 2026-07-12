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
            Assert.False(Document.CanContainDocuments);
            Assert.False(Document.CanAddDocument);
            Assert.False(TextFileItem.CanContainDocuments);
            Assert.False(TextFileItem.CanAddDocument);
            Assert.Throws<InvalidOperationException>(() => Document.AddDocument("Nested Book"));
            Assert.True(Directory.Exists(Document.FolderPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
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
            FolderItem InvalidStructure = new FolderItem();
            InvalidStructure.Title = "Chapter-Level";

            Assert.Throws<ArgumentNullException>(() => Document.SetStructure(null));
            Assert.Throws<Exception>(() => Document.SetStructure(EmptyStructure));
            Assert.Throws<Exception>(() => Document.SetStructure(InvalidStructure));
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
}
