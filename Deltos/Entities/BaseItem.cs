// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Base class for all project entities.
/// </summary>
public class BaseItem
{
    // ● protected
    /// <summary>
    /// Field for the Id property.
    /// </summary>
    protected string fId = string.Empty;
    /// <summary>
    /// Field for the Title property.
    /// </summary>
    protected string fTitle = string.Empty;
    /// <summary>
    /// Field for the OrderIndex property.
    /// </summary>
    protected int fOrderIndex;
    /// <summary>
    /// Field for the Info property.
    /// </summary>
    protected ItemInfo fInfo = new();
    
    // ● construction
    /// <summary>
    /// Initializes a new instance of the BaseItem class.
    /// </summary>
    public BaseItem()
    {
    }

    // ● protected
    /// <summary>
    /// Updates the item information before saving it.
    /// </summary>
    protected virtual void UpdateInfo()
    {
        Info.Id = Id;
        Info.Title = string.Empty;
        Info.Type = Type;
        Info.Category = string.Empty;
        Info.TagList = string.Empty;
        Info.IsFolder = false;
        Info.LevelTitle = string.Empty;
    }
    /// <summary>
    /// Applies the item information after loading it.
    /// </summary>
    protected virtual void ApplyInfoCore()
    {
        if (Info.Type != ItemType.None && Info.Type != Type)
            throw new InvalidOperationException($"Invalid item info type. Expected {Type}, found {Info.Type}.");

        if (Info.IsFolder != IsFolder)
            throw new InvalidOperationException($"Invalid item folder flag. Expected {IsFolder}, found {Info.IsFolder}.");

        if (!string.IsNullOrWhiteSpace(Info.Id))
            Id = Info.Id;
    }
    /// <summary>
    /// Renumbers a list of child items.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="Items">The items to renumber.</param>
    static protected void RenumberItems<T>(List<T> Items) where T: BaseItem
    {
        List<(string OldFolderPath, string NewFolderPath)> Moves = new();

        for (int Index = 0; Index < Items.Count; Index++)
        {
            T Item = Items[Index];
            string OldFolderPath = Item.OrderIndex > 0 ? Item.FolderPath : string.Empty;
            Item.fOrderIndex = Index + 1;
            string NewFolderPath = Item.FolderPath;

            if (!IsSamePath(OldFolderPath, NewFolderPath) && System.IO.Directory.Exists(OldFolderPath))
                Moves.Add((OldFolderPath, NewFolderPath));
        }

        MoveRenumberedStorage(Moves);
    }
    /// <summary>
    /// Moves an item inside a sibling list.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="Items">The sibling items.</param>
    /// <param name="Item">The item to move.</param>
    /// <param name="NewOrderIndex">The new one-based order index.</param>
    /// <returns>True if the item is moved; otherwise false.</returns>
    static protected bool MoveItem<T>(List<T> Items, T Item, int NewOrderIndex) where T: BaseItem
    {
        int OldIndex = Items.IndexOf(Item);
        if (OldIndex < 0)
            throw new InvalidOperationException("The item does not belong to the specified sibling list.");

        if (NewOrderIndex < 1 || NewOrderIndex > Items.Count)
            throw new ArgumentOutOfRangeException(nameof(NewOrderIndex));

        int NewIndex = NewOrderIndex - 1;
        if (OldIndex == NewIndex)
            return false;

        Items.RemoveAt(OldIndex);
        Items.Insert(NewIndex, Item);
        RenumberItems(Items);
        return true;
    }
    /// <summary>
    /// Moves an item one step inside a sibling list.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="Items">The sibling items.</param>
    /// <param name="Item">The item to move.</param>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <returns>True if the item is moved; otherwise false.</returns>
    static protected bool MoveItem<T>(List<T> Items, T Item, bool Up) where T: BaseItem
    {
        int OldIndex = Items.IndexOf(Item);
        if (OldIndex < 0)
            throw new InvalidOperationException("The item does not belong to the specified sibling list.");

        int NewOrderIndex = Up ? OldIndex : OldIndex + 2;
        return MoveItem(Items, Item, NewOrderIndex);
    }
    /// <summary>
    /// Returns true if an item can move inside a sibling list.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="Items">The sibling items.</param>
    /// <param name="Item">The item to check.</param>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <returns>True if the item can move; otherwise false.</returns>
    static protected bool CanMoveItem<T>(List<T> Items, T Item, bool Up) where T: BaseItem
    {
        int Index = Items.IndexOf(Item);
        if (Index < 0)
            return false;

        return Up ? Index > 0 : Index < Items.Count - 1;
    }
    /// <summary>
    /// Moves an item to another parent sibling list.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="SourceItems">The source sibling items.</param>
    /// <param name="TargetItems">The target sibling items.</param>
    /// <param name="Item">The item to move.</param>
    /// <param name="TargetParent">The target parent item.</param>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <returns>True if the item is moved; otherwise false.</returns>
    static protected bool MoveItem<T>(List<T> SourceItems, List<T> TargetItems, T Item, BaseItem TargetParent, bool Up) where T: BaseItem
    {
        if (!SourceItems.Contains(Item))
            throw new InvalidOperationException("The item does not belong to the specified source list.");

        if (SourceItems == TargetItems)
            return MoveItem(SourceItems, Item, Up);

        CheckCanAddItem(TargetItems);
        CheckDuplicateTitle(TargetItems, Item.Title);

        string OldFolderPath = Item.FolderPath;
        int NewOrderIndex = Up ? TargetItems.Count + 1 : 1;
        string NewFolderPath = GetTargetFolderPath(Item, TargetParent, NewOrderIndex);
        if (!IsSamePath(OldFolderPath, NewFolderPath) && System.IO.Directory.Exists(NewFolderPath))
            throw new InvalidOperationException($"Folder already exists: {NewFolderPath}");

        SourceItems.Remove(Item);

        int InsertIndex = Up ? TargetItems.Count : 0;
        TargetItems.Insert(InsertIndex, Item);
        Item.UpdateReferences(TargetParent);

        RenumberItems(SourceItems);
        RenumberItems(TargetItems);
        MoveStorage(OldFolderPath, Item.FolderPath);
        return true;
    }
    /// <summary>
    /// Returns the target folder path for a cross-parent move.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="Item">The item to move.</param>
    /// <param name="TargetParent">The target parent item.</param>
    /// <param name="NewOrderIndex">The new one-based order index.</param>
    /// <returns>The target folder path.</returns>
    static protected string GetTargetFolderPath<T>(T Item, BaseItem TargetParent, int NewOrderIndex) where T: BaseItem
    {
        string StorageName = GetStorageName(NewOrderIndex, Item.Title);

        Document DocumentParent = TargetParent as Document;
        if (DocumentParent != null)
        {
            if (Item is Folder)
                return System.IO.Path.Combine(DocumentParent.FoldersFolderPath, StorageName);

            if (Item is TextFile)
                return System.IO.Path.Combine(DocumentParent.TextFilesFolderPath, StorageName);
        }

        Folder FolderParent = TargetParent as Folder;
        if (FolderParent != null)
        {
            if (Item is Folder)
                return System.IO.Path.Combine(FolderParent.FoldersFolderPath, StorageName);

            if (Item is TextFile)
                return System.IO.Path.Combine(FolderParent.TextFilesFolderPath, StorageName);
        }

        return System.IO.Path.Combine(TargetParent.FolderPath, StorageName);
    }
    /// <summary>
    /// Moves an item folder from one path to another.
    /// </summary>
    /// <param name="OldFolderPath">The old folder path.</param>
    /// <param name="NewFolderPath">The new folder path.</param>
    static protected void MoveStorage(string OldFolderPath, string NewFolderPath)
    {
        if (IsSamePath(OldFolderPath, NewFolderPath) || !System.IO.Directory.Exists(OldFolderPath))
            return;

        if (System.IO.Directory.Exists(NewFolderPath))
            throw new InvalidOperationException($"Folder already exists: {NewFolderPath}");

        string ParentFolder = System.IO.Path.GetDirectoryName(NewFolderPath);
        if (!string.IsNullOrWhiteSpace(ParentFolder))
            System.IO.Directory.CreateDirectory(ParentFolder);

        System.IO.Directory.Move(OldFolderPath, NewFolderPath);
    }
    /// <summary>
    /// Returns an adjacent container that may contain a folder of the specified level.
    /// </summary>
    /// <param name="DocumentItem">The owning document.</param>
    /// <param name="CurrentContainer">The current parent container.</param>
    /// <param name="FolderLevel">The moving folder level.</param>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <param name="MovingFolder">The moving folder.</param>
    /// <returns>The adjacent move container, or null.</returns>
    static protected BaseItem GetAdjacentFolderMoveContainer(Document DocumentItem, BaseItem CurrentContainer, int FolderLevel, bool Up, Folder MovingFolder)
    {
        if (DocumentItem == null || CurrentContainer == null)
            return null;

        List<BaseItem> Containers = DocumentItem.GetFolderMoveContainers(FolderLevel);
        return GetAdjacentMoveContainer(Containers, CurrentContainer, Up, MovingFolder);
    }
    /// <summary>
    /// Returns an adjacent container that may contain text files.
    /// </summary>
    /// <param name="DocumentItem">The owning document.</param>
    /// <param name="CurrentContainer">The current parent container.</param>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <returns>The adjacent move container, or null.</returns>
    static protected BaseItem GetAdjacentTextFileMoveContainer(Document DocumentItem, BaseItem CurrentContainer, bool Up)
    {
        if (DocumentItem == null || CurrentContainer == null)
            return null;

        List<BaseItem> Containers = DocumentItem.GetTextFileMoveContainers();
        int Index = Containers.IndexOf(CurrentContainer);
        if (Index < 0)
            return null;

        int TargetIndex = Up ? Index - 1 : Index + 1;
        return TargetIndex >= 0 && TargetIndex < Containers.Count ? Containers[TargetIndex] : null;
    }
    /// <summary>
    /// Returns an adjacent valid move container from a specified container list.
    /// </summary>
    /// <param name="Containers">The available containers.</param>
    /// <param name="CurrentContainer">The current parent container.</param>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <param name="MovingFolder">The moving folder.</param>
    /// <returns>The adjacent move container, or null.</returns>
    static protected BaseItem GetAdjacentMoveContainer(List<BaseItem> Containers, BaseItem CurrentContainer, bool Up, Folder MovingFolder)
    {
        int Index = Containers.IndexOf(CurrentContainer);
        if (Index < 0)
            return null;

        int Step = Up ? -1 : 1;
        for (int I = Index + Step; I >= 0 && I < Containers.Count; I += Step)
        {
            BaseItem Container = Containers[I];
            if (IsValidFolderMoveContainer(Container, MovingFolder))
                return Container;
        }

        return null;
    }
    /// <summary>
    /// Returns true if a container can receive the moving folder.
    /// </summary>
    /// <param name="Container">The container to check.</param>
    /// <param name="MovingFolder">The moving folder.</param>
    /// <returns>True if the container can receive the moving folder; otherwise false.</returns>
    static protected bool IsValidFolderMoveContainer(BaseItem Container, Folder MovingFolder)
    {
        if (MovingFolder == null)
            return true;

        if (ReferenceEquals(Container, MovingFolder))
            return false;

        Folder FolderContainer = Container as Folder;
        return FolderContainer == null || !MovingFolder.ContainsFolder(FolderContainer);
    }
    /// <summary>
    /// Returns true if two folder paths point to the same path.
    /// </summary>
    /// <param name="A">The first path.</param>
    /// <param name="B">The second path.</param>
    /// <returns>True if the two paths are the same; otherwise false.</returns>
    static protected bool IsSamePath(string A, string B)
    {
        if (string.IsNullOrWhiteSpace(A) || string.IsNullOrWhiteSpace(B))
            return string.IsNullOrWhiteSpace(A) && string.IsNullOrWhiteSpace(B);

        string PathA = System.IO.Path.GetFullPath(A).TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
        string PathB = System.IO.Path.GetFullPath(B).TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
        return string.Equals(PathA, PathB, StringComparison.Ordinal);
    }
    /// <summary>
    /// Moves existing item folders after renumbering.
    /// </summary>
    /// <param name="Moves">The folder moves to execute.</param>
    static protected void MoveRenumberedStorage(List<(string OldFolderPath, string NewFolderPath)> Moves)
    {
        if (Moves.Count == 0)
            return;

        foreach ((string OldFolderPath, string NewFolderPath) in Moves)
        {
            bool TargetIsSource = Moves.Exists(Move => IsSamePath(Move.OldFolderPath, NewFolderPath));
            if (System.IO.Directory.Exists(NewFolderPath) && !TargetIsSource)
                throw new InvalidOperationException($"Folder already exists: {NewFolderPath}");
        }

        List<(string TempFolderPath, string NewFolderPath)> StagedMoves = new();
        foreach ((string OldFolderPath, string NewFolderPath) in Moves)
        {
            string ParentFolder = System.IO.Path.GetDirectoryName(OldFolderPath);
            if (string.IsNullOrWhiteSpace(ParentFolder))
                throw new InvalidOperationException($"Invalid folder path: {OldFolderPath}");

            string TempFolderPath = System.IO.Path.Combine(ParentFolder, $".deltos-renumber-{System.Guid.NewGuid():N}");
            System.IO.Directory.Move(OldFolderPath, TempFolderPath);
            StagedMoves.Add((TempFolderPath, NewFolderPath));
        }

        foreach ((string TempFolderPath, string NewFolderPath) in StagedMoves)
        {
            string ParentFolder = System.IO.Path.GetDirectoryName(NewFolderPath);
            if (!string.IsNullOrWhiteSpace(ParentFolder))
                System.IO.Directory.CreateDirectory(ParentFolder);

            System.IO.Directory.Move(TempFolderPath, NewFolderPath);
        }
    }
    /// <summary>
    /// Saves the item information file.
    /// </summary>
    protected virtual void SaveInfo()
    {
        PrepareInfo();
        System.IO.Directory.CreateDirectory(FolderPath);
        Json.SaveToFile(Info, InfoFilePath);
    }
    /// <summary>
    /// Loads the item information file.
    /// </summary>
    protected virtual void LoadInfo()
    {
        if (!System.IO.File.Exists(InfoFilePath))
            throw new InvalidOperationException($"The item information file does not exist: {InfoFilePath}");

        Json.LoadFromFile(Info, InfoFilePath);
        if (string.IsNullOrWhiteSpace(Info.Id))
            throw new InvalidOperationException($"The item information file has no item id: {InfoFilePath}");

        if (Info.Type == ItemType.None)
            throw new InvalidOperationException($"The item information file has no item type: {InfoFilePath}");

        ApplyInfo();
    }
    /// <summary>
    /// Loads child items from a folder.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="FolderPath">The folder path.</param>
    /// <returns>The loaded items.</returns>
    protected List<T> LoadItems<T>(string FolderPath) where T: BaseItem, new()
    {
        List<T> Result = new();

        if (System.IO.Directory.Exists(FolderPath))
        {
            string[] FilePaths = System.IO.Directory.GetFiles(FolderPath);
            if (FilePaths.Length > 0)
                throw new InvalidOperationException($"Storage bucket contains files: {FolderPath}");

            string[] FolderPaths = System.IO.Directory.GetDirectories(FolderPath);
            foreach (string ItemFolderPath in FolderPaths)
            {
                string StorageName = System.IO.Path.GetFileName(ItemFolderPath);
                if (!TryParseStorageName(StorageName, out _, out _, out _))
                    throw new InvalidOperationException($"Invalid item storage folder name: {ItemFolderPath}");

                T Item = new();
                Item.SetStorageName(StorageName);
                Item.UpdateReferences(this);
                Item.Load();
                Result.Add(Item);
            }
        }

        Result.Sort((A, B) => A.OrderIndex.CompareTo(B.OrderIndex));
        CheckLoadedItems(Result, FolderPath);
        return Result;
    }
    /// <summary>
    /// Checks loaded items for ambiguous order or duplicate titles.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="Items">The loaded items.</param>
    /// <param name="FolderPath">The source folder path.</param>
    protected void CheckLoadedItems<T>(List<T> Items, string FolderPath) where T: BaseItem
    {
        for (int Index = 0; Index < Items.Count; Index++)
        {
            T Item = Items[Index];
            int ExpectedOrderIndex = Index + 1;
            if (Item.OrderIndex != ExpectedOrderIndex)
                throw new InvalidOperationException($"Expected item order {ExpectedOrderIndex:000} but found {Item.OrderIndex:000} in folder: {FolderPath}");

            for (int OtherIndex = Index + 1; OtherIndex < Items.Count; OtherIndex++)
            {
                T OtherItem = Items[OtherIndex];
                if (Item.OrderIndex == OtherItem.OrderIndex)
                    throw new InvalidOperationException($"Duplicate item order {Item.OrderIndex:000} in folder: {FolderPath}");

                if (Item.Title.IsSameText(OtherItem.Title))
                    throw new InvalidOperationException($"Duplicate item title {Item.Title} in folder: {FolderPath}");

                if (Item.Id.IsSameText(OtherItem.Id))
                    throw new InvalidOperationException($"Duplicate item id {Item.Id} in folder: {FolderPath}");
            }
        }
    }
    /// <summary>
    /// Deletes the item folder from persistent storage.
    /// </summary>
    /// <param name="ItemFolderPath">The item folder path.</param>
    protected virtual void DeleteStorage(string ItemFolderPath)
    {
        if (!string.IsNullOrWhiteSpace(ItemFolderPath) && System.IO.Directory.Exists(ItemFolderPath))
            System.IO.Directory.Delete(ItemFolderPath, true);
    }
    /// <summary>
    /// Checks that an unused storage bucket is empty.
    /// </summary>
    /// <param name="FolderPath">The unused bucket folder path.</param>
    protected virtual void CheckUnusedStorageBucket(string FolderPath)
    {
        if (System.IO.Directory.Exists(FolderPath) && System.IO.Directory.EnumerateFileSystemEntries(FolderPath).Any())
            throw new InvalidOperationException($"Unused storage bucket contains items: {FolderPath}");
    }
    /// <summary>
    /// Returns true if the item has a resolved project storage path.
    /// </summary>
    /// <returns>True if the item can persist storage; otherwise false.</returns>
    protected bool CanPersistStorage()
    {
        Project ProjectItem = this as Project;
        if (ProjectItem != null)
            return !string.IsNullOrWhiteSpace(ProjectItem.ProjectPath) && System.IO.Path.IsPathFullyQualified(ProjectItem.ProjectPath);

        return Project != null && !string.IsNullOrWhiteSpace(Project.ProjectPath) && System.IO.Path.IsPathFullyQualified(Project.ProjectPath);
    }
    /// <summary>
    /// Saves the item if it has a resolved project storage path.
    /// </summary>
    /// <param name="Item">The item to save.</param>
    protected void SaveItemIfStorageReady(BaseItem Item)
    {
        if (CanPersistStorage())
            Item.Save();
    }
    /// <summary>
    /// Returns true if an item list can receive another child item.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="Items">The item list.</param>
    /// <returns>True if another item can be added; otherwise false.</returns>
    static protected bool CanAddItem<T>(List<T> Items) where T: BaseItem
    {
        return Items.Count < MaxOrderIndex;
    }
    /// <summary>
    /// Checks that an item list can receive another child item.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="Items">The item list.</param>
    static protected void CheckCanAddItem<T>(List<T> Items) where T: BaseItem
    {
        if (!CanAddItem(Items))
            throw new InvalidOperationException($"The item list cannot contain more than {MaxOrderIndex} items.");
    }
    /// <summary>
    /// Checks whether the item can be renamed.
    /// </summary>
    /// <param name="NewTitle">The new title.</param>
    protected virtual void CheckRenameTitle(string NewTitle)
    {
    }
    /// <summary>
    /// Throws an exception if the specified title already exists in a sibling list.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="Items">The sibling items.</param>
    /// <param name="Title">The title to check.</param>
    static protected void CheckDuplicateTitle<T>(List<T> Items, string Title) where T: BaseItem
    {
        string TrimmedTitle = Title == null ? string.Empty : Title.Trim();
        foreach (T Item in Items)
        {
            if (Item.Title.IsSameText(TrimmedTitle))
                throw new InvalidOperationException($"An item with the same title already exists: {Title}");
        }
    }
    /// <summary>
    /// Throws an exception if the specified title already exists in a sibling list.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="Items">The sibling items.</param>
    /// <param name="Title">The title to check.</param>
    /// <param name="IgnoredItem">The item to ignore.</param>
    static protected void CheckDuplicateTitle<T>(List<T> Items, string Title, T IgnoredItem) where T: BaseItem
    {
        string TrimmedTitle = Title == null ? string.Empty : Title.Trim();
        foreach (T Item in Items)
        {
            if (!ReferenceEquals(Item, IgnoredItem) && Item.Title.IsSameText(TrimmedTitle))
                throw new InvalidOperationException($"An item with the same title already exists: {Title}");
        }
    }
    /// <summary>
    /// Returns true if the specified title already exists in a sibling list.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="Items">The sibling items.</param>
    /// <param name="Title">The title to check.</param>
    /// <returns>True if the title exists; otherwise false.</returns>
    static protected bool ContainsTitle<T>(List<T> Items, string Title) where T: BaseItem
    {
        string TrimmedTitle = Title == null ? string.Empty : Title.Trim();
        foreach (T Item in Items)
        {
            if (Item.Title.IsSameText(TrimmedTitle))
                return true;
        }

        return false;
    }
    
