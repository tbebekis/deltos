// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Application sample project support.
/// </summary>
static public partial class AppHost
{
    // ● private
    /// <summary>
    /// Creates a generated sample project title.
    /// </summary>
    /// <param name="ParentFolderPath">The sample parent folder path.</param>
    /// <returns>The generated project title.</returns>
    static string CreateSampleProjectTitle(string ParentFolderPath)
    {
        string BaseTitle = $"Sample Project {DateTime.Now.ToString("yyyyMMdd HHmmss", CultureInfo.InvariantCulture)}";
        string Title = BaseTitle;
        int Index = 2;

        while (System.IO.Directory.Exists(Project.GetProjectFolderPath(ParentFolderPath, Title)))
        {
            Title = $"{BaseTitle} Copy {Index}";
            Index++;
        }

        return Title;
    }
    /// <summary>
    /// Creates a folder item chain from level titles.
    /// </summary>
    /// <param name="Levels">The level titles.</param>
    /// <returns>The created folder item chain.</returns>
    static FolderItem CreateStructure(params string[] Levels)
    {
        FolderItem Result = null;
        FolderItem Parent = null;

        foreach (string Level in Levels)
        {
            FolderItem Item = new FolderItem { Title = Level };
            if (Result == null)
                Result = Item;
            else
                Parent.Child = Item;

            Parent = Item;
        }

        Result?.UpdateReferences(null);
        return Result;
    }
    /// <summary>
    /// Applies text content to a text file.
    /// </summary>
    /// <param name="File">The text file.</param>
    /// <param name="Text">The primary text.</param>
    /// <param name="Text2">The secondary text.</param>
    /// <param name="Synopsis">The synopsis text.</param>
    /// <param name="Draft">The draft text.</param>
    static void ApplyTextFileContent(TextFile File, string Text, string Text2, string Synopsis, string Draft)
    {
        File.Text = Text;
        File.Text2 = Text2;
        File.Synopsis = Synopsis;
        File.Draft = Draft;
        File.Save();
    }
    /// <summary>
    /// Adds project sample reference content.
    /// </summary>
    /// <param name="Project">The project.</param>
    static void AddSampleReferenceContent(Project Project)
    {
        Project.TempFileText =
            "# Temp" + Environment.NewLine + Environment.NewLine +
            "Loose ideas, fragments, and reminders can stay here until they find a permanent place.";

        Note Note = Project.AddNote("Revision Notes");
        Note.Text =
            "# Revision Notes" + Environment.NewLine + Environment.NewLine +
            "- Check the opening hook." + Environment.NewLine +
            "- Keep terminology consistent." + Environment.NewLine +
            "- Mark sections that need a second pass.";
        Note.Save();

        Project.AddComponent(new Component
        {
            Title = "Main Character",
            Title2 = "Primary Reference",
            Category = "Characters",
            Tags = "sample; cast",
            Aliases = "Protagonist",
            Text = "# Main Character" + Environment.NewLine + Environment.NewLine + "A short reference entry for an important person or subject.",
            Text2 = "# Primary Reference" + Environment.NewLine + Environment.NewLine + "Secondary-language or alternate reference text."
        });

        Project.AddComponent(new Component
        {
            Title = "Central Place",
            Category = "Places",
            Tags = "sample; setting",
            Aliases = "Base",
            Text = "# Central Place" + Environment.NewLine + Environment.NewLine + "A location or domain entry that can be exported to the static wiki."
        });
    }
    /// <summary>
    /// Populates a flat sample document.
    /// </summary>
    /// <param name="Project">The project.</param>
    static void PopulateFlatSample(Project Project)
    {
        Document Document = Project.AddDocument("Sample Article");
        Document.Synopsis = "A flat document keeps text files directly under the document.";

        ApplyTextFileContent(
            Document.AddTextFile("Introduction"),
            "# Introduction" + Environment.NewLine + Environment.NewLine + "This is the first text file in a flat document.",
            "# Introduction 2" + Environment.NewLine + Environment.NewLine + "Secondary-language text can live beside the primary text.",
            "Introduce the topic and define the purpose.",
            "Draft notes for expanding the opening section.");

        ApplyTextFileContent(
            Document.AddTextFile("Main Text"),
            "# Main Text" + Environment.NewLine + Environment.NewLine + "Use more text files when a simple article still needs internal sections.",
            string.Empty,
            "Develop the main idea.",
            "Move research notes here before polishing.");

        Document.Save();
    }
    /// <summary>
    /// Populates a chapter sample document.
    /// </summary>
    /// <param name="Project">The project.</param>
    static void PopulateChapterSample(Project Project)
    {
        Document Document = Project.AddDocument("Sample Book", CreateStructure("Chapter"));
        Document.Synopsis = "A structured document with chapters that contain text files.";

        Folder ChapterOne = Document.AddFolder("Opening", "Chapter");
        ChapterOne.Synopsis = "The opening chapter establishes the voice and core situation.";
        ApplyTextFileContent(
            ChapterOne.AddTextFile("First Scene"),
            "# First Scene" + Environment.NewLine + Environment.NewLine + "The first scene starts close to the main action.",
            string.Empty,
            "Open with a concrete moment.",
            "Try a quieter alternate opening if needed.");
        ApplyTextFileContent(
            ChapterOne.AddTextFile("Second Scene"),
            "# Second Scene" + Environment.NewLine + Environment.NewLine + "The second scene changes pressure or perspective.",
            string.Empty,
            "Escalate the central question.",
            "Keep the transition short.");

        Folder ChapterTwo = Document.AddFolder("Complication", "Chapter");
        ChapterTwo.Synopsis = "The second chapter adds new information and raises the stakes.";
        ApplyTextFileContent(
            ChapterTwo.AddTextFile("New Problem"),
            "# New Problem" + Environment.NewLine + Environment.NewLine + "A later text file can be moved, renamed, or split as the draft grows.",
            string.Empty,
            "Introduce the complication.",
            "Add a note about continuity here.");

        Document.Save();
    }
    /// <summary>
    /// Populates a part/chapter sample document.
    /// </summary>
    /// <param name="Project">The project.</param>
    static void PopulatePartChapterSample(Project Project)
    {
        Document Document = Project.AddDocument("Sample Manual", CreateStructure("Part", "Chapter"));
        Document.Synopsis = "A larger document with parts, chapters, and text files.";

        Folder PartOne = Document.AddFolder("Foundations", "Part");
        PartOne.Synopsis = "The first part introduces the basic ideas.";

        Folder ChapterOne = PartOne.AddFolder("Orientation", "Chapter");
        ChapterOne.Synopsis = "The first chapter gives the reader a map.";
        ApplyTextFileContent(
            ChapterOne.AddTextFile("Purpose"),
            "# Purpose" + Environment.NewLine + Environment.NewLine + "This text file explains what the larger work is trying to do.",
            string.Empty,
            "Explain the purpose of the work.",
            "Add missing assumptions before publishing.");
        ApplyTextFileContent(
            ChapterOne.AddTextFile("Scope"),
            "# Scope" + Environment.NewLine + Environment.NewLine + "This text file describes boundaries, terminology, and constraints.",
            string.Empty,
            "Define what is inside and outside the scope.",
            "Check if this belongs before Purpose.");

        Folder PartTwo = Document.AddFolder("Reference", "Part");
        PartTwo.Synopsis = "The second part holds material that readers may revisit.";

        Folder ChapterTwo = PartTwo.AddFolder("Details", "Chapter");
        ChapterTwo.Synopsis = "The details chapter can grow into several text files.";
        ApplyTextFileContent(
            ChapterTwo.AddTextFile("Reference Entry"),
            "# Reference Entry" + Environment.NewLine + Environment.NewLine + "Structured projects keep deep outlines visible while editing text.",
            string.Empty,
            "Provide a reusable reference entry.",
            "Expand with examples.");

        Document.Save();
    }
    /// <summary>
    /// Populates sample project content.
    /// </summary>
    /// <param name="Project">The project.</param>
    /// <param name="Kind">The sample project kind.</param>
    static void PopulateSampleProject(Project Project, string Kind)
    {
        if (Kind.IsSameText(SampleProjectKindChapter))
            PopulateChapterSample(Project);
        else if (Kind.IsSameText(SampleProjectKindPartChapter))
            PopulatePartChapterSample(Project);
        else
            PopulateFlatSample(Project);

        AddSampleReferenceContent(Project);
    }

    // ● static public
    /// <summary>
    /// Gets the flat sample project kind.
    /// </summary>
    static public string SampleProjectKindFlat => "Flat";
    /// <summary>
    /// Gets the chapter sample project kind.
    /// </summary>
    static public string SampleProjectKindChapter => "Chapter";
    /// <summary>
    /// Gets the part/chapter sample project kind.
    /// </summary>
    static public string SampleProjectKindPartChapter => "PartChapter";
    /// <summary>
    /// Creates and opens a generated sample project.
    /// </summary>
    /// <param name="ParentFolderPath">The parent folder path.</param>
    /// <param name="Kind">The sample project kind.</param>
    /// <returns>The created sample project.</returns>
    static public Project CreateSampleProject(string ParentFolderPath, string Kind)
    {
        string Title = CreateSampleProjectTitle(ParentFolderPath);

        CloseProject();

        Project Project = Project.Create(ParentFolderPath, Title);
        PopulateSampleProject(Project, Kind);
        Project.Save();
        SetCurrentProject(Project, true);

        return Project;
    }
}
