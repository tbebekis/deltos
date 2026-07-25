// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Application UI and AppForm support.
/// </summary>
static public partial class AppHost
{
    // ● public
    /// <summary>
    /// Initializes application UI handlers.
    /// </summary>
    /// <param name="SideBarHandler">The sidebar pager handler.</param>
    /// <param name="ContentHandler">The content pager handler.</param>
    static public void InitializeUi(AppFormPagerHandler SideBarHandler, AppFormPagerHandler ContentHandler)
    {
        AppHost.SideBarHandler = SideBarHandler;
        AppHost.ContentHandler = ContentHandler;
    }

    /// <summary>
    /// Closes all AppForms in both main pagers.
    /// </summary>
    static public void CloseAllUi()
    {
        SideBarHandler?.Pager.Items.Clear();
        ContentHandler?.Pager.Items.Clear();
    }

    /// <summary>
    /// Shows an AppForm in the sidebar pager.
    /// </summary>
    /// <typeparam name="T">The AppForm type.</typeparam>
    /// <param name="FormId">The form identifier.</param>
    /// <param name="Title">The form title.</param>
    /// <param name="Tag">Optional user data.</param>
    /// <returns>The shown AppForm.</returns>
    static public T ShowSideBarForm<T>(string FormId = null, string Title = null, object Tag = null)
        where T: AppForm
    {
        return ShowAppForm<T>(SideBarHandler, FormId, Title, Tag);
    }

    /// <summary>
    /// Shows an AppForm in the content pager.
    /// </summary>
    /// <typeparam name="T">The AppForm type.</typeparam>
    /// <param name="FormId">The form identifier.</param>
    /// <param name="Title">The form title.</param>
    /// <param name="Tag">Optional user data.</param>
    /// <returns>The shown AppForm.</returns>
    static public T ShowContentForm<T>(string FormId = null, string Title = null, object Tag = null)
        where T: AppForm
    {
        return ShowAppForm<T>(ContentHandler, FormId, Title, Tag);
    }

    /// <summary>
    /// Shows an AppForm in a pager.
    /// </summary>
    /// <typeparam name="T">The AppForm type.</typeparam>
    /// <param name="Handler">The pager handler.</param>
    /// <param name="FormId">The form identifier.</param>
    /// <param name="Title">The form title.</param>
    /// <param name="Tag">Optional user data.</param>
    /// <returns>The shown AppForm.</returns>
    static public T ShowAppForm<T>(AppFormPagerHandler Handler, string FormId = null, string Title = null, object Tag = null)
        where T: AppForm
    {
        if (Handler == null)
            throw new InvalidOperationException("The AppForm pager handler is not initialized.");

        Type FormType = typeof(T);
        string ClassName = FormType.FullName;
        FormContext Context = FormContext.Create(FormId ?? ClassName, ClassName, FormDisplayMode.TabItem, MainWindow, Tag);

        if (!string.IsNullOrWhiteSpace(Title))
            Context.Title = Title;

        return Handler.ShowAppForm(Context) as T;
    }
    /// <summary>
    /// Notifies an open content form that an item title has changed.
    /// </summary>
    /// <param name="Item">The renamed item.</param>
    static public void NotifyItemTitleChanged(BaseItem Item)
    {
        if (Item == null || ContentHandler == null)
            return;

        Forms.TextFileForm Form = ContentHandler.FindAppForm(Item.Id) as Forms.TextFileForm;
        Form?.RefreshItemTitle();
    }
    /// <summary>
    /// Notifies the document list form that document metrics have changed.
    /// </summary>
    static public void NotifyDocumentMetricsChanged()
    {
        if (SideBarHandler == null)
            return;

        Forms.DocumentListForm Form = SideBarHandler.FindAppForm(nameof(Forms.DocumentListForm)) as Forms.DocumentListForm;
        Form?.RefreshMetrics();
    }
    /// <summary>
    /// Closes the open content form for an item.
    /// </summary>
    /// <param name="Item">The item.</param>
    static public void CloseContentFormForItem(BaseItem Item)
    {
        if (Item == null || ContentHandler == null)
            return;

        foreach (BaseItem ChildItem in Item.GetDescendantItems())
            ContentHandler.CloseForm(ChildItem.Id);

        ContentHandler.CloseForm(Item.Id);
    }

