using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SquidLib.SquidMath;

namespace SquidLib.SquidText {
    public class LanguageGen {
        public LanguageGen(string[] openingVowels, string[] midVowels, string[] openingConsonants,
                       string[] midConsonants, string[] closingConsonants, string[] closingSyllables, string[] vowelSplitters,
                       int[] syllableLengths, double[] syllableFrequencies, double vowelStartFrequency,
                       double vowelEndFrequency, double vowelSplitFrequency, double syllableEndFrequency,
                       Regex[] sane, bool clean) {
            this.OpeningVowels = openingVowels;
            this.MidVowels = new string[openingVowels.Length + midVowels.Length];
            Array.Copy(midVowels, 0, this.MidVowels, 0, midVowels.Length);
            Array.Copy(openingVowels, 0, this.MidVowels, midVowels.Length, openingVowels.Length);
            this.OpeningConsonants = openingConsonants;
            this.MidConsonants = new string[midConsonants.Length + closingConsonants.Length];
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
            Modifiers = new List<Modifier>(4);
        }

        internal LanguageGen(LanguageGen other) {
            OpeningVowels = ArrayTools.Copy(other.OpeningVowels);
            MidVowels = ArrayTools.Copy(other.MidVowels);
            OpeningConsonants = ArrayTools.Copy(other.OpeningConsonants);
            MidConsonants = ArrayTools.Copy(other.MidConsonants);
            ClosingConsonants = ArrayTools.Copy(other.ClosingConsonants);
            VowelSplitters = ArrayTools.Copy(other.VowelSplitters);
            ClosingSyllables = ArrayTools.Copy(other.ClosingSyllables);
            SyllableFrequencies = ArrayTools.Copy(other.SyllableFrequencies);
            TotalSyllableFrequency = other.TotalSyllableFrequency;
            VowelStartFrequency = other.VowelStartFrequency;
            VowelEndFrequency = other.VowelEndFrequency;
            VowelSplitFrequency = other.VowelSplitFrequency;
            SyllableEndFrequency = other.SyllableEndFrequency;
            Clean = other.Clean;
            SanityChecks = other.SanityChecks;
            Modifiers = new List<Modifier>(other.Modifiers);
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
        public List<Modifier> Modifiers { get; }
        internal string Summary { get; set; }
        public string Name { get; internal set; }


        public static IndexedDictionary<string, LanguageGen> Registry { get; } = new IndexedDictionary<string, LanguageGen>(64, StringComparer.OrdinalIgnoreCase);

        public LanguageGen Register(string languageName) {
            if (Registry.Count == 0) Registry[""] = null;
            Summary = Registry.Count + "@1";
            Name = languageName;
            Registry[languageName] = this;
            return this;
        }
        public static Replacer[] AccentFinders { get; } = new Replacer[]
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
            EnglishSanityChecks = new Regex[]
            {
                            new Regex("[aeiou]{3}", RegexOptions.IgnoreCase),
                            new Regex("(\\w)\\1\\1", RegexOptions.IgnoreCase),
                            new Regex("(.)\\1(.)\\2", RegexOptions.IgnoreCase),
                            new Regex("[a][ae]", RegexOptions.IgnoreCase),
                            new Regex("[u][umlkj]", RegexOptions.IgnoreCase),
                            new Regex("[i][iyqkhrl]", RegexOptions.IgnoreCase),
                            new Regex("[o][c]", RegexOptions.IgnoreCase),
                            new Regex("[y]([aiu])\\1", RegexOptions.IgnoreCase),
                            new Regex("[r][aeiouy]+[rh]", RegexOptions.IgnoreCase),
                            new Regex("[q]u[yu]", RegexOptions.IgnoreCase),
                            new Regex("[^oaei]uch", RegexOptions.IgnoreCase),
                            new Regex("[h][tcszi]?h", RegexOptions.IgnoreCase),
                            new Regex("[t]t[^aeiouy]{2}", RegexOptions.IgnoreCase),
                            new Regex("[y]h([^aeiouy]|$)", RegexOptions.IgnoreCase),
                            new Regex("[szrl]+?[^aeiouytdfgkcpbmnslr][szrl]", RegexOptions.IgnoreCase),
                            new Regex("[uiy][wy]", RegexOptions.IgnoreCase),
                            new Regex("^[ui][ae]", RegexOptions.IgnoreCase),
                            new Regex("q(?:u?)$", RegexOptions.IgnoreCase)
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

        /**
    * A pattern string that will match any vowel FakeLanguageGen can produce out-of-the-box, including Latin, Greek,
    * and Cyrillic; for use when a string will be interpreted as a regex (as in {@link FakeLanguageGen.Alteration}).
*/
        public static readonly string AnyVowel = "[àáâãäåæāăąǻǽaèéêëēĕėęěeìíîïĩīĭįıiòóôõöøōŏőœǿoùúûüũūŭůűųuýÿŷỳyαοειυωаеёийоуъыэюя]",
        /**
         * A pattern string that will match one or more of any vowels FakeLanguageGen can produce out-of-the-box, including
         * Latin, Greek, and Cyrillic; for use when a string will be interpreted as a regex (as in 
         * {@link FakeLanguageGen.Alteration}).
         */
        AnyVowelCluster = AnyVowel + '+',
        /**
         * A pattern string that will match any consonant FakeLanguageGen can produce out-of-the-box, including Latin,
         * Greek, and Cyrillic; for use when a string will be interpreted as a regex (as in
         * {@link FakeLanguageGen.Alteration}).
         */
        AnyConsonant = "[bcçćĉċčdþðďđfgĝğġģhĥħjĵȷkķlĺļľŀłmnñńņňŋpqrŕŗřsśŝşšștţťțvwŵẁẃẅxyýÿŷỳzźżžρσζτκχνθμπψβλγφξςбвгдклпрстфхцжмнзчшщ]",
        /**
         * A pattern string that will match one or more of any consonants FakeLanguageGen can produce out-of-the-box,
         * including Latin, Greek, and Cyrillic; for use when a string will be interpreted as a regex (as in
         * {@link FakeLanguageGen.Alteration}).
         */
        AnyConsonantCluster = AnyConsonant + '+';

        protected static readonly Regex repeats = new Regex("(.)\\1+", RegexOptions.IgnoreCase),
                vowelClusters = new Regex(AnyVowelCluster, RegexOptions.IgnoreCase),
                consonantClusters = new Regex(AnyConsonantCluster, RegexOptions.IgnoreCase);

        private static readonly string[] mid = new string[] { ",", ",", ",", ";" }, end = new string[] { ".", ".", ".", ".", "!", "?", "..." };

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
        public string Word() => Word(null, false, null);
        public string Word(IRNG rng) => Word(rng, false, null);
        public string Word(IRNG rng, bool capitalize) => Word(rng, capitalize, null);
        public string Word(IRNG rng, bool capitalize, int approximateSyllables) => Word(rng, capitalize, approximateSyllables, approximateSyllables, null);
        public string Word(IRNG rng, bool capitalize, int lowerSyllables, int upperSyllables) => Word(rng, capitalize, lowerSyllables, upperSyllables, null);
        public string Word(IRNG rng, bool capitalize, Regex[] additionalChecks) {
            if (rng == null) rng = new RNG();
            while (true) {
                sb.Length = 0;
                ender.Length = 0;

                double syllableChance = rng.NextDouble(TotalSyllableFrequency);
                int syllables = 1, i = 0;
                for (int s = 0; s < SyllableFrequencies.Length; s++) {
                    if (syllableChance < SyllableFrequencies[s]) {
                        syllables = s + 1;
                        break;
                    } else {
                        syllableChance -= SyllableFrequencies[s];
                    }
                }
                if (rng.NextDouble() < VowelStartFrequency) {
                    sb.Append(rng.RandomElement(OpeningVowels));
                    if (syllables == 1)
                        sb.Append(rng.RandomElement(ClosingConsonants));
                    else
                        sb.Append(rng.RandomElement(MidConsonants));
                    i++;
                } else {
                    sb.Append(rng.RandomElement(OpeningConsonants));
                }
                string close = "";
                bool redouble = false;
                if (i < syllables) {
                    if (rng.NextDouble() < SyllableEndFrequency) {
                        close = rng.RandomElement(ClosingSyllables);
                        if (close.Contains("@") && (syllables & 1) == 0) {
                            redouble = true;
                            syllables >>= 1;
                        }
                        if (!close.Contains("@"))
                            ender.Append(close);
                        else if (rng.NextDouble() < VowelEndFrequency) {
                            ender.Append(rng.RandomElement(MidVowels));
                            if (rng.NextDouble() < VowelSplitFrequency) {
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
                                if (close.Contains("@") && (syllables & 1) == 0) {
                                    redouble = true;
                                    syllables >>= 1;
                                }
                                if (!close.Contains("@"))
                                    ender.Append(close);
                            }
                        }
                    }
                    i += vowelClusters.Matches(ender.ToString()).Count;
                }

                for (; i < syllables; i++) {
                    sb.Append(rng.RandomElement(MidVowels));
                    if (rng.NextDouble() < VowelSplitFrequency) {
                        sb.Append(rng.RandomElement(VowelSplitters))
                                .Append(rng.RandomElement(MidVowels));
                    }
                    sb.Append(rng.RandomElement(MidConsonants));
                }

                sb.Append(ender);
                if (redouble && i <= syllables + 1) {
                    sb.Append(close.Replace("@", sb.ToString()));
                }

                if (capitalize)
                    sb[0] = char.ToUpperInvariant(sb[0]);
                string str = sb.ToString();
                if (SanityChecks != null && !CheckAll(str, SanityChecks))
                    continue;

                for (int m = 0; m < Modifiers.Count; m++) {
                    str = Modifiers[m].Modify(rng, str);
                }

                if (Clean && !CheckAll(str, VulgarChecks))
                    continue;

                if (additionalChecks != null && !CheckAll(str, additionalChecks))
                    continue;

                return str;
            }
        }

        public string Word(IRNG rng, bool capitalize, int lowerSyllables, int upperSyllables, Regex[] additionalChecks) {
            if (rng == null) rng = new RNG();
            if (lowerSyllables <= 0 || upperSyllables <= 0) {
                sb.Length = 0;
                sb.Append(rng.RandomElement(OpeningVowels));
                if (Modifiers.Count > 0) {
                    string str = sb.ToString();
                    for (int m = 0; m < Modifiers.Count; m++) {
                        str = Modifiers[m].Modify(rng, str);
                    }
                    if (capitalize) return char.ToUpperInvariant(str[0]) + str.Substring(1);
                    else return str;
                }
                if (capitalize) sb[0] = char.ToUpperInvariant(sb[0]);
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
                string close = "";
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
                    sb[0] = char.ToUpperInvariant(sb[0]);
                string str = sb.ToString();
                if (SanityChecks != null && !CheckAll(str, SanityChecks))
                    continue;

                for (int m = 0; m < Modifiers.Count; m++) {
                    str = Modifiers[m].Modify(rng, str);
                }

                if (Clean && !CheckAll(str, VulgarChecks))
                    continue;

                if (additionalChecks != null && !CheckAll(str, additionalChecks))
                    continue;

                return str;
            }
        }
        public string Sentence(IRNG rng) => Sentence(rng, 1, 7, mid, end, 0.2);
        public string Sentence(IRNG rng, int minWords, int maxWords) => Sentence(rng, minWords, maxWords, mid, end, 0.2);
        public string Sentence(IRNG rng, int minWords, int maxWords, int maxChars) => Sentence(rng, minWords, maxWords, mid, end, 0.2, maxChars);

        public string Sentence(IRNG rng, int minWords, int maxWords, string[] midPunctuation, string[] endPunctuation,
                           double midPunctuationFrequency) {
            if (rng == null) rng = new RNG();
            if (minWords < 1)
                minWords = 1;
            if (minWords > maxWords)
                maxWords = minWords;
            if (midPunctuationFrequency > 1.0) {
                midPunctuationFrequency = 1.0 / midPunctuationFrequency;
            }
            ssb.Length = 0;
            ssb.EnsureCapacity(12 * maxWords);
            ssb.Append(Word(rng, true, null));
            for (int i = 1; i < minWords; i++) {
                if (rng.NextDouble() < midPunctuationFrequency) {
                    ssb.Append(rng.RandomElement(midPunctuation));
                }
                ssb.Append(' ').Append(Word(rng, false, null));
            }
            for (int i = minWords; i < maxWords && rng.NextInt(2 * maxWords) > i; i++) {
                if (rng.NextDouble() < midPunctuationFrequency) {
                    ssb.Append(rng.RandomElement(midPunctuation));
                }
                ssb.Append(' ').Append(Word(rng, false, null));
            }
            if (endPunctuation != null && endPunctuation.Length > 0)
                ssb.Append(rng.RandomElement(endPunctuation));
            return ssb.ToString();
        }

        public string Sentence(IRNG rng, int minWords, int maxWords, string[] midPunctuation, string[] endPunctuation,
                       double midPunctuationFrequency, int maxChars) {
            if (rng == null) rng = new RNG();
            if (maxChars < 0)
                return Sentence(rng, minWords, maxWords, midPunctuation, endPunctuation, midPunctuationFrequency);
            if (minWords < 1)
                minWords = 1;
            if (minWords > maxWords)
                maxWords = minWords;
            if (midPunctuationFrequency > 1.0) {
                midPunctuationFrequency = 1.0 / midPunctuationFrequency;
            }
            if (maxChars < 4)
                return "!";
            if (maxChars <= 5 * minWords) {
                minWords = 1;
                maxWords = 1;
            }
            int frustration = 0;
            ssb.Length = 0;
            ssb.EnsureCapacity(maxChars);
            string next = Word(rng, true);
            while (next.Length >= maxChars - 1 && frustration < 50) {
                next = Word(rng, true);
                frustration++;
            }
            if (frustration >= 50) return "!";
            ssb.Append(next);
            for (int i = 1; i < minWords && ssb.Length < maxChars - 7; i++) {
                if (rng.NextDouble() < midPunctuationFrequency && ssb.Length < maxChars - 3) {
                    ssb.Append(rng.RandomElement(midPunctuation));
                }
                next = Word(rng, false);
                while (ssb.Length + next.Length >= maxChars - 2 && frustration < 50) {
                    next = Word(rng, false);
                    frustration++;
                }
                if (frustration >= 50) break;
                ssb.Append(' ').Append(next);
            }
            for (int i = minWords; i < maxWords && ssb.Length < maxChars - 7 && rng.NextInt(2 * maxWords) > i && frustration < 50; i++) {
                if (rng.NextDouble() < midPunctuationFrequency && ssb.Length < maxChars - 3) {
                    ssb.Append(rng.RandomElement(midPunctuation));
                }
                next = Word(rng, false);
                while (ssb.Length + next.Length >= maxChars - 2 && frustration < 50) {
                    next = Word(rng, false);
                    frustration++;
                }
                if (frustration >= 50) break;
                ssb.Append(' ');
                ssb.Append(next);
            }

            if (endPunctuation != null && endPunctuation.Length > 0) {

                next = rng.RandomElement(endPunctuation);
                if (ssb.Length + next.Length >= maxChars)
                    ssb.Append('.');
                else
                    ssb.Append(next);
            }

            if (ssb.Length > maxChars)
                return "!";
            return ssb.ToString();
        }

        private LanguageGen AddModifiers(params Modifier[] modifiers) {
            LanguageGen cp = new LanguageGen(this);
            cp.Modifiers.AddRange(modifiers);
            return cp;
        }

        #region LANGUAGES
        /* Goal languages to implement, in the order they should be registered:
Lovecraft
English
Greek Romanized
Greek Authentic
French
Russian Romanized
Russian Authentic
Japanese Romanized
Swahili
Somali
Hindi Romanized
Arabic Romanized
Inuktitut
Norse
Nahuatl
Mongolian
Fantasy
Fancy Fantasy
Goblin
Elf
Demonic
Infernal
Simplish
Alien A
Korean Romanized
Alien E
Alien I
Alien O
Alien U
Dragon
Kobold
Insect
Maori
Spanish
Deep Speech
Norse Simplified
Hletkip
Ancient Egyptian
Crow
Imp
Malay
Celestial
Chinese Romanized
Cherokee Romanized
Vietnamese
         */
        public static readonly LanguageGen LOVECRAFT = new LanguageGen(
                new string[] { "a", "i", "o", "e", "u", "a", "i", "o", "e", "u", "ia", "ai", "aa", "ei" },
                Array.Empty<string>(),
                new string[] { "s", "t", "k", "n", "y", "p", "k", "l", "g", "gl", "th", "sh", "ny", "ft", "hm", "zvr", "cth" },
                new string[] { "h", "gl", "gr", "nd", "mr", "vr", "kr" },
                new string[] { "l", "p", "s", "t", "n", "k", "g", "x", "rl", "th", "gg", "gh", "ts", "lt", "rk", "kh", "sh", "ng", "shk" },
                new string[] { "aghn", "ulhu", "urath", "oigor", "alos", "'yeh", "achtal", "elt", "ikhet", "adzek", "agd" },
                new string[] { "'", "-" }, new int[] { 1, 2, 3, 4 }, new double[] { 5, 7, 3, 2 },
                0.4, 0.31, 0.07, 0.04, null, true).Register("Lovecraft");

        public static readonly LanguageGen ENGLISH = new LanguageGen(
                new string[]{
                        "a", "a", "a", "a", "o", "o", "o", "e", "e", "e", "e", "e", "i", "i", "i", "i", "u",
                        "a", "a", "a", "a", "o", "o", "o", "e", "e", "e", "e", "e", "i", "i", "i", "i", "u",
                        "a", "a", "a", "o", "o", "e", "e", "e", "i", "i", "i", "u",
                        "a", "a", "a", "o", "o", "e", "e", "e", "i", "i", "i", "u",
                        "au", "ai", "ai", "ou", "ea", "ie", "io", "ei",
                },
                new string[] { "u", "u", "oa", "oo", "oo", "oo", "ee", "ee", "ee", "ee", },
                new string[]{
                        "b", "bl", "br", "c", "cl", "cr", "ch", "d", "dr", "f", "fl", "fr", "g", "gl", "gr", "h", "j", "k", "l", "m", "n",
                        "p", "pl", "pr", "qu", "r", "s", "sh", "sk", "st", "sp", "sl", "sm", "sn", "t", "tr", "th", "thr", "v", "w", "y", "z",
                        "b", "bl", "br", "c", "cl", "cr", "ch", "d", "dr", "f", "fl", "fr", "g", "gr", "h", "j", "k", "l", "m", "n",
                        "p", "pl", "pr", "r", "s", "sh", "st", "sp", "sl", "t", "tr", "th", "w", "y",
                        "b", "br", "c", "ch", "d", "dr", "f", "g", "h", "j", "l", "m", "n",
                        "p", "r", "s", "sh", "st", "sl", "t", "tr", "th",
                        "b", "d", "f", "g", "h", "l", "m", "n",
                        "p", "r", "s", "sh", "t", "th",
                        "b", "d", "f", "g", "h", "l", "m", "n",
                        "p", "r", "s", "sh", "t", "th",
                        "r", "s", "t", "l", "n",
                        "str", "spr", "spl", "wr", "kn", "kn", "gn",
                },
                new string[]{"x", "cst", "bs", "ff", "lg", "g", "gs",
                        "ll", "ltr", "mb", "mn", "mm", "ng", "ng", "ngl", "nt", "ns", "nn", "ps", "mbl", "mpr",
                        "pp", "ppl", "ppr", "rr", "rr", "rr", "rl", "rtn", "ngr", "ss", "sc", "rst", "tt", "tt", "ts", "ltr", "zz"
                },
                new string[]{"b", "rb", "bb", "c", "rc", "ld", "d", "ds", "dd", "f", "ff", "lf", "rf", "rg", "gs", "ch", "lch", "rch", "tch",
                        "ck", "ck", "lk", "rk", "l", "ll", "lm", "m", "rm", "mp", "n", "nk", "nch", "nd", "ng", "ng", "nt", "ns", "lp", "rp",
                        "p", "r", "rn", "rts", "s", "s", "s", "s", "ss", "ss", "st", "ls", "t", "t", "ts", "w", "wn", "x", "ly", "lly", "z",
                        "b", "c", "d", "f", "g", "k", "l", "m", "n", "p", "r", "s", "t", "w",
                },
                new string[]{"ate", "ite", "ism", "ist", "er", "er", "er", "ed", "ed", "ed", "es", "es", "ied", "y", "y", "y", "y",
                        "ate", "ite", "ism", "ist", "er", "er", "er", "ed", "ed", "ed", "es", "es", "ied", "y", "y", "y", "y",
                        "ate", "ite", "ism", "ist", "er", "er", "er", "ed", "ed", "ed", "es", "es", "ied", "y", "y", "y", "y",
                        "ay", "ay", "ey", "oy", "ay", "ay", "ey", "oy",
                        "ough", "aught", "ant", "ont", "oe", "ance", "ell", "eal", "oa", "urt", "ut", "iom", "ion", "ion", "ision", "ation", "ation", "ition",
                        "ough", "aught", "ant", "ont", "oe", "ance", "ell", "eal", "oa", "urt", "ut", "iom", "ion", "ion", "ision", "ation", "ation", "ition",
                        "ily", "ily", "ily", "adly", "owly", "oorly", "ardly", "iedly",
                },
                Array.Empty<string>(), new int[] { 1, 2, 3, 4 }, new double[] { 10, 11, 4, 1 },
                0.22, 0.1, 0.0, 0.22, EnglishSanityChecks, true).Register("English");

        public static readonly LanguageGen GREEK_ROMANIZED = new LanguageGen(
                new String[]{"a", "a", "a", "a", "a", "o", "o", "e", "e", "e", "i", "i", "i", "i", "i", "au", "ai", "ai", "oi", "oi",
                        "ia", "io", "u", "u", "eo", "ei", "o", "o", "ou", "oi", "y", "y", "y", "y"},
                new String[] { "ui", "ui", "ei", "ei" },
                new String[]{"rh", "s", "z", "t", "t", "k", "ch", "n", "th", "kth", "m", "p", "ps", "b", "l", "kr",
                        "g", "phth", "d", "t", "k", "ch", "n", "ph", "ph", "k",},
                new String[] { "lph", "pl", "l", "l", "kr", "nch", "nx", "ps" },
                new String[]{"s", "p", "t", "ch", "n", "m", "s", "p", "t", "ch", "n", "m", "b", "g", "st", "rst",
                        "rt", "sp", "rk", "ph", "x", "z", "nk", "ng", "th", "d", "k", "n", "n",},
                new String[]{"os", "os", "os", "is", "is", "us", "um", "eum", "ium", "iam", "us", "um", "es",
                        "anes", "eros", "or", "or", "ophon", "on", "on", "ikon", "otron", "ik",},
                Array.Empty<string>(), new int[] { 1, 2, 3, 4 }, new double[] { 5, 7, 4, 1 }, 0.45, 0.45, 0.0, 0.2, null, true).Register("Greek Romanized");

        public static readonly LanguageGen GREEK_AUTHENTIC = new LanguageGen(
            new String[]{"α", "α", "α", "α", "α", "ο", "ο", "ε", "ε", "ε", "ι", "ι", "ι", "ι", "ι", "αυ", "αι", "αι", "οι", "οι",
                    "ια", "ιο", "ου", "ου", "εο", "ει", "ω", "ω", "ωυ", "ωι", "υ", "υ", "υ", "υ"},
            new String[] { "υι", "υι", "ει", "ει" },
            new String[]{"ρ", "σ", "ζ", "τ", "τ", "κ", "χ", "ν", "θ", "κθ", "μ", "π", "ψ", "β", "λ", "κρ",
                    "γ", "φθ", "δ", "τ", "κ", "χ", "ν", "φ", "φ", "κ",},
            new String[] { "λφ", "πλ", "λ", "λ", "κρ", "γχ", "γξ", "ψ" },
            new String[]{"σ", "π", "τ", "χ", "ν", "μ", "σ", "π", "τ", "χ", "ν", "μ", "β", "γ", "στ", "ρστ",
                    "ρτ", "σπ", "ρκ", "φ", "ξ", "ζ", "γκ", "γγ", "θ", "δ", "κ", "ν", "ν",},
            new String[]{"ος", "ος", "ος", "ις", "ις", "υς", "υμ", "ευμ", "ιυμ", "ιαμ", "υς", "υμ", "ες",
                    "ανες", "ερος", "ορ", "ορ", "οφον", "ον", "ον", "ικον", "οτρον", "ικ",},
            Array.Empty<string>(), new int[] { 1, 2, 3, 4 }, new double[] { 5, 7, 4, 1 }, 0.45, 0.45, 0.0, 0.2, null, true).Register("Greek Authentic");

        public static readonly LanguageGen FRENCH = new LanguageGen(
                new String[]{"a", "a", "a", "e", "e", "e", "i", "i", "o", "u", "a", "a", "a", "e", "e", "e", "i", "i", "o",
                        "a", "a", "a", "e", "e", "e", "i", "i", "o", "u", "a", "a", "a", "e", "e", "e", "i", "i", "o",
                        "a", "a", "e", "e", "i", "o", "a", "a", "a", "e", "e", "e", "i", "i", "o",
                        "ai", "oi", "oui", "au", "œu", "ou"
                },
                new String[]{
                        "ai", "aie", "aou", "eau", "oi", "oui", "oie", "eu", "eu",
                        "à", "â", "ai", "aî", "aï", "aie", "aou", "aoû", "au", "ay", "e", "é", "ée", "è",
                        "ê", "eau", "ei", "eî", "eu", "eû", "i", "î", "ï", "o", "ô", "oe", "oê", "oë", "œu",
                        "oi", "oie", "oï", "ou", "oû", "oy", "u", "û", "ue",
                        "a", "a", "a", "e", "e", "e", "i", "i", "o", "u", "a", "a", "a", "e", "e", "e", "i", "i", "o",
                        "a", "a", "e", "e", "i", "o", "a", "a", "a", "e", "e", "e", "i", "i", "o",
                        "a", "a", "a", "e", "e", "e", "i", "i", "o", "u", "a", "a", "a", "e", "e", "e", "i", "i", "o",
                        "a", "a", "e", "e", "i", "o", "a", "a", "a", "e", "e", "e", "i", "i", "o",
                        "ai", "ai", "eau", "oi", "oi", "oui", "eu", "au", "au", "ei", "ei", "oe", "oe", "ou", "ou", "ue"
                },
                new String[]{"tr", "ch", "m", "b", "b", "br", "j", "j", "j", "j", "g", "t", "t", "t", "c", "d", "f", "f", "h", "n", "l", "l",
                        "s", "s", "s", "r", "r", "r", "v", "v", "p", "pl", "pr", "bl", "br", "dr", "gl", "gr"},
                new String[]{"cqu", "gu", "qu", "rqu", "nt", "ng", "ngu", "mb", "ll", "nd", "ndr", "nct", "st",
                        "xt", "mbr", "pl", "g", "gg", "ggr", "gl", "bl", "j", "gn",
                        "m", "m", "mm", "v", "v", "f", "f", "f", "ff", "b", "b", "bb", "d", "d", "dd", "s", "s", "s", "ss", "ss", "ss",
                        "cl", "cr", "ng", "ç", "ç", "rç", "rd", "lg", "rg"},
                new String[]{"rt", "ch", "m", "b", "b", "lb", "t", "t", "t", "t", "c", "d", "f", "f", "n", "n", "l", "l",
                        "s", "s", "s", "r", "r", "p", "rd", "ff", "ss", "ll"
                },
                new String[]{"e", "e", "e", "e", "e", "é", "é", "er", "er", "er", "er", "er", "es", "es", "es", "es", "es", "es",
                        "e", "e", "e", "e", "e", "é", "é", "er", "er", "er", "er", "er", "er", "es", "es", "es", "es", "es",
                        "e", "e", "e", "e", "e", "é", "é", "é", "er", "er", "er", "er", "er", "es", "es", "es", "es", "es",
                        "ent", "em", "en", "en", "aim", "ain", "an", "oin", "ien", "iere", "ors", "anse",
                        "ombs", "ommes", "ancs", "ends", "œufs", "erfs", "ongs", "aps", "ats", "ives", "ui", "illes",
                        "aen", "aon", "am", "an", "eun", "ein", "age", "age", "uile", "uin", "um", "un", "un", "un",
                        "aille", "ouille", "eille", "ille", "eur", "it", "ot", "oi", "oi", "oi", "aire", "om", "on", "on",
                        "im", "in", "in", "ien", "ien", "ine", "ion", "il", "eil", "oin", "oint", "iguïté", "ience", "incte",
                        "ang", "ong", "acré", "eau", "ouche", "oux", "oux", "ect", "ecri", "agne", "uer", "aix", "eth", "ut", "ant",
                        "anc", "anc", "anche", "ioche", "eaux", "ive", "eur", "ancois", "ecois", "ente", "enri",
                        "arc", "oc", "ouis", "arche", "ique", "ique", "ique", "oque", "arque", "uis", "este", "oir", "oir"
                },
                Array.Empty<string>(), new int[] { 1, 2, 3 }, new double[] { 15, 7, 2 }, 0.35, 1.0, 0.0, 0.4, null, true).Register("French");

        public static readonly LanguageGen RUSSIAN_ROMANIZED = new LanguageGen(
                new String[] { "a", "e", "e", "i", "i", "o", "u", "ie", "y", "e", "iu", "ia", "y", "a", "a", "o", "u" },
                Array.Empty<string>(),
                new String[]{"b", "v", "g", "d", "k", "l", "p", "r", "s", "t", "f", "kh", "ts",
                        "b", "v", "g", "d", "k", "l", "p", "r", "s", "t", "f", "kh", "ts",
                        "b", "v", "g", "d", "k", "l", "p", "r", "s", "t", "f",
                        "zh", "m", "n", "z", "ch", "sh", "shch",
                        "br", "sk", "tr", "bl", "gl", "kr", "gr"},
                new String[] { "bl", "br", "pl", "dzh", "tr", "gl", "gr", "kr" },
                new String[]{"b", "v", "g", "d", "zh", "z", "k", "l", "m", "n", "p", "r", "s", "t", "f", "kh", "ts", "ch", "sh",
                        "v", "f", "sk", "sk", "sk", "s", "b", "d", "d", "n", "r", "r"},
                new String[] { "odka", "odna", "usk", "ask", "usky", "ad", "ar", "ovich", "ev", "ov", "of", "agda", "etsky", "ich", "on", "akh", "iev", "ian" },
                Array.Empty<string>(), new int[] { 1, 2, 3, 4, 5, 6 }, new double[] { 4, 5, 6, 5, 3, 1 }, 0.1, 0.2, 0.0, 0.12, null, true).Register("Russian Romanized");

        public static readonly LanguageGen RUSSIAN_AUTHENTIC = new LanguageGen(
                new String[] { "а", "е", "ё", "и", "й", "о", "у", "ъ", "ы", "э", "ю", "я", "ы", "а", "а", "о", "у" },
                Array.Empty<string>(),
                new String[]{"б", "в", "г", "д", "к", "л", "п", "р", "с", "т", "ф", "х", "ц",
                        "б", "в", "г", "д", "к", "л", "п", "р", "с", "т", "ф", "х", "ц",
                        "б", "в", "г", "д", "к", "л", "п", "р", "с", "т", "ф",
                        "ж", "м", "н", "з", "ч", "ш", "щ",
                        "бр", "ск", "тр", "бл", "гл", "кр", "гр"},
                new String[] { "бл", "бр", "пл", "дж", "тр", "гл", "гр", "кр" },
                new String[]{"б", "в", "г", "д", "ж", "з", "к", "л", "м", "н", "п", "р", "с", "т", "ф", "х", "ц", "ч", "ш",
                        "в", "ф", "ск", "ск", "ск", "с", "б", "д", "д", "н", "р", "р"},
                new String[] { "одка", "одна", "уск", "аск", "ускы", "ад", "ар", "овйч", "ев", "ов", "оф", "агда", "ёцкы", "йч", "он", "ах", "ъв", "ян" },
                Array.Empty<string>(), new int[] { 1, 2, 3, 4, 5, 6 }, new double[] { 4, 5, 6, 5, 3, 1 }, 0.1, 0.2, 0.0, 0.12, null, true).Register("Russian Authentic");

        public static readonly LanguageGen JAPANESE_ROMANIZED = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Japanese Romanized");

        public static readonly LanguageGen SWAHILI = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Swahili");

        public static readonly LanguageGen SOMALI = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Somali");

        public static readonly LanguageGen HINDI_ROMANIZED = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Hindi Romanized");

        public static readonly LanguageGen ARABIC_ROMANIZED = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Arabic Romanized");

        public static readonly LanguageGen INUKTITUT = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Inuktitut");

        public static readonly LanguageGen NORSE = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Norse");

        public static readonly LanguageGen NAHUATL = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Nahuatl");

        public static readonly LanguageGen MONGOLIAN = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Mongolian");

        public static readonly LanguageGen FANTASY = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Fantasy");

        public static readonly LanguageGen FANCY_FANTASY = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Fancy Fantasy");

        public static readonly LanguageGen GOBLIN = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Goblin");

        public static readonly LanguageGen ELF = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Elf");

        public static readonly LanguageGen DEMONIC = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Demonic");

        public static readonly LanguageGen INFERNAL = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Infernal");

        public static readonly LanguageGen SIMPLISH = new LanguageGen(
                new string[]{
                        "a", "a", "a", "a", "o", "o", "o", "e", "e", "e", "e", "e", "i", "i", "i", "i", "u",
                        "a", "a", "a", "a", "o", "o", "o", "e", "e", "e", "e", "e", "i", "i", "i", "i", "u",
                        "a", "a", "a", "a", "o", "o", "o", "e", "e", "e", "e", "e", "i", "i", "i", "i", "u",
                        "a", "a", "a", "o", "o", "e", "e", "e", "i", "i", "i", "u",
                        "a", "a", "a", "o", "o", "e", "e", "e", "i", "i", "i", "u",
                        "ai", "ai", "ea", "io", "oi", "ia", "io", "eo"
                },
                new string[] { "u", "u", "oa" },
                new string[]{
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
                new string[]{"ch", "j", "w", "y", "v", "w", "y", "w", "y", "ch",
                        "b", "c", "d", "f", "g", "k", "l", "m", "n", "p", "r", "s", "sh", "t",
                },
                new string[]{"bs", "lt", "mb", "ng", "ng", "nt", "ns", "ps", "mp", "rt", "rg", "sk", "rs", "ts", "lk", "ct",
                        "b", "c", "d", "f", "g", "k", "l", "m", "n", "p", "r", "s", "sh", "t", "th", "z",
                        "b", "c", "d", "f", "g", "k", "l", "m", "n", "p", "r", "s", "sh", "t",
                        "b", "c", "d", "f", "g", "k", "l", "m", "n", "p", "r", "s", "sh", "t",
                        "d", "f", "g", "k", "l", "m", "n", "p", "r", "s", "sh", "t",
                        "d", "f", "g", "k", "l", "m", "n", "p", "r", "s", "sh", "t",
                        "d", "f", "g", "k", "l", "m", "n", "p", "r", "s", "sh", "t",
                        "d", "f", "g", "k", "l", "m", "n", "p", "r", "s", "sh", "t",
                },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2, 3, 4 }, new double[] { 7, 18, 6, 1 }, 0.26, 0.12, 0.0, 0.0, GenericSanityChecks, true).Register("Simplish");

        public static readonly LanguageGen ALIEN_A = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Alien A");

        public static readonly LanguageGen KOREAN_ROMANIZED = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Korean Romanized");

        public static readonly LanguageGen ALIEN_E = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Alien E");

        public static readonly LanguageGen ALIEN_I = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Alien I");

        public static readonly LanguageGen ALIEN_O = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Alien O");

        public static readonly LanguageGen ALIEN_U = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Alien U");

        public static readonly LanguageGen DRAGON = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Dragon");

        public static readonly LanguageGen KOBOLD = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Kobold");

        public static readonly LanguageGen INSECT = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Insect");

        public static readonly LanguageGen MAORI = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Maori");

        public static readonly LanguageGen SPANISH = new LanguageGen(
                new string[] { "a", "a", "a", "a", "a", "i", "i", "i", "o", "o", "o", "e", "e", "e", "e", "e", "u", "u" },
                new string[] { "a", "a", "a", "i", "i", "i", "i", "o", "o", "o", "o", "o", "e", "e", "e", "e",
                        "a", "a", "a", "a", "a", "a", "i", "i", "i", "i", "o", "o", "o", "o", "o", "e", "e", "e", "e", "e",
                        "a", "a", "a", "a", "a", "a", "i", "i", "i", "i", "o", "o", "o", "o", "o", "e", "e", "e", "e", "e",
                        "a", "a", "a", "a", "a", "a", "i", "i", "i", "i", "o", "o", "o", "o", "o", "e", "e", "e", "e", "e",
                        "a", "a", "a", "a", "a", "a", "i", "i", "i", "i", "o", "o", "o", "o", "o", "e", "e", "e", "e", "e",
                        "a", "a", "a", "a", "a", "a", "i", "i", "i", "i", "o", "o", "o", "o", "o", "e", "e", "e", "e", "e",
                        "a", "a", "a", "a", "a", "a", "i", "i", "i", "i", "o", "o", "o", "o", "o", "e", "e", "e", "e", "e",
                        "ai", "ai", "eo", "ia", "ia", "ie", "io", "iu", "oi", "ui", "ue", "ua",
                        "ai", "ai", "eo", "ia", "ia", "ie", "io", "iu", "oi", "ui", "ue", "ua",
                        "ai", "ai", "eo", "ia", "ia", "ie", "io", "iu", "oi", "ui", "ue", "ua",
                        "ái", "aí", "éo", "ía", "iá", "íe", "ié", "ío", "íu", "oí", "uí", "ué", "uá",
                        "á", "é", "í", "ó", "ú", "á", "é", "í", "ó",},
                new string[] { "b", "c", "ch", "d", "f", "g", "gu", "h", "j", "l", "m", "n", "p", "qu", "r", "s", "t", "v", "z",
                        "b", "s", "z", "r", "n", "h", "j", "j", "s", "c", "r",
                        "b", "s", "z", "r", "n", "h", "j", "s", "c", "r",
                        "b", "s", "r", "n", "h", "j", "s", "c", "r",
                        "n", "s", "l", "c", "n", "s", "l", "c",
                        "br", "gr", "fr"
                },
                new string[] { "ñ", "rr", "ll", "ñ", "rr", "ll", "mb", "nd", "ng", "nqu", "rqu", "zqu", "zc", "rd", "rb", "rt", "rt", "rc", "sm", "sd" },
                new string[] { "r", "n", "s", "s", "r", "n", "s", "s", "r", "n", "s", "s", "r", "n", "s", "s",
                        "r", "n", "s", "r", "n", "s", "r", "n", "s", "r", "n", "s",
                },
                new string[]{"on", "ez", "es", "es", "es", "es", "es",
                        "ador", "edor", "ando", "endo", "indo",
                        "ar", "as", "amos", "an", "oy", "ay",
                        "er", "es", "emos", "en", "e",
                        "ir", "es", "imos", "en", "io",
                        "o", "a", "o", "a", "o", "a", "o", "a", "os", "as", "os", "as", "os", "as"
                },
                Array.Empty<string>(), new int[] { 1, 2, 3, 4 }, new double[] { 4, 5, 3, 1 }, 0.1, 1.0, 0.0, 0.3, GenericSanityChecks, true)
                .AddModifiers(
                        new Modifier("([aeouáéóú])i$", "$1y"),
                        new Modifier("([qQ])ua", "$1ue"), // guapo, agua, guano, all real Spanish, we should allow gua
                        new Modifier("([qQ])uá", "$1ué"),
                        new Modifier("([qgQG])u[ouy]", "$1ui"),
                        new Modifier("([qgQG])u[óú]", "$1uí")).Register("Spanish");

        public static readonly LanguageGen DEEP_SPEECH = new LanguageGen(
                new string[]{
                        "a", "a", "o", "o", "o", "o", "u", "u", "u", "u",
                        "a", "a", "o", "o", "o", "o", "u", "u", "u", "u",
                        "a", "a", "o", "o", "o", "o", "u", "u", "u", "u",
                        "a", "a", "o", "o", "o", "o", "u", "u", "u", "u",
                        "a", "a", "o", "o", "o", "o", "u", "u", "u", "u",
                        "aa", "aa", "oo", "oo", "oo", "oo", "uu", "uu", "uu", "uu",
                        "aa", "aa", "oo", "oo", "oo", "oo", "uu", "uu", "uu", "uu",
                        "ah", "ah", "oh", "oh", "oh", "oh", "uh", "uh", "uh", "uh",
                        "aah", "ooh", "ooh", "uuh", "uuh",
                },
                Array.Empty<string>(),
                new string[]{
                        "m", "ng", "r", "x", "y", "z", "v", "l",
                        "m", "ng", "r", "x", "y", "z", "v", "l",
                        "m", "ng", "r", "x", "y", "z", "v", "l",
                        "m", "ng", "r", "x", "y", "z", "v", "l",
                        "m", "ng", "r", "x", "y", "z", "v", "l",
                        "m", "ng", "r", "z", "l",
                        "m", "ng", "r", "z", "l",
                        "m", "ng", "r", "z", "l",
                        "m", "ng", "r", "z", "l",
                        "mr", "vr", "ry", "zr",
                        "mw", "vw", "ly", "zw",
                        "zl", "vl"
                },
                Array.Empty<string>(),
                new string[]{
                        "m", "ng", "r", "x", "z", "v", "l",
                        "m", "ng", "r", "x", "z", "v", "l",
                        "m", "ng", "r", "x", "z", "v", "l",
                        "m", "ng", "r", "x", "z", "v", "l",
                        "rm", "rng", "rx", "rz", "rv", "rl",
                        "lm", "lx", "lz", "lv",
                },
                Array.Empty<string>(),
                new string[] { "'" }, new int[] { 1, 2, 3, 4 }, new double[] { 3, 6, 5, 1 }, 0.18, 0.25, 0.07, 0.0, null, true).Register("Deep Speech");

        public static readonly LanguageGen NORSE_SIMPLIFIED = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Norse Simplified");

        public static readonly LanguageGen HLETKIP = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Hletkip");

        public static readonly LanguageGen ANCIENT_EGYPTIAN = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Ancient Egyptian");

        public static readonly LanguageGen CROW = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Crow");

        public static readonly LanguageGen IMP = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Imp");

        public static readonly LanguageGen MALAY = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Malay");

        public static readonly LanguageGen CELESTIAL = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Celestial");

        public static readonly LanguageGen CHINESE_ROMANIZED = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Chinese Romanized");

        public static readonly LanguageGen CHEROKEE_ROMANIZED = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Cherokee Romanized");

        public static readonly LanguageGen VIETNAMESE = new LanguageGen(
                new string[] { "u" },
                new string[] { "au" },
                new string[] { "b" },
                new string[] { "b", "lb" },
                new string[] { "lb" },
                Array.Empty<string>(),
                Array.Empty<string>(), new int[] { 1, 2 }, new double[] { 5, 3 }, 0.45, 0.45, 0.0, 0.0, null, true).Register("Vietnamese");

        #endregion LANGUAGES
    }
}
