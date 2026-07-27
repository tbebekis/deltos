// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Export;

/// <summary>
/// Specifies exported languages.
/// </summary>
[Flags]
public enum ExportLanguage
{
    /// <summary>
    /// No language.
    /// </summary>
    None = 0,
    /// <summary>
    /// Primary text language.
    /// </summary>
    Primary = 1,
    /// <summary>
    /// Secondary text language.
    /// </summary>
    Secondary = 2
}

/// <summary>
/// Specifies exported content sources.
/// </summary>
[Flags]
public enum ExportSource
{
    /// <summary>
    /// No source.
    /// </summary>
    None = 0,
    /// <summary>
    /// TextFile text.
    /// </summary>
    Text = 1,
    /// <summary>
    /// Document, Folder and TextFile synopsis text.
    /// </summary>
    Synopsis = 2
}

/// <summary>
/// Specifies export file formats.
/// </summary>
[Flags]
public enum ExportFormat
{
    /// <summary>
    /// No format.
    /// </summary>
    None = 0,
    /// <summary>
    /// Plain text format.
    /// </summary>
    Txt = 1,
    /// <summary>
    /// HTML format.
    /// </summary>
    Html = 2,
    /// <summary>
    /// OpenDocument text format.
    /// </summary>
    Odt = 4,
    /// <summary>
    /// Markdown format.
    /// </summary>
    Markdown = 8,
    /// <summary>
    /// Internal markdown files, one file per TextFile.
    /// </summary>
    InternalMarkdown = 16
}

/// <summary>
/// Specifies title rendering options.
/// </summary>
[Flags]
public enum ExportTitleOptions
{
    /// <summary>
    /// No title.
    /// </summary>
    None = 0,
    /// <summary>
    /// Add a bullet before the title.
    /// </summary>
    Bullet = 1,
    /// <summary>
    /// Add an item number.
    /// </summary>
    Number = 2,
    /// <summary>
    /// Add the item type word.
    /// </summary>
    Word = 4,
    /// <summary>
    /// Add the item title.
    /// </summary>
    Title = 8
}

/// <summary>
/// Specifies document export options.
/// </summary>
public class ExportOptions
{
    // ● public
    /// <summary>
    /// Clears all option flags.
    /// </summary>
    public void Clear()
    {
        Language = ExportLanguage.None;
        Source = ExportSource.None;
        Format = ExportFormat.None;
        FolderTitle = ExportTitleOptions.None;
        TextFileTitle = ExportTitleOptions.None;
        TreatTextFilesAsPlainText = false;
        ImageMaxWidth = 400;
        PageBreakBeforeHeading1 = false;
    }

    // ● properties
    /// <summary>
    /// Gets or sets exported languages.
    /// </summary>
    public ExportLanguage Language { get; set; } = ExportLanguage.Primary;
    /// <summary>
    /// Gets or sets exported content sources.
    /// </summary>
    public ExportSource Source { get; set; } = ExportSource.Text;
    /// <summary>
    /// Gets or sets export file formats.
    /// </summary>
    public ExportFormat Format { get; set; } = ExportFormat.Txt;
    /// <summary>
    /// Gets or sets folder title options.
    /// </summary>
    public ExportTitleOptions FolderTitle { get; set; } = ExportTitleOptions.Title;
    /// <summary>
    /// Gets or sets text file title options.
    /// </summary>
    public ExportTitleOptions TextFileTitle { get; set; } = ExportTitleOptions.Title;
    /// <summary>
    /// Gets or sets a value indicating whether text files are exported as plain text instead of markdown.
    /// </summary>
    public bool TreatTextFilesAsPlainText { get; set; } = false;
    /// <summary>
    /// Gets or sets the maximum exported image width in pixels.
    /// </summary>
    public int ImageMaxWidth { get; set; } = 400;
    /// <summary>
    /// Gets or sets a value indicating whether Heading 1 starts on a new page in HTML and ODT exports.
    /// </summary>
    public bool PageBreakBeforeHeading1 { get; set; }
}