    /// <summary>
    /// Shows the default sidebar AppForms for the current project.
    /// </summary>
    static public void ShowSideBarForms()
    {
        if (CurrentProject == null || SideBarHandler == null)
            return;

        ShowSideBarForm<Forms.TagListForm>(nameof(Forms.TagListForm), "Tags");
        ShowSideBarForm<Forms.ComponentListForm>(nameof(Forms.ComponentListForm), "Components");
        ShowSideBarForm<Forms.GlobalSearchForm>(nameof(Forms.GlobalSearchForm), "Search");
        ShowSideBarForm<Forms.QuickViewForm>(nameof(Forms.QuickViewForm), "Quick View");
        ShowSideBarForm<Forms.NoteListForm>(nameof(Forms.NoteListForm), "Notes");
        ShowSideBarForm<Forms.TempFileForm>(nameof(Forms.TempFileForm), "Temp");
        ShowSideBarForm<Forms.DocumentListForm>(nameof(Forms.DocumentListForm), "Documents");
    }
    /// <summary>
    /// Shows the page represented by a link item.
    /// </summary>
    /// <param name="LinkItem">The link item.</param>
    /// <returns>The shown form, if any; otherwise null.</returns>
    static public AppForm ShowLinkItemPage(LinkItem LinkItem)
    {
        if (LinkItem == null || LinkItem.Item == null)
            return null;

        if (LinkItem.Item is Component Component)
            return ShowContentForm<Forms.ComponentForm>(Component.Id, Component.Title, Component);
        if (LinkItem.Item is Note Note)
            return ShowContentForm<Forms.NoteForm>(Note.Id, Note.Title, Note);
        if (LinkItem.Item is Document || LinkItem.Item is Folder || LinkItem.Item is TextFile)
            return ShowContentForm<Forms.TextFileForm>(LinkItem.Item.Id, LinkItem.Item.DisplayTitle, LinkItem.Item);
        if (LinkItem.Place == LinkPlace.TempFile)
            return ShowSideBarForm<Forms.TempFileForm>(nameof(Forms.TempFileForm), "Temp");

        return null;
    }
    /// <summary>
    /// Shows the list page represented by a link item.
    /// </summary>
    /// <param name="LinkItem">The link item.</param>
    static public void ShowItemInListPage(LinkItem LinkItem)
    {
        if (LinkItem == null || LinkItem.Item == null)
            return;

        if (LinkItem.Item is Component Component)
        {
            Forms.ComponentListForm Form = ShowSideBarForm<Forms.ComponentListForm>(nameof(Forms.ComponentListForm), "Components");
            Form.ShowComponentInList(Component);
        }
        else if (LinkItem.Item is Note Note)
        {
            Forms.NoteListForm Form = ShowSideBarForm<Forms.NoteListForm>(nameof(Forms.NoteListForm), "Notes");
            Form.ShowNoteInList(Note);
        }
        else if (LinkItem.Item is Document || LinkItem.Item is Folder || LinkItem.Item is TextFile)
        {
            Forms.DocumentListForm Form = ShowSideBarForm<Forms.DocumentListForm>(nameof(Forms.DocumentListForm), "Documents");
            Form.ShowItemInList(LinkItem.Item);
        }
        else if (LinkItem.Place == LinkPlace.TempFile)
        {
            ShowSideBarForm<Forms.TempFileForm>(nameof(Forms.TempFileForm), "Temp");
        }
    }
    /// <summary>
    /// Adds a link item to QuickView.
    /// </summary>
    /// <param name="LinkItem">The link item.</param>
    static public void AddToQuickView(LinkItem LinkItem)
    {
        Project Project = CurrentProject;
        if (Project == null || LinkItem == null || LinkItem.Item == null)
            return;

        Forms.QuickViewForm Form = ShowSideBarForm<Forms.QuickViewForm>(nameof(Forms.QuickViewForm), "Quick View");
        Form.AddToQuickView(LinkItem);
    }
    /// <summary>
    /// Returns the text represented by a link item.
    /// </summary>
    /// <param name="LinkItem">The link item.</param>
    /// <returns>The link text.</returns>
    static public string GetLinkItemText(LinkItem LinkItem)
    {
        if (LinkItem == null || LinkItem.Item == null)
            return string.Empty;

        if (LinkItem.Place == LinkPlace.Title)
            return LinkItem.Item.Title;
        if (LinkItem.Place == LinkPlace.TempFile && LinkItem.Item is Project Project)
            return Project.TempFileText;
        if (LinkItem.Item is Document Document)
            return Document.Synopsis;
        if (LinkItem.Item is Folder Folder)
            return Folder.Synopsis;
        if (LinkItem.Item is TextFile TextFile)
            return GetTextFileLinkText(TextFile, LinkItem.Place);
        if (LinkItem.Item is Component Component)
            return LinkItem.Place == LinkPlace.Text2 ? Component.Text2 : Component.Text;
        if (LinkItem.Item is Note Note)
            return Note.Text;

        return string.Empty;
    }
    /// <summary>
    /// Returns text from a text file by link place.
    /// </summary>
    /// <param name="TextFile">The text file.</param>
    /// <param name="Place">The link place.</param>
    /// <returns>The requested text.</returns>
    static string GetTextFileLinkText(TextFile TextFile, LinkPlace Place)
    {
        if (Place == LinkPlace.Text2)
            return TextFile.Text2;
        if (Place == LinkPlace.Synopsis)
            return TextFile.Synopsis;
        if (Place == LinkPlace.Draft)
            return TextFile.Draft;

        return TextFile.Text;
    }
    /// <summary>
    /// Returns the form identifier for a markdown preview tab.
    /// </summary>
    /// <param name="ItemId">The previewed item identifier.</param>
    /// <returns>The markdown preview form identifier.</returns>
    static public string GetMarkdownPreviewFormId(string ItemId)
    {
        return $"HTML-PREVIEW-{ItemId}";
    }
    /// <summary>
    /// Shows a markdown preview in the content pager.
    /// </summary>
    /// <param name="FormId">The preview form identifier.</param>
    /// <param name="Title">The preview title.</param>
    /// <param name="MarkdownText">The markdown text.</param>
    static public void ShowMarkdownPreview(string FormId, string Title, string MarkdownText)
    {
        Forms.MarkdownPreviewFormData Data = new Forms.MarkdownPreviewFormData();
        Data.Title = Title;
        Data.MarkdownText = MarkdownText;
        Forms.MarkdownPreviewForm Form = ShowContentForm<Forms.MarkdownPreviewForm>(FormId, Title, Data);
        Form.PreviewData = Data;
        Form.RefreshPreview();
    }
    /// <summary>
    /// Shows the application documentation.
    /// </summary>
    /// <returns>The documentation form.</returns>
    static public Forms.DocumentationForm ShowDocumentation()
    {
        Forms.DocumentationFormData Data = new Forms.DocumentationFormData();
        Data.Title = "Documentation";
        Data.MarkdownText = Documentation.ReadMainDocument();
        Forms.DocumentationForm Form = ShowContentForm<Forms.DocumentationForm>(nameof(Forms.DocumentationForm), Data.Title, Data);
        Form.PreviewData = Data;
        Form.RefreshPreview();
        return Form;
    }

    // ● properties
    /// <summary>
    /// Gets the sidebar pager handler.
    /// </summary>
    static public AppFormPagerHandler SideBarHandler { get; private set; }

    /// <summary>
    /// Gets the content pager handler.
    /// </summary>
    static public AppFormPagerHandler ContentHandler { get; private set; }
}
