// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Defines the place inside an item a link points to.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LinkPlace
{
    /// <summary>
    /// The item title.
    /// </summary>
    Title = 0,
    /// <summary>
    /// The primary item text.
    /// </summary>
    Text = 1,
    /// <summary>
    /// The secondary item text.
    /// </summary>
    Text2 = 2,
    /// <summary>
    /// The item synopsis.
    /// </summary>
    Synopsis = 3,
    /// <summary>
    /// The item draft.
    /// </summary>
    Draft = 4,
    /// <summary>
    /// The project temporary text.
    /// </summary>
    TempFile = 5
}
