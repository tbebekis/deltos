// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Application auto-save support.
/// </summary>
static public partial class AppHost
{
    // ● private fields
    /// <summary>
    /// Synchronizes dirty editor access.
    /// </summary>
    static readonly object fAutoSaveLock = new();
    /// <summary>
    /// The dirty text editors.
    /// </summary>
    static readonly List<AutoSaveEntry> fDirtyEditors = new();

    // ● private
    /// <summary>
    /// Holds a dirty editor and the model update action used before creating an auto-save snapshot.
    /// </summary>
    class AutoSaveEntry
    {
        // ● construction
        /// <summary>
        /// Initializes a new instance of the AutoSaveEntry class.
        /// </summary>
        /// <param name="Editor">The dirty editor.</param>
        /// <param name="ApplyTextProc">The model update action.</param>
        public AutoSaveEntry(Forms.TextEditorForm Editor, Action<Forms.TextEditorForm> ApplyTextProc)
        {
            this.Editor = Editor;
            this.ApplyTextProc = ApplyTextProc;
        }

        // ● properties
        /// <summary>
        /// Gets the dirty editor.
        /// </summary>
        public Forms.TextEditorForm Editor { get; }
        /// <summary>
        /// Gets the model update action.
        /// </summary>
        public Action<Forms.TextEditorForm> ApplyTextProc { get; }
    }
    /// <summary>
    /// Holds immutable editor text captured before background auto-save starts.
    /// </summary>
    class AutoSaveSnapshot
    {
        // ● construction
        /// <summary>
        /// Initializes a new instance of the AutoSaveSnapshot class.
        /// </summary>
        /// <param name="Editor">The dirty editor.</param>
        /// <param name="FilePath">The file path.</param>
        /// <param name="Text">The captured text.</param>
        /// <param name="ApplyTextProc">The model update action.</param>
        public AutoSaveSnapshot(Forms.TextEditorForm Editor, string FilePath, string Text, Action<Forms.TextEditorForm> ApplyTextProc)
        {
            this.Editor = Editor;
            this.FilePath = FilePath ?? string.Empty;
            this.Text = Text ?? string.Empty;
            this.ApplyTextProc = ApplyTextProc;
        }

