using System;
using System.Text;
using System.Text.RegularExpressions;
using SquidLib.SquidMath;

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

        public static Replacer[] AccentFinders => accentFinders;

        private static readonly StringBuilder sb = new StringBuilder(20);
        private static readonly StringBuilder ender = new StringBuilder(12);
        private static readonly StringBuilder ssb = new StringBuilder(80);

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
                    },
            VulgarChecks = new Regex[]
            {
                    new Regex("[sξζzkкκcсς][hнlι].{1,3}[dtтτΓг]", RegexOptions.IgnoreCase),
                    new Regex("(?:(?:[pрρ][hн])|[fd]).{1,3}[kкκcсςxхжχq]", RegexOptions.IgnoreCase), // lots of these end in a 'k' sound, huh
                    new Regex("[kкκcсςСQq][uμυνvhн]{1,3}[kкκcсςxхжχqmм]", RegexOptions.IgnoreCase),
                    new Regex("[bъыбвβЪЫБ].?[iτιyуλγУ].?[cсς]", RegexOptions.IgnoreCase),
                    new Regex("[hн][^aаαΛeезξεЗΣiτιyуλγУ][^aаαΛeезξεЗΣiτιyуλγУ]?[rяΓ]", RegexOptions.IgnoreCase),
                    new Regex("[tтτΓгcсς][iτιyуλγУ][tтτΓг]+$", RegexOptions.IgnoreCase),
                    new Regex("(?:(?:[pрρ][hн])|f)[aаαΛhн]{1,}[rяΓ][tтτΓг]", RegexOptions.IgnoreCase),
                    new Regex("[Ssξζzcсς][hн][iτιyуλγУ].?[sξζzcсς]", RegexOptions.IgnoreCase),
                    new Regex("[aаαΛ][nи][aаαΛeезξεЗΣiτιyуλγУoоюσοuμυνv]{1,2}[Ssξlιζz]", RegexOptions.IgnoreCase),
                    new Regex("[aаαΛ]([sξζz]{2})", RegexOptions.IgnoreCase),
                    new Regex("[kкκcсςСQq][hн]?[uμυνv]([hн]?)[nи]+[tтτΓг]", RegexOptions.IgnoreCase),
                    new Regex("[nиfvν]..?[jg]", RegexOptions.IgnoreCase), // might as well remove two possible slurs and a body part with one check
                    new Regex("[pрρ](?:(?:([eезξεЗΣoоюσοuμυνv])\\1)|(?:[eезξεЗΣiτιyуλγУuμυνv]+[sξζz]))", RegexOptions.IgnoreCase), // the grab bag of juvenile words
                    new Regex("[mм][hнwψшщ]?..?[rяΓ].?d", RegexOptions.IgnoreCase), // should pick up the #1 obscenity from Spanish and French
                    new Regex("[g][hн]?[aаαАΑΛeеёзξεЕЁЗΕΣ][yуλγУeеёзξεЕЁЗΕΣ]", RegexOptions.IgnoreCase), // could be inappropriate for random text
                    new Regex("[wψшщuμυνv](?:[hн]?)[aаαΛeеёзξεЗΕΣoоюσοuμυνv](?:[nи]+)[gkкκcсςxхжχq]", RegexOptions.IgnoreCase)
            };

        private static readonly Replacer[]
            accentFinders = new Replacer[]
            {
                    new Replacer("[àáâäăāãåąǻ]", "a"),
                    new Replacer("[èéêëĕēėęě]", "e"),
                    new Replacer("[ìíîïĭīĩįı]", "i"),
                    new Replacer("[òóôöŏōõøőǿ]", "o"),
                    new Replacer("[ùúûüŭūũůűų]", "u"),
                    new Replacer("[æǽ]", "ae"),
                    new Replacer("œ", "oe"),
                    new Replacer("[ÀÁÂÃÄÅĀĂĄǺ]", "A"),
                    new Replacer("[ÈÉÊËĒĔĖĘĚ]", "E"),
                    new Replacer("[ÌÍÎÏĨĪĬĮI]", "I"),
                    new Replacer("[ÒÓÔÕÖØŌŎŐǾ]", "O"),
                    new Replacer("[ÙÚÛÜŨŪŬŮŰŲ]", "U"),
                    new Replacer("[ÆǼ]", "Ae"),
                    new Replacer("Œ", "Oe"),
                    new Replacer("Ё", "Е"),
                    new Replacer("Й", "И"),
                    new Replacer("[çćĉċč]", "c"),
                    new Replacer("[þðďđ]", "d"),
                    new Replacer("[ĝğġģ]", "g"),
                    new Replacer("[ĥħ]", "h"),
                    new Replacer("[ĵȷ]", "j"),
                    new Replacer("ķ", "k"),
                    new Replacer("[ĺļľŀłļ]", "l"),
                    new Replacer("[ñńņňŋ]", "n"),
                    new Replacer("[ŕŗřŗŕ]", "r"),
                    new Replacer("[śŝşšș]", "s"),
                    new Replacer("[ţťŧț]", "t"),
                    new Replacer("[ŵẁẃẅ]", "w"),
                    new Replacer("[ýÿŷỳ]", "y"),
                    new Replacer("[źżž]", "z"),
                    new Replacer("[ÇĆĈĊČ]", "C"),
                    new Replacer("[ÞÐĎĐḌ]", "D"),
                    new Replacer("[ĜĞĠĢ]", "G"),
                    new Replacer("[ĤĦḤ]", "H"),
                    new Replacer("Ĵ", "J"),
                    new Replacer("Ķ", "K"),
                    new Replacer("[ĹĻĽĿŁḶḸĻ]", "L"),
                    new Replacer("Ṃ", "M"),
                    new Replacer("[ÑŃŅŇŊṄṆ]", "N"),
                    new Replacer("[ŔŖŘṚṜŖŔ]", "R"),
                    new Replacer("[ŚŜŞŠȘṢ]", "S"),
                    new Replacer("[ŢŤŦȚṬ]", "T"),
                    new Replacer("[ŴẀẂẄ]", "W"),
                    new Replacer("[ÝŸŶỲ]", "Y"),
                    new Replacer("[ŹŻŽ]", "Z"),
                    new Replacer("ё", "е"),
                    new Replacer("й", "и"),
            };

        /**
    * A pattern String that will match any vowel FakeLanguageGen can produce out-of-the-box, including Latin, Greek,
    * and Cyrillic; for use when a String will be interpreted as a regex (as in {@link FakeLanguageGen.Alteration}).
*/
        public static readonly string AnyVowel = "[àáâãäåæāăąǻǽaèéêëēĕėęěeìíîïĩīĭįıiòóôõöøōŏőœǿoùúûüũūŭůűųuýÿŷỳyαοειυωаеёийоуъыэюя]",
        /**
         * A pattern String that will match one or more of any vowels FakeLanguageGen can produce out-of-the-box, including
         * Latin, Greek, and Cyrillic; for use when a String will be interpreted as a regex (as in 
         * {@link FakeLanguageGen.Alteration}).
         */
        AnyVowelCluster = AnyVowel + '+',
        /**
         * A pattern String that will match any consonant FakeLanguageGen can produce out-of-the-box, including Latin,
         * Greek, and Cyrillic; for use when a String will be interpreted as a regex (as in
         * {@link FakeLanguageGen.Alteration}).
         */
        AnyConsonant = "[bcçćĉċčdþðďđfgĝğġģhĥħjĵȷkķlĺļľŀłmnñńņňŋpqrŕŗřsśŝşšștţťțvwŵẁẃẅxyýÿŷỳzźżžρσζτκχνθμπψβλγφξςбвгдклпрстфхцжмнзчшщ]",
        /**
         * A pattern String that will match one or more of any consonants FakeLanguageGen can produce out-of-the-box,
         * including Latin, Greek, and Cyrillic; for use when a String will be interpreted as a regex (as in
         * {@link FakeLanguageGen.Alteration}).
         */
        AnyConsonantCluster = AnyConsonant + '+';

        protected static readonly Regex repeats = new Regex("(.)\\1+", RegexOptions.IgnoreCase),
                vowelClusters = new Regex(AnyVowelCluster, RegexOptions.IgnoreCase),
                consonantClusters = new Regex(AnyConsonantCluster, RegexOptions.IgnoreCase);
        protected static bool CheckAll(string testing, Regex[] checks) {
            if (checks == null || checks.Length == 0) return true;
            testing = RemoveAccents(testing);
            for (int i = 0; i < checks.Length; i++) {
                if (checks[i].IsMatch(testing)) return false;
            }
            return true;
        }
        public static string RemoveAccents(string str) {
            string alteredString = str;
            for (int i = 0; i < AccentFinders.Length; i++) {
                alteredString = AccentFinders[i].Replace(alteredString);
            }
            return alteredString;
        }

        public string Word(IRNG rng, bool capitalize, int lowerSyllables, int upperSyllables, Regex[] additionalChecks) {
            if (lowerSyllables <= 0 || upperSyllables <= 0) {
                sb.Length = 0;
                sb.Append(rng.RandomElement(OpeningVowels));
                //for (int m = 0; m < modifiers.size(); m++) {
                //    modifiers.get(m).modify(rng, sb);
                //}
                if (capitalize) sb[0] = char.ToUpper(sb[0]);
                return sb.ToString();
            }
            int approxSyllables = rng.NextInt(lowerSyllables, upperSyllables + 1);
            while (true) {
                sb.Length = 0;
                ender.Length = 0;
                int i = 0;
                if (rng.NextDouble() < VowelStartFrequency) {
                    sb.Append(rng.RandomElement(OpeningVowels));
                    if (approxSyllables == 1 && ClosingConsonants.Length > 0)
                        sb.Append(rng.RandomElement(ClosingConsonants));
                    else if (MidConsonants.Length > 0)
                        sb.Append(rng.RandomElement(MidConsonants));
                    i++;
                } else if (OpeningConsonants.Length > 0) {
                    sb.Append(rng.RandomElement(OpeningConsonants));
                }
                String close = "";
                bool redouble = false;
                if (i < approxSyllables) {
                    if (ClosingSyllables.Length > 0 && rng.NextDouble() < SyllableEndFrequency) {
                        close = rng.RandomElement(ClosingSyllables);
                        if (close.Contains("@") && (approxSyllables & 1) == 0) {
                            redouble = true;
                            approxSyllables >>= 1;
                        }
                        if (!close.Contains("@"))
                            ender.Append(close);
                        else if (redouble && rng.NextDouble() < VowelEndFrequency) {
                            ender.Append(rng.RandomElement(MidVowels));
                            if (VowelSplitters.Length > 0 && rng.NextDouble() < VowelSplitFrequency) {
                                ender.Append(rng.RandomElement(VowelSplitters))
                                        .Append(rng.RandomElement(MidVowels));
                            }
                        }
                    } else {
                        ender.Append(rng.RandomElement(MidVowels));
                        if (rng.NextDouble() < VowelSplitFrequency) {
                            ender.Append(rng.RandomElement(VowelSplitters))
                                    .Append(rng.RandomElement(MidVowels));
                        }
                        if (rng.NextDouble() >= VowelEndFrequency) {
                            ender.Append(rng.RandomElement(ClosingConsonants));
                            if (rng.NextDouble() < SyllableEndFrequency) {
                                close = rng.RandomElement(ClosingSyllables);
                                if (close.Contains("@") && (approxSyllables & 1) == 0) {
                                    redouble = true;
                                    approxSyllables >>= 1;
                                }
                                if (!close.Contains("@"))
                                    ender.Append(close);
                            }
                        }
                    }
                    i += vowelClusters.Matches(ender.ToString()).Count;
                }

                for (; i < approxSyllables; i++) {
                    sb.Append(rng.RandomElement(MidVowels));
                    if (rng.NextDouble() < VowelSplitFrequency) {
                        sb.Append(rng.RandomElement(VowelSplitters))
                                .Append(rng.RandomElement(MidVowels));
                    }
                    sb.Append(rng.RandomElement(MidConsonants));
                }

                sb.Append(ender);
                if (redouble && i <= approxSyllables + 1) {
                    sb.Append(close.Replace("@", sb.ToString()));
                }
                if (capitalize)
                    sb[0] = char.ToUpper(sb[0]);
                string str = sb.ToString();
                if (SanityChecks != null && !CheckAll(str, SanityChecks))
                    continue;

                //for (int m = 0; m < modifiers.size(); m++) {
                //    str = modifiers.get(m).modify(rng, str);
                //}

                if (Clean && !CheckAll(str, VulgarChecks))
                    continue;

                if (additionalChecks != null && !CheckAll(str, additionalChecks))
                    continue;

                return str;
            }
        }

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
