// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Main application window.
/// </summary>
public partial class MainWindow : Window
{
    // ● private fields
    Tripous.Desktop.ToolBar fToolBar;
    AppFormPagerHandler fSideBarHandler;
    AppFormPagerHandler fContentHandler;
    GridLength fSideBarWidth;
    GridLength fLogHeight;

    // ● private
    /// <summary>
    /// Initializes the window controls.
    /// </summary>
    void WindowInitialize()
    {
        LogBox.Initialize(edtLog);
        fSideBarHandler = new AppFormPagerHandler(pagerSideBar);
        fContentHandler = new AppFormPagerHandler(pagerContent);
        fContentHandler.CanUserReorderTabs = true;
        fContentHandler.IsTabHeaderContextMenuVisible = true;
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
    void ConfigureToolBarButton(Button Button, string CommandName)
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
    Button AddToolBarButton(string ImageFileName, string ToolTipText, string CommandName)
    {
        Button Result = fToolBar.AddButton(ImageFileName, ToolTipText, AnyToolBarClick);
        ConfigureToolBarButton(Result, CommandName);
        return Result;
    }

    /// <summary>
    /// Adds a toolbar separator.
    /// </summary>
    void AddToolBarSeparator()
    {
        Border Separator = fToolBar.AddSeparator();
        Separator.Margin = new Thickness(3, 0);
    }

    /// <summary>
    /// Prompts the user to select the project parent folder.
    /// </summary>
    /// <returns>The selected parent folder path, or an empty string.</returns>
    async Task<string> SelectProjectParentFolder()
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
    async Task<string> SelectProjectFolder()
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
    /// Prompts the user to select the project backup parent folder.
    /// </summary>
    /// <returns>The selected backup parent folder path, or an empty string.</returns>
    async Task<string> SelectProjectBackupParentFolder()
    {
        TopLevel TopLevel = TopLevel.GetTopLevel(this);
        if (TopLevel?.StorageProvider == null)
            return string.Empty;

        FolderPickerOpenOptions Options = new FolderPickerOpenOptions();
        Options.Title = "Select Project Backup Folder";
        Options.AllowMultiple = false;

        IReadOnlyList<IStorageFolder> Folders = await TopLevel.StorageProvider.OpenFolderPickerAsync(Options);
        if (Folders == null || Folders.Count == 0)
            return string.Empty;

        return Folders[0].Path.LocalPath;
    }
    /// <summary>
    /// Normalizes a folder path for comparisons.
    /// </summary>
    /// <param name="FolderPath">The folder path.</param>
    /// <returns>The normalized folder path.</returns>
    static string NormalizeFolderPath(string FolderPath)
    {
        if (string.IsNullOrWhiteSpace(FolderPath))
            return string.Empty;

        return System.IO.Path.GetFullPath(FolderPath.Trim()).TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
    }
    /// <summary>
    /// Returns true if two folder paths point to the same folder.
    /// </summary>
    /// <param name="A">The first folder path.</param>
    /// <param name="B">The second folder path.</param>
    /// <returns>True if both paths point to the same folder; otherwise false.</returns>
    static bool IsSameFolderPath(string A, string B)
    {
        return string.Equals(NormalizeFolderPath(A), NormalizeFolderPath(B), StringComparison.OrdinalIgnoreCase);
    }
    /// <summary>
    /// Returns true if a folder is the same as, or inside, another folder.
    /// </summary>
    /// <param name="FolderPath">The folder path to check.</param>
    /// <param name="ParentFolderPath">The parent folder path.</param>
    /// <returns>True if the folder is the same as, or inside, the parent folder; otherwise false.</returns>
    static bool IsSameOrChildFolderPath(string FolderPath, string ParentFolderPath)
    {
        string Folder = NormalizeFolderPath(FolderPath);
        string ParentFolder = NormalizeFolderPath(ParentFolderPath);
        if (string.IsNullOrWhiteSpace(Folder) || string.IsNullOrWhiteSpace(ParentFolder))
            return false;

        if (string.Equals(Folder, ParentFolder, StringComparison.OrdinalIgnoreCase))
            return true;

        string Prefix = ParentFolder + System.IO.Path.DirectorySeparatorChar;
        return Folder.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase);
    }
    /// <summary>
    /// Copies a folder recursively.
    /// </summary>
    /// <param name="SourceFolderPath">The source folder path.</param>
    /// <param name="TargetFolderPath">The target folder path.</param>
    static void CopyFolder(string SourceFolderPath, string TargetFolderPath)
    {
        System.IO.Directory.CreateDirectory(TargetFolderPath);

        foreach (string FilePath in System.IO.Directory.GetFiles(SourceFolderPath))
        {
            string TargetFilePath = System.IO.Path.Combine(TargetFolderPath, System.IO.Path.GetFileName(FilePath));
            System.IO.File.Copy(FilePath, TargetFilePath);
        }

        foreach (string FolderPath in System.IO.Directory.GetDirectories(SourceFolderPath))
        {
            string TargetChildFolderPath = System.IO.Path.Combine(TargetFolderPath, System.IO.Path.GetFileName(FolderPath));
            CopyFolder(FolderPath, TargetChildFolderPath);
        }
    }
    /// <summary>
    /// Creates a unique project backup folder path.
    /// </summary>
    /// <param name="ProjectPath">The project path.</param>
    /// <param name="BackupParentFolderPath">The backup parent folder path.</param>
    /// <returns>The backup folder path.</returns>
    static string GetLegacyProjectBackupFolderPath(string ProjectPath, string BackupParentFolderPath)
    {
        string ProjectFolderName = System.IO.Path.GetFileName(NormalizeFolderPath(ProjectPath));
        string Timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        string BackupFolderName = $"{ProjectFolderName}-backup-before-v{ProjectManifest.CurrentStorageVersion}-{Timestamp}";
        string Result = System.IO.Path.Combine(BackupParentFolderPath, BackupFolderName);
        int Index = 2;

        while (System.IO.Directory.Exists(Result) || System.IO.File.Exists(Result))
        {
            Result = System.IO.Path.Combine(BackupParentFolderPath, $"{BackupFolderName}-{Index}");
            Index++;
        }

        return Result;
    }

