// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Forms;

/// <summary>
/// Displays a rendered markdown preview.
/// </summary>
public partial class MarkdownPreviewForm: AppForm
{
    // ● private fields
    /// <summary>
    /// Field for the PreviewData property.
    /// </summary>
    MarkdownPreviewFormData fPreviewData;

    // ● private
    /// <summary>
    /// Loads the preview data.
    /// </summary>
    void LoadPreview()
    {
        MarkdownPreviewFormData Data = PreviewData;
        TitleText = Data?.Title ?? "HTML Preview";
        MarkdownPreviewRenderer.Render(MarkdownPreviewPanel, Data?.MarkdownText);
    }

    // ● overrides
    /// <summary>
    /// Sets up this form after its context is assigned.
    /// </summary>
    protected override void Setup()
    {
        PreviewData = Context?.Tag as MarkdownPreviewFormData;
    }
    /// <summary>
    /// Called in order to initialize the form.
    /// </summary>
    protected override void FormInitialize()
    {
        LoadPreview();
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the MarkdownPreviewForm class.
    /// </summary>
    public MarkdownPreviewForm()
    {
        InitializeComponent();
    }

    // ● public
    /// <summary>
    /// Refreshes the preview content.
    /// </summary>
    public void RefreshPreview()
    {
        LoadPreview();
    }

    // ● properties
    /// <summary>
    /// Gets or sets the preview data.
    /// </summary>
    public MarkdownPreviewFormData PreviewData
    {
        get => fPreviewData;
        set => fPreviewData = value;
    }
}

/// <summary>
/// Provides data to a markdown preview form.
/// </summary>
public class MarkdownPreviewFormData
{
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
