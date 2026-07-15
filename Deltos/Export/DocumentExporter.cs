// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Export;

/// <summary>
/// Exports a document to external files.
/// </summary>
public class DocumentExporter
{
    // ● private fields
    /// <summary>
    /// The exported document.
    /// </summary>
    readonly Document fDocument;
    /// <summary>
    /// The export options.
    /// </summary>
    readonly ExportOptions fOptions;

    // ● private
    /// <summary>
    /// Finds the LibreOffice executable.
    /// </summary>
    /// <returns>The LibreOffice executable path, if found; otherwise null.</returns>
    static string FindLibreOffice()
    {
        string PathText = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (string Folder in PathText.Split(System.IO.Path.PathSeparator))
        {
            string SofficePath = System.IO.Path.Combine(Folder, "soffice");
            if (System.IO.File.Exists(SofficePath))
                return SofficePath;

            string LibreOfficePath = System.IO.Path.Combine(Folder, "libreoffice");
            if (System.IO.File.Exists(LibreOfficePath))
                return LibreOfficePath;

            string SofficeExePath = System.IO.Path.Combine(Folder, "soffice.exe");
            if (System.IO.File.Exists(SofficeExePath))
                return SofficeExePath;
        }

        string[] Candidates =
        {
            @"/usr/bin/libreoffice",
            @"/usr/bin/soffice",
            @"C:\Program Files\LibreOffice\program\soffice.exe",
            @"C:\Program Files (x86)\LibreOffice\program\soffice.exe"
        };

        return Candidates.FirstOrDefault(Item => System.IO.File.Exists(Item));
    }
    /// <summary>
    /// Returns a safe file title.
    /// </summary>
    /// <param name="Title">The title.</param>
    /// <returns>The safe file title.</returns>
    static string SafeTitle(string Title)
    {
        string Result = string.IsNullOrWhiteSpace(Title) ? "Document" : Title.Trim();
        foreach (char Char in System.IO.Path.GetInvalidFileNameChars())
            Result = Result.Replace(Char, '_');

        return Result.Replace(' ', '_');
    }
    /// <summary>
    /// Formats a title.
    /// </summary>
    /// <param name="Word">The item word.</param>
    /// <param name="OrderIndex">The item order index.</param>
    /// <param name="Title">The item title.</param>
    /// <param name="Options">The title options.</param>
    /// <returns>The formatted title.</returns>
    static string FormatTitle(string Word, int OrderIndex, string Title, ExportTitleOptions Options)
    {
        if (Options == ExportTitleOptions.None)
            return string.Empty;

        StringBuilder Builder = new StringBuilder();
        if (Options.HasFlag(ExportTitleOptions.Bullet))
            Builder.Append("●");

        if (Options.HasFlag(ExportTitleOptions.Word))
            AppendTitlePart(Builder, Word);

        if (Options.HasFlag(ExportTitleOptions.Number))
            AppendTitlePart(Builder, OrderIndex.ToString());

        if (Options.HasFlag(ExportTitleOptions.Title))
        {
            if (Options.HasFlag(ExportTitleOptions.Number))
                Builder.Append($". {Title}");
            else if (Options.HasFlag(ExportTitleOptions.Word))
                Builder.Append($": {Title}");
            else
                AppendTitlePart(Builder, Title);
        }

        return Builder.ToString();
    }
    /// <summary>
    /// Returns an item title for the selected export language.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <param name="UseSecondary">True to use the secondary title.</param>
    /// <returns>The item title.</returns>
    static string GetExportTitle(BaseItem Item, bool UseSecondary)
    {
        return Item?.GetTitle(UseSecondary) ?? string.Empty;
    }
    /// <summary>
    /// Appends a title part.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="Part">The title part.</param>
    static void AppendTitlePart(StringBuilder Builder, string Part)
    {
        if (string.IsNullOrWhiteSpace(Part))
            return;

        if (Builder.Length > 0)
            Builder.Append(' ');

        Builder.Append(Part);
    }
    /// <summary>
    /// Returns the heading level for a markdown heading inside a text file.
    /// </summary>
    /// <param name="TextFileLevel">The text file heading level.</param>
    /// <param name="MarkdownLevel">The markdown heading level.</param>
    /// <param name="BaseMarkdownLevel">The first exported markdown heading level.</param>
    /// <returns>The export heading level.</returns>
    static int GetNestedHeadingLevel(int TextFileLevel, int MarkdownLevel, int BaseMarkdownLevel)
    {
        return Math.Clamp(TextFileLevel + MarkdownLevel - BaseMarkdownLevel + 1, 1, 6);
    }
    /// <summary>
    /// Returns true if a line is a markdown heading.
    /// </summary>
    /// <param name="Line">The line.</param>
    /// <param name="Level">The heading level.</param>
    /// <param name="Text">The heading text.</param>
    /// <returns>True if the line is a heading.</returns>
    static bool TryParseMarkdownHeading(string Line, out int Level, out string Text)
    {
        Level = 0;
        Text = string.Empty;
        Match Match = Regex.Match(Line, @"^(#{1,6})\s+(.+?)\s*#*\s*$");
        if (!Match.Success)
            return false;

        Level = Match.Groups[1].Value.Length;
        Text = Match.Groups[2].Value.Trim();
        return !string.IsNullOrWhiteSpace(Text);
    }
    /// <summary>
    /// Returns true if a line starts or ends a markdown code fence.
    /// </summary>
    /// <param name="Line">The line.</param>
    /// <returns>True if the line starts or ends a code fence.</returns>
    static bool IsMarkdownFence(string Line)
    {
        string Value = Line.TrimStart();
        return Value.StartsWith("```", StringComparison.Ordinal) || Value.StartsWith("~~~", StringComparison.Ordinal);
    }
    /// <summary>
    /// Returns the first exported markdown heading level in text.
    /// </summary>
    /// <param name="Text">The markdown text.</param>
    /// <returns>The first exported markdown heading level.</returns>
    static int GetBaseMarkdownHeadingLevel(string Text)
    {
        int Result = 0;
        bool InFence = false;
        string[] Lines = Text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        foreach (string Line in Lines)
        {
            if (IsMarkdownFence(Line))
                InFence = !InFence;

            if (!InFence && TryParseMarkdownHeading(Line, out int Level, out string HeadingText))
            {
                if (Result == 0 || Level < Result)
                    Result = Level;
            }
        }

        return Result == 0 ? 1 : Result;
    }
    /// <summary>
    /// Appends markdown text as HTML.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="TocBuilder">The table of contents builder.</param>
    /// <param name="AnchorIndex">The next anchor index.</param>
    /// <param name="MarkdownText">The markdown text.</param>
    /// <param name="TextFileLevel">The text file heading level.</param>
    static void AppendMarkdownHtml(StringBuilder Builder, StringBuilder TocBuilder, ref int AnchorIndex, string MarkdownText, int TextFileLevel)
    {
        if (string.IsNullOrWhiteSpace(MarkdownText))
            return;

        StringBuilder TextBuilder = new StringBuilder();
        bool InFence = false;
        string[] Lines = MarkdownText.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        int BaseMarkdownLevel = GetBaseMarkdownHeadingLevel(MarkdownText);
        foreach (string Line in Lines)
        {
            if (IsMarkdownFence(Line))
                InFence = !InFence;

            if (!InFence && TryParseMarkdownHeading(Line, out int MarkdownLevel, out string HeadingText))
            {
                int Level = GetNestedHeadingLevel(TextFileLevel, MarkdownLevel, BaseMarkdownLevel);
                string AnchorId = CreateAnchorId(AnchorIndex++);
                AppendTocItem(TocBuilder, Level, AnchorId, HeadingText);
                TextBuilder.AppendLine(CreateHtmlHeading(Level, AnchorId, HeadingText, false));
            }
            else
            {
                TextBuilder.AppendLine(Line);
            }
        }

        Builder.AppendLine(Markdig.Markdown.ToHtml(TextBuilder.ToString()));
    }
    /// <summary>
    /// Appends plain text as HTML paragraphs.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="TocBuilder">The table of contents builder.</param>
    /// <param name="AnchorIndex">The next anchor index.</param>
    /// <param name="Text">The plain text.</param>
    /// <param name="TextFileLevel">The text file heading level.</param>
    static void AppendPlainTextHtml(StringBuilder Builder, StringBuilder TocBuilder, ref int AnchorIndex, string Text, int TextFileLevel)
    {
        if (string.IsNullOrWhiteSpace(Text))
            return;

        bool InFence = false;
        string[] Lines = Text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        int BaseMarkdownLevel = GetBaseMarkdownHeadingLevel(Text);
        foreach (string Line in Lines)
        {
            string Value = Line.Trim();
            if (string.IsNullOrWhiteSpace(Value))
                continue;

            if (IsMarkdownFence(Line))
                InFence = !InFence;

            if (!InFence && TryParseMarkdownHeading(Value, out int MarkdownLevel, out string HeadingText))
            {
                int Level = GetNestedHeadingLevel(TextFileLevel, MarkdownLevel, BaseMarkdownLevel);
                string AnchorId = CreateAnchorId(AnchorIndex++);
                AppendTocItem(TocBuilder, Level, AnchorId, HeadingText);
                Builder.AppendLine(CreateHtmlHeading(Level, AnchorId, HeadingText, false));
            }
            else
            {
                Builder.AppendLine($"<p>{WebUtility.HtmlEncode(Value)}</p>");
            }
        }
    }
    /// <summary>
    /// Creates an HTML anchor id.
    /// </summary>
    /// <param name="Index">The anchor index.</param>
    /// <returns>The HTML anchor id.</returns>
    static string CreateAnchorId(int Index)
    {
        return $"heading-{Index.ToString(CultureInfo.InvariantCulture)}";
    }
    /// <summary>
    /// Creates an HTML heading.
    /// </summary>
    /// <param name="Level">The heading level.</param>
    /// <param name="AnchorId">The anchor id.</param>
    /// <param name="Title">The heading title.</param>
    /// <param name="UsePageBreak">True to add a page break before the heading.</param>
    /// <returns>The HTML heading.</returns>
    static string CreateHtmlHeading(int Level, string AnchorId, string Title, bool UsePageBreak)
    {
        string Style = UsePageBreak ? " style=\"page-break-before: always; break-before: page;\"" : string.Empty;
        return $"<h{Level} id=\"{WebUtility.HtmlEncode(AnchorId)}\"{Style}>{WebUtility.HtmlEncode(Title)}</h{Level}>";
    }
    /// <summary>
    /// Appends a table of contents item.
    /// </summary>
    /// <param name="Builder">The table of contents builder.</param>
    /// <param name="Level">The heading level.</param>
    /// <param name="AnchorId">The anchor id.</param>
    /// <param name="Title">The item title.</param>
    static void AppendTocItem(StringBuilder Builder, int Level, string AnchorId, string Title)
    {
        Builder.AppendLine($"<a class=\"toc-item toc-level-{Level}\" href=\"#{WebUtility.HtmlEncode(AnchorId)}\">{WebUtility.HtmlEncode(Title)}</a>");
    }
    /// <summary>
    /// Wraps body HTML into a full HTML document.
    /// </summary>
    /// <param name="Title">The HTML document title.</param>
    /// <param name="Toc">The table of contents HTML.</param>
    /// <param name="Body">The body HTML.</param>
    /// <param name="IncludeToc">True to include the table of contents.</param>
    /// <param name="UseBlackHeadings">True to use black heading color.</param>
    /// <returns>The full HTML document.</returns>
    static string WrapHtml(string Title, string Toc, string Body, bool IncludeToc, bool UseBlackHeadings)
    {
        string HeadingColor = UseBlackHeadings ? "#000000" : "#8A4B16";
        string PageHtml = IncludeToc
            ? $$"""
                <div class="page">
                <nav class="toc">
                <div class="toc-title">Contents</div>
                {{Toc}}
                </nav>
                <main class="content">
                {{Body}}
                </main>
                </div>
                """
            : $$"""
                <main class="content single">
                {{Body}}
                </main>
                """;

        return $$"""
               <!DOCTYPE html>
               <html>
               <head>
               <meta charset="utf-8">
               <title>{{WebUtility.HtmlEncode(Title)}}</title>
               <style>
               html { scroll-behavior: smooth; }
               body { font-family: serif; line-height: 1.45; margin: 0; }
               .page { display: grid; grid-template-columns: 17rem minmax(0, 1fr); min-height: 100vh; }
               .toc { background: #F8F3EE; border-right: 1px solid #D8C5B4; box-sizing: border-box; height: 100vh; overflow: auto; padding: 1.5rem 1rem; position: sticky; top: 0; }
               .toc-title { color: #8A4B16; font-weight: bold; margin-bottom: 0.8rem; }
               .toc-item { color: #24364A; display: block; margin: 0 0 0.35rem 0; text-decoration: none; }
               .toc-item:hover { text-decoration: underline; }
               .toc-level-2 { padding-left: 0.7rem; }
               .toc-level-3 { padding-left: 1.4rem; font-size: 0.95rem; }
               .toc-level-4, .toc-level-5, .toc-level-6 { padding-left: 2.1rem; font-size: 0.9rem; }
               .content { box-sizing: border-box; padding: 3rem; }
               .content.single { max-width: 58rem; }
               h1, h2, h3, h4, h5, h6 { color: {{HeadingColor}}; }
               p { margin: 0 0 0.8rem 0; }
               @media print {
                   .page { display: block; }
                   .toc { height: auto; position: static; }
                   .content { padding: 2rem 0 0 0; }
               }
               </style>
               </head>
               <body>
               {{PageHtml}}
               </body>
               </html>
               """;
    }
    /// <summary>
    /// Builds plain text export.
    /// </summary>
    /// <param name="UseSecondary">True to use secondary text.</param>
    /// <returns>The plain text export.</returns>
    string BuildText(bool UseSecondary)
    {
        StringBuilder Builder = new StringBuilder();
        Builder.AppendLine(GetExportTitle(fDocument, UseSecondary));
        Builder.AppendLine();

        foreach (Folder Folder in fDocument.Folders.OrderBy(Item => Item.OrderIndex))
            AppendFolderText(Builder, Folder, UseSecondary);

        foreach (TextFile File in fDocument.Files.OrderBy(Item => Item.OrderIndex))
            AppendTextFileText(Builder, File, UseSecondary);

        return Builder.ToString();
    }
    /// <summary>
    /// Appends folder plain text.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="Folder">The folder.</param>
    /// <param name="UseSecondary">True to use secondary text.</param>
    void AppendFolderText(StringBuilder Builder, Folder Folder, bool UseSecondary)
    {
        string Title = FormatTitle(Folder.LevelTitle, Folder.OrderIndex, GetExportTitle(Folder, UseSecondary), fOptions.FolderTitle);
        if (!string.IsNullOrWhiteSpace(Title))
        {
            Builder.AppendLine(Title);
            Builder.AppendLine();
        }

        foreach (Folder ChildFolder in Folder.Folders.OrderBy(Item => Item.OrderIndex))
            AppendFolderText(Builder, ChildFolder, UseSecondary);

        foreach (TextFile File in Folder.Files.OrderBy(Item => Item.OrderIndex))
            AppendTextFileText(Builder, File, UseSecondary);
    }
    /// <summary>
    /// Appends text file plain text.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="File">The text file.</param>
    /// <param name="UseSecondary">True to use secondary text.</param>
    void AppendTextFileText(StringBuilder Builder, TextFile File, bool UseSecondary)
    {
        string Title = FormatTitle("TextFile", File.OrderIndex, GetExportTitle(File, UseSecondary), fOptions.TextFileTitle);
        if (!string.IsNullOrWhiteSpace(Title))
            Builder.AppendLine(Title);

        string Text = UseSecondary ? File.Text2 : File.Text;
        if (!string.IsNullOrWhiteSpace(Text))
            Builder.AppendLine(Text.Trim());

        Builder.AppendLine();
    }
    /// <summary>
    /// Builds markdown text export.
    /// </summary>
    /// <param name="UseSecondary">True to use secondary text.</param>
    /// <returns>The markdown export.</returns>
    string BuildMarkdown(bool UseSecondary)
    {
        StringBuilder Builder = new StringBuilder();
        int RootLevel = 1;

        foreach (Folder Folder in fDocument.Folders.OrderBy(Item => Item.OrderIndex))
            AppendFolderMarkdown(Builder, Folder, UseSecondary, RootLevel);

        foreach (TextFile File in fDocument.Files.OrderBy(Item => Item.OrderIndex))
            AppendTextFileMarkdown(Builder, File, UseSecondary, RootLevel);

        return Builder.ToString();
    }
    /// <summary>
    /// Appends folder markdown.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="Folder">The folder.</param>
    /// <param name="UseSecondary">True to use secondary text.</param>
    /// <param name="Level">The heading level.</param>
    void AppendFolderMarkdown(StringBuilder Builder, Folder Folder, bool UseSecondary, int Level)
    {
        string Title = FormatTitle(Folder.LevelTitle, Folder.OrderIndex, GetExportTitle(Folder, UseSecondary), fOptions.FolderTitle);
        if (!string.IsNullOrWhiteSpace(Title))
        {
            Builder.AppendLine($"{GetMarkdownHeading(Level)} {Title}");
            Builder.AppendLine();
        }

        foreach (Folder ChildFolder in Folder.Folders.OrderBy(Item => Item.OrderIndex))
            AppendFolderMarkdown(Builder, ChildFolder, UseSecondary, Math.Min(6, Level + 1));

        foreach (TextFile File in Folder.Files.OrderBy(Item => Item.OrderIndex))
            AppendTextFileMarkdown(Builder, File, UseSecondary, Math.Min(6, Level + 1));
    }
    /// <summary>
    /// Appends text file markdown.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="File">The text file.</param>
    /// <param name="UseSecondary">True to use secondary text.</param>
    /// <param name="Level">The heading level.</param>
    void AppendTextFileMarkdown(StringBuilder Builder, TextFile File, bool UseSecondary, int Level)
    {
        string Title = FormatTitle("TextFile", File.OrderIndex, GetExportTitle(File, UseSecondary), fOptions.TextFileTitle);
        if (!string.IsNullOrWhiteSpace(Title))
        {
            Builder.AppendLine($"{GetMarkdownHeading(Level)} {Title}");
            Builder.AppendLine();
        }

        string Text = UseSecondary ? File.Text2 : File.Text;
        if (!string.IsNullOrWhiteSpace(Text))
        {
            Builder.AppendLine(ShiftMarkdownHeadings(Text.Trim(), Level));
            Builder.AppendLine();
        }
    }
    /// <summary>
    /// Shifts markdown headings inside text file text below the text file heading level.
    /// </summary>
    /// <param name="Text">The markdown text.</param>
    /// <param name="TextFileLevel">The text file heading level.</param>
    /// <returns>The markdown text with shifted headings.</returns>
    string ShiftMarkdownHeadings(string Text, int TextFileLevel)
    {
        StringBuilder Builder = new StringBuilder();
        bool InFence = false;
        string[] Lines = Text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        int BaseMarkdownLevel = GetBaseMarkdownHeadingLevel(Text);
        foreach (string Line in Lines)
        {
            if (IsMarkdownFence(Line))
                InFence = !InFence;

            if (!InFence && TryParseMarkdownHeading(Line, out int MarkdownLevel, out string HeadingText))
                Builder.AppendLine($"{GetMarkdownHeading(GetNestedHeadingLevel(TextFileLevel, MarkdownLevel, BaseMarkdownLevel))} {HeadingText}");
            else
                Builder.AppendLine(Line);
        }

        return Builder.ToString().Trim();
    }
    /// <summary>
    /// Returns markdown heading marks for a level.
    /// </summary>
    /// <param name="Level">The heading level.</param>
    /// <returns>The markdown heading marks.</returns>
    string GetMarkdownHeading(int Level)
    {
        return new string('#', Math.Clamp(Level, 1, 6));
    }
    /// <summary>
    /// Builds synopsis plain text export.
    /// </summary>
    /// <returns>The synopsis text.</returns>
    string BuildSynopsisText()
    {
        StringBuilder Builder = new StringBuilder();
        Builder.AppendLine($"● DOCUMENT: {fDocument.Title}");
        if (!string.IsNullOrWhiteSpace(fDocument.Synopsis))
            Builder.AppendLine(fDocument.Synopsis.Trim());

        foreach (Folder Folder in fDocument.Folders.OrderBy(Item => Item.OrderIndex))
            AppendFolderSynopsis(Builder, Folder);

        foreach (TextFile File in fDocument.Files.OrderBy(Item => Item.OrderIndex))
            AppendTextFileSynopsis(Builder, File);

        return Builder.ToString();
    }
    /// <summary>
    /// Appends folder synopsis.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="Folder">The folder.</param>
    void AppendFolderSynopsis(StringBuilder Builder, Folder Folder)
    {
        Builder.AppendLine();
        Builder.AppendLine($"● {Folder.LevelTitle.ToUpperInvariant()} {Folder.OrderIndex}: {Folder.Title}");
        if (!string.IsNullOrWhiteSpace(Folder.Synopsis))
            Builder.AppendLine(Folder.Synopsis.Trim());

        foreach (Folder ChildFolder in Folder.Folders.OrderBy(Item => Item.OrderIndex))
            AppendFolderSynopsis(Builder, ChildFolder);

        foreach (TextFile File in Folder.Files.OrderBy(Item => Item.OrderIndex))
            AppendTextFileSynopsis(Builder, File);
    }
    /// <summary>
    /// Appends text file synopsis.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="File">The text file.</param>
    void AppendTextFileSynopsis(StringBuilder Builder, TextFile File)
    {
        Builder.AppendLine();
        Builder.AppendLine($"● TEXTFILE {File.OrderIndex}: {File.Title}");
        if (!string.IsNullOrWhiteSpace(File.Synopsis))
            Builder.AppendLine(File.Synopsis.Trim());
    }
    /// <summary>
    /// Builds synopsis markdown export.
    /// </summary>
    /// <returns>The synopsis markdown export.</returns>
    string BuildSynopsisMarkdown()
    {
        StringBuilder Builder = new StringBuilder();
        Builder.AppendLine($"# Synopsis - {fDocument.Title}");
        Builder.AppendLine();

        if (!string.IsNullOrWhiteSpace(fDocument.Synopsis))
        {
            Builder.AppendLine(fDocument.Synopsis.Trim());
            Builder.AppendLine();
        }

        foreach (Folder Folder in fDocument.Folders.OrderBy(Item => Item.OrderIndex))
            AppendFolderSynopsisMarkdown(Builder, Folder, 2);

        foreach (TextFile File in fDocument.Files.OrderBy(Item => Item.OrderIndex))
            AppendTextFileSynopsisMarkdown(Builder, File, 2);

        return Builder.ToString();
    }
    /// <summary>
    /// Appends folder synopsis markdown.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="Folder">The folder.</param>
    /// <param name="Level">The heading level.</param>
    void AppendFolderSynopsisMarkdown(StringBuilder Builder, Folder Folder, int Level)
    {
        Builder.AppendLine($"{GetMarkdownHeading(Level)} {Folder.LevelTitle} {Folder.OrderIndex}: {Folder.Title}");
        Builder.AppendLine();

        if (!string.IsNullOrWhiteSpace(Folder.Synopsis))
        {
            Builder.AppendLine(Folder.Synopsis.Trim());
            Builder.AppendLine();
        }

        foreach (Folder ChildFolder in Folder.Folders.OrderBy(Item => Item.OrderIndex))
            AppendFolderSynopsisMarkdown(Builder, ChildFolder, Math.Min(6, Level + 1));

        foreach (TextFile File in Folder.Files.OrderBy(Item => Item.OrderIndex))
            AppendTextFileSynopsisMarkdown(Builder, File, Math.Min(6, Level + 1));
    }
    /// <summary>
    /// Appends text file synopsis markdown.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="File">The text file.</param>
    /// <param name="Level">The heading level.</param>
    void AppendTextFileSynopsisMarkdown(StringBuilder Builder, TextFile File, int Level)
    {
        Builder.AppendLine($"{GetMarkdownHeading(Level)} TextFile {File.OrderIndex}: {File.Title}");
        Builder.AppendLine();

        if (!string.IsNullOrWhiteSpace(File.Synopsis))
        {
            Builder.AppendLine(File.Synopsis.Trim());
            Builder.AppendLine();
        }
    }
    /// <summary>
    /// Builds HTML text export.
    /// </summary>
    /// <param name="UseSecondary">True to use secondary text.</param>
    /// <returns>The HTML export.</returns>
    string BuildHtml(bool UseSecondary, bool IncludeToc, bool UseBlackHeadings, bool UseOdtHeadingLevels = false)
    {
        StringBuilder Builder = new StringBuilder();
        StringBuilder TocBuilder = new StringBuilder();
        int AnchorIndex = 1;
        int Heading1Count = 0;

        int RootLevel = 1;
        int RootTextFileLevel = 1;
        foreach (Folder Folder in fDocument.Folders.OrderBy(Item => Item.OrderIndex))
            AppendFolderHtml(Builder, TocBuilder, ref AnchorIndex, ref Heading1Count, Folder, UseSecondary, RootLevel, UseOdtHeadingLevels);

        foreach (TextFile File in fDocument.Files.OrderBy(Item => Item.OrderIndex))
            AppendTextFileHtml(Builder, TocBuilder, ref AnchorIndex, ref Heading1Count, File, UseSecondary, RootTextFileLevel);

        return WrapHtml(GetExportTitle(fDocument, UseSecondary), TocBuilder.ToString(), Builder.ToString(), IncludeToc, UseBlackHeadings);
    }
    /// <summary>
    /// Appends folder HTML.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="TocBuilder">The table of contents builder.</param>
    /// <param name="AnchorIndex">The next anchor index.</param>
    /// <param name="Heading1Count">The Heading 1 count.</param>
    /// <param name="Folder">The folder.</param>
    /// <param name="UseSecondary">True to use secondary text.</param>
    /// <param name="Level">The heading level.</param>
    /// <param name="UseOdtHeadingLevels">True to use ODT heading levels.</param>
    void AppendFolderHtml(StringBuilder Builder, StringBuilder TocBuilder, ref int AnchorIndex, ref int Heading1Count, Folder Folder, bool UseSecondary, int Level, bool UseOdtHeadingLevels)
    {
        string Title = FormatTitle(Folder.LevelTitle, Folder.OrderIndex, GetExportTitle(Folder, UseSecondary), fOptions.FolderTitle);
        if (!string.IsNullOrWhiteSpace(Title))
        {
            string AnchorId = CreateAnchorId(AnchorIndex++);
            AppendTocItem(TocBuilder, Level, AnchorId, Title);
            bool UsePageBreak = fOptions.PageBreakBeforeHeading1 && Level == 1 && Heading1Count > 0;
            Builder.AppendLine(CreateHtmlHeading(Level, AnchorId, Title, UsePageBreak));
            if (Level == 1)
                Heading1Count++;
        }

        foreach (Folder ChildFolder in Folder.Folders.OrderBy(Item => Item.OrderIndex))
            AppendFolderHtml(Builder, TocBuilder, ref AnchorIndex, ref Heading1Count, ChildFolder, UseSecondary, Math.Min(6, Level + 1), UseOdtHeadingLevels);

        int TextFileLevel = Math.Min(6, Level + 1);
        foreach (TextFile File in Folder.Files.OrderBy(Item => Item.OrderIndex))
            AppendTextFileHtml(Builder, TocBuilder, ref AnchorIndex, ref Heading1Count, File, UseSecondary, TextFileLevel);
    }
    /// <summary>
    /// Appends text file HTML.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="TocBuilder">The table of contents builder.</param>
    /// <param name="AnchorIndex">The next anchor index.</param>
    /// <param name="Heading1Count">The Heading 1 count.</param>
    /// <param name="File">The text file.</param>
    /// <param name="UseSecondary">True to use secondary text.</param>
    /// <param name="Level">The heading level.</param>
    void AppendTextFileHtml(StringBuilder Builder, StringBuilder TocBuilder, ref int AnchorIndex, ref int Heading1Count, TextFile File, bool UseSecondary, int Level)
    {
        string Title = FormatTitle("TextFile", File.OrderIndex, GetExportTitle(File, UseSecondary), fOptions.TextFileTitle);
        if (!string.IsNullOrWhiteSpace(Title))
        {
            string AnchorId = CreateAnchorId(AnchorIndex++);
            AppendTocItem(TocBuilder, Level, AnchorId, Title);
            bool UsePageBreak = fOptions.PageBreakBeforeHeading1 && Level == 1 && Heading1Count > 0;
            Builder.AppendLine(CreateHtmlHeading(Level, AnchorId, Title, UsePageBreak));
            if (Level == 1)
                Heading1Count++;
        }

        string Text = UseSecondary ? File.Text2 : File.Text;
        if (fOptions.TreatTextFilesAsPlainText)
            AppendPlainTextHtml(Builder, TocBuilder, ref AnchorIndex, Text, Level);
        else
            AppendMarkdownHtml(Builder, TocBuilder, ref AnchorIndex, Text, Level);
    }
    /// <summary>
    /// Builds synopsis HTML export.
    /// </summary>
    /// <returns>The synopsis HTML export.</returns>
    string BuildSynopsisHtml(bool UseBlackHeadings)
    {
        return WrapHtml($"Synopsis - {fDocument.Title}", string.Empty, Markdig.Markdown.ToHtml(BuildSynopsisText()), false, UseBlackHeadings);
    }
    /// <summary>
    /// Writes a file.
    /// </summary>
    /// <param name="FolderPath">The folder path.</param>
    /// <param name="FileName">The file name.</param>
    /// <param name="Text">The file text.</param>
    /// <returns>The written file path.</returns>
    string WriteFile(string FolderPath, string FileName, string Text)
    {
        string FilePath = System.IO.Path.Combine(FolderPath, FileName);
        System.IO.File.WriteAllText(FilePath, Text ?? string.Empty, Encoding.UTF8);
        return FilePath;
    }
    /// <summary>
    /// Converts an HTML file to ODT.
    /// </summary>
    /// <param name="HtmlFilePath">The HTML file path.</param>
    /// <param name="FolderPath">The output folder path.</param>
    void ConvertHtmlToOdt(string HtmlFilePath, string FolderPath, string OdtFileName)
    {
        string LibreOfficePath = FindLibreOffice();
        if (string.IsNullOrWhiteSpace(LibreOfficePath))
            throw new InvalidOperationException("LibreOffice is not installed or is not available in PATH.");

        ProcessStartInfo Info = new ProcessStartInfo
        {
            FileName = LibreOfficePath,
            Arguments = $"--headless --convert-to odt --outdir \"{FolderPath}\" \"{HtmlFilePath}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process Process = Process.Start(Info) ?? throw new InvalidOperationException("Cannot start LibreOffice.");
        if (!Process.WaitForExit(300000))
        {
            Process.Kill();
            throw new TimeoutException("LibreOffice conversion timed out.");
        }

        string SourceOdtFilePath = System.IO.Path.ChangeExtension(HtmlFilePath, ".odt");
        string TargetOdtFilePath = System.IO.Path.Combine(FolderPath, OdtFileName);
        if (!System.IO.File.Exists(SourceOdtFilePath))
            throw new InvalidOperationException($"LibreOffice did not create the ODT file: {SourceOdtFilePath}");

        if (!string.Equals(SourceOdtFilePath, TargetOdtFilePath, StringComparison.OrdinalIgnoreCase))
        {
            if (System.IO.File.Exists(TargetOdtFilePath))
                System.IO.File.Delete(TargetOdtFilePath);

            System.IO.File.Move(SourceOdtFilePath, TargetOdtFilePath);
        }

        NormalizeOdtHeadings(TargetOdtFilePath);

        if (System.IO.File.Exists(HtmlFilePath))
            System.IO.File.Delete(HtmlFilePath);
    }
    /// <summary>
    /// Normalizes LibreOffice imported heading paragraphs as ODT outline headings.
    /// </summary>
    /// <param name="OdtFilePath">The ODT file path.</param>
    void NormalizeOdtHeadings(string OdtFilePath)
    {
        using ZipArchive Archive = ZipFile.Open(OdtFilePath, ZipArchiveMode.Update);
        ZipArchiveEntry ContentEntry = Archive.GetEntry("content.xml") ?? throw new InvalidOperationException("ODT content.xml not found.");
        XDocument ContentDocument;
        using (Stream Stream = ContentEntry.Open())
            ContentDocument = XDocument.Load(Stream, LoadOptions.PreserveWhitespace);

        XNamespace TextNamespace = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
        XNamespace StyleNamespace = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";
        Dictionary<string, int> HeadingStyles = GetOdtHeadingStyles(ContentDocument, TextNamespace, StyleNamespace);
        List<XElement> Paragraphs = ContentDocument.Descendants(TextNamespace + "p").ToList();
        foreach (XElement Paragraph in Paragraphs)
        {
            string StyleName = (string)Paragraph.Attribute(TextNamespace + "style-name") ?? string.Empty;
            int Level = GetOdtHeadingStyleLevel(StyleName);
            if (Level == 0 && !HeadingStyles.TryGetValue(StyleName, out Level))
                continue;

            XElement Heading = new XElement(TextNamespace + "h",
                Paragraph.Attributes().Where(Item => Item.Name != TextNamespace + "style-name"),
                new XAttribute(TextNamespace + "style-name", StyleName),
                new XAttribute(TextNamespace + "outline-level", Level),
                Paragraph.Nodes());

            Paragraph.ReplaceWith(Heading);
        }

        ContentEntry.Delete();
        ContentEntry = Archive.CreateEntry("content.xml");
        using Stream OutputStream = ContentEntry.Open();
        ContentDocument.Save(OutputStream, SaveOptions.DisableFormatting);
    }
    /// <summary>
    /// Returns paragraph styles that represent headings in an ODT document.
    /// </summary>
    /// <param name="Document">The ODT content document.</param>
    /// <param name="TextNamespace">The ODT text namespace.</param>
    /// <param name="StyleNamespace">The ODT style namespace.</param>
    /// <returns>The heading styles.</returns>
    Dictionary<string, int> GetOdtHeadingStyles(XDocument Document, XNamespace TextNamespace, XNamespace StyleNamespace)
    {
        Dictionary<string, int> Result = new Dictionary<string, int>();
        foreach (XElement Style in Document.Descendants(StyleNamespace + "style"))
        {
            string Name = (string)Style.Attribute(StyleNamespace + "name") ?? string.Empty;
            string ParentName = (string)Style.Attribute(StyleNamespace + "parent-style-name") ?? string.Empty;
            int Level = GetOdtHeadingStyleLevel(Name);
            if (Level == 0)
                Level = GetOdtHeadingStyleLevel(ParentName);

            if (Level > 0)
                Result[Name] = Level;
        }

        return Result;
    }
    /// <summary>
    /// Returns the heading level of an ODT heading style.
    /// </summary>
    /// <param name="StyleName">The style name.</param>
    /// <returns>The heading level, or zero.</returns>
    int GetOdtHeadingStyleLevel(string StyleName)
    {
        Match Match = Regex.Match(StyleName ?? string.Empty, @"^Heading_20_([1-6])$");
        return Match.Success ? int.Parse(Match.Groups[1].Value, CultureInfo.InvariantCulture) : 0;
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the DocumentExporter class.
    /// </summary>
    /// <param name="Document">The exported document.</param>
    /// <param name="Options">The export options.</param>
    public DocumentExporter(Document Document, ExportOptions Options)
    {
        fDocument = Document ?? throw new ArgumentNullException(nameof(Document));
        fOptions = Options ?? throw new ArgumentNullException(nameof(Options));
    }

    // ● public
    /// <summary>
    /// Executes the export.
    /// </summary>
    /// <returns>The export folder path.</returns>
    public string Execute()
    {
        string ExportRootPath = System.IO.Path.Combine(fDocument.FolderPath, "Export");
        string ExportFolderPath = System.IO.Path.Combine(ExportRootPath, DateTime.Now.ToFileName());
        System.IO.Directory.CreateDirectory(ExportFolderPath);

        if (fOptions.Source.HasFlag(ExportSource.Text))
        {
            ExportLanguage[] Languages = { ExportLanguage.Primary, ExportLanguage.Secondary };
            foreach (ExportLanguage Language in Languages)
            {
                if (!fOptions.Language.HasFlag(Language))
                    continue;

                bool UseSecondary = Language == ExportLanguage.Secondary;
                string BaseName = SafeTitle(GetExportTitle(fDocument, UseSecondary));
                string Suffix = UseSecondary ? "_Secondary" : "_Primary";
                string Text = BuildText(UseSecondary);
                if (fOptions.Format.HasFlag(ExportFormat.Txt))
                    WriteFile(ExportFolderPath, $"{BaseName}{Suffix}.txt", Text);

                if (fOptions.Format.HasFlag(ExportFormat.Markdown))
                {
                    string Markdown = BuildMarkdown(UseSecondary);
                    WriteFile(ExportFolderPath, $"{BaseName}{Suffix}.md", Markdown);
                }

                if (fOptions.Format.HasFlag(ExportFormat.Html))
                {
                    string Html = BuildHtml(UseSecondary, true, false);
                    WriteFile(ExportFolderPath, $"{BaseName}{Suffix}.html", Html);
                }

                if (fOptions.Format.HasFlag(ExportFormat.Odt))
                {
                    string OdtSourceHtml = BuildHtml(UseSecondary, false, true, true);
                    string OdtSourceHtmlFilePath = WriteFile(ExportFolderPath, $"{BaseName}{Suffix}_ODT_SOURCE.html", OdtSourceHtml);
                    ConvertHtmlToOdt(OdtSourceHtmlFilePath, ExportFolderPath, $"{BaseName}{Suffix}.odt");
                }
            }
        }

        if (fOptions.Source.HasFlag(ExportSource.Synopsis))
        {
            string BaseName = SafeTitle(fDocument.Title);
            string SynopsisText = BuildSynopsisText();
            if (fOptions.Format.HasFlag(ExportFormat.Txt))
                WriteFile(ExportFolderPath, $"{BaseName}_Synopsis.txt", SynopsisText);

            if (fOptions.Format.HasFlag(ExportFormat.Markdown))
            {
                string SynopsisMarkdown = BuildSynopsisMarkdown();
                WriteFile(ExportFolderPath, $"{BaseName}_Synopsis.md", SynopsisMarkdown);
            }

            if (fOptions.Format.HasFlag(ExportFormat.Html))
            {
                string SynopsisHtml = BuildSynopsisHtml(false);
                WriteFile(ExportFolderPath, $"{BaseName}_Synopsis.html", SynopsisHtml);
            }

            if (fOptions.Format.HasFlag(ExportFormat.Odt))
            {
                string SynopsisHtml = BuildSynopsisHtml(true);
                string OdtSourceHtmlFilePath = WriteFile(ExportFolderPath, $"{BaseName}_Synopsis_ODT_SOURCE.html", SynopsisHtml);
                ConvertHtmlToOdt(OdtSourceHtmlFilePath, ExportFolderPath, $"{BaseName}_Synopsis.odt");
            }
        }

        return ExportFolderPath;
    }
}
