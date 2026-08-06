// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Edits document export options.
/// </summary>
public partial class ExportDialog: DialogWindow
{
    // ● private fields
    /// <summary>
    /// The edited export options.
    /// </summary>
    ExportOptions fOptions;

    // ● private
    /// <summary>
    /// Handles OK click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    async void OK_Click(object Sender, RoutedEventArgs Args)
    {
        if (await ControlsToOptions())
        {
            ResultData = fOptions;
            ModalResult = ModalResult.Ok;
        }
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
    /// Loads options into controls.
    /// </summary>
    void OptionsToControls()
    {
        chPrimary.IsChecked = fOptions.Language.HasFlag(ExportLanguage.Primary);
        chSecondary.IsChecked = fOptions.Language.HasFlag(ExportLanguage.Secondary);

        chText.IsChecked = fOptions.Source.HasFlag(ExportSource.Text);
        chSynopsis.IsChecked = fOptions.Source.HasFlag(ExportSource.Synopsis);

        chTxt.IsChecked = fOptions.Format.HasFlag(ExportFormat.Txt);
        chHtml.IsChecked = fOptions.Format.HasFlag(ExportFormat.Html);
        chOdt.IsChecked = fOptions.Format.HasFlag(ExportFormat.Odt);
        chMarkdown.IsChecked = fOptions.Format.HasFlag(ExportFormat.Markdown);
        chInternalMarkdown.IsChecked = fOptions.Format.HasFlag(ExportFormat.InternalMarkdown);

        chFolderBullet.IsChecked = fOptions.FolderTitle.HasFlag(ExportTitleOptions.Bullet);
        chFolderNumber.IsChecked = fOptions.FolderTitle.HasFlag(ExportTitleOptions.Number);
        chFolderWord.IsChecked = fOptions.FolderTitle.HasFlag(ExportTitleOptions.Word);
        chFolderTitle.IsChecked = fOptions.FolderTitle.HasFlag(ExportTitleOptions.Title);

        chTextFileBullet.IsChecked = fOptions.TextFileTitle.HasFlag(ExportTitleOptions.Bullet);
        chTextFileNumber.IsChecked = fOptions.TextFileTitle.HasFlag(ExportTitleOptions.Number);
        chTextFileWord.IsChecked = fOptions.TextFileTitle.HasFlag(ExportTitleOptions.Word);
        chTextFileTitle.IsChecked = fOptions.TextFileTitle.HasFlag(ExportTitleOptions.Title);

        chTreatTextFilesAsPlainText.IsChecked = fOptions.TreatTextFilesAsPlainText;
        chSingleLineBreaksCreateParagraphs.IsChecked = fOptions.SingleLineBreaksCreateParagraphs;
        edtImageMaxWidth.Value = Math.Clamp(fOptions.ImageMaxWidth, 100, 2000);
    }
    /// <summary>
    /// Saves controls into options.
    /// </summary>
    /// <returns>True if controls are valid; otherwise false.</returns>
    async Task<bool> ControlsToOptions()
    {
        fOptions.Clear();

        if (chPrimary.IsChecked == true)
            fOptions.Language |= ExportLanguage.Primary;
        if (chSecondary.IsChecked == true)
            fOptions.Language |= ExportLanguage.Secondary;

        if (chText.IsChecked == true)
            fOptions.Source |= ExportSource.Text;
        if (chSynopsis.IsChecked == true)
            fOptions.Source |= ExportSource.Synopsis;

        if (chTxt.IsChecked == true)
            fOptions.Format |= ExportFormat.Txt;
        if (chHtml.IsChecked == true)
            fOptions.Format |= ExportFormat.Html;
        if (chOdt.IsChecked == true)
            fOptions.Format |= ExportFormat.Odt;
        if (chMarkdown.IsChecked == true)
            fOptions.Format |= ExportFormat.Markdown;
        if (chInternalMarkdown.IsChecked == true)
            fOptions.Format |= ExportFormat.InternalMarkdown;

        if (chFolderBullet.IsChecked == true)
            fOptions.FolderTitle |= ExportTitleOptions.Bullet;
        if (chFolderNumber.IsChecked == true)
            fOptions.FolderTitle |= ExportTitleOptions.Number;
        if (chFolderWord.IsChecked == true)
            fOptions.FolderTitle |= ExportTitleOptions.Word;
        if (chFolderTitle.IsChecked == true)
            fOptions.FolderTitle |= ExportTitleOptions.Title;

        if (chTextFileBullet.IsChecked == true)
            fOptions.TextFileTitle |= ExportTitleOptions.Bullet;
        if (chTextFileNumber.IsChecked == true)
            fOptions.TextFileTitle |= ExportTitleOptions.Number;
        if (chTextFileWord.IsChecked == true)
            fOptions.TextFileTitle |= ExportTitleOptions.Word;
        if (chTextFileTitle.IsChecked == true)
            fOptions.TextFileTitle |= ExportTitleOptions.Title;

        fOptions.TreatTextFilesAsPlainText = chTreatTextFilesAsPlainText.IsChecked == true;
        fOptions.SingleLineBreaksCreateParagraphs = chSingleLineBreaksCreateParagraphs.IsChecked == true;
        fOptions.ImageMaxWidth = Math.Clamp((int)(edtImageMaxWidth.Value ?? 400), 100, 2000);

        if (fOptions.Language == ExportLanguage.None)
        {
            await Tripous.Desktop.MessageBox.Info("Please select at least one language.", this);
            return false;
        }

        if (fOptions.Source == ExportSource.None)
        {
            await Tripous.Desktop.MessageBox.Info("Please select at least one content source.", this);
            return false;
        }

        if (fOptions.Format == ExportFormat.None)
        {
            await Tripous.Desktop.MessageBox.Info("Please select at least one format.", this);
            return false;
        }

        if (fOptions.Format.HasFlag(ExportFormat.InternalMarkdown) && !fOptions.Source.HasFlag(ExportSource.Text))
        {
            await Tripous.Desktop.MessageBox.Info("Internal Markdown export requires Text content.", this);
            return false;
        }

        return true;
    }

    // ● protected
    /// <summary>
    /// Loads item values into the dialog controls.
    /// </summary>
    protected override async Task ItemToControls()
    {
        fOptions = InputData as ExportOptions ?? new ExportOptions();
        OptionsToControls();
        await Task.CompletedTask;
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the ExportDialog class.
    /// </summary>
    public ExportDialog()
    {
        InitializeComponent();
    }
}
