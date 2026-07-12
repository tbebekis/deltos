// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Application settings.
/// </summary>
public class AppSettings: SettingsBase
{
    // ● protected
    /// <summary>
    /// Called before loading settings from disk.
    /// </summary>
    protected override void LoadBefore()
    {
        base.LoadBefore();
    }

    /// <summary>
    /// Called after settings have been loaded from disk.
    /// </summary>
    protected override void LoadAfter()
    {
        base.LoadAfter();
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the AppSettings class.
    /// </summary>
    public AppSettings()
    {
    }

    // ● properties
    /// <summary>
    /// Gets or sets a value indicating whether the last project is loaded on startup.
    /// </summary>
    public bool LoadLastProjectOnStartup { get; set; } = true;

    /// <summary>
    /// Gets or sets the last project folder path.
    /// </summary>
    public string LastProjectFolderPath { get; set; } = "___";

    /// <summary>
    /// Gets or sets a value indicating whether auto-save is enabled.
    /// </summary>
    public bool AutoSave { get; set; } = true;

    /// <summary>
    /// Gets or sets the auto-save interval in seconds.
    /// </summary>
    public int AutoSaveSecondsInterval { get; set; } = 30;

    /// <summary>
    /// Gets or sets the editor font family.
    /// </summary>
    public string FontFamily { get; set; } = "Times New Roman";

    /// <summary>
    /// Gets or sets the editor font size.
    /// </summary>
    public int FontSize { get; set; } = 13;

    /// <summary>
    /// Gets or sets a value indicating whether the second language editor is visible.
    /// </summary>
    public bool SecondLanguageVisible { get; set; } = false;
}
