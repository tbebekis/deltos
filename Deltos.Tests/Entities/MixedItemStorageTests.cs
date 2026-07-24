// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Tests.Entities;

/// <summary>
/// Tests mixed folder and text file document storage.
/// </summary>
public class MixedItemStorageTests
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
    /// Removes a project manifest to simulate legacy storage.
    /// </summary>
    /// <param name="ProjectPath">The project path.</param>
    static void DeleteManifest(string ProjectPath)
    {
        string FilePath = ProjectManifest.GetFilePath(ProjectPath);
        if (File.Exists(FilePath))
            File.Delete(FilePath);
    }
    /// <summary>
    /// Moves a folder if the source exists.
    /// </summary>
    /// <param name="SourcePath">The source path.</param>
    /// <param name="TargetPath">The target path.</param>
    static void MoveFolder(string SourcePath, string TargetPath)
    {
        if (!Directory.Exists(SourcePath))
            return;

        Directory.CreateDirectory(Path.GetDirectoryName(TargetPath));
        Directory.Move(SourcePath, TargetPath);
    }

    // ● public
    /// <summary>
    /// Tests that document and folder items can mix folders and text files.
    /// </summary>
    [Fact]
    public void DocumentAndFolderSaveMixedItems()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Business Application Fundamentals");
            TextFile Preface = Document.AddTextFile("Preface");
            Folder Part = Document.AddFolder("The Nature of a Business Application", "Part");
            TextFile Introduction = Document.AddTextFile("Introduction");
            Part.AddTextFile("What Is a Business Application");
            Part.AddFolder("Types of Business Applications", "Chapter");

            Assert.Equal(new BaseItem[] { Preface, Part, Introduction }, Document.GetChildItems());
            Assert.True(Directory.Exists(Path.Combine(Document.ItemsFolderPath, "001._Preface")));
            Assert.True(Directory.Exists(Path.Combine(Document.ItemsFolderPath, "002._The_Nature_of_a_Business_Application")));
            Assert.True(Directory.Exists(Path.Combine(Document.ItemsFolderPath, "003._Introduction")));
            Assert.True(Directory.Exists(Part.ItemsFolderPath));

            Project OpenedProject = Project.Open(ProjectPath);
            Document OpenedDocument = OpenedProject.Documents[0];

            Assert.IsType<TextFile>(OpenedDocument.Items[0]);
            Assert.IsType<Folder>(OpenedDocument.Items[1]);
            Assert.IsType<TextFile>(OpenedDocument.Items[2]);
            Assert.Equal("Preface", OpenedDocument.Items[0].Title);
            Assert.Equal("The Nature of a Business Application", OpenedDocument.Items[1].Title);
            Assert.Equal("Introduction", OpenedDocument.Items[2].Title);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that legacy flat document storage loads and saves to Items.
    /// </summary>
    [Fact]
    public void LegacyFlatDocumentLoadsAndMigratesToItems()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Flat Book");
            Document.AddTextFile("Preface");
            string ItemsPath = Document.ItemsFolderPath;
            string LegacyTextFilesPath = Document.TextFilesFolderPath;
            MoveFolder(ItemsPath, LegacyTextFilesPath);
            DeleteManifest(ProjectPath);

            Project OpenedProject = Project.Open(ProjectPath);
            Document OpenedDocument = OpenedProject.Documents[0];

            Assert.Single(OpenedDocument.Items);
            Assert.IsType<TextFile>(OpenedDocument.Items[0]);

            OpenedProject.Save();

            Assert.True(File.Exists(ProjectManifest.GetFilePath(ProjectPath)));
            Assert.True(Directory.Exists(OpenedDocument.ItemsFolderPath));
            Assert.False(Directory.Exists(OpenedDocument.TextFilesFolderPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that legacy structured document storage loads and saves to Items.
    /// </summary>
    [Fact]
    public void LegacyStructuredDocumentLoadsAndMigratesToItems()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            FolderItem Structure = new FolderItem();
            Structure.Title = "Chapter";
            Document Document = Project.AddDocument("Structured Book", Structure);
            Folder Chapter = Document.AddFolder("Chapter One", string.Empty);
            Chapter.AddTextFile("Opening Scene");
            MoveFolder(Chapter.ItemsFolderPath, Chapter.TextFilesFolderPath);
            MoveFolder(Document.ItemsFolderPath, Document.FoldersFolderPath);
            DeleteManifest(ProjectPath);

            Project OpenedProject = Project.Open(ProjectPath);
            Document OpenedDocument = OpenedProject.Documents[0];
            Folder OpenedChapter = Assert.IsType<Folder>(OpenedDocument.Items[0]);

            Assert.Single(OpenedChapter.Items);
            Assert.IsType<TextFile>(OpenedChapter.Items[0]);

            OpenedProject.Save();

            Assert.True(Directory.Exists(OpenedDocument.ItemsFolderPath));
            Assert.False(Directory.Exists(OpenedDocument.FoldersFolderPath));
            Assert.True(Directory.Exists(OpenedChapter.ItemsFolderPath));
            Assert.False(Directory.Exists(OpenedChapter.TextFilesFolderPath));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that move commands stay inside the parent item boundary.
    /// </summary>
    [Fact]
    public void MoveCommandsStayInsideParentItemBoundary()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Book");
            Folder Part = Document.AddFolder("Part One", "Part");
            TextFile Scene = Part.AddTextFile("Opening Scene");
            TextFile Appendix = Document.AddTextFile("Appendix");

            bool MovedUp = Appendix.Move(true);

            Assert.True(MovedUp);
            Assert.Same(Document, Appendix.Parent);
            Assert.Same(Appendix, Document.Items[0]);
            Assert.Same(Part, Document.Items[1]);
            Assert.Same(Scene, Part.Items[0]);

            bool MovedUpAgain = Appendix.Move(true);

            Assert.False(MovedUpAgain);
            Assert.Same(Document, Appendix.Parent);
            Assert.Same(Appendix, Document.Items[0]);
            Assert.Same(Part, Document.Items[1]);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that move commands stop at folder edges.
    /// </summary>
    [Fact]
    public void MoveCommandsStopAtFolderEdges()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Book");
            Folder Part = Document.AddFolder("Part One", "Part");
            TextFile Opening = Part.AddTextFile("Opening");
            TextFile Appendix = Document.AddTextFile("Appendix");

            bool MovedUp = Opening.Move(true);

            Assert.False(MovedUp);
            Assert.Same(Part, Opening.Parent);
            Assert.Same(Part, Document.Items[0]);
            Assert.Same(Appendix, Document.Items[1]);
            Assert.Same(Opening, Part.Items[0]);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that moving up beside a folder sibling does not change parent.
    /// </summary>
    [Fact]
    public void MoveUpBesideFolderSiblingDoesNotChangeParent()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Book");
            Folder Folder = Document.AddFolder("Folder One", "Folder");
            TextFile First = Folder.AddTextFile("First");
            TextFile Second = Folder.AddTextFile("Second");
            TextFile Moving = Document.AddTextFile("AAA");

            bool Moved = Moving.Move(true);

            Assert.True(Moved);
            Assert.Same(Document, Moving.Parent);
            Assert.Same(Moving, Document.Items[0]);
            Assert.Same(Folder, Document.Items[1]);
            Assert.Same(First, Folder.Items[0]);
            Assert.Same(Second, Folder.Items[1]);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that container-root staged move folders are recovered as child items.
    /// </summary>
    [Fact]
    public void ContainerRootStagedMoveFolderLoadsAsChildItem()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Book");
            Folder Folder = Document.AddFolder("Folder One", "Folder");
            TextFile First = Folder.AddTextFile("First");
            TextFile Staged = Folder.AddTextFile("Text 1");
            string StagedPath = Staged.FolderPath;
            string TempPath = Path.Combine(Folder.FolderPath, ".deltos-move-test");
            Directory.Move(StagedPath, TempPath);

            Project OpenedProject = Project.Open(ProjectPath);
            Folder OpenedFolder = (Folder)OpenedProject.Documents[0].Items[0];

            Assert.Equal(2, OpenedFolder.Items.Count);
            Assert.Equal("First", OpenedFolder.Items[0].Title);
            Assert.Equal("Text 1", OpenedFolder.Items[1].Title);

            OpenedProject.Save();

            Assert.False(Directory.Exists(TempPath));
            Assert.True(Directory.Exists(Path.Combine(OpenedFolder.ItemsFolderPath, "002._Text_1")));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that change parent can move a nested folder to the document root.
    /// </summary>
    [Fact]
    public void ChangeParentMovesNestedFolderToDocumentRoot()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Book");
            Folder ParentFolder = Document.AddFolder("Parent Folder", "Folder");
            Folder ChildFolder = ParentFolder.AddFolder("Child Folder", "Folder");

            Assert.True(ChildFolder.CanChangeParent(Document));
            bool Changed = ChildFolder.ChangeParent(Document);

            Assert.True(Changed);
            Assert.Same(Document, ChildFolder.Parent);
            Assert.Empty(ParentFolder.Items);
            Assert.Equal(2, Document.Items.Count);
            Assert.Same(ParentFolder, Document.Items[0]);
            Assert.Same(ChildFolder, Document.Items[1]);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that change parent can move a root folder under another folder.
    /// </summary>
    [Fact]
    public void ChangeParentMovesRootFolderUnderFolder()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Book");
            Folder ParentFolder = Document.AddFolder("Parent Folder", "Folder");
            Folder MovingFolder = Document.AddFolder("Moving Folder", "Folder");

            Assert.True(MovingFolder.CanChangeParent(ParentFolder));
            bool Changed = MovingFolder.ChangeParent(ParentFolder);

            Assert.True(Changed);
            Assert.Same(ParentFolder, MovingFolder.Parent);
            Assert.Single(Document.Items);
            Assert.Single(ParentFolder.Items);
            Assert.Same(MovingFolder, ParentFolder.Items[0]);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
}
