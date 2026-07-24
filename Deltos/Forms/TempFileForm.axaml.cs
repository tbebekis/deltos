// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Forms;

/// <summary>
/// Edits the project temporary markdown text.
/// </summary>
public partial class TempFileForm: AppForm
{
    // ● private fields
    /// <summary>
    /// The toolbar helper.
    /// </summary>
    Tripous.Desktop.ToolBar fToolBar;
    /// <summary>
    /// True while values are loaded into controls.
    /// </summary>
    bool fLoading;

    // ● toolbar
    /// <summary>
    /// Creates the form toolbar.
    /// </summary>
    void CreateToolBar()
    {
        fToolBar = new();
        fToolBar.Panel = pnlToolBar;
        fToolBar.AddButton("html.png", "HTML Preview", PreviewTempText);
    }

    // ● private
    /// <summary>
    /// Loads the project temporary text into the editor.
    /// </summary>
    void LoadTempFile()
    {
        fLoading = true;
        try
        {
            Project Project = AppHost.CurrentProject;
            Editor.Title = "Temp";

            if (Project == null)
            {
                Editor.EditorText = string.Empty;
                Editor.FilePath = string.Empty;
                Editor.PreviewId = string.Empty;
                Editor.ReadOnly = true;
                TitleText = "Temp";
                return;
            }

            Editor.EditorText = Project.TempFileText;
            Editor.FilePath = Project.TempFilePath;
            Editor.PreviewId = AppHost.GetMarkdownPreviewFormId("TEMP-TEXT");
            Editor.ReadOnly = false;
            Editor.Modified = false;
            Editor.RegisterHighlighter(Editor.FilePath);
            AdjustTitle();
        }
        finally
        {
            fLoading = false;
        }
    }
    /// <summary>
    /// Saves the editor text to the project temporary markdown file.
    /// </summary>
    void SaveTempFile()
    {
        Project Project = AppHost.CurrentProject;
        if (Project == null)
            return;

        Project.TempFileText = Editor.EditorText;
        Project.SaveTempFile();
        Editor.Modified = false;
        AppHost.RemoveDirtyEditor(Editor);
        AdjustTitle();
        LogBox.AppendLine("Temp text saved.");
    }
    /// <summary>
    /// Updates the host title according to modified state.
    /// </summary>
    void AdjustTitle()
    {
        TitleText = Editor.Modified ? "Temp*" : "Temp";
    }
    /// <summary>
    /// Handles editor Save requests.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void Editor_SaveRequested(object Sender, EventArgs Args)
    {
        SaveTempFile();
    }
    /// <summary>
    /// Handles editor Show Folder requests.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void Editor_ShowFolderRequested(object Sender, EventArgs Args)
    {
        if (string.IsNullOrWhiteSpace(Editor.FilePath))
            return;

        Sys.OpenFileExplorer(Editor.FilePath);
    }
    /// <summary>
    /// Handles editor modified state changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void Editor_ModifiedChanged(object Sender, EventArgs Args)
    {
        if (fLoading)
            return;

        if (Editor.Modified)
            AppHost.AddDirtyEditor(Editor);

        AdjustTitle();
    }
    /// <summary>
    /// Previews the temporary markdown text.
    /// </summary>
    void PreviewTempText()
    {
        AppHost.ShowMarkdownPreview(AppHost.GetMarkdownPreviewFormId("TEMP-TEXT"), "HTML Preview: Temp Text", Editor.EditorText);
    }

    // ● overrides
    /// <summary>
    /// Called in order to initialize the form.
    /// </summary>
    protected override void FormInitialize()
    {
        TitleText = "Temp";
        ClosableByUser = false;
        CreateToolBar();
        Editor.SaveRequested += Editor_SaveRequested;
        Editor.ShowFolderRequested += Editor_ShowFolderRequested;
        Editor.ModifiedChanged += Editor_ModifiedChanged;
        LoadTempFile();
    }
    /// <summary>
    /// Called just before the form is closed.
    /// </summary>
    protected override void Closing()
    {
        Editor.SaveRequested -= Editor_SaveRequested;
        Editor.ShowFolderRequested -= Editor_ShowFolderRequested;
        Editor.ModifiedChanged -= Editor_ModifiedChanged;
        AppHost.RemoveDirtyEditor(Editor);

        base.Closing();
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the TempFileForm class.
    /// </summary>
    public TempFileForm()
    {
        InitializeComponent();
    }
}
