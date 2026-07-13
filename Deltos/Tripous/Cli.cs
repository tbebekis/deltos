// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Tripous;

/// <summary>
/// Executes command line processes.
/// </summary>
public class Cli
{
    // ● private fields
    /// <summary>
    /// Field for the Environment property.
    /// </summary>
    readonly Dictionary<string, string> fEnvironment = new(StringComparer.OrdinalIgnoreCase);

    // ● private
    /// <summary>
    /// Builds a display command line.
    /// </summary>
    /// <param name="ExecutablePath">The executable path.</param>
    /// <param name="Args">The command arguments.</param>
    /// <returns>The display command line.</returns>
    static string BuildCommandLine(string ExecutablePath, IEnumerable<string> Args)
    {
        List<string> Parts = new();
        Parts.Add(QuoteArgument(ExecutablePath));
        foreach (string Arg in Args)
            Parts.Add(QuoteArgument(Arg));

        return string.Join(" ", Parts);
    }
    /// <summary>
    /// Quotes an argument for display.
    /// </summary>
    /// <param name="Value">The argument value.</param>
    /// <returns>The quoted argument.</returns>
    static string QuoteArgument(string Value)
    {
        if (string.IsNullOrEmpty(Value))
            return "\"\"";

        if (Value.IndexOfAny(new[] { ' ', '\t', '"', '\'' }) < 0)
            return Value;

        return "\"" + Value.Replace("\"", "\\\"") + "\"";
    }
    /// <summary>
    /// Appends process output text.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="Text">The output text.</param>
    static void AppendOutput(StringBuilder Builder, string Text)
    {
        if (Text != null)
            Builder.AppendLine(Text);
    }
    /// <summary>
    /// Runs a process.
    /// </summary>
    /// <param name="ExecutablePath">The executable path.</param>
    /// <param name="Args">The arguments.</param>
    /// <param name="InputText">The standard input text.</param>
    /// <param name="TimeoutMilliseconds">The timeout in milliseconds.</param>
    /// <param name="ExtraEnvironment">Extra environment variables.</param>
    /// <returns>The CLI result.</returns>
    CliResult RunProcess(string ExecutablePath, IEnumerable<string> Args, string InputText, int TimeoutMilliseconds, Dictionary<string, string> ExtraEnvironment)
    {
        List<string> ArgList = Args == null ? new List<string>() : Args.ToList();
        CliResult Result = new CliResult();
        Result.CommandLine = BuildCommandLine(ExecutablePath, ArgList);

        StringBuilder StdOut = new();
        StringBuilder StdErr = new();
        Stopwatch Stopwatch = Stopwatch.StartNew();

        ProcessStartInfo Info = new ProcessStartInfo();
        Info.FileName = ExecutablePath;
        Info.UseShellExecute = false;
        Info.RedirectStandardOutput = true;
        Info.RedirectStandardError = true;
        Info.RedirectStandardInput = !string.IsNullOrEmpty(InputText);
        Info.CreateNoWindow = true;

        if (!string.IsNullOrWhiteSpace(WorkingDirectory))
            Info.WorkingDirectory = WorkingDirectory;

        foreach (string Arg in ArgList)
            Info.ArgumentList.Add(Arg);

        foreach (KeyValuePair<string, string> Pair in fEnvironment)
            Info.Environment[Pair.Key] = Pair.Value;

        if (ExtraEnvironment != null)
        {
            foreach (KeyValuePair<string, string> Pair in ExtraEnvironment)
                Info.Environment[Pair.Key] = Pair.Value;
        }

        using Process Process = new Process();
        Process.StartInfo = Info;
        Process.OutputDataReceived += (Sender, Args2) => AppendOutput(StdOut, Args2.Data);
        Process.ErrorDataReceived += (Sender, Args2) => AppendOutput(StdErr, Args2.Data);

        Process.Start();
        Process.BeginOutputReadLine();
        Process.BeginErrorReadLine();

        if (!string.IsNullOrEmpty(InputText))
        {
            Process.StandardInput.Write(InputText);
            Process.StandardInput.Close();
        }

        int EffectiveTimeout = TimeoutMilliseconds >= 0 ? TimeoutMilliseconds : DefaultTimeoutMilliseconds;
        if (EffectiveTimeout >= 0 && !Process.WaitForExit(EffectiveTimeout))
        {
            Result.TimedOut = true;
            Process.Kill(true);
            Process.WaitForExit();
        }
        else
        {
            Process.WaitForExit();
        }

        Stopwatch.Stop();
        Result.ExitCode = Result.TimedOut ? -1 : Process.ExitCode;
        Result.StdOut = StdOut.ToString();
        Result.StdErr = StdErr.ToString();
        Result.DurationMilliseconds = Stopwatch.ElapsedMilliseconds;
        return Result;
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the Cli class.
    /// </summary>
    public Cli()
    {
    }

    // ● public
    /// <summary>
    /// Executes an executable.
    /// </summary>
    /// <param name="ExecutablePath">The executable path.</param>
    /// <param name="Args">The arguments.</param>
    /// <param name="TimeoutMilliseconds">The timeout in milliseconds.</param>
    /// <param name="ExtraEnvironment">Extra environment variables.</param>
    /// <returns>The CLI result.</returns>
    public CliResult RunExe(string ExecutablePath, IEnumerable<string> Args, int TimeoutMilliseconds = -1, Dictionary<string, string> ExtraEnvironment = null)
    {
        return RunProcess(ExecutablePath, Args, string.Empty, TimeoutMilliseconds, ExtraEnvironment);
    }
    /// <summary>
    /// Executes an executable with standard input text.
    /// </summary>
    /// <param name="ExecutablePath">The executable path.</param>
    /// <param name="Args">The arguments.</param>
    /// <param name="InputText">The standard input text.</param>
    /// <param name="TimeoutMilliseconds">The timeout in milliseconds.</param>
    /// <param name="ExtraEnvironment">Extra environment variables.</param>
    /// <returns>The CLI result.</returns>
    public CliResult RunExeWithInput(string ExecutablePath, IEnumerable<string> Args, string InputText, int TimeoutMilliseconds = -1, Dictionary<string, string> ExtraEnvironment = null)
    {
        return RunProcess(ExecutablePath, Args, InputText, TimeoutMilliseconds, ExtraEnvironment);
    }

    // ● properties
    /// <summary>
    /// Gets or sets the default timeout in milliseconds.
    /// </summary>
    public int DefaultTimeoutMilliseconds { get; set; } = 60000;
    /// <summary>
    /// Gets or sets the process working directory.
    /// </summary>
    public string WorkingDirectory { get; set; } = string.Empty;
    /// <summary>
    /// Gets custom environment variables.
    /// </summary>
    public Dictionary<string, string> Environment => fEnvironment;
}
