// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Wiki;

/// <summary>
/// Provides wiki build output information.
/// </summary>
public class WikiBuildResult
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the WikiBuildResult class.
    /// </summary>
    public WikiBuildResult()
    {
    }

    // ● properties
    /// <summary>
    /// Gets emitted file relative paths.
    /// </summary>
    public List<string> EmittedFiles { get; } = new();
    /// <summary>
    /// Gets build log lines.
    /// </summary>
    public List<string> Log { get; } = new();
}
