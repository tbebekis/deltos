// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Forms;

/// <summary>
/// Displays the project document tree.
/// </summary>
public partial class DocumentListForm: AppForm
{
    // ● private fields
    Tripous.Desktop.ToolBar fToolBar;
    Button fMoveUpButton;
    Button fMoveDownButton;
    /// <summary>
    /// Field for the New command menu.
    /// </summary>
    ContextMenu fNewMenu;

    // ● toolbar
    /// <summary>
    /// Creates the form toolbar.
    /// </summary>
    void CreateToolBar()
    {
        fToolBar = new();
        fToolBar.Panel = pnlToolBar;

        fToolBar.AddDropDownButton("table_add.png", "New", CreateNewMenu(), NewMenuOpening);
        fToolBar.AddButton("table_edit.png", "Edit", async () => await EditSelectedItemInfo());
        fToolBar.AddButton("table_delete.png", "Delete", async () => await DeleteSelectedItem());
        fToolBar.AddSeparator();
        fToolBar.AddButton("page_edit.png", "Edit Text", EditSelectedItem);
        fToolBar.AddButton("html.png", "HTML Preview", PreviewSelectedItem);
        fToolBar.AddButton("table_export.png", "Export Document", async () => await ExportDocument());
        fToolBar.AddButton("scroll_pane_tree.png", "Change Parent", async () => await ChangeSelectedItemParent());
        fToolBar.AddSeparator();
        fToolBar.AddButton("arrow_out.png", "Expand All", () => tvProject.ExpandAll(Flag: true));
        fToolBar.AddButton("arrow_in.png", "Collapse All", () => tvProject.ExpandAll(Flag: false));
        fMoveUpButton = fToolBar.AddButton("arrow_up.png", "Up", async () => await MoveSelectedItem(true));
        fMoveDownButton = fToolBar.AddButton("arrow_down.png", "Down", async () => await MoveSelectedItem(false));
        UpdateMoveButtons();
    }
    /// <summary>
    /// Updates the move button enabled state.
    /// </summary>
    void UpdateMoveButtons()
    {
        BaseItem Item = GetSelectedBaseItem();
        if (fMoveUpButton != null)
            fMoveUpButton.IsEnabled = CanMoveSelectedItem(Item, true);

        if (fMoveDownButton != null)
            fMoveDownButton.IsEnabled = CanMoveSelectedItem(Item, false);
    }

