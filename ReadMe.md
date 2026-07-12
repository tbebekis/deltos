# Deltos

## Project Storage

A Deltos project is stored entirely in the folder pointed to by
`Project.ProjectPath`.

`ProjectPath` is ignored by JSON serialization. It is assigned when the user
executes Open Project from the UI, and it points to the root folder that
contains the whole project.

There is no `index.json` or central file describing the project structure. The
structure is derived from the file system. The order of the items is also
derived from the file system names.

All entities are `BaseItem` instances, including `Project`. More entity types,
such as `Component` and `Note`, will be added later.

Each `BaseItem` owns its own folder in the file system. The folder name is based
on the `Title` of the item, prefixed with a three digit order number, a dot, and
a space:

``` text
001. The Corp of the World
```

The title must pass `AppHost.IsValidFileName`. In practice, folder and file
names do not contain spaces. Spaces are replaced with underscores when written
to disk:

``` text
001._The_Corp_of_the_World
```

When reading an item from disk:

- The `XXX` prefix is parsed as `OrderIndex`.
- The remaining name segment is used as `Title`, with underscores converted
  back to spaces.
- The whole folder or file name is used as `DisplayTitle`, with underscores
  converted back to spaces.

Each `BaseItem` folder contains an `Info.json` file. This file stores
`BaseItem.Info`.

`Info.json` contains:

- `Id`: the `Id` of the `BaseItem`.
- `Type`: the `Type` of the `BaseItem`.
- `Category`: valid only for `Component` items.
- `TagList`: a semicolon-separated list of textual tags, valid only for
  `Component` items.
- `IsFolder`: `true` or `false`.
- `LevelTitle`: valid only for folders. Example values are `Part`, `Chapter`,
  `Section`, and similar user-defined level names.

`LevelTitle` is needed because documents use the following model:

``` text
Project
    Document
        Folder
            Folder
            TextFile
```

When the user creates a new `Document`, the user defines the folder structure of
that document. This is represented by `Document.Structure` and the `FolderItem`
class. The structure defines the tree of folder levels and the `LevelTitle` for
each level.

The `Document` folder also contains a `Structure.json` file. This file stores
the folder structure selected by the user for that document. The document
structure is loaded from this file.

A folder `BaseItem` may contain one or more child folder `BaseItem` instances
and one or more `TextFile` items.

Child items are stored in type-specific bucket folders. This keeps ordering
separate per parent and per item type, and avoids name collisions between
folders and text files.

The project root stores documents under:

``` text
Documents/
    001._My_Book/
```

Each `Document` and each `Folder` stores child folders and text files under:

``` text
Folders/
    001._Part_One/
TextFiles/
    001._Opening/
```

The `OrderIndex` is scoped by parent and item type. For example, child folders
under `Folders/` have their own numbering, and text files under `TextFiles/`
have their own numbering.

## Move Semantics

Moving folders and text files is one of the most sensitive parts of the storage
model because the in-memory tree and the file system must remain synchronized.

A `Folder` may move:

- within the same parent `Document` or `Folder`;
- to the previous or next parent folder;
- only inside the same `Document`.

A `TextFile` may move:

- within the same parent `Document` or `Folder`;
- to the previous or next parent folder;
- only inside the same `Document`.

Every move must update:

- the source parent collection;
- the target parent collection;
- runtime references such as `Parent`, `Project`, and `Document`;
- `OrderIndex` values for the source sibling bucket;
- `OrderIndex` values for the target sibling bucket;
- the corresponding file-system folder paths.

The file-system operation must move or rename the affected item folders. A move
must be designed carefully to avoid collisions, partial updates, and broken
references. The previous StoryWriter implementation used a path snapshot before
the move, then changed the in-memory list or parent, and finally moved the
folder from the old path to the new computed path. Deltos should follow the same
principle, but with extra care for the `Documents`, `Folders`, and `TextFiles`
bucket folders.

The old StoryWriter codebase at
`/home/teo/Dev/CSharp/tb.StoryWriter/StoryWriter.App` is a loose reference for
this behavior, especially the `Entities/Scene.cs` implementation.

Move implementation is intentionally deferred until this operation can be
designed as a safe, transaction-like workflow.

Although these items are called `TextFile`, each one is stored in its own folder
in the file system. A `TextFile` folder contains:

- `Info.json`.
- `Text.md`.
- `Text2.md`.
- `Abstraction.md`.
- `Draft.md`.

`Project.Load()` loads the entire project into memory. Immediately after
loading, it calls `UpdateReferences(null)`. This causes
`UpdateReferences(BaseItem ParentItem)` to be called for all child items as
well.
