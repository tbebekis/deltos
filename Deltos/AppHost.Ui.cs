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

        ShowSideBarForm<Forms.DocumentListForm>(nameof(Forms.DocumentListForm), "Documents");
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
