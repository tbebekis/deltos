// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents a category edit operation.
/// </summary>
public class CategoryManageItem
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the CategoryManageItem class.
    /// </summary>
    public CategoryManageItem()
    {
    }

    // ● properties
    /// <summary>
    /// Gets or sets the original category name.
    /// </summary>
    public string OriginalName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the current category name.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether this category is deleted.
    /// </summary>
    public bool Deleted { get; set; }
}

/// <summary>
/// Provides input and result data for the CategoryManageDialog.
/// </summary>
public class CategoryManageDialogData
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the CategoryManageDialogData class.
    /// </summary>
    public CategoryManageDialogData()
    {
    }

    // ● properties
    /// <summary>
    /// Gets or sets the default category name.
    /// </summary>
    public string DefaultCategory { get; set; } = Component.DefaultCategory;
    /// <summary>
    /// Gets or sets the category edit items.
    /// </summary>
    public List<CategoryManageItem> Categories { get; set; } = new();
}

/// <summary>
/// Lets the user rename and delete component categories.
/// </summary>
public partial class CategoryManageDialog: DialogWindow
{
    // ● private fields
    /// <summary>
    /// The edited dialog data.
    /// </summary>
    CategoryManageDialogData fData;

    // ● private
    /// <summary>
    /// Adds a category if it is missing.
    /// </summary>
    /// <param name="Category">The category name.</param>
    void AddCategory(string Category)
    {
        string Name = Category?.Trim();
        if (string.IsNullOrWhiteSpace(Name))
            Name = fData.DefaultCategory;

        if (fData.Categories.Any(Item => Item.OriginalName.IsSameText(Name)))
            return;

        fData.Categories.Add(new CategoryManageItem { OriginalName = Name, CategoryName = Name });
    }
    /// <summary>
    /// Creates a category list item.
    /// </summary>
    /// <param name="Item">The category item.</param>
    /// <returns>The created list box item.</returns>
    ListBoxItem CreateCategoryListItem(CategoryManageItem Item)
    {
        string Text = Item.CategoryName.IsSameText(fData.DefaultCategory)
            ? $"{Item.CategoryName} (Default)"
            : Item.CategoryName;

        return new ListBoxItem
        {
            Content = Text,
            Tag = Item
        };
    }
    /// <summary>
    /// Reloads the category list.
    /// </summary>
    void ReloadCategories()
    {
        string SelectedCategory = SelectedCategoryItem?.CategoryName;
        lboCategories.Items.Clear();

        foreach (CategoryManageItem Item in fData.Categories.Where(Item => !Item.Deleted).OrderBy(Item => Item.CategoryName))
            lboCategories.Items.Add(CreateCategoryListItem(Item));

        SelectCategory(SelectedCategory);

        if (lboCategories.SelectedItem == null && lboCategories.Items.Count > 0)
            lboCategories.SelectedIndex = 0;

        UpdateButtonState();
    }
    /// <summary>
    /// Selects a category by name.
    /// </summary>
    /// <param name="Category">The category name.</param>
    void SelectCategory(string Category)
    {
        if (string.IsNullOrWhiteSpace(Category))
            return;

        foreach (object Item in lboCategories.Items)
        {
            if (Item is ListBoxItem ListItem && ListItem.Tag is CategoryManageItem CategoryItem && CategoryItem.CategoryName.IsSameText(Category))
            {
                lboCategories.SelectedItem = ListItem;
                return;
            }
        }
    }
    /// <summary>
    /// Updates command button state.
    /// </summary>
    void UpdateButtonState()
    {
        CategoryManageItem Item = SelectedCategoryItem;
        bool IsDefault = Item != null && Item.CategoryName.IsSameText(fData.DefaultCategory);
        btnRename.IsEnabled = Item != null && !IsDefault;
        btnDelete.IsEnabled = Item != null && !IsDefault;
    }
    /// <summary>
    /// Renames the selected category.
    /// </summary>
    async Task RenameCategory()
    {
        CategoryManageItem Item = SelectedCategoryItem;
        if (Item == null || Item.CategoryName.IsSameText(fData.DefaultCategory))
            return;

        InputBoxData BoxData = await InputBox.ShowModal("Category", Item.CategoryName, this);
        if (BoxData == null || !BoxData.Result)
            return;

        string NewCategory = BoxData.Value?.Trim();
        if (string.IsNullOrWhiteSpace(NewCategory) || string.Equals(NewCategory, Item.CategoryName, StringComparison.Ordinal))
            return;

        if (!AppHost.IsValidFileName(NewCategory, false))
        {
            await Tripous.Desktop.MessageBox.Error($"Invalid category: {NewCategory}", this);
            return;
        }

        if (NewCategory.IsSameText(fData.DefaultCategory))
        {
            await Tripous.Desktop.MessageBox.Error($"Category already exists: {NewCategory}", this);
            return;
        }

        if (fData.Categories.Any(CategoryItem => !ReferenceEquals(CategoryItem, Item) && !CategoryItem.Deleted && CategoryItem.CategoryName.IsSameText(NewCategory)))
        {
            await Tripous.Desktop.MessageBox.Error($"Category already exists: {NewCategory}", this);
            return;
        }

        Item.CategoryName = NewCategory;
        ReloadCategories();
        SelectCategory(NewCategory);
    }
    /// <summary>
    /// Deletes the selected category.
    /// </summary>
    void DeleteCategory()
    {
        CategoryManageItem Item = SelectedCategoryItem;
        if (Item == null || Item.CategoryName.IsSameText(fData.DefaultCategory))
            return;

        Item.Deleted = true;
        Item.CategoryName = fData.DefaultCategory;
        ReloadCategories();
    }
    /// <summary>
    /// Handles category selection changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void CategoriesSelectionChanged(object Sender, SelectionChangedEventArgs Args)
    {
        UpdateButtonState();
    }
    /// <summary>
    /// Handles category double-tap.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    async void CategoriesDoubleTapped(object Sender, TappedEventArgs Args)
    {
        await RenameCategory();
    }
    /// <summary>
    /// Handles Rename click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    async void Rename_Click(object Sender, RoutedEventArgs Args)
    {
        await RenameCategory();
    }
    /// <summary>
    /// Handles Delete click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void Delete_Click(object Sender, RoutedEventArgs Args)
    {
        DeleteCategory();
    }
    /// <summary>
    /// Handles OK click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void OK_Click(object Sender, RoutedEventArgs Args)
    {
        ResultData = fData;
        ModalResult = ModalResult.Ok;
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

    // ● protected
    /// <summary>
    /// Loads item values into the dialog controls.
    /// </summary>
    protected override async Task ItemToControls()
    {
        CategoryManageDialogData Source = InputData as CategoryManageDialogData ?? new CategoryManageDialogData();
        fData = new CategoryManageDialogData { DefaultCategory = Source.DefaultCategory };

        foreach (CategoryManageItem Item in Source.Categories ?? new List<CategoryManageItem>())
            AddCategory(Item.CategoryName);

        AddCategory(fData.DefaultCategory);
        ReloadCategories();
        await Task.CompletedTask;
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the CategoryManageDialog class.
    /// </summary>
    public CategoryManageDialog()
    {
        InitializeComponent();
    }

    // ● properties
    /// <summary>
    /// Gets the selected category item.
    /// </summary>
    CategoryManageItem SelectedCategoryItem
    {
        get
        {
            if (lboCategories.SelectedItem is ListBoxItem Item)
                return Item.Tag as CategoryManageItem;

            return null;
        }
    }
}