    // ● static public
    /// <summary>
    /// Converts an item title to a file-system title segment.
    /// </summary>
    /// <param name="Title">The item title.</param>
    /// <returns>The file-system title segment.</returns>
    static public string EncodeTitle(string Title)
    {
        AppHost.CheckValidFileName(Title);
        return Title.Trim().Replace(' ', '_');
    }
    /// <summary>
    /// Converts a file-system title segment to an item title.
    /// </summary>
    /// <param name="Title">The file-system title segment.</param>
    /// <returns>The item title.</returns>
    static public string DecodeTitle(string Title)
    {
        if (Title == null)
            return string.Empty;

        return Title.Replace('_', ' ');
    }
    /// <summary>
    /// Returns the storage name for an ordered item title.
    /// </summary>
    /// <param name="OrderIndex">The order index.</param>
    /// <param name="Title">The item title.</param>
    /// <returns>The storage name.</returns>
    static public string GetStorageName(int OrderIndex, string Title)
    {
        if (OrderIndex < 1)
            throw new ArgumentOutOfRangeException(nameof(OrderIndex));

        if (OrderIndex > MaxOrderIndex)
            throw new ArgumentOutOfRangeException(nameof(OrderIndex));

        return $"{OrderIndex:000}._{EncodeTitle(Title)}";
    }
    /// <summary>
    /// Returns the display title for an ordered item title.
    /// </summary>
    /// <param name="OrderIndex">The order index.</param>
    /// <param name="Title">The item title.</param>
    /// <returns>The display title.</returns>
    static public string GetDisplayTitle(int OrderIndex, string Title)
    {
        return DecodeTitle(GetStorageName(OrderIndex, Title));
    }
    /// <summary>
    /// Tries to parse a storage name.
    /// </summary>
    /// <param name="StorageName">The storage name.</param>
    /// <param name="OrderIndex">The parsed order index.</param>
    /// <param name="Title">The parsed title.</param>
    /// <param name="DisplayTitle">The parsed display title.</param>
    /// <returns>True if the storage name is parsed successfully; otherwise false.</returns>
    static public bool TryParseStorageName(string StorageName, out int OrderIndex, out string Title, out string DisplayTitle)
    {
        OrderIndex = 0;
        Title = string.Empty;
        DisplayTitle = string.Empty;

        if (string.IsNullOrWhiteSpace(StorageName) || StorageName.Length < 5)
            return false;

        if (StorageName[3] != '.')
            return false;

        if (!int.TryParse(StorageName.Substring(0, 3), out OrderIndex))
            return false;

        if (OrderIndex < 1)
            return false;

        string EncodedTitle = StorageName.Substring(4);
        if (EncodedTitle.StartsWith("_"))
            EncodedTitle = EncodedTitle.Substring(1);

        Title = DecodeTitle(EncodedTitle);
        DisplayTitle = DecodeTitle(StorageName);

        if (Title != Title.Trim())
            return false;

        if (!AppHost.IsValidFileName(Title, false))
            return false;

        return !string.IsNullOrWhiteSpace(Title);
    }

