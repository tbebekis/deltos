// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Forms;

/// <summary>
/// Provides a reusable markdown text editor surface with a toolbar and status bar.
/// </summary>
public partial class TextEditorForm: UserControl
{
    // ● private fields
    /// <summary>
    /// The toolbar helper.
    /// </summary>
    private Tripous.Desktop.ToolBar fToolBar;
    /// <summary>
    /// The Save toolbar button.
    /// </summary>
    private Button fBtnSave;
    /// <summary>
    /// The Find toolbar button.
    /// </summary>
    private Button fBtnFind;
    /// <summary>
    /// The Search For Term toolbar button.
    /// </summary>
    private Button fBtnSearchForTerm;
    /// <summary>
    /// The Show Folder toolbar button.
    /// </summary>
    private Button fBtnShowFolder;
    /// <summary>
    /// The markdown preview toolbar button.
    /// </summary>
    Button fBtnMarkdownPreview;
    /// <summary>
    /// The installed search panel.
    /// </summary>
    private AvaloniaEdit.Search.SearchPanel fSearchPanel;
    /// <summary>
    /// The find and replace options.
    /// </summary>
    FindReplaceOptions fFindReplaceOptions = new();
    /// <summary>
    /// The find and replace handler.
    /// </summary>
    TextFindReplaceHandler fFindReplaceHandler;
    /// <summary>
    /// The nested ignore-modified counter.
    /// </summary>
    private int fIgnoreModifiedCount;
    /// <summary>
    /// True when the editor content is modified.
    /// </summary>
    private bool fModified;
    /// <summary>
    /// The edited file path.
    /// </summary>
    private string fFilePath = string.Empty;
    /// <summary>
    /// The markdown preview form identifier.
    /// </summary>
    string fPreviewId = string.Empty;
    /// <summary>
    /// Field for the HighlightMode property.
    /// </summary>
    private HighlightMode fHighlightMode = HighlightMode.Markdown;
    /// <summary>
    /// Field for the SaveButtonVisible property.
    /// </summary>
    private bool fSaveButtonVisible = true;
    /// <summary>
    /// Field for the FindButtonVisible property.
    /// </summary>
    private bool fFindButtonVisible = true;
    /// <summary>
    /// Field for the SearchForTermButtonVisible property.
    /// </summary>
    private bool fSearchForTermButtonVisible = true;
    /// <summary>
    /// Field for the ShowFolderButtonVisible property.
    /// </summary>
    private bool fShowFolderButtonVisible = true;
    /// <summary>
    /// The default editor font size.
    /// </summary>
    private const double DefaultFontSize = 14;
    /// <summary>
    /// The minimum editor font size.
    /// </summary>
    private const double MinFontSize = 8;
    /// <summary>
    /// The maximum editor font size.
    /// </summary>
    private const double MaxFontSize = 32;

