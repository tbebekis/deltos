// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Main application window.
/// </summary>
public partial class MainWindow : Window
{
    // ● private fields
    private Tripous.Desktop.ToolBar fToolBar;
    private AppFormPagerHandler fSideBarHandler;
    private AppFormPagerHandler fContentHandler;
    private GridLength fSideBarWidth;
    private GridLength fLogHeight;

    // ● private
    /// <summary>
    /// Initializes the window controls.
    /// </summary>
    private void WindowInitialize()
    {
        LogBox.Initialize(edtLog);
        fSideBarHandler = new AppFormPagerHandler(pagerSideBar);
        fContentHandler = new AppFormPagerHandler(pagerContent);
        AppHost.InitializeUi(fSideBarHandler, fContentHandler);

        fSideBarWidth = MainPanel.ColumnDefinitions[0].Width;
        fLogHeight = RightPanel.RowDefinitions[2].Height;

        CreateToolBar();
        UpdateProjectStatus("Ready");
        LogBox.AppendLine("Application started.");
    }

    /// <summary>
    /// Configures a toolbar button.
    /// </summary>
    /// <param name="Button">The button to configure.</param>
    /// <param name="CommandName">The command name.</param>
    private void ConfigureToolBarButton(Button Button, string CommandName)
    {
        Button.Tag = CommandName;
        Button.Width = 34;
        Button.Height = 34;
        Button.MinWidth = 0;
        Button.Padding = new Thickness(2);
        Button.Margin = new Thickness(0);
    }

    /// <summary>
    /// Adds a toolbar button.
    /// </summary>
    /// <param name="ImageFileName">The image file name.</param>
    /// <param name="ToolTipText">The tooltip text.</param>
    /// <param name="CommandName">The command name.</param>
    /// <returns>The created button.</returns>
    private Button AddToolBarButton(string ImageFileName, string ToolTipText, string CommandName)
    {
        Button Result = fToolBar.AddButton(ImageFileName, ToolTipText, AnyToolBarClick);
        ConfigureToolBarButton(Result, CommandName);
        return Result;
    }

    /// <summary>
    /// Adds a toolbar separator.
    /// </summary>
    private void AddToolBarSeparator()
    {
        Border Separator = fToolBar.AddSeparator();
        Separator.Margin = new Thickness(3, 0);
    }

    /// <summary>
    /// Prompts the user to select the project parent folder.
    /// </summary>
    /// <returns>The selected parent folder path, or an empty string.</returns>
    private async Task<string> SelectProjectParentFolder()
    {
        TopLevel TopLevel = TopLevel.GetTopLevel(this);
        if (TopLevel?.StorageProvider == null)
            return string.Empty;

        FolderPickerOpenOptions Options = new FolderPickerOpenOptions();
        Options.Title = "Select Project Parent Folder";
        Options.AllowMultiple = false;

        IReadOnlyList<IStorageFolder> Folders = await TopLevel.StorageProvider.OpenFolderPickerAsync(Options);
        if (Folders == null || Folders.Count == 0)
            return string.Empty;

        return Folders[0].Path.LocalPath;
    }
    /// <summary>
    /// Prompts the user to select the project folder.
    /// </summary>
    /// <returns>The selected project folder path, or an empty string.</returns>
    private async Task<string> SelectProjectFolder()
    {
        TopLevel TopLevel = TopLevel.GetTopLevel(this);
        if (TopLevel?.StorageProvider == null)
            return string.Empty;

        FolderPickerOpenOptions Options = new FolderPickerOpenOptions();
        Options.Title = "Open Project Folder";
        Options.AllowMultiple = false;

        IReadOnlyList<IStorageFolder> Folders = await TopLevel.StorageProvider.OpenFolderPickerAsync(Options);
        if (Folders == null || Folders.Count == 0)
            return string.Empty;

        return Folders[0].Path.LocalPath;
    }

