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

    // ● protected
    /// <summary>
    /// Loads item values into the dialog controls.
    /// </summary>
    protected override async Task ItemToControls()
    {
        fData = InputData as ItemTitleDialogData ?? new ItemTitleDialogData();
        Title = fData.Caption;
        edtTitle.Text = fData.Title;
        edtTitle2.Text = fData.Title2;
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
