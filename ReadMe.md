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

`LevelTitle` is needed because documents use a book-structure model:

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

For structured documents, the `Document` folder also contains a
`Structure.json` file. This file stores the folder structure selected by the
user for that document. The document structure is loaded from this file. Flat
documents do not use `Structure.json`.

The document may use one of two mutually exclusive content models:

- Structured document: the document has a folder structure and contains root
  folders.
- Flat document: the document has no folder structure and contains text files
  directly.

For structured documents, the document structure controls what each folder level
may contain:

- Non-leaf folders are containers. They may contain child folders only.
- Leaf folders are text containers. They may contain `TextFile` items only.
- A folder must not contain both child folders and text files.
- `Document` contains root folders only.

For flat documents:

- `Document` contains `TextFile` items directly.
- `Document` must not contain folders.
- No folder structure is used.

Use `Document.SetStructure(FolderItem)` to turn an empty flat document into a
structured document. Use `Document.ClearStructure()` to turn an empty structured
document into a flat document. These operations are rejected when the document
already contains items from the other content model.

For example:

``` text
Project
    Document
        Part
            Chapter
                Section
                    TextFile
```

``` text
Project
    Document
        Chapter
            Scene
                TextFile
```

Child items are stored in type-specific bucket folders. This keeps ordering
separate per parent and per item type, and avoids name collisions between
folders and text files.

The project root stores documents under:

``` text
Documents/
    001._My_Book/
```

Each `Document` stores root folders under:

``` text
Folders/
    001._Part_One/
```

For flat documents, `Document` stores text files under:

``` text
TextFiles/
    001._Opening/
```

Each non-leaf `Folder` stores child folders under:

``` text
Folders/
    001._Chapter_One/
```

Each leaf `Folder` stores text files under:

``` text
TextFiles/
    001._Opening/
```

The `OrderIndex` is scoped by parent and item type. In practice, a valid folder
level uses only one child bucket: either `Folders/` for non-leaf folders, or
`TextFiles/` for leaf folders.

A valid document also uses only one child bucket: either `Folders/` for
structured documents, or `TextFiles/` for flat documents.

## Move Semantics

Moving folders and text files is one of the most sensitive parts of the storage
model because the in-memory tree and the file system must remain synchronized.

A `Folder` may move:

- within the same parent `Document` or `Folder`;
- to the previous or next valid parent container of the same folder level;
- only inside the same `Document`.

A `TextFile` may move:

- within the same `Document` for flat documents;
- within the same leaf parent `Folder` for structured documents;
- to the previous or next leaf folder;
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
