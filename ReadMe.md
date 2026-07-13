# Deltos

Deltos is a desktop writing environment for structured long-form projects.

It is designed for authors who need more than a plain text editor: novelists,
worldbuilders, essayists, technical writers, documentation authors, and anyone
working with large bodies of connected text.

Deltos can be used to write a novella, a novel, a multi-book series, a
technical manual, an essay collection, a knowledge base, or a documentation
project. Its core idea is simple: writing belongs in a calm editor, structure
belongs in a visible tree, and the project itself should remain readable as
ordinary files and folders.

## Features

- Project-based writing workspace.
- File-system-first storage with ordinary folders, markdown files, and JSON
  metadata.
- Multiple documents inside a single project.
- Configurable document structures, such as Part, Chapter, Section, Scene.
- Flat documents for simpler projects.
- Folder and text-file organization with clear ordering.
- Markdown editing with primary and secondary language text.
- Synopsis and draft text areas for documents, folders, and text files.
- Temp text scratchpad.
- Notes list for project-level notes.
- Components for worldbuilding, reference material, characters, places,
  organizations, concepts, and technical entities.
- Component categories, tags, and aliases.
- Tag browser and component browser.
- HTML preview for markdown content.
- Project-wide search across documents, text files, components, notes, and temp
  text.
- Whole-word project search using quoted terms, such as `"empire"`.
- Find and replace inside every text editor, with highlighted matches and
  next/previous navigation.
- Quick View list for collecting important items and search hits while working.
- Text metrics for every editor.
- Auto-save support.
- Export per document to TXT, HTML, and ODT.
- Plain-text export mode for prose that should not be treated as markdown.
- Git commit and push integration for project folders.
- Static wiki generation from components.
- Light, direct desktop UI built around sidebars, trees, tabs, and editors.

## What Is Deltos?

Deltos is a writing workspace, not a single-document editor.

A Deltos project can hold the manuscript, its supporting notes, the world or
domain reference material, temporary text, images, exports, and a generated
static wiki. The user can move between the structure of the work and the actual
text without leaving the application.

For fiction, Deltos can manage books, chapters, scenes, characters, places,
timelines, and worldbuilding notes.

For non-fiction, it can manage manuals, sections, essays, articles, reference
entries, terms, categories, tags, and documentation pages.

The same model works because Deltos does not force a specific literary shape.
The document structure is chosen by the user.

## Core Concepts

### Project

A project is the root workspace.

It contains documents, components, notes, images, temp text, project settings,
and export/wiki configuration. A project is stored directly in a folder chosen
by the user.

### Document

A document is a major written work inside the project.

Examples:

- A novel.
- A novella.
- A short-story collection.
- A technical manual.
- A documentation volume.
- A long essay.

Each document may be flat or structured.

A flat document contains text files directly.

A structured document uses a user-defined hierarchy. For example:

```text
Document
    Part
        Chapter
            Scene
```

or:

```text
Document
    Chapter
        Section
```

The structure determines what kind of child item can be created at each level.

### Folder

A folder is a structural unit inside a document.

Its display title may be `Part`, `Chapter`, `Section`, or any other level name
defined by the document structure. Folders may also have synopsis text.

### TextFile

A text file is the actual writing unit.

It stores:

- Primary text.
- Secondary language text.
- Synopsis.
- Draft.

Text files are markdown files on disk, but Deltos can also treat prose as plain
text during export when markdown interpretation is not desired.

### Component

A component is a project-level reference item.

Components are useful for:

- Characters.
- Places.
- Countries.
- Organizations.
- Machines.
- Religions.
- Events.
- Concepts.
- Glossary terms.
- Technical subjects.

Each component has a title, category, tags, aliases, primary text, and secondary
language text. Categories and tags are derived from the components themselves,
so there is no separate taxonomy database to maintain.

Components can be exported as a static wiki.

### Notes

Notes are project-level text items for supporting material that does not belong
inside the document tree or the component library.

### Temp Text

Temp Text is a scratchpad for transient writing, fragments, reminders, and
material that has not yet found its place.

### Search

Search is a project navigation tool, not only a text lookup command.

Global Search scans the whole project and returns structured results grouped by
the item where each match was found. A result can point to a title, synopsis,
draft, primary text, secondary language text, note, component, or temp text.

