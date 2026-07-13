// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents a document note with a title and a single markdown text.
/// </summary>
public class Note: BaseItem
{
    // ● protected
    /// <summary>
    /// Field for the Text property.
    /// </summary>
    protected string fText = string.Empty;

    // ● protected
    /// <summary>
    /// Checks whether the required note content files exist.
    /// </summary>
    protected virtual void CheckRequiredContentFiles()
    {
        if (!System.IO.File.Exists(TextFilePath))
            throw new InvalidOperationException($"The note content file does not exist: {TextFilePath}");
    }
    /// <summary>
    /// Checks whether the note storage folder contains only known content files.
    /// </summary>
    protected virtual void CheckStorageFolder()
    {
        string[] FolderPaths = System.IO.Directory.GetDirectories(FolderPath);
        if (FolderPaths.Length > 0)
            throw new InvalidOperationException($"Note storage folder contains child folders: {FolderPath}");

        HashSet<string> FileNames = new(StringComparer.OrdinalIgnoreCase);
        FileNames.Add(InfoFileName);
        FileNames.Add(TextFileName);

        foreach (string FilePath in System.IO.Directory.GetFiles(FolderPath))
        {
            string FileName = System.IO.Path.GetFileName(FilePath);
            if (!FileNames.Contains(FileName))
                throw new InvalidOperationException($"Note storage folder contains an unknown file: {FilePath}");
        }
    }
    /// <summary>
    /// Checks whether the note can be renamed.
    /// </summary>
    /// <param name="NewTitle">The new title.</param>
    protected override void CheckRenameTitle(string NewTitle)
    {
        Project ProjectItem = Parent as Project;
        if (ProjectItem != null)
            CheckDuplicateTitle(ProjectItem.Notes, NewTitle, this);
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the Note class.
    /// </summary>
    public Note()
    {
    }

    // ● public
    /// <summary>
    /// Saves the note to persistent storage.
    /// </summary>
    public override void Save()
    {
        base.Save();
        SaveMarkdownFile(TextFilePath, Text);
    }
    /// <summary>
    /// Loads the note from persistent storage.
    /// </summary>
    public override void Load()
    {
        base.Load();
        CheckStorageFolder();
        CheckRequiredContentFiles();
        Text = LoadMarkdownFile(TextFilePath);
    }
    /// <summary>
    /// Deletes the note from persistent storage.
    /// </summary>
    public override void Delete()
    {
        if (!CanDelete())
            throw new InvalidOperationException("This note cannot be deleted.");

        string ItemFolderPath = FolderPath;
        DeleteStorage(ItemFolderPath);

        Project ProjectItem = Parent as Project;
        if (ProjectItem != null)
            ProjectItem.DetachNote(this);
    }
    /// <summary>
    /// Returns true if the note can move in the specified direction.
    /// </summary>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <returns>True if the note can move; otherwise false.</returns>
    public override bool CanMove(bool Up)
    {
        Project ProjectItem = Parent as Project;
        return ProjectItem != null && ProjectItem.CanContainNotes && CanMoveItem(ProjectItem.Notes, this, Up);
    }
    /// <summary>
    /// Moves the note one step in the specified direction.
    /// </summary>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <returns>True if the note is moved; otherwise false.</returns>
    public override bool Move(bool Up)
    {
        Project ProjectItem = Parent as Project;
        if (ProjectItem == null || !CanMove(Up))
            return false;

        bool Result = MoveItem(ProjectItem.Notes, this, Up);
        if (Result)
            ProjectItem.UpdateReferences(null);

        return Result;
    }

    // ● properties
    /// <summary>
    /// Gets the item type.
    /// </summary>
    public override ItemType Type => ItemType.Note;
    /// <summary>
    /// Gets the note text file name.
    /// </summary>
    static public string TextFileName => "Text.md";
    /// <summary>
    /// Gets or sets the note title.
    /// </summary>
    public override string Title
    {
        get => base.Title;
        set => base.Title = value;
    }
    /// <summary>
    /// Gets the note display title.
    /// </summary>
    public override string DisplayTitle => base.DisplayTitle;
    /// <summary>
    /// Gets the file-system folder path of the note.
    /// </summary>
    [JsonIgnore]
    public override string FolderPath
    {
        get
        {
            Project ProjectItem = Parent as Project;
            if (ProjectItem != null)
                return System.IO.Path.Combine(ProjectItem.NotesFolderPath, StorageName);

            return base.FolderPath;
        }
    }
    /// <summary>
    /// Gets the file-system path of the note text file.
    /// </summary>
    [JsonIgnore]
    public string TextFilePath => System.IO.Path.Combine(FolderPath, TextFileName);
    /// <summary>
    /// Gets or sets the note text.
    /// </summary>
    public string Text
    {
        get => fText;
        set => fText = value ?? string.Empty;
    }
}
