// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Tests.Export;

/// <summary>
/// Tests document export item metadata behavior.
/// </summary>
public class DocumentExporterMetadataTests
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
    /// Creates text export options.
    /// </summary>
    /// <param name="Format">The export format.</param>
    /// <returns>The export options.</returns>
    static Deltos.Export.ExportOptions CreateOptions(Deltos.Export.ExportFormat Format)
    {
        Deltos.Export.ExportOptions Result = new Deltos.Export.ExportOptions();
        Result.Language = Deltos.Export.ExportLanguage.Primary;
        Result.Source = Deltos.Export.ExportSource.Text;
        Result.Format = Format;
        Result.FolderTitle = Deltos.Export.ExportTitleOptions.Word | Deltos.Export.ExportTitleOptions.Number | Deltos.Export.ExportTitleOptions.Title;
        Result.TextFileTitle = Deltos.Export.ExportTitleOptions.Word | Deltos.Export.ExportTitleOptions.Number | Deltos.Export.ExportTitleOptions.Title;
        Result.TreatTextFilesAsPlainText = true;
        return Result;
    }
    /// <summary>
    /// Returns a generated export file.
    /// </summary>
    /// <param name="ExportFolderPath">The export folder path.</param>
    /// <param name="Extension">The file extension.</param>
    /// <returns>The export file path.</returns>
    static string GetExportFile(string ExportFolderPath, string Extension)
    {
        return Directory.GetFiles(ExportFolderPath, "*" + Extension)[0];
    }

    // ● public
    /// <summary>
    /// Tests that title visibility and numbering metadata affect text exports.
    /// </summary>
    [Fact]
    public void TextExportUsesItemTitleVisibilityAndNumbering()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Book");
            Folder Folder = Document.AddFolder("Part One", "Part");
            Folder.Numbering = ItemNumbering.Custom;
            Folder.CustomNumbering = "I";
            TextFile Hidden = Folder.AddTextFile("Hidden Scene");
            Hidden.IncludeTitleInOutput = false;
            Hidden.Text = "Hidden scene text.";
            TextFile Unnumbered = Folder.AddTextFile("Unnumbered Scene");
            Unnumbered.Numbering = ItemNumbering.None;
            Unnumbered.Text = "Unnumbered scene text.";

            string ExportFolderPath = new Deltos.Export.DocumentExporter(Document, CreateOptions(Deltos.Export.ExportFormat.Txt)).Execute();
            string Text = File.ReadAllText(GetExportFile(ExportFolderPath, ".txt"));

            Assert.Contains("Part I. Part One", Text);
            Assert.DoesNotContain("Hidden Scene", Text);
            Assert.Contains("Hidden scene text.", Text);
            Assert.Contains("TextFile: Unnumbered Scene", Text);
            Assert.DoesNotContain("TextFile 2. Unnumbered Scene", Text);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that page break metadata affects markdown exports.
    /// </summary>
    [Fact]
    public void MarkdownExportUsesItemPageBreakBefore()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Book");
            TextFile FileItem = Document.AddTextFile("Opening");
            FileItem.PageBreakBefore = true;
            FileItem.Text = "Opening text.";

            string ExportFolderPath = new Deltos.Export.DocumentExporter(Document, CreateOptions(Deltos.Export.ExportFormat.Markdown)).Execute();
            string Markdown = File.ReadAllText(GetExportFile(ExportFolderPath, ".md"));

            Assert.Contains("page-break-before: always", Markdown);
            Assert.Contains("# TextFile 1. Opening", Markdown);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that TOC metadata affects HTML exports.
    /// </summary>
    [Fact]
    public void HtmlExportUsesItemTocMetadata()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Book");
            TextFile FileItem = Document.AddTextFile("Opening");
            FileItem.IncludeInToc = false;
            FileItem.Text = "# Inner Heading" + Environment.NewLine + "Opening text.";

            string ExportFolderPath = new Deltos.Export.DocumentExporter(Document, CreateOptions(Deltos.Export.ExportFormat.Html)).Execute();
            string Html = File.ReadAllText(GetExportFile(ExportFolderPath, ".html"));

            Assert.Contains("<h1", Html);
            Assert.Contains("TextFile 1. Opening", Html);
            Assert.Contains("Inner Heading", Html);
            Assert.DoesNotContain("toc-item toc-level-1", Html);
            Assert.DoesNotContain("toc-item toc-level-2", Html);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that markdown pipe tables are rendered as HTML tables.
    /// </summary>
    [Fact]
    public void HtmlExportRendersMarkdownPipeTables()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Book");
            TextFile FileItem = Document.AddTextFile("Table Text");
            FileItem.Text =
                "| Concept | Example |" + Environment.NewLine +
                "|---|---|" + Environment.NewLine +
                "| DataModule | `InvoiceDataModule` |";
            Deltos.Export.ExportOptions Options = CreateOptions(Deltos.Export.ExportFormat.Html);
            Options.TreatTextFilesAsPlainText = false;

            string ExportFolderPath = new Deltos.Export.DocumentExporter(Document, Options).Execute();
            string Html = File.ReadAllText(GetExportFile(ExportFolderPath, ".html"));

            Assert.Contains("<table>", Html);
            Assert.Contains("<th>Concept</th>", Html);
            Assert.Contains("<code>InvoiceDataModule</code>", Html);
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
    /// <summary>
    /// Tests that internal markdown export writes one file per text file with full hierarchical numbering.
    /// </summary>
    [Fact]
    public void InternalMarkdownExportUsesHierarchicalFileNames()
    {
        string ProjectPath;
        Project Project = CreateProject(out ProjectPath);

        try
        {
            Document Document = Project.AddDocument("Book");
            TextFile Preface = Document.AddTextFile("Preface");
            Preface.Text = "Preface text.";
            TextFile Introduction = Document.AddTextFile("Introduction");
            Introduction.Text = "Introduction text.";
            Folder Part = Document.AddFolder("Part One", "Part");
            TextFile Opening = Part.AddTextFile("What Is a Business Application");
            Opening.Text = "Opening text.";
            TextFile Epilogue = Document.AddTextFile("Epilogue");
            Epilogue.Text = "Epilogue text.";

            string ExportFolderPath = new Deltos.Export.DocumentExporter(Document, CreateOptions(Deltos.Export.ExportFormat.InternalMarkdown)).Execute();
            string[] FilePaths = Directory.GetFiles(ExportFolderPath, "*.md").OrderBy(x => x).ToArray();
            string[] FileNames = FilePaths.Select(Path.GetFileName).ToArray();

            Assert.Equal(4, FileNames.Length);
            Assert.Contains("001.000.001_Preface_Primary.md", FileNames);
            Assert.Contains("001.000.002_Introduction_Primary.md", FileNames);
            Assert.Contains("001.001.001_What_Is_a_Business_Application_Primary.md", FileNames);
            Assert.Contains("001.002.001_Epilogue_Primary.md", FileNames);
            Assert.Contains("Preface text.", File.ReadAllText(Path.Combine(ExportFolderPath, "001.000.001_Preface_Primary.md")));
            Assert.Contains("Introduction text.", File.ReadAllText(Path.Combine(ExportFolderPath, "001.000.002_Introduction_Primary.md")));
            Assert.Contains("Opening text.", File.ReadAllText(Path.Combine(ExportFolderPath, "001.001.001_What_Is_a_Business_Application_Primary.md")));
            Assert.Contains("Epilogue text.", File.ReadAllText(Path.Combine(ExportFolderPath, "001.002.001_Epilogue_Primary.md")));
        }
        finally
        {
            DeleteFolder(ProjectPath);
        }
    }
}
