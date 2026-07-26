// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Forms;

/// <summary>
/// Edits the markdown text parts of a text item.
/// </summary>
public partial class TextFileForm: AppForm
{
    // ● private fields
    /// <summary>
    /// Field for the Item property.
    /// </summary>
    private BaseItem fItem;
    /// <summary>
    /// True while values are loaded into controls.
    /// </summary>
    private bool fLoading;
    /// <summary>
    /// The base tab title without modified marker.
    /// </summary>
    private string fBaseTitle = "Text";

    // ● private
    /// <summary>
    /// Wires editor events.
    /// </summary>
    private void WireEditors()
    {
        foreach (TextEditorForm Editor in Editors)
        {
            Editor.SaveRequested += Editor_SaveRequested;
            Editor.ShowFolderRequested += Editor_ShowFolderRequested;
            Editor.ShowItemInListRequested += Editor_ShowItemInListRequested;
            Editor.ModifiedChanged += Editor_ModifiedChanged;
        }
    }
    /// <summary>
    /// Loads the item into editor controls.
    /// </summary>
    private void LoadItem()
    {
        ApplySettings();
        fLoading = true;
        try
        {
            if (Item == null)
            {
                fBaseTitle = "Text";
                TitleText = fBaseTitle;
                foreach (TextEditorForm Editor in Editors)
                {
                    Editor.EditorText = string.Empty;
                    Editor.FilePath = string.Empty;
                    Editor.PreviewId = string.Empty;
                    Editor.ShowItemInListButtonVisible = false;
                    Editor.ReadOnly = true;
                }
                return;
            }

            fBaseTitle = Item.DisplayTitle;
            TitleText = fBaseTitle;

            if (Item is TextFile TextFile)
                LoadTextFile(TextFile);
            else if (Item is Document Document)
                LoadSynopsisOnly(Document.Synopsis, Document.SynopsisFilePath, $"Document: {Document.Title}");
            else if (Item is Folder Folder)
                LoadSynopsisOnly(Folder.Synopsis, Folder.SynopsisFilePath, string.IsNullOrWhiteSpace(Folder.LevelTitle) ? Folder.Title : $"{Folder.LevelTitle}: {Folder.Title}");

            AdjustTitles();
        }
        finally
        {
            fLoading = false;
        }
    }
    /// <summary>
    /// Loads a text file into all editors.
    /// </summary>
    /// <param name="TextFile">The text file.</param>
    private void LoadTextFile(TextFile TextFile)
    {
        tabText.IsVisible = true;
        tabDraft.IsVisible = true;

        EditorText.Title = TextFile.DisplayTitle;
        EditorText.EditorText = TextFile.Text;
        EditorText.FilePath = TextFile.TextFilePath;
        EditorText.PreviewId = AppHost.GetMarkdownPreviewFormId(TextFile.Id);

        EditorText2.Title = TextFile.DisplayTitle2;
        EditorText2.EditorText = TextFile.Text2;
        EditorText2.FilePath = TextFile.Text2FilePath;
        EditorText2.PreviewId = AppHost.GetMarkdownPreviewFormId(TextFile.Id);

        EditorSynopsis.Title = fBaseTitle;
        EditorSynopsis.EditorText = TextFile.Synopsis;
        EditorSynopsis.FilePath = TextFile.SynopsisFilePath;
        EditorSynopsis.PreviewId = AppHost.GetMarkdownPreviewFormId(TextFile.Id);

        EditorDraft.Title = fBaseTitle;
        EditorDraft.EditorText = TextFile.Draft;
        EditorDraft.FilePath = TextFile.DraftFilePath;
        EditorDraft.PreviewId = AppHost.GetMarkdownPreviewFormId(TextFile.Id);

        foreach (TextEditorForm Editor in Editors)
            PrepareEditor(Editor);

        pager.SelectedItem = tabText;
    }
    /// <summary>
    /// Loads a synopsis-only item into the synopsis editor.
    /// </summary>
    /// <param name="Synopsis">The synopsis text.</param>
    /// <param name="FilePath">The synopsis file path.</param>
    /// <param name="Title">The editor title.</param>
    private void LoadSynopsisOnly(string Synopsis, string FilePath, string Title)
    {
        tabText.IsVisible = false;
        tabDraft.IsVisible = false;

        EditorSynopsis.Title = Title;
        EditorSynopsis.EditorText = Synopsis;
        EditorSynopsis.FilePath = FilePath;
        EditorSynopsis.PreviewId = AppHost.GetMarkdownPreviewFormId(Item.Id);
        EditorSynopsis.ShowItemInListButtonVisible = false;
        PrepareEditor(EditorSynopsis);

        pager.SelectedItem = tabSynopsis;
    }
    /// <summary>
    /// Prepares an editor after loading it.
    /// </summary>
    /// <param name="Editor">The editor.</param>
    private void PrepareEditor(TextEditorForm Editor)
    {
        Editor.ApplyAppSettings();
        Editor.ReadOnly = false;
        Editor.ShowItemInListButtonVisible = Item is TextFile;
        Editor.Modified = false;
        Editor.RegisterHighlighter(Editor.FilePath);
    }
    /// <summary>
    /// Applies application settings to the form.
    /// </summary>
    private void ApplySettings()
    {
        bool Visible = AppHost.Settings?.SecondLanguageVisible == true;
        EditorText2.IsVisible = Visible;
        Text2Splitter.IsVisible = Visible;
        TextGrid.ColumnDefinitions[1].Width = Visible ? GridLength.Auto : new GridLength(0);
        TextGrid.ColumnDefinitions[2].Width = Visible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
    }
    /// <summary>
    /// Saves all editor values to the edited item.
    /// </summary>
    private void SaveItem()
    {
        if (Item == null)
            return;

        if (Item is TextFile TextFile)
        {
            TextFile.Text = EditorText.EditorText;
            TextFile.Text2 = EditorText2.EditorText;
            TextFile.Synopsis = EditorSynopsis.EditorText;
            TextFile.Draft = EditorDraft.EditorText;
            TextFile.Save();
        }
        else if (Item is Document Document)
        {
            Document.Synopsis = EditorSynopsis.EditorText;
            Document.Save();
        }
        else if (Item is Folder Folder)
        {
            Folder.Synopsis = EditorSynopsis.EditorText;
            Folder.Save();
        }

        foreach (TextEditorForm Editor in Editors)
            Editor.Modified = false;
        foreach (TextEditorForm Editor in Editors)
            AppHost.RemoveDirtyEditor(Editor);

        AdjustTitles();
        AppHost.NotifyDocumentMetricsChanged();
        LogBox.AppendLine($"Item saved: {Item.Title}");
    }
    /// <summary>
    /// Updates the host and inner tab titles.
    /// </summary>
    private void AdjustTitles()
    {
        // AppForm.TitleTextChanged() updates the host tab header.
        TitleText = IsModified ? $"{fBaseTitle}*" : fBaseTitle;
        tabText.Header = EditorText.Modified || EditorText2.Modified ? "Text*" : "Text";
        tabSynopsis.Header = EditorSynopsis.Modified ? "Synopsis*" : "Synopsis";
        tabDraft.Header = EditorDraft.Modified ? "Draft*" : "Draft";
    }
    /// <summary>
    /// Refreshes title and file path values after an item metadata change.
    /// </summary>
    private void RefreshItemInfo()
    {
        if (Item == null)
            return;

        fBaseTitle = Item.DisplayTitle;

        if (Item is TextFile TextFile)
        {
            EditorText.Title = fBaseTitle;
            EditorText.FilePath = TextFile.TextFilePath;
            EditorText.PreviewId = AppHost.GetMarkdownPreviewFormId(TextFile.Id);

            EditorText2.Title = TextFile.DisplayTitle2;
            EditorText2.FilePath = TextFile.Text2FilePath;
            EditorText2.PreviewId = AppHost.GetMarkdownPreviewFormId(TextFile.Id);

            EditorSynopsis.Title = fBaseTitle;
            EditorSynopsis.FilePath = TextFile.SynopsisFilePath;
            EditorSynopsis.PreviewId = AppHost.GetMarkdownPreviewFormId(TextFile.Id);

            EditorDraft.Title = fBaseTitle;
            EditorDraft.FilePath = TextFile.DraftFilePath;
            EditorDraft.PreviewId = AppHost.GetMarkdownPreviewFormId(TextFile.Id);
        }
        else if (Item is Document Document)
        {
            EditorSynopsis.Title = $"Document: {Document.Title}";
            EditorSynopsis.FilePath = Document.SynopsisFilePath;
            EditorSynopsis.PreviewId = AppHost.GetMarkdownPreviewFormId(Document.Id);
        }
        else if (Item is Folder Folder)
        {
            EditorSynopsis.Title = string.IsNullOrWhiteSpace(Folder.LevelTitle) ? Folder.Title : $"{Folder.LevelTitle}: {Folder.Title}";
            EditorSynopsis.FilePath = Folder.SynopsisFilePath;
            EditorSynopsis.PreviewId = AppHost.GetMarkdownPreviewFormId(Folder.Id);
        }

        AdjustTitles();
    }
    /// <summary>
    /// Handles editor Save requests.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    private void Editor_SaveRequested(object Sender, EventArgs Args)
    {
        SaveItem();
    }
    /// <summary>
    /// Handles editor Show Folder requests.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    private void Editor_ShowFolderRequested(object Sender, EventArgs Args)
    {
        TextEditorForm Editor = Sender as TextEditorForm;
        if (Editor == null || string.IsNullOrWhiteSpace(Editor.FilePath))
            return;

        Sys.OpenFileExplorer(Editor.FilePath);
    }
    /// <summary>
    /// Handles editor Show Item in List requests.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void Editor_ShowItemInListRequested(object Sender, EventArgs Args)
    {
        if (Item is not TextFile TextFile)
            return;

        AppHost.ShowItemInListPage(new LinkItem(TextFile.Type, LinkPlace.Text, TextFile.DisplayTitle, TextFile));
    }
    /// <summary>
    /// Handles editor modified state changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    private void Editor_ModifiedChanged(object Sender, EventArgs Args)
    {
        if (fLoading)
            return;

        if (Sender is TextEditorForm Editor && Editor.Modified)
            AppHost.AddDirtyEditor(Editor);

        AdjustTitles();
    }

