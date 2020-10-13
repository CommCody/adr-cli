using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace adr.Utils
{
    /// <summary>
    /// File management utilities
    /// </summary>
    internal static class FileUtils
    {
        /// <summary>
        /// Sanitize a file name from invalid characters and reserved words
        /// </summary>
        /// <param name="filename">the file name</param>
        /// <param name="substitution">the substitution character for invalid characters</param>
        /// <returns>a clean file name</returns>
        internal static string SanitizeFilename(string filename, char substitution = '_')
        {
            filename = filename.Replace(' ', substitution);
            string invalidCharacters = Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string regex = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidCharacters);

            var cleanFilename = Regex.Replace(filename, regex, substitution.ToString());

            var reservedWords = new[]
            {
                "AUX",
                "CON",
                "CLOCK$",
                "COM0",
                "COM1",
                "COM2",
                "COM3",
                "COM4",
                "COM5",
                "COM6",
                "COM7",
                "COM8",
                "COM9",
                "LPT0",
                "LPT1",
                "LPT2",
                "LPT3",
                "LPT4",
                "LPT5",
                "LPT6",
                "LPT7",
                "LPT8",
                "LPT9",
                "NUL",
                "PRN"
            };

            foreach (var reservedWord in reservedWords)
            {
                var reservedWordPattern = string.Format("^{0}\\.", reservedWord);

                cleanFilename = Regex.Replace(
                    cleanFilename,
                    reservedWordPattern,
                    "_reservedWord_.",
                    RegexOptions.IgnoreCase);
            }

            return cleanFilename;
        }

        /// <summary>
        /// Add a new line at a specific position in a text file
        /// </summary>
        /// <param name="fileName">full file path</param>
        /// <param name="lineToSearch">Line to search in the file (first occurrence). If not found, the text will be inserted at the beginning of the file.</param>
        /// <param name="linesToAdd">Lines to be added</param>
        /// <param name="insertionMode">A flag to specify whether to insert above, below or in place of the search line. Default: Append </param>
        internal static void InsertTextToFile(string fileName, string lineToSearch, string[] linesToAdd, TextInsertionMode insertionMode = TextInsertionMode.Append)
        {
            var txtLines = File.ReadAllLines(fileName)
                .ToList();

            int index = 0;

            if (txtLines.Count > 0)
            {
                switch (insertionMode)
                {
                    case TextInsertionMode.Append:
                        index = txtLines.IndexOf(lineToSearch) + 1;
                        break;

                    case TextInsertionMode.Prepend:
                        index = txtLines.IndexOf(lineToSearch);
                        break;

                    case TextInsertionMode.Replace:
                        index = txtLines.IndexOf(lineToSearch) + 1;

                        txtLines.Remove(lineToSearch);
                        break;
                }
            }

            if (index >= 0)
            {
                foreach (var lineToAdd in linesToAdd)
                {
                    txtLines.Insert(index, lineToAdd);
                    index++;
                }

                File.WriteAllLines(fileName, txtLines);
            }
        }

        /// <summary>
        /// Text insertion mode in file
        /// </summary>
        internal enum TextInsertionMode
        {
            /// <summary>
            /// Append text
            /// </summary>
            Append,

            /// <summary>
            /// Prepend text
            /// </summary>
            Prepend,

            /// <summary>
            /// Replace text
            /// </summary>
            Replace
        }
    }
}
