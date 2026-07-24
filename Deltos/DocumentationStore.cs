// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos;

/// <summary>
/// Seeds and reads Deltos documentation files from the application data folder.
/// </summary>
public class DocumentationStore
{
    // ● private
    const string MainDocumentFileName = "deltos-concepts.md";
    string GetResourcePrefix() => typeof(DocumentationStore).Namespace + ".Resources.Documentation.";
    string GetTargetFilePath(string ResourceName)
    {
        string Prefix = GetResourcePrefix();
        string FileName = ResourceName.StartsWith(Prefix) ? ResourceName.Substring(Prefix.Length) : ResourceName;
        return Path.Combine(FolderPath, FileName);
    }

    // ● construction
    /// <summary>
    /// Initializes a new instance of the DocumentationStore class.
    /// </summary>
    public DocumentationStore()
    {
    }

    // ● public
    /// <summary>
    /// Writes embedded documentation files to the application data folder when they are missing.
    /// </summary>
    public void EnsureCreated()
    {
        System.Reflection.Assembly Assembly = typeof(DocumentationStore).Assembly;
        string Prefix = GetResourcePrefix();
        foreach (string ResourceName in Assembly.GetManifestResourceNames().Where(Item => Item.StartsWith(Prefix)).OrderBy(Item => Item))
        {
            string FilePath = GetTargetFilePath(ResourceName);
            if (File.Exists(FilePath))
                continue;

            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            using Stream Stream = Assembly.GetManifestResourceStream(ResourceName);
            using FileStream FileStream = File.Create(FilePath);
            Stream.CopyTo(FileStream);
        }
    }
    /// <summary>
    /// Reads the main documentation markdown text.
    /// </summary>
    /// <returns>The main documentation markdown text.</returns>
    public string ReadMainDocument()
    {
        EnsureCreated();
        return File.Exists(MainDocumentFilePath) ? File.ReadAllText(MainDocumentFilePath) : string.Empty;
    }
    /// <summary>
    /// Opens the documentation folder in the file explorer.
    /// </summary>
    public void OpenFolder()
    {
        EnsureCreated();
        Sys.OpenFileExplorer(FolderPath);
    }

    // ● properties
    /// <summary>
    /// Gets the documentation folder path.
    /// </summary>
    public string FolderPath => Path.Combine(SysConfig.AppDataFolderPath, "Documentation");
    /// <summary>
    /// Gets the main documentation file path.
    /// </summary>
    public string MainDocumentFilePath => Path.Combine(FolderPath, MainDocumentFileName);
}