    /// <summary>
    /// Creates a new project using UI prompts.
    /// </summary>
    private async Task CreateNewProject()
    {
        InputBoxData Data = await InputBox.ShowModal("Project title", string.Empty, this);
        if (Data == null || !Data.Result)
            return;

        string Title = Data.Value.Trim();
        if (!AppHost.IsValidFileName(Title, false))
        {
            await Tripous.Desktop.MessageBox.Error($"Invalid project title: {Title}", this);
            return;
        }

        string ParentFolderPath = await SelectProjectParentFolder();
        if (string.IsNullOrWhiteSpace(ParentFolderPath))
            return;

        try
        {
            AppHost.ShowPleaseWait("Creating project...", this);

            Project Project = AppHost.CreateProject(ParentFolderPath, Title);

            UpdateProjectStatus($"Project created: {Project.Title}");
            LogBox.AppendLine($"Project created: {Project.ProjectPath}");
        }
        catch (Exception e)
        {
            LogBox.AppendLine(e);
            AppHost.HidePleaseWait();
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
        finally
        {
            AppHost.HidePleaseWait();
        }
    }
    /// <summary>
    /// Opens a project using UI prompts.
    /// </summary>
    private async Task OpenProject()
    {
        string ProjectPath = await SelectProjectFolder();
        if (string.IsNullOrWhiteSpace(ProjectPath))
            return;

        try
        {
            AppHost.ShowPleaseWait("Opening project...", this);

            Project Project = AppHost.OpenProject(ProjectPath);

            UpdateProjectStatus($"Project opened: {Project.Title}");
            LogBox.AppendLine($"Project opened: {Project.ProjectPath}");
        }
        catch (Exception e)
        {
            LogBox.AppendLine(e);
            AppHost.HidePleaseWait();
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
        finally
        {
            AppHost.HidePleaseWait();
        }
    }
    /// <summary>
    /// Shows the current project folder in the file explorer.
    /// </summary>
    private async Task ShowProjectFolder()
    {
        if (AppHost.CurrentProject == null)
        {
            await Tripous.Desktop.MessageBox.Info("No project is open.", this);
            return;
        }

        Sys.OpenFileExplorer(AppHost.CurrentProject.ProjectPath);
    }

    /// <summary>
    /// Creates the main toolbar.
    /// </summary>
    private void CreateToolBar()
    {
        fToolBar = new();
        fToolBar.Panel = pnlToolBar;

        AddToolBarButton("application_add.png", "New Project", "NewProject");
        AddToolBarButton("application_go.png", "Open Project", "OpenProject");
        AddToolBarButton("folder_go.png", "Show Project Folder", "ShowProjectFolder");

        AddToolBarSeparator();

        AddToolBarButton("setting_tools.png", "Settings", "Settings");

        AddToolBarSeparator();

        AddToolBarButton("layout_sidebar.png", "Show/Hide SideBar", "ToggleSideBar");
        AddToolBarButton("error_log.png", "Show/Hide Log", "ToggleLog");

        AddToolBarSeparator();

        AddToolBarButton("information.png", "About Deltos", "About");
        AddToolBarButton("door_out.png", "Exit Application", "Exit");
    }

    /// <summary>
    /// Shows or hides the sidebar.
    /// </summary>
    private void ToggleSideBar()
    {
        bool IsVisible = pagerSideBar.IsVisible;

        pagerSideBar.IsVisible = !IsVisible;
        MainSplitter.IsVisible = !IsVisible;
        MainPanel.ColumnDefinitions[0].Width = IsVisible ? new GridLength(0) : fSideBarWidth;
        MainPanel.ColumnDefinitions[1].Width = IsVisible ? new GridLength(0) : GridLength.Auto;

        UpdateStatusBar(IsVisible ? "SideBar hidden" : "SideBar visible", lblProjectStatus.Text);
    }

    /// <summary>
    /// Shows or hides the log panel.
    /// </summary>
    private void ToggleLog()
    {
        bool IsVisible = edtLog.IsVisible;

        edtLog.IsVisible = !IsVisible;
        LogSplitter.IsVisible = !IsVisible;
        RightPanel.RowDefinitions[1].Height = IsVisible ? new GridLength(0) : new GridLength(4);
        RightPanel.RowDefinitions[2].Height = IsVisible ? new GridLength(0) : fLogHeight;

        UpdateStatusBar(IsVisible ? "Log hidden" : "Log visible", lblProjectStatus.Text);
    }

    /// <summary>
    /// Handles toolbar button clicks.
    /// </summary>
    /// <param name="Sender">The event sender.</param>
    /// <param name="Args">The event arguments.</param>
    private async void AnyToolBarClick(object Sender, RoutedEventArgs Args)
    {
        if (Sender is Control Control && Control.Tag is string CommandName)
            await ExecuteToolBarCommand(CommandName);
    }

    /// <summary>
    /// Executes a toolbar command.
    /// </summary>
    /// <param name="CommandName">The command name.</param>
    private async Task ExecuteToolBarCommand(string CommandName)
    {
        switch (CommandName)
        {
            case "NewProject":
                await CreateNewProject();
                break;
            case "OpenProject":
                await OpenProject();
                break;
            case "ShowProjectFolder":
                await ShowProjectFolder();
                break;
            case "Settings":
                UpdateStatusBar($"Command: {CommandName}", "No project open");
                LogBox.AppendLine($"Command not implemented yet: {CommandName}");
                break;
            case "ToggleSideBar":
                ToggleSideBar();
                break;
            case "ToggleLog":
                ToggleLog();
                break;
            case "About":
                await DialogWindow.ShowModal<AboutDialog>(null, this);
                UpdateStatusBar("Ready", "No project open");
                break;
            case "Exit":
                Close();
                break;
            default:
                UpdateStatusBar($"Command: {CommandName}", "No project open");
                break;
        }
    }

    /// <summary>
    /// Updates the status bar.
    /// </summary>
    /// <param name="StatusText">The main status text.</param>
    /// <param name="ProjectText">The project status text.</param>
    private void UpdateStatusBar(string StatusText, string ProjectText)
    {
        lblStatus.Text = StatusText;
        lblProjectStatus.Text = ProjectText;
    }
    /// <summary>
    /// Updates the status bar based on the current project.
    /// </summary>
    /// <param name="StatusText">The main status text.</param>
    private void UpdateProjectStatus(string StatusText)
    {
        string ProjectText = AppHost.CurrentProject == null ? "No project open" : AppHost.CurrentProject.ProjectPath;
        UpdateStatusBar(StatusText, ProjectText);
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the MainWindow class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        WindowInitialize();
    }

    // ● properties
    /// <summary>
    /// Gets the sidebar pager handler.
    /// </summary>
    public AppFormPagerHandler SideBarHandler => fSideBarHandler;
    /// <summary>
    /// Gets the content pager handler.
    /// </summary>
    public AppFormPagerHandler ContentHandler => fContentHandler;
}
