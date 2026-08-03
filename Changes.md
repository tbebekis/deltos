# Changes

## 2026.8.3 - 2026-08-03

- Added application theme selection with Default, Light, and Dark options.
- Made Dark the initial application theme for new settings.
- Added theme-aware Tripous desktop shared styles through explicit
  `TripousDesktop.axaml` include.
- Removed local duplicate toolbar, status bar, modal dialog, button, and
  splitter styling now provided by Tripous desktop styles.
- Made text metrics and markdown preview colors theme-aware for Light and Dark.
- Made copied built-in documentation refresh when the application assembly is
  newer than the app-data documentation files.

## 2026.7.27 - 2026-07-27

- Added configurable words-per-page text metric setting.
- Added `Internal Markdown` export format that writes one markdown file per
  text file with hierarchical file names.
- Added markdown table rendering to HTML preview.
- Added markdown pipe table support to HTML and ODT export.
- Made ODT table exports use relative table width so tables adapt to page size
  changes in LibreOffice.
- Changed export defaults so text files are treated as markdown by default.
- Improved export please-wait rendering before long-running export work.
- Suppressed LibreOffice conversion output from the debug console.

## 2026.7.26 - 2026-07-26

- Added document export support for project images referenced from markdown.
- HTML and ODT exports now copy referenced images into the export folder's
  `Images` subfolder and rewrite generated image paths.
- Added `Image max width` export option for capping exported image dimensions.
- Updated export options dialog sizing and layout.
- Clarified metrics panel headers as project and document sections.
- Refreshed sidebar text metrics after saving text items.
- Documented project image markdown references and export behavior.
- Added Text Editor support for showing text files, components, and notes in
  their list page.

## 2026.7.24 - 2026-07-24

- Refactored document storage to a mixed ordered item model.
- Documents and folders now contain a common ordered child list.
- Folder and text-file children are stored under a shared `Items` folder.
- The child item type is read from each item's `Info.json`.
- The UI no longer shows storage order prefixes in item titles.
- Document structure now works as a folder-level template instead of a strict
  content model.
- Text files can be placed directly under documents and under any folder.
- Folders can contain both folders and text files.
- `Up` and `Down` now reorder items only inside their current parent.
- Added `Change Parent` for moving folders and text files between valid
  document containers.
- Added `ProjectManifest.json` with storage version tracking.
- Added legacy project conversion from the old storage format to the current
  format.
- Added mandatory external backup before converting a legacy project.
- Kept legacy loading support for old projects.
- Added shared Add/Edit item metadata fields for documents, folders, and text
  files.
- Added item output metadata:
  - Include title in output.
  - Page break before.
  - Include in TOC.
  - Numbering.
  - Custom numbering.
- Applied per-item output metadata during TXT, Markdown, HTML, and ODT export.
- Added `ShowInTaskbar="False"` to dialogs.
- Updated document tree traversal, parent selection, stats, and counts to use
  the mixed item model.
- Added built-in documentation opened from the main toolbar.
- Added tests for mixed item storage, migration behavior, movement boundaries,
  change-parent behavior, output metadata persistence, and export metadata.
- Added release assets and application icon support.
- Updated startup and about visuals.
- Added application version display to the About dialog.
- Added an optional HTML preview button to text editor toolbars.
- Standardized HTML preview tab identifiers.
- Improved legacy project conversion feedback with visible please-wait messages,
  desktop notes, and log entries.
