// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Forms;

/// <summary>
/// Edits a project note.
/// </summary>
public partial class NoteForm: AppForm
{
    // ● private fields
    /// <summary>
    /// Field for the Note property.
    /// </summary>
    Note fNote;
    /// <summary>
    /// True while values are loaded into controls.
    /// </summary>
    bool fLoading;
    /// <summary>
    /// The base tab title without modified marker.
    /// </summary>
    string fBaseTitle = "Note";

    // ● private
    /// <summary>
    /// Loads the note into the editor.
    /// </summary>
    void LoadNote()
    {
        fLoading = true;
        try
        {
            if (Note == null)
            {
                fBaseTitle = "Note";
                TitleText = fBaseTitle;
                Editor.Title = fBaseTitle;
                Editor.EditorText = string.Empty;
                Editor.FilePath = string.Empty;
                Editor.ReadOnly = true;
                return;
            }

            fBaseTitle = Note.DisplayTitle;
            TitleText = fBaseTitle;
            Editor.Title = fBaseTitle;
            Editor.EditorText = Note.Text;
            Editor.FilePath = Note.TextFilePath;
            Editor.ReadOnly = false;
            Editor.Modified = false;
            Editor.RegisterHighlighter(Editor.FilePath);
        }
        finally
        {
            fLoading = false;
        }
    }
    /// <summary>
    /// Saves the editor text to the note.
    /// </summary>
    void SaveNote()
    {
        if (Note == null)
            return;

        Note.Text = Editor.EditorText;
        Note.Save();
        Editor.Modified = false;
        AppHost.RemoveDirtyEditor(Editor);
        AdjustTitle();
        LogBox.AppendLine($"Note saved: {Note.Title}");
    }
    /// <summary>
    /// Updates the host title according to modified state.
    /// </summary>
    void AdjustTitle()
    {
        TitleText = Editor.Modified ? $"{fBaseTitle}*" : fBaseTitle;
    }
    /// <summary>
    /// Handles editor Save requests.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void Editor_SaveRequested(object Sender, EventArgs Args)
    {
        SaveNote();
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

    // ● overrides
    /// <summary>
    /// Sets up this form after its context is assigned.
    /// </summary>
    protected override void Setup()
    {
        Note = Context?.Tag as Note;
    }
    /// <summary>
    /// Called in order to initialize the form.
    /// </summary>
    protected override void FormInitialize()
    {
        Editor.SaveRequested += Editor_SaveRequested;
        Editor.ShowFolderRequested += Editor_ShowFolderRequested;
        Editor.ModifiedChanged += Editor_ModifiedChanged;
        LoadNote();
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
    /// Initializes a new instance of the NoteForm class.
    /// </summary>
    public NoteForm()
    {
        InitializeComponent();
    }

    // ● public
    /// <summary>
    /// Refreshes the form after the note title changes.
    /// </summary>
    public void RefreshNoteTitle()
    {
        if (Note == null)
            return;

        fBaseTitle = Note.DisplayTitle;
        Editor.Title = fBaseTitle;
        Editor.FilePath = Note.TextFilePath;
        AdjustTitle();
    }
    /// <summary>
    /// Highlights a search term in the note editor.
    /// </summary>
    /// <param name="LinkItem">The link item.</param>
    /// <param name="Term">The search term.</param>
    /// <param name="WholeWord">True for whole-word search.</param>
    /// <param name="MatchCase">True for case-sensitive search.</param>
    public void HighlightAll(LinkItem LinkItem, string Term, bool WholeWord, bool MatchCase)
    {
        if (LinkItem == null)
            return;

        Editor.HighlightSearchTerm(Term, WholeWord, MatchCase, LinkItem.Line, LinkItem.Column);
    }

    // ● properties
    /// <summary>
    /// Gets or sets the edited note.
    /// </summary>
    public Note Note
    {
        get => fNote;
        set => fNote = value;
    }
}
