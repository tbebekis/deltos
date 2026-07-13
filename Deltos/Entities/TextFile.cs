// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents a markdown text item stored in its own folder.
/// </summary>
public class TextFile: BaseItem
{
    // ● protected
    /// <summary>
    /// Field for the Text property.
    /// </summary>
    protected string fText = string.Empty;
    /// <summary>
    /// Field for the Text2 property.
    /// </summary>
    protected string fText2 = string.Empty;
    /// <summary>
    /// Field for the Synopsis property.
    /// </summary>
    protected string fSynopsis = string.Empty;
    /// <summary>
    /// Field for the Draft property.
    /// </summary>
    protected string fDraft = string.Empty;

    // ● protected
    /// <summary>
    /// Checks whether the required text file content files exist.
    /// </summary>
    protected virtual void CheckRequiredContentFiles()
    {
        if (!System.IO.File.Exists(TextFilePath))
            throw new InvalidOperationException($"The text file content file does not exist: {TextFilePath}");
    }
    /// <summary>
    /// Checks whether the text file storage folder contains only known content files.
    /// </summary>
    protected virtual void CheckStorageFolder()
    {
        string[] FolderPaths = System.IO.Directory.GetDirectories(FolderPath);
        if (FolderPaths.Length > 0)
            throw new InvalidOperationException($"Text file storage folder contains child folders: {FolderPath}");

        HashSet<string> FileNames = new(StringComparer.OrdinalIgnoreCase);
        FileNames.Add(InfoFileName);
        FileNames.Add(TextFileName);
        FileNames.Add(Text2FileName);
        FileNames.Add(SynopsisFileName);
        FileNames.Add(DraftFileName);

        foreach (string FilePath in System.IO.Directory.GetFiles(FolderPath))
        {
            string FileName = System.IO.Path.GetFileName(FilePath);
            if (!FileNames.Contains(FileName))
                throw new InvalidOperationException($"Text file storage folder contains an unknown file: {FilePath}");
        }
    }
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
    /// <summary>
    /// Returns the source text files list.
    /// </summary>
    /// <returns>The source text files list.</returns>
    protected virtual List<TextFile> GetSourceTextFiles()
    {
        Document DocumentItem = Parent as Document;
        if (DocumentItem != null)
            return DocumentItem.Files;

        Folder FolderItem = Parent as Folder;
        if (FolderItem != null)
            return FolderItem.Files;

        return null;
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

        SaveMarkdownFile(TextFilePath, Text);
        SaveMarkdownFile(Text2FilePath, Text2);
        SaveMarkdownFile(SynopsisFilePath, Synopsis);
        SaveMarkdownFile(DraftFilePath, Draft);
    }
    /// <summary>
    /// Loads the text file from persistent storage.
    /// </summary>
    public override void Load()
    {
        base.Load();
        CheckStorageFolder();
        CheckRequiredContentFiles();

        Text = LoadMarkdownFile(TextFilePath);
        Text2 = LoadMarkdownFile(Text2FilePath);
        Synopsis = LoadMarkdownFile(SynopsisFilePath);
        Draft = LoadMarkdownFile(DraftFilePath);
    }
    /// <summary>
    /// Deletes the text file from persistent storage.
    /// </summary>
    public override void Delete()
    {
        if (!CanDelete())
            throw new InvalidOperationException("This text file cannot be deleted.");

        string ItemFolderPath = FolderPath;
        DeleteStorage(ItemFolderPath);

        Document DocumentItem = Parent as Document;
        if (DocumentItem != null)
            DocumentItem.DetachTextFile(this);

        Folder FolderItem = Parent as Folder;
        if (FolderItem != null)
            FolderItem.DetachTextFile(this);
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
                return CanAddItem(TargetDocument.Files) && !ContainsTitle(TargetDocument.Files, Title);

            Folder TargetFolder = TargetContainer as Folder;
            return TargetFolder != null && CanAddItem(TargetFolder.Files) && !ContainsTitle(TargetFolder.Files, Title);
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
    /// <summary>
    /// Returns true if the text file can change parent.
    /// </summary>
    /// <param name="TargetParent">The target parent item.</param>
    /// <returns>True if the text file can change parent; otherwise false.</returns>
    public override bool CanChangeParent(BaseItem TargetParent)
    {
        Folder TargetFolder = TargetParent as Folder;
        if (TargetFolder == null || ReferenceEquals(Parent, TargetFolder))
            return false;

        if (Project == null || !ReferenceEquals(Project, TargetFolder.Project))
            return false;

        if (!TargetFolder.CanAddTextFile)
            return false;

        return !ContainsTitle(TargetFolder.Files, Title);
    }
    /// <summary>
    /// Changes the text file parent.
    /// </summary>
    /// <param name="TargetParent">The target parent item.</param>
    /// <returns>True if the text file parent is changed; otherwise false.</returns>
    public override bool ChangeParent(BaseItem TargetParent)
    {
        if (!CanChangeParent(TargetParent))
            return false;

        Folder TargetFolder = TargetParent as Folder;
        BaseItem SourceParent = Parent;
        List<TextFile> SourceItems = GetSourceTextFiles();
        bool Result = MoveItem(SourceItems, TargetFolder.Files, this, TargetFolder);
        if (Result)
        {
            SourceParent.UpdateReferences(SourceParent.Parent);
            TargetFolder.UpdateReferences(TargetFolder.Parent);
        }

        return Result;
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
    /// Gets the synopsis text file name.
    /// </summary>
    static public string SynopsisFileName => "Synopsis.md";
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
    /// Gets the file-system path of the synopsis text file.
    /// </summary>
    [JsonIgnore]
    public string SynopsisFilePath => System.IO.Path.Combine(FolderPath, SynopsisFileName);
    /// <summary>
    /// Gets the file-system path of the draft text file.
    /// </summary>
    [JsonIgnore]
    public string DraftFilePath => System.IO.Path.Combine(FolderPath, DraftFileName);
    /// <summary>
    /// Gets or sets the primary text.
    /// </summary>
    public string Text
    {
        get => fText;
        set => fText = value ?? string.Empty;
    }
    /// <summary>
    /// Gets or sets the secondary text.
    /// </summary>
    public string Text2
    {
        get => fText2;
        set => fText2 = value ?? string.Empty;
    }
    /// <summary>
    /// Gets or sets the synopsis text.
    /// </summary>
    public string Synopsis
    {
        get => fSynopsis;
        set => fSynopsis = value ?? string.Empty;
    }
    /// <summary>
    /// Gets or sets the draft text.
    /// </summary>
    public string Draft
    {
        get => fDraft;
        set => fDraft = value ?? string.Empty;
    }
}
