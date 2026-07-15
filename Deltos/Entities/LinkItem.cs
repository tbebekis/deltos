// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents a link to a project item and a specific text location inside it.
/// </summary>
public class LinkItem
{
    // ● private
    /// <summary>
    /// Field for the Id property.
    /// </summary>
    string fId = string.Empty;
    /// <summary>
    /// Field for the Title property.
    /// </summary>
    string fTitle = string.Empty;

    // ● construction
    /// <summary>
    /// Initializes a new instance of the LinkItem class.
    /// </summary>
    public LinkItem()
    {
    }
    /// <summary>
    /// Initializes a new instance of the LinkItem class.
    /// </summary>
    /// <param name="ItemType">The item type.</param>
    /// <param name="Place">The link place.</param>
    /// <param name="Title">The display title.</param>
    /// <param name="Item">The linked item.</param>
    public LinkItem(ItemType ItemType, LinkPlace Place, string Title, BaseItem Item)
    {
        this.ItemType = ItemType;
        this.Place = Place;
        this.Title = Title;
        this.Item = Item;
    }

    // ● public
    /// <summary>
    /// Returns the display text.
    /// </summary>
    /// <returns>The display text.</returns>
    public override string ToString()
    {
        return $"{ItemType} - {Title}";
    }
    /// <summary>
    /// Loads the linked item from a project.
    /// </summary>
    /// <param name="Project">The project.</param>
    public void LoadItem(Project Project)
    {
        Item = null;
        if (Project == null || string.IsNullOrWhiteSpace(Id))
            return;

        Item = Project.GetDescendantItems(true).FirstOrDefault(Item => Item.Type == ItemType && Item.Id.IsSameText(Id));
    }

    // ● properties
    /// <summary>
    /// Gets or sets the linked item id.
    /// </summary>
    public string Id
    {
        get
        {
            if (string.IsNullOrWhiteSpace(fId) && Item != null)
                fId = Item.Id;

            return fId;
        }
        set => fId = value ?? string.Empty;
    }
    /// <summary>
    /// Gets or sets the link display title.
    /// </summary>
    public string Title
    {
        get
        {
            if (Item == null)
                return fTitle;

            return Place == LinkPlace.Text2 ? Item.DisplayTitle2 : Item.DisplayTitle;
        }
        set => fTitle = value ?? string.Empty;
    }
    /// <summary>
    /// Gets or sets the linked item type.
    /// </summary>
    public ItemType ItemType { get; set; } = ItemType.None;
    /// <summary>
    /// Gets or sets the place inside the item.
    /// </summary>
    public LinkPlace Place { get; set; } = LinkPlace.Title;
    /// <summary>
    /// Gets or sets a value indicating whether the link points to the secondary text.
    /// </summary>
    public bool IsText2 { get; set; }
    /// <summary>
    /// Gets or sets the zero-based line of the linked text match.
    /// </summary>
    public int Line { get; set; }
    /// <summary>
    /// Gets or sets the zero-based column of the linked text match.
    /// </summary>
    public int Column { get; set; }
    /// <summary>
    /// Gets or sets the line text of the linked text match.
    /// </summary>
    public string LineText { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the linked item.
    /// </summary>
    [JsonIgnore]
    public BaseItem Item { get; set; }
}