    // ● private
    /// <summary>
    /// Initializes the editor control.
    /// </summary>
    private void Initialize()
    {
        PrepareToolBar();
        fSearchPanel = AvaloniaEdit.Search.SearchPanel.Install(edtText);
        fFindReplaceHandler = new TextFindReplaceHandler(this, edtText);

        edtText.Options.ShowSpaces = false;
        edtText.Options.ShowTabs = false;
        edtText.Options.ShowEndOfLine = false;
        edtText.Options.ShowBoxForControlCharacters = false;
        ApplyAppSettings();

        edtText.TextChanged += Editor_TextChanged;
        edtText.TextArea.Caret.PositionChanged += Caret_PositionChanged;
        edtText.KeyDown += Editor_KeyDown;
        edtText.TextArea.AddHandler(InputElement.PointerReleasedEvent, Editor_PointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);

        edtText.IsModified = false;
        SetHighlightMode(HighlightMode.Markdown);
        UpdateStatusBar();
    }
    /// <summary>
    /// Creates the toolbar buttons.
    /// </summary>
    private void PrepareToolBar()
    {
        fToolBar = new();
        fToolBar.Panel = pnlToolBar;

        fBtnFind = fToolBar.AddButton("page_find.png", "Find and Replace (Ctrl + F)", async () => await Find());
        fBtnSearchForTerm = fToolBar.AddButton("table_tab_search.png", "Search for Term (Ctrl + T)", SearchForTerm);
        fBtnSave = fToolBar.AddButton("disk.png", "Save (Ctrl + S)", SaveText);
        fBtnShowFolder = fToolBar.AddButton("folder_go.png", "Show in folder", ShowFolder);
        fBtnMarkdownPreview = fToolBar.AddButton("html.png", "HTML Preview", ShowMarkdownPreview);
        UpdateButtonVisibility();
    }
    /// <summary>
    /// Handles editor text changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    private void Editor_TextChanged(object Sender, EventArgs Args)
    {
        if (!IgnoreModified)
            Modified = true;

        UpdateStatusBar();
        TextChanged?.Invoke(this, EventArgs.Empty);
    }
    /// <summary>
    /// Handles caret position changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    private void Caret_PositionChanged(object Sender, EventArgs Args)
    {
        UpdateStatusBarLineColumn();
    }
    /// <summary>
    /// Handles editor shortcut keys.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The key event arguments.</param>
    private void Editor_KeyDown(object Sender, KeyEventArgs Args)
    {
        if (Args.Key == Key.F3)
        {
            Args.Handled = true;
            FindNext(Args.KeyModifiers.HasFlag(KeyModifiers.Shift));
            return;
        }

        if (Args.Key == Key.Escape)
        {
            Args.Handled = true;
            ClearSearchHighlights();
            return;
        }

        bool Ctrl = Args.KeyModifiers.HasFlag(KeyModifiers.Control);
        if (!Ctrl)
            return;

        if (Args.Key == Key.S)
        {
            Args.Handled = true;
            SaveText();
        }
        else if (Args.Key == Key.F)
        {
            Args.Handled = true;
            _ = Find();
        }
        else if (Args.Key == Key.T)
        {
            Args.Handled = true;
            SearchForTerm();
        }
        else if (Args.Key == Key.G)
        {
            Args.Handled = true;
            _ = SearchForTerm(true);
        }
        else if (Args.Key == Key.B)
        {
            Args.Handled = true;
            ToggleMarkdownMarker("**");
        }
        else if (Args.Key == Key.I)
        {
            Args.Handled = true;
            ToggleMarkdownMarker("*");
        }
        else if (Args.Key == Key.Add || Args.Key == Key.OemPlus)
        {
            Args.Handled = true;
            IncreaseFontSize();
        }
        else if (Args.Key == Key.Subtract || Args.Key == Key.OemMinus)
        {
            Args.Handled = true;
            DecreaseFontSize();
        }
        else if (Args.Key == Key.D0 || Args.Key == Key.NumPad0)
        {
            Args.Handled = true;
            ResetFontSize();
        }
    }
    /// <summary>
    /// Handles editor pointer release.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The pointer event arguments.</param>
    void Editor_PointerReleased(object Sender, PointerReleasedEventArgs Args)
    {
        if (!Args.KeyModifiers.HasFlag(KeyModifiers.Control))
            return;

        PointerPoint Point = Args.GetCurrentPoint(edtText);
        if (Point.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonReleased)
            return;

        Args.Handled = true;
        _ = SearchForTerm(true);
    }
    /// <summary>
    /// Returns the word at the current caret offset.
    /// </summary>
    /// <returns>The word at the current caret offset.</returns>
    private string GetWordAtCaret()
    {
        string Text = EditorText;
        if (string.IsNullOrWhiteSpace(Text))
            return string.Empty;

        int Offset = Math.Clamp(edtText.CaretOffset, 0, Text.Length);
        if (Offset == Text.Length && Offset > 0)
            Offset--;

        if (Offset < 0 || Offset >= Text.Length || !IsWordChar(Text[Offset]))
            return string.Empty;

        int Start = Offset;
        while (Start > 0 && IsWordChar(Text[Start - 1]))
            Start--;

        int End = Offset;
        while (End < Text.Length - 1 && IsWordChar(Text[End + 1]))
            End++;

        return Text.Substring(Start, End - Start + 1);
    }
    /// <summary>
    /// Gets the word span at the current caret offset.
    /// </summary>
    /// <param name="Start">The word start offset.</param>
    /// <param name="Length">The word length.</param>
    /// <returns>True if a word span is found; otherwise false.</returns>
    bool TryGetWordSpanAtCaret(out int Start, out int Length)
    {
        Start = 0;
        Length = 0;

        string Text = EditorText;
        if (string.IsNullOrWhiteSpace(Text))
            return false;

        int Offset = Math.Clamp(edtText.CaretOffset, 0, Text.Length);
        if (Offset == Text.Length && Offset > 0)
            Offset--;

        if (Offset < 0 || Offset >= Text.Length)
            return false;

        if (!IsWordChar(Text[Offset]) && Offset > 0 && IsWordChar(Text[Offset - 1]))
            Offset--;

        if (!IsWordChar(Text[Offset]))
            return false;

        int WordStart = Offset;
        while (WordStart > 0 && IsWordChar(Text[WordStart - 1]))
            WordStart--;

        int WordEnd = Offset;
        while (WordEnd < Text.Length - 1 && IsWordChar(Text[WordEnd + 1]))
            WordEnd++;

        Start = WordStart;
        Length = WordEnd - WordStart + 1;
        return Length > 0;
    }
    /// <summary>
    /// Toggles a markdown marker around the selection or current word.
    /// </summary>
    /// <param name="Marker">The markdown marker.</param>
    void ToggleMarkdownMarker(string Marker)
    {
        if (TextEditor.IsReadOnly || string.IsNullOrEmpty(Marker))
            return;

        int Start = TextEditor.SelectionStart;
        int Length = TextEditor.SelectionLength;
        if (Length <= 0 && !TryGetWordSpanAtCaret(out Start, out Length))
            return;

        ToggleMarkdownMarker(Start, Length, Marker);
    }
    /// <summary>
    /// Toggles a markdown marker around a text span.
    /// </summary>
    /// <param name="Start">The text span start offset.</param>
    /// <param name="Length">The text span length.</param>
    /// <param name="Marker">The markdown marker.</param>
    void ToggleMarkdownMarker(int Start, int Length, string Marker)
    {
        string Text = EditorText;
        int MarkerLength = Marker.Length;
        bool HasBefore = Start >= MarkerLength && Text.Substring(Start - MarkerLength, MarkerLength) == Marker;
        bool HasAfter = Start + Length + MarkerLength <= Text.Length && Text.Substring(Start + Length, MarkerLength) == Marker;

        if (HasBefore && HasAfter)
        {
            TextEditor.Document.Remove(Start + Length, MarkerLength);
            TextEditor.Document.Remove(Start - MarkerLength, MarkerLength);
            TextEditor.Select(Start - MarkerLength, Length);
            TextEditor.CaretOffset = Start - MarkerLength + Length;
        }
        else
        {
            TextEditor.Document.Insert(Start + Length, Marker);
            TextEditor.Document.Insert(Start, Marker);
            TextEditor.Select(Start + MarkerLength, Length);
            TextEditor.CaretOffset = Start + MarkerLength + Length;
        }
    }
    /// <summary>
    /// Returns true if a character is part of a searchable word.
    /// </summary>
    /// <param name="Value">The character to check.</param>
    /// <returns>True if the character is a word character; otherwise false.</returns>
    private bool IsWordChar(char Value)
    {
        return char.IsLetterOrDigit(Value) || Value == '_';
    }
    /// <summary>
    /// Normalizes text before displaying it in the editor.
    /// </summary>
    /// <param name="Text">The source text.</param>
    /// <returns>The normalized text.</returns>
    string NormalizeEditorText(string Text)
    {
        if (string.IsNullOrEmpty(Text))
            return string.Empty;

        Text = Regex.Replace(Text, @"[ \t]+(\r?\n|$)", "$1");
        StringBuilder Builder = null;
        for (int Index = 0; Index < Text.Length; Index++)
        {
            char Ch = Text[Index];
            bool Remove = char.IsControl(Ch) && Ch != '\r' && Ch != '\n' && Ch != '\t';
            Remove = Remove || Ch == '\u200B' || Ch == '\u200C' || Ch == '\u200D' || Ch == '\uFEFF' || Ch == '\uFFFC' || Ch == '\uFFFD';
            char Replacement = Ch == '\u00A0' ? ' ' : Ch;
            if (Remove || Replacement != Ch || Builder != null)
            {
                if (Builder == null)
                {
                    Builder = new StringBuilder(Text.Length);
                    Builder.Append(Text, 0, Index);
                }

                if (!Remove)
                    Builder.Append(Replacement);
            }
        }

        return Builder == null ? Text : Builder.ToString();
    }
    /// Updates toolbar button visibility from the public visibility properties.
    /// </summary>
    private void UpdateButtonVisibility()
    {
        if (fBtnSave != null)
            fBtnSave.IsVisible = SaveButtonVisible;

        if (fBtnFind != null)
            fBtnFind.IsVisible = FindButtonVisible;

        if (fBtnSearchForTerm != null)
            fBtnSearchForTerm.IsVisible = SearchForTermButtonVisible;

        if (fBtnShowFolder != null)
            fBtnShowFolder.IsVisible = ShowFolderButtonVisible;

        if (fBtnMarkdownPreview != null)
            fBtnMarkdownPreview.IsVisible = AppHost.Settings?.ShowMarkdownPreviewButton == true;
    }

