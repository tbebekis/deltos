// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents a folder level in a document structure.
/// </summary>
public class FolderItem
{
    // ● protected
    /// <summary>
    /// Field for the Title property.
    /// </summary>
    protected string fTitle = string.Empty;
    /// <summary>
    /// Field for the Child property.
    /// </summary>
    protected FolderItem fChild;

    // ● construction
    /// <summary>
    /// Initializes a new instance of the FolderItem class.
    /// </summary>
    public FolderItem()
    {
    }

    // ● private
    /// <summary>
    /// Checks whether this folder item and its child items are valid.
    /// </summary>
    /// <param name="VisitedItems">The visited folder items.</param>
    void CheckValid(HashSet<FolderItem> VisitedItems)
    {
        if (VisitedItems.Contains(this))
            throw new InvalidOperationException("The folder structure contains a cycle.");

        VisitedItems.Add(this);
        AppHost.CheckValidFolderLevelTitle(Title);
        Child?.CheckValid(VisitedItems);
    }
    /// <summary>
    /// Clones this folder item and its child items.
    /// </summary>
    /// <param name="VisitedItems">The visited source folder items.</param>
    /// <returns>The cloned folder item.</returns>
    FolderItem Clone(HashSet<FolderItem> VisitedItems)
    {
        if (VisitedItems.Contains(this))
            throw new InvalidOperationException("The folder structure contains a cycle.");

        VisitedItems.Add(this);

        FolderItem Result = new FolderItem();
        Result.Title = Title;
        Result.Child = Child?.Clone(VisitedItems);
        return Result;
    }

    // ● public
    /// <summary>
    /// Checks whether this folder item and its child items are valid.
    /// </summary>
    public void CheckValid()
    {
        CheckValid(new HashSet<FolderItem>());
    }
    /// <summary>
    /// Returns true if this folder item and its child items are valid.
    /// </summary>
    /// <returns>True if the folder item graph is valid; otherwise false.</returns>
    public bool IsValid()
    {
        try
        {
            CheckValid();
            return true;
        }
        catch
        {
            return false;
        }
    }
    /// <summary>
    /// Clones this folder item graph.
    /// </summary>
    /// <returns>The cloned folder item graph.</returns>
    public FolderItem Clone()
    {
        FolderItem Result = Clone(new HashSet<FolderItem>());
        Result.UpdateReferences(null);
        return Result;
    }
    /// <summary>
    /// Updates runtime references after loading the folder item graph.
    /// </summary>
    /// <param name="ParentItem">The parent folder item.</param>
    public void UpdateReferences(FolderItem ParentItem)
    {
        Parent = ParentItem;
        Child?.UpdateReferences(this);
    }

    // ● properties
    /// <summary>
    /// Gets or sets the parent folder item.
    /// </summary>
    [JsonIgnore]
    public FolderItem Parent { get; set; }
    /// <summary>
    /// Gets a value indicating whether this is the top folder item.
    /// </summary>
    public bool IsTop => Parent == null;
    /// <summary>
    /// Gets a value indicating whether this is a leaf folder item.
    /// </summary>
    public bool IsLeaf => Child == null;
    /// <summary>
    /// Gets the zero-based folder item level.
    /// </summary>
    public int Level => IsTop ? 0 : Parent.Level + 1;
    /// <summary>
    /// Gets or sets the child folder item.
    /// </summary>
    public FolderItem Child
    {
        get => fChild;
        set
        {
            if (fChild != null && ReferenceEquals(fChild.Parent, this))
                fChild.Parent = null;

            fChild = value;
            if (fChild != null)
                fChild.Parent = this;
        }
    }
    /// <summary>
    /// Gets or sets the display title of a folder item, such as Part, Chapter, or Section.
    /// </summary>
    public string Title
    {
        get => fTitle;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                fTitle = string.Empty;
                return;
            }

            AppHost.CheckValidFolderLevelTitle(value);
            fTitle = value.Trim();
        }
    }
}
