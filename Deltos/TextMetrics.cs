// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Holds text metric values.
/// </summary>
public class TextStats
{
    // ● public
    /// <summary>
    /// Resets all metric values.
    /// </summary>
    public void Reset()
    {
        WordCount = 0;
        CharCount = 0;
        CharCountNoSpaces = 0;
        LineCount = 0;
        ParagraphCount = 0;
        EstimatedPages = 0;
    }
    /// <summary>
    /// Adds another text stats instance to this instance.
    /// </summary>
    /// <param name="Source">The source stats.</param>
    public void Add(TextStats Source)
    {
        if (Source == null)
            return;

        WordCount += Source.WordCount;
        CharCount += Source.CharCount;
        CharCountNoSpaces += Source.CharCountNoSpaces;
        LineCount += Source.LineCount;
        ParagraphCount += Source.ParagraphCount;
        EstimatedPages += Source.EstimatedPages;
    }

    // ● properties
    /// <summary>
    /// Gets or sets the word count.
    /// </summary>
    public int WordCount { get; set; }
    /// <summary>
    /// Gets or sets the character count.
    /// </summary>
    public int CharCount { get; set; }
    /// <summary>
    /// Gets or sets the character count excluding whitespace.
    /// </summary>
    public int CharCountNoSpaces { get; set; }
    /// <summary>
    /// Gets or sets the line count.
    /// </summary>
    public int LineCount { get; set; }
    /// <summary>
    /// Gets or sets the paragraph count.
    /// </summary>
    public int ParagraphCount { get; set; }
    /// <summary>
    /// Gets or sets the estimated page count.
    /// </summary>
    public double EstimatedPages { get; set; }
}

/// <summary>
/// Provides text metric calculation.
/// </summary>
static public class TextMetrics
{
    // ● private fields
    /// <summary>
    /// The default words per page.
    /// </summary>
    const int DefaultWordsPerPage = 250;

    // ● private
    /// <summary>
    /// Returns true when a character is a word core character.
    /// </summary>
    /// <param name="Value">The character.</param>
    /// <returns>True if the character is a word core character; otherwise false.</returns>
    static bool IsWordCore(char Value)
    {
        UnicodeCategory Category = char.GetUnicodeCategory(Value);
        return char.IsLetterOrDigit(Value)
            || Category == UnicodeCategory.NonSpacingMark
            || Category == UnicodeCategory.SpacingCombiningMark;
    }
    /// <summary>
    /// Returns true when a character may appear inside a word.
    /// </summary>
    /// <param name="Value">The character.</param>
    /// <returns>True if the character may appear inside a word; otherwise false.</returns>
    static bool IsInnerWordPunctuation(char Value)
    {
        return Value == '\'' || Value == '’' || Value == '-';
    }
    /// <summary>
    /// Finalizes derived metric values.
    /// </summary>
    /// <param name="Stats">The text stats.</param>
    /// <param name="WordsPerPage">The words per page.</param>
    static void FinalizeStats(TextStats Stats, int WordsPerPage)
    {
        Stats.EstimatedPages = 0;
        if (WordsPerPage > 0)
            Stats.EstimatedPages = (double)Stats.WordCount / WordsPerPage;
    }

    // ● static public
    /// <summary>
    /// Computes text metrics.
    /// </summary>
    /// <param name="Text">The text.</param>
    /// <param name="WordsPerPage">The words per page.</param>
    /// <returns>The computed text stats.</returns>
    static public TextStats Compute(string Text, int WordsPerPage = DefaultWordsPerPage)
    {
        TextStats Result = new TextStats();
        Accumulate(Result, Text);
        FinalizeStats(Result, WordsPerPage);
        return Result;
    }
    /// <summary>
    /// Accumulates text metrics into an existing stats instance.
    /// </summary>
    /// <param name="Stats">The stats to update.</param>
    /// <param name="Text">The text.</param>
    static public void Accumulate(TextStats Stats, string Text)
    {
        if (Stats == null || string.IsNullOrEmpty(Text))
            return;

        bool InWord = false;
        bool HasNonSpaceInParagraph = false;
        bool LastWasLineFeed = false;
        Stats.CharCount += Text.Length;
        Stats.LineCount++;

        for (int Index = 0; Index < Text.Length; Index++)
        {
            char Char = Text[Index];
            bool IsWhiteSpace = char.IsWhiteSpace(Char);
            bool IsLineFeed = Char == '\n';

            if (!IsWhiteSpace)
            {
                Stats.CharCountNoSpaces++;
                HasNonSpaceInParagraph = true;
            }

            if (IsLineFeed)
            {
                Stats.LineCount++;
                if (LastWasLineFeed && HasNonSpaceInParagraph)
                {
                    Stats.ParagraphCount++;
                    HasNonSpaceInParagraph = false;
                }

                LastWasLineFeed = true;
            }
            else if (Char != '\r')
            {
                LastWasLineFeed = false;
            }

            bool IsWordChar = IsWordCore(Char) || (InWord && IsInnerWordPunctuation(Char));
            if (IsWordChar)
            {
                if (!InWord)
                {
                    InWord = true;
                    Stats.WordCount++;
                }
            }
            else
            {
                InWord = false;
            }
        }

        if (HasNonSpaceInParagraph)
            Stats.ParagraphCount++;
    }
}
