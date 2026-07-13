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
    /// <summary>
    /// True while list controls are being reloaded.
    /// </summary>
    bool fLoading;

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
    }

    // ● loading
    /// <summary>
    /// Reloads the category list.
    /// </summary>
    void LoadComponents()
    {
        string SelectedCategoryText = SelectedCategory;
        string SelectedComponentId = SelectedComponent?.Id;
        LoadComponents(SelectedCategoryText, SelectedComponentId);
    }
    /// <summary>
    /// Reloads the category list.
    /// </summary>
    /// <param name="SelectedCategoryText">The category text to select after loading.</param>
    /// <param name="SelectedComponentId">The component id to select after loading.</param>
    void LoadComponents(string SelectedCategoryText, string SelectedComponentId)
    {

        fLoading = true;
        try
        {
            lboCategories.Items.Clear();

            Project Project = AppHost.CurrentProject;
            if (Project != null)
            {
                foreach (string Category in GetFilteredCategories(Project))
                    lboCategories.Items.Add(CreateCategoryItem(Category));
            }

            SelectCategory(SelectedCategoryText);

            if (lboCategories.SelectedItem == null && lboCategories.Items.Count > 0)
                lboCategories.SelectedIndex = 0;
        }
        finally
        {
            fLoading = false;
        }

        LoadCategoryComponents(SelectedComponentId);
    }
    /// <summary>
    /// Reloads the component list for the selected category.
    /// </summary>
    /// <param name="SelectedComponentId">The component id to select after loading.</param>
    void LoadCategoryComponents(string SelectedComponentId = null)
    {
        SelectedComponentId = SelectedComponentId ?? SelectedComponent?.Id;

        fLoading = true;
        try
        {
            lboComponents.Items.Clear();

            Project Project = AppHost.CurrentProject;
            string Category = SelectedCategory;
            if (Project != null && Category != null)
            {
                foreach (Component Component in GetCategoryComponents(Project, Category))
                    lboComponents.Items.Add(CreateComponentItem(Component));
            }

            SelectComponent(SelectedComponentId);

            if (lboComponents.SelectedItem == null && lboComponents.Items.Count > 0)
                lboComponents.SelectedIndex = 0;
        }
        finally
        {
            fLoading = false;
        }

        ShowComponent(SelectedComponent);
    }
    /// <summary>
    /// Returns the filtered project category list.
    /// </summary>
    /// <param name="Project">The project.</param>
    /// <returns>The filtered category list.</returns>
    List<string> GetFilteredCategories(Project Project)
    {
        string Filter = edtFilter.Text?.Trim() ?? string.Empty;
        List<Component> Components = Project.GetComponentList()
            .Where(Component => ComponentMatchesFilter(Component, Filter))
            .ToList();

        return Components
            .Select(Component => Component.Category ?? string.Empty)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(Category => Category)
            .ToList();
    }
    /// <summary>
    /// Returns the components assigned to a category.
    /// </summary>
    /// <param name="Project">The project.</param>
    /// <param name="Category">The category.</param>
    /// <returns>The component list.</returns>
    List<Component> GetCategoryComponents(Project Project, string Category)
    {
        string Filter = edtFilter.Text?.Trim() ?? string.Empty;
        return Project.GetComponentList()
            .Where(Component => (Component.Category ?? string.Empty).IsSameText(Category))
            .Where(Component => ComponentMatchesFilter(Component, Filter))
            .OrderBy(Component => Component.Title)
            .ToList();
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
    /// Creates a category list item.
    /// </summary>
    /// <param name="Category">The category.</param>
    /// <returns>The list item.</returns>
    ListBoxItem CreateCategoryItem(string Category)
    {
        return new ListBoxItem
        {
            Content = string.IsNullOrWhiteSpace(Category) ? "No Category" : Category,
            Tag = Category ?? string.Empty
        };
    }
    /// <summary>
    /// Creates a component list item.
    /// </summary>
    /// <param name="Component">The component.</param>
    /// <returns>The list item.</returns>
    ListBoxItem CreateComponentItem(Component Component)
    {
        return new ListBoxItem
        {
            Content = Component.Title,
            Tag = Component
        };
    }
    /// <summary>
    /// Selects a category by text.
    /// </summary>
    /// <param name="Category">The category text.</param>
    void SelectCategory(string Category)
    {
        if (Category == null)
            return;

        foreach (object Item in lboCategories.Items)
        {
            if (Item is ListBoxItem ListItem && ListItem.Tag is string Text && Text.IsSameText(Category))
            {
                lboCategories.SelectedItem = ListItem;
                return;
            }
        }
    }
    /// <summary>
    /// Selects a component by id.
    /// </summary>
    /// <param name="Id">The component id.</param>
    void SelectComponent(string Id)
    {
        if (string.IsNullOrWhiteSpace(Id))
            return;

        foreach (object Item in lboComponents.Items)
        {
            if (Item is ListBoxItem ListItem && ListItem.Tag is Component Component && Component.Id.IsSameText(Id))
            {
                lboComponents.SelectedItem = ListItem;
                return;
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

        lblComponentTitle.Text = Component.Title;
        SetMarkdownPreview(Component.Text);

        ReloadList(lboTags, Component.TagList);
        ReloadList(lboAliases, Component.AliasList);
    }
    /// <summary>
    /// Clears the component preview area.
    /// </summary>
    void ShowNoSelectedComponent()
    {
        lblComponentTitle.Text = "Component";
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
            LoadComponents(Component.Category ?? string.Empty, Component.Id);
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

            LoadComponents(Component.Category ?? string.Empty, Component.Id);
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
    /// Handles category selection changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void CategoriesSelectionChanged(object Sender, SelectionChangedEventArgs Args)
    {
        if (!fLoading)
            LoadCategoryComponents();
    }
    /// <summary>
    /// Handles component selection changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void ComponentsSelectionChanged(object Sender, SelectionChangedEventArgs Args)
    {
        if (!fLoading)
            ShowComponent(SelectedComponent);
    }
    /// <summary>
    /// Handles component double-tap events.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void ComponentsDoubleTapped(object Sender, TappedEventArgs Args)
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
    /// Gets the selected category.
    /// </summary>
    string SelectedCategory
    {
        get
        {
            if (lboCategories.SelectedItem is ListBoxItem Item)
                return Item.Tag as string;

            return null;
        }
    }
    /// <summary>
    /// Gets the selected component.
    /// </summary>
    Component SelectedComponent
    {
        get
        {
            if (lboComponents.SelectedItem is ListBoxItem Item)
                return Item.Tag as Component;

            return null;
        }
    }
}
