// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Forms;

/// <summary>
/// Renders markdown text to Avalonia controls.
/// </summary>
static public class MarkdownPreviewRenderer
{
    // ● private fields
    /// <summary>
    /// The markdown parser pipeline.
    /// </summary>
    static readonly MarkdownPipeline fMarkdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    /// <summary>
    /// The heading foreground brush.
    /// </summary>
    static readonly IBrush fHeadingBrush = new SolidColorBrush(Color.Parse("#8A4B16"));

    // ● private
    /// <summary>
    /// Adds a markdown block to the preview.
    /// </summary>
    /// <param name="Panel">The target panel.</param>
    /// <param name="Block">The markdown block.</param>
    /// <param name="Indent">The indent level.</param>
    static void AddPreviewBlock(StackPanel Panel, Block Block, int Indent)
    {
        if (Block is HeadingBlock HeadingBlock)
        {
            TextBlock TextBlock = CreatePreviewTextBlock(GetHeadingFontSize(HeadingBlock.Level), FontWeight.Bold, GetIndentedMargin(Indent, 6, 2));
            TextBlock.Foreground = fHeadingBrush;
            AddInlineContent(Panel, TextBlock, HeadingBlock.Inline, FontWeight.Bold, FontStyle.Normal);
            Panel.Children.Add(TextBlock);
        }
        else if (Block is ParagraphBlock ParagraphBlock)
        {
            TextBlock TextBlock = CreatePreviewTextBlock(GetPreviewFontSize(), FontWeight.Normal, GetIndentedMargin(Indent, 0, 2));
            AddInlineContent(Panel, TextBlock, ParagraphBlock.Inline, FontWeight.Normal, FontStyle.Normal);
            Panel.Children.Add(TextBlock);
        }
        else if (Block is ListBlock ListBlock)
        {
            AddPreviewList(Panel, ListBlock, Indent);
        }
        else if (Block is QuoteBlock QuoteBlock)
        {
            AddPreviewQuote(Panel, QuoteBlock, Indent);
        }
        else if (Block is CodeBlock CodeBlock)
        {
            AddPreviewCodeBlock(Panel, CodeBlock, Indent);
        }
        else if (Block is ThematicBreakBlock)
        {
            Border Border = new Border();
            Border.Height = 1;
            Border.Background = Brushes.LightGray;
            Border.Margin = GetIndentedMargin(Indent, 6, 6);
            Panel.Children.Add(Border);
        }
    }
    /// <summary>
    /// Adds a markdown list to the preview.
    /// </summary>
    /// <param name="Panel">The target panel.</param>
    /// <param name="ListBlock">The list block.</param>
    /// <param name="Indent">The indent level.</param>
    static void AddPreviewList(StackPanel Panel, ListBlock ListBlock, int Indent)
    {
        int Index;
        if (!int.TryParse(ListBlock.OrderedStart, out Index))
            Index = 1;

        foreach (Block ItemBlock in ListBlock)
        {
            ListItemBlock ListItemBlock = ItemBlock as ListItemBlock;
            if (ListItemBlock == null)
                continue;

            bool FirstBlock = true;
            foreach (Block ChildBlock in ListItemBlock)
            {
                if (FirstBlock && ChildBlock is ParagraphBlock ParagraphBlock)
                {
                    string Prefix = ListBlock.IsOrdered ? $"{Index}. " : "• ";
                    TextBlock TextBlock = CreatePreviewTextBlock(GetPreviewFontSize(), FontWeight.Normal, GetIndentedMargin(Indent + 1, 0, 0));
                    TextBlock.Inlines.Add(new Run(Prefix));
                    AddInlineContent(Panel, TextBlock, ParagraphBlock.Inline, FontWeight.Normal, FontStyle.Normal);
                    Panel.Children.Add(TextBlock);
                }
                else
                {
                    AddPreviewBlock(Panel, ChildBlock, Indent + 1);
                }

                FirstBlock = false;
            }

            Index++;
        }
    }
    /// <summary>
    /// Adds a markdown quote to the preview.
    /// </summary>
    /// <param name="Panel">The target panel.</param>
    /// <param name="QuoteBlock">The quote block.</param>
    /// <param name="Indent">The indent level.</param>
    static void AddPreviewQuote(StackPanel Panel, QuoteBlock QuoteBlock, int Indent)
    {
        Border Border = new Border();
        Border.BorderBrush = Brushes.LightGray;
        Border.BorderThickness = new Thickness(3, 0, 0, 0);
        Border.Padding = new Thickness(8, 0, 0, 0);
        Border.Margin = GetIndentedMargin(Indent, 4, 4);

        StackPanel QuotePanel = new StackPanel();
        QuotePanel.Spacing = 4;
        Border.Child = QuotePanel;
        Panel.Children.Add(Border);

        foreach (Block ChildBlock in QuoteBlock)
            AddPreviewBlock(QuotePanel, ChildBlock, 0);
    }
    /// <summary>
    /// Adds a markdown code block to the preview.
    /// </summary>
    /// <param name="Panel">The target panel.</param>
    /// <param name="CodeBlock">The code block.</param>
    /// <param name="Indent">The indent level.</param>
    static void AddPreviewCodeBlock(StackPanel Panel, CodeBlock CodeBlock, int Indent)
    {
        TextBlock TextBlock = CreatePreviewTextBlock(Math.Max(8, GetPreviewFontSize() - 1), FontWeight.Normal, GetIndentedMargin(Indent, 4, 4));
        TextBlock.FontFamily = new FontFamily("Liberation Mono, Cascadia Code, Consolas, Monospace");
        TextBlock.Background = Brushes.WhiteSmoke;
        TextBlock.Padding = new Thickness(6);
        TextBlock.Text = CodeBlock.Lines.ToString();
        Panel.Children.Add(TextBlock);
    }
    /// <summary>
    /// Adds inline markdown content to a text block.
    /// </summary>
    /// <param name="Panel">The target panel.</param>
    /// <param name="TextBlock">The text block.</param>
    /// <param name="Inline">The inline markdown container.</param>
    /// <param name="FontWeight">The font weight.</param>
    /// <param name="FontStyle">The font style.</param>
    static void AddInlineContent(StackPanel Panel, TextBlock TextBlock, ContainerInline Inline, FontWeight FontWeight, FontStyle FontStyle)
    {
        if (Inline == null)
            return;

        Markdig.Syntax.Inlines.Inline Child = Inline.FirstChild;
        while (Child != null)
        {
            AddInline(Panel, TextBlock, Child, FontWeight, FontStyle);
            Child = Child.NextSibling;
        }
    }
    /// <summary>
    /// Adds an inline markdown node to a text block.
    /// </summary>
    /// <param name="Panel">The target panel.</param>
    /// <param name="TextBlock">The text block.</param>
    /// <param name="Inline">The inline markdown node.</param>
    /// <param name="FontWeight">The inherited font weight.</param>
    /// <param name="FontStyle">The inherited font style.</param>
    static void AddInline(StackPanel Panel, TextBlock TextBlock, Markdig.Syntax.Inlines.Inline Inline, FontWeight FontWeight, FontStyle FontStyle)
    {
        if (Inline is LiteralInline LiteralInline)
        {
            TextBlock.Inlines.Add(CreateRun(LiteralInline.Content.ToString(), FontWeight, FontStyle));
        }
        else if (Inline is CodeInline CodeInline)
        {
            Run Run = CreateRun(CodeInline.Content, FontWeight.Normal, FontStyle.Normal);
            Run.FontFamily = new FontFamily("Liberation Mono, Cascadia Code, Consolas, Monospace");
            TextBlock.Inlines.Add(Run);
        }
        else if (Inline is EmphasisInline EmphasisInline)
        {
            FontWeight NewFontWeight = EmphasisInline.DelimiterCount >= 2 ? FontWeight.Bold : FontWeight;
            FontStyle NewFontStyle = EmphasisInline.DelimiterCount == 1 ? FontStyle.Italic : FontStyle;
            AddInlineContent(Panel, TextBlock, EmphasisInline, NewFontWeight, NewFontStyle);
        }
        else if (Inline is LinkInline LinkInline)
        {
            if (LinkInline.IsImage)
            {
                AddPreviewImage(Panel, LinkInline.Url, GetInlineText(LinkInline));
            }
            else
            {
                Run Run = CreateRun(GetInlineText(LinkInline), FontWeight, FontStyle);
                Run.Foreground = Brushes.DodgerBlue;
                TextBlock.Inlines.Add(Run);
            }
        }
        else if (Inline is LineBreakInline)
        {
            TextBlock.Inlines.Add(new LineBreak());
        }
    }
    /// <summary>
    /// Returns text from an inline node.
    /// </summary>
    /// <param name="Inline">The inline node.</param>
    /// <returns>The extracted text.</returns>
    static string GetInlineText(ContainerInline Inline)
    {
        System.Text.StringBuilder Builder = new();
        Markdig.Syntax.Inlines.Inline Child = Inline.FirstChild;
        while (Child != null)
        {
            if (Child is LiteralInline LiteralInline)
                Builder.Append(LiteralInline.Content.ToString());
            else if (Child is CodeInline CodeInline)
                Builder.Append(CodeInline.Content);
            else if (Child is ContainerInline ContainerInline)
                Builder.Append(GetInlineText(ContainerInline));

            Child = Child.NextSibling;
        }

        return Builder.ToString();
    }
    /// <summary>
    /// Creates a preview text block.
    /// </summary>
    /// <param name="FontSize">The font size.</param>
    /// <param name="FontWeight">The font weight.</param>
    /// <param name="Margin">The margin.</param>
    /// <returns>The text block.</returns>
    static TextBlock CreatePreviewTextBlock(double FontSize, FontWeight FontWeight, Thickness Margin)
    {
        TextBlock Result = new TextBlock();
        Result.FontFamily = GetPreviewFontFamily();
        Result.FontSize = FontSize;
        Result.FontWeight = FontWeight;
        Result.Foreground = Brushes.Black;
        Result.Margin = Margin;
        Result.TextWrapping = TextWrapping.Wrap;
        return Result;
    }
    /// <summary>
    /// Creates a text run.
    /// </summary>
    /// <param name="Text">The text.</param>
    /// <param name="FontWeight">The font weight.</param>
    /// <param name="FontStyle">The font style.</param>
    /// <returns>The text run.</returns>
    static Run CreateRun(string Text, FontWeight FontWeight, FontStyle FontStyle)
    {
        Run Result = new Run(Text ?? string.Empty);
        Result.FontWeight = FontWeight;
        Result.FontStyle = FontStyle;
        return Result;
    }
    /// <summary>
    /// Adds plain text to the markdown preview.
    /// </summary>
    /// <param name="Panel">The target panel.</param>
    /// <param name="Text">The text.</param>
    /// <param name="FontSize">The font size.</param>
    /// <param name="FontWeight">The font weight.</param>
    /// <param name="Margin">The margin.</param>
    static void AddPreviewText(StackPanel Panel, string Text, double FontSize, FontWeight FontWeight, Thickness Margin)
    {
        TextBlock TextBlock = CreatePreviewTextBlock(FontSize, FontWeight, Margin);
        TextBlock.Text = Text ?? string.Empty;
        Panel.Children.Add(TextBlock);
    }
    /// <summary>
    /// Adds an image to the markdown preview.
    /// </summary>
    /// <param name="Panel">The target panel.</param>
    /// <param name="ImagePath">The markdown image path.</param>
    /// <param name="AltText">The alternate text.</param>
    static void AddPreviewImage(StackPanel Panel, string ImagePath, string AltText)
    {
        string FilePath = ResolveImagePath(ImagePath);
        if (string.IsNullOrWhiteSpace(FilePath) || !System.IO.File.Exists(FilePath))
        {
            AddPreviewText(Panel, $"[Missing image: {ImagePath}]", GetPreviewFontSize(), FontWeight.Normal, new Thickness(0));
            return;
        }

        try
        {
            Image Image = new Image();
            Image.Source = new Bitmap(FilePath);
            Image.Stretch = Stretch.Uniform;
            Image.MaxHeight = 320;
            Image.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            Image.Margin = new Thickness(0, 4, 0, 2);
            Panel.Children.Add(Image);

            if (!string.IsNullOrWhiteSpace(AltText))
                AddPreviewText(Panel, AltText, Math.Max(8, GetPreviewFontSize() - 1), FontWeight.Normal, new Thickness(0, 0, 0, 4));
        }
        catch
        {
            AddPreviewText(Panel, $"[Invalid image: {ImagePath}]", GetPreviewFontSize(), FontWeight.Normal, new Thickness(0));
        }
    }
    /// <summary>
    /// Resolves a markdown image path.
    /// </summary>
    /// <param name="ImagePath">The markdown image path.</param>
    /// <returns>The resolved file path.</returns>
    static string ResolveImagePath(string ImagePath)
    {
        string Result = (ImagePath ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(Result))
            return string.Empty;

        if (System.IO.Path.IsPathFullyQualified(Result))
            return Result;

        Project Project = AppHost.CurrentProject;
        if (Project == null)
            return Result;

        string ImagesPath = System.IO.Path.Combine(Project.ImagesFolderPath, Result);
        if (System.IO.File.Exists(ImagesPath))
            return ImagesPath;

        string ProjectPath = System.IO.Path.Combine(Project.FolderPath, Result);
        if (System.IO.File.Exists(ProjectPath))
            return ProjectPath;

        return ImagesPath;
    }
    /// <summary>
    /// Returns a heading font size.
    /// </summary>
    /// <param name="Level">The heading level.</param>
    /// <returns>The font size.</returns>
    static double GetHeadingFontSize(int Level)
    {
        double BaseSize = GetPreviewFontSize();
        return Level == 1 ? BaseSize + 8 : Level == 2 ? BaseSize + 5 : Level == 3 ? BaseSize + 3 : BaseSize + 2;
    }
    /// <summary>
    /// Returns the preview body font size.
    /// </summary>
    /// <returns>The preview body font size.</returns>
    static double GetPreviewFontSize()
    {
        return AppHost.Settings?.FontSize ?? 13;
    }
    /// <summary>
    /// Returns the preview font family.
    /// </summary>
    /// <returns>The preview font family.</returns>
    static FontFamily GetPreviewFontFamily()
    {
        return new FontFamily(AppHost.Settings?.FontFamily ?? "Liberation Mono, Cascadia Code, Consolas, Monospace");
    }
    /// <summary>
    /// Returns an indented margin.
    /// </summary>
    /// <param name="Indent">The indent level.</param>
    /// <param name="Top">The top margin.</param>
    /// <param name="Bottom">The bottom margin.</param>
    /// <returns>The margin.</returns>
    static Thickness GetIndentedMargin(int Indent, double Top, double Bottom)
    {
        return new Thickness(Indent * 14, Top, 0, Bottom);
    }

    // ● static public
    /// <summary>
    /// Renders markdown text to a target panel.
    /// </summary>
    /// <param name="Panel">The target panel.</param>
    /// <param name="MarkdownText">The markdown text.</param>
    static public void Render(StackPanel Panel, string MarkdownText)
    {
        Panel.Children.Clear();

        MarkdownDocument Document = Markdown.Parse(MarkdownText ?? string.Empty, fMarkdownPipeline);
        foreach (Block Block in Document)
            AddPreviewBlock(Panel, Block, 0);
    }
}
