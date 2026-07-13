// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Forms;

/// <summary>
/// Displays project components grouped by category.
/// </summary>
public partial class ComponentListForm: AppForm
{
    // ● private fields
    /// <summary>
    /// The toolbar helper.
    /// </summary>
    Tripous.Desktop.ToolBar fToolBar;

    // ● toolbar
    /// <summary>
    /// Creates the form toolbar.
    /// </summary>
    void CreateToolBar()
    {
        fToolBar = new();
        fToolBar.Panel = pnlToolBar;

        fToolBar.AddButton("table_add.png", "New", async () => await NewComponent());
        fToolBar.AddButton("table_edit.png", "Edit", async () => await EditComponentInfo());
        fToolBar.AddButton("table_delete.png", "Delete", async () => await DeleteComponent());
        fToolBar.AddSeparator();
        fToolBar.AddButton("page_edit.png", "Edit Text", EditComponentText);
        fToolBar.AddButton("html.png", "HTML Preview", PreviewComponentText);
        fToolBar.AddButton("wishlist_add.png", "Quick View", QuickViewComponent);
        fToolBar.AddSeparator();
        fToolBar.AddButton("arrow_out.png", "Expand All", ExpandAll);
        fToolBar.AddButton("arrow_in.png", "Collapse All", CollapseAll);
    }

