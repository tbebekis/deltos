// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Forms;

/// <summary>
/// Renders text search highlights.
/// </summary>
public class TextSearchHighlightRenderer: AvaloniaEdit.Rendering.IBackgroundRenderer
{
    // ● private fields
    /// <summary>
    /// The text editor.
    /// </summary>
    TextEditor fEditor;
    /// <summary>
    /// The search match list.
    /// </summary>
    List<TextSearchMatch> fMatches = new();

    // ● construction
    /// <summary>
    /// Initializes a new instance of the TextSearchHighlightRenderer class.
    /// </summary>
    /// <param name="Editor">The text editor.</param>
    public TextSearchHighlightRenderer(TextEditor Editor)
    {
        fEditor = Editor;
    }

    // ● public
    /// <summary>
    /// Sets the rendered matches.
    /// </summary>
    /// <param name="Matches">The matches.</param>
    public void SetMatches(List<TextSearchMatch> Matches)
    {
        fMatches = Matches ?? new List<TextSearchMatch>();
        fEditor.TextArea.TextView.InvalidateLayer(Layer);
    }
    /// <summary>
    /// Draws the highlighted matches.
    /// </summary>
    /// <param name="TextView">The text view.</param>
    /// <param name="DrawingContext">The drawing context.</param>
    public void Draw(AvaloniaEdit.Rendering.TextView TextView, DrawingContext DrawingContext)
    {
        if (fMatches.Count == 0 || fEditor.Document == null)
            return;

        TextView.EnsureVisualLines();
        IBrush Brush = new SolidColorBrush(Color.FromArgb(120, 255, 221, 87));

        foreach (TextSearchMatch Match in fMatches)
        {
            if (Match.Length <= 0)
                continue;

            AvaloniaEdit.Document.TextSegment Segment = new AvaloniaEdit.Document.TextSegment
            {
                StartOffset = Match.Offset,
                Length = Match.Length
            };

            foreach (Rect Rect in AvaloniaEdit.Rendering.BackgroundGeometryBuilder.GetRectsForSegment(TextView, Segment))
                DrawingContext.FillRectangle(Brush, Rect);
        }
    }

    // ● properties
    /// <summary>
    /// Gets the renderer layer.
    /// </summary>
    public AvaloniaEdit.Rendering.KnownLayer Layer => AvaloniaEdit.Rendering.KnownLayer.Selection;
}
