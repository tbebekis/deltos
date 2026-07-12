// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Forms;

/// <summary>
/// Displays the project document tree.
/// </summary>
public partial class DocumentListForm: AppForm
{
    // ● private fields
    private Tripous.Desktop.ToolBar fToolBar;
    /// <summary>
    /// Field for the New command menu.
    /// </summary>
    private ContextMenu fNewMenu;

    // ● private
    /// <summary>
    /// Creates the form toolbar.
    /// </summary>
    private void CreateToolBar()
    {
        fToolBar = new();
        fToolBar.Panel = pnlToolBar;

        fToolBar.AddDropDownButton("table_add.png", "New", CreateNewMenu(), NewMenuOpening);
        fToolBar.AddButton("table_edit.png", "Edit", async () => await EditSelectedItemInfo());
        fToolBar.AddButton("table_delete.png", "Delete", async () => await DeleteSelectedItem());
        fToolBar.AddSeparator();
        fToolBar.AddButton("page_edit.png", "Edit Text", EditSelectedItem);
        fToolBar.AddButton("table_export.png", "Export Document", () => ExecutePlaceholderCommand("ExportDocument"));
        fToolBar.AddButton("scroll_pane_tree.png", "Change Parent", () => ExecutePlaceholderCommand("ChangeParent"));
        fToolBar.AddSeparator();
        fToolBar.AddButton("arrow_out.png", "Expand All", ExpandAll);
        fToolBar.AddButton("arrow_in.png", "Collapse All", CollapseAll);
        fToolBar.AddButton("arrow_up.png", "Up", () => ExecutePlaceholderCommand("Up"));
        fToolBar.AddButton("arrow_down.png", "Down", () => ExecutePlaceholderCommand("Down"));
    }