        // ● properties
        /// <summary>
        /// Gets the dirty editor.
        /// </summary>
        public Forms.TextEditorForm Editor { get; }
        /// <summary>
        /// Gets the file path.
        /// </summary>
        public string FilePath { get; }
        /// <summary>
        /// Gets the captured text.
        /// </summary>
        public string Text { get; }
        /// <summary>
        /// Gets the model update action.
        /// </summary>
        public Action<Forms.TextEditorForm> ApplyTextProc { get; }
        /// <summary>
        /// Gets or sets the temporary file path created by background auto-save.
        /// </summary>
        public string TempFilePath { get; set; } = string.Empty;
    }
    /// <summary>
    /// Saves dirty editors.
    /// </summary>
    static async Task AutoSaveProc()
    {
        List<AutoSaveEntry> Entries;
        lock (fAutoSaveLock)
        {
            Entries = fDirtyEditors.ToList();
            fDirtyEditors.Clear();
        }

        List<AutoSaveSnapshot> Snapshots = CreateAutoSaveSnapshots(Entries);
        if (Snapshots.Count == 0)
            return;

        try
        {
            await Task.Run(() => SaveAutoSaveSnapshots(Snapshots));
        }
        catch
        {
            foreach (AutoSaveSnapshot Snapshot in Snapshots)
            {
                DeleteAutoSaveTempFile(Snapshot.TempFilePath);
                AddDirtyEditor(Snapshot.Editor, Snapshot.ApplyTextProc);
            }

            throw;
        }

        CompleteAutoSaveSnapshots(Snapshots);

        NotifyDocumentMetricsChanged();
    }
    /// <summary>
    /// Creates immutable auto-save snapshots from dirty editors.
    /// </summary>
    /// <param name="Entries">The dirty editor entries.</param>
    /// <returns>The created snapshots.</returns>
    static List<AutoSaveSnapshot> CreateAutoSaveSnapshots(List<AutoSaveEntry> Entries)
    {
        List<AutoSaveSnapshot> Result = new();
        foreach (AutoSaveEntry Entry in Entries)
        {
            try
            {
                if (Entry.Editor == null || !Entry.Editor.Modified)
                    continue;

                string FilePath = Entry.Editor.FilePath;
                if (string.IsNullOrWhiteSpace(FilePath))
                {
                    AddDirtyEditor(Entry.Editor, Entry.ApplyTextProc);
                    continue;
                }

                Entry.ApplyTextProc?.Invoke(Entry.Editor);
                Result.Add(new AutoSaveSnapshot(Entry.Editor, FilePath, Entry.Editor.EditorText, Entry.ApplyTextProc));
            }
            catch (Exception e)
            {
                LogBox.AppendLine(e);
                AddDirtyEditor(Entry.Editor, Entry.ApplyTextProc);
            }
        }

        return Result;
    }
    /// <summary>
    /// Saves auto-save snapshots on a background thread.
    /// </summary>
    /// <param name="Snapshots">The snapshots to save.</param>
    static void SaveAutoSaveSnapshots(List<AutoSaveSnapshot> Snapshots)
    {
        foreach (AutoSaveSnapshot Snapshot in Snapshots)
            Snapshot.TempFilePath = WriteAutoSaveTempFile(Snapshot.FilePath, Snapshot.Text);
    }
    /// <summary>
    /// Completes auto-save snapshots whose editor text has not changed since the background write started.
    /// </summary>
    /// <param name="Snapshots">The snapshots to complete.</param>
    static void CompleteAutoSaveSnapshots(List<AutoSaveSnapshot> Snapshots)
    {
        foreach (AutoSaveSnapshot Snapshot in Snapshots)
        {
            try
            {
                if (Snapshot.Editor.FilePath == Snapshot.FilePath && Snapshot.Editor.EditorText == Snapshot.Text)
                {
                    System.IO.File.Move(Snapshot.TempFilePath, Snapshot.FilePath, true);
                    Snapshot.Editor.Modified = false;
                    RemoveDirtyEditor(Snapshot.Editor);
                }
                else
                {
                    DeleteAutoSaveTempFile(Snapshot.TempFilePath);
                    AddDirtyEditor(Snapshot.Editor, Snapshot.ApplyTextProc);
                }
            }
            catch (Exception e)
            {
                DeleteAutoSaveTempFile(Snapshot.TempFilePath);
                LogBox.AppendLine(e);
                AddDirtyEditor(Snapshot.Editor, Snapshot.ApplyTextProc);
            }
        }
    }
    /// <summary>
    /// Writes a temporary auto-save file without replacing the target file.
    /// </summary>
    /// <param name="FilePath">The target file path.</param>
    /// <param name="Text">The text to save.</param>
    /// <returns>The temporary file path.</returns>
    static string WriteAutoSaveTempFile(string FilePath, string Text)
    {
        string FolderPath = System.IO.Path.GetDirectoryName(FilePath);
        if (string.IsNullOrWhiteSpace(FolderPath))
            throw new InvalidOperationException($"Invalid auto-save file path: {FilePath}");

        if (!System.IO.Directory.Exists(FolderPath))
            throw new InvalidOperationException($"Auto-save folder does not exist: {FolderPath}");

        string TempFilePath = System.IO.Path.Combine(FolderPath, $".autosave-{Guid.NewGuid():N}.tmp");
        try
        {
            System.IO.File.WriteAllText(TempFilePath, Text ?? string.Empty);
            return TempFilePath;
        }
        catch
        {
            DeleteAutoSaveTempFile(TempFilePath);
            throw;
        }
    }
    /// <summary>
    /// Deletes a temporary auto-save file if it exists.
    /// </summary>
    /// <param name="FilePath">The temporary file path.</param>
    static void DeleteAutoSaveTempFile(string FilePath)
    {
        if (!string.IsNullOrWhiteSpace(FilePath) && System.IO.File.Exists(FilePath))
            System.IO.File.Delete(FilePath);
    }
    /// <summary>
    /// Initializes auto-save.
    /// </summary>
    static void InitializeAutoSave()
    {
        AutoSaveService = new AutoSaveService(AutoSaveProc);
        AutoSaveService.ApplySettings(Settings);
        AutoSaveService.Saved += AutoSaveService_Saved;
    }
    /// <summary>
    /// Handles auto-save completion.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Time">The saved time.</param>
    static void AutoSaveService_Saved(object Sender, DateTime Time)
    {
        LogBox.AppendLine($"Auto-save completed: {Time:HH:mm:ss}");
    }

    // ● public
    /// <summary>
    /// Applies the current auto-save settings.
    /// </summary>
    static public void ApplyAutoSaveSettings()
    {
        AutoSaveService?.ApplySettings(Settings);
    }
    /// <summary>
    /// Adds a dirty editor to the auto-save list.
    /// </summary>
    /// <param name="Editor">The dirty editor.</param>
    static public void AddDirtyEditor(Forms.TextEditorForm Editor)
    {
        AddDirtyEditor(Editor, null);
    }
    /// <summary>
    /// Adds a dirty editor to the auto-save list.
    /// </summary>
    /// <param name="Editor">The dirty editor.</param>
    /// <param name="ApplyTextProc">The model update action.</param>
    static public void AddDirtyEditor(Forms.TextEditorForm Editor, Action<Forms.TextEditorForm> ApplyTextProc)
    {
        if (Editor == null || AutoSaveService == null || !Editor.Modified)
            return;

        lock (fAutoSaveLock)
        {
            AutoSaveEntry Entry = fDirtyEditors.FirstOrDefault(Item => Item.Editor == Editor);
            if (Entry != null)
                fDirtyEditors.Remove(Entry);

            fDirtyEditors.Add(new AutoSaveEntry(Editor, ApplyTextProc));
        }

        AutoSaveService.MarkAsDirty();
    }
    /// <summary>
    /// Removes an editor from the auto-save list.
    /// </summary>
    /// <param name="Editor">The editor.</param>
    static public void RemoveDirtyEditor(Forms.TextEditorForm Editor)
    {
        if (Editor == null)
            return;

        lock (fAutoSaveLock)
            fDirtyEditors.RemoveAll(Item => Item.Editor == Editor);
    }
    /// <summary>
    /// Clears all dirty editors.
    /// </summary>
    static public void ClearDirtyEditors()
    {
        lock (fAutoSaveLock)
            fDirtyEditors.Clear();
    }

    // ● properties
    /// <summary>
    /// Gets the auto-save service.
    /// </summary>
    static public AutoSaveService AutoSaveService { get; private set; }
}
