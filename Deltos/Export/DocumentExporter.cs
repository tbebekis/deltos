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
    /// <summary>
    /// The current export folder path.
    /// </summary>
    string fExportFolderPath = string.Empty;
    /// <summary>
    /// The copied export image paths by source file path.
    /// </summary>
    readonly Dictionary<string, string> fExportImagePaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
    /// Returns true if a URL should not be copied as a local export image.
    /// </summary>
    /// <param name="Url">The URL.</param>
    /// <returns>True if the URL is external or embedded.</returns>
    static bool IsExternalImageUrl(string Url)
    {
        string Value = (Url ?? string.Empty).Trim();
        return Value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || Value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || Value.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            || Value.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase);
    }
    /// <summary>
    /// Removes any query string or fragment from an image path.
    /// </summary>
    /// <param name="ImagePath">The image path.</param>
    /// <returns>The image path without query string or fragment.</returns>
    static string RemoveImagePathSuffix(string ImagePath)
    {
        string Result = ImagePath ?? string.Empty;
        int QueryIndex = Result.IndexOf('?');
        int FragmentIndex = Result.IndexOf('#');
        int Index = -1;
        if (QueryIndex >= 0 && FragmentIndex >= 0)
            Index = Math.Min(QueryIndex, FragmentIndex);
        else if (QueryIndex >= 0)
            Index = QueryIndex;
        else if (FragmentIndex >= 0)
            Index = FragmentIndex;

        return Index < 0 ? Result : Result.Substring(0, Index);
    }
    /// <summary>
    /// Returns a safe export image file name.
    /// </summary>
    /// <param name="FileName">The file name.</param>
    /// <returns>The safe export image file name.</returns>
    static string SafeExportImageFileName(string FileName)
    {
        string Result = string.IsNullOrWhiteSpace(FileName) ? "Image" : FileName.Trim();
        foreach (char Char in System.IO.Path.GetInvalidFileNameChars())
            Result = Result.Replace(Char, '_');

        return string.IsNullOrWhiteSpace(Result) ? "Image" : Result;
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
        return FormatTitle(Word, OrderIndex.ToString(CultureInfo.InvariantCulture), Title, Options);
    }
    /// <summary>
    /// Formats a title.
    /// </summary>
    /// <param name="Word">The item word.</param>
    /// <param name="NumberText">The item number text.</param>
    /// <param name="Title">The item title.</param>
    /// <param name="Options">The title options.</param>
    /// <returns>The formatted title.</returns>
    static string FormatTitle(string Word, string NumberText, string Title, ExportTitleOptions Options)
    {
        if (Options == ExportTitleOptions.None)
            return string.Empty;

        StringBuilder Builder = new StringBuilder();
        if (Options.HasFlag(ExportTitleOptions.Bullet))
            Builder.Append("●");

        if (Options.HasFlag(ExportTitleOptions.Word))
            AppendTitlePart(Builder, Word);

        bool HasNumber = Options.HasFlag(ExportTitleOptions.Number) && !string.IsNullOrWhiteSpace(NumberText);
        if (HasNumber)
            AppendTitlePart(Builder, NumberText);

        if (Options.HasFlag(ExportTitleOptions.Title))
        {
            if (HasNumber)
                Builder.Append($". {Title}");
            else if (Options.HasFlag(ExportTitleOptions.Word))
                Builder.Append($": {Title}");
            else
                AppendTitlePart(Builder, Title);
        }

        return Builder.ToString();
    }
    /// <summary>
    /// Formats an item title.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <param name="Word">The item word.</param>
    /// <param name="Title">The item title.</param>
    /// <param name="Options">The title options.</param>
    /// <returns>The formatted item title.</returns>
    static string FormatItemTitle(BaseItem Item, string Word, string Title, ExportTitleOptions Options)
    {
        if (Item == null || !Item.IncludeTitleInOutput)
            return string.Empty;

        return FormatTitle(Word, GetItemNumberText(Item), Title, Options);
    }
    /// <summary>
    /// Returns the item number text.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <returns>The item number text.</returns>
    static string GetItemNumberText(BaseItem Item)
    {
        if (Item == null)
            return string.Empty;

        if (Item.Numbering == ItemNumbering.None)
            return string.Empty;

        if (Item.Numbering == ItemNumbering.Custom)
            return Item.CustomNumbering;

        return Item.OrderIndex.ToString(CultureInfo.InvariantCulture);
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
    void AppendMarkdownHtml(StringBuilder Builder, StringBuilder TocBuilder, ref int AnchorIndex, string MarkdownText, int TextFileLevel, bool IncludeToc)
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
                if (IncludeToc)
                    AppendTocItem(TocBuilder, Level, AnchorId, HeadingText);

                TextBuilder.AppendLine(CreateHtmlHeading(Level, AnchorId, HeadingText, false));
            }
            else
            {
                TextBuilder.AppendLine(Line);
            }
        }

        Builder.AppendLine(PrepareExportImageHtml(Markdig.Markdown.ToHtml(TextBuilder.ToString())));
    }
    /// <summary>
    /// Appends plain text as HTML paragraphs.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="TocBuilder">The table of contents builder.</param>
    /// <param name="AnchorIndex">The next anchor index.</param>
    /// <param name="Text">The plain text.</param>
    /// <param name="TextFileLevel">The text file heading level.</param>
    static void AppendPlainTextHtml(StringBuilder Builder, StringBuilder TocBuilder, ref int AnchorIndex, string Text, int TextFileLevel, bool IncludeToc)
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
                if (IncludeToc)
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
    /// Appends a page break marker to a markdown export.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    static void AppendMarkdownPageBreak(StringBuilder Builder)
    {
        Builder.AppendLine("<div style=\"page-break-before: always; break-before: page;\"></div>");
        Builder.AppendLine();
    }
    /// <summary>
    /// Appends a page break marker to a plain text export.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    static void AppendTextPageBreak(StringBuilder Builder)
    {
        Builder.AppendLine("\f");
    }
    /// <summary>
    /// Formats a synopsis item title.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <param name="Word">The item word.</param>
    /// <returns>The synopsis item title.</returns>
    static string FormatSynopsisTitle(BaseItem Item, string Word)
    {
        if (Item == null || !Item.IncludeTitleInOutput)
            return string.Empty;

        string NumberText = GetItemNumberText(Item);
        return string.IsNullOrWhiteSpace(NumberText)
            ? $"● {Word}: {Item.Title}"
            : $"● {Word} {NumberText}: {Item.Title}";
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
               img { display: block; height: auto; margin: 1rem auto; max-width: 100%; }
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
    /// Rewrites exported HTML image paths to copied export images.
    /// </summary>
    /// <param name="Html">The HTML text.</param>
    /// <returns>The HTML text with export-local image paths.</returns>
    string PrepareExportImageHtml(string Html)
    {
        if (string.IsNullOrWhiteSpace(Html))
            return Html ?? string.Empty;

        return Regex.Replace(Html, "(<img\\b[^>]*?\\bsrc\\s*=\\s*)([\"'])(.*?)(\\2)", Match =>
        {
            string Source = WebUtility.HtmlDecode(Match.Groups[3].Value);
            string ExportPath = CopyExportImage(Source, out int Width, out int Height);
            if (string.IsNullOrWhiteSpace(ExportPath))
                return Match.Value;

            string SizeText = Width > 0 && Height > 0
                ? $" width=\"{Width.ToString(CultureInfo.InvariantCulture)}\" height=\"{Height.ToString(CultureInfo.InvariantCulture)}\""
                : string.Empty;

            return Match.Groups[1].Value + Match.Groups[2].Value + WebUtility.HtmlEncode(ExportPath) + Match.Groups[2].Value + SizeText;
        }, RegexOptions.IgnoreCase);
    }
    /// <summary>
    /// Copies an image used by HTML export and returns its export-local relative path.
    /// </summary>
    /// <param name="ImagePath">The markdown image path.</param>
    /// <returns>The export-local relative path.</returns>
    string CopyExportImage(string ImagePath, out int Width, out int Height)
    {
        Width = 0;
        Height = 0;
        string SourcePath = ResolveExportImagePath(ImagePath);
        if (string.IsNullOrWhiteSpace(SourcePath))
            return string.Empty;

        SourcePath = System.IO.Path.GetFullPath(SourcePath);
        if (fExportImagePaths.TryGetValue(SourcePath, out string Result))
        {
            GetExportImageSize(SourcePath, out Width, out Height);
            return Result;
        }

        string ExportImagesFolderPath = System.IO.Path.Combine(fExportFolderPath, Project.ImagesFolderName);
        System.IO.Directory.CreateDirectory(ExportImagesFolderPath);

        string FileName = SafeExportImageFileName(System.IO.Path.GetFileName(SourcePath));
        string FileStem = System.IO.Path.GetFileNameWithoutExtension(FileName);
        string Extension = System.IO.Path.GetExtension(FileName);
        string DestFileName = FileName;
        string DestFilePath = System.IO.Path.Combine(ExportImagesFolderPath, DestFileName);
        int Index = 2;
        while (System.IO.File.Exists(DestFilePath))
        {
            if (string.Equals(System.IO.Path.GetFullPath(DestFilePath), SourcePath, StringComparison.OrdinalIgnoreCase))
                break;

            DestFileName = $"{FileStem}-{Index}{Extension}";
            DestFilePath = System.IO.Path.Combine(ExportImagesFolderPath, DestFileName);
            Index++;
        }

        if (!System.IO.File.Exists(DestFilePath))
            System.IO.File.Copy(SourcePath, DestFilePath);

        Result = $"{Project.ImagesFolderName}/{DestFileName}";
        fExportImagePaths[SourcePath] = Result;
        GetExportImageSize(SourcePath, out Width, out Height);
        return Result;
    }
    /// <summary>
    /// Returns capped export image dimensions.
    /// </summary>
    /// <param name="ImagePath">The image file path.</param>
    /// <param name="Width">The image width.</param>
    /// <param name="Height">The image height.</param>
    void GetExportImageSize(string ImagePath, out int Width, out int Height)
    {
        Width = 0;
        Height = 0;
        try
        {
            using Bitmap ImageBitmap = new Bitmap(ImagePath);
            Width = ImageBitmap.PixelSize.Width;
            Height = ImageBitmap.PixelSize.Height;
            int MaxWidth = Math.Clamp(fOptions.ImageMaxWidth, 100, 2000);
            if (Width > MaxWidth && Height > 0)
            {
                double Ratio = (double)MaxWidth / Width;
                Width = MaxWidth;
                Height = Math.Max(1, (int)Math.Round(Height * Ratio));
            }
        }
        catch
        {
            Width = 0;
            Height = 0;
        }
    }
    /// <summary>
    /// Resolves a markdown image path for export.
    /// </summary>
    /// <param name="ImagePath">The markdown image path.</param>
    /// <returns>The resolved file path, if found; otherwise empty.</returns>
    string ResolveExportImagePath(string ImagePath)
    {
        string Result = WebUtility.HtmlDecode(ImagePath ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(Result) || IsExternalImageUrl(Result))
            return string.Empty;

        Result = RemoveImagePathSuffix(Result);
        if (Uri.TryCreate(Result, UriKind.Absolute, out Uri ImageUri) && ImageUri.IsFile)
            Result = ImageUri.LocalPath;

        try
        {
            Result = Uri.UnescapeDataString(Result);
        }
        catch
        {
        }

        if (System.IO.Path.IsPathFullyQualified(Result) && System.IO.File.Exists(Result))
            return Result;

        Project Project = fDocument.Project;
        if (Project == null)
            return string.Empty;

        string ImagesPath = System.IO.Path.Combine(Project.ImagesFolderPath, Result);
        if (System.IO.File.Exists(ImagesPath))
            return ImagesPath;

        string ProjectPath = System.IO.Path.Combine(Project.FolderPath, Result);
        if (System.IO.File.Exists(ProjectPath))
            return ProjectPath;

        return string.Empty;
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

        foreach (BaseItem Item in fDocument.GetChildItems())
            AppendItemText(Builder, Item, UseSecondary);

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
        string Title = FormatItemTitle(Folder, Folder.LevelTitle, GetExportTitle(Folder, UseSecondary), fOptions.FolderTitle);
        if (!string.IsNullOrWhiteSpace(Title))
        {
            if (Folder.PageBreakBefore)
                AppendTextPageBreak(Builder);

            Builder.AppendLine(Title);
            Builder.AppendLine();
        }

        foreach (BaseItem Item in Folder.GetChildItems())
            AppendItemText(Builder, Item, UseSecondary);
    }
    /// <summary>
    /// Appends item plain text.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="Item">The item.</param>
    /// <param name="UseSecondary">True to use secondary text.</param>
    void AppendItemText(StringBuilder Builder, BaseItem Item, bool UseSecondary)
    {
        Folder Folder = Item as Folder;
        if (Folder != null)
        {
            AppendFolderText(Builder, Folder, UseSecondary);
            return;
        }

        TextFile File = Item as TextFile;
        if (File != null)
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
        string Title = FormatItemTitle(File, "TextFile", GetExportTitle(File, UseSecondary), fOptions.TextFileTitle);
        if (!string.IsNullOrWhiteSpace(Title))
        {
            if (File.PageBreakBefore)
                AppendTextPageBreak(Builder);

            Builder.AppendLine(Title);
        }

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

        foreach (BaseItem Item in fDocument.GetChildItems())
            AppendItemMarkdown(Builder, Item, UseSecondary, RootLevel);

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
        string Title = FormatItemTitle(Folder, Folder.LevelTitle, GetExportTitle(Folder, UseSecondary), fOptions.FolderTitle);
        if (!string.IsNullOrWhiteSpace(Title))
        {
            if (Folder.PageBreakBefore)
                AppendMarkdownPageBreak(Builder);

            Builder.AppendLine($"{GetMarkdownHeading(Level)} {Title}");
            Builder.AppendLine();
        }

        int ChildLevel = string.IsNullOrWhiteSpace(Title) ? Level : Math.Min(6, Level + 1);
        foreach (BaseItem Item in Folder.GetChildItems())
            AppendItemMarkdown(Builder, Item, UseSecondary, ChildLevel);
    }
    /// <summary>
    /// Appends item markdown.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="Item">The item.</param>
    /// <param name="UseSecondary">True to use secondary text.</param>
    /// <param name="Level">The heading level.</param>
    void AppendItemMarkdown(StringBuilder Builder, BaseItem Item, bool UseSecondary, int Level)
    {
        Folder Folder = Item as Folder;
        if (Folder != null)
        {
            AppendFolderMarkdown(Builder, Folder, UseSecondary, Level);
            return;
        }

        TextFile File = Item as TextFile;
        if (File != null)
            AppendTextFileMarkdown(Builder, File, UseSecondary, Level);
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
        string Title = FormatItemTitle(File, "TextFile", GetExportTitle(File, UseSecondary), fOptions.TextFileTitle);
        if (!string.IsNullOrWhiteSpace(Title))
        {
            if (File.PageBreakBefore)
                AppendMarkdownPageBreak(Builder);

            Builder.AppendLine($"{GetMarkdownHeading(Level)} {Title}");
            Builder.AppendLine();
        }

        string Text = UseSecondary ? File.Text2 : File.Text;
        if (!string.IsNullOrWhiteSpace(Text))
        {
            int TextFileLevel = string.IsNullOrWhiteSpace(Title) ? Math.Max(1, Level - 1) : Level;
            Builder.AppendLine(ShiftMarkdownHeadings(Text.Trim(), TextFileLevel));
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

        foreach (BaseItem Item in fDocument.GetChildItems())
            AppendItemSynopsis(Builder, Item);

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
        string Title = FormatSynopsisTitle(Folder, Folder.LevelTitle.ToUpperInvariant());
        if (!string.IsNullOrWhiteSpace(Title))
            Builder.AppendLine(Title);
        if (!string.IsNullOrWhiteSpace(Folder.Synopsis))
            Builder.AppendLine(Folder.Synopsis.Trim());

        foreach (BaseItem Item in Folder.GetChildItems())
            AppendItemSynopsis(Builder, Item);
    }
    /// <summary>
    /// Appends item synopsis.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="Item">The item.</param>
    void AppendItemSynopsis(StringBuilder Builder, BaseItem Item)
    {
        Folder Folder = Item as Folder;
        if (Folder != null)
        {
            AppendFolderSynopsis(Builder, Folder);
            return;
        }

        TextFile File = Item as TextFile;
        if (File != null)
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
        string Title = FormatSynopsisTitle(File, "TEXTFILE");
        if (!string.IsNullOrWhiteSpace(Title))
            Builder.AppendLine(Title);
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

        foreach (BaseItem Item in fDocument.GetChildItems())
            AppendItemSynopsisMarkdown(Builder, Item, 2);

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
        if (Folder.IncludeTitleInOutput)
        {
            if (Folder.PageBreakBefore)
                AppendMarkdownPageBreak(Builder);

            Builder.AppendLine($"{GetMarkdownHeading(Level)} {Folder.LevelTitle} {GetItemNumberText(Folder)}: {Folder.Title}");
            Builder.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(Folder.Synopsis))
        {
            Builder.AppendLine(Folder.Synopsis.Trim());
            Builder.AppendLine();
        }

        int ChildLevel = Folder.IncludeTitleInOutput ? Math.Min(6, Level + 1) : Level;
        foreach (BaseItem Item in Folder.GetChildItems())
            AppendItemSynopsisMarkdown(Builder, Item, ChildLevel);
    }
    /// <summary>
    /// Appends item synopsis markdown.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="Item">The item.</param>
    /// <param name="Level">The heading level.</param>
    void AppendItemSynopsisMarkdown(StringBuilder Builder, BaseItem Item, int Level)
    {
        Folder Folder = Item as Folder;
        if (Folder != null)
        {
            AppendFolderSynopsisMarkdown(Builder, Folder, Level);
            return;
        }

        TextFile File = Item as TextFile;
        if (File != null)
            AppendTextFileSynopsisMarkdown(Builder, File, Level);
    }
    /// <summary>
    /// Appends text file synopsis markdown.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="File">The text file.</param>
    /// <param name="Level">The heading level.</param>
    void AppendTextFileSynopsisMarkdown(StringBuilder Builder, TextFile File, int Level)
    {
        if (File.IncludeTitleInOutput)
        {
            if (File.PageBreakBefore)
                AppendMarkdownPageBreak(Builder);

            Builder.AppendLine($"{GetMarkdownHeading(Level)} TextFile {GetItemNumberText(File)}: {File.Title}");
            Builder.AppendLine();
        }

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
        foreach (BaseItem Item in fDocument.GetChildItems())
            AppendItemHtml(Builder, TocBuilder, ref AnchorIndex, ref Heading1Count, Item, UseSecondary, RootLevel, UseOdtHeadingLevels);

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
        string Title = FormatItemTitle(Folder, Folder.LevelTitle, GetExportTitle(Folder, UseSecondary), fOptions.FolderTitle);
        if (!string.IsNullOrWhiteSpace(Title))
        {
            string AnchorId = CreateAnchorId(AnchorIndex++);
            if (Folder.IncludeInToc)
                AppendTocItem(TocBuilder, Level, AnchorId, Title);

            bool UsePageBreak = Folder.PageBreakBefore;
            Builder.AppendLine(CreateHtmlHeading(Level, AnchorId, Title, UsePageBreak));
            if (Level == 1)
                Heading1Count++;
        }

        int TextFileLevel = string.IsNullOrWhiteSpace(Title) ? Level : Math.Min(6, Level + 1);
        foreach (BaseItem Item in Folder.GetChildItems())
            AppendItemHtml(Builder, TocBuilder, ref AnchorIndex, ref Heading1Count, Item, UseSecondary, TextFileLevel, UseOdtHeadingLevels);
    }
    /// <summary>
    /// Appends item HTML.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="TocBuilder">The table of contents builder.</param>
    /// <param name="AnchorIndex">The next anchor index.</param>
    /// <param name="Heading1Count">The Heading 1 count.</param>
    /// <param name="Item">The item.</param>
    /// <param name="UseSecondary">True to use secondary text.</param>
    /// <param name="Level">The heading level.</param>
    /// <param name="UseOdtHeadingLevels">True to use ODT heading levels.</param>
    void AppendItemHtml(StringBuilder Builder, StringBuilder TocBuilder, ref int AnchorIndex, ref int Heading1Count, BaseItem Item, bool UseSecondary, int Level, bool UseOdtHeadingLevels)
    {
        Folder Folder = Item as Folder;
        if (Folder != null)
        {
            AppendFolderHtml(Builder, TocBuilder, ref AnchorIndex, ref Heading1Count, Folder, UseSecondary, Level, UseOdtHeadingLevels);
            return;
        }

        TextFile File = Item as TextFile;
        if (File != null)
            AppendTextFileHtml(Builder, TocBuilder, ref AnchorIndex, ref Heading1Count, File, UseSecondary, Level);
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
        string Title = FormatItemTitle(File, "TextFile", GetExportTitle(File, UseSecondary), fOptions.TextFileTitle);
        if (!string.IsNullOrWhiteSpace(Title))
        {
            string AnchorId = CreateAnchorId(AnchorIndex++);
            if (File.IncludeInToc)
                AppendTocItem(TocBuilder, Level, AnchorId, Title);

            bool UsePageBreak = File.PageBreakBefore;
            Builder.AppendLine(CreateHtmlHeading(Level, AnchorId, Title, UsePageBreak));
            if (Level == 1)
                Heading1Count++;
        }

        string Text = UseSecondary ? File.Text2 : File.Text;
        int TextFileLevel = string.IsNullOrWhiteSpace(Title) ? Math.Max(1, Level - 1) : Level;
        if (fOptions.TreatTextFilesAsPlainText)
            AppendPlainTextHtml(Builder, TocBuilder, ref AnchorIndex, Text, TextFileLevel, File.IncludeInToc);
        else
            AppendMarkdownHtml(Builder, TocBuilder, ref AnchorIndex, Text, TextFileLevel, File.IncludeInToc);
    }
    /// <summary>
    /// Builds synopsis HTML export.
    /// </summary>
    /// <returns>The synopsis HTML export.</returns>
    string BuildSynopsisHtml(bool UseBlackHeadings)
    {
        return WrapHtml($"Synopsis - {fDocument.Title}", string.Empty, PrepareExportImageHtml(Markdig.Markdown.ToHtml(BuildSynopsisText())), false, UseBlackHeadings);
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
        fExportFolderPath = ExportFolderPath;
        fExportImagePaths.Clear();

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
