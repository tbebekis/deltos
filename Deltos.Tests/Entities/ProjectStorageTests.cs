// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Tests.Entities;

/// <summary>
/// Tests the project storage contract.
/// </summary>
public class ProjectStorageTests
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
    /// Loads a project from a folder path.
    /// </summary>
    /// <param name="ProjectPath">The project folder path.</param>
    /// <returns>The loaded project.</returns>
    static Project LoadProject(string ProjectPath)
    {
        return Project.Open(ProjectPath);
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
    /// Tests that the project create entry point creates and saves a new project.
    /// </summary>
    [Fact]
    public void ProjectCreateCreatesAndSavesProject()
    {
        string ParentFolderPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        string ProjectPath = Path.Combine(ParentFolderPath, "Created_Project");
        DeleteFolder(ParentFolderPath);

        try
        {
            Directory.CreateDirectory(ParentFolderPath);
            Project Project = Project.Create(ParentFolderPath, "Created Project");

            Assert.Equal("Created Project", Project.Title);
            Assert.Equal(ProjectPath, Project.ProjectPath);
            Assert.True(Directory.Exists(ProjectPath));
            Assert.True(File.Exists(Path.Combine(ProjectPath, BaseItem.InfoFileName)));
            Assert.True(Directory.Exists(Project.DocumentsFolderPath));
        }
        finally
        {
            DeleteFolder(ParentFolderPath);
        }
    }
    /// <summary>
    /// Tests that the project folder path helper uses the project create naming rules.
    /// </summary>
    [Fact]
    public void GetProjectFolderPathReturnsActualProjectRootPath()
    {
        string ParentFolderPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        string ExpectedProjectPath = Path.Combine(ParentFolderPath, "Created_Project");
        DeleteFolder(ParentFolderPath);

        try
        {
            Directory.CreateDirectory(ParentFolderPath);

            string ProjectPath = Project.GetProjectFolderPath(ParentFolderPath, "Created Project");

            Assert.Equal(ExpectedProjectPath, ProjectPath);
        }
        finally
        {
            DeleteFolder(ParentFolderPath);
        }
    }
    /// <summary>
    /// Tests that the project open entry point loads an existing project.
    /// </summary>
    [Fact]
    public void ProjectOpenLoadsExistingProject()
    {
        string ParentFolderPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        string ProjectPath = Path.Combine(ParentFolderPath, "Created_Project");
        DeleteFolder(ParentFolderPath);

        try
        {
            Directory.CreateDirectory(ParentFolderPath);
            Project CreatedProject = Project.Create(ParentFolderPath, "Created Project");
            CreatedProject.AddDocument("Book");

            Project OpenedProject = Project.Open(ProjectPath);

            Assert.Equal("Created Project", OpenedProject.Title);
            Assert.Equal(ProjectPath, OpenedProject.ProjectPath);
            Assert.Single(OpenedProject.Documents);
            Assert.Equal("Book", OpenedProject.Documents[0].Title);
        }
        finally
        {
            DeleteFolder(ParentFolderPath);
        }
    }
    /// <summary>
    /// Tests that opening a project requires an absolute storage path.
    /// </summary>
    [Fact]
    public void ProjectOpenRequiresAbsoluteProjectPath()
    {
        Assert.Throws<InvalidOperationException>(() => Project.Open("RelativeProject"));
    }
    /// <summary>
    /// Tests that opening a project normalizes an absolute storage path.
    /// </summary>
    [Fact]
    public void ProjectOpenNormalizesAbsoluteProjectPath()
    {
        string ParentPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests");
        string UnusedSegment = Guid.NewGuid().ToString("N");
        string ProjectSegment = Guid.NewGuid().ToString("N");
        string ParentFolderPath = Path.Combine(ParentPath, ProjectSegment);
        string ProjectPath = Path.Combine(ParentPath, UnusedSegment, "..", ProjectSegment, "Created_Project");
        string ExpectedProjectPath = Path.GetFullPath(ProjectPath);
        DeleteFolder(ParentFolderPath);

        try
        {
            Directory.CreateDirectory(ParentFolderPath);
            Project.Create(ParentFolderPath, "Created Project");

            Project OpenedProject = Project.Open(ProjectPath);

            Assert.Equal(ExpectedProjectPath, OpenedProject.ProjectPath);
            Assert.Equal("Created Project", OpenedProject.Title);
        }
        finally
        {
            DeleteFolder(ParentFolderPath);
        }
    }
    /// <summary>
    /// Tests that creating a project allows a non-empty parent folder.
    /// </summary>
    [Fact]
    public void ProjectCreateAllowsNonEmptyParentFolder()
    {
        string ParentFolderPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        string ProjectPath = Path.Combine(ParentFolderPath, "Created_Project");
        DeleteFolder(ParentFolderPath);

        try
        {
            Directory.CreateDirectory(ParentFolderPath);
            File.WriteAllText(Path.Combine(ParentFolderPath, "Other.txt"), "Not a project.");

            Project Project = Project.Create(ParentFolderPath, "Created Project");

            Assert.Equal(ProjectPath, Project.ProjectPath);
            Assert.True(File.Exists(Path.Combine(ParentFolderPath, "Other.txt")));
            Assert.True(File.Exists(Path.Combine(ProjectPath, BaseItem.InfoFileName)));
        }
        finally
        {
            DeleteFolder(ParentFolderPath);
        }
    }
    /// <summary>
    /// Tests that creating a project rejects an existing non-empty project folder.
    /// </summary>
    [Fact]
    public void ProjectCreateRejectsExistingNonEmptyProjectFolder()
    {
        string ParentFolderPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        string ProjectPath = Path.Combine(ParentFolderPath, "Created_Project");
        DeleteFolder(ParentFolderPath);

        try
        {
            Directory.CreateDirectory(ProjectPath);
            File.WriteAllText(Path.Combine(ProjectPath, "Other.txt"), "Existing project collision.");

            Assert.Throws<InvalidOperationException>(() => Project.Create(ParentFolderPath, "Created Project"));
        }
        finally
        {
            DeleteFolder(ParentFolderPath);
        }
    }
    /// <summary>
    /// Tests that creating a project requires an existing parent folder.
    /// </summary>
    [Fact]
    public void ProjectCreateRequiresExistingParentFolder()
    {
        string ParentFolderPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        DeleteFolder(ParentFolderPath);

        Assert.Throws<InvalidOperationException>(() => Project.Create(ParentFolderPath, "Created Project"));
    }
    /// <summary>
    /// Tests that creating a project requires a valid project title.
    /// </summary>
    [Fact]
    public void ProjectCreateRequiresValidProjectTitle()
    {
        string ParentFolderPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        DeleteFolder(ParentFolderPath);

        try
        {
            Directory.CreateDirectory(ParentFolderPath);

            Assert.Throws<Exception>(() => Project.Create(ParentFolderPath, "123 Project"));
            Assert.Empty(Directory.EnumerateFileSystemEntries(ParentFolderPath));
        }
        finally
        {
            DeleteFolder(ParentFolderPath);
        }
    }
    /// <summary>
    /// Tests that creating a project requires an absolute parent folder path.
    /// </summary>
    [Fact]
    public void ProjectCreateRequiresAbsoluteParentFolderPath()
    {
        Assert.Throws<InvalidOperationException>(() => Project.Create("RelativeProject", "Created Project"));
    }
    /// <summary>
    /// Tests that creating a project normalizes an absolute parent folder path.
    /// </summary>
    [Fact]
    public void ProjectCreateNormalizesAbsoluteParentFolderPath()
    {
        string ParentPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests");
        string UnusedSegment = Guid.NewGuid().ToString("N");
        string ProjectSegment = Guid.NewGuid().ToString("N");
        string ParentFolderPath = Path.Combine(ParentPath, UnusedSegment, "..", ProjectSegment);
        string ExpectedProjectPath = Path.Combine(Path.GetFullPath(ParentFolderPath), "Created_Project");
        DeleteFolder(Path.GetFullPath(ParentFolderPath));

        try
        {
            Directory.CreateDirectory(Path.GetFullPath(ParentFolderPath));
            Project Project = Project.Create(ParentFolderPath, "Created Project");

            Assert.Equal(ExpectedProjectPath, Project.ProjectPath);
            Assert.True(Directory.Exists(ExpectedProjectPath));
            Assert.True(File.Exists(Path.Combine(ExpectedProjectPath, BaseItem.InfoFileName)));
        }
        finally
        {
            DeleteFolder(Path.GetFullPath(ParentFolderPath));
        }
    }
    /// <summary>
    /// Tests that saving a project requires a storage path.
    /// </summary>
    [Fact]
    public void ProjectSaveRequiresProjectPath()
    {
        Project Project = new Project();
        Project.Title = "Test Project";
        Project.UpdateReferences(null);

        Assert.Throws<InvalidOperationException>(() => Project.Save());
    }
    /// <summary>
    /// Tests that saving a project requires an absolute storage path.
    /// </summary>
    [Fact]
    public void ProjectSaveRequiresAbsoluteProjectPath()
    {
        Project Project = new Project();
        Project.Title = "Test Project";
        Project.ProjectPath = "RelativeProject";
        Project.UpdateReferences(null);

        Assert.Throws<InvalidOperationException>(() => Project.Save());
    }
    /// <summary>
    /// Tests that loading a project requires an existing storage path.
    /// </summary>
    [Fact]
    public void ProjectLoadRequiresExistingProjectPath()
    {
        string ProjectPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        DeleteFolder(ProjectPath);

        Project Project = new Project();
        Project.ProjectPath = ProjectPath;

        Assert.Throws<InvalidOperationException>(() => Project.Load());
    }
    /// <summary>
    /// Tests that loading a project requires a project information file.
    /// </summary>
    [Fact]
    public void ProjectLoadRequiresProjectInfoFile()
    {
        string ProjectPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        DeleteFolder(ProjectPath);

        try
        {
            Directory.CreateDirectory(ProjectPath);
            Project Project = new Project();
            Project.ProjectPath = ProjectPath;

            Assert.Throws<InvalidOperationException>(() => Project.Load());
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that loading a project rejects a root information file with an invalid item type.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsInvalidProjectInfoType()
    {
        string ProjectPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        DeleteFolder(ProjectPath);

        try
        {
            Directory.CreateDirectory(ProjectPath);
            ItemInfo Info = new ItemInfo();
            Info.Id = "InvalidId";
            Info.Title = "Invalid Project";
            Info.Type = ItemType.Document;
            Tripous.Json.SaveToFile(Info, Path.Combine(ProjectPath, BaseItem.InfoFileName));

            Project Project = new Project();
            Project.ProjectPath = ProjectPath;

            Assert.Throws<InvalidOperationException>(() => Project.Load());
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that project save creates a missing project root folder.
    /// </summary>
    [Fact]
    public void ProjectSaveCreatesMissingProjectRootFolder()
    {
        string ProjectPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        DeleteFolder(ProjectPath);

        try
        {
            Project Project = new Project();
            Project.Title = "Test Project";
            Project.ProjectPath = ProjectPath;
            Project.UpdateReferences(null);

            Project.Save();

            Assert.True(Directory.Exists(ProjectPath));
            Assert.True(File.Exists(Path.Combine(ProjectPath, BaseItem.InfoFileName)));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that project save rejects a non-empty folder that is not already a project.
    /// </summary>
    [Fact]
    public void ProjectSaveRejectsNonEmptyNonProjectFolder()
    {
        string ProjectPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        DeleteFolder(ProjectPath);

        try
        {
            Directory.CreateDirectory(ProjectPath);
            File.WriteAllText(Path.Combine(ProjectPath, "Other.txt"), "Not a project.");

            Project Project = new Project();
            Project.Title = "Test Project";
            Project.ProjectPath = ProjectPath;
            Project.UpdateReferences(null);

            Assert.Throws<InvalidOperationException>(() => Project.Save());
            Assert.False(File.Exists(Path.Combine(ProjectPath, BaseItem.InfoFileName)));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that project save rejects a folder whose information file is not a project root.
    /// </summary>
    [Fact]
    public void ProjectSaveRejectsNonProjectInfoFile()
    {
        string ProjectPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        DeleteFolder(ProjectPath);

        try
        {
            Directory.CreateDirectory(ProjectPath);
            ItemInfo Info = new ItemInfo();
            Info.Id = "DocumentInfo";
            Info.Type = ItemType.Document;
            Tripous.Json.SaveToFile(Info, Path.Combine(ProjectPath, BaseItem.InfoFileName));

            Project Project = new Project();
            Project.Title = "Test Project";
            Project.ProjectPath = ProjectPath;
            Project.UpdateReferences(null);

            Assert.Throws<InvalidOperationException>(() => Project.Save());
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that project save rejects an existing project information file without an id.
    /// </summary>
    [Fact]
    public void ProjectSaveRejectsProjectInfoWithoutId()
    {
        string ProjectPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        DeleteFolder(ProjectPath);

        try
        {
            Directory.CreateDirectory(ProjectPath);
            ItemInfo Info = new ItemInfo();
            Info.Type = ItemType.Project;
            Tripous.Json.SaveToFile(Info, Path.Combine(ProjectPath, BaseItem.InfoFileName));

            Project Project = new Project();
            Project.Title = "Test Project";
            Project.ProjectPath = ProjectPath;
            Project.UpdateReferences(null);

            Assert.Throws<InvalidOperationException>(() => Project.Save());
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that the project path remains runtime-only and is not persisted.
    /// </summary>
    [Fact]
    public void ProjectPathIsNotPersisted()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            string JsonText = File.ReadAllText(Path.Combine(ProjectPath, BaseItem.InfoFileName));

            Assert.DoesNotContain(nameof(Project.ProjectPath), JsonText);
            Assert.DoesNotContain(ProjectPath, JsonText);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that project information is persisted and restored.
    /// </summary>
    [Fact]
    public void ProjectInfoPersistsAndReloads()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            string ProjectId = Project.Id;

            Project LoadedProject = LoadProject(ProjectPath);

            Assert.Equal("Test Project", LoadedProject.Title);
            Assert.Equal(ProjectId, LoadedProject.Id);
            Assert.Equal(ItemType.Project, LoadedProject.Info.Type);
            Assert.Equal(ProjectPath, LoadedProject.ProjectPath);
            Assert.Same(LoadedProject, LoadedProject.Project);
            Assert.Null(LoadedProject.Parent);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that loading old project information without a title falls back to the project folder name.
    /// </summary>
    [Fact]
    public void ProjectInfoWithoutTitleFallsBackToProjectFolderName()
    {
        string ProjectPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", "LegacyProject");
        DeleteFolder(ProjectPath);

        try
        {
            Directory.CreateDirectory(ProjectPath);
            ItemInfo Info = new ItemInfo();
            Info.Id = "LegacyId";
            Info.Type = ItemType.Project;
            Tripous.Json.SaveToFile(Info, Path.Combine(ProjectPath, BaseItem.InfoFileName));

            Project LoadedProject = LoadProject(ProjectPath);

            Assert.Equal("LegacyProject", LoadedProject.Title);
            Assert.Equal("LegacyId", LoadedProject.Id);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that loading a project rejects duplicate child order indexes.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsDuplicateDocumentOrderIndex()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Project.AddDocument("First Book");
            string DuplicateDocumentPath = Path.Combine(Project.DocumentsFolderPath, "001._Second_Book");
            Directory.CreateDirectory(DuplicateDocumentPath);
            ItemInfo Info = new ItemInfo();
            Info.Id = "DuplicateOrderDocument";
            Info.Type = ItemType.Document;
            Tripous.Json.SaveToFile(Info, Path.Combine(DuplicateDocumentPath, BaseItem.InfoFileName));

            Assert.Throws<InvalidOperationException>(() => LoadProject(ProjectPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that loading a project rejects invalid child storage folder names.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsInvalidDocumentStorageFolderName()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Directory.CreateDirectory(Path.Combine(Project.DocumentsFolderPath, "Invalid_Document"));

            Assert.Throws<InvalidOperationException>(() => LoadProject(ProjectPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that loading a project rejects child item folders without information files.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsDocumentWithoutInfoFile()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Directory.CreateDirectory(Path.Combine(Project.DocumentsFolderPath, "001._Missing_Info_Book"));

            Assert.Throws<InvalidOperationException>(() => LoadProject(ProjectPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that loading a project rejects child item information without an item id.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsDocumentInfoWithoutId()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            string DocumentPath = Path.Combine(Project.DocumentsFolderPath, "001._Missing_Id_Book");
            Directory.CreateDirectory(DocumentPath);
            ItemInfo Info = new ItemInfo();
            Info.Type = ItemType.Document;
            Tripous.Json.SaveToFile(Info, Path.Combine(DocumentPath, BaseItem.InfoFileName));

            Assert.Throws<InvalidOperationException>(() => LoadProject(ProjectPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that loading a project rejects child item information without an item type.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsDocumentInfoWithoutType()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            string DocumentPath = Path.Combine(Project.DocumentsFolderPath, "001._Missing_Type_Book");
            Directory.CreateDirectory(DocumentPath);
            ItemInfo Info = new ItemInfo();
            Info.Id = "MissingTypeDocument";
            Tripous.Json.SaveToFile(Info, Path.Combine(DocumentPath, BaseItem.InfoFileName));

            Assert.Throws<InvalidOperationException>(() => LoadProject(ProjectPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that loading a project rejects duplicate child titles.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsDuplicateDocumentTitle()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Project.AddDocument("First Book");
            string DuplicateDocumentPath = Path.Combine(Project.DocumentsFolderPath, "002._First_Book");
            Directory.CreateDirectory(DuplicateDocumentPath);
            ItemInfo Info = new ItemInfo();
            Info.Id = "DuplicateTitleDocument";
            Info.Type = ItemType.Document;
            Tripous.Json.SaveToFile(Info, Path.Combine(DuplicateDocumentPath, BaseItem.InfoFileName));

            Assert.Throws<InvalidOperationException>(() => LoadProject(ProjectPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that loading a document rejects an invalid persisted folder structure.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsInvalidDocumentStructure()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book");
            FolderItem InvalidStructure = new FolderItem();
            InvalidStructure.Title = "Chapter-Level";
            Tripous.Json.SaveToFile(InvalidStructure, Document.StructureFilePath);

            Assert.Throws<Exception>(() => LoadProject(ProjectPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that a flat document persists text files and reloads them.
    /// </summary>
    [Fact]
    public void FlatDocumentPersistsTextFiles()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            TextFile TextFileItem = Document.AddTextFile("Opening Scene");
            TextFileItem.Text = "Opening text.";
            TextFileItem.Save();
            Project.Save();

            string DocumentPath = Path.Combine(ProjectPath, Project.DocumentsFolderName, "001._Flat_Book");
            string FilePath = Path.Combine(DocumentPath, Document.TextFilesFolderName, "001._Opening_Scene");
            Assert.True(Directory.Exists(FilePath));
            Assert.True(File.Exists(Path.Combine(FilePath, BaseItem.InfoFileName)));
            Assert.True(File.Exists(Path.Combine(FilePath, TextFile.TextFileName)));
            Assert.False(File.Exists(Path.Combine(DocumentPath, Document.StructureFileName)));

            Project LoadedProject = LoadProject(ProjectPath);
            Document LoadedDocument = LoadedProject.Documents[0];
            TextFile LoadedFile = LoadedDocument.Files[0];

            Assert.True(LoadedDocument.IsFlatDocument);
            Assert.Single(LoadedDocument.Files);
            Assert.Empty(LoadedDocument.Folders);
            Assert.Equal("Opening text.", LoadedFile.Text);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that a structured document persists folders, leaf text files, and reloads them.
    /// </summary>
    [Fact]
    public void StructuredDocumentPersistsFoldersAndLeafTextFiles()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book");
            Document.SetStructure(CreateChapterSceneStructure());
            Folder Chapter = Document.AddFolder("Chapter One", string.Empty);
            Folder Scene = Chapter.AddFolder("Scene Group", string.Empty);
            TextFile TextFileItem = Scene.AddTextFile("Opening Scene");
            TextFileItem.Text = "Scene text.";
            TextFileItem.Save();
            Project.Save();

            string DocumentPath = Path.Combine(ProjectPath, Project.DocumentsFolderName, "001._Structured_Book");
            string ScenePath = Path.Combine(DocumentPath, Document.FoldersFolderName, "001._Chapter_One", Folder.FoldersFolderName, "001._Scene_Group");
            string FilePath = Path.Combine(ScenePath, Folder.TextFilesFolderName, "001._Opening_Scene");
            Assert.True(File.Exists(Path.Combine(DocumentPath, Document.StructureFileName)));
            Assert.True(Directory.Exists(FilePath));
            Assert.False(Directory.Exists(Path.Combine(DocumentPath, Document.TextFilesFolderName)));

            Project LoadedProject = LoadProject(ProjectPath);
            Document LoadedDocument = LoadedProject.Documents[0];
            Folder LoadedChapter = LoadedDocument.Folders[0];
            Folder LoadedScene = LoadedChapter.Folders[0];
            TextFile LoadedFile = LoadedScene.Files[0];

            Assert.True(LoadedDocument.HasFolderStructure);
            Assert.Single(LoadedDocument.Folders);
            Assert.Empty(LoadedDocument.Files);
            Assert.True(LoadedScene.CanAddTextFile);
            Assert.Equal("Scene text.", LoadedFile.Text);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that RemoveChild deletes the child item folder from persistent storage.
    /// </summary>
    [Fact]
    public void RemoveChildDeletesPersistentStorage()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            TextFile TextFileItem = Document.AddTextFile("Opening Scene");
            string FileFolderPath = TextFileItem.FolderPath;

            Assert.True(Directory.Exists(FileFolderPath));

            bool Removed = Document.RemoveChild(TextFileItem);

            Assert.True(Removed);
            Assert.Empty(Document.Files);
            Assert.False(Directory.Exists(FileFolderPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
}
