// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Forms;

/// <summary>
/// Displays rendered Deltos documentation markdown.
/// </summary>
public partial class DocumentationForm: AppForm
{
    // ● private fields
    DocumentationFormData fPreviewData;
    Tripous.Desktop.ToolBar fToolBar;
    Button fOpenFolderButton;
    Button fCloseButton;

    // ● private
    /// <summary>
    /// Creates the form toolbar.
    /// </summary>
    void CreateToolBar()
    {
        if (fToolBar != null)
            return;

        fToolBar = new() { Panel = pnlToolBar };
        fOpenFolderButton = fToolBar.AddButton("folder_go.png", "Show Documentation Folder", AnyClick);
        fToolBar.AddSeparator();
        fCloseButton = fToolBar.AddButton("door_out.png", "Close", AnyClick);
    }
    /// <summary>
    /// Loads the preview data.
    /// </summary>
    void LoadPreview()
    {
        DocumentationFormData Data = PreviewData;
        TitleText = Data?.Title ?? "Documentation";
        MarkdownPreviewRenderer.Render(MarkdownPreviewPanel, Data?.MarkdownText);
    }

    // ● event handlers
    /// <summary>
    /// Handles toolbar clicks.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    async void AnyClick(object Sender, RoutedEventArgs Args)
    {
        if (Sender == fOpenFolderButton)
        {
            try
            {
                AppHost.Documentation.OpenFolder();
            }
            catch (Exception e)
            {
                await MessageBox.Error(e.Message, this);
            }
        }
        else if (Sender == fCloseButton)
        {
            CloseForm();
        }
    }

    // ● overrides
    /// <summary>
    /// Sets up this form after its context is assigned.
    /// </summary>
    protected override void Setup()
    {
        PreviewData = Context?.Tag as DocumentationFormData;
    }
    /// <summary>
    /// Initializes this form.
    /// </summary>
    protected override void FormInitialize()
    {
        CreateToolBar();
        LoadPreview();
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the DocumentationForm class.
    /// </summary>
    public DocumentationForm()
    {
        InitializeComponent();
    }

    // ● public
    /// <summary>
    /// Refreshes the rendered markdown preview.
    /// </summary>
    public void RefreshPreview()
    {
        LoadPreview();
    }

    // ● properties
    /// <summary>
    /// Gets or sets the preview data.
    /// </summary>
    public DocumentationFormData PreviewData
    {
        get => fPreviewData;
        set => fPreviewData = value;
    }
}

/// <summary>
/// Provides data to the documentation form.
/// </summary>
public class DocumentationFormData
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the DocumentationFormData class.
    /// </summary>
    public DocumentationFormData()
    {
    }

    // ● properties
    /// <summary>
    /// Gets or sets the preview title.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the markdown text.
    /// </summary>
    public string MarkdownText { get; set; } = string.Empty;
}
