using adr.Utils;
using System;
using System.IO;
using System.Linq;

namespace adr.Adr
{
    internal class ArchitectureDecisionLog
    {
        private string _docFolder;

        public ArchitectureDecisionLog(string docFolder)
        {
            this._docFolder = docFolder ?? throw new ArgumentNullException(nameof(docFolder));

            if (!Directory.Exists(this._docFolder))
            {
                throw new DirectoryNotFoundException(this._docFolder);
            }
        }

        /// <summary>
        /// Search a record by number
        /// </summary>
        /// <param name="adrNumber">the number of the record</param>
        /// <returns>an <see cref="AdrEntry"/> or <code>null</code> if not found</returns>
        internal AdrEntry SearchAdr(int adrNumber)
        {
            var needle = adrNumber.ToString().PadLeft(4, '0');

            var allFiles = this.GetRecords();

            var file = allFiles.FirstOrDefault(f => f.Name.StartsWith(needle));

            AdrEntry entry = null;

            if (file != null)
            {
                entry = this.LoadAdr(file);
            }

            return entry;
        }

        /// <summary>
        /// Get all markdown files
        /// </summary>
        /// <returns>an array of all markdown files' <see cref="FileInfo"/></returns>
        internal FileInfo[] GetRecords()
        {
            var files = new FileInfo[0];

            if (!Directory.Exists(this._docFolder))
            {
                throw new DirectoryNotFoundException(this._docFolder);
            }

            DirectoryInfo di = new DirectoryInfo(this._docFolder);

            files = di.GetFiles("*.md", SearchOption.TopDirectoryOnly);

            return files;
        }

        /// <summary>
        /// Supercedes an existing record with a new one
        /// </summary>
        /// <param name="supercededRecordNumber">the number of the superceded record</param>
        /// <param name="newRecord">the new record</param>
        internal void SupercedesAdr(int supercededRecordNumber, AdrEntry newRecord)
        {
            if (newRecord is null)
            {
                throw new ArgumentNullException(nameof(newRecord));
            }

            AdrEntry supercededRecord = this.SearchAdr(supercededRecordNumber);

            this.SupercedesAdr(supercededRecord, newRecord);
        }

        /// <summary>
        /// Links an existing record with a new one
        /// </summary>
        /// <param name="linkedRecordNumber">the number of the linked record</param>
        /// <param name="newRecord">the new record</param>
        /// <param name="link">the link details</param>
        internal void LinksAdr(AdrEntry newRecord, AdrLink link)
        {
            if (newRecord is null)
            {
                throw new ArgumentNullException(nameof(newRecord));
            }

            if (link is null)
            {
                throw new ArgumentNullException(nameof(link));
            }

            AdrEntry linkedRecord = this.SearchAdr(link.Number);

            this.LinksAdr(linkedRecord, newRecord, link);
        }

        /// <summary>
        /// Load entry from file
        /// </summary>
        /// <param name="file">the source <see cref="FileInfo"/></param>
        /// <returns>an <see cref="AdrEntry"/> or <code>null</code> if not found</returns>
        private AdrEntry LoadAdr(FileInfo file)
        {
            AdrEntry entry = null;

            if (File.Exists(file.FullName))
            {
                entry = new AdrEntry(TemplateType.New);

                entry.Title = File.ReadLines(file.FullName)
                    .FirstOrDefault()?
                    .Replace("# ", string.Empty) ?? string.Empty;
                entry.File = file;
            }

            return entry;
        }

        /// <summary>
        /// Supercedes an existing record with a new one
        /// </summary>
        /// <param name="supercededRecord">the superceded record</param>
        /// <param name="newRecord">the new record</param>
        private void SupercedesAdr(AdrEntry supercededRecord, AdrEntry newRecord)
        {
            if (supercededRecord is null)
            {
                throw new ArgumentNullException(nameof(supercededRecord));
            }

            if (newRecord is null)
            {
                throw new ArgumentNullException(nameof(newRecord));
            }

            this.ClearAdrStatus(supercededRecord);

            FileUtils.InsertTextToFile(
                newRecord.File.FullName,
                "## Context",
                new[] {
                    $"Supercedes: [{supercededRecord.Title}]({supercededRecord.File.Name})",
                    string.Empty
                },
                FileUtils.TextInsertionMode.Prepend
                );

            FileUtils.InsertTextToFile(
                supercededRecord.File.FullName,
                "## Context",
                new[] {
                    string.Empty,
                    $"Superceded by: [{newRecord.Title}]({newRecord.File.Name})",
                    string.Empty
                },
                FileUtils.TextInsertionMode.Prepend
                );
        }

        /// <summary>
        /// Remove the status paragraph from file
        /// </summary>
        /// <param name="adr">the <see cref="AdrEntry"/></param>
        private void ClearAdrStatus(AdrEntry adr)
        {
            if (adr is null)
            {
                throw new ArgumentNullException(nameof(adr));
            }

            var fileName = adr.File.FullName;
            var txtLines = File.ReadAllLines(fileName)
                            .ToList();

            int statusLineIndex = txtLines.IndexOf("## Status");
            int contextLineIndex = txtLines.IndexOf("## Context");

            // something is wrong, cannot clear status paragraph
            if (statusLineIndex == -1 || contextLineIndex == -1 || contextLineIndex <= statusLineIndex)
            {
                return;
            }

            int elementsToRemove = contextLineIndex - statusLineIndex - 1;
            txtLines.RemoveRange(statusLineIndex + 1, elementsToRemove);

            File.WriteAllLines(fileName, txtLines);
        }

        /// <summary>
        /// Links an existing record with a new one
        /// </summary>
        /// <param name="linkedRecord">the linked record</param>
        /// <param name="newRecord">the new record</param>
        /// <param name="link">the link details</param>
        private void LinksAdr(AdrEntry linkedRecord, AdrEntry newRecord, AdrLink link)
        {
            if (linkedRecord is null)
            {
                throw new ArgumentNullException(nameof(linkedRecord));
            }

            if (newRecord is null)
            {
                throw new ArgumentNullException(nameof(newRecord));
            }

            if (link is null)
            {
                throw new ArgumentNullException(nameof(link));
            }

            FileUtils.InsertTextToFile(
                newRecord.File.FullName,
                "## Context",
                new[] {
                    $"{link.LinkDescription}: [{linkedRecord.Title}]({linkedRecord.File.Name})",
                    string.Empty
                },
                FileUtils.TextInsertionMode.Prepend
                );

            FileUtils.InsertTextToFile(
                linkedRecord.File.FullName,
                "## Context",
                new[] {
                    string.Empty,
                    $"{link.ReverseLinkDescription}: [{newRecord.Title}]({newRecord.File.Name})",
                    string.Empty
                },
                FileUtils.TextInsertionMode.Prepend
                );
        }
    }
}