    // ● public
    /// <summary>
    /// Sets the item title fields from a storage name.
    /// </summary>
    /// <param name="StorageName">The storage name.</param>
    public virtual void SetStorageName(string StorageName)
    {
        if (!TryParseStorageName(StorageName, out int OrderIndex, out string Title, out string DisplayTitle))
            throw new ArgumentException($"Invalid storage name: {StorageName}", nameof(StorageName));

        fOrderIndex = OrderIndex;
        fTitle = Title.Trim();
    }
    /// <summary>
    /// Prepares persisted item information before saving the item.
    /// </summary>
    public virtual void PrepareInfo()
    {
        UpdateInfo();
    }
    /// <summary>
    /// Applies persisted item information after loading the item.
    /// </summary>
    public virtual void ApplyInfo()
    {
        ApplyInfoCore();
    }
    /// <summary>
    /// Renumbers child items.
    /// </summary>
    public virtual void RenumberChildren()
    {
    }
    /// <summary>
    /// Updates runtime references after loading the item graph.
    /// </summary>
    /// <param name="ParentItem">The parent item.</param>
    public virtual void UpdateReferences(BaseItem ParentItem)
    {
        Parent = ParentItem;
        Project = ParentItem == null ? Project : ParentItem.Project;
    }
    /// <summary>
    /// Clears runtime references when the item is detached from its parent.
    /// </summary>
    public virtual void ClearReferences()
    {
        Parent = null;
        Project = null;
        fOrderIndex = 0;
    }
    /// <summary>
    /// Returns the visible child items.
    /// </summary>
    /// <returns>The visible child items.</returns>
    public virtual List<BaseItem> GetChildItems()
    {
        return new List<BaseItem>();
    }
    /// <summary>
    /// Returns all descendant items.
    /// </summary>
    /// <param name="IncludeSelf">True to include this item in the result.</param>
    /// <returns>The descendant items.</returns>
    public virtual List<BaseItem> GetDescendantItems(bool IncludeSelf = false)
    {
        List<BaseItem> Result = new();
        if (IncludeSelf)
            Result.Add(this);

        foreach (BaseItem ChildItem in GetChildItems())
        {
            Result.Add(ChildItem);
            Result.AddRange(ChildItem.GetDescendantItems());
        }

        return Result;
    }
    /// <summary>
    /// Returns true if the item can move in the specified direction.
    /// </summary>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <returns>True if the item can move; otherwise false.</returns>
    public virtual bool CanMove(bool Up)
    {
        return false;
    }
    /// <summary>
    /// Returns true if the item can be renamed.
    /// </summary>
    /// <returns>True if the item can be renamed; otherwise false.</returns>
    public virtual bool CanRename()
    {
        return true;
    }
    /// <summary>
    /// Returns true if the item can be deleted from its parent.
    /// </summary>
    /// <returns>True if the item can be deleted; otherwise false.</returns>
    public virtual bool CanDelete()
    {
        return Parent != null;
    }
    /// <summary>
    /// Deletes the item from its parent and from persistent storage.
    /// </summary>
    /// <returns>True if the item is deleted; otherwise false.</returns>
    public virtual bool DeleteFromParent()
    {
        return CanDelete() && Parent != null && Parent.RemoveChild(this);
    }
    /// <summary>
    /// Moves the item one step in the specified direction.
    /// </summary>
    /// <param name="Up">True for upward movement; false for downward movement.</param>
    /// <returns>True if the item is moved; otherwise false.</returns>
    public virtual bool Move(bool Up)
    {
        return false;
    }
    /// <summary>
    /// Sets a document folder structure.
    /// </summary>
    /// <param name="Structure">The document folder structure.</param>
    public virtual void SetStructure(FolderItem Structure)
    {
        throw new InvalidOperationException("This item does not support a document structure.");
    }
    /// <summary>
    /// Clears a document folder structure.
    /// </summary>
    public virtual void ClearStructure()
    {
        throw new InvalidOperationException("This item does not support a document structure.");
    }
    /// <summary>
    /// Adds a child document.
    /// </summary>
    /// <param name="Title">The document title.</param>
    /// <returns>The added document.</returns>
    public virtual Document AddDocument(string Title)
    {
        throw new InvalidOperationException("This item cannot contain child documents.");
    }
    /// <summary>
    /// Adds a structured child document.
    /// </summary>
    /// <param name="Title">The document title.</param>
    /// <param name="Structure">The document folder structure.</param>
    /// <returns>The added document.</returns>
    public virtual Document AddDocument(string Title, FolderItem Structure)
    {
        throw new InvalidOperationException("This item cannot contain child documents.");
    }
    /// <summary>
    /// Adds a child folder.
    /// </summary>
    /// <param name="Title">The folder title.</param>
    /// <param name="LevelTitle">The folder level title.</param>
    /// <returns>The added folder.</returns>
    public virtual Folder AddFolder(string Title, string LevelTitle)
    {
        throw new InvalidOperationException("This item cannot contain child folders.");
    }
    /// <summary>
    /// Adds a child text file.
    /// </summary>
    /// <param name="Title">The text file title.</param>
    /// <returns>The added text file.</returns>
    public virtual TextFile AddTextFile(string Title)
    {
        throw new InvalidOperationException("This item cannot contain text files.");
    }
    /// <summary>
    /// Deletes a child item from memory and persistent storage.
    /// </summary>
    /// <param name="Item">The child item to delete.</param>
    /// <returns>True if the child item is deleted; otherwise false.</returns>
    public virtual bool RemoveChild(BaseItem Item)
    {
        return false;
    }
    /// <summary>
    /// Saves the item to persistent storage.
    /// </summary>
    public virtual void Save()
    {
        SaveInfo();
    }
    /// <summary>
    /// Loads the item from persistent storage.
    /// </summary>
    public virtual void Load()
    {
        LoadInfo();
    }
    /// <summary>
    /// Deletes the item from persistent storage.
    /// </summary>
    public virtual void Delete()
    {
        if (!CanDelete())
            throw new InvalidOperationException("This item cannot be deleted.");

        DeleteStorage(FolderPath);
    }
    /// <summary>
    /// Renames the item and its folder in persistent storage.
    /// </summary>
    /// <param name="NewTitle">The new title.</param>
    public virtual void Rename(string NewTitle)
    {
        if (!CanRename())
            throw new InvalidOperationException("This item cannot be renamed.");

        string OldTitle = Title;
        string OldFolderPath = FolderPath;
        CheckRenameTitle(NewTitle);

        Title = NewTitle;

        string NewFolderPath = FolderPath;
        if (!IsSamePath(OldFolderPath, NewFolderPath))
        {
            if (System.IO.Directory.Exists(NewFolderPath))
            {
                Title = OldTitle;
                throw new InvalidOperationException($"Folder already exists: {NewFolderPath}");
            }

            if (System.IO.Directory.Exists(OldFolderPath))
            {
                string ParentFolder = System.IO.Path.GetDirectoryName(NewFolderPath);
                if (!string.IsNullOrWhiteSpace(ParentFolder))
                    System.IO.Directory.CreateDirectory(ParentFolder);

                try
                {
                    System.IO.Directory.Move(OldFolderPath, NewFolderPath);
                }
                catch
                {
                    Title = OldTitle;
                    throw;
                }
            }
        }

        if (System.IO.Directory.Exists(NewFolderPath))
            SaveInfo();
    }
    
