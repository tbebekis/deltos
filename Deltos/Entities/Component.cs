// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Represents a project component with bilingual markdown text and metadata.
/// </summary>
public class Component: BaseItem
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
    /// Field for the Category property.
    /// </summary>
    protected string fCategory = string.Empty;
    /// <summary>
    /// Field for the TagList property.
    /// </summary>
    protected List<string> fTagList = new();
    /// <summary>
    /// Field for the AliasList property.
    /// </summary>
    protected List<string> fAliasList = new();

    // ● protected
    /// <summary>
    /// Updates the item information before saving it.
    /// </summary>
    protected override void UpdateInfo()
    {
        base.UpdateInfo();

        Info.Title = Title;
        Info.Category = Category;
        Info.TagList = ListToText(TagList);
        Info.AliasList = ListToText(AliasList);
    }
    /// <summary>
    /// Applies the item information after loading it.
    /// </summary>
    protected override void ApplyInfoCore()
    {
        base.ApplyInfoCore();

        Title = Info.Title;
        Category = Info.Category;
        Tags = Info.TagList;
        Aliases = Info.AliasList;
    }
    /// <summary>
    /// Checks whether the required component content files exist.
    /// </summary>
    protected virtual void CheckRequiredContentFiles()
    {
        if (!System.IO.File.Exists(TextFilePath))
            throw new InvalidOperationException($"The component text file does not exist: {TextFilePath}");
    }
    /// <summary>
    /// Checks whether the component storage folder contains only known content files.
    /// </summary>
    protected virtual void CheckStorageFolder()
    {
        string[] FolderPaths = System.IO.Directory.GetDirectories(FolderPath);
        if (FolderPaths.Length > 0)
            throw new InvalidOperationException($"Component storage folder contains child folders: {FolderPath}");

        HashSet<string> FileNames = new(StringComparer.OrdinalIgnoreCase);
        FileNames.Add(InfoFileName);
        FileNames.Add(TextFileName);
        FileNames.Add(Text2FileName);

        foreach (string FilePath in System.IO.Directory.GetFiles(FolderPath))
        {
            string FileName = System.IO.Path.GetFileName(FilePath);
            if (!FileNames.Contains(FileName))
                throw new InvalidOperationException($"Component storage folder contains an unknown file: {FilePath}");
        }
    }
    /// <summary>
    /// Checks whether the component can be renamed.
    /// </summary>
    /// <param name="NewTitle">The new title.</param>
    protected override void CheckRenameTitle(string NewTitle)
    {
        Project ProjectItem = Parent as Project;
        if (ProjectItem != null)
            CheckDuplicateTitle(ProjectItem.Components, NewTitle, this);
    }
    /// <summary>
    /// Converts a semicolon-separated text to a list.
    /// </summary>
    /// <param name="Text">The semicolon-separated text.</param>
    /// <returns>The parsed list.</returns>
    static protected List<string> TextToList(string Text)
    {
        List<string> Result = new();
        if (string.IsNullOrWhiteSpace(Text))
            return Result;

        string[] Parts = Text.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (string Part in Parts)
        {
            string Item = Part.Trim();
            if (!string.IsNullOrWhiteSpace(Item) && !Result.Contains(Item, StringComparer.OrdinalIgnoreCase))
                Result.Add(Item);
        }

        return Result;
    }
    /// <summary>
    /// Converts a list to semicolon-separated text.
    /// </summary>
    /// <param name="List">The source list.</param>
    /// <returns>The semicolon-separated text.</returns>
    static protected string ListToText(List<string> List)
    {
        return List == null ? string.Empty : string.Join("; ", List.Where(Item => !string.IsNullOrWhiteSpace(Item)).Select(Item => Item.Trim()));
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the Component class.
    /// </summary>
    public Component()
    {
    }

    // ● public
    /// <summary>
    /// Saves the component to persistent storage.
    /// </summary>
    public override void Save()
    {
        base.Save();
        SaveMarkdownFile(TextFilePath, Text);
        SaveMarkdownFile(Text2FilePath, Text2);
    }
    /// <summary>
    /// Loads the component from persistent storage.
    /// </summary>
    public override void Load()
    {
        base.Load();
        CheckStorageFolder();
        CheckRequiredContentFiles();
        Text = LoadMarkdownFile(TextFilePath);
        Text2 = LoadMarkdownFile(Text2FilePath);
    }
    /// <summary>
    /// Deletes the component from persistent storage.
    /// </summary>
    public override void Delete()
    {
        if (!CanDelete())
            throw new InvalidOperationException("This component cannot be deleted.");

        string ItemFolderPath = FolderPath;
        DeleteStorage(ItemFolderPath);

        Project ProjectItem = Parent as Project;
        if (ProjectItem != null)
            ProjectItem.DetachComponent(this);
    }
    /// <summary>
    /// Returns true if the component contains a tag.
    /// </summary>
    /// <param name="Tag">The tag.</param>
    /// <returns>True if the component contains the tag; otherwise false.</returns>
    public bool ContainsTag(string Tag)
    {
        return TagList.Contains(Tag, StringComparer.OrdinalIgnoreCase);
    }
    /// <summary>
    /// Returns true if the component has an alias.
    /// </summary>
    /// <param name="Alias">The alias.</param>
    /// <returns>True if the component has the alias; otherwise false.</returns>
    public bool HasAlias(string Alias)
    {
        return AliasList.Contains(Alias, StringComparer.OrdinalIgnoreCase);
    }

    // ● properties
    /// <summary>
    /// Gets the item type.
    /// </summary>
    public override ItemType Type => ItemType.Component;
    /// <summary>
    /// Gets the primary text file name.
    /// </summary>
    static public string TextFileName => "Text.md";
    /// <summary>
    /// Gets the secondary text file name.
    /// </summary>
    static public string Text2FileName => "Text2.md";
    /// <summary>
    /// Gets the default component category.
    /// </summary>
    static public string DefaultCategory => "No Category";
    /// <summary>
    /// Gets the component display title.
    /// </summary>
    public override string DisplayTitle => Title;
    /// <summary>
    /// Gets the secondary component display title.
    /// </summary>
    public override string DisplayTitle2 => Title2OrTitle;
    /// <summary>
    /// Gets the file-system storage name of the component folder.
    /// </summary>
    [JsonIgnore]
    public override string StorageName => EncodeTitle(Title);
    /// <summary>
    /// Gets the file-system folder path of the component.
    /// </summary>
    [JsonIgnore]
    public override string FolderPath
    {
        get
        {
            Project ProjectItem = Parent as Project;
            if (ProjectItem != null)
                return System.IO.Path.Combine(ProjectItem.ComponentsFolderPath, StorageName);

            return base.FolderPath;
        }
    }
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
    /// Gets or sets the component category.
    /// </summary>
    public string Category
    {
        get => fCategory;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                fCategory = DefaultCategory;
                return;
            }

            AppHost.CheckValidFileName(value);
            fCategory = value.Trim();
        }
    }
    /// <summary>
    /// Gets or sets the semicolon-separated tags.
    /// </summary>
    public string Tags
    {
        get => ListToText(TagList);
        set => TagList = TextToList(value);
    }
    /// <summary>
    /// Gets or sets the component tag list.
    /// </summary>
    [JsonIgnore]
    public List<string> TagList
    {
        get => fTagList;
        set => fTagList = value ?? new List<string>();
    }
    /// <summary>
    /// Gets or sets the semicolon-separated aliases.
    /// </summary>
    public string Aliases
    {
        get => ListToText(AliasList);
        set => AliasList = TextToList(value);
    }
    /// <summary>
    /// Gets or sets the component alias list.
    /// </summary>
    [JsonIgnore]
    public List<string> AliasList
    {
        get => fAliasList;
        set => fAliasList = value ?? new List<string>();
    }
}
