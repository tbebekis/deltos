// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Edits the folder level structure of a new document.
/// </summary>
public partial class DocumentStructureDialog: DialogWindow
{
    // ● private fields
    /// <summary>
    /// True while the dialog updates controls internally.
    /// </summary>
    private bool fUpdatingControls;
    /// <summary>
    /// The root document tree node.
    /// </summary>
    private TreeViewItem fDocumentNode;

    // ● private
    /// <summary>
    /// Creates the template choices.
    /// </summary>
    private void CreateTemplates()
    {
        fUpdatingControls = true;
        try
        {
            cboTemplates.ItemsSource = new[]
            {
                "Document",
                "Document / Part / Chapter / Section",
                "Document / Part / Chapter",
                "Document / Part / Chapter / Scene",
                "Document / Chapter / Scene"
            };
            cboTemplates.SelectedIndex = 0;
        }
        finally
        {
            fUpdatingControls = false;
        }
    }
    /// <summary>
    /// Applies a template selection to the structure tree.
    /// </summary>
    /// <param name="TemplateIndex">The template index.</param>
    private void ApplyTemplate(int TemplateIndex)
    {
        string[] Levels = TemplateIndex switch
        {
            1 => new[] { "Part", "Chapter", "Section" },
            2 => new[] { "Part", "Chapter" },
            3 => new[] { "Part", "Chapter", "Scene" },
            4 => new[] { "Chapter", "Scene" },
            _ => Array.Empty<string>()
        };

        CreateTree(Levels);
    }
    /// <summary>
    /// Creates the structure tree from level titles.
    /// </summary>
    /// <param name="Levels">The folder level titles.</param>
    private void CreateTree(IEnumerable<string> Levels)
    {
        tvStructure.Items.Clear();
        fDocumentNode = CreateDocumentNode();
        tvStructure.Items.Add(fDocumentNode);

        TreeViewItem ParentNode = fDocumentNode;
        foreach (string Level in Levels)
        {
            TreeViewItem Node = CreateFolderNode(Level);
            ParentNode.Items.Add(Node);
            ParentNode.IsExpanded = true;
            ParentNode = Node;
        }

        fDocumentNode.IsExpanded = true;
        tvStructure.SelectedItem = fDocumentNode;
    }
    /// <summary>
    /// Creates the root document node.
    /// </summary>
    /// <returns>The created node.</returns>
    private TreeViewItem CreateDocumentNode()
    {
        return Ui.CreateContainerNode("Document", Tag: null, IconFile: "book.png", NegativeMargin: 10);
    }
    /// <summary>
    /// Creates a folder level node.
    /// </summary>
    /// <param name="Title">The folder level title.</param>
    /// <returns>The created node.</returns>
    private TreeViewItem CreateFolderNode(string Title)
    {
        return Ui.CreateContainerNode(Title, Tag: Title, IconFile: "folder.png", NegativeMargin: 10);
    }
    /// <summary>
    /// Returns the selected structure node.
    /// </summary>
    /// <returns>The selected node.</returns>
    private TreeViewItem GetSelectedNode()
    {
        return tvStructure.SelectedItem as TreeViewItem ?? fDocumentNode;
    }
    /// <summary>
    /// Finds the parent node of a child node.
    /// </summary>
    /// <param name="ParentNode">The parent node to search.</param>
    /// <param name="ChildNode">The child node to find.</param>
    /// <returns>The parent node, if found; otherwise null.</returns>
    private TreeViewItem FindParentNode(TreeViewItem ParentNode, TreeViewItem ChildNode)
    {
        foreach (object Item in ParentNode.Items)
        {
            if (ReferenceEquals(Item, ChildNode))
                return ParentNode;

            if (Item is TreeViewItem Node)
            {
                TreeViewItem Result = FindParentNode(Node, ChildNode);
                if (Result != null)
                    return Result;
            }
        }

        return null;
    }
    /// <summary>
    /// Builds a folder item chain from a tree node.
    /// </summary>
    /// <param name="Node">The tree node.</param>
    /// <returns>The folder item chain.</returns>
    private FolderItem BuildFolderItem(TreeViewItem Node)
    {
        if (Node == null)
            return null;

        string Title = Node.Tag as string;
        if (string.IsNullOrWhiteSpace(Title))
            return null;

        FolderItem Result = new FolderItem { Title = Title };
        if (Node.Items.Count > 0 && Node.Items[0] is TreeViewItem ChildNode)
            Result.Child = BuildFolderItem(ChildNode);

        return Result;
    }
    /// <summary>
    /// Handles template selection changes.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    private void TemplateSelectionChanged(object Sender, SelectionChangedEventArgs Args)
    {
        if (fUpdatingControls)
            return;

        ApplyTemplate(cboTemplates.SelectedIndex);
    }
    /// <summary>
    /// Handles the Add button click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    private async void Add_Click(object Sender, RoutedEventArgs Args)
    {
        TreeViewItem SelectedNode = GetSelectedNode();
        if (SelectedNode.Items.Count > 0)
        {
            await Tripous.Desktop.MessageBox.Info("The selected level already has a child level.", this);
            return;
        }

        InputBoxData BoxData = await InputBox.ShowModal("Folder level title", "Chapter", this);
        if (!BoxData.Result)
            return;

        string Title = BoxData.Value?.Trim();
        if (!AppHost.IsValidFolderLevelTitle(Title, false))
        {
            await Tripous.Desktop.MessageBox.Error($"Invalid folder level title: {Title}", this);
            return;
        }

        TreeViewItem Node = CreateFolderNode(Title);
        SelectedNode.Items.Add(Node);
        SelectedNode.IsExpanded = true;
        tvStructure.SelectedItem = Node;
    }
    /// <summary>
    /// Handles the Remove button click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    private async void Remove_Click(object Sender, RoutedEventArgs Args)
    {
        TreeViewItem SelectedNode = GetSelectedNode();
        if (ReferenceEquals(SelectedNode, fDocumentNode))
        {
            await Tripous.Desktop.MessageBox.Info("The Document node cannot be removed.", this);
            return;
        }

        TreeViewItem ParentNode = FindParentNode(fDocumentNode, SelectedNode);
        if (ParentNode == null)
            return;

        ParentNode.Items.Remove(SelectedNode);
        tvStructure.SelectedItem = ParentNode;
    }
    /// <summary>
    /// Handles the OK button click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    private void OK_Click(object Sender, RoutedEventArgs Args)
    {
        FolderItem Structure = null;
        if (fDocumentNode.Items.Count > 0 && fDocumentNode.Items[0] is TreeViewItem ChildNode)
            Structure = BuildFolderItem(ChildNode);

        Structure?.UpdateReferences(null);
        ResultData = Structure;
        ModalResult = ModalResult.Ok;
    }
    /// <summary>
    /// Handles the Cancel button click.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    private void Cancel_Click(object Sender, RoutedEventArgs Args)
    {
        ModalResult = ModalResult.Cancel;
    }

    // ● protected
    /// <summary>
    /// Initializes the window.
    /// </summary>
    protected override async Task WindowInitialize()
    {
        CreateTemplates();
        ApplyTemplate(0);
        await Task.CompletedTask;
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the DocumentStructureDialog class.
    /// </summary>
    public DocumentStructureDialog()
    {
        InitializeComponent();
    }
}
