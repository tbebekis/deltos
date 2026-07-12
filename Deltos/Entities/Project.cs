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
    /// Adds a document.
    /// </summary>
    /// <param name="Title">The document title.</param>
    /// <returns>The added document.</returns>
    public Document AddDocument(string Title)
    {
        AppHost.CheckValidFileName(Title);

        Document Result = new Document();
        Result.Title = Title;
        Documents.Add(Result);
        Result.UpdateReferences(this);
        RenumberChildren();
        return Result;
    }
    /// <summary>
    /// Removes a document.
    /// </summary>
    /// <param name="Document">The document to remove.</param>
    /// <returns>True if the document is removed; otherwise false.</returns>
    public bool RemoveDocument(Document Document)
    {
        bool Result = Documents.Remove(Document);
        if (Result)
            RenumberChildren();

        return Result;
    }
    /// <summary>
    /// Saves the project to persistent storage.
    /// </summary>
    public override void Save()
    {
        UpdateReferences(null);
        RenumberChildren();
        base.Save();

        System.IO.Directory.CreateDirectory(DocumentsFolderPath);

        foreach (Document Document in Documents)
            Document.Save();
    }
    /// <summary>
    /// Loads the project from persistent storage.
    /// </summary>
    public override void Load()
    {
        UpdateReferences(null);
        base.Load();

        Documents = LoadItems<Document>(DocumentsFolderPath);
        UpdateReferences(null);
    }
    /// <summary>
    /// Prepares persisted item information before saving the project.
    /// </summary>
    public override void PrepareInfo()
    {
        base.PrepareInfo();

        foreach (Document Document in Documents)
            Document.PrepareInfo();
    }
    /// <summary>
    /// Applies persisted item information after loading the project.
    /// </summary>
    public override void ApplyInfo()
    {
        base.ApplyInfo();

        foreach (Document Document in Documents)
            Document.ApplyInfo();
    }
    /// <summary>
    /// Renumbers project child items.
    /// </summary>
    public override void RenumberChildren()
    {
        RenumberItems(Documents);

        foreach (Document Document in Documents)
            Document.RenumberChildren();
    }
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
    /// Gets the documents folder name.
    /// </summary>
    static public string DocumentsFolderName => "Documents";
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
    /// Gets the file-system folder path of the project.
    /// </summary>
    [JsonIgnore]
    public override string FolderPath => ProjectPath;
    /// <summary>
    /// Gets the file-system storage name of the project folder.
    /// </summary>
    [JsonIgnore]
    public override string StorageName => string.Empty;
    /// <summary>
    /// Gets the file-system folder path of the project documents bucket.
    /// </summary>
    [JsonIgnore]
    public string DocumentsFolderPath => System.IO.Path.Combine(FolderPath, DocumentsFolderName);
    /// <summary>
    /// Gets or sets the project documents.
    /// </summary>
    public List<Document> Documents { get; set; } = new();
}
