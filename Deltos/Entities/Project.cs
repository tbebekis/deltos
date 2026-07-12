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
        CheckDuplicateTitle(Documents, Title);

        Document Result = new Document();
        Result.Title = Title;
        Documents.Add(Result);
        Result.UpdateReferences(this);
        RenumberChildren();
        SaveItemIfStorageReady(Result);
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
        {
            Document.ClearReferences();
            RenumberChildren();
            UpdateReferences(null);
        }

        return Result;
    }
    /// <summary>
    /// Deletes a child item from memory and persistent storage.
    /// </summary>
    /// <param name="Item">The child item to delete.</param>
    /// <returns>True if the child item is deleted; otherwise false.</returns>
    public override bool RemoveChild(BaseItem Item)
    {
        Document Document = Item as Document;
        if (Document == null || !Documents.Contains(Document))
            return false;

        Document.Delete();
        return true;
    }
    /// <summary>
    /// Moves a document inside the project documents list.
    /// </summary>
    /// <param name="Document">The document to move.</param>
    /// <param name="NewOrderIndex">The new one-based order index.</param>
    /// <returns>True if the document is moved; otherwise false.</returns>
    public bool MoveDocument(Document Document, int NewOrderIndex)
    {
        bool Result = MoveItem(Documents, Document, NewOrderIndex);
        if (Result)
            UpdateReferences(null);

        return Result;
    }
    /// <summary>
    /// Saves the project to persistent storage.
    /// </summary>
    public override void Save()
    {
        RenumberChildren();
        UpdateReferences(null);
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
    /// Renames the project.
    /// </summary>
    /// <param name="NewTitle">The new title.</param>
    public override void Rename(string NewTitle)
    {
        Title = NewTitle;
        SaveInfo();
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
    /// <summary>
    /// Returns the project child items.
    /// </summary>
    /// <returns>The project child items.</returns>
    public override List<BaseItem> GetChildItems()
    {
        List<BaseItem> Result = new();
        foreach (Document Document in Documents)
            Result.Add(Document);

        return Result;
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
