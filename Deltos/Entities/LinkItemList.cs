// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents a serializable list of link items.
/// </summary>
public class LinkItemList
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the LinkItemList class.
    /// </summary>
    public LinkItemList()
    {
    }

    // ● public
    /// <summary>
    /// Adds a link item.
    /// </summary>
    /// <returns>The added link item.</returns>
    public LinkItem Add()
    {
        LinkItem Result = new();
        List.Add(Result);
        return Result;
    }
    /// <summary>
    /// Adds a link item.
    /// </summary>
    /// <param name="Item">The link item.</param>
    public void Add(LinkItem Item)
    {
        if (Item != null)
            List.Add(Item);
    }
    /// <summary>
    /// Finds a link item by linked item id.
    /// </summary>
    /// <param name="Id">The linked item id.</param>
    /// <returns>The link item if found; otherwise null.</returns>
    public LinkItem FindById(string Id)
    {
        return List.FirstOrDefault(Item => Item.Id.IsSameText(Id));
    }
    /// <summary>
    /// Loads all linked items from a project.
    /// </summary>
    /// <param name="Project">The project.</param>
    public void LoadItems(Project Project)
    {
        foreach (LinkItem Item in List)
            Item.LoadItem(Project);
    }
    /// <summary>
    /// Removes all link items.
    /// </summary>
    public void Clear()
    {
        List.Clear();
    }

    // ● properties
    /// <summary>
    /// Gets or sets the link item list.
    /// </summary>
    public List<LinkItem> List { get; set; } = new();
    /// <summary>
    /// Gets the number of link items.
    /// </summary>
    [JsonIgnore]
    public int Count => List.Count;
    /// <summary>
    /// Gets or sets the link item at an index.
    /// </summary>
    /// <param name="Index">The index.</param>
    /// <returns>The link item.</returns>
    [JsonIgnore]
    public LinkItem this[int Index]
    {
        get => List[Index];
        set => List[Index] = value;
    }
}
