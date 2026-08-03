# Deltos Documentation

## Overview

Deltos is a desktop writing workspace for long-form projects.

The application stores projects as ordinary folders, Markdown files, and JSON
metadata. The main writing surface is a visible document tree with editors,
previews, metrics, notes, components, search, quick view, wiki generation, and
Git commands.

## Projects

A project is the root workspace. It contains:

- Documents.
- Components.
- Notes.
- Images.
- Temporary text.
- Project settings.
- Wiki and export output.

The project storage version is tracked in `ProjectManifest.json`.

## Documents And Items

A document contains an ordered mixed item list. Each child item can be either a
folder or a text file.

Folders are containers too, and can also contain an ordered mix of folders and
text files.

Example:

```text
Document
    Preface
    Introduction
    Part I
        Chapter 1
        Chapter 2
            Scene 1
            Scene 2
    Back Matter
        Glossary
```

Items are numbered only in storage folder names. The UI shows clean item titles
without the storage prefixes.

## Document Structure

Document structure is a template for folder level names.

It helps the UI create folders such as Part, Chapter, Section, or Scene, but it
does not restrict where text files may be placed.

## Add And Edit Item

The Add/Edit item dialog edits shared metadata for documents, folders, and text
files.

Fields:

- Type.
- Level.
- Title.
- Title 2.
- Include title in output.
- Page break before.
- Include in TOC.
- Numbering.
- Custom numbering.

The item type cannot be changed after creation.

The output metadata is stored in `Info.json` and is applied during document
export.

## Images

Project images are stored in the project `Images` folder.

To use a project image in markdown text, write only the image file name:

```md
![Diagram](diagram.png)
```

Deltos resolves that file name against the project `Images` folder for preview
and export. Existing project-relative paths such as `Images/diagram.png` and
`../Images/diagram.png` are also supported.

## Moving Items

`Up` and `Down` reorder an item inside its current parent.

Use `Change Parent` to move a folder or text file to another document or folder
container.

## Application Theme

Deltos supports Default, Light, and Dark application themes.

Default follows the operating system theme. Light and Dark force that theme
immediately without restarting the application.

The theme can be changed from the main toolbar or from Settings. New
application settings start in Dark theme.

## Built-In Documentation

Built-in documentation is copied to the application data folder so it can be
opened from the main toolbar.

When the application assembly is newer than the copied documentation files,
Deltos refreshes the app-data documentation from the embedded files.

## Legacy Project Conversion

Projects without `ProjectManifest.json` are treated as legacy projects.

When a legacy project is opened, Deltos asks whether to convert it to the
current format. If conversion is accepted, Deltos first asks for an external
backup folder and creates a full copy of the old project there.

The original project is converted only after the backup succeeds.

## Export

Deltos can export a document to:

- TXT.
- Markdown.
- HTML.
- ODT.

Per-item output metadata controls exported item title visibility, page breaks,
HTML table-of-contents entries, and numbering.

HTML and ODT exports copy referenced project images into the export folder's
`Images` subfolder and rewrite generated HTML image paths to those copied files.
The `Image max width` export option limits exported image width while preserving
image aspect ratio.

## Components And Wiki

Components are project-level reference items for characters, places,
organizations, concepts, glossary terms, technical subjects, and other domain
material.

Deltos can generate a static wiki from components.
