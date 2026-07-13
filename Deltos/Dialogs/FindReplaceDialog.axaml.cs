// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Edits find and replace options.
/// </summary>
public partial class FindReplaceDialog: DialogWindow
{
    // ● private fields
    /// <summary>
    /// The edited options.
    /// </summary>
    FindReplaceOptions fOptions;

    // ● private
    /// <summary>
    /// Loads options into controls.
    /// </summary>
    void OptionsToControls()
    {
        edtFind.Text = fOptions.TextToFind;
        edtReplace.Text = fOptions.ReplaceWith;
        chMatchCase.IsChecked = fOptions.MatchCase;
        chWholeWord.IsChecked = fOptions.WholeWord;
        chReplace.IsChecked = fOptions.Replace;
        chReplaceAll.IsChecked = fOptions.ReplaceAll;
    }
    /// <summary>
    /// Saves controls into options.
    /// </summary>
    /// <returns>True if options are valid; otherwise false.</returns>
    bool ControlsToOptions()
    {
        string TextToFind = edtFind.Text == null ? string.Empty : edtFind.Text.Trim();
        if (string.IsNullOrWhiteSpace(TextToFind))
            return false;

        fOptions.TextToFind = TextToFind;
        fOptions.ReplaceWith = edtReplace.Text ?? string.Empty;
        fOptions.MatchCase = chMatchCase.IsChecked == true;
        fOptions.WholeWord = chWholeWord.IsChecked == true;
        fOptions.Replace = chReplace.IsChecked == true;
        fOptions.ReplaceAll = chReplaceAll.IsChecked == true;
        return true;
    }
    /// <summary>
    /// Handles the OK button click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    async void OK_Click(object Sender, RoutedEventArgs Args)
    {
        if (!ControlsToOptions())
        {
            await Tripous.Desktop.MessageBox.Info("Find text is required.", this);
            return;
        }

        ResultData = fOptions;
        ModalResult = ModalResult.Ok;
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
    /// Sets up the dialog before display.
    /// </summary>
    protected override async Task WindowInitialize()
    {
        await base.WindowInitialize();
        fOptions = InputData as FindReplaceOptions ?? new FindReplaceOptions();
        OptionsToControls();
        edtFind.Focus();
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the FindReplaceDialog class.
    /// </summary>
    public FindReplaceDialog()
    {
        InitializeComponent();
    }
}
