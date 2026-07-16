// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents a sample project structure choice.
/// </summary>
public class SampleProjectChoice
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the SampleProjectChoice class.
    /// </summary>
    public SampleProjectChoice()
    {
    }

    // ● properties
    /// <summary>
    /// Gets or sets the sample project kind.
    /// </summary>
    public string Kind { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the choice title.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the choice description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Selects a sample project structure.
/// </summary>
public partial class SampleProjectDialog: DialogWindow
{
    // ● private
    /// <summary>
    /// Creates a sample project choice list item.
    /// </summary>
    /// <param name="Choice">The sample project choice.</param>
    /// <returns>The created list item.</returns>
    ListBoxItem CreateChoiceItem(SampleProjectChoice Choice)
    {
        TextBlock TitleBlock = new TextBlock();
        TitleBlock.Text = Choice.Title;
        TitleBlock.FontWeight = FontWeight.SemiBold;

        TextBlock DescriptionBlock = new TextBlock();
        DescriptionBlock.Text = Choice.Description;
        DescriptionBlock.TextWrapping = TextWrapping.Wrap;
        DescriptionBlock.Margin = new Thickness(0, 4, 0, 0);

        StackPanel Panel = new StackPanel();
        Panel.Margin = new Thickness(4);
        Panel.Children.Add(TitleBlock);
        Panel.Children.Add(DescriptionBlock);

        ListBoxItem Result = new ListBoxItem();
        Result.Content = Panel;
        Result.Tag = Choice;
        return Result;
    }
    /// <summary>
    /// Loads the sample choices.
    /// </summary>
    void LoadChoices()
    {
        lboChoices.Items.Clear();
        lboChoices.Items.Add(CreateChoiceItem(new SampleProjectChoice
        {
            Kind = AppHost.SampleProjectKindFlat,
            Title = "Document -> TextFile",
            Description = "A simple article or standalone text with text files directly under the document."
        }));
        lboChoices.Items.Add(CreateChoiceItem(new SampleProjectChoice
        {
            Kind = AppHost.SampleProjectKindChapter,
            Title = "Document -> Chapter -> TextFile",
            Description = "A chapter-based writing project with text files inside each chapter."
        }));
        lboChoices.Items.Add(CreateChoiceItem(new SampleProjectChoice
        {
            Kind = AppHost.SampleProjectKindPartChapter,
            Title = "Document -> Part -> Chapter -> TextFile",
            Description = "A larger structured project with parts, chapters, and text files."
        }));
        lboChoices.SelectedIndex = 0;
    }
    /// <summary>
    /// Selects the current choice and closes the dialog.
    /// </summary>
    void SelectCurrentChoice()
    {
        ListBoxItem Item = lboChoices.SelectedItem as ListBoxItem;
        if (Item == null)
            return;

        ResultData = Item.Tag;
        ModalResult = ModalResult.Ok;
    }
    /// <summary>
    /// Handles OK click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void OK_Click(object Sender, RoutedEventArgs Args)
    {
        SelectCurrentChoice();
    }
    /// <summary>
    /// Handles Cancel click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void Cancel_Click(object Sender, RoutedEventArgs Args)
    {
        ModalResult = ModalResult.Cancel;
    }
    /// <summary>
    /// Handles choice double-tap events.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    void ChoicesDoubleTapped(object Sender, TappedEventArgs Args)
    {
        SelectCurrentChoice();
        Args.Handled = true;
    }

    // ● protected
    /// <summary>
    /// Initializes the window.
    /// </summary>
    protected override async Task WindowInitialize()
    {
        LoadChoices();
        await Task.CompletedTask;
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the SampleProjectDialog class.
    /// </summary>
    public SampleProjectDialog()
    {
        InitializeComponent();
    }
}