    /// <summary>
    /// Creates the New command menu.
    /// </summary>
    /// <returns>The created context menu.</returns>
    private ContextMenu CreateNewMenu()
    {
        fNewMenu = new ContextMenu();
        BuildNewMenu();
        return fNewMenu;
    }
    /// <summary>
    /// Handles the New command menu opening.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    private void NewMenuOpening(object Sender, System.ComponentModel.CancelEventArgs Args)
    {
        BuildNewMenu();
    }
    /// <summary>
    /// Builds the New command menu.
    /// </summary>
    private void BuildNewMenu()
    {
        if (fNewMenu == null)
            return;

        fNewMenu.Items.Clear();

        BaseItem SelectedItem = GetSelectedBaseItem();

        MenuItem DocumentItem = new MenuItem { Header = "Document", IsEnabled = AppHost.CurrentProject?.CanAddDocument == true };
        DocumentItem.Click += async (Sender, Args) => await NewDocument();
        fNewMenu.Items.Add(DocumentItem);

        MenuItem FolderItem = new MenuItem { Header = "Folder", IsEnabled = SelectedItem is Document || SelectedItem is Folder };
        AddFolderLevelMenuItems(FolderItem, SelectedItem);
        fNewMenu.Items.Add(FolderItem);

        MenuItem TextItem = new MenuItem { Header = "Text", IsEnabled = SelectedItem?.CanAddTextFile == true };
        TextItem.Click += async (Sender, Args) => await NewText();
        fNewMenu.Items.Add(TextItem);
    }
    /// <summary>
    /// Adds folder level menu items.
    /// </summary>
    /// <param name="FolderItem">The folder menu item.</param>
    /// <param name="SelectedItem">The selected base item.</param>
    private void AddFolderLevelMenuItems(MenuItem FolderItem, BaseItem SelectedItem)
    {
        Document Document = GetDocumentContext(SelectedItem);
        List<string> LevelTitles = GetStructureLevelTitles(Document);

        foreach (string LevelTitle in LevelTitles)
        {
            MenuItem LevelItem = new MenuItem
            {
                Header = LevelTitle,
                IsEnabled = CanAddFolderLevel(SelectedItem, LevelTitle),
                Tag = LevelTitle
            };
            LevelItem.Click += async (Sender, Args) =>
            {
                if (Sender is MenuItem MenuItem && MenuItem.Tag is string SelectedLevelTitle)
                    await NewFolder(SelectedLevelTitle);
            };

            FolderItem.Items.Add(LevelItem);
        }

        if (LevelTitles.Count == 0)
            FolderItem.IsEnabled = false;
    }
    /// <summary>
    /// Returns the document context of an item.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <returns>The document context, if any; otherwise null.</returns>
    private Document GetDocumentContext(BaseItem Item)
    {
        if (Item is Document Document)
            return Document;

        return Item?.Document;
    }
    /// <summary>
    /// Returns the structure level titles of a document.
    /// </summary>
    /// <param name="Document">The document.</param>
    /// <returns>The structure level titles.</returns>
    private List<string> GetStructureLevelTitles(Document Document)
    {
        List<string> Result = new();
        FolderItem Item = Document?.Structure;

        while (Item != null && !string.IsNullOrWhiteSpace(Item.Title))
        {
            Result.Add(Item.Title);
            Item = Item.Child;
        }

        return Result;
    }
    /// <summary>
    /// Returns the expected child folder level title for an item.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <returns>The expected child folder level title, if any; otherwise an empty string.</returns>
    private string GetExpectedChildFolderLevelTitle(BaseItem Item)
    {
        if (Item is Document Document && Document.CanAddFolder)
            return Document.Structure?.Title ?? string.Empty;

        if (Item is Folder Folder && Folder.CanAddFolder)
            return Folder.StructureItem?.Child?.Title ?? string.Empty;

        return string.Empty;
    }
    /// <summary>
    /// Returns true if a folder level can be added to an item.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <param name="LevelTitle">The folder level title.</param>
    /// <returns>True if the folder level can be added; otherwise false.</returns>
    private bool CanAddFolderLevel(BaseItem Item, string LevelTitle)
    {
        if (!(Item is Document || Item is Folder) || Item.CanAddFolder == false)
            return false;

        string ExpectedLevelTitle = GetExpectedChildFolderLevelTitle(Item);
        return string.Equals(ExpectedLevelTitle, LevelTitle, StringComparison.OrdinalIgnoreCase);
    }
    /// <summary>
    /// Creates a new document.
    /// </summary>
    private async Task NewDocument()
    {
        if (AppHost.CurrentProject == null)
        {
            await Tripous.Desktop.MessageBox.Info("No project is open.", this);
            return;
        }

        InputBoxData BoxData = await InputBox.ShowModal("Document title", string.Empty, this);
        if (!BoxData.Result)
            return;

        string Title = BoxData.Value?.Trim();
        if (!AppHost.IsValidFileName(Title, false))
        {
            await Tripous.Desktop.MessageBox.Error($"Invalid document title: {Title}", this);
            return;
        }

        DialogInfo Info = await DialogWindow.ShowModal<DocumentStructureDialog>(null, this);
        if (!Info.Result)
            return;

        try
        {
            AppHost.ShowPleaseWait("Creating document...", this.GetOwnerWindow());

            FolderItem Structure = Info.ResultData as FolderItem;
            Document Document = Structure == null
                ? AppHost.CurrentProject.AddDocument(Title)
                : AppHost.CurrentProject.AddDocument(Title, Structure);

            CreateProjectTree();
            SelectTreeItem(Document);
            ShowSelectedItem(Document);
            LogBox.AppendLine($"Document created: {Document.Title}");
        }
        catch (Exception e)
        {
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
        finally
        {
            AppHost.HidePleaseWait();
        }
    }
    /// <summary>
    /// Creates a new folder under the selected item.
    /// </summary>
    /// <param name="LevelTitle">The folder level title.</param>
    private async Task NewFolder(string LevelTitle)
    {
        BaseItem SelectedItem = GetSelectedBaseItem();
        if (!CanAddFolderLevel(SelectedItem, LevelTitle))
            return;

        InputBoxData BoxData = await InputBox.ShowModal($"{LevelTitle} title", string.Empty, this);
        if (!BoxData.Result)
            return;

        string Title = BoxData.Value?.Trim();
        if (!AppHost.IsValidFileName(Title, false))
        {
            await Tripous.Desktop.MessageBox.Error($"Invalid {LevelTitle} title: {Title}", this);
            return;
        }

        try
        {
            AppHost.ShowPleaseWait($"Creating {LevelTitle}...", this.GetOwnerWindow());

            Folder Folder = SelectedItem.AddFolder(Title, LevelTitle);
            CreateProjectTree();
            SelectTreeItem(Folder);
            ShowSelectedItem(Folder);
            LogBox.AppendLine($"{LevelTitle} created: {Folder.Title}");
        }
        catch (Exception e)
        {
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
        finally
        {
            AppHost.HidePleaseWait();
        }
    }
    /// <summary>
    /// Creates a new text file under the selected item.
    /// </summary>
    private async Task NewText()
    {
        BaseItem SelectedItem = GetSelectedBaseItem();
        if (SelectedItem?.CanAddTextFile != true)
            return;

        InputBoxData BoxData = await InputBox.ShowModal("Text title", string.Empty, this);
        if (!BoxData.Result)
            return;

        string Title = BoxData.Value?.Trim();
        if (!AppHost.IsValidFileName(Title, false))
        {
            await Tripous.Desktop.MessageBox.Error($"Invalid text title: {Title}", this);
            return;
        }

        try
        {
            AppHost.ShowPleaseWait("Creating text...", this.GetOwnerWindow());

            TextFile File = SelectedItem.AddTextFile(Title);
            CreateProjectTree();
            SelectTreeItem(File);
            ShowSelectedItem(File);
            LogBox.AppendLine($"Text created: {File.Title}");
        }
        catch (Exception e)
        {
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
        finally
        {
            AppHost.HidePleaseWait();
        }
    }
    /// <summary>
    /// Executes a placeholder command.
    /// </summary>
    /// <param name="CommandName">The command name.</param>
    private void ExecutePlaceholderCommand(string CommandName)
    {
        LogBox.AppendLine($"DocumentListForm command not implemented yet: {CommandName}");
    }
    /// <summary>
    /// Edits the selected item information.
    /// </summary>
    private async Task EditSelectedItemInfo()
    {
        BaseItem Item = GetSelectedBaseItem();
        if (Item == null)
            return;

        if (!Item.CanRename())
        {
            await Tripous.Desktop.MessageBox.Info("The selected item cannot be renamed.", this);
            return;
        }

        InputBoxData BoxData = await InputBox.ShowModal("Title", Item.Title, this);
        if (!BoxData.Result)
            return;

        string Title = BoxData.Value?.Trim();
        if (string.Equals(Title, Item.Title, StringComparison.Ordinal))
            return;

        if (!AppHost.IsValidFileName(Title, false))
        {
            await Tripous.Desktop.MessageBox.Error($"Invalid title: {Title}", this);
            return;
        }

        try
        {
            AppHost.ShowPleaseWait("Renaming item...", this.GetOwnerWindow());

            Item.Rename(Title);
            AppHost.NotifyItemTitleChanged(Item);
            CreateProjectTree();
            SelectTreeItem(Item);
            ShowSelectedItem(Item);
            LogBox.AppendLine($"Item renamed: {Item.DisplayTitle}");
        }
        catch (Exception e)
        {
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
        finally
        {
            AppHost.HidePleaseWait();
        }
    }
    /// <summary>
    /// Deletes the selected item.
    /// </summary>
    private async Task DeleteSelectedItem()
    {
        BaseItem Item = GetSelectedBaseItem();
        if (Item == null)
            return;

        if (!Item.CanDelete())
        {
            await Tripous.Desktop.MessageBox.Info("The selected item cannot be deleted.", this);
            return;
        }

        bool Confirmed = await Tripous.Desktop.MessageBox.YesNo($"Delete {Item.DisplayTitle}?", this);
        if (!Confirmed)
            return;

        try
        {
            AppHost.ShowPleaseWait("Deleting item...", this.GetOwnerWindow());

            string DeletedTitle = Item.DisplayTitle;
            AppHost.CloseContentFormForItem(Item);

            if (!Item.DeleteFromParent())
                return;

            CreateProjectTree();
            ShowNoSelectedItem();
            LogBox.AppendLine($"Item deleted: {DeletedTitle}");
        }
        catch (Exception e)
        {
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
        finally
        {
            AppHost.HidePleaseWait();
        }
    }
    /// <summary>
    /// Returns the currently selected base item.
    /// </summary>
    /// <returns>The selected base item, if any; otherwise null.</returns>
    private BaseItem GetSelectedBaseItem()
    {
        if (tvProject.SelectedItem is TreeViewItem Node)
            return Node.Tag as BaseItem;

        return null;
    }
    /// <summary>
    /// Selects a base item in the project tree.
    /// </summary>
    /// <param name="Item">The base item.</param>
    private void SelectTreeItem(BaseItem Item)
    {
        SelectTreeItem(tvProject, Item);
    }
    /// <summary>
    /// Selects a base item in a tree branch.
    /// </summary>
    /// <param name="ParentNode">The parent tree node.</param>
    /// <param name="Item">The base item.</param>
    /// <returns>True if the item is selected; otherwise false.</returns>
    private bool SelectTreeItem(ItemsControl ParentNode, BaseItem Item)
    {
        if (ParentNode == null || Item == null)
            return false;

        foreach (object Child in ParentNode.Items)
        {
            if (Child is TreeViewItem Node)
            {
                if (ReferenceEquals(Node.Tag, Item))
                {
                    Node.IsSelected = true;
                    return true;
                }

                if (SelectTreeItem(Node, Item))
                {
                    Node.IsExpanded = true;
                    return true;
                }
            }
        }

        return false;
    }
    /// <summary>
    /// Edits the selected item text in the content area.
    /// </summary>
    private void EditSelectedItem()
    {
        BaseItem Item = GetSelectedBaseItem();
        EditItem(Item);
    }
    /// <summary>
    /// Edits an item text in the content area.
    /// </summary>
    /// <param name="Item">The item to edit.</param>
    private void EditItem(BaseItem Item)
    {
        if (Item == null)
            return;

        AppHost.ShowContentForm<TextFileForm>(Item.Id, Item.DisplayTitle, Item);
    }

    /// <summary>
    /// Creates the project tree nodes.
    /// </summary>
    private void CreateProjectTree()
    {
        tvProject.Items.Clear();

        Project Project = AppHost.CurrentProject;
        if (Project == null)
        {
            lblProjectTitle.Text = "No project open";
            txtMetrics.Text = "No project open.";
            lblTextTitle.Text = "Text";
            Editor.EditorText = string.Empty;
            Editor.FilePath = string.Empty;
            return;
        }

        lblProjectTitle.Text = Project.DisplayTitle;

        foreach (Document Document in Project.Documents)
            AddDocumentNode(tvProject, Document);

        txtMetrics.Text = BuildMetricsText(Project);
    }

    /// <summary>
    /// Adds a document node.
    /// </summary>
    /// <param name="ParentNode">The parent tree node.</param>
    /// <param name="Document">The document.</param>
    private void AddDocumentNode(ItemsControl ParentNode, Document Document)
    {
        TreeViewItem DocumentNode = CreateNode(Document.DisplayTitle, Document);
        ParentNode.Items.Add(DocumentNode);

        foreach (Folder Folder in Document.Folders)
            AddFolderNode(DocumentNode, Folder);

        foreach (TextFile File in Document.Files)
            AddTextFileNode(DocumentNode, File);
    }

    /// <summary>
    /// Adds a folder node.
    /// </summary>
    /// <param name="ParentNode">The parent tree node.</param>
    /// <param name="Folder">The folder.</param>
    private void AddFolderNode(TreeViewItem ParentNode, Folder Folder)
    {
        TreeViewItem FolderNode = CreateNode(Folder.DisplayTitle, Folder);
        ParentNode.Items.Add(FolderNode);

        foreach (Folder ChildFolder in Folder.Folders)
            AddFolderNode(FolderNode, ChildFolder);

        foreach (TextFile File in Folder.Files)
            AddTextFileNode(FolderNode, File);
    }

    /// <summary>
    /// Adds a text file node.
    /// </summary>
    /// <param name="ParentNode">The parent tree node.</param>
    /// <param name="File">The text file.</param>
    private void AddTextFileNode(TreeViewItem ParentNode, TextFile File)
    {
        ParentNode.Items.Add(CreateNode(File.DisplayTitle, File));
    }

    /// <summary>
    /// Creates a tree node.
    /// </summary>
    /// <param name="Text">The node text.</param>
    /// <param name="Tag">The node tag.</param>
    /// <returns>The created tree node.</returns>
    private TreeViewItem CreateNode(string Text, object Tag)
    {
        return Tag switch
        {
            Project => Ui.CreateContainerNode(Text, Tag, IconFile: "application_home.png", NegativeMargin: 10),
            Document => Ui.CreateContainerNode(Text, Tag, IconFile: "book.png", NegativeMargin: 10),
            Folder => Ui.CreateContainerNode(Text, Tag, IconFile: "folder.png", NegativeMargin: 10),
            TextFile => Ui.CreateLeafNode(Text, Tag, IconFile: "table.png", NegativeMargin: 10),
            _ => new TreeViewItem { Header = Text, Tag = Tag }
        };
    }

    /// <summary>
    /// Builds the text metrics display.
    /// </summary>
    /// <param name="Project">The project.</param>
    /// <returns>The text metrics display.</returns>
    private string BuildMetricsText(Project Project)
    {
        int DocumentCount = Project.Documents.Count;
        int FolderCount = Project.Documents.Sum(CountFolders);
        int TextFileCount = Project.Documents.Sum(CountTextFiles);

        return $"""
               Project: {Project.Title}

               Documents: {DocumentCount}
               Folders: {FolderCount}
               Text files: {TextFileCount}
               """;
    }

    /// <summary>
    /// Counts all folders in a document.
    /// </summary>
    /// <param name="Document">The document.</param>
    /// <returns>The folder count.</returns>
    private int CountFolders(Document Document)
    {
        return Document.Folders.Sum(CountFolders);
    }

    /// <summary>
    /// Counts all folders in a folder.
    /// </summary>
    /// <param name="Folder">The folder.</param>
    /// <returns>The folder count.</returns>
    private int CountFolders(Folder Folder)
    {
        return 1 + Folder.Folders.Sum(CountFolders);
    }

    /// <summary>
    /// Counts all text files in a document.
    /// </summary>
    /// <param name="Document">The document.</param>
    /// <returns>The text file count.</returns>
    private int CountTextFiles(Document Document)
    {
        return Document.Files.Count + Document.Folders.Sum(CountTextFiles);
    }

    /// <summary>
    /// Counts all text files in a folder.
    /// </summary>
    /// <param name="Folder">The folder.</param>
    /// <returns>The text file count.</returns>
    private int CountTextFiles(Folder Folder)
    {
        return Folder.Files.Count + Folder.Folders.Sum(CountTextFiles);
    }

    /// <summary>
    /// Handles selected tree item changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    private void TreeSelectionChanged(object Sender, SelectionChangedEventArgs Args)
    {
        if (tvProject.SelectedItem is TreeViewItem Node)
            ShowSelectedItem(Node.Tag as BaseItem);
    }
    /// <summary>
    /// Handles tree pointer press events.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    private void TreePointerPressed(object Sender, PointerPressedEventArgs Args)
    {
        if (!Args.GetCurrentPoint(tvProject).Properties.IsRightButtonPressed)
            return;

        TreeViewItem Node = GetTreeNode(Args.Source);
        BaseItem Item = Node?.Tag as BaseItem;
        if (Item == null)
            return;

        Node.IsSelected = true;
        EditItem(Item);
        Args.Handled = true;
    }
    /// <summary>
    /// Returns the tree node that raised an input event.
    /// </summary>
    /// <param name="Source">The input event source.</param>
    /// <returns>The source tree node, if any; otherwise null.</returns>
    private TreeViewItem GetTreeNode(object Source)
    {
        if (Source is TreeViewItem Node)
            return Node;

        if (Source is Avalonia.Visual Visual)
            return Visual.FindAncestorOfType<TreeViewItem>();

        return null;
    }

    /// <summary>
    /// Shows selected item information.
    /// </summary>
    /// <param name="Item">The selected item.</param>
    private void ShowSelectedItem(BaseItem Item)
    {
        if (Item == null)
            return;

        if (Item is Document Document)
        {
            lblTextTitle.Text = $"Document: {Document.Title}";
            Editor.EditorText = Document.Synopsis;
            Editor.FilePath = Document.SynopsisFilePath;
        }
        else if (Item is Folder Folder)
        {
            lblTextTitle.Text = $"{Folder.LevelTitle}: {Folder.Title}";
            Editor.EditorText = Folder.Synopsis;
            Editor.FilePath = Folder.SynopsisFilePath;
        }
        else if (Item is TextFile File)
        {
            lblTextTitle.Text = $"TextFile: {File.Title}";
            Editor.EditorText = File.Text;
            Editor.FilePath = File.TextFilePath;
        }
        else
        {
            lblTextTitle.Text = Item.DisplayTitle;
            Editor.EditorText = string.Empty;
            Editor.FilePath = string.Empty;
        }
    }
    /// <summary>
    /// Clears selected item information.
    /// </summary>
    private void ShowNoSelectedItem()
    {
        lblTextTitle.Text = "Text";
        Editor.EditorText = string.Empty;
        Editor.FilePath = string.Empty;
    }

    /// <summary>
    /// Expands all tree nodes.
    /// </summary>
    private void ExpandAll()
    {
        tvProject.ExpandAll(true);
    }

    /// <summary>
    /// Collapses all tree nodes.
    /// </summary>
    private void CollapseAll()
    {
        tvProject.ExpandAll(false);
    }

    // ● overrides
    /// <summary>
    /// Called in order to initialize the form.
    /// </summary>
    protected override void FormInitialize()
    {
        TitleText = "Documents";
        ClosableByUser = false;
        CreateToolBar();
        CreateProjectTree();
        tvProject.SelectionChanged += TreeSelectionChanged;
        tvProject.AddHandler(InputElement.PointerPressedEvent, TreePointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the DocumentListForm class.
    /// </summary>
    public DocumentListForm()
    {
        InitializeComponent();
    }
}
