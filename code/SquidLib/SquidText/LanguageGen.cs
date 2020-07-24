using System;
using System.Text.RegularExpressions;

namespace SquidLib.SquidText {
    public class LanguageGen {
        public LanguageGen(String[] openingVowels, String[] midVowels, String[] openingConsonants,
                       String[] midConsonants, String[] closingConsonants, String[] closingSyllables, String[] vowelSplitters,
                       int[] syllableLengths, double[] syllableFrequencies, double vowelStartFrequency,
                       double vowelEndFrequency, double vowelSplitFrequency, double syllableEndFrequency,
                       Regex[] sane, bool clean) {
            this.OpeningVowels = openingVowels;
            this.MidVowels = new String[openingVowels.Length + midVowels.Length];
            Array.Copy(midVowels, 0, this.MidVowels, 0, midVowels.Length);
            Array.Copy(openingVowels, 0, this.MidVowels, midVowels.Length, openingVowels.Length);
            this.OpeningConsonants = openingConsonants;
            this.MidConsonants = new String[midConsonants.Length + closingConsonants.Length];
            Array.Copy(midConsonants, 0, this.MidConsonants, 0, midConsonants.Length);
            Array.Copy(closingConsonants, 0, this.MidConsonants, midConsonants.Length, closingConsonants.Length);
            this.ClosingConsonants = closingConsonants;
            this.VowelSplitters = vowelSplitters;
            this.ClosingSyllables = closingSyllables;

            this.SyllableFrequencies = new double[syllableLengths[syllableLengths.Length - 1]];
            TotalSyllableFrequency = 0.0;
            for (int i = 0; i < syllableLengths.Length; i++) {
                TotalSyllableFrequency += (this.SyllableFrequencies[syllableLengths[i] - 1] = syllableFrequencies[i]);
            }

            if (vowelStartFrequency > 1.0)
                this.VowelStartFrequency = 1.0 / vowelStartFrequency;
            else
                this.VowelStartFrequency = vowelStartFrequency;
            if (vowelEndFrequency > 1.0)
                this.VowelEndFrequency = 1.0 / vowelEndFrequency;
            else
                this.VowelEndFrequency = vowelEndFrequency;
            if (vowelSplitters.Length == 0)
                this.VowelSplitFrequency = 0.0;
            else if (vowelSplitFrequency > 1.0)
                this.VowelSplitFrequency = 1.0 / vowelSplitFrequency;
            else
                this.VowelSplitFrequency = vowelSplitFrequency;
            if (closingSyllables.Length == 0)
                this.SyllableEndFrequency = 0.0;
            else if (syllableEndFrequency > 1.0)
                this.SyllableEndFrequency = 1.0 / syllableEndFrequency;
            else
                this.SyllableEndFrequency = syllableEndFrequency;
            this.Clean = clean;
            SanityChecks = sane;
            //modifiers = new List<>(4);
        }

        public string[] OpeningVowels { get; }
        public string[] MidVowels { get; }
        public string[] OpeningConsonants { get; }
        public string[] MidConsonants { get; }
        public string[] ClosingConsonants { get; }
        public string[] VowelSplitters { get; }
        public string[] ClosingSyllables { get; }
        public double[] SyllableFrequencies { get; }
        public double TotalSyllableFrequency { get; }
        public double VowelStartFrequency { get; }
        public double VowelEndFrequency { get; }
        public double VowelSplitFrequency { get; }
        public double SyllableEndFrequency { get; }
        public bool Clean { get; }
        public Regex[] SanityChecks { get; }

        public static readonly Regex[] GenericSanityChecks = new Regex[]
                    {
                        new Regex("[aeiou]{3}", RegexOptions.IgnoreCase),
                            new Regex("(\\p{L})\\1\\1", RegexOptions.IgnoreCase),
                            new Regex("[i][iyq]", RegexOptions.IgnoreCase),
                            new Regex("[y]([aiu])\\1", RegexOptions.IgnoreCase),
                            new Regex("[r][uy]+[rh]", RegexOptions.IgnoreCase),
                            new Regex("[q]u[yu]", RegexOptions.IgnoreCase),
                            new Regex("[^oaei]uch", RegexOptions.IgnoreCase),
                            new Regex("[h][tcszi]?h", RegexOptions.IgnoreCase),
                            new Regex("[t]t[^aeiouy]{2}", RegexOptions.IgnoreCase),
                            new Regex("[y]h([^aeiouy]|$)", RegexOptions.IgnoreCase),
                            new Regex("([xqy])\\1$", RegexOptions.IgnoreCase),
                            new Regex("[qi]y$", RegexOptions.IgnoreCase),
                            new Regex("[szrlL]+?[^aeiouytdfgkcpbmnslrv][rlsz]", RegexOptions.IgnoreCase),
                            new Regex("[uiy][wy]", RegexOptions.IgnoreCase),
                            new Regex("^[ui]e", RegexOptions.IgnoreCase),
                            new Regex("^([^aeioyl])\\1", RegexOptions.IgnoreCase)
                    };