    // ● new menu
    /// <summary>
    /// Creates the New command menu.
    /// </summary>
    /// <returns>The created context menu.</returns>
    ContextMenu CreateNewMenu()
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
    void NewMenuOpening(object Sender, System.ComponentModel.CancelEventArgs Args)
    {
        BuildNewMenu();
    }
    /// <summary>
    /// Builds the New command menu.
    /// </summary>
    void BuildNewMenu()
    {
        if (fNewMenu == null)
            return;

        fNewMenu.Items.Clear();

        BaseItem SelectedItem = GetSelectedBaseItem();

        MenuItem DocumentItem = new MenuItem { Header = "Document", IsEnabled = AppHost.CurrentProject?.CanAddDocument == true };
        DocumentItem.Click += async (Sender, Args) => await NewDocument();
        fNewMenu.Items.Add(DocumentItem);

        MenuItem FolderItem = new MenuItem { Header = "Folder" };
        FolderItem.IsEnabled = AddFolderLevelMenuItems(FolderItem, SelectedItem);
        fNewMenu.Items.Add(FolderItem);

        MenuItem TextItem = new MenuItem { Header = "Text", IsEnabled = GetTextCreationParent(SelectedItem) != null };
        TextItem.Click += async (Sender, Args) => await NewText();
        fNewMenu.Items.Add(TextItem);
    }
    /// <summary>
    /// Adds folder level menu items.
    /// </summary>
    /// <param name="FolderItem">The folder menu item.</param>
    /// <param name="SelectedItem">The selected base item.</param>
    /// <returns>True if at least one folder level can be added; otherwise false.</returns>
    bool AddFolderLevelMenuItems(MenuItem FolderItem, BaseItem SelectedItem)
    {
        Document Document = GetDocumentContext(SelectedItem);
        List<string> LevelTitles = GetStructureLevelTitles(Document);
        bool Result = false;

        foreach (string LevelTitle in LevelTitles)
        {
            bool CanAddLevel = GetFolderCreationParent(SelectedItem, LevelTitle) != null;
            MenuItem LevelItem = new MenuItem
            {
                Header = LevelTitle,
                IsEnabled = CanAddLevel,
                Tag = LevelTitle
            };
            LevelItem.Click += async (Sender, Args) =>
            {
                if (Sender is MenuItem MenuItem && MenuItem.Tag is string SelectedLevelTitle)
                    await NewFolder(SelectedLevelTitle);
            };

            FolderItem.Items.Add(LevelItem);
            Result = Result || CanAddLevel;
        }

        return Result;
    }
    /// <summary>
    /// Returns the document context of an item.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <returns>The document context, if any; otherwise null.</returns>
    Document GetDocumentContext(BaseItem Item)
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
    List<string> GetStructureLevelTitles(Document Document)
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
    string GetExpectedChildFolderLevelTitle(BaseItem Item)
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
    bool CanAddFolderLevel(BaseItem Item, string LevelTitle)
    {
        if (!(Item is Document || Item is Folder) || Item.CanAddFolder == false)
            return false;

        string ExpectedLevelTitle = GetExpectedChildFolderLevelTitle(Item);
        return string.Equals(ExpectedLevelTitle, LevelTitle, StringComparison.OrdinalIgnoreCase);
    }
    /// <summary>
    /// Returns the parent item that should receive a new folder.
    /// </summary>
    /// <param name="Item">The selected item.</param>
    /// <param name="LevelTitle">The folder level title.</param>
    /// <returns>The folder creation parent, if any; otherwise null.</returns>
    BaseItem GetFolderCreationParent(BaseItem Item, string LevelTitle)
    {
        if (CanAddFolderLevel(Item, LevelTitle))
            return Item;

        if (Item is Folder Folder && CanAddFolderLevel(Folder.Parent, LevelTitle))
            return Folder.Parent;

        return null;
    }
    /// <summary>
    /// Returns the parent item that should receive a new text file.
    /// </summary>
    /// <param name="Item">The selected item.</param>
    /// <returns>The text creation parent, if any; otherwise null.</returns>
    BaseItem GetTextCreationParent(BaseItem Item)
    {
        if (Item?.CanAddTextFile == true)
            return Item;

        if (Item is TextFile TextFile && TextFile.Parent?.CanAddTextFile == true)
            return TextFile.Parent;

        return null;
    }

