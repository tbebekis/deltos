// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Forms;

/// <summary>
/// Displays and previews project notes.
/// </summary>
public partial class NoteListForm: AppForm
{
    // ● private fields
    /// <summary>
    /// The toolbar helper.
    /// </summary>
    Tripous.Desktop.ToolBar fToolBar;

    // ● toolbar
    /// <summary>
    /// Creates the form toolbar.
    /// </summary>
    void CreateToolBar()
    {
        fToolBar = new();
        fToolBar.Panel = pnlToolBar;

        fToolBar.AddButton("table_add.png", "New", async () => await NewNote());
        fToolBar.AddButton("table_edit.png", "Edit", async () => await EditNoteInfo());
        fToolBar.AddButton("table_delete.png", "Delete", async () => await DeleteNote());
        fToolBar.AddSeparator();
        fToolBar.AddButton("page_edit.png", "Edit Text", EditNoteText);
        fToolBar.AddButton("html.png", "HTML Preview", PreviewNoteText);
        fToolBar.AddButton("wishlist_add.png", "Quick View", QuickViewNote);
        fToolBar.AddSeparator();
        fToolBar.AddButton("arrow_up.png", "Up", async () => await MoveNote(true));
        fToolBar.AddButton("arrow_down.png", "Down", async () => await MoveNote(false));
    }

