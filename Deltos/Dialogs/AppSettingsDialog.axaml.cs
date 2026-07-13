// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Edits application settings.
/// </summary>
public partial class AppSettingsDialog: DialogWindow
{
    // ● private fields
    /// <summary>
    /// The edited settings copy.
    /// </summary>
    AppSettings fSettings;

    // ● private
    /// <summary>
    /// Handles OK click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void OK_Click(object Sender, RoutedEventArgs Args)
    {
        ControlsToSettings();
        ResultData = fSettings;
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
    /// <summary>
    /// Creates an editable settings copy.
    /// </summary>
    /// <param name="Source">The source settings.</param>
    /// <returns>The settings copy.</returns>
    AppSettings CreateSettingsCopy(AppSettings Source)
    {
        AppSettings Result = new AppSettings();
        if (Source == null)
            return Result;

        Result.LoadLastProjectOnStartup = Source.LoadLastProjectOnStartup;
        Result.LastProjectFolderPath = Source.LastProjectFolderPath;
        Result.AutoSave = Source.AutoSave;
        Result.AutoSaveSecondsInterval = Source.AutoSaveSecondsInterval;
        Result.FontFamily = Source.FontFamily;
        Result.FontSize = Source.FontSize;
        Result.SecondLanguageVisible = Source.SecondLanguageVisible;
        return Result;
    }
    /// <summary>
    /// Loads settings into controls.
    /// </summary>
    void SettingsToControls()
    {
        chkAutoSave.IsChecked = fSettings.AutoSave;
        edtAutoSaveSecondsInterval.Value = fSettings.AutoSaveSecondsInterval;
        edtFontFamily.Text = fSettings.FontFamily;
        edtFontSize.Value = fSettings.FontSize;
        chkSecondLanguageVisible.IsChecked = fSettings.SecondLanguageVisible;
    }
    /// <summary>
    /// Saves controls into settings.
    /// </summary>
    void ControlsToSettings()
    {
        fSettings.AutoSave = chkAutoSave.IsChecked == true;
        fSettings.AutoSaveSecondsInterval = Math.Clamp((int)(edtAutoSaveSecondsInterval.Value ?? 30), 5, 3600);
        fSettings.FontFamily = string.IsNullOrWhiteSpace(edtFontFamily.Text) ? "Liberation Mono, Cascadia Code, Consolas, Monospace" : edtFontFamily.Text.Trim();
        fSettings.FontSize = Math.Clamp((int)(edtFontSize.Value ?? 13), 8, 32);
        fSettings.SecondLanguageVisible = chkSecondLanguageVisible.IsChecked == true;
    }

    // ● protected
    /// <summary>
    /// Loads item values into the dialog controls.
    /// </summary>
    protected override async Task ItemToControls()
    {
        fSettings = CreateSettingsCopy(InputData as AppSettings);
        SettingsToControls();
        edtFontFamily.Focus();
        await Task.CompletedTask;
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the AppSettingsDialog class.
    /// </summary>
    public AppSettingsDialog()
    {
        InitializeComponent();
    }
}
