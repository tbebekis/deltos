// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Executes a save action when auto-save is enabled and dirty content exists.
/// </summary>
public class AutoSaveService
{
    // ● private fields
    /// <summary>
    /// Synchronizes access to service state.
    /// </summary>
    readonly object fLock = new();
    /// <summary>
    /// The timer that executes auto-save.
    /// </summary>
    readonly Avalonia.Threading.DispatcherTimer fTimer;
    /// <summary>
    /// The save action.
    /// </summary>
    readonly Action fSaveProc;
    /// <summary>
    /// True when there is dirty content to save.
    /// </summary>
    bool fIsDirty;
    /// <summary>
    /// True while the save action is executing.
    /// </summary>
    bool fIsSaving;

    // ● private
    /// <summary>
    /// Executes the save action when needed.
    /// </summary>
    void Execute()
    {
        lock (fLock)
        {
            if (!Enabled || !fIsDirty || fIsSaving)
                return;

            fIsSaving = true;
        }

        try
        {
            fSaveProc?.Invoke();

            lock (fLock)
                fIsDirty = false;

            Saved?.Invoke(this, DateTime.Now);
        }
        catch (Exception e)
        {
            LogBox.AppendLine(e);
        }
        finally
        {
            lock (fLock)
                fIsSaving = false;
        }
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the AutoSaveService class.
    /// </summary>
    /// <param name="SaveProc">The save action.</param>
    public AutoSaveService(Action SaveProc)
    {
        fSaveProc = SaveProc;
        fTimer = new Avalonia.Threading.DispatcherTimer();
        fTimer.Tick += (Sender, Args) => Execute();
    }

    // ● public
    /// <summary>
    /// Marks the service as having dirty content.
    /// </summary>
    public void MarkAsDirty()
    {
        lock (fLock)
            fIsDirty = true;
    }
    /// <summary>
    /// Applies settings to the service.
    /// </summary>
    /// <param name="Settings">The application settings.</param>
    public void ApplySettings(AppSettings Settings)
    {
        if (Settings == null)
            return;

        AutoSaveSecondsInterval = Settings.AutoSaveSecondsInterval;
        Enabled = Settings.AutoSave;
    }

    // ● properties
    /// <summary>
    /// Gets or sets a value indicating whether auto-save is enabled.
    /// </summary>
    public bool Enabled
    {
        get => fTimer.IsEnabled;
        set
        {
            if (value)
                fTimer.Start();
            else
                fTimer.Stop();
        }
    }
    /// <summary>
    /// Gets or sets the auto-save interval in seconds.
    /// </summary>
    public int AutoSaveSecondsInterval
    {
        get => (int)fTimer.Interval.TotalSeconds;
        set => fTimer.Interval = TimeSpan.FromSeconds(Math.Max(1, value));
    }

    // ● events
    /// <summary>
    /// Occurs after the save action is called.
    /// </summary>
    public event EventHandler<DateTime> Saved;
}