    // ● private
    /// <summary>
    /// Reloads the note list.
    /// </summary>
    void LoadNotes()
    {
        string SelectedId = SelectedNote?.Id;
        lboNotes.Items.Clear();

        Project Project = AppHost.CurrentProject;
        if (Project != null)
        {
            foreach (Note Note in Project.Notes)
                lboNotes.Items.Add(CreateNoteItem(Note));
        }

        SelectNote(SelectedId);

        if (lboNotes.SelectedItem == null && lboNotes.Items.Count > 0)
            lboNotes.SelectedIndex = 0;

        if (lboNotes.SelectedItem == null)
            ShowNoSelectedNote();
    }
    /// <summary>
    /// Creates a list box item for a note.
    /// </summary>
    /// <param name="Note">The note.</param>
    /// <returns>The created list box item.</returns>
    ListBoxItem CreateNoteItem(Note Note)
    {
        return new ListBoxItem
        {
            Content = Note.DisplayTitle,
            Tag = Note
        };
    }
    /// <summary>
    /// Selects a note by id.
    /// </summary>
    /// <param name="Id">The note id.</param>
    void SelectNote(string Id)
    {
        if (string.IsNullOrWhiteSpace(Id))
            return;

        foreach (object Item in lboNotes.Items)
        {
            if (Item is ListBoxItem ListItem && ListItem.Tag is Note Note && Note.Id.IsSameText(Id))
            {
                lboNotes.SelectedItem = ListItem;
                return;
            }
        }
    }
    /// <summary>
    /// Shows a note in the preview editor.
    /// </summary>
    /// <param name="Note">The note.</param>
    void ShowNote(Note Note)
    {
        if (Note == null)
        {
            ShowNoSelectedNote();
            return;
        }

        lblNoteTitle.Text = Note.Title;
        Editor.Title = Note.DisplayTitle;
        Editor.EditorText = Note.Text;
        Editor.FilePath = Note.TextFilePath;
        Editor.ReadOnly = true;
        Editor.Modified = false;
        Editor.RegisterHighlighter(Editor.FilePath);
    }
    /// <summary>
    /// Clears the note preview.
    /// </summary>
    void ShowNoSelectedNote()
    {
        lblNoteTitle.Text = "Note";
        Editor.Title = "Note";
        Editor.EditorText = string.Empty;
        Editor.FilePath = string.Empty;
        Editor.ReadOnly = true;
        Editor.Modified = false;
    }
    /// <summary>
    /// Creates a new note.
    /// </summary>
    async Task NewNote()
    {
        Project Project = AppHost.CurrentProject;
        if (Project == null)
        {
            await Tripous.Desktop.MessageBox.Info("No project is open.", this);
            return;
        }

        InputBoxData BoxData = await InputBox.ShowModal("Note title", string.Empty, this);
        if (!BoxData.Result)
            return;

        string Title = BoxData.Value?.Trim();
        if (!AppHost.IsValidFileName(Title, false))
        {
            await Tripous.Desktop.MessageBox.Error($"Invalid note title: {Title}", this);
            return;
        }

        try
        {
            Note Note = Project.AddNote(Title);
            LoadNotes();
            SelectNote(Note.Id);
            ShowNote(Note);
            LogBox.AppendLine($"Note created: {Note.Title}");
        }
        catch (Exception e)
        {
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
    }
    /// <summary>
    /// Edits the selected note title.
    /// </summary>
    async Task EditNoteInfo()
    {
        Note Note = SelectedNote;
        if (Note == null)
            return;

        InputBoxData BoxData = await InputBox.ShowModal("Note title", Note.Title, this);
        if (!BoxData.Result)
            return;

        string Title = BoxData.Value?.Trim();
        if (!AppHost.IsValidFileName(Title, false))
        {
            await Tripous.Desktop.MessageBox.Error($"Invalid note title: {Title}", this);
            return;
        }

        try
        {
            Note.Rename(Title);
            LoadNotes();
            SelectNote(Note.Id);
            ShowNote(Note);
            RefreshOpenNoteForm(Note);
            LogBox.AppendLine($"Note renamed: {Note.Title}");
        }
        catch (Exception e)
        {
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
    }
    /// <summary>
    /// Deletes the selected note.
    /// </summary>
    async Task DeleteNote()
    {
        Note Note = SelectedNote;
        if (Note == null)
            return;

        bool Confirmed = await Tripous.Desktop.MessageBox.YesNo($"Delete {Note.DisplayTitle}?", this);
        if (!Confirmed)
            return;

        try
        {
            string Title = Note.Title;
            AppHost.CloseContentFormForItem(Note);
            Note.Delete();
            LoadNotes();
            LogBox.AppendLine($"Note deleted: {Title}");
        }
        catch (Exception e)
        {
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
    }
    /// <summary>
    /// Opens the selected note text for editing.
    /// </summary>
    void EditNoteText()
    {
        Note Note = SelectedNote;
        if (Note == null)
            return;

        AppHost.ShowContentForm<NoteForm>(Note.Id, Note.DisplayTitle, Note);
    }
    /// <summary>
    /// Previews the selected note markdown text.
    /// </summary>
    void PreviewNoteText()
    {
        Note Note = SelectedNote;
        if (Note == null)
            return;

        AppHost.ShowMarkdownPreview($"{Note.Id}.HtmlPreview", $"HTML Preview: {Note.DisplayTitle}", Note.Text);
    }
    /// <summary>
    /// Placeholder for Quick View command.
    /// </summary>
    void QuickViewNote()
    {
        LogBox.AppendLine("Quick View command not implemented yet.");
    }
    /// <summary>
    /// Moves the selected note.
    /// </summary>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    async Task MoveNote(bool Up)
    {
        Project Project = AppHost.CurrentProject;
        Note Note = SelectedNote;
        if (Project == null || Note == null)
            return;

        int NewOrderIndex = Up ? Note.OrderIndex - 1 : Note.OrderIndex + 1;
        if (NewOrderIndex < 1 || NewOrderIndex > Project.Notes.Count)
            return;

        try
        {
            if (Project.MoveNote(Note, NewOrderIndex))
            {
                LoadNotes();
                SelectNote(Note.Id);
                ShowNote(Note);
                RefreshOpenNoteForm(Note);
                LogBox.AppendLine($"Note moved: {Note.DisplayTitle}");
            }
        }
        catch (Exception e)
        {
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
    }
    /// <summary>
    /// Refreshes an open note editor form.
    /// </summary>
    /// <param name="Note">The note.</param>
    void RefreshOpenNoteForm(Note Note)
    {
        if (Note == null || AppHost.ContentHandler == null)
            return;

        NoteForm Form = AppHost.ContentHandler.FindAppForm(Note.Id) as NoteForm;
        Form?.RefreshNoteTitle();
    }
    /// <summary>
    /// Handles selected note changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void NotesSelectionChanged(object Sender, SelectionChangedEventArgs Args)
    {
        ShowNote(SelectedNote);
    }
    /// <summary>
    /// Handles note list double-tap events.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void NotesDoubleTapped(object Sender, TappedEventArgs Args)
    {
        EditNoteText();
        Args.Handled = true;
    }

    // ● overrides
    /// <summary>
    /// Called in order to initialize the form.
    /// </summary>
    protected override void FormInitialize()
    {
        TitleText = "Notes";
        ClosableByUser = false;
        CreateToolBar();
        LoadNotes();
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the NoteListForm class.
    /// </summary>
    public NoteListForm()
    {
        InitializeComponent();
    }

    // ● properties
    /// <summary>
    /// Gets the selected note.
    /// </summary>
    Note SelectedNote
    {
        get
        {
            if (lboNotes.SelectedItem is ListBoxItem Item)
                return Item.Tag as Note;

            return null;
        }
    }
}