    // ● properties
    /// <summary>
    /// Gets the maximum supported order index for three-digit storage names.
    /// </summary>
    static public int MaxOrderIndex => 999;
    /// <summary>
    /// Gets the parent item.
    /// </summary>
    [JsonIgnore]
    public virtual BaseItem Parent { get; protected set; }
    /// <summary>
    /// Gets the owning project.
    /// </summary>
    [JsonIgnore]
    public virtual Project Project { get; protected set; }
    /// <summary>
    /// Gets the owning document.
    /// </summary>
    [JsonIgnore]
    public virtual Document Document
    {
        get
        {
            Document Result = this as Document;
            if (Result != null)
                return Result;

            return Parent == null ? null : Parent.Document;
        }
    }
    /// <summary>
    /// Gets the item type.
    /// </summary>
    public virtual ItemType Type => ItemType.None;
    /// <summary>
    /// Gets a value indicating whether this item is a project.
    /// </summary>
    [JsonIgnore]
    public bool IsProject => Type == ItemType.Project;
    /// <summary>
    /// Gets a value indicating whether this item is a document.
    /// </summary>
    [JsonIgnore]
    public bool IsDocument => Type == ItemType.Document;
    /// <summary>
    /// Gets a value indicating whether this item is a folder.
    /// </summary>
    [JsonIgnore]
    public bool IsFolder => Type == ItemType.Folder;
    /// <summary>
    /// Gets a value indicating whether this item is a text file.
    /// </summary>
    [JsonIgnore]
    public bool IsTextFile => Type == ItemType.TextFile;
    /// <summary>
    /// Gets a value indicating whether this item can contain documents.
    /// </summary>
    [JsonIgnore]
    public virtual bool CanContainDocuments => false;
    /// <summary>
    /// Gets a value indicating whether this item can contain folders.
    /// </summary>
    [JsonIgnore]
    public virtual bool CanContainFolders => false;
    /// <summary>
    /// Gets a value indicating whether this item can contain text files.
    /// </summary>
    [JsonIgnore]
    public virtual bool CanContainTextFiles => false;
    /// <summary>
    /// Gets a value indicating whether a document can be added to this item.
    /// </summary>
    [JsonIgnore]
    public virtual bool CanAddDocument => false;
    /// <summary>
    /// Gets a value indicating whether a structured document can be added to this item.
    /// </summary>
    [JsonIgnore]
    public virtual bool CanAddStructuredDocument => false;
    /// <summary>
    /// Gets a value indicating whether a folder structure can be set.
    /// </summary>
    [JsonIgnore]
    public virtual bool CanSetStructure => false;
    /// <summary>
    /// Gets a value indicating whether a folder structure can be cleared.
    /// </summary>
    [JsonIgnore]
    public virtual bool CanClearStructure => false;
    /// <summary>
    /// Gets a value indicating whether a folder can be added to this item.
    /// </summary>
    [JsonIgnore]
    public virtual bool CanAddFolder => false;
    /// <summary>
    /// Gets a value indicating whether a text file can be added to this item.
    /// </summary>
    [JsonIgnore]
    public virtual bool CanAddTextFile => false;
    /// <summary>
    /// Gets or sets the unique item identifier.
    /// </summary>
    public string Id
    {
        get
        {
            if (string.IsNullOrWhiteSpace(fId))
                fId = Sys.GenId(UseBrackets: false);
            return fId;
        }
        set => fId = value == null ? string.Empty : value.Trim();
    }
    /// <summary>
    /// Gets or sets the item title.
    /// </summary>
    public virtual string Title
    {
        get => fTitle;
        set
        {
            AppHost.CheckValidFileName(value);
            fTitle = value.Trim();
        }
    }
    /// <summary>
    /// Gets the display title of the item.
    /// </summary>
    public virtual string DisplayTitle
    {
        get
        {
            if (IsProject || OrderIndex <= 0)
                return Title;

            return GetDisplayTitle(OrderIndex, Title);
        }
    }
    /// <summary>
    /// Gets the file-system storage name of the item folder.
    /// </summary>
    [JsonIgnore]
    public virtual string StorageName => GetStorageName(OrderIndex, Title);
    /// <summary>
    /// Gets the file-system folder path of the item.
    /// </summary>
    [JsonIgnore]
    public virtual string FolderPath
    {
        get
        {
            if (Parent == null)
                return string.Empty;

            return System.IO.Path.Combine(Parent.FolderPath, StorageName);
        }
    }
    /// <summary>
    /// Gets the item order index among its siblings.
    /// </summary>
    public virtual int OrderIndex => fOrderIndex;
    /// <summary>
    /// Gets or sets the persisted item information.
    /// </summary>
    public virtual ItemInfo Info
    {
        get => fInfo;
        set => fInfo = value ?? new ItemInfo();
    }
    /// <summary>
    /// Gets the file-system path of the item information file.
    /// </summary>
    [JsonIgnore]
    public string InfoFilePath => System.IO.Path.Combine(FolderPath, InfoFileName);
    /// <summary>
    /// Gets the item information file name.
    /// </summary>
    static public string InfoFileName => "Info.json";
}