    // ● private
    /// <summary>
    /// Reloads the component tree.
    /// </summary>
    void LoadComponents()
    {
        string SelectedId = SelectedComponent?.Id;
        tvComponents.Items.Clear();

        Project Project = AppHost.CurrentProject;
        if (Project != null)
        {
            string Filter = edtFilter.Text?.Trim() ?? string.Empty;
            List<Component> Components = Project.GetComponentList()
                .Where(Item => ComponentMatchesFilter(Item, Filter))
                .ToList();

            foreach (string Category in Components.Select(Item => Item.Category).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(Item => Item))
            {
                TreeViewItem CategoryNode = Ui.CreateContainerNode(Category, Category, IconFile: "folder.png", NegativeMargin: 10);
                tvComponents.Items.Add(CategoryNode);

                foreach (Component Component in Components.Where(Item => Item.Category.IsSameText(Category)).OrderBy(Item => Item.Title))
                    CategoryNode.Items.Add(Ui.CreateLeafNode(Component.Title, Component, IconFile: "table.png", NegativeMargin: 10));

                CategoryNode.IsExpanded = true;
            }
        }

        SelectComponent(SelectedId);

        if (tvComponents.SelectedItem == null)
            ShowNoSelectedComponent();
    }
    /// <summary>
    /// Returns true if a component matches the filter text.
    /// </summary>
    /// <param name="Component">The component.</param>
    /// <param name="Filter">The filter text.</param>
    /// <returns>True if the component matches; otherwise false.</returns>
    bool ComponentMatchesFilter(Component Component, string Filter)
    {
        if (string.IsNullOrWhiteSpace(Filter))
            return true;

        return Component.Title.ContainsText(Filter)
            || Component.Category.ContainsText(Filter)
            || Component.Tags.ContainsText(Filter)
            || Component.Aliases.ContainsText(Filter);
    }
    /// <summary>
    /// Selects a component by id.
    /// </summary>
    /// <param name="Id">The component id.</param>
    void SelectComponent(string Id)
    {
        if (string.IsNullOrWhiteSpace(Id))
            return;

        foreach (object CategoryObject in tvComponents.Items)
        {
            if (CategoryObject is TreeViewItem CategoryNode)
            {
                foreach (object ComponentObject in CategoryNode.Items)
                {
                    if (ComponentObject is TreeViewItem ComponentNode && ComponentNode.Tag is Component Component && Component.Id.IsSameText(Id))
                    {
                        CategoryNode.IsExpanded = true;
                        ComponentNode.IsSelected = true;
                        return;
                    }
                }
            }
        }
    }
    /// <summary>
    /// Shows a component in the preview area.
    /// </summary>
    /// <param name="Component">The component.</param>
    void ShowComponent(Component Component)
    {
        if (Component == null)
        {
            ShowNoSelectedComponent();
            return;
        }

        SetMarkdownPreview(Component.Text);

        ReloadList(lboTags, Component.TagList);
        ReloadList(lboAliases, Component.AliasList);
    }
    /// <summary>
    /// Clears the component preview area.
    /// </summary>
    void ShowNoSelectedComponent()
    {
        ClearMarkdownPreview();
        lboTags.Items.Clear();
        lboAliases.Items.Clear();
    }
    /// <summary>
    /// Clears the markdown preview.
    /// </summary>
    void ClearMarkdownPreview()
    {
        SetMarkdownPreview(string.Empty);
    }
    /// <summary>
    /// Sets the markdown preview text.
    /// </summary>
    /// <param name="MarkdownText">The markdown text.</param>
    void SetMarkdownPreview(string MarkdownText)
    {
        MarkdownPreviewRenderer.Render(MarkdownPreviewPanel, MarkdownText);
    }
    /// <summary>
    /// Reloads a list box.
    /// </summary>
    /// <param name="Box">The list box.</param>
    /// <param name="List">The source list.</param>
    void ReloadList(ListBox Box, List<string> List)
    {
        Box.Items.Clear();
        foreach (string Item in List.OrderBy(Item => Item))
            Box.Items.Add(Item);
    }
    /// <summary>
    /// Creates a default new component.
    /// </summary>
    /// <returns>The new component.</returns>
    Component CreateDefaultComponent()
    {
        Component Result = new Component();
        Result.Title = "New Component";
        Result.Category = "No Category";
        return Result;
    }
    /// <summary>
    /// Creates a metadata copy of a component.
    /// </summary>
    /// <param name="Source">The source component.</param>
    /// <returns>The copied component.</returns>
    Component CopyComponentMetadata(Component Source)
    {
        Component Result = new Component();
        Result.Title = Source.Title;
        Result.Category = Source.Category;
        Result.Aliases = Source.Aliases;
        Result.Tags = Source.Tags;
        return Result;
    }
    /// <summary>
    /// Creates a new component.
    /// </summary>
    async Task NewComponent()
    {
        Project Project = AppHost.CurrentProject;
        if (Project == null)
        {
            await Tripous.Desktop.MessageBox.Info("No project is open.", this);
            return;
        }

        Component Component = CreateDefaultComponent();
        ComponentEditDialogData Data = new ComponentEditDialogData { Component = Component, IsInsert = true, OriginalTitle = string.Empty };
        DialogInfo Info = await DialogWindow.ShowModal<ComponentEditDialog>(Data, this);
        if (!Info.Result)
            return;

        try
        {
            Project.AddComponent(Component);
            LoadComponents();
            SelectComponent(Component.Id);
            ShowComponent(Component);
            LogBox.AppendLine($"Component created: {Component.Title}");
        }
        catch (Exception e)
        {
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
    }
    /// <summary>
    /// Edits the selected component metadata.
    /// </summary>
    async Task EditComponentInfo()
    {
        Component Component = SelectedComponent;
        if (Component == null)
            return;

        Component EditedComponent = CopyComponentMetadata(Component);
        ComponentEditDialogData Data = new ComponentEditDialogData { Component = EditedComponent, IsInsert = false, OriginalTitle = Component.Title };
        DialogInfo Info = await DialogWindow.ShowModal<ComponentEditDialog>(Data, this);
        if (!Info.Result)
            return;

        try
        {
            if (!Component.Title.IsSameText(EditedComponent.Title))
                Component.Rename(EditedComponent.Title);

            Component.Category = EditedComponent.Category;
            Component.Tags = EditedComponent.Tags;
            Component.Aliases = EditedComponent.Aliases;
            Component.Save();

            LoadComponents();
            SelectComponent(Component.Id);
            ShowComponent(Component);
            RefreshOpenComponentForm(Component);
            LogBox.AppendLine($"Component updated: {Component.Title}");
        }
        catch (Exception e)
        {
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
    }
    /// <summary>
    /// Deletes the selected component.
    /// </summary>
    async Task DeleteComponent()
    {
        Component Component = SelectedComponent;
        if (Component == null)
            return;

        bool Confirmed = await Tripous.Desktop.MessageBox.YesNo($"Delete {Component.Title}?", this);
        if (!Confirmed)
            return;

        try
        {
            string Title = Component.Title;
            AppHost.CloseContentFormForItem(Component);
            Component.Delete();
            LoadComponents();
            LogBox.AppendLine($"Component deleted: {Title}");
        }
        catch (Exception e)
        {
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
    }
    /// <summary>
    /// Opens the selected component text for editing.
    /// </summary>
    void EditComponentText()
    {
        Component Component = SelectedComponent;
        if (Component == null)
            return;

        AppHost.ShowContentForm<ComponentForm>(Component.Id, Component.Title, Component);
    }
    /// <summary>
    /// Previews the selected component markdown text.
    /// </summary>
    void PreviewComponentText()
    {
        Component Component = SelectedComponent;
        if (Component == null)
            return;

        AppHost.ShowMarkdownPreview($"{Component.Id}.HtmlPreview", $"HTML Preview: {Component.Title}", Component.Text);
    }
    /// <summary>
    /// Placeholder for Quick View command.
    /// </summary>
    void QuickViewComponent()
    {
        LogBox.AppendLine("Quick View command not implemented yet.");
    }
    /// <summary>
    /// Expands all component tree nodes.
    /// </summary>
    void ExpandAll()
    {
        tvComponents.ExpandAll(true);
    }
    /// <summary>
    /// Collapses all component tree nodes.
    /// </summary>
    void CollapseAll()
    {
        tvComponents.ExpandAll(false);
    }
    /// <summary>
    /// Refreshes an open component editor form.
    /// </summary>
    /// <param name="Component">The component.</param>
    void RefreshOpenComponentForm(Component Component)
    {
        if (Component == null || AppHost.ContentHandler == null)
            return;

        ComponentForm Form = AppHost.ContentHandler.FindAppForm(Component.Id) as ComponentForm;
        Form?.RefreshComponentInfo();
    }
    /// <summary>
    /// Handles filter text changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void FilterTextChanged(object Sender, TextChangedEventArgs Args)
    {
        LoadComponents();
    }
    /// <summary>
    /// Handles selected tree item changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void TreeSelectionChanged(object Sender, SelectionChangedEventArgs Args)
    {
        ShowComponent(SelectedComponent);
    }
    /// <summary>
    /// Handles tree double-tap events.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void TreeDoubleTapped(object Sender, TappedEventArgs Args)
    {
        if (SelectedComponent == null)
            return;

        EditComponentText();
        Args.Handled = true;
    }

    // ● overrides
    /// <summary>
    /// Called in order to initialize the form.
    /// </summary>
    protected override void FormInitialize()
    {
        TitleText = "Components";
        ClosableByUser = false;
        CreateToolBar();
        LoadComponents();
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the ComponentListForm class.
    /// </summary>
    public ComponentListForm()
    {
        InitializeComponent();
    }

    // ● public
    /// <summary>
    /// Refreshes the component list.
    /// </summary>
    public void RefreshComponents()
    {
        LoadComponents();
    }

    // ● properties
    /// <summary>
    /// Gets the selected component.
    /// </summary>
    Component SelectedComponent
    {
        get
        {
            if (tvComponents.SelectedItem is TreeViewItem Item)
                return Item.Tag as Component;

            return null;
        }
    }
}
