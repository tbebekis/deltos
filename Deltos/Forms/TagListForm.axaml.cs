// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Forms;

/// <summary>
/// Displays project tags and the components assigned to each tag.
/// </summary>
public partial class TagListForm: AppForm
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

        fToolBar.AddButton("table_delete.png", "Delete", async () => await DeleteTag());
        fToolBar.AddSeparator();
        fToolBar.AddButton("page_edit.png", "Edit Component Text", EditComponentText);
        fToolBar.AddButton("wishlist_add.png", "Quick View", QuickViewComponent);
    }

    // ● loading
    /// <summary>
    /// Reloads the tag list.
    /// </summary>
    void LoadTags()
    {
        string SelectedTagText = SelectedTag;

        fLoading = true;
        try
        {
            lboTags.Items.Clear();

            Project Project = AppHost.CurrentProject;
            if (Project != null)
            {
                foreach (string Tag in GetFilteredTags(Project))
                    lboTags.Items.Add(Tag);
            }

            SelectTag(SelectedTagText);

            if (lboTags.SelectedItem == null && lboTags.Items.Count > 0)
                lboTags.SelectedIndex = 0;
        }
        finally
        {
            fLoading = false;
        }

        LoadComponents();
    }
    /// <summary>
    /// Reloads the components for the selected tag.
    /// </summary>
    void LoadComponents()
    {
        string SelectedComponentId = SelectedComponent?.Id;

        fLoading = true;
        try
        {
            lboComponents.Items.Clear();

            Project Project = AppHost.CurrentProject;
            string Tag = SelectedTag;
            if (Project != null && !string.IsNullOrWhiteSpace(Tag))
            {
                foreach (Component Component in GetTaggedComponents(Project, Tag))
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
    /// Returns the filtered project tag list.
    /// </summary>
    /// <param name="Project">The project.</param>
    /// <returns>The filtered tag list.</returns>
    List<string> GetFilteredTags(Project Project)
    {
        string Filter = edtFilter.Text?.Trim() ?? string.Empty;
        List<string> Tags = Project.GetTagList();
        if (string.IsNullOrWhiteSpace(Filter))
            return Tags;

        return Tags
            .Where(Tag => Tag.ContainsText(Filter) || GetTaggedComponents(Project, Tag).Any(Component => ComponentMatchesFilter(Component, Filter)))
            .ToList();
    }
    /// <summary>
    /// Returns the components assigned to a tag.
    /// </summary>
    /// <param name="Project">The project.</param>
    /// <param name="Tag">The tag.</param>
    /// <returns>The component list.</returns>
    List<Component> GetTaggedComponents(Project Project, string Tag)
    {
        string Filter = edtFilter.Text?.Trim() ?? string.Empty;
        return Project.GetComponentList()
            .Where(Component => Component.ContainsTag(Tag))
            .Where(Component => ComponentMatchesFilter(Component, Filter))
            .OrderBy(Component => Component.Category)
            .ThenBy(Component => Component.Title)
            .ToList();
    }
    /// <summary>
    /// Returns true if a component matches the filter.
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
    /// Creates a component list item.
    /// </summary>
    /// <param name="Component">The component.</param>
    /// <returns>The created list item.</returns>
    ListBoxItem CreateComponentItem(Component Component)
    {
        string Text = string.IsNullOrWhiteSpace(Component.Category)
            ? Component.Title
            : $"{Component.Title} ({Component.Category})";

        return new ListBoxItem
        {
            Content = Text,
            Tag = Component
        };
    }
    /// <summary>
    /// Selects a tag by text.
    /// </summary>
    /// <param name="Tag">The tag text.</param>
    void SelectTag(string Tag)
    {
        if (string.IsNullOrWhiteSpace(Tag))
            return;

        foreach (object Item in lboTags.Items)
        {
            if (Item is string Text && Text.IsSameText(Tag))
            {
                lboTags.SelectedItem = Item;
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
    /// Shows the selected component preview.
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
        MarkdownPreviewRenderer.Render(MarkdownPreviewPanel, Component.Text);
    }
    /// <summary>
    /// Clears the component preview.
    /// </summary>
    void ShowNoSelectedComponent()
    {
        lblComponentTitle.Text = "Component";
        MarkdownPreviewRenderer.Render(MarkdownPreviewPanel, string.Empty);
    }

    // ● commands
    /// <summary>
    /// Deletes the selected tag from all components.
    /// </summary>
    async Task DeleteTag()
    {
        Project Project = AppHost.CurrentProject;
        string Tag = SelectedTag;
        if (Project == null || string.IsNullOrWhiteSpace(Tag))
            return;

        bool Confirmed = await Tripous.Desktop.MessageBox.YesNo($"Delete tag '{Tag}' from all components?", this);
        if (!Confirmed)
            return;

        try
        {
            int Count = 0;
            foreach (Component Component in Project.GetComponentList().Where(Component => Component.ContainsTag(Tag)))
            {
                Component.TagList = Component.TagList
                    .Where(Item => !Item.IsSameText(Tag))
                    .ToList();
                Component.Save();
                Count++;
            }

            LoadTags();
            RefreshComponentListForm();
            LogBox.AppendLine($"Tag deleted: {Tag} ({Count} components updated)");
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
    /// Placeholder for Quick View command.
    /// </summary>
    void QuickViewComponent()
    {
        LogBox.AppendLine("Quick View command not implemented yet.");
    }
    /// <summary>
    /// Refreshes the component list form when it is open.
    /// </summary>
    void RefreshComponentListForm()
    {
        ComponentListForm Form = AppHost.SideBarHandler?.FindAppForm(nameof(ComponentListForm)) as ComponentListForm;
        Form?.RefreshComponents();
    }

    // ● events
    /// <summary>
    /// Handles filter text changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void FilterTextChanged(object Sender, TextChangedEventArgs Args)
    {
        LoadTags();
    }
    /// <summary>
    /// Handles tag selection changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void TagsSelectionChanged(object Sender, SelectionChangedEventArgs Args)
    {
        if (!fLoading)
            LoadComponents();
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
        TitleText = "Tags";
        ClosableByUser = false;
        CreateToolBar();
        LoadTags();
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the TagListForm class.
    /// </summary>
    public TagListForm()
    {
        InitializeComponent();
    }

    // ● properties
    /// <summary>
    /// Gets the selected tag.
    /// </summary>
    string SelectedTag => lboTags.SelectedItem as string;
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
