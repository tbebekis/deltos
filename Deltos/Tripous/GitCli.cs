// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Tripous;

/// <summary>
/// Git command line exception.
/// </summary>
public class GitCliException: Exception
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the GitCliException class.
    /// </summary>
    /// <param name="Message">The exception message.</param>
    public GitCliException(string Message)
        : base(Message)
    {
    }
}

/// <summary>
/// Executes git commands.
/// </summary>
public class GitCli
{
    // ● private fields
    /// <summary>
    /// The CLI executor.
    /// </summary>
    readonly Cli fCli;

    // ● private
    /// <summary>
    /// Creates git environment variables.
    /// </summary>
    /// <returns>The git environment variables.</returns>
    static Dictionary<string, string> CreateGitEnvironment()
    {
        return new Dictionary<string, string> { ["GIT_TERMINAL_PROMPT"] = "0" };
    }
    /// <summary>
    /// Trims all output text to a single line.
    /// </summary>
    /// <param name="Text">The output text.</param>
    /// <returns>The trimmed text.</returns>
    static string TrimAll(string Text)
    {
        return (Text ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ').Trim();
    }
    /// <summary>
    /// Returns the default branch argument.
    /// </summary>
    /// <returns>The branch argument.</returns>
    string GetDefaultBranchArg()
    {
        return string.IsNullOrWhiteSpace(Branch) ? "HEAD" : Branch.Trim();
    }
    /// <summary>
    /// Checks whether the repository folder exists.
    /// </summary>
    void CheckRepoFolder()
    {
        if (string.IsNullOrWhiteSpace(RepoDir))
            throw new GitCliException("No repository folder.");

        if (!System.IO.Directory.Exists(RepoDir))
            throw new GitCliException($"Folder not found: {RepoDir}");
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the GitCli class.
    /// </summary>
    /// <param name="Cli">The CLI executor.</param>
    public GitCli(Cli Cli = null)
    {
        if (Cli == null)
        {
            fCli = new Cli();
        }
        else
        {
            fCli = Cli;
        }
    }

    // ● public
    /// <summary>
    /// Executes a git command.
    /// </summary>
    /// <param name="Args">The git command arguments.</param>
    /// <returns>The CLI result.</returns>
    public CliResult Git(params string[] Args)
    {
        string SaveDir = fCli.WorkingDirectory;
        try
        {
            fCli.WorkingDirectory = RepoDir;
            return fCli.RunExe("git", Args, -1, CreateGitEnvironment());
        }
        finally
        {
            fCli.WorkingDirectory = SaveDir;
        }
    }
    /// <summary>
    /// Returns true when git is installed.
    /// </summary>
    /// <returns>True when git is installed; otherwise false.</returns>
    public bool IsGitInstalled()
    {
        try
        {
            CliResult Result = fCli.RunExe("git", new[] { "--version" }, 10000, CreateGitEnvironment());
            return Result.Success;
        }
        catch
        {
            return false;
        }
    }
    /// <summary>
    /// Throws when git is not installed.
    /// </summary>
    public void CheckGitInstalled()
    {
        if (!IsGitInstalled())
            throw new GitCliException("Git is not installed or is not available in PATH.");
    }
    /// <summary>
    /// Returns true if the repository folder is a git repository.
    /// </summary>
    /// <returns>True if the repository folder is a git repository; otherwise false.</returns>
    public bool IsGitRepo()
    {
        CheckRepoFolder();
        CliResult Result = Git("rev-parse", "--is-inside-work-tree");
        return Result.Success && Result.StdOut.Trim().IsSameText("true");
    }
    /// <summary>
    /// Throws when the repository folder is not a git repository.
    /// </summary>
    public void CheckIsRepo()
    {
        if (!IsGitRepo())
            throw new GitCliException($"Not a git repository: {RepoDir}");
    }
    /// <summary>
    /// Initializes a git repository.
    /// </summary>
    /// <returns>True if the repository was initialized; otherwise false.</returns>
    public bool InitRepo()
    {
        if (IsGitRepo())
            return false;

        CliResult Result = Git("init");
        if (!Result.Success)
            throw new GitCliException(TrimAll(Result.StdErr));

        return true;
    }
    /// <summary>
    /// Returns true when the repository contains uncommitted changes.
    /// </summary>
    /// <returns>True when uncommitted changes exist; otherwise false.</returns>
    public bool HasUncommittedChanges()
    {
        CheckIsRepo();
        CliResult Result = Git("status", "--porcelain");
        return Result.Success && !string.IsNullOrWhiteSpace(Result.StdOut);
    }
    /// <summary>
    /// Creates a commit when pending changes exist.
    /// </summary>
    /// <param name="MessageText">The commit message.</param>
    /// <returns>True if a commit was created; otherwise false.</returns>
    public bool CommitIfNeeded(string MessageText)
    {
        CheckIsRepo();
        if (!HasUncommittedChanges())
            return false;

        CliResult Result = Git("add", "-A");
        if (!Result.Success)
            throw new GitCliException(TrimAll(Result.StdErr));

        Result = Git("commit", "-m", MessageText);
        if (!Result.Success)
            throw new GitCliException(TrimAll(Result.StdErr));

        return true;
    }
    /// <summary>
    /// Returns true if a remote exists.
    /// </summary>
    /// <param name="RemoteName">The remote name.</param>
    /// <returns>True if the remote exists; otherwise false.</returns>
    public bool HasRemote(string RemoteName)
    {
        CheckIsRepo();
        CliResult Result = Git("remote", "get-url", RemoteName);
        return Result.Success && !string.IsNullOrWhiteSpace(Result.StdOut);
    }
    /// <summary>
    /// Adds a git remote.
    /// </summary>
    /// <param name="RemoteName">The remote name.</param>
    /// <param name="RemoteUrl">The remote URL.</param>
    public void AddRemote(string RemoteName, string RemoteUrl)
    {
        CheckIsRepo();
        CliResult Result = Git("remote", "add", RemoteName, RemoteUrl);
        if (!Result.Success)
            throw new GitCliException(TrimAll(Result.StdErr));
    }
    /// <summary>
    /// Pushes the current branch to the configured remote.
    /// </summary>
    /// <returns>The CLI result.</returns>
    public CliResult Push()
    {
        CheckIsRepo();
        if (HasUncommittedChanges())
            throw new GitCliException("There are uncommitted changes. Please commit first.");

        string BranchArg = GetDefaultBranchArg();
        CliResult Result = string.IsNullOrWhiteSpace(BranchArg)
            ? Git("push", RemoteName)
            : Git("push", RemoteName, BranchArg);

        if (!Result.Success)
            throw new GitCliException(TrimAll(Result.StdErr));

        return Result;
    }

    // ● properties
    /// <summary>
    /// Gets the CLI executor.
    /// </summary>
    public Cli Cli => fCli;
    /// <summary>
    /// Gets or sets the repository directory.
    /// </summary>
    public string RepoDir { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the remote name.
    /// </summary>
    public string RemoteName { get; set; } = "origin";
    /// <summary>
    /// Gets or sets the branch name.
    /// </summary>
    public string Branch { get; set; } = string.Empty;
}
