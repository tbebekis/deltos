// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Tests;

/// <summary>
/// Tests application host helpers.
/// </summary>
public class AppHostTests
{
    // ● public
    /// <summary>
    /// Tests that valid file names allow human title punctuation.
    /// </summary>
    [Fact]
    public void IsValidFileNameAcceptsHumanTitlePunctuation()
    {
        Assert.True(AppHost.IsValidFileName("Project One", false));
        Assert.True(AppHost.IsValidFileName("Project 1", false));
        Assert.True(AppHost.IsValidFileName("Scene 2 -1", false));
        Assert.True(AppHost.IsValidFileName("Chapter 1, Home", false));
        Assert.True(AppHost.IsValidFileName("Project.Name", false));
        Assert.True(AppHost.IsValidFileName("Project@Name", false));
        Assert.True(AppHost.IsValidFileName("Project?Name", false));
    }
    /// <summary>
    /// Tests that valid file names reject empty, numbered, and storage-empty titles.
    /// </summary>
    [Fact]
    public void IsValidFileNameRejectsInvalidTitles()
    {
        Assert.False(AppHost.IsValidFileName(string.Empty, false));
        Assert.False(AppHost.IsValidFileName("123 Project", false));
        Assert.False(AppHost.IsValidFileName(" 123 Project ", false));
        Assert.False(AppHost.IsValidFileName("???", false));
        Assert.False(AppHost.IsValidFileName("???1", false));
        Assert.False(AppHost.IsValidFileName("///", false));
    }
}
