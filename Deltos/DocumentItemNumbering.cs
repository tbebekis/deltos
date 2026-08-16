// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Provides document item numbering compatible with internal markdown export.
/// </summary>
static public class DocumentItemNumbering
{
    // ● private
    /// <summary>
    /// Formats a number segment.
    /// </summary>
    /// <param name="Value">The segment value.</param>
    /// <param name="Padded">True to use export padding.</param>
    /// <returns>The formatted number segment.</returns>
    static string FormatSegment(int Value, bool Padded)
    {
        return Padded ? Value.ToString("000", CultureInfo.InvariantCulture) : Value.ToString(CultureInfo.InvariantCulture);
    }
    /// <summary>
    /// Builds item numbers for direct document children.
    /// </summary>
    /// <param name="Result">The number map.</param>
    /// <param name="Items">The document child items.</param>
    /// <param name="DocumentSegments">The document number segments.</param>
    /// <param name="Padded">True to use export padding.</param>
    static void BuildDocumentChildNumbers(Dictionary<BaseItem, string> Result, List<BaseItem> Items, List<string> DocumentSegments, bool Padded)
    {
        int ContainerIndex = 0;
        int TextFileIndex = 0;
        bool InVirtualContainer = false;

        foreach (BaseItem Item in Items)
        {
            Folder Folder = Item as Folder;
            if (Folder != null)
            {
                ContainerIndex++;
                TextFileIndex = 0;
                InVirtualContainer = false;

                List<string> Segments = new List<string>(DocumentSegments) { FormatSegment(ContainerIndex, Padded) };
                Result[Folder] = JoinSegments(Segments);
                BuildChildNumbers(Result, Folder.GetChildItems(), Segments, Padded);
                continue;
            }

            TextFile File = Item as TextFile;
            if (File != null)
            {
                if (!InVirtualContainer)
                {
                    if (ContainerIndex > 0)
                        ContainerIndex++;

                    TextFileIndex = 0;
                    InVirtualContainer = true;
                }

                TextFileIndex++;
                List<string> Segments = new List<string>(DocumentSegments)
                {
                    FormatSegment(ContainerIndex, Padded),
                    FormatSegment(TextFileIndex, Padded)
                };
                Result[File] = JoinSegments(Segments);
            }
        }
    }
    /// <summary>
    /// Builds item numbers recursively.
    /// </summary>
    /// <param name="Result">The number map.</param>
    /// <param name="Items">The child items.</param>
    /// <param name="ParentSegments">The parent number segments.</param>
    /// <param name="Padded">True to use export padding.</param>
    static void BuildChildNumbers(Dictionary<BaseItem, string> Result, List<BaseItem> Items, List<string> ParentSegments, bool Padded)
    {
        foreach (BaseItem Item in Items)
        {
            List<string> Segments = new List<string>(ParentSegments) { GetTypeRelativeOrderSegment(Item, Items, Padded) };
            Result[Item] = JoinSegments(Segments);

            Folder Folder = Item as Folder;
            if (Folder != null)
                BuildChildNumbers(Result, Folder.GetChildItems(), Segments, Padded);
        }
    }
    /// <summary>
    /// Returns the type-relative item order segment.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <param name="Siblings">The sibling items.</param>
    /// <param name="Padded">True to use export padding.</param>
    /// <returns>The item order segment.</returns>
    static string GetTypeRelativeOrderSegment(BaseItem Item, List<BaseItem> Siblings, bool Padded)
    {
        int OrderIndex = 0;
        foreach (BaseItem Sibling in Siblings)
        {
            if ((Item is Folder && Sibling is Folder) || (Item is TextFile && Sibling is TextFile))
                OrderIndex++;

            if (ReferenceEquals(Item, Sibling))
                break;
        }

        return FormatSegment(OrderIndex, Padded);
    }
    /// <summary>
    /// Joins number segments.
    /// </summary>
    /// <param name="Segments">The number segments.</param>
    /// <returns>The joined number.</returns>
    static string JoinSegments(List<string> Segments)
    {
        return string.Join(".", Segments);
    }

    // ● static public
    /// <summary>
    /// Builds item numbers for a document.
    /// </summary>
    /// <param name="Document">The document.</param>
    /// <param name="Padded">True to use export padding.</param>
    /// <returns>The item number map.</returns>
    static public Dictionary<BaseItem, string> BuildNumbers(Document Document, bool Padded)
    {
        Dictionary<BaseItem, string> Result = new Dictionary<BaseItem, string>();
        if (Document == null)
            return Result;

        List<string> DocumentSegments = new List<string> { FormatSegment(Document.OrderIndex, Padded) };
        Result[Document] = JoinSegments(DocumentSegments);
        BuildDocumentChildNumbers(Result, Document.GetChildItems(), DocumentSegments, Padded);
        return Result;
    }
    /// <summary>
    /// Returns an item number.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <param name="Padded">True to use export padding.</param>
    /// <returns>The item number, if any; otherwise empty.</returns>
    static public string GetNumber(BaseItem Item, bool Padded)
    {
        Document Document = Item?.Document;
        if (Document == null)
            return string.Empty;

        Dictionary<BaseItem, string> Numbers = BuildNumbers(Document, Padded);
        return Numbers.TryGetValue(Item, out string Result) ? Result : string.Empty;
    }
    /// <summary>
    /// Returns an item title prefixed with its display number when enabled.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <param name="Title">The item title.</param>
    /// <returns>The display title.</returns>
    static public string GetNumberedDisplayTitle(BaseItem Item, string Title)
    {
        string Result = Title ?? string.Empty;
        if (AppHost.Settings?.ShowDocumentItemNumbers != true)
            return Result;

        string Number = GetNumber(Item, false);
        return string.IsNullOrWhiteSpace(Number) ? Result : $"{Number}. {Result}";
    }
}
