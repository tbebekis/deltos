// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Application settings.
/// </summary>
public class AppSettings: SettingsBase
{
    // ● private
    /// <summary>
    /// Normalizes the recent project list.
    /// </summary>
    void NormalizeRecentProjects()
    {
        List<string> List = new();

        foreach (string ProjectPath in RecentProjects ?? new List<string>())
        {
            string NormalizedPath = NormalizeProjectPath(ProjectPath);
            if (!string.IsNullOrWhiteSpace(NormalizedPath) && !List.Any(x => x.IsSameText(NormalizedPath)))
                List.Add(NormalizedPath);
        }

        while (List.Count > 10)
            List.RemoveAt(List.Count - 1);

        RecentProjects = List;
    }
    /// <summary>
    /// Normalizes a project path.
    /// </summary>
    /// <param name="ProjectPath">The project path.</param>
    /// <returns>The normalized project path.</returns>
    string NormalizeProjectPath(string ProjectPath)
    {
        if (string.IsNullOrWhiteSpace(ProjectPath))
            return string.Empty;

        string Result = ProjectPath.Trim();

        try
        {
            Result = System.IO.Path.GetFullPath(Result);
        }
        catch
        {
        }

        return Result;
    }

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
        NormalizeRecentProjects();
        WordsPerPage = Math.Clamp(WordsPerPage, 50, 1000);
        Theme = AppHost.NormalizeTheme(Theme);
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the AppSettings class.
    /// </summary>
    public AppSettings()
    {
    }

    // ● public
    /// <summary>
    /// Copies editable settings from another settings instance.
    /// </summary>
    /// <param name="Source">The source settings.</param>
    public void CopyEditableSettingsFrom(AppSettings Source)
    {
        if (Source == null)
            return;

        AutoSave = Source.AutoSave;
        AutoSaveSecondsInterval = Source.AutoSaveSecondsInterval;
        FontFamily = Source.FontFamily;
        FontSize = Source.FontSize;
        WordsPerPage = Source.WordsPerPage;
        Theme = Source.Theme;
        SecondLanguageVisible = Source.SecondLanguageVisible;
        ShowMarkdownPreviewButton = Source.ShowMarkdownPreviewButton;
        ShowFolderLevelTitleInTree = Source.ShowFolderLevelTitleInTree;
    }
    /// <summary>
    /// Adds a project path to the recent project list.
    /// </summary>
    /// <param name="ProjectPath">The project path.</param>
    public void AddRecentProject(string ProjectPath)
    {
        string NormalizedPath = NormalizeProjectPath(ProjectPath);
        if (string.IsNullOrWhiteSpace(NormalizedPath))
            return;

        NormalizeRecentProjects();
        RecentProjects.RemoveAll(x => x.IsSameText(NormalizedPath));
        RecentProjects.Insert(0, NormalizedPath);

        while (RecentProjects.Count > 10)
            RecentProjects.RemoveAt(RecentProjects.Count - 1);
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
    /// Gets or sets the recent project folder paths.
    /// </summary>
    public List<string> RecentProjects { get; set; } = new();
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
    public string FontFamily { get; set; } = "Liberation Mono, Cascadia Code, Consolas, Monospace";
    /// <summary>
    /// Gets or sets the editor font size.
    /// </summary>
    public int FontSize { get; set; } = 13;
    /// <summary>
    /// Gets or sets the words per estimated page.
    /// </summary>
    public int WordsPerPage { get; set; } = 250;
    /// <summary>
    /// Gets or sets the application theme.
    /// </summary>
    public string Theme { get; set; } = "Dark";
    /// <summary>
    /// Gets or sets a value indicating whether the second language editor is visible.
    /// </summary>
    public bool SecondLanguageVisible { get; set; } = false;
    /// <summary>
    /// Gets or sets a value indicating whether text editors show the markdown preview button.
    /// </summary>
    public bool ShowMarkdownPreviewButton { get; set; } = true;
    /// <summary>
    /// Gets or sets a value indicating whether folder level titles are shown before folder titles in the UI tree.
    /// </summary>
    public bool ShowFolderLevelTitleInTree { get; set; } = true;
}
