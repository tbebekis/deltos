// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents the project quick-view list.
/// </summary>
public class QuickView
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the QuickView class.
    /// </summary>
    public QuickView()
    {
    }

    // ● public
    /// <summary>
    /// Loads the quick-view list from the project folder.
    /// </summary>
    /// <param name="Project">The project.</param>
    /// <returns>The loaded quick-view instance.</returns>
    static public QuickView Load(Project Project)
    {
        QuickView Result = new QuickView();
        if (Project == null || string.IsNullOrWhiteSpace(Project.ProjectPath))
            return Result;

        string FilePath = Project.QuickViewFilePath;
        if (System.IO.File.Exists(FilePath))
            Json.LoadFromFile(Result, FilePath);

        Result.List ??= new LinkItemList();
        Result.List.LoadItems(Project);
        Result.RemoveMissingItems();
        return Result;
    }
    /// <summary>
    /// Saves the quick-view list to the project folder.
    /// </summary>
    /// <param name="Project">The project.</param>
    public void Save(Project Project)
    {
        if (Project == null || string.IsNullOrWhiteSpace(Project.ProjectPath))
            return;

        Json.SaveToFile(this, Project.QuickViewFilePath);
    }
    /// <summary>
    /// Adds a link item when its target item is not already present.
    /// </summary>
    /// <param name="Item">The item.</param>
    /// <param name="Project">The project.</param>
    /// <returns>True if the item is added; otherwise false.</returns>
    public bool Add(LinkItem Item, Project Project)
    {
        if (Item == null || Item.Item == null || FindById(Item.Id) != null)
            return false;

        List.Add(Item);
        Save(Project);
        return true;
    }
    /// <summary>
    /// Removes a link item.
    /// </summary>
    /// <param name="Item">The link item.</param>
    /// <param name="Project">The project.</param>
    /// <returns>True if the item is removed; otherwise false.</returns>
    public bool Remove(LinkItem Item, Project Project)
    {
        if (Item == null)
            return false;

        bool Result = List.List.Remove(Item);
        if (Result)
            Save(Project);

        return Result;
    }
    /// <summary>
    /// Clears the quick-view list.
    /// </summary>
    /// <param name="Project">The project.</param>
    public void Clear(Project Project)
    {
        List.Clear();
        Save(Project);
    }
    /// <summary>
    /// Finds a link item by target item id.
    /// </summary>
    /// <param name="Id">The target item id.</param>
    /// <returns>The link item, if found; otherwise null.</returns>
    public LinkItem FindById(string Id)
    {
        return List.FindById(Id);
    }
    /// <summary>
    /// Moves an item one position up or down.
    /// </summary>
    /// <param name="Item">The link item.</param>
    /// <param name="Up">True to move up; false to move down.</param>
    /// <param name="Project">The project.</param>
    /// <returns>True if the item is moved; otherwise false.</returns>
    public bool Move(LinkItem Item, bool Up, Project Project)
    {
        int Index = List.List.IndexOf(Item);
        if (Index < 0)
            return false;

        int NewIndex = Up ? Index - 1 : Index + 1;
        if (NewIndex < 0 || NewIndex >= List.Count)
            return false;

        List.List.RemoveAt(Index);
        List.List.Insert(NewIndex, Item);
        Save(Project);
        return true;
    }
    /// <summary>
    /// Removes items whose linked target no longer exists.
    /// </summary>
    public void RemoveMissingItems()
    {
        List.List = List.List.Where(Item => Item.Item != null).ToList();
    }

    // ● properties
    /// <summary>
    /// Gets or sets the link item list.
    /// </summary>
    public LinkItemList List { get; set; } = new();
    /// <summary>
    /// Gets the number of link items.
    /// </summary>
    [JsonIgnore]
    public int Count => List.Count;
    /// <summary>
    /// Gets the quick-view file name.
    /// </summary>
    static public string FileName => "QuickView.json";
}
