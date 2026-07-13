// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Migrator;

/// <summary>
/// Migrates old StoryWriter projects to the current Deltos storage format.
/// </summary>
static public class Program
{
    // ● private fields
    const string SourcePath = "/home/teo/Dev/Stories/The_Corpers";
    const string TargetParentPath = "/home/teo/DeltosProjects";
    const string TargetProjectTitle = "The Corpers";

    // ● private
    /// <summary>
    /// Executes the migration.
    /// </summary>
    static public int Main()
    {
        try
        {
            Migrator Migrator = new Migrator(SourcePath, TargetParentPath, TargetProjectTitle);
            Migrator.Execute();
            return 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return 1;
        }
    }
}

/// <summary>
/// Migrates The Corpers old project data.
/// </summary>
public class Migrator
{
    // ● private fields
    readonly string fSourcePath;
    readonly string fTargetParentPath;
    readonly string fTargetProjectTitle;
    OldProject fOldProject;
    Project fProject;

    // ● private
    /// <summary>
    /// Reads the old project json file.
    /// </summary>
    OldProject ReadOldProject()
    {
        string FilePath = Path.Combine(fSourcePath, "Project.json");
        string JsonText = File.ReadAllText(FilePath);
        OldProject Result = JsonSerializer.Deserialize<OldProject>(JsonText);
        if (Result == null)
            throw new InvalidOperationException($"Could not read old project file: {FilePath}");

        return Result;
    }
    /// <summary>
    /// Recreates the target project folder.
    /// </summary>
    void PrepareTargetFolder()
    {
        Directory.CreateDirectory(fTargetParentPath);
        string TargetPath = Path.Combine(fTargetParentPath, Project.GetProjectFolderName(fTargetProjectTitle));
        if (Directory.Exists(TargetPath))
            Directory.Delete(TargetPath, true);
    }
    /// <summary>
    /// Creates the target project object.
    /// </summary>
    void CreateProject()
    {
        fProject = new Project();
        fProject.ProjectPath = Path.Combine(fTargetParentPath, Project.GetProjectFolderName(fTargetProjectTitle));
        fProject.Id = NormalizeId(fOldProject.Id);
        fProject.Title = fTargetProjectTitle;
        fProject.TempFileText = string.Empty;
    }
    /// <summary>
    /// Migrates all documents.
    /// </summary>
    void MigrateDocuments()
    {
        for (int Index = 0; Index < fOldProject.StoryList.Count; Index++)
        {
            OldStory Story = fOldProject.StoryList[Index];
            Document Document = CreateDocument(Story, Index + 1);
            fProject.Documents.Add(Document);
        }
    }
    /// <summary>
    /// Creates a document from an old story.
    /// </summary>
    /// <param name="Story">The old story.</param>
    /// <param name="OrderIndex">The one-based order index.</param>
    /// <returns>The new document.</returns>
    Document CreateDocument(OldStory Story, int OrderIndex)
    {
        string StoryFolderName = GetOldStorageName(OrderIndex, Story.Title);
        string StoryPath = Path.Combine(fSourcePath, StoryFolderName);

        Document Result = new Document();
        Result.Id = NormalizeId(Story.Id);
        Result.Title = Story.Title;
        Result.Structure.Title = "Chapter";
        Result.Synopsis = ReadText(Path.Combine(StoryPath, "Synopsis.txt"));

        for (int Index = 0; Index < Story.ChapterList.Count; Index++)
            Result.Folders.Add(CreateChapter(StoryPath, Story.ChapterList[Index], Index + 1));

        return Result;
    }
    /// <summary>
    /// Creates a document chapter folder.
    /// </summary>
    /// <param name="StoryPath">The old story path.</param>
    /// <param name="Chapter">The old chapter.</param>
    /// <param name="OrderIndex">The one-based order index.</param>
    /// <returns>The new folder.</returns>
    Folder CreateChapter(string StoryPath, OldChapter Chapter, int OrderIndex)
    {
        string ChapterFolderName = GetOldStorageName(OrderIndex, Chapter.Title);
        string ChapterPath = Path.Combine(StoryPath, ChapterFolderName);

        Folder Result = new Folder();
        Result.Id = NormalizeId(Chapter.Id);
        Result.Title = Chapter.Title;
        Result.LevelTitle = "Chapter";
        Result.Synopsis = ReadText(Path.Combine(ChapterPath, "Synopsis.txt"));

        for (int Index = 0; Index < Chapter.SceneList.Count; Index++)
            Result.Files.Add(CreateScene(ChapterPath, Chapter.SceneList[Index], Index + 1));

        return Result;
    }
    /// <summary>
    /// Creates a text file from an old scene.
    /// </summary>
    /// <param name="ChapterPath">The old chapter path.</param>
    /// <param name="Scene">The old scene.</param>
    /// <param name="OrderIndex">The one-based order index.</param>
    /// <returns>The new text file.</returns>
    TextFile CreateScene(string ChapterPath, OldScene Scene, int OrderIndex)
    {
        string SceneFolderName = GetOldStorageName(OrderIndex, Scene.Title);
        string ScenePath = Path.Combine(ChapterPath, SceneFolderName);

        TextFile Result = new TextFile();
        Result.Id = NormalizeId(Scene.Id);
        Result.Title = Scene.Title;
        Result.Text = ReadText(Path.Combine(ScenePath, "Text.txt"));
        Result.Text2 = ReadText(Path.Combine(ScenePath, "TextEn.txt"));
        Result.Synopsis = ReadText(Path.Combine(ScenePath, "Synopsis.txt"));
        Result.Draft = ReadText(Path.Combine(ScenePath, "Timeline.txt"));
        return Result;
    }
    /// <summary>
    /// Migrates all components.
    /// </summary>
    void MigrateComponents()
    {
        foreach (OldComponent OldComponent in fOldProject.ComponentList)
            fProject.Components.Add(CreateComponent(OldComponent));
    }
    /// <summary>
    /// Creates a component.
    /// </summary>
    /// <param name="OldComponent">The old component.</param>
    /// <returns>The new component.</returns>
    Component CreateComponent(OldComponent OldComponent)
    {
        string FileName = GetComponentFileName(OldComponent.Title);

        Component Result = new Component();
        Result.Id = NormalizeId(OldComponent.Id);
        Result.Title = OldComponent.Title;
        Result.Category = OldComponent.Category;
        Result.TagList = OldComponent.TagList.ToList();
        Result.AliasList = OldComponent.AliasList.ToList();
        Result.Text = ReadText(Path.Combine(fSourcePath, "Components", FileName));
        Result.Text2 = ReadText(Path.Combine(fSourcePath, "ComponentsEn", FileName));
        return Result;
    }
    /// <summary>
    /// Migrates notes from the old note list.
    /// </summary>
    void MigrateNotes()
    {
        for (int Index = 0; Index < fOldProject.NoteList.Count; Index++)
        {
            OldNote OldNote = fOldProject.NoteList[Index];
            string FilePath = Path.Combine(fSourcePath, "Notes", GetOldStorageName(Index + 1, OldNote.Title) + ".txt");
            if (!File.Exists(FilePath))
                continue;

            Note Note = new Note();
            Note.Id = NormalizeId(OldNote.Id);
            Note.Title = OldNote.Title;
            Note.Text = ReadText(FilePath);
            fProject.Notes.Add(Note);
        }
    }
    /// <summary>
    /// Copies project image files.
    /// </summary>
    void CopyImages()
    {
        string SourceImagesPath = Path.Combine(fSourcePath, "Images");
        if (!Directory.Exists(SourceImagesPath))
            return;

        Directory.CreateDirectory(fProject.ImagesFolderPath);

        foreach (string SourceFilePath in Directory.GetFiles(SourceImagesPath))
        {
            string FileName = Path.GetFileName(SourceFilePath);
            File.Copy(SourceFilePath, Path.Combine(fProject.ImagesFolderPath, FileName), true);
        }
    }
    /// <summary>
    /// Saves and validates the migrated project.
    /// </summary>
    void SaveAndValidate()
    {
        fProject.UpdateReferences(null);
        fProject.Save();
        CopyImages();

        Project OpenedProject = Project.Open(fProject.ProjectPath);
        int DocumentCount = OpenedProject.Documents.Count;
        int ChapterCount = OpenedProject.Documents.Sum(Document => Document.Folders.Count);
        int SceneCount = OpenedProject.Documents.Sum(Document => Document.Folders.Sum(Folder => Folder.Files.Count));
        int ComponentCount = OpenedProject.Components.Count;
        int NoteCount = OpenedProject.Notes.Count;
        int ImageCount = Directory.Exists(OpenedProject.ImagesFolderPath) ? Directory.GetFiles(OpenedProject.ImagesFolderPath).Length : 0;

        Console.WriteLine($"Migrated project: {OpenedProject.ProjectPath}");
        Console.WriteLine($"Documents: {DocumentCount}");
        Console.WriteLine($"Chapters: {ChapterCount}");
        Console.WriteLine($"Scenes: {SceneCount}");
        Console.WriteLine($"Components: {ComponentCount}");
        Console.WriteLine($"Notes: {NoteCount}");
        Console.WriteLine($"Images: {ImageCount}");
    }
    /// <summary>
    /// Returns an old numbered storage name.
    /// </summary>
    /// <param name="OrderIndex">The one-based order index.</param>
    /// <param name="Title">The title.</param>
    /// <returns>The old storage name.</returns>
    static string GetOldStorageName(int OrderIndex, string Title)
    {
        return $"{OrderIndex}. {Title}";
    }
    /// <summary>
    /// Returns the old component file name.
    /// </summary>
    /// <param name="Title">The component title.</param>
    /// <returns>The old component file name.</returns>
    static string GetComponentFileName(string Title)
    {
        string Stem = Regex.Replace(Title ?? string.Empty, "[^A-Za-z0-9-]+", string.Empty);
        return Stem + ".md";
    }
    /// <summary>
    /// Normalizes an old identifier.
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <returns>The normalized identifier.</returns>
    static string NormalizeId(string Id)
    {
        return string.IsNullOrWhiteSpace(Id) ? Guid.NewGuid().ToString().ToUpperInvariant() : Id.Trim();
    }
    /// <summary>
    /// Reads a text file.
    /// </summary>
    /// <param name="FilePath">The file path.</param>
    /// <returns>The file text or an empty string.</returns>
    static string ReadText(string FilePath)
    {
        if (!File.Exists(FilePath))
            return string.Empty;

        return File.ReadAllText(FilePath);
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the Migrator class.
    /// </summary>
    /// <param name="SourcePath">The old project path.</param>
    /// <param name="TargetParentPath">The target parent folder path.</param>
    /// <param name="TargetProjectTitle">The target project title.</param>
    public Migrator(string SourcePath, string TargetParentPath, string TargetProjectTitle)
    {
        fSourcePath = SourcePath;
        fTargetParentPath = TargetParentPath;
        fTargetProjectTitle = TargetProjectTitle;
    }

    // ● public
    /// <summary>
    /// Executes the migration.
    /// </summary>
    public void Execute()
    {
        fOldProject = ReadOldProject();
        PrepareTargetFolder();
        CreateProject();
        MigrateDocuments();
        MigrateComponents();
        MigrateNotes();
        SaveAndValidate();
    }
}

/// <summary>
/// Represents an old StoryWriter project.
/// </summary>
public class OldProject
{
    // ● properties
    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the project title.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the old story list.
    /// </summary>
    public List<OldStory> StoryList { get; set; } = new();
    /// <summary>
    /// Gets or sets the old component list.
    /// </summary>
    public List<OldComponent> ComponentList { get; set; } = new();
    /// <summary>
    /// Gets or sets the old note list.
    /// </summary>
    public List<OldNote> NoteList { get; set; } = new();
}

/// <summary>
/// Represents an old StoryWriter story.
/// </summary>
public class OldStory
{
    // ● properties
    /// <summary>
    /// Gets or sets the story identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the story title.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the old chapter list.
    /// </summary>
    public List<OldChapter> ChapterList { get; set; } = new();
}

/// <summary>
/// Represents an old StoryWriter chapter.
/// </summary>
public class OldChapter
{
    // ● properties
    /// <summary>
    /// Gets or sets the chapter identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the chapter title.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the old scene list.
    /// </summary>
    public List<OldScene> SceneList { get; set; } = new();
}

/// <summary>
/// Represents an old StoryWriter scene.
/// </summary>
public class OldScene
{
    // ● properties
    /// <summary>
    /// Gets or sets the scene identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the scene title.
    /// </summary>
    public string Title { get; set; } = string.Empty;
}

/// <summary>
/// Represents an old StoryWriter component.
/// </summary>
public class OldComponent
{
    // ● properties
    /// <summary>
    /// Gets or sets the component identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the component title.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the component category.
    /// </summary>
    public string Category { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the component tag list.
    /// </summary>
    public List<string> TagList { get; set; } = new();
    /// <summary>
    /// Gets or sets the component alias list.
    /// </summary>
    public List<string> AliasList { get; set; } = new();
}

/// <summary>
/// Represents an old StoryWriter note.
/// </summary>
public class OldNote
{
    // ● properties
    /// <summary>
    /// Gets or sets the note identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the note title.
    /// </summary>
    public string Title { get; set; } = string.Empty;
}