    // ● overrides
    /// <summary>
    /// Sets up this form after its context is assigned.
    /// </summary>
    protected override void Setup()
    {
        Item = Context?.Tag as BaseItem;
    }
    /// <summary>
    /// Called in order to initialize the form.
    /// </summary>
    protected override void FormInitialize()
    {
        WireEditors();
        LoadItem();
    }
    /// <summary>
    /// Called just before the form is closed.
    /// </summary>
    protected override void Closing()
    {
        foreach (TextEditorForm Editor in Editors)
        {
            Editor.SaveRequested -= Editor_SaveRequested;
            Editor.ShowFolderRequested -= Editor_ShowFolderRequested;
            Editor.ShowItemInListRequested -= Editor_ShowItemInListRequested;
            Editor.ModifiedChanged -= Editor_ModifiedChanged;
            AppHost.RemoveDirtyEditor(Editor);
        }

        base.Closing();
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the TextFileForm class.
    /// </summary>
    public TextFileForm()
    {
        InitializeComponent();
    }

    // ● public
    /// <summary>
    /// Refreshes the form after the edited item title changes.
    /// </summary>
    public void RefreshItemTitle()
    {
        RefreshItemInfo();
    }
    /// <summary>
    /// Highlights a search term in the editor represented by a link item.
    /// </summary>
    /// <param name="LinkItem">The link item.</param>
    /// <param name="Term">The search term.</param>
    /// <param name="WholeWord">True for whole-word search.</param>
    /// <param name="MatchCase">True for case-sensitive search.</param>
    public void HighlightAll(LinkItem LinkItem, string Term, bool WholeWord, bool MatchCase)
    {
        if (LinkItem == null)
            return;

        SelectTabFor(LinkItem);
        TextEditorForm Editor = EditorFor(LinkItem);
        if (Editor == null)
            return;

        Editor.HighlightSearchTerm(Term, WholeWord, MatchCase, LinkItem.Line, LinkItem.Column);
    }

    // ● properties
    /// <summary>
    /// Returns the editor represented by a link item.
    /// </summary>
    /// <param name="LinkItem">The link item.</param>
    /// <returns>The editor, if any; otherwise null.</returns>
    TextEditorForm EditorFor(LinkItem LinkItem)
    {
        if (LinkItem.Place == LinkPlace.Text2)
            return EditorText2.IsVisible ? EditorText2 : null;
        if (LinkItem.Place == LinkPlace.Synopsis)
            return EditorSynopsis;
        if (LinkItem.Place == LinkPlace.Draft)
            return EditorDraft;

        return EditorText;
    }
    /// <summary>
    /// Selects the tab represented by a link item.
    /// </summary>
    /// <param name="LinkItem">The link item.</param>
    void SelectTabFor(LinkItem LinkItem)
    {
        if (LinkItem.Place == LinkPlace.Synopsis)
            pager.SelectedItem = tabSynopsis;
        else if (LinkItem.Place == LinkPlace.Draft)
            pager.SelectedItem = tabDraft;
        else
            pager.SelectedItem = tabText;
    }
    /// <summary>
    /// Gets the text editors.
    /// </summary>
    private TextEditorForm[] Editors => new[] { EditorText, EditorText2, EditorSynopsis, EditorDraft };
    /// <summary>
    /// Gets or sets the edited item.
    /// </summary>
    public BaseItem Item
    {
        get => fItem;
        set => fItem = value;
    }
    /// <summary>
    /// Gets a value indicating whether any editor is modified.
    /// </summary>
    public bool IsModified => Editors.Any(Editor => Editor.Modified);
}
