// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Forms;

/// <summary>
/// Displays the project quick-view list.
/// </summary>
public partial class QuickViewForm: AppForm
{
    // ● private fields
    /// <summary>
    /// The toolbar helper.
    /// </summary>
    Tripous.Desktop.ToolBar fToolBar;
    /// <summary>
    /// True while the list is being reloaded.
    /// </summary>
    bool fLoading;

    // ● toolbar
    /// <summary>
    /// Creates the toolbar.
    /// </summary>
    void CreateToolBar()
    {
        fToolBar = new();
        fToolBar.Panel = pnlToolBar;

        fToolBar.AddButton("table_delete.png", "Remove Item", RemoveSelectedItem);
        fToolBar.AddButton("shape_square_delete.png", "Remove All", async () => await RemoveAllItems());
        fToolBar.AddButton("page_edit.png", "Edit Text", EditSelectedItemText);
        fToolBar.AddSeparator();
        fToolBar.AddButton("table_select_row.png", "Show item in its List Page", ShowSelectedItemInListPage);
        fToolBar.AddSeparator();
        fToolBar.AddButton("arrow_up.png", "Up", () => MoveSelectedItem(true));
        fToolBar.AddButton("arrow_down.png", "Down", () => MoveSelectedItem(false));
    }

    // ● loading
    /// <summary>
    /// Reloads the quick-view list.
    /// </summary>
    void LoadItems()
    {
        string SelectedId = SelectedLinkItem?.Id;
        fLoading = true;
        try
        {
            lboItems.Items.Clear();
            Project Project = AppHost.CurrentProject;
            if (Project != null)
            {
                foreach (LinkItem Item in Project.QuickView.List.List)
                    lboItems.Items.Add(CreateListItem(Item));
            }

            SelectItem(SelectedId);
            if (lboItems.SelectedItem == null && lboItems.Items.Count > 0)
                lboItems.SelectedIndex = 0;
        }
        finally
        {
            fLoading = false;
        }

        ShowLinkItem(SelectedLinkItem);
    }
    /// <summary>
    /// Creates a list item.
    /// </summary>
    /// <param name="LinkItem">The link item.</param>
    /// <returns>The created list item.</returns>
    ListBoxItem CreateListItem(LinkItem LinkItem)
    {
        Grid Grid = new Grid();
        Grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(90)));
        Grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

        Grid.Children.Add(CreateCell(LinkItem.ItemType.ToString(), 0));
        Grid.Children.Add(CreateCell(LinkItem.Title, 1));

        return new ListBoxItem
        {
            Content = Grid,
            Tag = LinkItem
        };
    }
    /// <summary>
    /// Creates a list cell.
    /// </summary>
    /// <param name="Text">The cell text.</param>
    /// <param name="Column">The cell column.</param>
    /// <returns>The created cell.</returns>
    TextBlock CreateCell(string Text, int Column)
    {
        TextBlock Result = new TextBlock();
        Result.Text = Text ?? string.Empty;
        Result.TextTrimming = TextTrimming.CharacterEllipsis;
        Result.Margin = new Thickness(6, 2);
        Grid.SetColumn(Result, Column);
        return Result;
    }
    /// <summary>
    /// Selects a link item by id.
    /// </summary>
    /// <param name="Id">The linked item id.</param>
    void SelectItem(string Id)
    {
        if (string.IsNullOrWhiteSpace(Id))
            return;

        foreach (object Item in lboItems.Items)
        {
            if (Item is ListBoxItem ListItem && ListItem.Tag is LinkItem LinkItem && LinkItem.Id.IsSameText(Id))
            {
                lboItems.SelectedItem = ListItem;
                return;
            }
        }
    }

    // ● commands
    /// <summary>
    /// Adds a link item to QuickView.
    /// </summary>
    /// <param name="LinkItem">The link item.</param>
    public void AddToQuickView(LinkItem LinkItem)
    {
        Project Project = AppHost.CurrentProject;
        if (Project == null || LinkItem == null || LinkItem.Item == null)
            return;

        if (Project.QuickView.Add(LinkItem, Project))
        {
            LoadItems();
            SelectItem(LinkItem.Id);
            LogBox.AppendLine($"Item added to Quick View: {LinkItem.Title}");
        }
        else
        {
            LogBox.AppendLine($"Item is already in Quick View: {LinkItem.Title}");
        }
    }
    /// <summary>
    /// Removes the selected item.
    /// </summary>
    void RemoveSelectedItem()
    {
        Project Project = AppHost.CurrentProject;
        LinkItem LinkItem = SelectedLinkItem;
        if (Project == null || LinkItem == null)
            return;

        Project.QuickView.Remove(LinkItem, Project);
        LoadItems();
        LogBox.AppendLine($"Item removed from Quick View: {LinkItem.Title}");
    }
    /// <summary>
    /// Removes all items.
    /// </summary>
    async Task RemoveAllItems()
    {
        Project Project = AppHost.CurrentProject;
        if (Project == null || Project.QuickView.Count == 0)
            return;

        bool Confirmed = await Tripous.Desktop.MessageBox.YesNo("Remove all items from Quick View?", this);
        if (!Confirmed)
            return;

        Project.QuickView.Clear(Project);
        LoadItems();
        LogBox.AppendLine("Quick View cleared.");
    }
    /// <summary>
    /// Opens the selected item editor.
    /// </summary>
    void EditSelectedItemText()
    {
        LinkItem LinkItem = SelectedLinkItem;
        if (LinkItem != null)
            AppHost.ShowLinkItemPage(LinkItem);
    }
    /// <summary>
    /// Shows the selected item in its list page.
    /// </summary>
    void ShowSelectedItemInListPage()
    {
        LinkItem LinkItem = SelectedLinkItem;
        if (LinkItem != null)
            AppHost.ShowItemInListPage(LinkItem);
    }
    /// <summary>
    /// Moves the selected item.
    /// </summary>
    /// <param name="Up">True to move up; false to move down.</param>
    void MoveSelectedItem(bool Up)
    {
        Project Project = AppHost.CurrentProject;
        LinkItem LinkItem = SelectedLinkItem;
        if (Project == null || LinkItem == null)
            return;

        if (Project.QuickView.Move(LinkItem, Up, Project))
        {
            LoadItems();
            SelectItem(LinkItem.Id);
        }
    }

    // ● preview
    /// <summary>
    /// Shows a link item preview.
    /// </summary>
    /// <param name="LinkItem">The link item.</param>
    void ShowLinkItem(LinkItem LinkItem)
    {
        if (LinkItem == null)
        {
            lblTitle.Text = "No selection";
            Editor.EditorText = string.Empty;
            return;
        }

        lblTitle.Text = $"{LinkItem.ItemType} - {LinkItem.Title} - {LinkItem.Place}";
        Editor.EditorText = AppHost.GetLinkItemText(LinkItem);
    }

    // ● events
    /// <summary>
    /// Handles selected item changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void ItemsSelectionChanged(object Sender, SelectionChangedEventArgs Args)
    {
        if (!fLoading)
            ShowLinkItem(SelectedLinkItem);
    }
    /// <summary>
    /// Handles item double-tap.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void ItemsDoubleTapped(object Sender, TappedEventArgs Args)
    {
        EditSelectedItemText();
        Args.Handled = true;
    }

    // ● overrides
    /// <summary>
    /// Called in order to initialize the form.
    /// </summary>
    protected override void FormInitialize()
    {
        TitleText = "Quick View";
        ClosableByUser = false;
        CreateToolBar();
        Editor.ToolBarVisible = false;
        Editor.ReadOnly = true;
        LoadItems();
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the QuickViewForm class.
    /// </summary>
    public QuickViewForm()
    {
        InitializeComponent();
    }

    // ● properties
    /// <summary>
    /// Gets the selected link item.
    /// </summary>
    LinkItem SelectedLinkItem
    {
        get
        {
            if (lboItems.SelectedItem is ListBoxItem Item)
                return Item.Tag as LinkItem;

            return null;
        }
    }
}
