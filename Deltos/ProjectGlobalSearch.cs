// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Executes project-wide searches and returns link items.
/// </summary>
static public class ProjectGlobalSearch
{
    // ● private
    /// <summary>
    /// Trims the search term and detects whole-word search.
    /// </summary>
    /// <param name="Term">The raw search term.</param>
    /// <param name="WholeWord">True when the search is whole-word.</param>
    /// <returns>The normalized search term.</returns>
    static string NormalizeTerm(string Term, out bool WholeWord)
    {
        string Result = Term == null ? string.Empty : Term.Trim();
        WholeWord = Result.Length > 2 && Result.StartsWith("\"") && Result.EndsWith("\"");
        if (WholeWord)
            Result = Result.Substring(1, Result.Length - 2).Trim();

        return Result;
    }
    /// <summary>
    /// Returns true if a character is part of a searchable word.
    /// </summary>
    /// <param name="Value">The character.</param>
    /// <returns>True if the character is a word character; otherwise false.</returns>
    static bool IsWordChar(char Value)
    {
        return char.IsLetterOrDigit(Value) || Value == '_';
    }
    /// <summary>
    /// Returns true if the match at an index is whole-word.
    /// </summary>
    /// <param name="Text">The searched text.</param>
    /// <param name="Index">The match index.</param>
    /// <param name="Length">The match length.</param>
    /// <returns>True if the match is whole-word; otherwise false.</returns>
    static bool IsWholeWordMatch(string Text, int Index, int Length)
    {
        bool BeforeOk = Index == 0 || !IsWordChar(Text[Index - 1]);
        int AfterIndex = Index + Length;
        bool AfterOk = AfterIndex >= Text.Length || !IsWordChar(Text[AfterIndex]);
        return BeforeOk && AfterOk;
    }
    /// <summary>
    /// Gets line information for a match index.
    /// </summary>
    /// <param name="Text">The searched text.</param>
    /// <param name="Index">The match index.</param>
    /// <param name="Line">The zero-based line.</param>
    /// <param name="Column">The zero-based column.</param>
    /// <param name="LineText">The matched line text.</param>
    static void GetLineInfo(string Text, int Index, out int Line, out int Column, out string LineText)
    {
        Text = Text ?? string.Empty;
        Index = Math.Clamp(Index, 0, Math.Max(0, Text.Length - 1));

        Line = 0;
        int LineStart = 0;
        for (int i = 0; i < Index; i++)
        {
            if (Text[i] == '\r')
            {
                Line++;
                if (i + 1 < Index && Text[i + 1] == '\n')
                    i++;
                LineStart = i + 1;
            }
            else if (Text[i] == '\n')
            {
                Line++;
                LineStart = i + 1;
            }
        }

        Column = Index - LineStart;

        int LineEnd = LineStart;
        while (LineEnd < Text.Length && Text[LineEnd] != '\r' && Text[LineEnd] != '\n')
            LineEnd++;

        LineText = Text.Substring(LineStart, LineEnd - LineStart);
    }
    /// <summary>
    /// Adds search matches for a text value.
    /// </summary>
    /// <param name="Result">The result list.</param>
    /// <param name="Item">The searched item.</param>
    /// <param name="Place">The searched place.</param>
    /// <param name="Title">The display title.</param>
    /// <param name="Text">The searched text.</param>
    /// <param name="Term">The normalized search term.</param>
    /// <param name="WholeWord">True when whole-word search is used.</param>
    static void AddMatches(LinkItemList Result, BaseItem Item, LinkPlace Place, string Title, string Text, string Term, bool WholeWord)
    {
        if (Item == null || string.IsNullOrWhiteSpace(Text) || string.IsNullOrWhiteSpace(Term))
            return;

        int Index = 0;
        while (Index < Text.Length)
        {
            int MatchIndex = Text.IndexOf(Term, Index, StringComparison.InvariantCultureIgnoreCase);
            if (MatchIndex < 0)
                break;

            if (!WholeWord || IsWholeWordMatch(Text, MatchIndex, Term.Length))
            {
                GetLineInfo(Text, MatchIndex, out int Line, out int Column, out string LineText);
                LinkItem LinkItem = new LinkItem(Item.Type, Place, Title, Item);
                LinkItem.IsText2 = Place == LinkPlace.Text2;
                LinkItem.Line = Line;
                LinkItem.Column = Column;
                LinkItem.LineText = LineText;
                Result.Add(LinkItem);
            }

            Index = MatchIndex + Math.Max(1, Term.Length);
        }
    }
    /// <summary>
    /// Adds search matches for a project item.
    /// </summary>
    /// <param name="Result">The result list.</param>
    /// <param name="Item">The searched item.</param>
    /// <param name="Term">The normalized search term.</param>
    /// <param name="WholeWord">True when whole-word search is used.</param>
    static void AddItemMatches(LinkItemList Result, BaseItem Item, string Term, bool WholeWord)
    {
        string Title = Item.DisplayTitle;
        AddMatches(Result, Item, LinkPlace.Title, Title, Item.Title, Term, WholeWord);

        if (Item is Document Document)
        {
            AddMatches(Result, Item, LinkPlace.Synopsis, Title, Document.Synopsis, Term, WholeWord);
        }
        else if (Item is Folder Folder)
        {
            AddMatches(Result, Item, LinkPlace.Synopsis, Title, Folder.Synopsis, Term, WholeWord);
        }
        else if (Item is TextFile TextFile)
        {
            AddMatches(Result, Item, LinkPlace.Text, Title, TextFile.Text, Term, WholeWord);
            AddMatches(Result, Item, LinkPlace.Text2, Title, TextFile.Text2, Term, WholeWord);
            AddMatches(Result, Item, LinkPlace.Synopsis, Title, TextFile.Synopsis, Term, WholeWord);
            AddMatches(Result, Item, LinkPlace.Draft, Title, TextFile.Draft, Term, WholeWord);
        }
        else if (Item is Component Component)
        {
            AddMatches(Result, Item, LinkPlace.Text, Title, Component.Text, Term, WholeWord);
            AddMatches(Result, Item, LinkPlace.Text2, Title, Component.Text2, Term, WholeWord);
        }
        else if (Item is Note Note)
        {
            AddMatches(Result, Item, LinkPlace.Text, Title, Note.Text, Term, WholeWord);
        }
    }
    /// <summary>
    /// Sorts a link item list.
    /// </summary>
    /// <param name="List">The list to sort.</param>
    static void Sort(LinkItemList List)
    {
        List.List = List.List
            .OrderBy(Item => Item.ItemType)
            .ThenBy(Item => Item.Title)
            .ThenBy(Item => Item.Place)
            .ThenBy(Item => Item.Line)
            .ThenBy(Item => Item.Column)
            .ToList();
    }

    // ● public
    /// <summary>
    /// Executes a project-wide search.
    /// </summary>
    /// <param name="Project">The project.</param>
    /// <param name="Term">The search term.</param>
    /// <returns>The link item results.</returns>
    static public LinkItemList Execute(Project Project, string Term)
    {
        LinkItemList Result = new();
        string SearchTerm = NormalizeTerm(Term, out bool WholeWord);
        if (Project == null || SearchTerm.Length < 3)
            return Result;

        foreach (BaseItem Item in Project.GetDescendantItems())
            AddItemMatches(Result, Item, SearchTerm, WholeWord);

        AddMatches(Result, Project, LinkPlace.TempFile, "Temp Text", Project.TempFileText, SearchTerm, WholeWord);
        Sort(Result);
        return Result;
    }
}
