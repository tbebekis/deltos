// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Provides data for the item title dialog.
/// </summary>
public class ItemTitleDialogData
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the ItemTitleDialogData class.
    /// </summary>
    public ItemTitleDialogData()
    {
    }

    // ● properties
    /// <summary>
    /// Gets or sets the dialog caption.
    /// </summary>
    public string Caption { get; set; } = "Title";
    /// <summary>
    /// Gets or sets the primary title.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the secondary title.
    /// </summary>
    public string Title2 { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the item type.
    /// </summary>
    public ItemType Type { get; set; } = ItemType.None;
    /// <summary>
    /// Gets or sets the item level title.
    /// </summary>
    public string LevelTitle { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether the item title is included in output.
    /// </summary>
    public bool IncludeTitleInOutput { get; set; } = true;
    /// <summary>
    /// Gets or sets a value indicating whether output should add a page break before this item.
    /// </summary>
    public bool PageBreakBefore { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this item is included in the table of contents.
    /// </summary>
    public bool IncludeInToc { get; set; } = true;
    /// <summary>
    /// Gets or sets the item numbering behavior.
    /// </summary>
    public ItemNumbering Numbering { get; set; } = ItemNumbering.Automatic;
    /// <summary>
    /// Gets or sets custom numbering text.
    /// </summary>
    public string CustomNumbering { get; set; } = string.Empty;
}

/// <summary>
/// Edits primary and secondary item titles.
/// </summary>
public partial class ItemTitleDialog: DialogWindow
{
    // ● private fields
    /// <summary>
    /// The dialog data.
    /// </summary>
    ItemTitleDialogData fData;

    // ● private
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
    /// Handles numbering selection changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void Numbering_SelectionChanged(object Sender, SelectionChangedEventArgs Args)
    {
        UpdateCustomNumberingEnabled();
    }
    /// <summary>
    /// Updates custom numbering controls.
    /// </summary>
    void UpdateCustomNumberingEnabled()
    {
        if (edtCustomNumbering == null)
            return;

        edtCustomNumbering.IsEnabled = cboNumbering.SelectedIndex == (int)ItemNumbering.Custom;
    }

    // ● protected
    /// <summary>
    /// Loads item values into the dialog controls.
    /// </summary>
    protected override async Task ItemToControls()
    {
        fData = InputData as ItemTitleDialogData ?? new ItemTitleDialogData();
        Title = fData.Caption;
        txtType.Text = fData.Type == ItemType.None ? string.Empty : fData.Type.ToString();
        txtLevelTitle.Text = fData.LevelTitle;
        pnlLevelTitle.IsVisible = !string.IsNullOrWhiteSpace(fData.LevelTitle);
        edtTitle.Text = fData.Title;
        edtTitle2.Text = fData.Title2;
        chkIncludeTitleInOutput.IsChecked = fData.IncludeTitleInOutput;
        chkPageBreakBefore.IsChecked = fData.PageBreakBefore;
        chkIncludeInToc.IsChecked = fData.IncludeInToc;
        cboNumbering.ItemsSource = new[] { "Automatic", "None", "Custom" };
        cboNumbering.SelectedIndex = (int)fData.Numbering;
        edtCustomNumbering.Text = fData.CustomNumbering;
        UpdateCustomNumberingEnabled();
        edtTitle.Focus();
        await Task.CompletedTask;
    }
    /// <summary>
    /// Saves dialog control values to the item.
    /// </summary>
    protected override async Task ControlsToItem()
    {
        string Title = edtTitle.Text?.Trim();
        if (string.IsNullOrWhiteSpace(Title))
        {
            await Tripous.Desktop.MessageBox.Error("Title cannot be empty.", this);
            return;
        }

        fData.Title = Title;
        fData.Title2 = edtTitle2.Text?.Trim() ?? string.Empty;
        fData.IncludeTitleInOutput = chkIncludeTitleInOutput.IsChecked == true;
        fData.PageBreakBefore = chkPageBreakBefore.IsChecked == true;
        fData.IncludeInToc = chkIncludeInToc.IsChecked == true;
        fData.Numbering = (ItemNumbering)cboNumbering.SelectedIndex;
        fData.CustomNumbering = edtCustomNumbering.Text?.Trim() ?? string.Empty;
        if (fData.Numbering == ItemNumbering.Custom && string.IsNullOrWhiteSpace(fData.CustomNumbering))
        {
            await Tripous.Desktop.MessageBox.Error("Custom numbering cannot be empty.", this);
            return;
        }

        ResultData = fData;
        ModalResult = ModalResult.Ok;
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the ItemTitleDialog class.
    /// </summary>
    public ItemTitleDialog()
    {
        InitializeComponent();
    }
}
