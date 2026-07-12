// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents a markdown text item stored in its own folder.
/// </summary>
public class TextFile: BaseItem
{
    // ● private
    /// <summary>
    /// Saves a text file.
    /// </summary>
    /// <param name="FilePath">The file path.</param>
    /// <param name="Text">The text to save.</param>
    static void SaveTextFile(string FilePath, string Text)
    {
        string Folder = System.IO.Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrWhiteSpace(Folder))
            System.IO.Directory.CreateDirectory(Folder);

        System.IO.File.WriteAllText(FilePath, Text);
    }
    /// <summary>
    /// Loads a text file.
    /// </summary>
    /// <param name="FilePath">The file path.</param>
    /// <returns>The loaded text.</returns>
    static string LoadTextFile(string FilePath)
    {
        return System.IO.File.Exists(FilePath) ? System.IO.File.ReadAllText(FilePath) : string.Empty;
    }

    // ● protected
    /// <summary>
    /// Checks whether the text file can be renamed.
    /// </summary>
    /// <param name="NewTitle">The new title.</param>
    protected override void CheckRenameTitle(string NewTitle)
    {
        Document DocumentItem = Parent as Document;
        if (DocumentItem != null)
            CheckDuplicateTitle(DocumentItem.Files, NewTitle, this);

        Folder FolderItem = Parent as Folder;
        if (FolderItem != null)
            CheckDuplicateTitle(FolderItem.Files, NewTitle, this);
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the TextFile class.
    /// </summary>
    public TextFile()
    {
    }

    // ● public
    /// <summary>
    /// Saves the text file to persistent storage.
    /// </summary>
    public override void Save()
    {
        base.Save();

        SaveTextFile(TextFilePath, Text);
        SaveTextFile(Text2FilePath, Text2);
        SaveTextFile(AbstractionFilePath, Abstraction);
        SaveTextFile(DraftFilePath, Draft);
    }
    /// <summary>
    /// Loads the text file from persistent storage.
    /// </summary>
    public override void Load()
    {
        base.Load();

        Text = LoadTextFile(TextFilePath);
        Text2 = LoadTextFile(Text2FilePath);
        Abstraction = LoadTextFile(AbstractionFilePath);
        Draft = LoadTextFile(DraftFilePath);
    }
    /// <summary>
    /// Deletes the text file from persistent storage.
    /// </summary>
    public override void Delete()
    {
        if (!CanDelete())
            throw new InvalidOperationException("This text file cannot be deleted.");

        string ItemFolderPath = FolderPath;

        Document DocumentItem = Parent as Document;
        if (DocumentItem != null)
            DocumentItem.DetachTextFile(this);

        Folder FolderItem = Parent as Folder;
        if (FolderItem != null)
            FolderItem.DetachTextFile(this);

        DeleteStorage(ItemFolderPath);
    }
    /// <summary>
    /// Updates runtime references after loading the text file.
    /// </summary>
    /// <param name="ParentItem">The parent item.</param>
    public override void UpdateReferences(BaseItem ParentItem)
    {
        base.UpdateReferences(ParentItem);
    }
    /// <summary>
    /// Returns true if the text file can move in the specified direction.
    /// </summary>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <returns>True if the text file can move; otherwise false.</returns>
    public override bool CanMove(bool Up)
    {
        Document DocumentItem = Parent as Document;
        if (DocumentItem != null)
        {
            if (!DocumentItem.CanContainTextFiles)
                return false;

            return CanMoveItem(DocumentItem.Files, this, Up);
        }

        Folder FolderItem = Parent as Folder;
        if (FolderItem != null)
        {
            if (!FolderItem.CanContainTextFiles)
                return false;

            if (CanMoveItem(FolderItem.Files, this, Up))
                return true;

            BaseItem TargetContainer = GetAdjacentTextFileMoveContainer(Document, FolderItem, Up);
            Document TargetDocument = TargetContainer as Document;
            if (TargetDocument != null)
                return !ContainsTitle(TargetDocument.Files, Title);

            Folder TargetFolder = TargetContainer as Folder;
            return TargetFolder != null && !ContainsTitle(TargetFolder.Files, Title);
        }

        return false;
    }
    /// <summary>
    /// Moves the text file one step in the specified direction.
    /// </summary>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <returns>True if the text file is moved; otherwise false.</returns>
    public override bool Move(bool Up)
    {
        if (!CanMove(Up))
            return false;

        Document DocumentItem = Parent as Document;
        if (DocumentItem != null)
        {
            if (!DocumentItem.CanContainTextFiles)
                return false;

            bool Result = MoveItem(DocumentItem.Files, this, Up);
            if (Result)
                DocumentItem.UpdateReferences(DocumentItem.Parent);

            return Result;
        }

        Folder FolderItem = Parent as Folder;
        if (FolderItem != null)
        {
            if (!FolderItem.CanContainTextFiles)
                return false;

            bool Result = false;
            if (CanMoveItem(FolderItem.Files, this, Up))
            {
                Result = MoveItem(FolderItem.Files, this, Up);
            }
            else
            {
                BaseItem TargetContainer = GetAdjacentTextFileMoveContainer(Document, FolderItem, Up);
                Document TargetDocument = TargetContainer as Document;
                if (TargetDocument != null)
                    Result = MoveItem(FolderItem.Files, TargetDocument.Files, this, TargetDocument, Up);

                Folder TargetFolder = TargetContainer as Folder;
                if (TargetFolder != null)
                    Result = MoveItem(FolderItem.Files, TargetFolder.Files, this, TargetFolder, Up);
            }

            if (Result)
                FolderItem.UpdateReferences(FolderItem.Parent);

            return Result;
        }

        return false;
    }
    
    // ● properties
    /// <summary>
    /// Gets the item type.
    /// </summary>
    public override ItemType Type => ItemType.TextFile;
    /// <summary>
    /// Gets the primary text file name.
    /// </summary>
    static public string TextFileName => "Text.md";
    /// <summary>
    /// Gets the secondary text file name.
    /// </summary>
    static public string Text2FileName => "Text2.md";
    /// <summary>
    /// Gets the abstraction text file name.
    /// </summary>
    static public string AbstractionFileName => "Abstraction.md";
    /// <summary>
    /// Gets the draft text file name.
    /// </summary>
    static public string DraftFileName => "Draft.md";
    /// <summary>
    /// Gets or sets the text file title.
    /// </summary>
    public override string Title
    {
        get => base.Title;
        set => base.Title = value;
    }
    /// <summary>
    /// Gets the text file display title.
    /// </summary>
    public override string DisplayTitle => base.DisplayTitle;
    /// <summary>
    /// Gets the file-system folder path of the text file.
    /// </summary>
    [JsonIgnore]
    public override string FolderPath
    {
        get
        {
            Document DocumentItem = Parent as Document;
            if (DocumentItem != null)
                return System.IO.Path.Combine(DocumentItem.TextFilesFolderPath, StorageName);

            Folder FolderItem = Parent as Folder;
            if (FolderItem != null)
                return System.IO.Path.Combine(FolderItem.TextFilesFolderPath, StorageName);

            return base.FolderPath;
        }
    }
    /// <summary>
    /// Gets the owning folder.
    /// </summary>
    [JsonIgnore]
    public Folder Folder => Parent as Folder;
    /// <summary>
    /// Gets a value indicating whether the text file belongs directly to a document.
    /// </summary>
    [JsonIgnore]
    public bool IsDocumentFile => Parent is Document;
    /// <summary>
    /// Gets a value indicating whether the text file belongs to a folder.
    /// </summary>
    [JsonIgnore]
    public bool IsFolderFile => Parent is Folder;
    /// <summary>
    /// Gets the file-system path of the primary text file.
    /// </summary>
    [JsonIgnore]
    public string TextFilePath => System.IO.Path.Combine(FolderPath, TextFileName);
    /// <summary>
    /// Gets the file-system path of the secondary text file.
    /// </summary>
    [JsonIgnore]
    public string Text2FilePath => System.IO.Path.Combine(FolderPath, Text2FileName);
    /// <summary>
    /// Gets the file-system path of the abstraction text file.
    /// </summary>
    [JsonIgnore]
    public string AbstractionFilePath => System.IO.Path.Combine(FolderPath, AbstractionFileName);
    /// <summary>
    /// Gets the file-system path of the draft text file.
    /// </summary>
    [JsonIgnore]
    public string DraftFilePath => System.IO.Path.Combine(FolderPath, DraftFileName);
    /// <summary>
    /// Gets or sets the primary text.
    /// </summary>
    public string Text { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the secondary text.
    /// </summary>
    public string Text2 { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the abstraction text.
    /// </summary>
    public string Abstraction { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the draft text.
    /// </summary>
    public string Draft { get; set; } = string.Empty;
}
