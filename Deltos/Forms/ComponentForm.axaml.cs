// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Forms;

/// <summary>
/// Edits component markdown text.
/// </summary>
public partial class ComponentForm: AppForm
{
    // ● private fields
    /// <summary>
    /// Field for the Component property.
    /// </summary>
    Component fComponent;
    /// <summary>
    /// True while values are loaded into controls.
    /// </summary>
    bool fLoading;
    /// <summary>
    /// The base tab title without modified marker.
    /// </summary>
    string fBaseTitle = "Component";

    // ● private
    /// <summary>
    /// Wires editor events.
    /// </summary>
    void WireEditors()
    {
        foreach (TextEditorForm Editor in Editors)
        {
            Editor.SaveRequested += Editor_SaveRequested;
            Editor.ShowFolderRequested += Editor_ShowFolderRequested;
            Editor.ModifiedChanged += Editor_ModifiedChanged;
        }
    }
    /// <summary>
    /// Loads the component into the editors.
    /// </summary>
    void LoadComponent()
    {
        ApplySettings();
        fLoading = true;
        try
        {
            if (Component == null)
            {
                fBaseTitle = "Component";
                TitleText = fBaseTitle;
                foreach (TextEditorForm Editor in Editors)
                {
                    Editor.EditorText = string.Empty;
                    Editor.FilePath = string.Empty;
                    Editor.ReadOnly = true;
                }
                return;
            }

            fBaseTitle = Component.Title;
            TitleText = fBaseTitle;

            EditorText.Title = Component.Title;
            EditorText.EditorText = Component.Text;
            EditorText.FilePath = Component.TextFilePath;

            EditorText2.Title = Component.Title;
            EditorText2.EditorText = Component.Text2;
            EditorText2.FilePath = Component.Text2FilePath;

            foreach (TextEditorForm Editor in Editors)
            {
                Editor.ApplyAppSettings();
                Editor.ReadOnly = false;
                Editor.Modified = false;
                Editor.RegisterHighlighter(Editor.FilePath);
            }
        }
        finally
        {
            fLoading = false;
        }
    }
    /// <summary>
    /// Saves all editor values to the component.
    /// </summary>
    void SaveComponent()
    {
        if (Component == null)
            return;

        Component.Text = EditorText.EditorText;
        Component.Text2 = EditorText2.EditorText;
        Component.Save();

        foreach (TextEditorForm Editor in Editors)
            Editor.Modified = false;
        foreach (TextEditorForm Editor in Editors)
            AppHost.RemoveDirtyEditor(Editor);

        AdjustTitle();
        LogBox.AppendLine($"Component saved: {Component.Title}");
    }
    /// <summary>
    /// Updates the host title according to modified state.
    /// </summary>
    void AdjustTitle()
    {
        TitleText = IsModified ? $"{fBaseTitle}*" : fBaseTitle;
    }
    /// <summary>
    /// Applies application settings to the form.
    /// </summary>
    void ApplySettings()
    {
        bool Visible = AppHost.Settings?.SecondLanguageVisible == true;
        EditorText2.IsVisible = Visible;
        Text2Splitter.IsVisible = Visible;
        EditorGrid.ColumnDefinitions[1].Width = Visible ? GridLength.Auto : new GridLength(0);
        EditorGrid.ColumnDefinitions[2].Width = Visible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
    }
    /// <summary>
    /// Handles editor Save requests.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void Editor_SaveRequested(object Sender, EventArgs Args)
    {
        SaveComponent();
    }
    /// <summary>
    /// Handles editor Show Folder requests.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void Editor_ShowFolderRequested(object Sender, EventArgs Args)
    {
        TextEditorForm Editor = Sender as TextEditorForm;
        if (Editor == null || string.IsNullOrWhiteSpace(Editor.FilePath))
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

        if (Sender is TextEditorForm Editor && Editor.Modified)
            AppHost.AddDirtyEditor(Editor);

        AdjustTitle();
    }

    // ● overrides
    /// <summary>
    /// Sets up this form after its context is assigned.
    /// </summary>
    protected override void Setup()
    {
        Component = Context?.Tag as Component;
    }
    /// <summary>
    /// Called in order to initialize the form.
    /// </summary>
    protected override void FormInitialize()
    {
        WireEditors();
        LoadComponent();
    }
    /// <summary>
    /// Called just before the form is closed.
    /// </summary>
    protected override void Closing()
    {
        foreach (TextEditorForm Editor in Editors)
        {
            Editor.SaveRequested -= Editor_SaveRequested;
            Editor.ShowFolderRequested -= Editor_ShowFolderRequested;
            Editor.ModifiedChanged -= Editor_ModifiedChanged;
            AppHost.RemoveDirtyEditor(Editor);
        }

        base.Closing();
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the ComponentForm class.
    /// </summary>
    public ComponentForm()
    {
        InitializeComponent();
    }

    // ● public
    /// <summary>
    /// Refreshes title and file path values after component metadata changes.
    /// </summary>
    public void RefreshComponentInfo()
    {
        if (Component == null)
            return;

        fBaseTitle = Component.Title;
        EditorText.Title = Component.Title;
        EditorText.FilePath = Component.TextFilePath;
        EditorText2.Title = Component.Title;
        EditorText2.FilePath = Component.Text2FilePath;
        AdjustTitle();
    }

    // ● properties
    /// <summary>
    /// Gets the text editors.
    /// </summary>
    TextEditorForm[] Editors => new[] { EditorText, EditorText2 };
    /// <summary>
    /// Gets or sets the edited component.
    /// </summary>
    public Component Component
    {
        get => fComponent;
        set => fComponent = value;
    }
    /// <summary>
    /// Gets a value indicating whether any editor is modified.
    /// </summary>
    public bool IsModified => Editors.Any(Editor => Editor.Modified);
}
