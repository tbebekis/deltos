// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Forms;

/// <summary>
/// Displays project-wide search results.
/// </summary>
public partial class GlobalSearchForm: AppForm
{
    // ● private fields
    /// <summary>
    /// The toolbar helper.
    /// </summary>
    Tripous.Desktop.ToolBar fToolBar;
    /// <summary>
    /// The current search result list.
    /// </summary>
    LinkItemList fLinkItems = new();

    // ● toolbar
    /// <summary>
    /// Creates the toolbar.
    /// </summary>
    void CreateToolBar()
    {
        fToolBar = new();
        fToolBar.Panel = pnlToolBar;

        fToolBar.AddButton("wishlist_add.png", "Add to Quick View", AddSelectedItemToQuickView);
        fToolBar.AddButton("table_select_row.png", "Show Item in its List Page", ShowSelectedItemInListPage);
        fToolBar.AddSeparator();
        fToolBar.AddButton("arrow_in.png", "Collapse All", CollapseAll);
        fToolBar.AddButton("arrow_out.png", "Expand All", ExpandAll);
    }

    // ● search
    /// <summary>
    /// Executes the current search.
    /// </summary>
    async Task Search()
    {
        Project Project = AppHost.CurrentProject;
        string Term = edtTerm.Text == null ? string.Empty : edtTerm.Text.Trim();
        if (Project == null || Term.Length < 3)
        {
            ClearResults();
            return;
        }

        try
        {
            AppHost.ShowPleaseWait("Searching project...", this.GetOwnerWindow());
            await Task.Yield();

            LogBox.AppendLine($"Global search started: {Term}");
            fLinkItems = await Task.Run(() => Project.GlobalSearch(Term));
            LoadResults();
            LogBox.AppendLine($"Global search completed. Found items: {fLinkItems.Count}");
        }
        catch (Exception e)
        {
            LogBox.AppendLine("Global search FAILED.");
            LogBox.AppendLine(e);
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
        finally
        {
            AppHost.HidePleaseWait();
        }
    }
    /// <summary>
    /// Clears the search results.
    /// </summary>
    void ClearResults()
    {
        tvResults.Items.Clear();
        lblTitle.Text = "No selection";
        Editor.EditorText = string.Empty;
        Editor.FilePath = string.Empty;
        fLinkItems = new();
    }
    /// <summary>
    /// Loads the search results into the tree.
    /// </summary>
    void LoadResults()
    {
        tvResults.Items.Clear();
        BaseItem CurrentItem = null;
        TreeViewItem ParentNode = null;

        foreach (LinkItem LinkItem in fLinkItems.List)
        {
            if (LinkItem.Item != CurrentItem)
            {
                CurrentItem = LinkItem.Item;
                ParentNode = CreateParentNode(LinkItem);
                tvResults.Items.Add(ParentNode);
            }

            if (ParentNode != null)
                ParentNode.Items.Add(CreateChildNode(LinkItem));
        }

        CollapseAll();
        ShowNoSelection();
    }
    /// <summary>
    /// Creates a parent result node.
    /// </summary>
    /// <param name="LinkItem">The link item.</param>
    /// <returns>The created node.</returns>
    TreeViewItem CreateParentNode(LinkItem LinkItem)
    {
        TreeViewItem Result = new();
        Result.Header = $"{LinkItem.ItemType} - {LinkItem.Title}";
        Result.Tag = LinkItem;
        return Result;
    }
    /// <summary>
    /// Creates a child result node.
    /// </summary>
    /// <param name="LinkItem">The link item.</param>
    /// <returns>The created node.</returns>
    TreeViewItem CreateChildNode(LinkItem LinkItem)
    {
        TreeViewItem Result = new();
        Result.Header = $"{LinkItem.Place} - [{LinkItem.Line + 1}, {LinkItem.Column + 1}] - {LinkItem.LineText}";
        Result.Tag = LinkItem;
        return Result;
    }

    // ● commands
    /// <summary>
    /// Adds the selected item to QuickView.
    /// </summary>
    void AddSelectedItemToQuickView()
    {
        LinkItem LinkItem = SelectedLinkItem;
        if (LinkItem == null)
            return;

        AppHost.AddToQuickView(LinkItem);
    }
    /// <summary>
    /// Shows the selected item in its list page.
    /// </summary>
    void ShowSelectedItemInListPage()
    {
        LinkItem LinkItem = SelectedLinkItem;
        if (LinkItem == null)
            return;

        AppHost.ShowItemInListPage(LinkItem);
    }
    /// <summary>
    /// Opens the selected item page.
    /// </summary>
    async Task ShowSelectedItemPage()
    {
        LinkItem LinkItem = SelectedLinkItem;
        if (LinkItem == null)
            return;

        AppForm Form = AppHost.ShowLinkItemPage(LinkItem);
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(
            () => HighlightLinkItem(Form, LinkItem),
            Avalonia.Threading.DispatcherPriority.Background);
    }
    /// <summary>
    /// Highlights the selected link item in an opened form.
    /// </summary>
    /// <param name="Form">The opened form.</param>
    /// <param name="LinkItem">The link item.</param>
    void HighlightLinkItem(AppForm Form, LinkItem LinkItem)
    {
        string Term = GetSearchTerm(out bool WholeWord);
        bool MatchCase = WholeWord;
        if (string.IsNullOrWhiteSpace(Term) || Form == null || LinkItem == null)
            return;

        if (Form is TextFileForm TextFileForm)
            TextFileForm.HighlightAll(LinkItem, Term, WholeWord, MatchCase);
        else if (Form is ComponentForm ComponentForm)
            ComponentForm.HighlightAll(LinkItem, Term, WholeWord, MatchCase);
        else if (Form is NoteForm NoteForm)
            NoteForm.HighlightAll(LinkItem, Term, WholeWord, MatchCase);
    }
    /// <summary>
    /// Returns the normalized search term.
    /// </summary>
    /// <param name="WholeWord">True when the term is whole-word.</param>
    /// <returns>The normalized term.</returns>
    string GetSearchTerm(out bool WholeWord)
    {
        string Result = edtTerm.Text == null ? string.Empty : edtTerm.Text.Trim();
        WholeWord = Result.Length > 2 && Result.StartsWith("\"") && Result.EndsWith("\"");
        if (WholeWord)
            Result = Result.Substring(1, Result.Length - 2).Trim();

        return Result;
    }
    /// <summary>
    /// Expands all result nodes.
    /// </summary>
    void ExpandAll()
    {
        ExpandItems(tvResults.Items, true);
    }
    /// <summary>
    /// Collapses all result nodes.
    /// </summary>
    void CollapseAll()
    {
        ExpandItems(tvResults.Items, false);
    }
    /// <summary>
    /// Expands or collapses tree items.
    /// </summary>
    /// <param name="Items">The items.</param>
    /// <param name="Expanded">True to expand; false to collapse.</param>
    void ExpandItems(ItemCollection Items, bool Expanded)
    {
        foreach (object Item in Items)
        {
            if (Item is TreeViewItem Node)
            {
                Node.IsExpanded = Expanded;
                ExpandItems(Node.Items, Expanded);
            }
        }
    }

    // ● preview
    /// <summary>
    /// Shows the selected link item.
    /// </summary>
    /// <param name="LinkItem">The link item.</param>
    void ShowLinkItem(LinkItem LinkItem)
    {
        if (LinkItem == null)
        {
            ShowNoSelection();
            return;
        }

        lblTitle.Text = $"{LinkItem.ItemType} - {LinkItem.Title} - {LinkItem.Place}";
        Editor.EditorText = GetLinkText(LinkItem);
        Editor.FilePath = string.Empty;
    }
    /// <summary>
    /// Clears the preview.
    /// </summary>
    void ShowNoSelection()
    {
        lblTitle.Text = "No selection";
        Editor.EditorText = string.Empty;
        Editor.FilePath = string.Empty;
    }
    /// <summary>
    /// Returns the text represented by a link item.
    /// </summary>
    /// <param name="LinkItem">The link item.</param>
    /// <returns>The link item text.</returns>
    string GetLinkText(LinkItem LinkItem)
    {
        if (LinkItem == null || LinkItem.Item == null)
            return string.Empty;

        if (LinkItem.Place == LinkPlace.Title)
            return LinkItem.Item.Title;
        if (LinkItem.Place == LinkPlace.TempFile && LinkItem.Item is Project Project)
            return Project.TempFileText;
        if (LinkItem.Item is Document Document)
            return Document.Synopsis;
        if (LinkItem.Item is Folder Folder)
            return Folder.Synopsis;
        if (LinkItem.Item is TextFile TextFile)
            return GetTextFileText(TextFile, LinkItem.Place);
        if (LinkItem.Item is Component Component)
            return LinkItem.Place == LinkPlace.Text2 ? Component.Text2 : Component.Text;
        if (LinkItem.Item is Note Note)
            return Note.Text;

        return string.Empty;
    }
    /// <summary>
    /// Returns text from a text file by link place.
    /// </summary>
    /// <param name="TextFile">The text file.</param>
    /// <param name="Place">The link place.</param>
    /// <returns>The requested text.</returns>
    string GetTextFileText(TextFile TextFile, LinkPlace Place)
    {
        if (Place == LinkPlace.Text2)
            return TextFile.Text2;
        if (Place == LinkPlace.Synopsis)
            return TextFile.Synopsis;
        if (Place == LinkPlace.Draft)
            return TextFile.Draft;

        return TextFile.Text;
    }

    // ● events
    /// <summary>
    /// Handles search term key presses.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The key event arguments.</param>
    async void TermKeyDown(object Sender, KeyEventArgs Args)
    {
        if (Args.Key != Key.Enter)
            return;

        Args.Handled = true;
        await Search();
    }
    /// <summary>
    /// Handles result selection changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void ResultsSelectionChanged(object Sender, SelectionChangedEventArgs Args)
    {
        ShowLinkItem(SelectedLinkItem);
    }
    /// <summary>
    /// Handles result double-tap events.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    async void ResultsDoubleTapped(object Sender, TappedEventArgs Args)
    {
        await ShowSelectedItemPage();
        Args.Handled = true;
    }

    // ● overrides
    /// <summary>
    /// Called in order to initialize the form.
    /// </summary>
    protected override void FormInitialize()
    {
        TitleText = "Search";
        ClosableByUser = false;
        CreateToolBar();
        Editor.ToolBarVisible = false;
        Editor.ReadOnly = true;
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the GlobalSearchForm class.
    /// </summary>
    public GlobalSearchForm()
    {
        InitializeComponent();
    }

    // ● public
    /// <summary>
    /// Executes a search for a term.
    /// </summary>
    /// <param name="Term">The search term.</param>
    /// <param name="WholeWord">True for whole-word search.</param>
    public async Task SearchForTerm(string Term, bool WholeWord)
    {
        if (string.IsNullOrWhiteSpace(Term))
            return;

        edtTerm.Text = WholeWord ? $"\"{Term.Trim()}\"" : Term.Trim();
        await Search();
    }

    // ● properties
    /// <summary>
    /// Gets the selected link item.
    /// </summary>
    LinkItem SelectedLinkItem
    {
        get
        {
            if (tvResults.SelectedItem is TreeViewItem Node)
                return Node.Tag as LinkItem;

            return null;
        }
    }
}
