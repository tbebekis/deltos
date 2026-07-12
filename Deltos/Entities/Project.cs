// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents a Deltos project.
/// </summary>
public class Project: BaseItem
{
    // ● construction
    /// <summary>
    /// Initializes a new instance of the Project class.
    /// </summary>
    public Project()
    {
    }
    
    // ● public
    /// <summary>
    /// Updates runtime references after loading the project graph.
    /// </summary>
    /// <param name="ParentItem">The parent item.</param>
    public override void UpdateReferences(BaseItem ParentItem)
    {
        Parent = ParentItem;
        Project = this;

        foreach (Document Document in Documents)
            Document.UpdateReferences(this);
    }

    // ● properties
    /// <summary>
    /// Gets the item type.
    /// </summary>
    public override ItemType Type => ItemType.Project;
    /// <summary>
    /// Gets the owning document.
    /// </summary>
    [JsonIgnore]
    public override Document Document => null;
    /// <summary>
    /// Gets or sets the project root folder path.
    /// </summary>
    [JsonIgnore]
    public string ProjectPath { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the project documents.
    /// </summary>
    public List<Document> Documents { get; set; } = new();
}