    /// <summary>
    /// Creates a new project using UI prompts.
    /// </summary>
    async Task CreateNewProject()
    {
        InputBoxData Data = await InputBox.ShowModal("Project title", string.Empty, this);
        if (Data == null || !Data.Result)
            return;

        string Title = Data.Value.Trim();
        if (string.IsNullOrWhiteSpace(Title))
        {
            await Tripous.Desktop.MessageBox.Error("Project title cannot be empty.", this);
            return;
        }

        string ParentFolderPath = await SelectProjectParentFolder();
        if (string.IsNullOrWhiteSpace(ParentFolderPath))
            return;

        try
        {
            AppHost.ShowPleaseWait("Creating project...", this);
            await Task.Yield();

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
    /// Creates a generated sample project using UI prompts.
    /// </summary>
    async Task CreateSampleProject()
    {
        DialogInfo Info = await DialogWindow.ShowModal<SampleProjectDialog>(null, this);
        if (!Info.Result)
            return;

        SampleProjectChoice Choice = Info.ResultData as SampleProjectChoice;
        if (Choice == null)
            return;

        string ParentFolderPath = await SelectProjectParentFolder();
        if (string.IsNullOrWhiteSpace(ParentFolderPath))
            return;

        try
        {
            AppHost.ShowPleaseWait("Creating sample project...", this);
            await Task.Yield();

            Project Project = AppHost.CreateSampleProject(ParentFolderPath, Choice.Kind);

            UpdateProjectStatus($"Sample project created: {Project.Title}");
            LogBox.AppendLine($"Sample project created: {Project.ProjectPath}");
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
    async Task OpenProject()
    {
        string ProjectPath = await SelectProjectFolder();
        if (string.IsNullOrWhiteSpace(ProjectPath))
            return;

        await OpenProject(ProjectPath);
    }
    /// <summary>
    /// Opens the last project using UI prompts when conversion is required.
    /// </summary>
    async Task OpenLastProject()
    {
        if (AppHost.Settings == null || !AppHost.Settings.LoadLastProjectOnStartup)
            return;

        string ProjectPath = AppHost.Settings.LastProjectFolderPath;
        if (string.IsNullOrWhiteSpace(ProjectPath) || ProjectPath == "___")
            return;

        if (!System.IO.Directory.Exists(ProjectPath))
            return;

        await OpenProject(ProjectPath, false);
    }
    /// <summary>
    /// Confirms whether an older project should be converted.
    /// </summary>
    /// <param name="ProjectPath">The project path.</param>
    /// <param name="Manifest">The project manifest.</param>
    /// <returns>True if the project may be opened; otherwise false.</returns>
    async Task<bool> ConfirmProjectStorageVersion(string ProjectPath, ProjectManifest Manifest)
    {
        if (Manifest.StorageVersion > ProjectManifest.CurrentStorageVersion)
        {
            await Tripous.Desktop.MessageBox.Error($"Unsupported project storage version {Manifest.StorageVersion}.", this);
            return false;
        }

        if (Manifest.StorageVersion != 0)
            return true;

        bool Confirmed = await Tripous.Desktop.MessageBox.YesNo("This project uses an older storage format. Convert it to the current format?", this);
        if (!Confirmed)
        {
            LogBox.AppendLine($"Legacy project open cancelled: {ProjectPath}");
            return false;
        }

        string BackupParentFolderPath = await SelectProjectBackupParentFolder();
        if (string.IsNullOrWhiteSpace(BackupParentFolderPath))
        {
            LogBox.AppendLine($"Legacy project open cancelled because no backup folder was selected: {ProjectPath}");
            return false;
        }

        string ProjectParentFolderPath = System.IO.Path.GetDirectoryName(NormalizeFolderPath(ProjectPath));
        if (IsSameOrChildFolderPath(BackupParentFolderPath, ProjectParentFolderPath) || IsSameFolderPath(BackupParentFolderPath, ProjectPath))
        {
            await Tripous.Desktop.MessageBox.Error("The backup folder must be outside the existing project's parent folder.", this);
            LogBox.AppendLine($"Legacy project open cancelled because backup folder is not external: {BackupParentFolderPath}");
            return false;
        }

        string BackupFolderPath = GetLegacyProjectBackupFolderPath(ProjectPath, BackupParentFolderPath);
        AppHost.ShowPleaseWait("Backing up project...", this);
        await Task.Yield();
        CopyFolder(ProjectPath, BackupFolderPath);
        AppHost.HidePleaseWait();
        LogBox.AppendLine($"Legacy project backup created: {BackupFolderPath}");

        return true;
    }
    /// <summary>
    /// Opens a project using the specified project path.
    /// </summary>
    /// <param name="ProjectPath">The project path.</param>
    async Task OpenProject(string ProjectPath)
    {
        await OpenProject(ProjectPath, true);
    }
    /// <summary>
    /// Opens a project using the specified project path.
    /// </summary>
    /// <param name="ProjectPath">The project path.</param>
    /// <param name="SaveSettings">True to persist the project path as the last project.</param>
    async Task OpenProject(string ProjectPath, bool SaveSettings)
    {
        try
        {
            ProjectManifest Manifest = ProjectManifest.Load(ProjectPath);
            if (!await ConfirmProjectStorageVersion(ProjectPath, Manifest))
                return;

            bool ConvertProject = Manifest.StorageVersion == 0;

            AppHost.ShowPleaseWait(ConvertProject ? "Converting project..." : "Opening project...", this);
            await Task.Yield();

            Project Project = AppHost.OpenProject(ProjectPath, SaveSettings);
            if (ConvertProject)
            {
                Project.Save();
                LogBox.AppendLine($"Project converted to storage version {ProjectManifest.CurrentStorageVersion}: {Project.ProjectPath}");
            }

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
    /// Shows recent projects and opens the selected project.
    /// </summary>
    async Task ShowRecentProjects()
    {
        if (AppHost.Settings == null)
            return;

        RecentProjectsDialogData Data = new RecentProjectsDialogData();
        Data.RecentProjects.AddRange(AppHost.Settings.RecentProjects);

        DialogInfo Info = await DialogWindow.ShowModal<RecentProjectsDialog>(Data, this);
        if (!Info.Result)
            return;

        RecentProjectsDialogData Result = Info.ResultData as RecentProjectsDialogData;
        if (Result == null)
            return;

        AppHost.Settings.RecentProjects = Result.RecentProjects ?? new List<string>();
        AppHost.Settings.Save();

        if (string.IsNullOrWhiteSpace(Result.SelectedProjectPath))
            return;

        if (IsSameProjectPath(Result.SelectedProjectPath, AppHost.CurrentProject?.ProjectPath))
            return;

        await OpenProject(Result.SelectedProjectPath);
    }
    /// <summary>
    /// Returns true if two project paths point to the same folder.
    /// </summary>
    /// <param name="A">The first project path.</param>
    /// <param name="B">The second project path.</param>
    /// <returns>True if the paths are the same; otherwise false.</returns>
    bool IsSameProjectPath(string A, string B)
    {
        if (string.IsNullOrWhiteSpace(A) || string.IsNullOrWhiteSpace(B))
            return false;

        try
        {
            A = System.IO.Path.GetFullPath(A.Trim());
            B = System.IO.Path.GetFullPath(B.Trim());
        }
        catch
        {
            A = A.Trim();
            B = B.Trim();
        }

        return A.IsSameText(B);
    }
    /// <summary>
    /// Shows the current project folder in the file explorer.
    /// </summary>
    async Task ShowProjectFolder()
    {
        if (AppHost.CurrentProject == null)
        {
            await Tripous.Desktop.MessageBox.Info("No project is open.", this);
            return;
        }

        Sys.OpenFileExplorer(AppHost.CurrentProject.ProjectPath);
    }
    /// <summary>
    /// Adds an image file to the current project images folder.
    /// </summary>
    async Task AddProjectImage()
    {
        Project Project = AppHost.CurrentProject;
        if (Project == null)
        {
            await Tripous.Desktop.MessageBox.Info("No project is open.", this);
            return;
        }

        string SourceFilePath = await Tripous.Desktop.Ui.OpenFileDialog(this, "png", "jpg", "jpeg", "gif", "bmp", "webp");
        if (string.IsNullOrWhiteSpace(SourceFilePath))
            return;

        try
        {
            string ImagePath = Project.AddImage(SourceFilePath);
            string MarkdownText = $"![{System.IO.Path.GetFileNameWithoutExtension(ImagePath)}]({ImagePath})";
            LogBox.AppendLine($"Image added: {System.IO.Path.Combine(Project.ImagesFolderName, ImagePath)}");
            LogBox.AppendLine($"Markdown: {MarkdownText}");
            UpdateProjectStatus($"Image added: {ImagePath}");
        }
        catch (Exception e)
        {
            LogBox.AppendLine(e);
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
    }
    /// <summary>
    /// Creates a git CLI for the current project.
    /// </summary>
    /// <returns>The git CLI.</returns>
    GitCli CreateProjectGitCli()
    {
        Project Project = AppHost.CurrentProject;
        if (Project == null)
            throw new InvalidOperationException("No project is open.");

        ProjectSettings Settings = ProjectSettings.Load(Project);
        GitCli Result = new GitCli();
        Result.RepoDir = Project.ProjectPath;
        Result.RemoteName = Settings.Git.RemoteName;
        Result.Branch = Settings.Git.Branch;
        return Result;
    }
    /// <summary>
    /// Writes the project git ignore file if it does not exist.
    /// </summary>
    /// <param name="Project">The project.</param>
    void EnsureProjectGitIgnore(Project Project)
    {
        string FilePath = System.IO.Path.Combine(Project.ProjectPath, ".gitignore");
        if (System.IO.File.Exists(FilePath))
            return;

        string Text =
            "**/bin" + Environment.NewLine +
            "**/obj" + Environment.NewLine +
            "**/.vs" + Environment.NewLine +
            "**/Wiki" + Environment.NewLine +
            "**/Export";

        System.IO.File.WriteAllText(FilePath, Text, Encoding.UTF8);
    }
    /// <summary>
    /// Edits project settings.
    /// </summary>
    /// <returns>True if the settings were saved; otherwise false.</returns>
    async Task<bool> EditProjectSettings()
    {
        Project Project = AppHost.CurrentProject;
        if (Project == null)
        {
            await Tripous.Desktop.MessageBox.Info("No project is open.", this);
            return false;
        }

        ProjectSettings Settings = ProjectSettings.Load(Project);
        DialogInfo Info = await DialogWindow.ShowModal<ProjectSettingsDialog>(Settings, this);
        if (!Info.Result)
            return false;

        Settings = Info.ResultData as ProjectSettings ?? Settings;
        Settings.Save(Project);
        LogBox.AppendLine("Project settings saved.");
        return true;
    }
    /// <summary>
    /// Commits project changes to git.
    /// </summary>
    async Task CommitToGit()
    {
        Project Project = AppHost.CurrentProject;
        if (Project == null)
        {
            await Tripous.Desktop.MessageBox.Info("No project is open.", this);
            return;
        }

        try
        {
            AppHost.ShowPleaseWait("Checking git repository...", this);
            await Task.Yield();
            LogBox.AppendLine("Commit to git started.");

            GitCli Git = CreateProjectGitCli();
            Git.CheckGitInstalled();

            if (!Git.IsGitRepo())
            {
                LogBox.AppendLine("Initializing git repository...");
                Git.InitRepo();
                EnsureProjectGitIgnore(Project);
                LogBox.AppendLine("Git repository initialized.");
            }

            if (!Git.HasUncommittedChanges())
            {
                LogBox.AppendLine("There are no uncommitted changes.");
                LogBox.AppendLine("Commit to git completed: nothing to commit.");
                AppHost.HidePleaseWait();
                await Tripous.Desktop.MessageBox.Info("There are no uncommitted changes.", this);
                return;
            }
        }
        catch (Exception e)
        {
            LogBox.AppendLine("Commit to git FAILED.");
            LogBox.AppendLine(e);
            AppHost.HidePleaseWait();
            await Tripous.Desktop.MessageBox.Error(e, this);
            return;
        }
        finally
        {
            AppHost.HidePleaseWait();
        }

        string DefaultMessage = $"Auto-commit {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        InputBoxData Data = await InputBox.ShowModal("Commit message", DefaultMessage, this);
        if (Data == null || !Data.Result)
        {
            LogBox.AppendLine("Commit to git canceled.");
            return;
        }

        string CommitMessage = string.IsNullOrWhiteSpace(Data.Value) ? DefaultMessage : Data.Value.Trim();

        try
        {
            AppHost.ShowPleaseWait("Committing to git...", this);
            await Task.Yield();

            GitCli Git = CreateProjectGitCli();
            if (Git.CommitIfNeeded(CommitMessage))
            {
                LogBox.AppendLine($"Committed to git: {CommitMessage}");
                LogBox.AppendLine("Commit to git completed.");
                UpdateProjectStatus("Committed to git");
            }
            else
            {
                LogBox.AppendLine("Nothing to commit.");
                LogBox.AppendLine("Commit to git completed: nothing to commit.");
            }
        }
        catch (Exception e)
        {
            LogBox.AppendLine("Commit to git FAILED.");
            LogBox.AppendLine(e);
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
        finally
        {
            AppHost.HidePleaseWait();
        }
    }
    /// <summary>
    /// Pushes project changes to the remote git repository.
    /// </summary>
    async Task PushToRemoteGitRepository()
    {
        Project Project = AppHost.CurrentProject;
        if (Project == null)
        {
            await Tripous.Desktop.MessageBox.Info("No project is open.", this);
            return;
        }

        ProjectSettings Settings = ProjectSettings.Load(Project);
        GitCli Git = CreateProjectGitCli();

        try
        {
            AppHost.ShowPleaseWait("Checking git repository...", this);
            await Task.Yield();
            LogBox.AppendLine("Push to remote git repository started.");

            Git.CheckGitInstalled();
            if (!Git.IsGitRepo())
                throw new InvalidOperationException($"There is no git repository in folder: {Project.ProjectPath}");

            if (Git.HasUncommittedChanges())
                throw new InvalidOperationException("There are uncommitted changes. Please commit them first.");

            if (!Git.HasRemote(Settings.Git.RemoteName))
            {
                AppHost.HidePleaseWait();
                if (string.IsNullOrWhiteSpace(Settings.Git.RemoteUrl))
                {
                    bool Saved = await EditProjectSettings();
                    if (!Saved)
                    {
                        LogBox.AppendLine("Push to remote git repository canceled.");
                        return;
                    }

                    Settings = ProjectSettings.Load(Project);
                    Git = CreateProjectGitCli();
                }

                if (string.IsNullOrWhiteSpace(Settings.Git.RemoteUrl))
                {
                    LogBox.AppendLine("Push to remote git repository FAILED: remote URL is required.");
                    await Tripous.Desktop.MessageBox.Info("Remote URL is required.", this);
                    return;
                }

                AppHost.ShowPleaseWait("Adding git remote...", this);
                await Task.Yield();
                Git.AddRemote(Settings.Git.RemoteName, Settings.Git.RemoteUrl);
                LogBox.AppendLine($"Git remote added: {Settings.Git.RemoteName}");
            }

            AppHost.HidePleaseWait();
        }
        catch (Exception e)
        {
            LogBox.AppendLine("Push to remote git repository FAILED.");
            LogBox.AppendLine(e);
            AppHost.HidePleaseWait();
            await Tripous.Desktop.MessageBox.Error(e, this);
            return;
        }

        try
        {
            AppHost.ShowPleaseWait("Pushing to remote git repository...", this);
            await Task.Yield();

            CliResult Result = Git.Push();
            LogBox.AppendLine("Pushing to remote git repository succeeded.");
            LogBox.AppendLine("Git output follows:");
            LogBox.AppendLine(Result.ToString());
            UpdateProjectStatus("Pushed to remote git repository");
        }
        catch (Exception e)
        {
            LogBox.AppendLine("Push to remote git repository FAILED.");
            LogBox.AppendLine(e);
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
        finally
        {
            AppHost.HidePleaseWait();
        }
    }
    /// <summary>
    /// Builds the project wiki.
    /// </summary>
    /// <param name="UseSecondaryText">True to build the secondary wiki.</param>
    async Task BuildWiki(bool UseSecondaryText)
    {
        Project Project = AppHost.CurrentProject;
        if (Project == null)
        {
            await Tripous.Desktop.MessageBox.Info("No project is open.", this);
            return;
        }

        ProjectSettings Settings = ProjectSettings.Load(Project);
        Settings.EnsureDefaults();

        string OutputFolderPath = UseSecondaryText ? Settings.Wiki.WikiFolderPath2 : Settings.Wiki.WikiFolderPath;
        if (string.IsNullOrWhiteSpace(OutputFolderPath))
        {
            bool Saved = await EditProjectSettings();
            if (!Saved)
                return;

            Settings = ProjectSettings.Load(Project);
            OutputFolderPath = UseSecondaryText ? Settings.Wiki.WikiFolderPath2 : Settings.Wiki.WikiFolderPath;
        }

        if (string.IsNullOrWhiteSpace(OutputFolderPath))
        {
            await Tripous.Desktop.MessageBox.Info("Wiki output folder is required.", this);
            return;
        }

        try
        {
            AppHost.ShowPleaseWait(UseSecondaryText ? "Building wiki 2..." : "Building wiki...", this);
            await Task.Yield();

            LogBox.AppendLine(UseSecondaryText ? "Build Wiki 2 started." : "Build Wiki started.");

            WikiBuildInfo Info = new WikiBuildInfo(UseSecondaryText);
            Info.Project = Project;
            Info.OutputFolderPath = OutputFolderPath;
            Info.HomeComponentTitle = Settings.Wiki.HomeComponentTitle;
            Info.AboutComponentTitle = Settings.Wiki.AboutComponentTitle;
            Info.GenerateTagPages = Settings.Wiki.GenerateTagPages;
            Info.SiteBaseUrl = Settings.Wiki.SiteBaseUrl;
            Info.DefaultSocialImageUrl = Settings.Wiki.DefaultSocialImageUrl;

            WikiBuildResult Result = WikiBuilder.Build(Info);
            LogBox.AppendLine("Wiki build result follows:");
            foreach (string Line in Result.Log)
                LogBox.AppendLine(Line);

            LogBox.AppendLine($"Emitted files: {Result.EmittedFiles.Count}");
            LogBox.AppendLine(UseSecondaryText ? "Build Wiki 2 completed." : "Build Wiki completed.");
            Sys.OpenFileExplorer(OutputFolderPath);
            UpdateProjectStatus(UseSecondaryText ? "Wiki 2 built" : "Wiki built");
        }
        catch (Exception e)
        {
            LogBox.AppendLine(UseSecondaryText ? "Build Wiki 2 FAILED." : "Build Wiki FAILED.");
            LogBox.AppendLine(e);
            await Tripous.Desktop.MessageBox.Error(e, this);
        }
        finally
        {
            AppHost.HidePleaseWait();
        }
    }
    /// <summary>
    /// Edits application settings.
    /// </summary>
    async Task EditSettings()
    {
        string ProjectPath = AppHost.CurrentProject?.ProjectPath;

        if (!string.IsNullOrWhiteSpace(ProjectPath))
        {
            AppHost.CloseProject();
            UpdateProjectStatus("Project closed for settings");
        }

        DialogInfo Info = await DialogWindow.ShowModal<AppSettingsDialog>(AppHost.Settings, this);
        if (Info.Result && Info.ResultData is AppSettings EditedSettings)
        {
            AppHost.Settings.CopyEditableSettingsFrom(EditedSettings);
            AppHost.Settings.Save();
            AppHost.ApplyAutoSaveSettings();
            LogBox.AppendLine("Settings saved.");
        }

        if (!string.IsNullOrWhiteSpace(ProjectPath) && System.IO.Directory.Exists(ProjectPath))
        {
            try
            {
                AppHost.ShowPleaseWait("Opening project...", this);
                await Task.Yield();
                Project Project = AppHost.OpenProject(ProjectPath, false);
                UpdateProjectStatus($"Project opened: {Project.Title}");
            }
            catch (Exception e)
            {
                LogBox.AppendLine(e);
                await Tripous.Desktop.MessageBox.Error(e, this);
            }
            finally
            {
                AppHost.HidePleaseWait();
            }
        }
        else
        {
            UpdateProjectStatus("Ready");
        }
    }

    /// <summary>
    /// Creates the main toolbar.
    /// </summary>
    void CreateToolBar()
    {
        fToolBar = new();
        fToolBar.Panel = pnlToolBar;

        AddToolBarButton("application_add.png", "New Project", "NewProject");
        AddToolBarButton("application_add.png", "Create Sample Project", "CreateSampleProject");
        AddToolBarButton("application_go.png", "Open Project", "OpenProject");
        AddToolBarButton("folder_vertical_open.png", "Recent Projects", "RecentProjects");
        AddToolBarButton("folder_go.png", "Show Project Folder", "ShowProjectFolder");
        AddToolBarButton("folder_vertical_document.png", "Add Image", "AddImage");

        AddToolBarSeparator();

        AddToolBarButton("setting_tools.png", "Settings", "Settings");
        AddToolBarButton("setting_tools.png", "Project Settings", "ProjectSettings");

        AddToolBarSeparator();

        AddToolBarButton("layout_sidebar.png", "Show/Hide SideBar", "ToggleSideBar");
        AddToolBarButton("error_log.png", "Show/Hide Log", "ToggleLog");
        AddToolBarButton("draw_eraser.png", "Clear Log", "ClearLog");

        AddToolBarSeparator();

        AddToolBarButton("compile.png", "Build Wiki", "BuildWiki");
        AddToolBarButton("compile.png", "Build Wiki 2", "BuildWiki2");

        AddToolBarButton("book.png", "Commit to git", "CommitToGit");
        AddToolBarButton("book_go.png", "Push to remote git repository", "PushToRemoteGitRepository");

        AddToolBarSeparator();

        AddToolBarButton("book_open.png", "Documentation", "Documentation");
        AddToolBarButton("information.png", "About Deltos", "About");
        AddToolBarButton("door_out.png", "Exit Application", "Exit");
    }

    /// <summary>
    /// Shows or hides the sidebar.
    /// </summary>
    void ToggleSideBar()
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
    void ToggleLog()
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
    async void AnyToolBarClick(object Sender, RoutedEventArgs Args)
    {
        if (Sender is Control Control && Control.Tag is string CommandName)
            await ExecuteToolBarCommand(CommandName);
    }

    /// <summary>
    /// Executes a toolbar command.
    /// </summary>
    /// <param name="CommandName">The command name.</param>
    async Task ExecuteToolBarCommand(string CommandName)
    {
        switch (CommandName)
        {
            case "NewProject":
                await CreateNewProject();
                break;
            case "CreateSampleProject":
                await CreateSampleProject();
                break;
            case "OpenProject":
                await OpenProject();
                break;
            case "RecentProjects":
                await ShowRecentProjects();
                break;
            case "ShowProjectFolder":
                await ShowProjectFolder();
                break;
            case "AddImage":
                await AddProjectImage();
                break;
            case "CommitToGit":
                await CommitToGit();
                break;
            case "PushToRemoteGitRepository":
                await PushToRemoteGitRepository();
                break;
            case "Settings":
                await EditSettings();
                break;
            case "ProjectSettings":
                await EditProjectSettings();
                break;
            case "ToggleSideBar":
                ToggleSideBar();
                break;
            case "ToggleLog":
                ToggleLog();
                break;
            case "ClearLog":
                ClearLog();
                break;
            case "BuildWiki":
                await BuildWiki(false);
                break;
            case "BuildWiki2":
                await BuildWiki(true);
                break;
            case "Documentation":
                AppHost.ShowDocumentation();
                UpdateStatusBar("Documentation opened", lblProjectStatus.Text);
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
    /// Clears the application log.
    /// </summary>
    void ClearLog()
    {
        LogBox.Clear();
        UpdateStatusBar("Log cleared", lblProjectStatus.Text);
    }
    /// <summary>
    /// Updates the status bar.
    /// </summary>
    /// <param name="StatusText">The main status text.</param>
    /// <param name="ProjectText">The project status text.</param>
    void UpdateStatusBar(string StatusText, string ProjectText)
    {
        lblStatus.Text = StatusText;
        lblProjectStatus.Text = ProjectText;
    }
    /// <summary>
    /// Updates the status bar based on the current project.
    /// </summary>
    /// <param name="StatusText">The main status text.</param>
    void UpdateProjectStatus(string StatusText)
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

    /// <summary>
    /// Opens startup project state after the main window is ready.
    /// </summary>
    public async Task OpenStartupProject()
    {
        await OpenLastProject();
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