    // ● create commands
    /// <summary>
    /// Shows the item title dialog.
    /// </summary>
    /// <param name="Caption">The dialog caption.</param>
    /// <param name="Title">The primary title.</param>
    /// <param name="Title2">The secondary title.</param>
    /// <returns>The entered title data, if accepted; otherwise null.</returns>
    async Task<ItemTitleDialogData> ShowTitleDialog(string Caption, string Title, string Title2)
    {
        ItemTitleDialogData Data = new ItemTitleDialogData { Caption = Caption, Title = Title, Title2 = Title2 };
        DialogInfo Info = await DialogWindow.ShowModal<ItemTitleDialog>(Data, this);
        if (!Info.Result)
            return null;

        return Info.ResultData as ItemTitleDialogData;
    }
    /// <summary>
    /// Creates a new document.
    /// </summary>
    async Task NewDocument()
    {
        if (AppHost.CurrentProject == null)
        {
            await Tripous.Desktop.MessageBox.Info("No project is open.", this);
            return;
        }

        ItemTitleDialogData TitleData = await ShowTitleDialog("Document title", string.Empty, string.Empty);
        if (TitleData == null)
            return;

        string Title = TitleData.Title;
        DialogInfo Info = await DialogWindow.ShowModal<DocumentStructureDialog>(null, this);
        if (!Info.Result)
            return;

        try
        {
            AppHost.ShowPleaseWait("Creating document...", this.GetOwnerWindow());
            await Task.Yield();

            FolderItem Structure = Info.ResultData as FolderItem;
            Document Document = Structure == null
                ? AppHost.CurrentProject.AddDocument(Title)
                : AppHost.CurrentProject.AddDocument(Title, Structure);
            Document.Title2 = TitleData.Title2;
            Document.SaveMetadata();

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
    async Task NewFolder(string LevelTitle)
    {
        BaseItem SelectedItem = GetSelectedBaseItem();
        BaseItem ParentItem = GetFolderCreationParent(SelectedItem, LevelTitle);
        if (ParentItem == null)
            return;

        ItemTitleDialogData TitleData = await ShowTitleDialog($"{LevelTitle} title", string.Empty, string.Empty);
        if (TitleData == null)
            return;

        string Title = TitleData.Title;
        try
        {
            AppHost.ShowPleaseWait($"Creating {LevelTitle}...", this.GetOwnerWindow());
            await Task.Yield();

            Folder Folder = ParentItem.AddFolder(Title, LevelTitle);
            Folder.Title2 = TitleData.Title2;
            Folder.SaveMetadata();
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
    async Task NewText()
    {
        BaseItem SelectedItem = GetSelectedBaseItem();
        BaseItem ParentItem = GetTextCreationParent(SelectedItem);
        if (ParentItem == null)
            return;

        ItemTitleDialogData TitleData = await ShowTitleDialog("Text title", string.Empty, string.Empty);
        if (TitleData == null)
            return;

        string Title = TitleData.Title;
        try
        {
            AppHost.ShowPleaseWait("Creating text...", this.GetOwnerWindow());
            await Task.Yield();

            TextFile File = ParentItem.AddTextFile(Title);
            File.Title2 = TitleData.Title2;
            File.SaveMetadata();
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
    // ● context menu
    /// <summary>
    /// Creates a menu item.
    /// </summary>
    /// <param name="Header">The menu item header.</param>
    /// <param name="IsEnabled">True if the menu item is enabled.</param>
    /// <param name="Click">The click handler.</param>
    /// <returns>The created menu item.</returns>
    MenuItem CreateMenuItem(string Header, bool IsEnabled, EventHandler<RoutedEventArgs> Click)
    {
        MenuItem Result = new MenuItem
        {
            Header = Header,
            IsEnabled = IsEnabled
        };
        Result.Click += Click;
        return Result;
    }
    /// <summary>
    /// Adds new-folder menu items to a context menu.
    /// </summary>
    /// <param name="Menu">The context menu.</param>
    /// <param name="Item">The selected item.</param>
    void AddContextNewFolderMenuItems(ContextMenu Menu, BaseItem Item)
    {
        Document Document = GetDocumentContext(Item);
        foreach (string LevelTitle in GetStructureLevelTitles(Document))
        {
            if (GetFolderCreationParent(Item, LevelTitle) == null)
                continue;

            Menu.Items.Add(CreateMenuItem($"New {LevelTitle}", true, async (Sender, Args) => await NewFolder(LevelTitle)));
        }
    }
    /// <summary>
    /// Creates the tree context menu for an item.
    /// </summary>
    /// <param name="Item">The selected item.</param>
    /// <returns>The created context menu.</returns>
    ContextMenu CreateTreeContextMenu(BaseItem Item)
    {
        ContextMenu Result = new ContextMenu();

        if (Item is TextFile)
        {
            Result.Items.Add(CreateMenuItem("New TextFile", GetTextCreationParent(Item) != null, async (Sender, Args) => await NewText()));
        }
        else if (Item is Folder)
        {
            AddContextNewFolderMenuItems(Result, Item);
            if (GetTextCreationParent(Item) != null)
                Result.Items.Add(CreateMenuItem("New TextFile", true, async (Sender, Args) => await NewText()));
        }
        else if (Item is Document)
        {
            Result.Items.Add(CreateMenuItem("New Document", AppHost.CurrentProject?.CanAddDocument == true, async (Sender, Args) => await NewDocument()));
            AddContextNewFolderMenuItems(Result, Item);
            if (GetTextCreationParent(Item) != null)
                Result.Items.Add(CreateMenuItem("New TextFile", true, async (Sender, Args) => await NewText()));
        }

        Result.Items.Add(new Separator());
        Result.Items.Add(CreateMenuItem("Edit", Item.CanRename(), async (Sender, Args) => await EditSelectedItemInfo()));
        Result.Items.Add(CreateMenuItem("Delete", Item.CanDelete(), async (Sender, Args) => await DeleteSelectedItem()));
        Result.Items.Add(CreateMenuItem("Edit Text", true, (Sender, Args) => EditSelectedItem()));

        if (!(Item is Document))
            Result.Items.Add(CreateMenuItem("Change Parent", GetParentCandidates(Item).Count > 0, async (Sender, Args) => await ChangeSelectedItemParent()));

        Result.Items.Add(new Separator());
        Result.Items.Add(CreateMenuItem("Up", CanMoveSelectedItem(Item, true), async (Sender, Args) => await MoveSelectedItem(true)));
        Result.Items.Add(CreateMenuItem("Down", CanMoveSelectedItem(Item, false), async (Sender, Args) => await MoveSelectedItem(false)));

        if (Item is Document)
        {
            Result.Items.Add(new Separator());
            Result.Items.Add(CreateMenuItem("Export", true, async (Sender, Args) => await ExportDocument()));
        }

        return Result;
    }

    // ● export
    /// <summary>
    /// Returns the selected document context.
    /// </summary>
    /// <returns>The selected document, if any; otherwise null.</returns>
    Document GetSelectedDocument()
    {
        BaseItem Item = GetSelectedBaseItem();
        if (Item is Document Document)
            return Document;

        return Item?.Document;
    }
    /// <summary>
    /// Exports the selected document.
    /// </summary>
    async Task ExportDocument()
    {
        Document Document = GetSelectedDocument();
        if (Document == null)
        {
            await Tripous.Desktop.MessageBox.Info("Select a document item first.", this);
            return;
        }

        ExportOptions Options = new ExportOptions();
        DialogInfo Info = await DialogWindow.ShowModal<ExportDialog>(Options, this);
        if (!Info.Result)
            return;

        Options = Info.ResultData as ExportOptions ?? Options;

        try
        {
            AppHost.ShowPleaseWait("Exporting document...", this.GetOwnerWindow());
            await Task.Yield();

            string ExportFolderPath = new DocumentExporter(Document, Options).Execute();
            LogBox.AppendLine($"Document exported: {ExportFolderPath}");
            Sys.OpenFileExplorer(ExportFolderPath);
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

    // ● change parent
    /// <summary>
    /// Adds folder parent candidates recursively.
    /// </summary>
    /// <param name="Result">The candidate list.</param>
    /// <param name="Item">The item that changes parent.</param>
    /// <param name="Folders">The folders to check.</param>
    void AddFolderParentCandidates(List<BaseItem> Result, BaseItem Item, List<Folder> Folders)
    {
        foreach (Folder Folder in Folders)
        {
            if (Item.CanChangeParent(Folder))
                Result.Add(Folder);

            AddFolderParentCandidates(Result, Item, Folder.Folders);
        }
    }
    /// <summary>
    /// Returns the available change-parent candidates for an item.
    /// </summary>
    /// <param name="Item">The item that changes parent.</param>
    /// <returns>The available parent candidates.</returns>
    List<BaseItem> GetParentCandidates(BaseItem Item)
    {
        List<BaseItem> Result = new();
        Project Project = AppHost.CurrentProject;
        if (Project == null || Item == null)
            return Result;

        foreach (Document Document in Project.Documents)
        {
            if (Item.CanChangeParent(Document))
                Result.Add(Document);

            AddFolderParentCandidates(Result, Item, Document.Folders);
        }

        return Result;
    }
    /// <summary>
    /// Changes the selected item parent.
    /// </summary>
    async Task ChangeSelectedItemParent()
    {
        BaseItem Item = GetSelectedBaseItem();
        if (!(Item is Folder || Item is TextFile))
        {
            await Tripous.Desktop.MessageBox.Info("Select a folder or text file first.", this);
            return;
        }

        List<BaseItem> ParentList = GetParentCandidates(Item);
        if (ParentList.Count == 0)
        {
            await Tripous.Desktop.MessageBox.Info("No valid parent is available for the selected item.", this);
            return;
        }

        SelectParentDialogData Data = new SelectParentDialogData
        {
            Item = Item,
            ParentList = ParentList
        };

        DialogInfo Info = await DialogWindow.ShowModal<SelectParentDialog>(Data, this);
        if (!Info.Result)
            return;

        BaseItem TargetParent = Info.ResultData as BaseItem;
        if (TargetParent == null)
            return;

        try
        {
            AppHost.ShowPleaseWait("Changing parent...", this.GetOwnerWindow());
            await Task.Yield();

            if (!Item.ChangeParent(TargetParent))
                return;

            AppHost.NotifyItemTitleChanged(Item);
            CreateProjectTree();
            SelectTreeItem(Item);
            ShowSelectedItem(Item);
            LogBox.AppendLine($"Item parent changed: {Item.DisplayTitle}");
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

    // ● move commands
    /// <summary>
    /// Moves the selected item one step.
    /// </summary>
    /// <param name="Up">True to move up; false to move down.</param>
    async Task MoveSelectedItem(bool Up)
    {
        BaseItem Item = GetSelectedBaseItem();
        if (!CanMoveSelectedItem(Item, Up))
            return;

        try
        {
            AppHost.ShowPleaseWait("Moving item...", this.GetOwnerWindow());
            await Task.Yield();

            if (!MoveItemInParent(Item, Up))
                return;

            AppHost.NotifyItemTitleChanged(Item);
            CreateProjectTree();
            SelectTreeItem(Item);
            ShowSelectedItem(Item);
            LogBox.AppendLine($"Item moved: {Item.DisplayTitle}");
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
    /// Returns true if an item can move in the specified direction.
    /// </summary>
    /// <param name="Item">The item to check.</param>
    /// <param name="Up">True to move up; false to move down.</param>
    /// <returns>True if the item can move; otherwise false.</returns>
    bool CanMoveSelectedItem(BaseItem Item, bool Up)
    {
        return Item != null && Item.CanMove(Up);
    }
    /// <summary>
    /// Moves an item in the specified direction.
    /// </summary>
    /// <param name="Item">The item to move.</param>
    /// <param name="Up">True to move up; false to move down.</param>
    /// <returns>True if the item is moved; otherwise false.</returns>
    bool MoveItemInParent(BaseItem Item, bool Up)
    {
        return Item != null && Item.Move(Up);
    }

    // ● edit commands
    /// <summary>
    /// Edits the selected item information.
    /// </summary>
    async Task EditSelectedItemInfo()
    {
        BaseItem Item = GetSelectedBaseItem();
        if (Item == null)
            return;

        if (!Item.CanRename())
        {
            await Tripous.Desktop.MessageBox.Info("The selected item cannot be renamed.", this);
            return;
        }

        ItemTitleDialogData TitleData = await ShowTitleDialog("Title", Item.Title, Item.Title2);
        if (TitleData == null)
            return;

        string Title = TitleData.Title;
        string Title2 = TitleData.Title2;
        if (string.Equals(Title, Item.Title, StringComparison.Ordinal) && string.Equals(Title2, Item.Title2, StringComparison.Ordinal))
            return;

        try
        {
            AppHost.ShowPleaseWait("Renaming item...", this.GetOwnerWindow());
            await Task.Yield();

            if (!string.Equals(Title, Item.Title, StringComparison.Ordinal))
                Item.Rename(Title);

            Item.Title2 = Title2;
            Item.SaveMetadata();
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
    async Task DeleteSelectedItem()
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
            await Task.Yield();

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

    // ● selection
    /// <summary>
    /// Returns the currently selected base item.
    /// </summary>
    /// <returns>The selected base item, if any; otherwise null.</returns>
    BaseItem GetSelectedBaseItem()
    {
        if (tvProject.SelectedItem is TreeViewItem Node)
            return Node.Tag as BaseItem;

        return null;
    }
    /// <summary>
    /// Selects a base item in the project tree.
    /// </summary>
    /// <param name="Item">The base item.</param>
    void SelectTreeItem(BaseItem Item)
    {
        SelectTreeItem(tvProject, Item);
    }
    /// <summary>
    /// Selects a base item in a tree branch.
    /// </summary>
    /// <param name="ParentNode">The parent tree node.</param>
    /// <param name="Item">The base item.</param>
    /// <returns>True if the item is selected; otherwise false.</returns>
    bool SelectTreeItem(ItemsControl ParentNode, BaseItem Item)
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

    // ● open item
    /// <summary>
    /// Edits the selected item text in the content area.
    /// </summary>
    void EditSelectedItem()
    {
        BaseItem Item = GetSelectedBaseItem();
        EditItem(Item);
    }
    /// <summary>
    /// Edits an item text in the content area.
    /// </summary>
    /// <param name="Item">The item to edit.</param>
    void EditItem(BaseItem Item)
    {
        if (Item == null)
            return;

        AppHost.ShowContentForm<TextFileForm>(Item.Id, Item.DisplayTitle, Item);
    }
    /// <summary>
    /// Previews the selected item markdown text in the content area.
    /// </summary>
    void PreviewSelectedItem()
    {
        BaseItem Item = GetSelectedBaseItem();
        if (Item == null)
            return;

        string Title = $"HTML Preview: {Item.DisplayTitle}";
        string Text = string.Empty;

        if (Item is Document Document)
            Text = Document.Synopsis;
        else if (Item is Folder Folder)
            Text = Folder.Synopsis;
        else if (Item is TextFile File)
            Text = File.Text;

        AppHost.ShowMarkdownPreview($"{Item.Id}.HtmlPreview", Title, Text);
    }

    // ● tree
    /// <summary>
    /// Creates the project tree nodes.
    /// </summary>
    void CreateProjectTree()
    {
        tvProject.Items.Clear();

        Project Project = AppHost.CurrentProject;
        if (Project == null)
        {
            lblProjectTitle.Text = "No project open";
            RenderNoProjectMetrics();
            lblTextTitle.Text = "Text";
            Editor.EditorText = string.Empty;
            Editor.FilePath = string.Empty;
            UpdateMoveButtons();
            return;
        }

        lblProjectTitle.Text = Project.DisplayTitle;

        foreach (Document Document in Project.Documents)
            AddDocumentNode(tvProject, Document);

        RenderMetrics(Project);
        UpdateMoveButtons();
    }
    /// <summary>
    /// Adds a document node.
    /// </summary>
    /// <param name="ParentNode">The parent tree node.</param>
    /// <param name="Document">The document.</param>
    void AddDocumentNode(ItemsControl ParentNode, Document Document)
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
    void AddFolderNode(TreeViewItem ParentNode, Folder Folder)
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
    void AddTextFileNode(TreeViewItem ParentNode, TextFile File)
    {
        ParentNode.Items.Add(CreateNode(File.DisplayTitle, File));
    }
    /// <summary>
    /// Creates a folder tree node.
    /// </summary>
    /// <param name="Text">The node text.</param>
    /// <param name="Tag">The node tag.</param>
    /// <returns>The created tree node.</returns>
    TreeViewItem CreateFolderNode(string Text, object Tag)
    {
        TreeViewItem Result = Ui.CreateContainerNode(Text, Tag, IconFile: "folder.png", NegativeMargin: 10);
        if (Result.Header is StackPanel Panel)
        {
            foreach (Control Control in Panel.Children)
            {
                if (Control is TextBlock TextBlock)
                {
                    TextBlock.FontSize = 12;
                    TextBlock.FontWeight = FontWeight.Medium;
                }
            }
        }

        return Result;
    }
    /// <summary>
    /// Creates a tree node.
    /// </summary>
    /// <param name="Text">The node text.</param>
    /// <param name="Tag">The node tag.</param>
    /// <returns>The created tree node.</returns>
    TreeViewItem CreateNode(string Text, object Tag)
    {
        return Tag switch
        {
            Project => Ui.CreateContainerNode(Text, Tag, IconFile: "application_home.png", NegativeMargin: 10),
            Document => Ui.CreateContainerNode(Text, Tag, IconFile: "book.png", NegativeMargin: 10),
            Folder => CreateFolderNode(Text, Tag),
            TextFile => Ui.CreateLeafNode(Text, Tag, IconFile: "table.png", NegativeMargin: 10),
            _ => new TreeViewItem { Header = Text, Tag = Tag }
        };
    }

    // ● metrics
    /// <summary>
    /// Renders an empty metrics display.
    /// </summary>
    void RenderNoProjectMetrics()
    {
        MetricsPanel.Children.Clear();
        AddMetricText("No project open.", 12, FontWeight.Normal, Brushes.Black);
    }
    /// <summary>
    /// Renders the text metrics display.
    /// </summary>
    /// <param name="Project">The project.</param>
    void RenderMetrics(Project Project)
    {
        int DocumentCount = Project.Documents.Count;
        int FolderCount = Project.Documents.Sum(CountFolders);
        int TextFileCount = Project.Documents.Sum(CountTextFiles);
        TextStats ProjectStats = new TextStats();
        TextStats ProjectStats2 = new TextStats();

        foreach (Document Document in Project.Documents)
        {
            ProjectStats.Add(GetDocumentTextStats(Document, false));
            ProjectStats2.Add(GetDocumentTextStats(Document, true));
        }

        MetricsPanel.Children.Clear();
        AddMetricHeader(Project.Title, 16);
        AddMetricGroup("Project", Panel =>
        {
            AddMetricRow(Panel, "Documents", DocumentCount.ToString());
            AddMetricRow(Panel, "Folders", FolderCount.ToString());
            AddMetricRow(Panel, "Text files", TextFileCount.ToString());
            AddMetricRow(Panel, "Components", Project.Components.Count.ToString());
            AddMetricRow(Panel, "Notes", Project.Notes.Count.ToString());
        });

        AddStatsGroup("Total Text", ProjectStats);
        AddStatsGroup("Total Text2", ProjectStats2);

        foreach (Document Document in Project.Documents)
        {
            AddMetricHeader(Document.DisplayTitle, 14);
            AddStatsGroup("Text", GetDocumentTextStats(Document, false));
            AddStatsGroup("Text2", GetDocumentTextStats(Document, true));
        }
    }
    /// <summary>
    /// Adds metric text to the metrics panel.
    /// </summary>
    /// <param name="Text">The text.</param>
    /// <param name="FontSize">The font size.</param>
    /// <param name="FontWeight">The font weight.</param>
    /// <param name="Foreground">The foreground brush.</param>
    void AddMetricText(string Text, double FontSize, FontWeight FontWeight, IBrush Foreground)
    {
        MetricsPanel.Children.Add(new TextBlock
        {
            Text = Text,
            FontSize = FontSize,
            FontWeight = FontWeight,
            Foreground = Foreground,
            Margin = new Thickness(0, 2, 0, 0),
            TextWrapping = TextWrapping.Wrap
        });
    }
    /// <summary>
    /// Adds a metric header.
    /// </summary>
    /// <param name="Text">The header text.</param>
    /// <param name="FontSize">The font size.</param>
    void AddMetricHeader(string Text, double FontSize)
    {
        AddMetricText(Text, FontSize, FontWeight.Bold, new SolidColorBrush(Color.Parse("#8A4B16")));
    }
    /// <summary>
    /// Adds a metric group.
    /// </summary>
    /// <param name="Title">The group title.</param>
    /// <param name="LoadRows">The row loader.</param>
    void AddMetricGroup(string Title, Action<Grid> LoadRows)
    {
        Border Border = new Border();
        Border.BorderBrush = new SolidColorBrush(Color.Parse("#D8C5B4"));
        Border.BorderThickness = new Thickness(0, 1, 0, 0);
        Border.Padding = new Thickness(0, 4, 0, 0);

        Grid Grid = new Grid();
        Grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        Grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(74)));
        Grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        Grid.RowSpacing = 1;
        Grid.ColumnSpacing = 6;

        TextBlock Header = new TextBlock
        {
            Text = Title,
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            Foreground = Brushes.Black
        };
        Grid.SetColumnSpan(Header, 2);
        Grid.Children.Add(Header);

        LoadRows(Grid);
        Border.Child = Grid;
        MetricsPanel.Children.Add(Border);
    }
    /// <summary>
    /// Adds a metric row.
    /// </summary>
    /// <param name="Grid">The target grid.</param>
    /// <param name="Label">The label.</param>
    /// <param name="Value">The value.</param>
    void AddMetricRow(Grid Grid, string Label, string Value)
    {
        int RowIndex = Grid.RowDefinitions.Count;
        Grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        TextBlock LabelBlock = new TextBlock
        {
            Text = Label,
            FontSize = 12,
            Foreground = Brushes.Black,
            Opacity = 0.82,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        TextBlock ValueBlock = new TextBlock
        {
            Text = Value,
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            FontFamily = new FontFamily("Cascadia Code, Consolas, Monospace"),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            TextAlignment = TextAlignment.Right,
            TextWrapping = TextWrapping.NoWrap
        };

        Grid.SetRow(LabelBlock, RowIndex);
        Grid.SetColumn(LabelBlock, 0);
        Grid.SetRow(ValueBlock, RowIndex);
        Grid.SetColumn(ValueBlock, 1);
        Grid.Children.Add(LabelBlock);
        Grid.Children.Add(ValueBlock);
    }
    /// <summary>
    /// Adds a text stats group.
    /// </summary>
    /// <param name="Title">The group title.</param>
    /// <param name="Stats">The text stats.</param>
    void AddStatsGroup(string Title, TextStats Stats)
    {
        AddMetricGroup(Title, Panel =>
        {
            AddMetricRow(Panel, "Words", Stats.WordCount.ToString());
            AddMetricRow(Panel, "Pages", $"{Stats.EstimatedPages:0.00}");
            AddMetricRow(Panel, "Chars", Stats.CharCount.ToString());
            AddMetricRow(Panel, "Chars - spaces", Stats.CharCountNoSpaces.ToString());
            AddMetricRow(Panel, "Lines", Stats.LineCount.ToString());
            AddMetricRow(Panel, "Pars", Stats.ParagraphCount.ToString());
        });
    }
    /// <summary>
    /// Returns document text stats.
    /// </summary>
    /// <param name="Document">The document.</param>
    /// <param name="UseSecondText">True to use Text2; otherwise false.</param>
    /// <returns>The document text stats.</returns>
    TextStats GetDocumentTextStats(Document Document, bool UseSecondText)
    {
        TextStats Result = new TextStats();

        foreach (TextFile File in Document.Files)
            Result.Add(TextMetrics.Compute(UseSecondText ? File.Text2 : File.Text));

        foreach (Folder Folder in Document.Folders)
            Result.Add(GetFolderTextStats(Folder, UseSecondText));

        return Result;
    }
    /// <summary>
    /// Returns folder text stats.
    /// </summary>
    /// <param name="Folder">The folder.</param>
    /// <param name="UseSecondText">True to use Text2; otherwise false.</param>
    /// <returns>The folder text stats.</returns>
    TextStats GetFolderTextStats(Folder Folder, bool UseSecondText)
    {
        TextStats Result = new TextStats();

        foreach (TextFile File in Folder.Files)
            Result.Add(TextMetrics.Compute(UseSecondText ? File.Text2 : File.Text));

        foreach (Folder ChildFolder in Folder.Folders)
            Result.Add(GetFolderTextStats(ChildFolder, UseSecondText));

        return Result;
    }
    /// <summary>
    /// Counts all folders in a document.
    /// </summary>
    /// <param name="Document">The document.</param>
    /// <returns>The folder count.</returns>
    int CountFolders(Document Document)
    {
        return Document.Folders.Sum(CountFolders);
    }
    /// <summary>
    /// Counts all folders in a folder.
    /// </summary>
    /// <param name="Folder">The folder.</param>
    /// <returns>The folder count.</returns>
    int CountFolders(Folder Folder)
    {
        return 1 + Folder.Folders.Sum(CountFolders);
    }
    /// <summary>
    /// Counts all text files in a document.
    /// </summary>
    /// <param name="Document">The document.</param>
    /// <returns>The text file count.</returns>
    int CountTextFiles(Document Document)
    {
        return Document.Files.Count + Document.Folders.Sum(CountTextFiles);
    }
    /// <summary>
    /// Counts all text files in a folder.
    /// </summary>
    /// <param name="Folder">The folder.</param>
    /// <returns>The text file count.</returns>
    int CountTextFiles(Folder Folder)
    {
        return Folder.Files.Count + Folder.Folders.Sum(CountTextFiles);
    }

    // ● tree events
    /// <summary>
    /// Handles selected tree item changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void TreeSelectionChanged(object Sender, SelectionChangedEventArgs Args)
    {
        if (tvProject.SelectedItem is TreeViewItem Node)
            ShowSelectedItem(Node.Tag as BaseItem);
        else
            ShowNoSelectedItem();

        UpdateMoveButtons();
    }
    /// <summary>
    /// Handles tree pointer press events.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void TreePointerPressed(object Sender, PointerPressedEventArgs Args)
    {
        if (!Args.GetCurrentPoint(tvProject).Properties.IsRightButtonPressed)
            return;

        TreeViewItem Node = GetTreeNode(Args.Source);
        BaseItem Item = Node?.Tag as BaseItem;
        if (Item == null)
            return;

        Node.IsSelected = true;
        Node.ContextMenu = CreateTreeContextMenu(Item);
        Node.ContextMenu.Open(Node);
        Args.Handled = true;
    }
    /// <summary>
    /// Handles tree double-tap events.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void TreeDoubleTapped(object Sender, TappedEventArgs Args)
    {
        TreeViewItem Node = GetTreeNode(Args.Source);
        TextFile File = Node?.Tag as TextFile;
        if (File == null)
            return;

        Node.IsSelected = true;
        EditItem(File);
        Args.Handled = true;
    }
    /// <summary>
    /// Returns the tree node that raised an input event.
    /// </summary>
    /// <param name="Source">The input event source.</param>
    /// <returns>The source tree node, if any; otherwise null.</returns>
    TreeViewItem GetTreeNode(object Source)
    {
        if (Source is TreeViewItem Node)
            return Node;

        if (Source is Avalonia.Visual Visual)
            return Visual.FindAncestorOfType<TreeViewItem>();

        return null;
    }

    // ● preview
    /// <summary>
    /// Shows selected item information.
    /// </summary>
    /// <param name="Item">The selected item.</param>
    void ShowSelectedItem(BaseItem Item)
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
    void ShowNoSelectedItem()
    {
        lblTextTitle.Text = "Text";
        Editor.EditorText = string.Empty;
        Editor.FilePath = string.Empty;
    }

    // ● overrides
    /// <summary>
    /// Called in order to initialize the form.
    /// </summary>
    protected override void FormInitialize()
    {
        TitleText = "Documents";
        ClosableByUser = false;
        CreateProjectTree();
        CreateToolBar();
        tvProject.SelectionChanged += TreeSelectionChanged;
        tvProject.DoubleTapped += TreeDoubleTapped;
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

    // ● public
    /// <summary>
    /// Selects and shows an item in the document tree.
    /// </summary>
    /// <param name="Item">The item.</param>
    public void ShowItemInList(BaseItem Item)
    {
        if (Item == null)
            return;

        SelectTreeItem(Item);
        ShowSelectedItem(Item);
    }
}
