using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KmbTextConversion
{
    /// <summary>
    /// Represents the output files and counts produced by a KMB dictionary build.
    /// </summary>
    public sealed class KmbDictionaryBuildResult
    {
        /// <summary>
        /// Gets or sets the generated phrase dictionary file path.
        /// </summary>
        public string PhraseFilePath { get; set; }

        /// <summary>
        /// Gets or sets the generated character dictionary file path.
        /// </summary>
        public string CharacterFilePath { get; set; }

        /// <summary>
        /// Gets or sets the number of phrase pairs written to disk.
        /// </summary>
        public int PhraseCount { get; set; }

        /// <summary>
        /// Gets or sets the number of character pairs written to disk.
        /// </summary>
        public int CharacterCount { get; set; }
    }

    /// <summary>
    /// Builds KMB-specific dictionary files from exported route CSV files.
    /// </summary>
    public static class KmbDictionaryBuilder
    {
        /// <summary>
        /// Generates phrase and character dictionary files from a folder of route CSV exports.
        /// </summary>
        /// <param name="inputFolder">The folder containing route CSV files.</param>
        /// <param name="outputFolder">The folder where the dictionary TSV files should be written.</param>
        /// <returns>The generated dictionary file information.</returns>
        public static KmbDictionaryBuildResult BuildFromRouteCsvFolder(string inputFolder, string outputFolder)
        {
            if (string.IsNullOrWhiteSpace(inputFolder))
            {
                throw new ArgumentException("Input folder is required.", nameof(inputFolder));
            }

            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                throw new ArgumentException("Output folder is required.", nameof(outputFolder));
            }

            string fullInputFolder = Path.GetFullPath(inputFolder);
            string fullOutputFolder = Path.GetFullPath(outputFolder);

            if (!Directory.Exists(fullInputFolder))
            {
                throw new DirectoryNotFoundException("Input folder not found: " + fullInputFolder);
            }

            Directory.CreateDirectory(fullOutputFolder);

            var phrasePairs = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string filePath in Directory.GetFiles(fullInputFolder, "*.csv"))
            {
                foreach (Dictionary<string, string> row in ReadCsv(filePath))
                {
                    AddPair(phrasePairs, ReadValue(row, "stop_name"), ReadValue(row, "stop_name_sc"));
                    AddPair(phrasePairs, ReadValue(row, "direction"), ReadValue(row, "direction_sc"));
                }
            }

            List<string> phraseLines = phrasePairs
                .OrderByDescending(pair => pair.Key.Length)
                .ThenBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => pair.Key + "\t" + pair.Value)
                .ToList();

            Dictionary<string, string> charPairs = BuildCharPairs(phrasePairs);
            List<string> charLines = charPairs
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => pair.Key + "\t" + pair.Value)
                .ToList();

            string phrasePath = Path.Combine(fullOutputFolder, "kmb_tc_to_sc.tsv");
            string charPath = Path.Combine(fullOutputFolder, "kmb_char_tc_to_sc.tsv");

            File.WriteAllLines(phrasePath, phraseLines, new UTF8Encoding(true));
            File.WriteAllLines(charPath, charLines, new UTF8Encoding(true));

            return new KmbDictionaryBuildResult
            {
                PhraseFilePath = phrasePath,
                CharacterFilePath = charPath,
                PhraseCount = phraseLines.Count,
                CharacterCount = charLines.Count
            };
        }

        /// <summary>
        /// Parses a single CSV line while respecting quoted fields and escaped quotes.
        /// </summary>
        /// <param name="line">The CSV line to parse.</param>
        /// <returns>The parsed field list.</returns>
        public static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var builder = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char current = line[i];
                if (current == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        builder.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }

                    continue;
                }

                if (current == ',' && !inQuotes)
                {
                    fields.Add(builder.ToString());
                    builder.Clear();
                    continue;
                }

                builder.Append(current);
            }

            fields.Add(builder.ToString());
            return fields;
        }

        /// <summary>
        /// Adds a Traditional/Simplified pair to the phrase dictionary when both values are present.
        /// </summary>
        /// <param name="pairs">The dictionary being populated.</param>
        /// <param name="traditional">The Traditional Chinese text.</param>
        /// <param name="simplified">The Simplified Chinese text.</param>
        private static void AddPair(IDictionary<string, string> pairs, string traditional, string simplified)
        {
            if (string.IsNullOrWhiteSpace(traditional) || string.IsNullOrWhiteSpace(simplified))
            {
                return;
            }

            if (!pairs.ContainsKey(traditional))
            {
                pairs[traditional] = simplified;
            }
        }

        /// <summary>
        /// Reads a column value from a CSV row dictionary.
        /// </summary>
        /// <param name="row">The row dictionary.</param>
        /// <param name="columnName">The column name to retrieve.</param>
        /// <returns>The column value or an empty string when the column is missing.</returns>
        private static string ReadValue(IDictionary<string, string> row, string columnName)
        {
            string value;
            return row.TryGetValue(columnName, out value) ? value ?? string.Empty : string.Empty;
        }

        /// <summary>
        /// Derives the most likely character-level mapping from the collected phrase pairs.
        /// </summary>
        /// <param name="phrasePairs">The phrase dictionary pairs.</param>
        /// <returns>The inferred character-level dictionary.</returns>
        private static Dictionary<string, string> BuildCharPairs(IDictionary<string, string> phrasePairs)
        {
            var counts = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, string> pair in phrasePairs)
            {
                if (pair.Key.Length != pair.Value.Length)
                {
                    continue;
                }

                for (int i = 0; i < pair.Key.Length; i++)
                {
                    string traditional = pair.Key.Substring(i, 1);
                    string simplified = pair.Value.Substring(i, 1);

                    if (traditional == simplified)
                    {
                        continue;
                    }

                    Dictionary<string, int> targets;
                    if (!counts.TryGetValue(traditional, out targets))
                    {
                        targets = new Dictionary<string, int>(StringComparer.Ordinal);
                        counts[traditional] = targets;
                    }

                    int frequency;
                    targets.TryGetValue(simplified, out frequency);
                    targets[simplified] = frequency + 1;
                }
            }

            return counts.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.OrderByDescending(candidate => candidate.Value).ThenBy(candidate => candidate.Key, StringComparer.Ordinal).First().Key,
                StringComparer.Ordinal);
        }

        /// <summary>
        /// Reads CSV rows from a file into dictionaries keyed by column name.
        /// </summary>
        /// <param name="filePath">The CSV file path.</param>
        /// <returns>The sequence of row dictionaries.</returns>
        private static IEnumerable<Dictionary<string, string>> ReadCsv(string filePath)
        {
            using (var reader = new StreamReader(filePath, Encoding.UTF8, true))
            {
                List<string> headers = null;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    List<string> fields = ParseCsvLine(line);
                    if (headers == null)
                    {
                        headers = fields;
                        continue;
                    }

                    var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < headers.Count; i++)
                    {
                        row[headers[i]] = i < fields.Count ? fields[i] : string.Empty;
                    }

                    yield return row;
                }
            }
        }
    }
}
