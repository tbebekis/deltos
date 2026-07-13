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
            Assert.Equal(ParentFolderPath, Project.ParentFolderPath);
            Assert.True(Directory.Exists(ProjectPath));
            Assert.True(File.Exists(Path.Combine(ProjectPath, BaseItem.InfoFileName)));
            Assert.True(Directory.Exists(Project.DocumentsFolderPath));
            Assert.True(Directory.Exists(Project.ImagesFolderPath));
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
    /// Tests that the project folder name helper uses the project title encoding rules.
    /// </summary>
    [Fact]
    public void GetProjectFolderNameReturnsEncodedProjectTitle()
    {
        Assert.Equal("Created_Project", Project.GetProjectFolderName("Created Project"));
        Assert.Throws<Exception>(() => Project.GetProjectFolderName("123 Project"));
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

            Project OpenedProject = Project.Open($"  {ProjectPath}  ");

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
    /// Tests that project temporary text is saved and loaded from the temporary markdown file.
    /// </summary>
    [Fact]
    public void ProjectSaveAndOpenPersistsTempFileText()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Project.TempFileText = "Temporary project notes.";
            Project.SaveTempFile();

            Project LoadedProject = LoadProject(ProjectPath);

            Assert.Equal("Temporary project notes.", LoadedProject.TempFileText);
            Assert.True(File.Exists(Path.Combine(ProjectPath, Deltos.Project.TempFileName)));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that project images are copied to the project images bucket.
    /// </summary>
    [Fact]
    public void ProjectAddImageCopiesImageToImagesFolder()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            string SourceFilePath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N") + ".png");
            Directory.CreateDirectory(Path.GetDirectoryName(SourceFilePath));
            File.WriteAllBytes(SourceFilePath, new byte[] { 1, 2, 3 });

            string ImagePath = Project.AddImage(SourceFilePath);
            string SecondImagePath = Project.AddImage(SourceFilePath);

            Assert.Equal(Path.GetFileName(SourceFilePath), ImagePath);
            Assert.Equal(Path.GetFileNameWithoutExtension(SourceFilePath) + "-2.png", SecondImagePath);
            Assert.True(File.Exists(Path.Combine(Project.ImagesFolderPath, ImagePath)));
            Assert.True(File.Exists(Path.Combine(Project.ImagesFolderPath, SecondImagePath)));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that project notes are saved and loaded from the notes bucket.
    /// </summary>
    [Fact]
    public void ProjectSaveAndOpenPersistsProjectNotes()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Note Note = Project.AddNote("Research");
            Note.Text = "Research note text.";
            Project.Save();

            Project LoadedProject = LoadProject(ProjectPath);
            Note LoadedNote = LoadedProject.Notes[0];

            Assert.Single(LoadedProject.Notes);
            Assert.Equal("Research", LoadedNote.Title);
            Assert.Equal("Research note text.", LoadedNote.Text);
            Assert.True(File.Exists(Path.Combine(Project.NotesFolderPath, "001._Research", Deltos.Note.TextFileName)));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that project components are saved and loaded from the components bucket.
    /// </summary>
    [Fact]
    public void ProjectSaveAndOpenPersistsProjectComponents()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Component Component = new Component();
            Component.Title = "Aldrion";
            Component.Category = "Character";
            Component.Tags = "Corp; Corpers";
            Component.Aliases = "Ald; Captain";
            Component.Text = "Primary component text.";
            Component.Text2 = "Secondary component text.";
            Project.AddComponent(Component);
            Project.Save();

            Project LoadedProject = LoadProject(ProjectPath);
            Component LoadedComponent = LoadedProject.Components[0];

            Assert.Single(LoadedProject.Components);
            Assert.Equal("Aldrion", LoadedComponent.Title);
            Assert.Equal("Character", LoadedComponent.Category);
            Assert.Equal(new[] { "Corp", "Corpers" }, LoadedComponent.TagList);
            Assert.Equal(new[] { "Ald", "Captain" }, LoadedComponent.AliasList);
            Assert.Equal("Primary component text.", LoadedComponent.Text);
            Assert.Equal("Secondary component text.", LoadedComponent.Text2);
            Assert.True(File.Exists(Path.Combine(Project.ComponentsFolderPath, "Aldrion", Deltos.Component.TextFileName)));
            Assert.True(File.Exists(Path.Combine(Project.ComponentsFolderPath, "Aldrion", Deltos.Component.Text2FileName)));
        }
        finally
        {
            DeleteFolder(ProjectPath);
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
    /// Tests that creating a project rejects a file at the target project path.
    /// </summary>
    [Fact]
    public void ProjectCreateRejectsExistingProjectPathFile()
    {
        string ParentFolderPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        string ProjectPath = Path.Combine(ParentFolderPath, "Created_Project");
        DeleteFolder(ParentFolderPath);

        try
        {
            Directory.CreateDirectory(ParentFolderPath);
            File.WriteAllText(ProjectPath, "Not a folder.");

            Assert.Throws<InvalidOperationException>(() => Project.Create(ParentFolderPath, "Created Project"));
        }
        finally
        {
            DeleteFolder(ParentFolderPath);
        }
    }
    /// <summary>
    /// Tests that creating a project may reuse an existing empty project folder.
    /// </summary>
    [Fact]
    public void ProjectCreateAllowsExistingEmptyProjectFolder()
    {
        string ParentFolderPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        string ProjectPath = Path.Combine(ParentFolderPath, "Created_Project");
        DeleteFolder(ParentFolderPath);

        try
        {
            Directory.CreateDirectory(ProjectPath);

            Project Project = Project.Create(ParentFolderPath, "Created Project");

            Assert.Equal(ProjectPath, Project.ProjectPath);
            Assert.True(File.Exists(Path.Combine(ProjectPath, BaseItem.InfoFileName)));
        }
        finally
        {
            DeleteFolder(ParentFolderPath);
        }
    }
    /// <summary>
    /// Tests that creating a project trims the title before persisting it.
    /// </summary>
    [Fact]
    public void ProjectCreateTrimsProjectTitle()
    {
        string ParentFolderPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        string ProjectPath = Path.Combine(ParentFolderPath, "Created_Project");
        DeleteFolder(ParentFolderPath);

        try
        {
            Directory.CreateDirectory(ParentFolderPath);

            Project Project = Project.Create(ParentFolderPath, "  Created Project  ");
            Project LoadedProject = Project.Open(ProjectPath);

            Assert.Equal("Created Project", Project.Title);
            Assert.Equal(ProjectPath, Project.ProjectPath);
            Assert.Equal("Created Project", LoadedProject.Title);
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
    /// Tests that saving a project rejects a storage path that points to a file.
    /// </summary>
    [Fact]
    public void ProjectSaveRejectsProjectPathFile()
    {
        string ProjectPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        DeleteFolder(ProjectPath);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ProjectPath));
            File.WriteAllText(ProjectPath, "Not a folder.");

            Project Project = new Project();
            Project.Title = "Test Project";
            Project.ProjectPath = ProjectPath;
            Project.UpdateReferences(null);

            Assert.Throws<InvalidOperationException>(() => Project.Save());
        }
        finally
        {
            if (File.Exists(ProjectPath))
                File.Delete(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that child commands do not write relative storage before project save validates the path.
    /// </summary>
    [Fact]
    public void ChildCommandDoesNotPersistRelativeProjectPath()
    {
        string RelativeProjectPath = "RelativeProject";
        DeleteFolder(RelativeProjectPath);

        try
        {
            Project Project = new Project();
            Project.Title = "Test Project";
            Project.ProjectPath = RelativeProjectPath;
            Project.UpdateReferences(null);

            Project.AddDocument("Book");

            Assert.False(Directory.Exists(RelativeProjectPath));
            Assert.Throws<InvalidOperationException>(() => Project.Save());
        }
        finally
        {
            DeleteFolder(RelativeProjectPath);
        }
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
    /// Tests that project save rejects a storage path that belongs to another project id.
    /// </summary>
    [Fact]
    public void ProjectSaveRejectsDifferentExistingProjectId()
    {
        string ParentFolderPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        DeleteFolder(ParentFolderPath);

        try
        {
            Directory.CreateDirectory(ParentFolderPath);
            Project ExistingProject = Project.Create(ParentFolderPath, "Existing Project");

            Project OtherProject = new Project();
            OtherProject.Title = "Other Project";
            OtherProject.ProjectPath = ExistingProject.ProjectPath;
            OtherProject.UpdateReferences(null);

            Assert.Throws<InvalidOperationException>(() => OtherProject.Save());
        }
        finally
        {
            DeleteFolder(ParentFolderPath);
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
            Assert.DoesNotContain(nameof(Project.ParentFolderPath), JsonText);
            Assert.DoesNotContain(ProjectPath, JsonText);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that child item information does not persist a title field value.
    /// </summary>
    [Fact]
    public void ChildInfoTitleIsClearedBeforePersisting()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Book");
            Document.Info.Title = "Stale Title";
            Document.Save();

            ItemInfo Info = new ItemInfo();
            Tripous.Json.LoadFromFile(Info, Document.InfoFilePath);

            Assert.Equal(string.Empty, Info.Title);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that non-component item information clears category, tag, and alias fields before persisting.
    /// </summary>
    [Fact]
    public void NonComponentInfoClearsCategoryAndTagsBeforePersisting()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Book");
            Document.Info.Category = "World";
            Document.Info.TagList = "tag1;tag2";
            Document.Info.AliasList = "alias1;alias2";
            Document.Save();

            ItemInfo Info = new ItemInfo();
            Tripous.Json.LoadFromFile(Info, Document.InfoFilePath);

            Assert.Equal(string.Empty, Info.Category);
            Assert.Equal(string.Empty, Info.TagList);
            Assert.Equal(string.Empty, Info.AliasList);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that a null item information assignment is normalized before persisting.
    /// </summary>
    [Fact]
    public void NullInfoSetterNormalizesBeforePersisting()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Book");
            Document.Info = null;
            Document.Save();

            ItemInfo Info = new ItemInfo();
            Tripous.Json.LoadFromFile(Info, Document.InfoFilePath);

            Assert.False(string.IsNullOrWhiteSpace(Info.Id));
            Assert.Equal(ItemType.Document, Info.Type);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that item information string setters normalize null values.
    /// </summary>
    [Fact]
    public void ItemInfoStringSettersNormalizeNullValues()
    {
        ItemInfo Info = new ItemInfo();

        Info.Id = null;
        Info.Title = null;
        Info.Category = null;
        Info.TagList = null;
        Info.LevelTitle = null;

        Assert.Equal(string.Empty, Info.Id);
        Assert.Equal(string.Empty, Info.Title);
        Assert.Equal(string.Empty, Info.Category);
        Assert.Equal(string.Empty, Info.TagList);
        Assert.Equal(string.Empty, Info.LevelTitle);
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
    /// Tests that item ids are trimmed before they are persisted.
    /// </summary>
    [Fact]
    public void ItemIdIsTrimmedBeforePersisting()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Book");
            Document.Id = "  DocumentId  ";
            Document.Save();

            Project LoadedProject = LoadProject(ProjectPath);

            Assert.Equal("DocumentId", Document.Id);
            Assert.Equal("DocumentId", LoadedProject.Documents[0].Id);
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
        string ProjectPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", "Legacy_Project");
        DeleteFolder(ProjectPath);

        try
        {
            Directory.CreateDirectory(ProjectPath);
            ItemInfo Info = new ItemInfo();
            Info.Id = "LegacyId";
            Info.Type = ItemType.Project;
            Tripous.Json.SaveToFile(Info, Path.Combine(ProjectPath, BaseItem.InfoFileName));

            Project LoadedProject = LoadProject(ProjectPath);

            Assert.Equal("Legacy Project", LoadedProject.Title);
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
    /// Tests that loading a project rejects child order gaps.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsDocumentOrderGap()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Project.AddDocument("First Book");
            string GapDocumentPath = Path.Combine(Project.DocumentsFolderPath, "003._Third_Book");
            Directory.CreateDirectory(GapDocumentPath);
            ItemInfo Info = new ItemInfo();
            Info.Id = "GapDocument";
            Info.Type = ItemType.Document;
            Tripous.Json.SaveToFile(Info, Path.Combine(GapDocumentPath, BaseItem.InfoFileName));

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
    /// Tests that loading a project rejects files inside storage buckets.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsFileInsideDocumentsBucket()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            File.WriteAllText(Path.Combine(Project.DocumentsFolderPath, "Stray.txt"), "Invalid bucket file.");

            Assert.Throws<InvalidOperationException>(() => LoadProject(ProjectPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that loading a structured document rejects a non-empty text files bucket.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsStructuredDocumentTextFilesBucket()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book", CreateChapterSceneStructure());
            Directory.CreateDirectory(Document.TextFilesFolderPath);
            File.WriteAllText(Path.Combine(Document.TextFilesFolderPath, "Stray.txt"), "Invalid structured document bucket.");

            Assert.Throws<InvalidOperationException>(() => LoadProject(ProjectPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that loading a flat document rejects a non-empty folders bucket.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsFlatDocumentFoldersBucket()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            Directory.CreateDirectory(Document.FoldersFolderPath);
            File.WriteAllText(Path.Combine(Document.FoldersFolderPath, "Stray.txt"), "Invalid flat document bucket.");

            Assert.Throws<InvalidOperationException>(() => LoadProject(ProjectPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that loading a non-leaf folder rejects a non-empty text files bucket.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsNonLeafFolderTextFilesBucket()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book", CreateChapterSceneStructure());
            Folder Chapter = Document.AddFolder("Chapter One", string.Empty);
            Directory.CreateDirectory(Chapter.TextFilesFolderPath);
            File.WriteAllText(Path.Combine(Chapter.TextFilesFolderPath, "Stray.txt"), "Invalid non-leaf folder bucket.");

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
    /// Tests that loading a project rejects document information marked as folder.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsDocumentInfoWithFolderFlag()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            Document.Info.IsFolder = true;
            Tripous.Json.SaveToFile(Document.Info, Document.InfoFilePath);

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
    /// Tests that loading a project rejects duplicate child item ids.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsDuplicateDocumentId()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("First Book");
            string DuplicateDocumentPath = Path.Combine(Project.DocumentsFolderPath, "002._Second_Book");
            Directory.CreateDirectory(DuplicateDocumentPath);
            ItemInfo Info = new ItemInfo();
            Info.Id = Document.Id;
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
    /// Tests that loading a project rejects duplicate ids across the whole item tree.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsDuplicateTreeItemId()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            TextFile TextFileItem = Document.AddTextFile("Opening Scene");
            TextFileItem.Id = Document.Id;
            TextFileItem.Save();

            Assert.Throws<InvalidOperationException>(() => LoadProject(ProjectPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that saving a project rejects duplicate ids across the runtime item tree.
    /// </summary>
    [Fact]
    public void ProjectSaveRejectsDuplicateTreeItemId()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            TextFile TextFileItem = Document.AddTextFile("Opening Scene");
            TextFileItem.Id = Document.Id;

            Assert.Throws<InvalidOperationException>(() => Project.Save());
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
            File.WriteAllText(Document.StructureFilePath, "{\"Title\":\"Chapter-Level\"}");

            Assert.ThrowsAny<Exception>(() => LoadProject(ProjectPath));
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
            Document.Synopsis = "Flat document synopsis.";
            TextFile TextFileItem = Document.AddTextFile("Opening Scene");
            TextFileItem.Text = "Opening text.";
            TextFileItem.Synopsis = "Opening synopsis.";
            TextFileItem.Save();
            Project.Save();

            string DocumentPath = Path.Combine(ProjectPath, Project.DocumentsFolderName, "001._Flat_Book");
            string FilePath = Path.Combine(DocumentPath, Document.TextFilesFolderName, "001._Opening_Scene");
            Assert.True(Directory.Exists(FilePath));
            Assert.True(File.Exists(Path.Combine(FilePath, BaseItem.InfoFileName)));
            Assert.True(File.Exists(Path.Combine(FilePath, TextFile.TextFileName)));
            Assert.True(File.Exists(Path.Combine(FilePath, TextFile.SynopsisFileName)));
            Assert.True(File.Exists(Path.Combine(DocumentPath, Document.SynopsisFileName)));
            Assert.False(File.Exists(Path.Combine(DocumentPath, Document.StructureFileName)));

            Project LoadedProject = LoadProject(ProjectPath);
            Document LoadedDocument = LoadedProject.Documents[0];
            TextFile LoadedFile = LoadedDocument.Files[0];

            Assert.True(LoadedDocument.IsFlatDocument);
            Assert.Single(LoadedDocument.Files);
            Assert.Empty(LoadedDocument.Folders);
            Assert.Equal("Flat document synopsis.", LoadedDocument.Synopsis);
            Assert.Equal("Opening text.", LoadedFile.Text);
            Assert.Equal("Opening synopsis.", LoadedFile.Synopsis);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that null text file content is persisted as empty text.
    /// </summary>
    [Fact]
    public void TextFileSavePersistsNullContentAsEmptyText()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            TextFile TextFileItem = Document.AddTextFile("Opening Scene");
            Document.Synopsis = null;
            TextFileItem.Text = null;
            TextFileItem.Text2 = null;
            TextFileItem.Synopsis = null;
            TextFileItem.Draft = null;
            Document.Save();
            TextFileItem.Save();

            Assert.Equal(string.Empty, Document.Synopsis);
            Assert.Equal(string.Empty, TextFileItem.Text);
            Assert.Equal(string.Empty, TextFileItem.Text2);
            Assert.Equal(string.Empty, TextFileItem.Synopsis);
            Assert.Equal(string.Empty, TextFileItem.Draft);

            Project LoadedProject = LoadProject(ProjectPath);
            Document LoadedDocument = LoadedProject.Documents[0];
            TextFile LoadedFile = LoadedProject.Documents[0].Files[0];

            Assert.Equal(string.Empty, LoadedDocument.Synopsis);
            Assert.Equal(string.Empty, LoadedFile.Text);
            Assert.Equal(string.Empty, LoadedFile.Text2);
            Assert.Equal(string.Empty, LoadedFile.Synopsis);
            Assert.Equal(string.Empty, LoadedFile.Draft);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that loading a text file requires the primary markdown content file.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsTextFileWithoutPrimaryContentFile()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            TextFile TextFileItem = Document.AddTextFile("Opening Scene");
            File.Delete(TextFileItem.TextFilePath);

            Assert.Throws<InvalidOperationException>(() => LoadProject(ProjectPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that loading a text file rejects unknown files in its storage folder.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsTextFileUnknownStorageFile()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            TextFile TextFileItem = Document.AddTextFile("Opening Scene");
            File.WriteAllText(Path.Combine(TextFileItem.FolderPath, "Unknown.txt"), "Unknown content.");

            Assert.Throws<InvalidOperationException>(() => LoadProject(ProjectPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that loading a text file rejects child folders in its storage folder.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsTextFileChildFolder()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            TextFile TextFileItem = Document.AddTextFile("Opening Scene");
            Directory.CreateDirectory(Path.Combine(TextFileItem.FolderPath, "ChildFolder"));

            Assert.Throws<InvalidOperationException>(() => LoadProject(ProjectPath));
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
    /// Tests that saving a folder requires a non-empty valid level title.
    /// </summary>
    [Fact]
    public void FolderSaveRequiresValidLevelTitle()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book", CreateChapterSceneStructure());
            Folder Folder = Document.AddFolder("Chapter One", string.Empty);
            Folder.LevelTitle = string.Empty;

            Assert.Throws<Exception>(() => Folder.Save());
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that loading a project rejects folder information not marked as folder.
    /// </summary>
    [Fact]
    public void ProjectLoadRejectsFolderInfoWithoutFolderFlag()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book", CreateChapterSceneStructure());
            Folder Folder = Document.AddFolder("Chapter One", string.Empty);
            Folder.Info.IsFolder = false;
            Tripous.Json.SaveToFile(Folder.Info, Folder.InfoFilePath);

            Assert.Throws<InvalidOperationException>(() => LoadProject(ProjectPath));
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