Typing a plain term performs a contains search. Typing the term inside double
quotes performs a whole-word search:

```text
empire
"empire"
```

Opening a search result takes the user back to the real editor and highlights
the matching term in context. This keeps search connected to writing instead of
turning it into a separate report.

Each editor also has local find and replace. Local search highlights every
match, can move to the next or previous match, and can replace the current
match or all matches.

### Quick View

Quick View is a temporary working list.

It is useful when several items matter at the same time: a group of scenes, a
set of components, a few notes, or specific search results. Items can be added
from lists, tags, components, notes, and global search.

Quick View is saved with the project, so it can be used as a small research
desk or task tray for the current writing session.

## Typical Workflow

1. Create or open a project.
2. Create a document.
3. Choose the document structure.
4. Add folders and text files.
5. Write in the editor.
6. Add synopsis and draft material where useful.
7. Create components for reference material.
8. Categorize and tag components.
9. Preview markdown as HTML.
10. Use Global Search to move through the project.
11. Collect active items in Quick View.
12. Review text metrics.
13. Export a document to TXT, HTML, or ODT.
14. Generate a static wiki from components.
15. Commit and push the project with Git.

## Project Storage

Deltos stores a project as ordinary folders and files.

There is no opaque database file. The project folder contains readable
markdown, JSON metadata, images, resources, and generated output.

The item tree is derived from the file system. Ordering is encoded in folder
and file names using numeric prefixes.

Example:

```text
001._The_Corp_of_the_World
002._The_Corp_of_the_Fallen_Worlds
003._Blog_Stories
```

When Deltos reads an item from disk:

- The numeric prefix becomes the order index.
- The remaining name becomes the title.
- Underscores are converted back to spaces for display.

Each item has an `Info.json` file for metadata. Text content is stored in
markdown files such as `Text.md`, `Text2.md`, `Synopsis.md`, and `Draft.md`.

## Document Structure

Document structure is one of the central ideas of Deltos.

The user decides the shape of a document when creating or configuring it. A
novel might use parts, chapters, and scenes. A manual might use chapters,
sections, and topics. An essay collection might use only one level.

Deltos uses that structure to keep the tree consistent. A folder level may
contain the next folder level, or text files when it is the final writing level.

This keeps large works navigable and avoids accidental structural drift.

## Editing

The main workspace is tab-based.

Opening a text file, component, note, preview, or metrics page creates a tab in
the main content area. Tabs can be reordered. Text editors include status
information and text metrics.

The editor supports project-wide font settings and optional secondary language
visibility.

Text editing includes local find and replace. Matches are highlighted directly
inside the editor, and the user can move between them without leaving the text.

Global Search complements local editor search. It finds text across the whole
project, then opens the matching item in the main content area and highlights
the term in the correct editor.

## Export

Deltos exports documents, not the whole project.

Supported formats:

- TXT.
- HTML.
- ODT.

The export process can include document headings, folder headings, text-file
titles, primary language text, secondary language text, and plain-text handling
for prose that should not be parsed as markdown.

## Static Wiki

Deltos can generate a static wiki from project components.

The wiki includes:

- Component pages.
- Category navigation.
- Tag navigation.
- Search index.
- Theme support.
- Copied images and assets.
- Optional About page.

This is useful for worldbuilding, public reference material, documentation, or
project knowledge bases.

## Git Integration

Deltos can run Git commit and push commands for the project folder.

Credentials are not handled by Deltos. The user configures Git normally on the
machine and in the repository. Deltos only provides convenient toolbar actions
and logs the command result.

## Screenshots

Screenshots will be added here.

Suggested sections:

- Main workspace.
- Document list.
- Text editor.
- Component list.
- Tags.
- Export dialog.
- Static wiki.

## Author

Deltos is created by Theodoros Bebekis.

Theodoros Bebekis is the author of the sci-fi series **The Corp of the World**,
published on Amazon.

https://www.amazon.com/dp/B0DJH77BDJ

## License

Deltos is licensed under the MIT License.

## Icons

Deltos uses icons from the Silk icon set 1.3.

Author: Mark James

Project: https://github.com/legacy-icons/famfamfam-silk

License: Creative Commons Attribution 2.5

License URL: http://creativecommons.org/licenses/by/2.5/
