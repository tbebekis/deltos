// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.


namespace Deltos;

static public partial class AppHost
{
    static Regex EnglishLettersRegex = new Regex("^[a-zA-Z0-9. -_?]*$");
    const string SInvalidTitle = @"Invalid title {0}.";
    const string SValidTitle = @"
A valid title
  • can contain only letters, numbers and spaces
  • cannot contain special characters
  • cannot start with a number
  • must be in English
";
    const string SInvalidTitleErrorMessage = SInvalidTitle + SValidTitle;
    
    // ● filenames and paths
    /// <summary>
    /// Returns true if the provided item title is valid (not empty, no invalid characters, does not start with a digit).
    /// </summary>
    static public bool IsValidFileName(string Title, bool ShowMessage)
    {

        bool IsValidFileNameNested()
        {
            if (string.IsNullOrWhiteSpace(Title))
                return false;

            char[] InvalidChars = System.IO.Path.GetInvalidFileNameChars();

            foreach (char c in InvalidChars)
            {
                if (Title.Contains(c))
                    return false;
            }

            if (char.IsDigit(Title[0]))
                return false;

            if (!EnglishLettersRegex.IsMatch(Title))
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
