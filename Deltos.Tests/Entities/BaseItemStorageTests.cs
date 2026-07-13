// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Tests.Entities;

/// <summary>
/// Tests base item storage name helpers.
/// </summary>
public class BaseItemStorageTests
{
    // ● public
    /// <summary>
    /// Tests that title decoding handles null and encoded spaces.
    /// </summary>
    [Fact]
    public void DecodeTitleHandlesNullAndEncodedSpaces()
    {
        Assert.Equal(string.Empty, BaseItem.DecodeTitle(null));
        Assert.Equal("Opening Scene", BaseItem.DecodeTitle("Opening_Scene"));
    }
    /// <summary>
    /// Tests that storage names are parsed into order, title, and display title.
    /// </summary>
    [Fact]
    public void TryParseStorageNameParsesValidStorageName()
    {
        bool Parsed = BaseItem.TryParseStorageName("001._Opening_Scene", out int OrderIndex, out string Title, out string DisplayTitle);

        Assert.True(Parsed);
        Assert.Equal(1, OrderIndex);
        Assert.Equal("Opening Scene", Title);
        Assert.Equal("001. Opening Scene", DisplayTitle);
        Assert.True(BaseItem.TryParseStorageName("001._Scene_2_-1", out _, out string HyphenTitle, out _));
        Assert.Equal("Scene 2 -1", HyphenTitle);
    }
    /// <summary>
    /// Tests that display titles use non-padded order indexes.
    /// </summary>
    [Fact]
    public void GetDisplayTitleUsesNonPaddedOrderIndex()
    {
        Assert.Equal("1. Opening Scene", BaseItem.GetDisplayTitle(1, "Opening Scene"));
    }
    /// <summary>
    /// Tests that storage names reject titles with leading or trailing spaces.
    /// </summary>
    [Fact]
    public void TryParseStorageNameRejectsUntrimmedTitle()
    {
        Assert.False(BaseItem.TryParseStorageName("001.__Opening_Scene", out _, out _, out _));
        Assert.False(BaseItem.TryParseStorageName("001._Opening_Scene_", out _, out _, out _));
    }
    /// <summary>
    /// Tests that storage name creation requires a positive one-based order index.
    /// </summary>
    [Fact]
    public void GetStorageNameRequiresPositiveOrderIndex()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BaseItem.GetStorageName(0, "Opening Scene"));
        Assert.Throws<ArgumentOutOfRangeException>(() => BaseItem.GetStorageName(-1, "Opening Scene"));
        Assert.Equal("001._Opening_Scene", BaseItem.GetStorageName(1, "Opening Scene"));
    }
    /// <summary>
    /// Tests that storage name creation requires a three-digit order index.
    /// </summary>
    [Fact]
    public void GetStorageNameRequiresThreeDigitOrderIndex()
    {
        Assert.Equal(999, BaseItem.MaxOrderIndex);
        Assert.Equal("999._Opening_Scene", BaseItem.GetStorageName(BaseItem.MaxOrderIndex, "Opening Scene"));
        Assert.Throws<ArgumentOutOfRangeException>(() => BaseItem.GetStorageName(BaseItem.MaxOrderIndex + 1, "Opening Scene"));
    }
    /// <summary>
    /// Tests that storage names reject invalid order prefixes and invalid titles.
    /// </summary>
    [Fact]
    public void TryParseStorageNameRejectsInvalidStorageNames()
    {
        Assert.False(BaseItem.TryParseStorageName("000._Opening_Scene", out _, out _, out _));
        Assert.False(BaseItem.TryParseStorageName("1000._Opening_Scene", out _, out _, out _));
        Assert.False(BaseItem.TryParseStorageName("001._", out _, out _, out _));
        Assert.False(BaseItem.TryParseStorageName("ABC._Opening_Scene", out _, out _, out _));
        Assert.False(BaseItem.TryParseStorageName("001_Opening_Scene", out _, out _, out _));
        Assert.False(BaseItem.TryParseStorageName("001._123_Opening", out _, out _, out _));
    }
}
