// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Provides input data for the SelectParentDialog.
/// </summary>
public class SelectParentDialogData
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the SelectParentDialogData class.
    /// </summary>
    public SelectParentDialogData()
    {
    }

    // ● properties
    /// <summary>
    /// Gets or sets the item that changes parent.
    /// </summary>
    public BaseItem Item { get; set; }
    /// <summary>
    /// Gets or sets the available parent items.
    /// </summary>
    public List<BaseItem> ParentList { get; set; } = new();
}

/// <summary>
/// Lets the user select a new parent for an item.
/// </summary>
public partial class SelectParentDialog: DialogWindow
{
    // ● private fields
    /// <summary>
    /// Field for the dialog input data.
    /// </summary>
    SelectParentDialogData fData;

    // ● private
    /// <summary>
    /// Returns a display path for an item.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <returns>The display path.</returns>
    string GetItemPath(BaseItem Item)
    {
        List<string> Parts = new();
        BaseItem Current = Item;
        while (Current != null && !(Current is Project))
        {
            Parts.Insert(0, GetItemPathPart(Current));
            Current = Current.Parent;
        }

        return string.Join(" / ", Parts);
    }
    /// <summary>
    /// Returns a display path part for an item.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <returns>The display path part.</returns>
    string GetItemPathPart(BaseItem Item)
    {
        Folder Folder = Item as Folder;
        if (Folder != null)
            return string.IsNullOrWhiteSpace(Folder.LevelTitle) ? Folder.Title : $"{Folder.LevelTitle}: {Folder.Title}";

        return Item.Title;
    }
    /// <summary>
    /// Creates a parent list item.
    /// </summary>
    /// <param name="Item">The parent item.</param>
    /// <returns>The created list box item.</returns>
    ListBoxItem CreateParentListItem(BaseItem Item)
    {
        return new ListBoxItem
        {
            Content = GetItemPath(Item),
            Tag = Item
        };
    }
    /// <summary>
    /// Handles parent selection changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void ParentSelectionChanged(object Sender, SelectionChangedEventArgs Args)
    {
        btnOK.IsEnabled = lboParents.SelectedItem is ListBoxItem;
    }
    /// <summary>
    /// Handles the OK button click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void OK_Click(object Sender, RoutedEventArgs Args)
    {
        if (lboParents.SelectedItem is ListBoxItem Item && Item.Tag is BaseItem ParentItem)
        {
            ResultData = ParentItem;
            ModalResult = ModalResult.Ok;
        }
    }
    /// <summary>
    /// Handles the Cancel button click.
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
        fData = InputData as SelectParentDialogData ?? new SelectParentDialogData();
        txtItem.Text = fData.Item == null ? string.Empty : GetItemPath(fData.Item);
        lboParents.Items.Clear();

        foreach (BaseItem ParentItem in fData.ParentList)
            lboParents.Items.Add(CreateParentListItem(ParentItem));

        if (lboParents.Items.Count > 0)
            lboParents.SelectedIndex = 0;

        btnOK.IsEnabled = lboParents.SelectedItem is ListBoxItem;
        await Task.CompletedTask;
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the SelectParentDialog class.
    /// </summary>
    public SelectParentDialog()
    {
        InitializeComponent();
    }
}
