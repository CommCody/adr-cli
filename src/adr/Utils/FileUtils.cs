using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace adr.Utils
{
    /// <summary>
    /// File management utilities
    /// </summary>
    public static class FileUtils
    {
        /// <summary>
        /// Sanitize a file name from invalid characters and reserved words
        /// </summary>
        /// <param name="filename">the file name</param>
        /// <param name="substitution">the substitution character for invalid characters</param>
        /// <returns>a clean file name</returns>
        public static string SanitizeFilename(string filename, string substitution = "_")
        {
            string invalidCharacters = Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string regex = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidCharacters);

            var cleanFilename = Regex.Replace(filename, regex, substitution);

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
    }
}
