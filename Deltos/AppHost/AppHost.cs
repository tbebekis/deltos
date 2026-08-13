// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.


namespace Deltos;

static public partial class AppHost
{
    /// <summary>
    /// Matches valid folder level title characters.
    /// </summary>
    static Regex LevelTitleCharsRegex = new Regex(@"^[\p{L}\p{N} ]*$");
    /// <summary>
    /// The invalid title message format.
    /// </summary>
    const string SInvalidTitle = @"Invalid title {0}.";
    /// <summary>
    /// The valid title rule message.
    /// </summary>
    const string SValidTitle = @"
A valid title
  • cannot be empty
  • must produce a valid storage name
  • cannot start with a number
";
    /// <summary>
    /// The invalid title error message format.
    /// </summary>
    const string SInvalidTitleErrorMessage = SInvalidTitle + SValidTitle;
    /// <summary>
    /// The invalid folder level title message format.
    /// </summary>
    const string SInvalidFolderLevelTitle = @"Invalid folder level title {0}.";
    /// <summary>
    /// The valid folder level title rule message.
    /// </summary>
    const string SValidFolderLevelTitle = @"
A valid folder level title
  • can contain only letters, numbers and spaces
  • cannot contain special characters
  • cannot start with a number
";
    /// <summary>
    /// The invalid folder level title error message format.
    /// </summary>
    const string SInvalidFolderLevelTitleErrorMessage = SInvalidFolderLevelTitle + SValidFolderLevelTitle;
    
    // ● filenames and paths
    /// <summary>
    /// Returns true if the provided item title is valid.
    /// </summary>
    static public bool IsValidFileName(string Title, bool ShowMessage)
    {
        string TrimmedTitle = Title == null ? string.Empty : Title.Trim();

        bool Result = IsValidTitleName(TrimmedTitle);
        if (!Result && ShowMessage)
        {
            string Message = string.Format(SInvalidTitleErrorMessage, Title);
            // TODO: ErrorBox(Message);
            LogBox.AppendLine(Message);
        }

        return Result;
    }
    /// <summary>
    /// Returns true if the provided folder level title is valid.
    /// </summary>
    static public bool IsValidFolderLevelTitle(string Title, bool ShowMessage)
    {
        return IsValidName(Title, ShowMessage, LevelTitleCharsRegex, SInvalidFolderLevelTitleErrorMessage);
    }
    /// <summary>
    /// Returns true if the provided title is valid.
    /// </summary>
    /// <param name="TrimmedTitle">The trimmed title.</param>
    /// <returns>True if the title is valid; otherwise false.</returns>
    static bool IsValidTitleName(string TrimmedTitle)
    {
        if (string.IsNullOrWhiteSpace(TrimmedTitle))
            return false;

        if (char.IsDigit(TrimmedTitle[0]))
            return false;

        string StorageName = TrimmedTitle;
        foreach (char Char in System.IO.Path.GetInvalidFileNameChars())
            StorageName = StorageName.Replace(Char, '_');

        foreach (char Char in "<>:\"/\\|?*")
            StorageName = StorageName.Replace(Char, '_');

        StorageName = StorageName.Replace(' ', '_');
        StorageName = Regex.Replace(StorageName, "_+", "_").Trim('_');
        if (string.IsNullOrWhiteSpace(StorageName))
            return false;

        return !char.IsDigit(StorageName[0]);
    }
    /// <summary>
    /// Returns true if the provided name is valid.
    /// </summary>
    /// <param name="Title">The name to validate.</param>
    /// <param name="ShowMessage">True to log an error message.</param>
    /// <param name="CharsRegex">The accepted character regex.</param>
    /// <param name="ErrorMessageFormat">The error message format.</param>
    /// <returns>True if the name is valid; otherwise false.</returns>
    static bool IsValidName(string Title, bool ShowMessage, Regex CharsRegex, string ErrorMessageFormat)
    {
        string TrimmedTitle = Title == null ? string.Empty : Title.Trim();

        bool IsValidFileNameNested()
        {
            if (string.IsNullOrWhiteSpace(TrimmedTitle))
                return false;

            char[] InvalidChars = System.IO.Path.GetInvalidFileNameChars();

            foreach (char c in InvalidChars)
            {
                if (TrimmedTitle.Contains(c))
                    return false;
            }

            if (char.IsDigit(TrimmedTitle[0]))
                return false;

            if (!CharsRegex.IsMatch(TrimmedTitle))
                return false;

            return true;
        }

        bool Result = IsValidFileNameNested();
        if (!Result && ShowMessage)
        {
            string Message = string.Format(ErrorMessageFormat, Title);
            // TODO: ErrorBox(Message);
            LogBox.AppendLine(Message);
        }

        return Result;
    }
    /// <summary>
    /// Throws an exception if the provided item title is not valid
    /// </summary>
    static public void CheckValidFileName(string Title)
    {
        string Message = string.Format(SInvalidTitleErrorMessage, Title);

        if (!IsValidFileName(Title, false))
            throw new Exception(Message);
    }
    /// <summary>
    /// Throws an exception if the provided folder level title is not valid.
    /// </summary>
    static public void CheckValidFolderLevelTitle(string Title)
    {
        string Message = string.Format(SInvalidFolderLevelTitleErrorMessage, Title);

        if (!IsValidFolderLevelTitle(Title, false))
            throw new Exception(Message);
    }
}
