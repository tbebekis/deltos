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
    static readonly List<Forms.TextEditorForm> fDirtyEditors = new();

    // ● private
    /// <summary>
    /// Saves dirty editors.
    /// </summary>
    static void AutoSaveProc()
    {
        List<Forms.TextEditorForm> Editors;
        lock (fAutoSaveLock)
        {
            Editors = fDirtyEditors.ToList();
            fDirtyEditors.Clear();
        }

        foreach (Forms.TextEditorForm Editor in Editors)
        {
            try
            {
                if (Editor.Modified)
                    Editor.SaveText();
            }
            catch (Exception e)
            {
                LogBox.AppendLine(e);
            }
        }
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
        if (Editor == null || AutoSaveService == null || !Editor.Modified)
            return;

        lock (fAutoSaveLock)
        {
            if (!fDirtyEditors.Contains(Editor))
                fDirtyEditors.Add(Editor);
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
            fDirtyEditors.Remove(Editor);
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
