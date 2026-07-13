// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Forms;

/// <summary>
/// Provides find, replace, and highlight behavior for a text editor.
/// </summary>
public class TextFindReplaceHandler
{
    // ● private fields
    /// <summary>
    /// The owning editor form.
    /// </summary>
    TextEditorForm fOwner;
    /// <summary>
    /// The text editor.
    /// </summary>
    TextEditor fEditor;
    /// <summary>
    /// The highlight renderer.
    /// </summary>
    TextSearchHighlightRenderer fRenderer;
    /// <summary>
    /// The current match list.
    /// </summary>
    List<TextSearchMatch> fMatches = new();
    /// <summary>
    /// The current match index.
    /// </summary>
    int fCurrentIndex = -1;

    // ● private
    /// <summary>
    /// Returns true if a character is a word character.
    /// </summary>
    /// <param name="Value">The character.</param>
    /// <returns>True if the character is a word character; otherwise false.</returns>
    bool IsWordChar(char Value)
    {
        return char.IsLetterOrDigit(Value) || Value == '_';
    }
    /// <summary>
    /// Returns true if a match is whole-word.
    /// </summary>
    /// <param name="Text">The text.</param>
    /// <param name="Offset">The match offset.</param>
    /// <param name="Length">The match length.</param>
    /// <returns>True if the match is whole-word; otherwise false.</returns>
    bool IsWholeWordMatch(string Text, int Offset, int Length)
    {
        bool BeforeOk = Offset == 0 || !IsWordChar(Text[Offset - 1]);
        int AfterOffset = Offset + Length;
        bool AfterOk = AfterOffset >= Text.Length || !IsWordChar(Text[AfterOffset]);
        return BeforeOk && AfterOk;
    }
    /// <summary>
    /// Selects a match.
    /// </summary>
    /// <param name="Index">The match index.</param>
    void SelectMatch(int Index)
    {
        if (Index < 0 || Index >= fMatches.Count)
            return;

        fCurrentIndex = Index;
        TextSearchMatch Match = fMatches[Index];
        fEditor.Select(Match.Offset, Match.Length);
        fEditor.CaretOffset = Match.Offset + Match.Length;
        fEditor.TextArea.Caret.BringCaretToView();
    }
    /// <summary>
    /// Finds the match index after the current caret.
    /// </summary>
    /// <param name="Backward">True to search backwards.</param>
    /// <returns>The match index.</returns>
    int FindMatchIndexFromCaret(bool Backward)
    {
        if (fMatches.Count == 0)
            return -1;

        int Offset = fEditor.CaretOffset;
        if (Backward)
        {
            for (int Index = fMatches.Count - 1; Index >= 0; Index--)
            {
                if (fMatches[Index].Offset < Offset)
                    return Index;
            }

            return fMatches.Count - 1;
        }

        for (int Index = 0; Index < fMatches.Count; Index++)
        {
            if (fMatches[Index].Offset >= Offset)
                return Index;
        }

        return 0;
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the TextFindReplaceHandler class.
    /// </summary>
    /// <param name="Owner">The owning editor form.</param>
    /// <param name="Editor">The text editor.</param>
    public TextFindReplaceHandler(TextEditorForm Owner, TextEditor Editor)
    {
        fOwner = Owner;
        fEditor = Editor;
        fRenderer = new TextSearchHighlightRenderer(Editor);
        fEditor.TextArea.TextView.BackgroundRenderers.Add(fRenderer);
    }

    // ● public
    /// <summary>
    /// Finds all matches.
    /// </summary>
    /// <param name="Options">The options.</param>
    /// <returns>The match count.</returns>
    public int HighlightAll(FindReplaceOptions Options)
    {
        fMatches.Clear();
        fCurrentIndex = -1;
        string Text = fEditor.Text ?? string.Empty;
        string Term = Options?.TextToFind ?? string.Empty;
        if (string.IsNullOrWhiteSpace(Text) || string.IsNullOrWhiteSpace(Term))
        {
            ClearHighlights();
            return 0;
        }

        int Offset = 0;
        StringComparison Comparison = Options.MatchCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
        while (Offset < Text.Length)
        {
            int MatchOffset = Text.IndexOf(Term, Offset, Comparison);
            if (MatchOffset < 0)
                break;

            if (!Options.WholeWord || IsWholeWordMatch(Text, MatchOffset, Term.Length))
            {
                fMatches.Add(new TextSearchMatch
                {
                    Offset = MatchOffset,
                    Length = Term.Length
                });
            }

            Offset = MatchOffset + Math.Max(1, Term.Length);
        }

        fRenderer.SetMatches(fMatches);
        return fMatches.Count;
    }
    /// <summary>
    /// Clears highlights.
    /// </summary>
    public void ClearHighlights()
    {
        fMatches.Clear();
        fCurrentIndex = -1;
        fRenderer.SetMatches(fMatches);
    }
    /// <summary>
    /// Finds and selects the first match.
    /// </summary>
    /// <param name="Options">The options.</param>
    /// <returns>The match count.</returns>
    public int Find(FindReplaceOptions Options)
    {
        int Count = HighlightAll(Options);
        if (Count > 0)
            SelectMatch(0);

        return Count;
    }
    /// <summary>
    /// Selects the next or previous match.
    /// </summary>
    /// <param name="Backward">True to move backwards.</param>
    /// <returns>True if a match is selected; otherwise false.</returns>
    public bool FindNext(bool Backward)
    {
        if (fMatches.Count == 0)
            return false;

        int Index;
        if (fCurrentIndex < 0)
        {
            Index = FindMatchIndexFromCaret(Backward);
        }
        else
        {
            Index = Backward ? fCurrentIndex - 1 : fCurrentIndex + 1;
            if (Index < 0)
                Index = fMatches.Count - 1;
            else if (Index >= fMatches.Count)
                Index = 0;
        }

        SelectMatch(Index);
        return true;
    }
    /// <summary>
    /// Replaces the current match.
    /// </summary>
    /// <param name="Options">The options.</param>
    /// <returns>True if a match is replaced; otherwise false.</returns>
    public bool ReplaceCurrent(FindReplaceOptions Options)
    {
        if (fMatches.Count == 0)
            Find(Options);

        if (fCurrentIndex < 0 || fCurrentIndex >= fMatches.Count)
            return false;

        TextSearchMatch Match = fMatches[fCurrentIndex];
        fEditor.Document.Replace(Match.Offset, Match.Length, Options.ReplaceWith ?? string.Empty);
        HighlightAll(Options);
        if (fMatches.Count > 0)
            SelectMatch(Math.Min(fCurrentIndex, fMatches.Count - 1));

        return true;
    }
    /// <summary>
    /// Replaces all matches.
    /// </summary>
    /// <param name="Options">The options.</param>
    /// <returns>The replacement count.</returns>
    public int ReplaceAll(FindReplaceOptions Options)
    {
        HighlightAll(Options);
        if (fMatches.Count == 0)
            return 0;

        int Count = fMatches.Count;
        string Replacement = Options.ReplaceWith ?? string.Empty;
        for (int Index = fMatches.Count - 1; Index >= 0; Index--)
        {
            TextSearchMatch Match = fMatches[Index];
            fEditor.Document.Replace(Match.Offset, Match.Length, Replacement);
        }

        HighlightAll(Options);
        return Count;
    }

    // ● properties
    /// <summary>
    /// Gets the current matches.
    /// </summary>
    public List<TextSearchMatch> Matches => fMatches;
}