        public static readonly LanguageGen SIMPLISH = new LanguageGen(
                new String[]{
                        "a", "a", "a", "a", "o", "o", "o", "e", "e", "e", "e", "e", "i", "i", "i", "i", "u",
                        "a", "a", "a", "a", "o", "o", "o", "e", "e", "e", "e", "e", "i", "i", "i", "i", "u",
                        "a", "a", "a", "a", "o", "o", "o", "e", "e", "e", "e", "e", "i", "i", "i", "i", "u",
                        "a", "a", "a", "o", "o", "e", "e", "e", "i", "i", "i", "u",
                        "a", "a", "a", "o", "o", "e", "e", "e", "i", "i", "i", "u",
                        "ai", "ai", "ea", "io", "oi", "ia", "io", "eo"
                },
                new String[] { "u", "u", "oa" },
                new String[]{
                        "b", "bl", "br", "c", "cl", "cr", "ch", "d", "dr", "f", "fl", "fr", "g", "gl", "gr", "h", "j", "k", "l", "m", "n",
                        "p", "pl", "pr", "r", "s", "sh", "sk", "st", "sp", "sl", "sm", "sn", "t", "tr", "th", "v", "w", "y", "z",
                        "b", "bl", "br", "c", "cl", "cr", "ch", "d", "dr", "f", "fl", "fr", "g", "gr", "h", "j", "k", "l", "m", "n",
                        "p", "pl", "pr", "r", "s", "sh", "st", "sp", "sl", "t", "tr", "th", "w", "y",
                        "b", "c", "ch", "d", "f", "g", "h", "j", "k", "l", "m", "n",
                        "p", "r", "s", "sh", "t", "th",
                        "b", "c", "ch", "d", "f", "g", "h", "j", "k", "l", "m", "n",
                        "p", "r", "s", "sh", "t", "th",
                        "b", "c", "ch", "d", "f", "g", "h", "j", "k", "l", "m", "n",
                        "p", "r", "s", "sh", "t", "th",
                        "b", "c", "ch", "d", "f", "g", "h", "j", "k", "l", "m", "n",
                        "p", "r", "s", "sh", "t", "th",
                        "b", "d", "f", "g", "h", "l", "m", "n",
                        "p", "r", "s", "sh", "t", "th",
                        "b", "d", "f", "g", "h", "l", "m", "n",
                        "p", "r", "s", "sh", "t", "th",
                        "r", "s", "t", "l", "n",
                },
                new String[]{"ch", "j", "w", "y", "v", "w", "y", "w", "y", "ch",
                        "b", "c", "d", "f", "g", "k", "l", "m", "n", "p", "r", "s", "sh", "t",
                },
                new String[]{"bs", "lt", "mb", "ng", "ng", "nt", "ns", "ps", "mp", "rt", "rg", "sk", "rs", "ts", "lk", "ct",
                        "b", "c", "d", "f", "g", "k", "l", "m", "n", "p", "r", "s", "sh", "t", "th", "z",
                        "b", "c", "d", "f", "g", "k", "l", "m", "n", "p", "r", "s", "sh", "t",
                        "b", "c", "d", "f", "g", "k", "l", "m", "n", "p", "r", "s", "sh", "t",
                        "d", "f", "g", "k", "l", "m", "n", "p", "r", "s", "sh", "t",
                        "d", "f", "g", "k", "l", "m", "n", "p", "r", "s", "sh", "t",
                        "d", "f", "g", "k", "l", "m", "n", "p", "r", "s", "sh", "t",
                        "d", "f", "g", "k", "l", "m", "n", "p", "r", "s", "sh", "t",
                },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2, 3, 4 }, new double[] { 7, 18, 6, 1 }, 0.26, 0.12, 0.0, 0.0, GenericSanityChecks, true);
    }
}