    // ● public
    /// <summary>
    /// Saves the current editor text through the SaveRequested event.
    /// </summary>
    public void SaveText()
    {
        SaveRequested?.Invoke(this, EventArgs.Empty);
    }
    /// <summary>
    /// Opens the editor find panel.
    /// </summary>
    public async Task Find()
    {
        string Term = GetWordAtCaret();
        if (!string.IsNullOrWhiteSpace(Term))
            fFindReplaceOptions.TextToFind = Term;

        Deltos.FindReplaceOptions Options = new Deltos.FindReplaceOptions();
        Options.TextToFind = fFindReplaceOptions.TextToFind;
        Options.ReplaceWith = fFindReplaceOptions.ReplaceWith;
        Options.MatchCase = fFindReplaceOptions.MatchCase;
        Options.WholeWord = fFindReplaceOptions.WholeWord;
        Options.Replace = fFindReplaceOptions.Replace;
        Options.ReplaceAll = fFindReplaceOptions.ReplaceAll;

        DialogInfo Info = await DialogWindow.ShowModal<Deltos.FindReplaceDialog>(Options, this);
        if (!Info.Result || Info.ResultData is not Deltos.FindReplaceOptions EditedOptions)
            return;

        fFindReplaceOptions = EditedOptions;
        if (EditedOptions.ReplaceAll)
        {
            int Count = fFindReplaceHandler.ReplaceAll(EditedOptions);
            LogBox.AppendLine($"Replace all completed. Replacements: {Count}");
        }
        else if (EditedOptions.Replace)
        {
            bool Replaced = fFindReplaceHandler.ReplaceCurrent(EditedOptions);
            LogBox.AppendLine(Replaced ? "Replace completed." : "No match found.");
        }
        else
        {
            int Count = fFindReplaceHandler.Find(EditedOptions);
            LogBox.AppendLine($"Find completed. Matches: {Count}");
        }
    }
    /// <summary>
    /// Highlights all matches for a search term.
    /// </summary>
    /// <param name="Term">The search term.</param>
    /// <param name="WholeWord">True for whole-word search.</param>
    /// <param name="MatchCase">True for case-sensitive search.</param>
    /// <returns>The match count.</returns>
    public int HighlightSearchTerm(string Term, bool WholeWord, bool MatchCase)
    {
        fFindReplaceOptions.TextToFind = Term ?? string.Empty;
        fFindReplaceOptions.WholeWord = WholeWord;
        fFindReplaceOptions.MatchCase = MatchCase;
        fFindReplaceOptions.Replace = false;
        fFindReplaceOptions.ReplaceAll = false;
        return fFindReplaceHandler.HighlightAll(fFindReplaceOptions);
    }
    /// <summary>
    /// Highlights all matches and moves the caret to a position.
    /// </summary>
    /// <param name="Term">The search term.</param>
    /// <param name="WholeWord">True for whole-word search.</param>
    /// <param name="MatchCase">True for case-sensitive search.</param>
    /// <param name="Line">The zero-based line.</param>
    /// <param name="Column">The zero-based column.</param>
    public void HighlightSearchTerm(string Term, bool WholeWord, bool MatchCase, int Line, int Column)
    {
        HighlightSearchTerm(Term, WholeWord, MatchCase);
        int DocumentLine = Math.Clamp(Line + 1, 1, TextEditor.Document.LineCount);
        AvaloniaEdit.Document.DocumentLine LineInfo = TextEditor.Document.GetLineByNumber(DocumentLine);
        int Offset = Math.Clamp(LineInfo.Offset + Math.Max(0, Column), LineInfo.Offset, LineInfo.EndOffset);
        TextEditor.CaretOffset = Offset;
        fFindReplaceHandler.FindNext(false);
    }
    /// <summary>
    /// Moves to the next or previous highlighted match.
    /// </summary>
    /// <param name="Previous">True to move to the previous match.</param>
    public void FindNext(bool Previous)
    {
        fFindReplaceHandler.FindNext(Previous);
    }
    /// <summary>
    /// Clears search highlights.
    /// </summary>
    public void ClearSearchHighlights()
    {
        fFindReplaceHandler.ClearHighlights();
    }
    /// <summary>
    /// Requests a global search for the word at the caret.
    /// </summary>
    public async void SearchForTerm()
    {
        await SearchForTerm(false);
    }
    /// <summary>
    /// Requests a global search for the word at the caret.
    /// </summary>
    /// <param name="WholeWord">True for whole-word search.</param>
    public async Task SearchForTerm(bool WholeWord)
    {
        string Term = GetWordAtCaret();
        if (Term.Length <= 2)
            return;

        SearchForTermRequested?.Invoke(this, new TextEditorTermEventArgs(Term, WholeWord));

        GlobalSearchForm Form = AppHost.ShowSideBarForm<GlobalSearchForm>(nameof(GlobalSearchForm), "Search");
        if (Form != null)
            await Form.SearchForTerm(Term, WholeWord);
    }
    /// <summary>
    /// Requests showing the edited file in the file manager.
    /// </summary>
    public void ShowFolder()
    {
        ShowFolderRequested?.Invoke(this, EventArgs.Empty);
    }
    /// <summary>
    /// Shows the current markdown text as an HTML preview tab.
    /// </summary>
    public void ShowMarkdownPreview()
    {
        string PreviewTitle = string.IsNullOrWhiteSpace(Title) ? "HTML Preview" : $"HTML Preview: {Title}";
        string PreviewId = string.IsNullOrWhiteSpace(this.PreviewId) ? $"{GetHashCode()}.HtmlPreview" : this.PreviewId;
        AppHost.ShowMarkdownPreview(PreviewId, PreviewTitle, EditorText);
    }
    /// <summary>
    /// Increases the editor font size.
    /// </summary>
    public void IncreaseFontSize()
    {
        EditorFontSize = Math.Min(MaxFontSize, EditorFontSize + 1);
    }
    /// <summary>
    /// Decreases the editor font size.
    /// </summary>
    public void DecreaseFontSize()
    {
        EditorFontSize = Math.Max(MinFontSize, EditorFontSize - 1);
    }
    /// <summary>
    /// Resets the editor font size.
    /// </summary>
    public void ResetFontSize()
    {
        EditorFontSize = AppHost.Settings?.FontSize ?? DefaultFontSize;
    }
    /// <summary>
    /// Applies global application settings to the editor.
    /// </summary>
    public void ApplyAppSettings()
    {
        AppSettings Settings = AppHost.Settings;
        if (Settings == null)
            return;

        TextEditor.FontFamily = new FontFamily(Settings.FontFamily);
        EditorFontSize = Settings.FontSize;
        UpdateButtonVisibility();
    }
    /// <summary>
    /// Sets the syntax highlighter by mode.
    /// </summary>
    /// <param name="Mode">The highlight mode.</param>
    public void SetHighlightMode(HighlightMode Mode)
    {
        fHighlightMode = Mode;
        TextEditor.SyntaxHighlighting = Mode == HighlightMode.None ? null : Highlighters.Find(Mode);
    }
    /// <summary>
    /// Sets the syntax highlighter by file path extension.
    /// </summary>
    /// <param name="FilePath">The file path.</param>
    public void RegisterHighlighter(string FilePath)
    {
        string Extension = System.IO.Path.GetExtension(FilePath);
        TextEditor.SyntaxHighlighting = Highlighters.FindByExtension(Extension) ?? Highlighters.Find(HighlightMode.Markdown);
    }
    /// <summary>
    /// Updates the line and column status bar panel.
    /// </summary>
    public void UpdateStatusBarLineColumn()
    {
        lblLineColumn.Text = $"Ln: {edtText.TextArea.Caret.Line}, Col: {edtText.TextArea.Caret.Column}";
    }
    /// <summary>
    /// Updates all status bar panels.
    /// </summary>
    public void UpdateStatusBar()
    {
        UpdateStatusBarLineColumn();

        TextStats Stats = TextMetrics.Compute(EditorText);
        lblMetrics.Text = $"Pages: {Stats.EstimatedPages:0.00}, Words: {Stats.WordCount}, Chars: {Stats.CharCount}, Lines: {Stats.LineCount}, Pars: {Stats.ParagraphCount}";
        lblReadOnly.Text = $"ReadOnly: {TextEditor.IsReadOnly}";
        lblModified.Text = Modified ? "Modified" : "Saved";
    }
    /// <summary>
    /// Sets the editor text without marking the editor as modified.
    /// </summary>
    /// <param name="Text">The editor text.</param>
    public void SetEditorText(string Text)
    {
        IgnoreModified = true;
        try
        {
            TextEditor.Text = NormalizeEditorText(Text);
            Modified = false;
        }
        finally
        {
            IgnoreModified = false;
        }
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the TextEditorForm class.
    /// </summary>
    public TextEditorForm()
    {
        InitializeComponent();
        Initialize();
    }

    // ● events
    /// <summary>
    /// Occurs when the editor text changes.
    /// </summary>
    public event EventHandler TextChanged;
    /// <summary>
    /// Occurs when the editor modified state changes.
    /// </summary>
    public event EventHandler ModifiedChanged;
    /// <summary>
    /// Occurs when the Save command is requested.
    /// </summary>
    public event EventHandler SaveRequested;
    /// <summary>
    /// Occurs when the Search For Term command is requested.
    /// </summary>
    public event EventHandler<TextEditorTermEventArgs> SearchForTermRequested;
    /// <summary>
    /// Occurs when the Show Folder command is requested.
    /// </summary>
    public event EventHandler ShowFolderRequested;

    // ● properties
    /// <summary>
    /// Gets the text editor control.
    /// </summary>
    public TextEditor TextEditor => edtText;
    /// <summary>
    /// Gets or sets the editor font size.
    /// </summary>
    public double EditorFontSize
    {
        get => TextEditor.FontSize;
        set => TextEditor.FontSize = Math.Clamp(value, MinFontSize, MaxFontSize);
    }
    /// <summary>
    /// Gets or sets the title displayed in the toolbar.
    /// </summary>
    public string Title
    {
        get => edtTitle.Text ?? string.Empty;
        set => edtTitle.Text = value ?? string.Empty;
    }
    /// <summary>
    /// Gets or sets the editor text.
    /// </summary>
    public string EditorText
    {
        get => TextEditor.Text ?? string.Empty;
        set => SetEditorText(value);
    }
    /// <summary>
    /// Gets or sets a value indicating whether the editor text is modified.
    /// </summary>
    public bool Modified
    {
        get => fModified;
        set
        {
            if (fModified == value)
                return;

            fModified = value;
            TextEditor.IsModified = value;
            UpdateStatusBar();
            ModifiedChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    /// <summary>
    /// Gets or sets a value indicating whether the toolbar is visible.
    /// </summary>
    public bool ToolBarVisible
    {
        get => ToolBarBorder.IsVisible;
        set => ToolBarBorder.IsVisible = value;
    }
    /// <summary>
    /// Gets or sets a value indicating whether modified tracking is ignored.
    /// </summary>
    public bool IgnoreModified
    {
        get => fIgnoreModifiedCount > 0;
        set
        {
            if (value)
                fIgnoreModifiedCount++;
            else
                fIgnoreModifiedCount = Math.Max(0, fIgnoreModifiedCount - 1);
        }
    }
    /// <summary>
    /// Gets or sets the edited file path.
    /// </summary>
    public string FilePath
    {
        get => fFilePath;
        set
        {
            fFilePath = value ?? string.Empty;
            UpdateStatusBar();
        }
    }
    /// <summary>
    /// Gets or sets the markdown preview form identifier.
    /// </summary>
    public string PreviewId
    {
        get => fPreviewId;
        set => fPreviewId = value ?? string.Empty;
    }
    /// <summary>
    /// Gets or sets a value indicating whether the status bar is visible.
    /// </summary>
    public bool StatusBarVisible
    {
        get => StatusBar.IsVisible;
        set => StatusBar.IsVisible = value;
    }
    /// <summary>
    /// Gets or sets a value indicating whether the editor is read-only.
    /// </summary>
    public bool ReadOnly
    {
        get => TextEditor.IsReadOnly;
        set
        {
            TextEditor.IsReadOnly = value;
            UpdateStatusBar();
        }
    }
    /// <summary>
    /// Gets or sets the editor highlight mode.
    /// </summary>
    public HighlightMode HighlightMode
    {
        get => fHighlightMode;
        set => SetHighlightMode(value);
    }
    /// <summary>
    /// Gets or sets a value indicating whether the Save toolbar button is visible.
    /// </summary>
    public bool SaveButtonVisible
    {
        get => fSaveButtonVisible;
        set
        {
            fSaveButtonVisible = value;
            UpdateButtonVisibility();
        }
    }
    /// <summary>
    /// Gets or sets a value indicating whether the Find toolbar button is visible.
    /// </summary>
    public bool FindButtonVisible
    {
        get => fFindButtonVisible;
        set
        {
            fFindButtonVisible = value;
            UpdateButtonVisibility();
        }
    }
    /// <summary>
    /// Gets or sets a value indicating whether the Search For Term toolbar button is visible.
    /// </summary>
    public bool SearchForTermButtonVisible
    {
        get => fSearchForTermButtonVisible;
        set
        {
            fSearchForTermButtonVisible = value;
            UpdateButtonVisibility();
        }
    }
    /// <summary>
    /// Gets or sets a value indicating whether the Show Folder toolbar button is visible.
    /// </summary>
    public bool ShowFolderButtonVisible
    {
        get => fShowFolderButtonVisible;
        set
        {
            fShowFolderButtonVisible = value;
            UpdateButtonVisibility();
        }
    }
}

/// <summary>
/// Provides data for a text editor search term event.
/// </summary>
public class TextEditorTermEventArgs: EventArgs
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the TextEditorTermEventArgs class.
    /// </summary>
    /// <param name="Term">The search term.</param>
    public TextEditorTermEventArgs(string Term)
        : this(Term, false)
    {
    }
    /// <summary>
    /// Initializes a new instance of the TextEditorTermEventArgs class.
    /// </summary>
    /// <param name="Term">The search term.</param>
    /// <param name="WholeWord">True for whole-word search.</param>
    public TextEditorTermEventArgs(string Term, bool WholeWord)
    {
        this.Term = Term ?? string.Empty;
        this.WholeWord = WholeWord;
    }

    // ● properties
    /// <summary>
    /// Gets the search term.
    /// </summary>
    public string Term { get; }
    /// <summary>
    /// Gets a value indicating whether the search is whole-word.
    /// </summary>
    public bool WholeWord { get; }
}
