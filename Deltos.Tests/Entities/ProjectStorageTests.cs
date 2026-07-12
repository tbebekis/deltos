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
        ProjectPath = Path.Combine(Path.GetTempPath(), "Deltos.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(ProjectPath);

        Project Result = new Project();
        Result.Title = "Test Project";
        Result.ProjectPath = ProjectPath;
        Result.UpdateReferences(null);
        Result.Save();
        return Result;
    }
    /// <summary>
    /// Loads a project from a folder path.
    /// </summary>
    /// <param name="ProjectPath">The project folder path.</param>
    /// <returns>The loaded project.</returns>
    static Project LoadProject(string ProjectPath)
    {
        Project Result = new Project();
        Result.ProjectPath = ProjectPath;
        Result.Load();
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
