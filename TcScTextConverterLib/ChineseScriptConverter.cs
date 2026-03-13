using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TcScTextConverterLib
{
    /// <summary>
    /// Defines conversion operations between Traditional Chinese and Simplified Chinese text.
    /// </summary>
    public interface IChineseScriptConverter
    {
        /// <summary>
        /// Converts Traditional Chinese text to Simplified Chinese text.
        /// </summary>
        /// <param name="traditionalText">The source text in Traditional Chinese.</param>
        /// <returns>The converted Simplified Chinese text.</returns>
        string ToSimplified(string traditionalText);

        /// <summary>
        /// Converts Simplified Chinese text to Traditional Chinese text.
        /// </summary>
        /// <param name="simplifiedText">The source text in Simplified Chinese.</param>
        /// <returns>The converted Traditional Chinese text.</returns>
        string ToTraditional(string simplifiedText);
    }

    /// <summary>
    /// Provides an offline Chinese script converter backed by embedded OpenCC-style dictionaries.
    /// </summary>
    public sealed class ChineseScriptConverter : IChineseScriptConverter
    {
        private const string OpenCcTsPhraseResourceName = "TcScTextConverterLib.Data.OpenCC.TSPhrases.txt";
        private const string OpenCcTsCharResourceName = "TcScTextConverterLib.Data.OpenCC.TSCharacters.txt";
        private const string OpenCcStPhraseResourceName = "TcScTextConverterLib.Data.OpenCC.STPhrases.txt";
        private const string OpenCcStCharResourceName = "TcScTextConverterLib.Data.OpenCC.STCharacters.txt";
        private const string OpenCcTwPhrasesRevResourceName = "TcScTextConverterLib.Data.OpenCC.TWPhrasesRev.txt";
        private const string OpenCcTwVariantsRevPhrasesResourceName = "TcScTextConverterLib.Data.OpenCC.TWVariantsRevPhrases.txt";
        private const string OpenCcTwVariantsResourceName = "TcScTextConverterLib.Data.OpenCC.TWVariants.txt";
        private const string OpenCcHkVariantsRevPhrasesResourceName = "TcScTextConverterLib.Data.OpenCC.HKVariantsRevPhrases.txt";
        private const string OpenCcHkVariantsResourceName = "TcScTextConverterLib.Data.OpenCC.HKVariants.txt";

        private static readonly Lazy<DictionaryBundle[]> TraditionalToSimplifiedPipeline =
            new Lazy<DictionaryBundle[]>(() => new[]
            {
                LoadBundle(
                    new[]
                    {
                        new DictionaryResource(OpenCcTwPhrasesRevResourceName, false, false),
                        new DictionaryResource(OpenCcTwVariantsRevPhrasesResourceName, false, false),
                        new DictionaryResource(OpenCcHkVariantsRevPhrasesResourceName, false, false)
                    },
                    new[]
                    {
                        new DictionaryResource(OpenCcTwVariantsResourceName, true, false),
                        new DictionaryResource(OpenCcHkVariantsResourceName, true, false)
                    }),
                LoadBundle(
                    new[]
                    {
                        new DictionaryResource(OpenCcTsPhraseResourceName, false, false)
                    },
                    new[]
                    {
                        new DictionaryResource(OpenCcTsCharResourceName, false, false)
                    })
            });

        private static readonly Lazy<DictionaryBundle[]> SimplifiedToTraditionalPipeline =
            new Lazy<DictionaryBundle[]>(() => new[]
            {
                LoadBundle(
                    new[]
                    {
                        new DictionaryResource(OpenCcStPhraseResourceName, false, false)
                    },
                    new[]
                    {
                        new DictionaryResource(OpenCcStCharResourceName, false, false)
                    })
            });

        /// <summary>
        /// Converts Traditional Chinese text to Simplified Chinese by applying the configured conversion pipeline.
        /// </summary>
        /// <param name="traditionalText">The source Traditional Chinese text.</param>
        /// <returns>The converted Simplified Chinese text.</returns>
        public string ToSimplified(string traditionalText)
        {
            return Convert(traditionalText, TraditionalToSimplifiedPipeline.Value);
        }

        /// <summary>
        /// Converts Simplified Chinese text to Traditional Chinese by applying the configured conversion pipeline.
        /// </summary>
        /// <param name="simplifiedText">The source Simplified Chinese text.</param>
        /// <returns>The converted Traditional Chinese text.</returns>
        public string ToTraditional(string simplifiedText)
        {
            return Convert(simplifiedText, SimplifiedToTraditionalPipeline.Value);
        }

        /// <summary>
        /// Runs all dictionary stages in sequence for the supplied text.
        /// </summary>
        private static string Convert(string text, IEnumerable<DictionaryBundle> pipeline)
        {
            string current = text;
            foreach (DictionaryBundle bundle in pipeline)
            {
                current = ConvertSingleStage(current, bundle);
            }

            return current;
        }

        /// <summary>
        /// Converts text using a single dictionary stage with phrase matching followed by character fallback.
        /// </summary>
        private static string ConvertSingleStage(string text, DictionaryBundle bundle)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text ?? string.Empty;
            }

            string exactMatch;
            if (bundle.PhraseMap.TryGetValue(text, out exactMatch))
            {
                return exactMatch;
            }

            var builder = new StringBuilder(text.Length);
            int index = 0;
            while (index < text.Length)
            {
                string phraseMatch;
                int phraseLength;
                if (TryMatchPhrase(text, index, bundle, out phraseMatch, out phraseLength))
                {
                    builder.Append(phraseMatch);
                    index += phraseLength;
                    continue;
                }

                string single = text.Substring(index, 1);
                string charMatch;
                if (bundle.CharMap.TryGetValue(single, out charMatch))
                {
                    builder.Append(charMatch);
                }
                else
                {
                    builder.Append(single);
                }

                index++;
            }

            return builder.ToString();
        }

        /// <summary>
        /// Attempts to match the longest available phrase at the current text position.
        /// </summary>
        private static bool TryMatchPhrase(string text, int index, DictionaryBundle bundle, out string replacement, out int matchLength)
        {
            int maxLength = Math.Min(bundle.MaxPhraseLength, text.Length - index);
            for (int length = maxLength; length >= 2; length--)
            {
                string candidate = text.Substring(index, length);
                if (bundle.PhraseMap.TryGetValue(candidate, out replacement))
                {
                    matchLength = length;
                    return true;
                }
            }

            replacement = null;
            matchLength = 0;
            return false;
        }

        /// <summary>
        /// Loads one phrase dictionary set and one character dictionary set into a single conversion stage.
        /// </summary>
        private static DictionaryBundle LoadBundle(IEnumerable<DictionaryResource> phraseResources, IEnumerable<DictionaryResource> charResources)
        {
            var phraseMap = LoadMaps(phraseResources);
            var charMap = LoadMaps(charResources);
            int maxPhraseLength = phraseMap.Count == 0 ? 0 : phraseMap.Keys.Max(key => key.Length);
            return new DictionaryBundle(phraseMap, charMap, maxPhraseLength);
        }

        /// <summary>
        /// Loads and merges multiple dictionary resources into a single lookup map.
        /// </summary>
        private static Dictionary<string, string> LoadMaps(IEnumerable<DictionaryResource> resources)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (DictionaryResource resource in resources)
            {
                foreach (KeyValuePair<string, string> pair in LoadMap(resource))
                {
                    map[pair.Key] = pair.Value;
                }
            }

            return map;
        }

        /// <summary>
        /// Loads a single embedded dictionary resource into memory.
        /// </summary>
        private static Dictionary<string, string> LoadMap(DictionaryResource resource)
        {
            Assembly assembly = typeof(ChineseScriptConverter).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(resource.ResourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("Missing embedded dictionary resource: " + resource.ResourceName);
                }

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var map = new Dictionary<string, string>(StringComparer.Ordinal);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                        {
                            continue;
                        }

                        string[] parts = line.Split(new[] { '\t' }, 2);
                        if (parts.Length != 2 || string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
                        {
                            continue;
                        }

                        string source = resource.Reverse ? parts[1] : parts[0];
                        string target = resource.Reverse ? parts[0] : parts[1];
                        if (!resource.PreserveWholeTarget)
                        {
                            string[] values = target.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            target = values.Length == 0 ? target : values[0];
                        }

                        if (!string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(target) && !map.ContainsKey(source))
                        {
                            map[source] = target;
                        }
                    }

                    return map;
                }
            }
        }

        /// <summary>
        /// Stores one conversion stage containing phrase and character lookup tables.
        /// </summary>
        private sealed class DictionaryBundle
        {
            public DictionaryBundle(Dictionary<string, string> phraseMap, Dictionary<string, string> charMap, int maxPhraseLength)
            {
                PhraseMap = phraseMap;
                CharMap = charMap;
                MaxPhraseLength = maxPhraseLength;
            }

            public Dictionary<string, string> PhraseMap { get; private set; }
            public Dictionary<string, string> CharMap { get; private set; }
            public int MaxPhraseLength { get; private set; }
        }

        /// <summary>
        /// Describes how an embedded dictionary resource should be interpreted when loaded.
        /// </summary>
        private sealed class DictionaryResource
        {
            public DictionaryResource(string resourceName, bool reverse, bool preserveWholeTarget)
            {
                ResourceName = resourceName;
                Reverse = reverse;
                PreserveWholeTarget = preserveWholeTarget;
            }

            public string ResourceName { get; private set; }
            public bool Reverse { get; private set; }
            public bool PreserveWholeTarget { get; private set; }
        }
    }
}
