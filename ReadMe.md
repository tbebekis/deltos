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
- `FolderTitle`: valid only for folders. Example values are `Part`, `Chapter`,
  `Section`, and similar user-defined level names.

`FolderTitle` is needed because documents use the following model:

``` text
Project
    Document
        Folder
            Folder
            TextFile
```

When the user creates a new `Document`, the user defines the folder structure of
that document. This is represented by `Document.Structure` and the `FolderItem`
class. The structure defines the tree of folder levels and the `FolderTitle` for
each level.

The `Document` folder also contains a `Structure.json` file. This file stores
the folder structure selected by the user for that document. The document
structure is loaded from this file.

A folder `BaseItem` may contain one or more child folder `BaseItem` instances
and one or more `TextFile` items.

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
