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
    /// Tests that valid file names allow English letters, numbers, and spaces.
    /// </summary>
    [Fact]
    public void IsValidFileNameAcceptsEnglishLettersNumbersAndSpaces()
    {
        Assert.True(AppHost.IsValidFileName("Project One", false));
        Assert.True(AppHost.IsValidFileName("Project 1", false));
    }
    /// <summary>
    /// Tests that valid file names reject empty, numbered, and special-character titles.
    /// </summary>
    [Fact]
    public void IsValidFileNameRejectsInvalidTitles()
    {
        Assert.False(AppHost.IsValidFileName(string.Empty, false));
        Assert.False(AppHost.IsValidFileName("123 Project", false));
        Assert.False(AppHost.IsValidFileName("Project-Name", false));
        Assert.False(AppHost.IsValidFileName("Project_Name", false));
        Assert.False(AppHost.IsValidFileName("Project.Name", false));
        Assert.False(AppHost.IsValidFileName("Project@Name", false));
        Assert.False(AppHost.IsValidFileName("Project?Name", false));
    }
}
