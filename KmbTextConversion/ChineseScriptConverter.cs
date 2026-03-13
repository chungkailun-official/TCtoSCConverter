using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace KmbTextConversion
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
    /// Provides an offline Chinese script converter backed by embedded OpenCC-style dictionaries and KMB overrides.
    /// </summary>
    public sealed class ChineseScriptConverter : IChineseScriptConverter
    {
        private const string OpenCcTsPhraseResourceName = "KmbTextConversion.Data.OpenCC.TSPhrases.txt";
        private const string OpenCcTsCharResourceName = "KmbTextConversion.Data.OpenCC.TSCharacters.txt";
        private const string OpenCcStPhraseResourceName = "KmbTextConversion.Data.OpenCC.STPhrases.txt";
        private const string OpenCcStCharResourceName = "KmbTextConversion.Data.OpenCC.STCharacters.txt";
        private const string OpenCcTwPhrasesRevResourceName = "KmbTextConversion.Data.OpenCC.TWPhrasesRev.txt";
        private const string OpenCcTwVariantsRevPhrasesResourceName = "KmbTextConversion.Data.OpenCC.TWVariantsRevPhrases.txt";
        private const string OpenCcTwVariantsResourceName = "KmbTextConversion.Data.OpenCC.TWVariants.txt";
        private const string OpenCcHkVariantsRevPhrasesResourceName = "KmbTextConversion.Data.OpenCC.HKVariantsRevPhrases.txt";
        private const string OpenCcHkVariantsResourceName = "KmbTextConversion.Data.OpenCC.HKVariants.txt";
        private const string KmbPhraseResourceName = "KmbTextConversion.Data.kmb_tc_to_sc.tsv";
        private const string KmbCharResourceName = "KmbTextConversion.Data.kmb_char_tc_to_sc.tsv";

        private static readonly Lazy<DictionaryBundle[]> TraditionalToSimplifiedPipeline =
            new Lazy<DictionaryBundle[]>(() => new[]
            {
                LoadBundle(
                    new[]
                    {
                        new DictionaryResource(KmbPhraseResourceName, false, true)
                    },
                    new[]
                    {
                        new DictionaryResource(KmbCharResourceName, false, true)
                    }),
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
                        new DictionaryResource(KmbPhraseResourceName, true, true)
                    },
                    new[]
                    {
                        new DictionaryResource(KmbCharResourceName, true, true)
                    }),
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
        /// <param name="text">The source text to process.</param>
        /// <param name="pipeline">The ordered dictionary stages to apply.</param>
        /// <returns>The converted text after all stages have been applied.</returns>
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
        /// <param name="text">The input text.</param>
        /// <param name="bundle">The dictionary stage to apply.</param>
        /// <returns>The converted text for the stage.</returns>
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
        /// <param name="text">The full input text.</param>
        /// <param name="index">The current character index.</param>
        /// <param name="bundle">The dictionary stage being used.</param>
        /// <param name="replacement">Receives the replacement phrase when a match is found.</param>
        /// <param name="matchLength">Receives the matched source phrase length.</param>
        /// <returns><c>true</c> when a phrase match is found; otherwise, <c>false</c>.</returns>
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
        /// <param name="phraseResources">The phrase dictionary resources.</param>
        /// <param name="charResources">The character dictionary resources.</param>
        /// <returns>A bundled dictionary stage.</returns>
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
        /// <param name="resources">The resources to load.</param>
        /// <returns>A merged lookup map.</returns>
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
        /// <param name="resource">The dictionary resource metadata.</param>
        /// <returns>A lookup map built from the resource.</returns>
        private static Dictionary<string, string> LoadMap(DictionaryResource resource)
        {
            var assembly = typeof(ChineseScriptConverter).Assembly;
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
            /// <summary>
            /// Initializes a new dictionary bundle.
            /// </summary>
            /// <param name="phraseMap">The phrase-level lookup map.</param>
            /// <param name="charMap">The character-level lookup map.</param>
            /// <param name="maxPhraseLength">The longest phrase length in the bundle.</param>
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
            /// <summary>
            /// Initializes a new dictionary resource descriptor.
            /// </summary>
            /// <param name="resourceName">The fully qualified embedded resource name.</param>
            /// <param name="reverse">Whether the source and target columns should be swapped.</param>
            /// <param name="preserveWholeTarget">Whether the full target value should be preserved instead of taking the first candidate.</param>
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
