// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.


namespace Deltos;

static public partial class AppHost
{
    /// <summary>
    /// Matches valid title characters.
    /// </summary>
    static Regex EnglishLettersRegex = new Regex("^[a-zA-Z0-9 ]*$");
    /// <summary>
    /// The invalid title message format.
    /// </summary>
    const string SInvalidTitle = @"Invalid title {0}.";
    /// <summary>
    /// The valid title rule message.
    /// </summary>
    const string SValidTitle = @"
A valid title
  • can contain only letters, numbers and spaces
  • cannot contain special characters
  • cannot start with a number
  • must be in English
";
    /// <summary>
    /// The invalid title error message format.
    /// </summary>
    const string SInvalidTitleErrorMessage = SInvalidTitle + SValidTitle;
    
    // ● filenames and paths
    /// <summary>
    /// Returns true if the provided item title is valid (not empty, no invalid characters, does not start with a digit).
    /// </summary>
    static public bool IsValidFileName(string Title, bool ShowMessage)
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

            if (!EnglishLettersRegex.IsMatch(TrimmedTitle))
                return false;

            return true;
        }

        bool Result = IsValidFileNameNested();
        if (!Result && ShowMessage)
        {
            string Message = string.Format(SInvalidTitleErrorMessage, Title);
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
}
