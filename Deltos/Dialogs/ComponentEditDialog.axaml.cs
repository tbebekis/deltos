// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Provides input data for the component edit dialog.
/// </summary>
public class ComponentEditDialogData
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the ComponentEditDialogData class.
    /// </summary>
    public ComponentEditDialogData()
    {
    }

    // ● properties
    /// <summary>
    /// Gets or sets the edited component.
    /// </summary>
    public Component Component { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this is an insert operation.
    /// </summary>
    public bool IsInsert { get; set; }
    /// <summary>
    /// Gets or sets the original component title.
    /// </summary>
    public string OriginalTitle { get; set; } = string.Empty;
}

/// <summary>
/// Edits component metadata.
/// </summary>
public partial class ComponentEditDialog: DialogWindow
{
    // ● private fields
    /// <summary>
    /// The dialog data.
    /// </summary>
    ComponentEditDialogData fData;
    /// <summary>
    /// The available categories.
    /// </summary>
    List<string> fCategories = new();
    /// <summary>
    /// The available tags.
    /// </summary>
    List<string> fAvailableTags = new();
    /// <summary>
    /// The selected tags.
    /// </summary>
    List<string> fSelectedTags = new();

    // ● private
    /// <summary>
    /// Adds an item to a list if missing.
    /// </summary>
    /// <param name="List">The list.</param>
    /// <param name="Value">The value.</param>
    void AddUnique(List<string> List, string Value)
    {
        string Item = Value == null ? string.Empty : Value.Trim();
        if (!string.IsNullOrWhiteSpace(Item) && !List.Contains(Item, StringComparer.OrdinalIgnoreCase))
            List.Add(Item);
    }
    /// <summary>
    /// Removes an item from a list.
    /// </summary>
    /// <param name="List">The list.</param>
    /// <param name="Value">The value.</param>
    void RemoveValue(List<string> List, string Value)
    {
        string Existing = List.FirstOrDefault(Item => Item.IsSameText(Value));
        if (Existing != null)
            List.Remove(Existing);
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
    /// Reloads the tag list boxes.
    /// </summary>
    void ReloadTagLists()
    {
        ReloadList(lboAvailableTags, fAvailableTags);
        ReloadList(lboSelectedTags, fSelectedTags);
    }
    /// <summary>
    /// Adds a category from the category text box.
    /// </summary>
    async Task AddCategory()
    {
        string Category = edtCategory.Text?.Trim();
        if (string.IsNullOrWhiteSpace(Category))
            return;

        if (!AppHost.IsValidFileName(Category, false))
        {
            await Tripous.Desktop.MessageBox.Error($"Invalid category: {Category}", this);
            return;
        }

        AddUnique(fCategories, Category);
        ReloadList(lboCategories, fCategories);
        lboCategories.SelectedItem = fCategories.FirstOrDefault(Item => Item.IsSameText(Category));
        edtCategory.Text = string.Empty;
    }
    /// <summary>
    /// Adds tags from the tags text box.
    /// </summary>
    async Task AddTags()
    {
        string Text = edtTags.Text?.Trim();
        if (string.IsNullOrWhiteSpace(Text))
            return;

        foreach (string Part in Text.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            string Tag = Part.Trim();
            if (!AppHost.IsValidFileName(Tag, false))
            {
                await Tripous.Desktop.MessageBox.Error($"Invalid tag: {Tag}", this);
                return;
            }

            AddUnique(fSelectedTags, Tag);
            RemoveValue(fAvailableTags, Tag);
        }

        ReloadTagLists();
        edtTags.Text = string.Empty;
    }
    /// <summary>
    /// Selects all available tags.
    /// </summary>
    void SelectAll()
    {
        foreach (string Tag in fAvailableTags)
            AddUnique(fSelectedTags, Tag);

        fAvailableTags.Clear();
        ReloadTagLists();
    }
    /// <summary>
    /// Unselects all selected tags.
    /// </summary>
    void UnselectAll()
    {
        foreach (string Tag in fSelectedTags)
            AddUnique(fAvailableTags, Tag);

        fSelectedTags.Clear();
        ReloadTagLists();
    }
    /// <summary>
    /// Selects one available tag.
    /// </summary>
    void SelectOne()
    {
        string Tag = lboAvailableTags.SelectedItem as string;
        if (Tag == null)
            return;

        RemoveValue(fAvailableTags, Tag);
        AddUnique(fSelectedTags, Tag);
        ReloadTagLists();
        lboSelectedTags.SelectedItem = Tag;
    }
    /// <summary>
    /// Unselects one selected tag.
    /// </summary>
    void UnselectOne()
    {
        string Tag = lboSelectedTags.SelectedItem as string;
        if (Tag == null)
            return;

        RemoveValue(fSelectedTags, Tag);
        AddUnique(fAvailableTags, Tag);
        ReloadTagLists();
        lboAvailableTags.SelectedItem = Tag;
    }
    /// <summary>
    /// Handles OK click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    async void OK_Click(object Sender, RoutedEventArgs Args)
    {
        await ControlsToItem();
    }
    /// <summary>
    /// Handles Cancel click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void Cancel_Click(object Sender, RoutedEventArgs Args)
    {
        ModalResult = ModalResult.Cancel;
    }
    /// <summary>
    /// Handles Add Category click.
    /// </summary>
    async void AddCategory_Click(object Sender, RoutedEventArgs Args)
    {
        await AddCategory();
    }
    /// <summary>
    /// Handles Add Tags click.
    /// </summary>
    async void AddTags_Click(object Sender, RoutedEventArgs Args)
    {
        await AddTags();
    }
    /// <summary>
    /// Handles Select All click.
    /// </summary>
    void SelectAll_Click(object Sender, RoutedEventArgs Args)
    {
        SelectAll();
    }
    /// <summary>
    /// Handles Unselect All click.
    /// </summary>
    void UnselectAll_Click(object Sender, RoutedEventArgs Args)
    {
        UnselectAll();
    }
    /// <summary>
    /// Handles Select One click.
    /// </summary>
    void SelectOne_Click(object Sender, RoutedEventArgs Args)
    {
        SelectOne();
    }
    /// <summary>
    /// Handles Unselect One click.
    /// </summary>
    void UnselectOne_Click(object Sender, RoutedEventArgs Args)
    {
        UnselectOne();
    }
    /// <summary>
    /// Handles category Enter key.
    /// </summary>
    async void CategoryKeyDown(object Sender, KeyEventArgs Args)
    {
        if (Args.Key == Key.Enter)
        {
            Args.Handled = true;
            await AddCategory();
        }
    }
    /// <summary>
    /// Handles tags Enter key.
    /// </summary>
    async void TagsKeyDown(object Sender, KeyEventArgs Args)
    {
        if (Args.Key == Key.Enter)
        {
            Args.Handled = true;
            await AddTags();
        }
    }
    /// <summary>
    /// Handles available tag double-tap.
    /// </summary>
    void AvailableTagsDoubleTapped(object Sender, TappedEventArgs Args)
    {
        SelectOne();
    }
    /// <summary>
    /// Handles selected tag double-tap.
    /// </summary>
    void SelectedTagsDoubleTapped(object Sender, TappedEventArgs Args)
    {
        UnselectOne();
    }

    // ● protected
    /// <summary>
    /// Loads item values into the dialog controls.
    /// </summary>
    protected override async Task ItemToControls()
    {
        fData = InputData as ComponentEditDialogData ?? new ComponentEditDialogData();
        Component Component = fData.Component ?? new Component();
        Project Project = AppHost.CurrentProject;

        edtTitle.Text = Component.Title;
        edtAliases.Text = Component.Aliases;

        fCategories = Project?.GetCategoryList() ?? new List<string>();
        AddUnique(fCategories, Component.Category);
        ReloadList(lboCategories, fCategories);
        lboCategories.SelectedItem = fCategories.FirstOrDefault(Item => Item.IsSameText(Component.Category));

        fSelectedTags = Component.TagList.ToList();
        fAvailableTags = Project?.GetTagList() ?? new List<string>();
        foreach (string Tag in fSelectedTags)
            RemoveValue(fAvailableTags, Tag);

        ReloadTagLists();
        edtTitle.Focus();
        await Task.CompletedTask;
    }
    /// <summary>
    /// Saves dialog control values to the item.
    /// </summary>
    protected override async Task ControlsToItem()
    {
        Component Component = fData.Component;
        Project Project = AppHost.CurrentProject;
        string Title = edtTitle.Text?.Trim();

        if (!AppHost.IsValidFileName(Title, false))
        {
            await Tripous.Desktop.MessageBox.Error($"Invalid component title: {Title}", this);
            return;
        }

        if (lboCategories.SelectedItem == null)
        {
            await Tripous.Desktop.MessageBox.Info("Please select a category.", this);
            return;
        }

        int Count = Project?.CountComponentTitle(Title) ?? 0;
        int MaxCount = !fData.IsInsert && Title.IsSameText(fData.OriginalTitle) ? 1 : 0;
        if (Count > MaxCount)
        {
            await Tripous.Desktop.MessageBox.Error($"Component already exists: {Title}", this);
            return;
        }

        Component.Title = Title;
        Component.Category = lboCategories.SelectedItem as string;
        Component.Aliases = edtAliases.Text?.Trim();
        Component.TagList = fSelectedTags.OrderBy(Item => Item).ToList();
        ResultData = Component;
        ModalResult = ModalResult.Ok;
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the ComponentEditDialog class.
    /// </summary>
    public ComponentEditDialog()
    {
        InitializeComponent();
    }
}
