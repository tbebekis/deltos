// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Tests.Entities;

/// <summary>
/// Tests entity commands that update persistent storage.
/// </summary>
public class ItemCommandTests
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
    /// Adds placeholder folders to a folder.
    /// </summary>
    /// <param name="Folder">The parent folder.</param>
    /// <param name="Count">The item count.</param>
    static void AddPlaceholderFolders(Folder Folder, int Count)
    {
        for (int Index = 0; Index < Count; Index++)
        {
            Folder ChildFolder = new Folder();
            ChildFolder.Title = $"Scene {Index + 1}";
            Folder.Folders.Add(ChildFolder);
        }
    }
    /// <summary>
    /// Adds placeholder text files to a folder.
    /// </summary>
    /// <param name="Folder">The parent folder.</param>
    /// <param name="Count">The item count.</param>
    static void AddPlaceholderTextFiles(Folder Folder, int Count)
    {
        for (int Index = 0; Index < Count; Index++)
        {
            TextFile TextFile = new TextFile();
            TextFile.Title = $"Text File {Index + 1}";
            Folder.Files.Add(TextFile);
        }
    }

    // ● public
    /// <summary>
    /// Tests that renaming an item moves its persistent folder.
    /// </summary>
    [Fact]
    public void RenameMovesPersistentFolder()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            TextFile TextFileItem = Document.AddTextFile("Opening Scene");
            string OldFolderPath = TextFileItem.FolderPath;

            TextFileItem.Rename("Renamed Scene");
            string NewFolderPath = TextFileItem.FolderPath;

            Assert.False(Directory.Exists(OldFolderPath));
            Assert.True(Directory.Exists(NewFolderPath));
            Assert.Equal("001._Renamed_Scene", Path.GetFileName(NewFolderPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that renaming to the same trimmed title leaves persistent storage in place.
    /// </summary>
    [Fact]
    public void RenameToSameTrimmedTitleKeepsPersistentFolder()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            TextFile TextFileItem = Document.AddTextFile("Opening Scene");
            string FolderPath = TextFileItem.FolderPath;

            TextFileItem.Rename("  Opening Scene  ");

            Assert.Equal("Opening Scene", TextFileItem.Title);
            Assert.Equal(FolderPath, TextFileItem.FolderPath);
            Assert.True(Directory.Exists(FolderPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that item titles are trimmed before storage names are computed.
    /// </summary>
    [Fact]
    public void ItemTitleIsTrimmedBeforeStorageNameIsComputed()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("  Flat Book  ");

            Assert.Equal("Flat Book", Document.Title);
            Assert.Equal("001._Flat_Book", Document.StorageName);
            Assert.True(Directory.Exists(Path.Combine(Project.DocumentsFolderPath, "001._Flat_Book")));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that renaming a project persists the new project title.
    /// </summary>
    [Fact]
    public void ProjectRenamePersistsProjectInfo()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Project.Rename("Renamed Project");

            Project OpenedProject = Project.Open(ProjectPath);

            Assert.Equal("Renamed Project", Project.Title);
            Assert.Equal("Renamed Project", OpenedProject.Title);
            Assert.True(File.Exists(Path.Combine(ProjectPath, BaseItem.InfoFileName)));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that project rename rejects storage that belongs to another project id.
    /// </summary>
    [Fact]
    public void ProjectRenameRejectsDifferentExistingProjectId()
    {
        string ProjectPath;
        Project ExistingProject = CreateProject(out ProjectPath);

        try
        {
            Project Project = new Project();
            Project.Title = "Other Project";
            Project.ProjectPath = ExistingProject.ProjectPath;
            Project.UpdateReferences(null);

            Assert.Throws<InvalidOperationException>(() => Project.Rename("Renamed Project"));
            Assert.Equal("Other Project", Project.Title);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that moving a text file inside the same parent renumbers persistent folders.
    /// </summary>
    [Fact]
    public void SameParentTextFileMoveRenumbersPersistentFolders()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            TextFile First = Document.AddTextFile("First Scene");
            TextFile Second = Document.AddTextFile("Second Scene");

            Assert.True(Second.CanMove(true));
            bool Moved = Second.Move(true);

            Assert.True(Moved);
            Assert.Equal(1, Second.OrderIndex);
            Assert.Equal(2, First.OrderIndex);
            Assert.True(Directory.Exists(Path.Combine(Document.TextFilesFolderPath, "001._Second_Scene")));
            Assert.True(Directory.Exists(Path.Combine(Document.TextFilesFolderPath, "002._First_Scene")));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that moving a folder inside the same parent renumbers persistent folders.
    /// </summary>
    [Fact]
    public void SameParentFolderMoveRenumbersPersistentFolders()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book");
            Document.SetStructure(CreateChapterSceneStructure());
            Folder First = Document.AddFolder("First Chapter", string.Empty);
            Folder Second = Document.AddFolder("Second Chapter", string.Empty);

            Assert.True(Second.CanMove(true));
            bool Moved = Second.Move(true);

            Assert.True(Moved);
            Assert.Equal(1, Second.OrderIndex);
            Assert.Equal(2, First.OrderIndex);
            Assert.True(Directory.Exists(Path.Combine(Document.FoldersFolderPath, "001._Second_Chapter")));
            Assert.True(Directory.Exists(Path.Combine(Document.FoldersFolderPath, "002._First_Chapter")));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that moving a document inside the project renumbers persistent folders.
    /// </summary>
    [Fact]
    public void SameParentDocumentMoveRenumbersPersistentFolders()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document First = Project.AddDocument("First Book");
            Document Second = Project.AddDocument("Second Book");

            Assert.True(Second.CanMove(true));
            bool Moved = Second.Move(true);

            Assert.True(Moved);
            Assert.Equal(1, Second.OrderIndex);
            Assert.Equal(2, First.OrderIndex);
            Assert.True(Directory.Exists(Path.Combine(Project.DocumentsFolderPath, "001._Second_Book")));
            Assert.True(Directory.Exists(Path.Combine(Project.DocumentsFolderPath, "002._First_Book")));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that Project.MoveDocument renumbers persistent folders.
    /// </summary>
    [Fact]
    public void ProjectMoveDocumentRenumbersPersistentFolders()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document First = Project.AddDocument("First Book");
            Document Second = Project.AddDocument("Second Book");

            bool Moved = Project.MoveDocument(Second, 1);

            Assert.True(Moved);
            Assert.Equal(1, Second.OrderIndex);
            Assert.Equal(2, First.OrderIndex);
            Assert.True(Directory.Exists(Path.Combine(Project.DocumentsFolderPath, "001._Second_Book")));
            Assert.True(Directory.Exists(Path.Combine(Project.DocumentsFolderPath, "002._First_Book")));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that Document.MoveFolder renumbers persistent folders.
    /// </summary>
    [Fact]
    public void DocumentMoveFolderRenumbersPersistentFolders()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book");
            Document.SetStructure(CreateChapterSceneStructure());
            Folder First = Document.AddFolder("First Chapter", string.Empty);
            Folder Second = Document.AddFolder("Second Chapter", string.Empty);

            bool Moved = Document.MoveFolder(Second, 1);

            Assert.True(Moved);
            Assert.Equal(1, Second.OrderIndex);
            Assert.Equal(2, First.OrderIndex);
            Assert.True(Directory.Exists(Path.Combine(Document.FoldersFolderPath, "001._Second_Chapter")));
            Assert.True(Directory.Exists(Path.Combine(Document.FoldersFolderPath, "002._First_Chapter")));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that Folder.MoveFolder renumbers persistent folders.
    /// </summary>
    [Fact]
    public void FolderMoveFolderRenumbersPersistentFolders()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book");
            Document.SetStructure(CreateChapterSceneStructure());
            Folder Chapter = Document.AddFolder("Chapter One", string.Empty);
            Folder First = Chapter.AddFolder("First Scene Group", string.Empty);
            Folder Second = Chapter.AddFolder("Second Scene Group", string.Empty);

            bool Moved = Chapter.MoveFolder(Second, 1);

            Assert.True(Moved);
            Assert.Equal(1, Second.OrderIndex);
            Assert.Equal(2, First.OrderIndex);
            Assert.True(Directory.Exists(Path.Combine(Chapter.FoldersFolderPath, "001._Second_Scene_Group")));
            Assert.True(Directory.Exists(Path.Combine(Chapter.FoldersFolderPath, "002._First_Scene_Group")));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that Document.MoveTextFile renumbers persistent folders.
    /// </summary>
    [Fact]
    public void DocumentMoveTextFileRenumbersPersistentFolders()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            TextFile First = Document.AddTextFile("First Scene");
            TextFile Second = Document.AddTextFile("Second Scene");

            bool Moved = Document.MoveTextFile(Second, 1);

            Assert.True(Moved);
            Assert.Equal(1, Second.OrderIndex);
            Assert.Equal(2, First.OrderIndex);
            Assert.True(Directory.Exists(Path.Combine(Document.TextFilesFolderPath, "001._Second_Scene")));
            Assert.True(Directory.Exists(Path.Combine(Document.TextFilesFolderPath, "002._First_Scene")));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that Folder.MoveTextFile renumbers persistent folders.
    /// </summary>
    [Fact]
    public void FolderMoveTextFileRenumbersPersistentFolders()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book");
            Document.SetStructure(CreateChapterSceneStructure());
            Folder Chapter = Document.AddFolder("Chapter One", string.Empty);
            Folder Scene = Chapter.AddFolder("Scene Group", string.Empty);
            TextFile First = Scene.AddTextFile("First Scene");
            TextFile Second = Scene.AddTextFile("Second Scene");

            bool Moved = Scene.MoveTextFile(Second, 1);

            Assert.True(Moved);
            Assert.Equal(1, Second.OrderIndex);
            Assert.Equal(2, First.OrderIndex);
            Assert.True(Directory.Exists(Path.Combine(Scene.TextFilesFolderPath, "001._Second_Scene")));
            Assert.True(Directory.Exists(Path.Combine(Scene.TextFilesFolderPath, "002._First_Scene")));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests indexed move boundary behavior.
    /// </summary>
    [Fact]
    public void IndexedMoveCommandsHandleSamePositionAndOutOfRangeIndex()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document First = Project.AddDocument("First Book");
            Document Second = Project.AddDocument("Second Book");

            Assert.False(Project.MoveDocument(First, 1));
            Assert.Equal(1, First.OrderIndex);
            Assert.Equal(2, Second.OrderIndex);
            Assert.Throws<ArgumentOutOfRangeException>(() => Project.MoveDocument(First, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => Project.MoveDocument(First, 3));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that indexed move commands reject an item that does not belong to the container.
    /// </summary>
    [Fact]
    public void IndexedMoveCommandsRejectForeignItem()
    {
        string ProjectPath;
        string OtherProjectPath;
        Project Project = CreateProject(out ProjectPath);
        Project OtherProject = CreateProject(out OtherProjectPath);

        try
        {
            Document First = Project.AddDocument("First Book");
            Document Foreign = OtherProject.AddDocument("Foreign Book");
            string FirstFolderPath = First.FolderPath;
            string ForeignFolderPath = Foreign.FolderPath;

            Assert.Throws<InvalidOperationException>(() => Project.MoveDocument(Foreign, 1));

            Assert.Single(Project.Documents);
            Assert.Single(OtherProject.Documents);
            Assert.True(Directory.Exists(FirstFolderPath));
            Assert.True(Directory.Exists(ForeignFolderPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
            DeleteFolder(OtherProjectPath);
        }
    }
    /// <summary>
    /// Tests that moving a text file to the previous leaf folder updates memory and persistent storage.
    /// </summary>
    [Fact]
    public void CrossParentTextFileMoveUpdatesPersistentStorage()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book");
            Document.SetStructure(CreateChapterSceneStructure());
            Folder Chapter = Document.AddFolder("Chapter One", string.Empty);
            Folder FirstScene = Chapter.AddFolder("First Scene Group", string.Empty);
            Folder SecondScene = Chapter.AddFolder("Second Scene Group", string.Empty);
            TextFile Existing = FirstScene.AddTextFile("Existing Scene");
            TextFile Moving = SecondScene.AddTextFile("Moving Scene");
            string OldFolderPath = Moving.FolderPath;

            Assert.True(Moving.CanMove(true));
            bool Moved = Moving.Move(true);

            Assert.True(Moved);
            Assert.False(Directory.Exists(OldFolderPath));
            Assert.Same(FirstScene, Moving.Folder);
            Assert.Empty(SecondScene.Files);
            Assert.Equal(1, Existing.OrderIndex);
            Assert.Equal(2, Moving.OrderIndex);
            Assert.True(Directory.Exists(Path.Combine(FirstScene.TextFilesFolderPath, "001._Existing_Scene")));
            Assert.True(Directory.Exists(Path.Combine(FirstScene.TextFilesFolderPath, "002._Moving_Scene")));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that moving a folder to the previous same-level parent updates memory and persistent storage.
    /// </summary>
    [Fact]
    public void CrossParentFolderMoveUpdatesPersistentStorage()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book");
            Document.SetStructure(CreateChapterSceneStructure());
            Folder FirstChapter = Document.AddFolder("First Chapter", string.Empty);
            Folder SecondChapter = Document.AddFolder("Second Chapter", string.Empty);
            Folder ExistingScene = FirstChapter.AddFolder("Existing Scene", string.Empty);
            Folder MovingScene = SecondChapter.AddFolder("Moving Scene", string.Empty);
            string OldFolderPath = MovingScene.FolderPath;

            Assert.True(MovingScene.CanMove(true));
            bool Moved = MovingScene.Move(true);

            Assert.True(Moved);
            Assert.False(Directory.Exists(OldFolderPath));
            Assert.Same(FirstChapter, MovingScene.Parent);
            Assert.Empty(SecondChapter.Folders);
            Assert.Equal(1, ExistingScene.OrderIndex);
            Assert.Equal(2, MovingScene.OrderIndex);
            Assert.True(Directory.Exists(Path.Combine(FirstChapter.FoldersFolderPath, "001._Existing_Scene")));
            Assert.True(Directory.Exists(Path.Combine(FirstChapter.FoldersFolderPath, "002._Moving_Scene")));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that moving a text file to a full adjacent parent is rejected before mutation.
    /// </summary>
    [Fact]
    public void CrossParentTextFileMoveRejectsFullTargetParent()
    {
        Project Project = new Project();
        Project.Title = "Test Project";
        Document Document = Project.AddDocument("Structured Book", CreateChapterSceneStructure());
        Folder Chapter = Document.AddFolder("Chapter One", string.Empty);
        Folder FirstScene = Chapter.AddFolder("First Scene Group", string.Empty);
        Folder SecondScene = Chapter.AddFolder("Second Scene Group", string.Empty);
        AddPlaceholderTextFiles(FirstScene, BaseItem.MaxOrderIndex);
        TextFile Moving = SecondScene.AddTextFile("Moving Scene");
        Document.UpdateReferences(Project);

        Assert.False(Moving.CanMove(true));
        Assert.False(Moving.Move(true));
        Assert.Same(SecondScene, Moving.Parent);
        Assert.Equal(BaseItem.MaxOrderIndex, FirstScene.Files.Count);
        Assert.Single(SecondScene.Files);
    }
    /// <summary>
    /// Tests that moving a folder to a full adjacent parent is rejected before mutation.
    /// </summary>
    [Fact]
    public void CrossParentFolderMoveRejectsFullTargetParent()
    {
        Project Project = new Project();
        Project.Title = "Test Project";
        Document Document = Project.AddDocument("Structured Book", CreateChapterSceneStructure());
        Folder FirstChapter = Document.AddFolder("First Chapter", string.Empty);
        Folder SecondChapter = Document.AddFolder("Second Chapter", string.Empty);
        AddPlaceholderFolders(FirstChapter, BaseItem.MaxOrderIndex);
        Folder Moving = SecondChapter.AddFolder("Moving Scene", string.Empty);
        Document.UpdateReferences(Project);

        Assert.False(Moving.CanMove(true));
        Assert.False(Moving.Move(true));
        Assert.Same(SecondChapter, Moving.Parent);
        Assert.Equal(BaseItem.MaxOrderIndex, FirstChapter.Folders.Count);
        Assert.Single(SecondChapter.Folders);
    }
    /// <summary>
    /// Tests that adding a duplicate text file title is rejected.
    /// </summary>
    [Fact]
    public void AddDuplicateTitleIsRejected()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            Document.AddTextFile("Opening Scene");

            Assert.Throws<InvalidOperationException>(() => Document.AddTextFile("Opening Scene"));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that duplicate title checks use trimmed titles.
    /// </summary>
    [Fact]
    public void AddDuplicateTitleWithExtraSpacesIsRejected()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            Document.AddTextFile("Opening Scene");

            Assert.Throws<InvalidOperationException>(() => Document.AddTextFile("  Opening Scene  "));
            Assert.Single(Document.Files);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that adding a duplicate document title is rejected.
    /// </summary>
    [Fact]
    public void AddDuplicateDocumentTitleIsRejected()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Project.AddDocument("Book");

            Assert.Throws<InvalidOperationException>(() => Project.AddDocument("Book"));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that renaming to a duplicate title is rejected and keeps memory and storage unchanged.
    /// </summary>
    [Fact]
    public void RenameDuplicateTitleIsRejectedAndRolledBack()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            TextFile First = Document.AddTextFile("First Scene");
            TextFile Second = Document.AddTextFile("Second Scene");
            string OldFolderPath = Second.FolderPath;

            Assert.Throws<InvalidOperationException>(() => Second.Rename("First Scene"));

            Assert.Equal("Second Scene", Second.Title);
            Assert.True(Directory.Exists(OldFolderPath));
            Assert.True(Directory.Exists(First.FolderPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that cross-parent moves reject duplicate titles in the target parent.
    /// </summary>
    [Fact]
    public void CrossParentMoveDuplicateTitleIsRejected()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book");
            Document.SetStructure(CreateChapterSceneStructure());
            Folder Chapter = Document.AddFolder("Chapter One", string.Empty);
            Folder FirstScene = Chapter.AddFolder("First Scene Group", string.Empty);
            Folder SecondScene = Chapter.AddFolder("Second Scene Group", string.Empty);
            FirstScene.AddTextFile("Shared Scene");
            TextFile Moving = SecondScene.AddTextFile("Shared Scene");
            string OldFolderPath = Moving.FolderPath;

            Assert.False(Moving.CanMove(true));
            Assert.False(Moving.Move(true));

            Assert.Same(SecondScene, Moving.Parent);
            Assert.Single(FirstScene.Files);
            Assert.Single(SecondScene.Files);
            Assert.True(Directory.Exists(OldFolderPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that removing a document deletes its persistent folder.
    /// </summary>
    [Fact]
    public void RemoveDocumentDeletesPersistentStorage()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document First = Project.AddDocument("First Book");
            Document Second = Project.AddDocument("Second Book");
            string FirstFolderPath = First.FolderPath;

            Assert.True(Directory.Exists(FirstFolderPath));

            bool Removed = Project.RemoveChild(First);

            Assert.True(Removed);
            Assert.Single(Project.Documents);
            Assert.Same(Second, Project.Documents[0]);
            Assert.Equal(1, Second.OrderIndex);
            Assert.False(Directory.Exists(FirstFolderPath));
            Assert.True(Directory.Exists(Path.Combine(Project.DocumentsFolderPath, "001._Second_Book")));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that RemoveDocument deletes memory and persistent storage.
    /// </summary>
    [Fact]
    public void RemoveDocumentCommandDeletesPersistentStorage()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Book");
            string DocumentFolderPath = Document.FolderPath;

            bool Removed = Project.RemoveDocument(Document);

            Assert.True(Removed);
            Assert.Empty(Project.Documents);
            Assert.False(Directory.Exists(DocumentFolderPath));
            Assert.False(Document.CanDelete());
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that Document.RemoveTextFile deletes memory and persistent storage.
    /// </summary>
    [Fact]
    public void DocumentRemoveTextFileCommandDeletesPersistentStorage()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            TextFile TextFileItem = Document.AddTextFile("Opening Scene");
            string FileFolderPath = TextFileItem.FolderPath;

            bool Removed = Document.RemoveTextFile(TextFileItem);

            Assert.True(Removed);
            Assert.Empty(Document.Files);
            Assert.False(Directory.Exists(FileFolderPath));
            Assert.False(TextFileItem.CanDelete());
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that Document.RemoveFolder deletes memory and persistent storage.
    /// </summary>
    [Fact]
    public void DocumentRemoveFolderCommandDeletesPersistentStorage()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book");
            Document.SetStructure(CreateChapterSceneStructure());
            Folder FolderItem = Document.AddFolder("Chapter One", string.Empty);
            string FolderPath = FolderItem.FolderPath;

            bool Removed = Document.RemoveFolder(FolderItem);

            Assert.True(Removed);
            Assert.Empty(Document.Folders);
            Assert.False(Directory.Exists(FolderPath));
            Assert.False(FolderItem.CanDelete());
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that removing the first folder deletes storage before renumbering siblings.
    /// </summary>
    [Fact]
    public void DocumentRemoveFirstFolderRenumbersSiblingStorage()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book", CreateChapterSceneStructure());
            Folder First = Document.AddFolder("First Chapter", string.Empty);
            Folder Second = Document.AddFolder("Second Chapter", string.Empty);
            string FirstFolderPath = First.FolderPath;

            bool Removed = Document.RemoveFolder(First);

            Assert.True(Removed);
            Assert.False(Directory.Exists(FirstFolderPath));
            Assert.Single(Document.Folders);
            Assert.Same(Second, Document.Folders[0]);
            Assert.Equal(1, Second.OrderIndex);
            Assert.True(Directory.Exists(Path.Combine(Document.FoldersFolderPath, "001._Second_Chapter")));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that Folder.RemoveTextFile deletes memory and persistent storage.
    /// </summary>
    [Fact]
    public void FolderRemoveTextFileCommandDeletesPersistentStorage()
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
            string FileFolderPath = TextFileItem.FolderPath;

            bool Removed = Scene.RemoveTextFile(TextFileItem);

            Assert.True(Removed);
            Assert.Empty(Scene.Files);
            Assert.False(Directory.Exists(FileFolderPath));
            Assert.False(TextFileItem.CanDelete());
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that removing the first text file deletes storage before renumbering siblings.
    /// </summary>
    [Fact]
    public void FolderRemoveFirstTextFileRenumbersSiblingStorage()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Structured Book", CreateChapterSceneStructure());
            Folder Chapter = Document.AddFolder("Chapter One", string.Empty);
            Folder Scene = Chapter.AddFolder("Scene Group", string.Empty);
            TextFile First = Scene.AddTextFile("First Scene");
            TextFile Second = Scene.AddTextFile("Second Scene");
            string FirstFolderPath = First.FolderPath;

            bool Removed = Scene.RemoveTextFile(First);

            Assert.True(Removed);
            Assert.False(Directory.Exists(FirstFolderPath));
            Assert.Single(Scene.Files);
            Assert.Same(Second, Scene.Files[0]);
            Assert.Equal(1, Second.OrderIndex);
            Assert.True(Directory.Exists(Path.Combine(Scene.TextFilesFolderPath, "001._Second_Scene")));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests rename and delete capability helpers for project items.
    /// </summary>
    [Fact]
    public void ProjectRenameAndDeleteCapabilitiesAreGuarded()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Book");
            TextFile TextFileItem = Document.AddTextFile("Opening Scene");

            Assert.True(Project.CanRename());
            Assert.False(Project.CanDelete());
            Assert.False(Project.DeleteFromParent());
            Assert.Throws<InvalidOperationException>(() => Project.Delete());

            Assert.True(Document.CanRename());
            Assert.True(Document.CanDelete());
            Assert.True(TextFileItem.CanRename());
            Assert.True(TextFileItem.CanDelete());
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that DeleteFromParent deletes the selected item through its parent command.
    /// </summary>
    [Fact]
    public void DeleteFromParentDeletesPersistentStorage()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            TextFile TextFileItem = Document.AddTextFile("Opening Scene");
            string FileFolderPath = TextFileItem.FolderPath;

            bool Deleted = TextFileItem.DeleteFromParent();

            Assert.True(Deleted);
            Assert.Empty(Document.Files);
            Assert.False(Directory.Exists(FileFolderPath));
            Assert.False(TextFileItem.CanDelete());
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that DeleteFromParent returns false for a detached item.
    /// </summary>
    [Fact]
    public void DeleteFromParentReturnsFalseForDetachedItem()
    {
        TextFile TextFileItem = new TextFile();
        TextFileItem.Title = "Opening Scene";

        bool Deleted = TextFileItem.DeleteFromParent();

        Assert.False(Deleted);
        Assert.False(TextFileItem.CanDelete());
    }
}
