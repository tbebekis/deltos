// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Tripous;

/// <summary>
/// Represents the result of a command line execution.
/// </summary>
public class CliResult
{
    // ● public
    /// <summary>
    /// Returns the result as text.
    /// </summary>
    /// <returns>The result text.</returns>
    public override string ToString()
    {
        return
            $"> {CommandLine}{Environment.NewLine}" +
            $"ExitCode: {ExitCode}{Environment.NewLine}" +
            $"TimedOut: {TimedOut}{Environment.NewLine}" +
            $"Duration: {DurationMilliseconds} ms{Environment.NewLine}{Environment.NewLine}" +
            $"--- StdOut ---{Environment.NewLine}" +
            $"{StdOut}{Environment.NewLine}" +
            $"--- StdErr ---{Environment.NewLine}" +
            StdErr;
    }

    // ● properties
    /// <summary>
    /// Gets or sets the command line.
    /// </summary>
    public string CommandLine { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the process exit code.
    /// </summary>
    public int ExitCode { get; set; } = -1;
    /// <summary>
    /// Gets or sets standard output text.
    /// </summary>
    public string StdOut { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets standard error text.
    /// </summary>
    public string StdErr { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether execution timed out.
    /// </summary>
    public bool TimedOut { get; set; }
    /// <summary>
    /// Gets or sets execution duration in milliseconds.
    /// </summary>
    public long DurationMilliseconds { get; set; }
    /// <summary>
    /// Gets a value indicating whether execution succeeded.
    /// </summary>
    public bool Success => !TimedOut && ExitCode == 0;
}
