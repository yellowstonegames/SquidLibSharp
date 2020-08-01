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
        private static readonly char[][] accentedVowels = new char[][]{
            new char[]{'a', 'à', 'á', 'â', 'ä', 'ā', 'ă', 'ã', 'å', 'ą', 'ǻ'},
            new char[]{'e', 'è', 'é', 'ê', 'ë', 'ē', 'ĕ', 'ė', 'ę', 'ě'},
            new char[]{'i', 'ì', 'í', 'î', 'ï', 'ī', 'ĭ', 'ĩ', 'į', 'ı',},
            new char[]{'o', 'ò', 'ó', 'ô', 'ö', 'ō', 'ŏ', 'õ', 'ø', 'ő', 'ǿ'},
            new char[]{'u', 'ù', 'ú', 'û', 'ü', 'ū', 'ŭ', 'ũ', 'ů', 'ű', 'ų'}
    },
        accentedConsonants = new char[][]
                {
                            new char[]{
                                    'b'
                            },
                            new char[]{
                                    'c', 'ç', 'ć', 'ĉ', 'ċ', 'č',
                            },
                            new char[]{
                                    'd', 'þ', 'ð', 'ď', 'đ',
                            },
                            new char[]{
                                    'f'
                            },
                            new char[]{
                                    'g', 'ĝ', 'ğ', 'ġ', 'ģ',
                            },
                            new char[]{
                                    'h', 'ĥ', 'ħ',
                            },
                            new char[]{
                                    'j', 'ĵ', 'ȷ',
                            },
                            new char[]{
                                    'k', 'ķ',
                            },
                            new char[]{
                                    'l', 'ĺ', 'ļ', 'ľ', 'ŀ', 'ł',
                            },
                            new char[]{
                                    'm',
                            },
                            new char[]{
                                    'n', 'ñ', 'ń', 'ņ', 'ň', 'ŋ',
                            },
                            new char[]{
                                    'p',
                            },
                            new char[]{
                                    'q',
                            },
                            new char[]{
                                    'r', 'ŕ', 'ŗ', 'ř',
                            },
                            new char[]{
                                    's', 'ś', 'ŝ', 'ş', 'š', 'ș',
                            },
                            new char[]{
                                    't', 'ţ', 'ť', 'ț',
                            },
                            new char[]{
                                    'v',
                            },
                            new char[]{
                                    'w', 'ŵ', 'ẁ', 'ẃ', 'ẅ',
                            },
                            new char[]{
                                    'x',
                            },
                            new char[]{
                                    'y', 'ý', 'ÿ', 'ŷ', 'ỳ',
                            },
                            new char[]{
                                    'z', 'ź', 'ż', 'ž',
                            },
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
            JapaneseSanityChecks = new Regex[]
                    {
                            new Regex("[AEIOUaeiou]{3}", RegexOptions.IgnoreCase),
                            new Regex("(\\w)\\1\\1", RegexOptions.IgnoreCase),
                            new Regex("[Tt]s[^u]", RegexOptions.IgnoreCase),
                            new Regex("[Ff][^u]", RegexOptions.IgnoreCase),
                            new Regex("[Yy][^auo]", RegexOptions.IgnoreCase),
                            new Regex("[Tt][ui]", RegexOptions.IgnoreCase),
                            new Regex("[SsZzDd]i", RegexOptions.IgnoreCase),
                            new Regex("[Hh]u", RegexOptions.IgnoreCase),
                    },
            ArabicSanityChecks = new Regex[]
                    {
                            new Regex("(.)\\1\\1", RegexOptions.IgnoreCase),
                            new Regex("-[^aeiou](?:[^aeiou]|$)", RegexOptions.IgnoreCase),
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
    * A pattern string that will match any vowel LanguageGen can produce out-of-the-box, including Latin, Greek,
    * and Cyrillic; for use when a string will be interpreted as a regex (as in {@link LanguageGen.Alteration}).
*/
        public static readonly string AnyVowel = "[àáâãäåæāăąǻǽaèéêëēĕėęěeìíîïĩīĭįıiòóôõöøōŏőœǿoùúûüũūŭůűųuýÿŷỳyαοειυωаеёийоуъыэюя]",
        /**
         * A pattern string that will match one or more of any vowels LanguageGen can produce out-of-the-box, including
         * Latin, Greek, and Cyrillic; for use when a string will be interpreted as a regex (as in 
         * {@link LanguageGen.Alteration}).
         */
        AnyVowelCluster = AnyVowel + '+',
        /**
         * A pattern string that will match any consonant LanguageGen can produce out-of-the-box, including Latin,
         * Greek, and Cyrillic; for use when a string will be interpreted as a regex (as in
         * {@link LanguageGen.Alteration}).
         */
        AnyConsonant = "[bcçćĉċčdþðďđfgĝğġģhĥħjĵȷkķlĺļľŀłmnñńņňŋpqrŕŗřsśŝşšștţťțvwŵẁẃẅxyýÿŷỳzźżžρσζτκχνθμπψβλγφξςбвгдклпрстфхцжмнзчшщ]",
        /**
         * A pattern string that will match one or more of any consonants LanguageGen can produce out-of-the-box,
         * including Latin, Greek, and Cyrillic; for use when a string will be interpreted as a regex (as in
         * {@link LanguageGen.Alteration}).
         */
        AnyConsonantCluster = AnyConsonant + '+';

        protected static readonly Regex repeats = new Regex("(.)\\1+", RegexOptions.IgnoreCase),
                vowelClusters = new Regex(AnyVowelCluster, RegexOptions.IgnoreCase),
                consonantClusters = new Regex(AnyConsonantCluster, RegexOptions.IgnoreCase);

        private static readonly string[] mid = new string[] { ",", ",", ",", ";" }, end = new string[] { ".", ".", ".", ".", "!", "?", "..." };

        private static readonly IndexedDictionary<string, string> openVowels = new IndexedDictionary<string, string>() {
        {"a", "a aa ae ai au ea ia oa ua" },
        {"e", "e ae ea ee ei eo eu ie ue" },
        {"i", "i ai ei ia ie io iu oi ui" },
        {"o", "o eo io oa oi oo ou" },
        {"u", "u au eu iu ou ua ue ui" }
        };

        private static readonly IndexedDictionary<string, string> openCons = new IndexedDictionary<string, string>() {
{"b", "b bl br by bw bh"},
{"bh", "bh"},
{"c", "c cl cr cz cth sc scl"},
{"ch", "ch ch chw"},
{"d", "d dr dz dy dw dh"},
{"dh", "dh"},
{"f", "f fl fr fy fw sf"},
{"g", "g gl gr gw gy gn"},
{"h", "bh cth ch ch chw dh h hm hy hw kh khl khw ph phl phr sh shl shqu shk shp shm shn shr shw shpl th th thr thl thw"},
{"j", "j j"},
{"k", "k kr kl ky kn sk skl shk"},
{"kh", "kh khl khw"},
{"l", "bl cl fl gl kl khl l pl phl scl skl spl sl shl shpl tl thl vl zl"},
{"m", "hm m mr mw my sm smr shm"},
{"n", "gn kn n nw ny pn sn shn"},
{"p", "p pl pr py pw pn sp spr spl shp shpl ph phl phr"},
{"ph", "ph phl phr"},
{"q", "q"},
{"qu", "qu squ shqu"},
{"r", "br cr dr fr gr kr mr pr phr r str spr smr shr tr thr vr wr zvr"},
{"s", "s sc scl sf sk skl st str sp spr spl sl sm smr sn sw sy squ ts sh shl shqu shk shp shm shn shr shw shpl"},
{"sh", "sh shl shqu shk shp shm shn shr shw shpl"},
{"t", "st str t ts tr tl ty tw tl"},
{"th", "cth th thr thl thw"},
{"tl", "tl"},
{"v", "v vr vy zv zvr vl"},
{"w", "bw chw dw fw gw hw khw mw nw pw sw shw tw thw w wr zw"},
{"x", "x"},
{"y", "by dy fy gy hy ky my ny py sy ty vy y zy"},
{"z", "cz dz z zv zvr zl zy zw"}
        };

        private static readonly IndexedDictionary<string, string> midCons = new IndexedDictionary<string, string>() {
{"b", "lb rb bj bl br lbr rbl skbr scbr zb bq bdh dbh bbh lbh rbh bb"},
{"bh", "bbh dbh lbh rbh"},
{"c", "lc lsc rc rsc cl cqu cr ct lcr rcl sctr scdr scbr scpr msc mscr nsc nscr ngscr ndscr cc"},
{"ch", "lch rch rch"},
{"d", "ld ld rd rd skdr scdr dr dr dr rdr ldr zd zdr ndr ndscr ndskr ndst dq ldh rdh dbh bdh ddh dd"},
{"dh", "bdh ddh ldh rdh"},
{"f", "lf rf fl fr fl fr fl fr lfr rfl ft ff"},
{"g", "lg lg rg rg gl gr gl gr gl gr lgr rgl zg zgr ngr ngl ngscr ngskr gq gg"},
{"h", "lch lph lth lsh rch rph rsh rth phl phr lphr rphl shl shr lshr rshl msh mshr zth bbh dbh lbh rbh bdh ddh ldh rdh"},
{"j", "bj lj rj"},
{"k", "lk lsk rk rsk kl kr lkr rkl sktr skdr skbr skpr tk zk zkr msk mskr nsk nskr ngskr ndskr kq kk"},
{"kh", "lkh rkh"},
{"l", "lb lc lch ld lf lg lj lk lm ln lp lph ls lst lt lth lsc lsk lsp lv lz lsh bl lbr rbl cl lcr rcl fl lfr rfl gl lgr rgl kl lkr rkl pl lpr rpl phl lphr rphl shl lshr rshl sl rsl lsl ldr ltr lx ngl nsl msl nsl ll lth tl ltl rtl vl"},
{"m", "lm rm zm msl msc mscr msh mshr mst msp msk mskr mm"},
{"n", "ln rn nx zn zn ndr nj ntr ntr ngr ngl nsl nsl nsc nscr ngscr ndscr nsk nskr ngskr ndskr nst ndst nsp nn"},
{"p", "lp lsp rp rsp pl pr lpr rpl skpr scpr zp msp nsp lph rph phl phr lphr rphl pq pp"},
{"ph", "lph lph rph rph phl phr lphr rphl"},
{"q", "bq dq gq kq pq tq"},
{"qu", "cqu lqu rqu"},
{"r", "rb rc rch rd rf rg rj rk rm rn rp rph rs rsh rst rt rth rsc rsk rsp rv rz br br br lbr rbl cr cr cr lcr rcl fr fr fr lfr rfl gr gr gr lgr rgl kr kr kr lkr rkl pr pr pr lpr rpl phr phr phr lphr rphl shr shr shr lshr rshl rsl sktr sctr skdr scdr skbr scbr skpr scpr dr dr dr rdr ldr tr tr tr rtr ltr vr rx zr zdr ztr zgr zkr ntr ntr ndr ngr mscr mshr mskr nscr ngscr ndscr nskr ngskr ndskr rr"},
{"s", "ls lst lsc lsk lsp rs rst rsc rsk rsp sl rsl lsl sktr sctr skdr scdr skbr scbr skpr scpr nsl msl msc mscr mst msp msk mskr nsl nsc nscr ngscr ndscr nsk nskr ngskr ndskr nst ndst nsp lsh rsh sh shl shqu shk shp shm shn shr shw shpl lshr rshl msh mshr ss"},
{"sh", "lsh rsh sh shl shqu shk shp shm shn shr shw shpl lshr rshl msh mshr"},
{"t", "ct ft lst lt rst rt sktr sctr tk tr rtr ltr zt ztr ntr ntr mst nst ndst tq ltl rtl tt"},
{"th", "lth rth zth cth"},
{"tl", "ltl rtl"},
{"v", "lv rv vv vl vr"},
{"w", "bw chw dw fw gw hw khw mw nw pw sw shw tw thw w wr wy zw"},
{"x", "nx rx lx"},
{"y", "by dy fy gy hy ky my ny py sy ty vy wy zy"},
{"z", "lz rz zn zd zt zg zk zm zn zp zb zr zdr ztr zgr zkr zth zz"}
        };

        private static readonly IndexedDictionary<string, string> closeCons = new IndexedDictionary<string, string>() {
{"b", "b lb rb bs bz mb mbs bh bh lbh rbh mbh bb"},
{"bh", "bh lbh rbh mbh"},
{"c", "c ck cks lc rc cs cz ct cz cth sc"},
{"ch", "ch lch rch tch pch kch mch nch"},
{"d", "d ld rd ds dz dt dsh dth gd nd nds dh dh ldh rdh ndh dd"},
{"dh", "dh ldh rdh ndh"},
{"f", "f lf rf fs fz ft fsh ft fth ff"},
{"g", "g lg rg gs gz gd gsh gth ng ngs gg"},
{"h", "cth ch lch rch tch pch kch mch nch dsh dth fsh fth gsh gth h hs ksh kth psh pth ph ph ph ph ph ph lph rph phs pht phth"},
{"j", "j"},
{"k", "ck cks kch k lk rk ks kz kt ksh kth nk nks sk"},
{"kh", "kh"},
{"l", "lb lc lch ld lf lg lk l ls lz lp lph ll"},
{"m", "mch m ms mb mt mp mbs mps mz sm mm"},
{"n", "nch n ns nd nt nk nds nks nz ng ngs nn"},
{"p", "pch mp mps p lp rp ps pz pt psh pth sp sp ph lph rph phs pht phth"},
{"ph", "ph lph rph phs pht phth"},
{"q", "q"},
{"qu", ""},
{"r", "rb rc rch rd rf rg rk rp rph r rs rz"},
{"s", "bs cks cs ds fs gs hs ks ls ms mbs mps ns nds nks ngs ps phs rs s st sp st sp sc sk sm ts lsh rsh sh shk shp msh ss"},
{"sh", "lsh rsh sh shk shp msh"},
{"t", "ct ft tch dt ft kt mt nt pt pht st st t ts tz tt"},
{"th", "cth dth fth gth kth pth phth th ths"},
{"tl", "tl"},
{"v", "v"},
{"w", ""},
{"x", "x"},
{"y", ""},
{"z", "bz cz dz fz gz kz lz mz nz pz rz tz z zz"}
        };


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

        public LanguageGen RemoveAccents() {

            string[] ov = ArrayTools.Copy(OpeningVowels),
                    mv = ArrayTools.Copy(MidVowels),
                    oc = ArrayTools.Copy(OpeningConsonants),
                    mc = ArrayTools.Copy(MidConsonants),
                    cc = ArrayTools.Copy(ClosingConsonants),
                    cs = ArrayTools.Copy(ClosingSyllables);
            for (int i = 0; i < ov.Length; i++) {
                ov[i] = RemoveAccents(OpeningVowels[i]);
            }
            for (int i = 0; i < mv.Length; i++) {
                mv[i] = RemoveAccents(MidVowels[i]);
            }
            for (int i = 0; i < oc.Length; i++) {
                oc[i] = RemoveAccents(OpeningConsonants[i]);
            }
            for (int i = 0; i < mc.Length; i++) {
                mc[i] = RemoveAccents(MidConsonants[i]);
            }
            for (int i = 0; i < cc.Length; i++) {
                cc[i] = RemoveAccents(ClosingConsonants[i]);
            }
            for (int i = 0; i < cs.Length; i++) {
                cs[i] = RemoveAccents(ClosingSyllables[i]);
            }
            int[] syllableCounts = new int[SyllableFrequencies.Length];
            double[] syllableFrequencies = ArrayTools.Copy(SyllableFrequencies);
            for (int i = 0; i < syllableCounts.Length; i++) {
                syllableCounts[i] = i + 1;
            }

            LanguageGen lang = new LanguageGen(ov, mv, oc, mc, cc, cs, VowelSplitters, syllableCounts, syllableFrequencies,
                    VowelStartFrequency,
                    VowelEndFrequency,
                    VowelSplitFrequency,
                    SyllableEndFrequency, SanityChecks, Clean);
            lang.Modifiers.AddRange(Modifiers);
            lang.Name = Name;
            return lang;
        }

        public LanguageGen Mix(double myWeight, params (LanguageGen lang, double weight)[] pairs) {
            if (pairs == null || pairs.Length == 0)
                return new LanguageGen(this);
            Modifier[] mods = Modifiers.ToArray();
            LanguageGen mixer = RemoveModifiers();
            LanguageGen[] languages = new LanguageGen[2 + pairs.Length];
            double[] weights = new double[languages.Length];
            string[] summaries = new string[languages.Length];
            double total = 0.0, current, weight;
            languages[0] = mixer;
            total += weights[0] = myWeight;
            for (int i = 0; i < pairs.Length; i++) {
                languages[i + 1] = pairs[i].lang.RemoveModifiers();
                total += weights[i + 1] = pairs[i].weight;
            }
            if (total == 0)
                return new LanguageGen(this);
            current = myWeight / total;
            for (int i = 1; i < languages.Length; i++) {
                if ((weight = weights[i]) > 0)
                    mixer = mixer.MixPair(languages[i], weight / total / (current += weight / total));
            }
            return mixer.AddModifiers(mods);
        }
        public LanguageGen MixPair(LanguageGen other, double otherInfluence) {
            if (other == null || otherInfluence <= 0.0)
                return new LanguageGen(this);
            otherInfluence = Math.Max(0.0, Math.Min(otherInfluence, 1.0));
            double myInfluence = 1.0 - otherInfluence;

            RNG rng = new RNG(SeededHash.Murmur.Hash64(Name), SeededHash.Murmur.Hash64(other.Name) ^ (ulong)BitConverter.DoubleToInt64Bits(otherInfluence));

            string[] ov = Merge1000(rng, OpeningVowels, other.OpeningVowels, otherInfluence),
                    mv = Merge1000(rng, MidVowels, other.MidVowels, otherInfluence),
                    oc = Merge1000(rng, OpeningConsonants, other.OpeningConsonants, otherInfluence *
                            Math.Max(0.0, Math.Min(1.0, 1.0 - other.VowelStartFrequency + VowelStartFrequency))),
                    mc = Merge1000(rng, MidConsonants, other.MidConsonants, otherInfluence),
                    cc = Merge1000(rng, ClosingConsonants, other.ClosingConsonants, otherInfluence *
                            Math.Max(0.0, Math.Min(1.0, 1.0 - other.VowelEndFrequency + VowelEndFrequency))),
                    cs = Merge1000(rng, ClosingSyllables, other.ClosingSyllables, otherInfluence *
                            Math.Max(0.0, Math.Min(1.0, other.SyllableEndFrequency - SyllableEndFrequency))),
                    splitters = Merge1000(rng, VowelSplitters, other.VowelSplitters, otherInfluence);

            double[] fr = new double[Math.Max(SyllableFrequencies.Length, other.SyllableFrequencies.Length)];
            int[] syllableCounts = new int[fr.Length];
            for (int i = 0; i < fr.Length; i++) {
                syllableCounts[i] = i + 1;
            }
            Array.Copy(SyllableFrequencies, 0, fr, 0, SyllableFrequencies.Length);
            for (int i = 0; i < other.SyllableFrequencies.Length; i++) {
                fr[i] += other.SyllableFrequencies[i];
            }

            LanguageGen lang = new LanguageGen(ov, mv, oc, mc, cc, cs, splitters, syllableCounts, fr,
                    VowelStartFrequency * myInfluence + other.VowelStartFrequency * otherInfluence,
                    VowelEndFrequency * myInfluence + other.VowelEndFrequency * otherInfluence,
                    VowelSplitFrequency * myInfluence + other.VowelSplitFrequency * otherInfluence,
                    SyllableEndFrequency * myInfluence + other.SyllableEndFrequency * otherInfluence,
                    (SanityChecks == null) ? other.SanityChecks : SanityChecks, true);
            lang.Modifiers.AddRange(Modifiers);
            lang.Modifiers.AddRange(other.Modifiers);
            lang.Name = (otherInfluence > 0.5 ? other.Name + "/" + Name : Name + "/" + other.Name);
            return lang;
        }
        private static string[] Merge1000(RNG rng, string[] me, string[] other, double otherInfluence) {
            if (other.Length <= 0 && me.Length <= 0)
                return Array.Empty<string>();
            string[] ret = new string[1000];
            int otherCount = (int)(1000 * otherInfluence);
            int idx = 0;
            if (other.Length > 0) {
                string[] tmp = new string[other.Length];
                rng.Shuffle(other, tmp);
                for (idx = 0; idx < otherCount; idx++) {
                    ret[idx] = tmp[idx % tmp.Length];
                }
            }
            if (me.Length > 0) {
                string[] tmp = new string[me.Length];
                rng.Shuffle(me, tmp);
                for (; idx < 1000; idx++) {
                    ret[idx] = tmp[idx % tmp.Length];
                }
            } else {
                for (; idx < 1000; idx++) {
                    ret[idx] = other[idx % other.Length];
                }
            }
            return ret;
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

        public LanguageGen AddModifiers(params Modifier[] modifiers) {
            LanguageGen cp = new LanguageGen(this);
            cp.Modifiers.AddRange(modifiers);
            return cp;
        }


        public LanguageGen RemoveModifiers() {
            LanguageGen cp = new LanguageGen(this);
            cp.Modifiers.Clear();
            return cp;
        }

        public LanguageGen AddAccents(double vowelInfluence, double consonantInfluence) {
            vowelInfluence = Math.Max(0.0, Math.Min(vowelInfluence, 1.0));
            consonantInfluence = Math.Max(0.0, Math.Min(consonantInfluence, 1.0));
            RNG rng = new RNG(SeededHash.Andrealphus.Hash64(Name), (ulong)BitConverter.DoubleToInt64Bits(vowelInfluence) ^ (ulong)BitConverter.DoubleToInt64Bits(consonantInfluence));
            string[] ov = AccentVowels(rng, OpeningVowels, vowelInfluence),
                    mv = AccentVowels(rng, MidVowels, vowelInfluence),
                    oc = AccentConsonants(rng, OpeningConsonants, consonantInfluence),
                    mc = AccentConsonants(rng, MidConsonants, consonantInfluence),
                    cc = AccentConsonants(rng, ClosingConsonants, consonantInfluence),
                    cs = AccentBoth(rng, ClosingSyllables, vowelInfluence, consonantInfluence);

            int[] syllableCounts = new int[SyllableFrequencies.Length];
            double[] syllableFrequencies = ArrayTools.Copy(SyllableFrequencies);
            for (int i = 0; i < syllableCounts.Length; i++) {
                syllableCounts[i] = i + 1;
            }

            LanguageGen lang = new LanguageGen(ov, mv, oc, mc, cc, cs, VowelSplitters, syllableCounts, syllableFrequencies,
                    VowelStartFrequency,
                    VowelEndFrequency,
                    VowelSplitFrequency,
                    SyllableEndFrequency, SanityChecks, Clean);
            lang.Modifiers.AddRange(Modifiers);
            lang.Name = Name + "-Bònüs";
            return lang;
        }

        private string[] AccentVowels(RNG rng, string[] me, double influence) {
            string[] ret = new string[1000];
            int otherCount = (int)(1000 * influence);
            int idx;
            if (me.Length > 0) {
                string[] tmp = new string[me.Length];
                rng.Shuffle(me, tmp);
                for (idx = 0; idx < otherCount; idx++) {
                    ret[idx] = tmp[idx % tmp.Length]
                        .Replace('a', rng.RandomElement(accentedVowels[0]))
                        .Replace('e', rng.RandomElement(accentedVowels[1]))
                        .Replace('i', rng.RandomElement(accentedVowels[2]))
                        .Replace('o', rng.RandomElement(accentedVowels[3]))
                        .Replace('u', rng.RandomElement(accentedVowels[4]));
                    ret[idx] = repeats.Replace(ret[idx], rng.RandomElement(me));
                }
                for (; idx < 1000; idx++) {
                    ret[idx] = tmp[idx % tmp.Length];
                }
            } else
                return Array.Empty<string>();
            return ret;
        }

        private string[] AccentConsonants(RNG rng, string[] me, double influence) {
            string[] ret = new string[1000];
            int otherCount = (int)(1000 * influence);
            int idx;
            if (me.Length > 0) {
                string[] tmp = new string[me.Length];
                rng.Shuffle(me, tmp);
                for (idx = 0; idx < otherCount; idx++) {
                    ret[idx] = tmp[idx % tmp.Length]
                            .Replace('c', rng.RandomElement(accentedConsonants[1]))
                            .Replace('d', rng.RandomElement(accentedConsonants[2]))
                            .Replace('f', rng.RandomElement(accentedConsonants[3]))
                            .Replace('g', rng.RandomElement(accentedConsonants[4]))
                            .Replace('h', rng.RandomElement(accentedConsonants[5]))
                            .Replace('j', rng.RandomElement(accentedConsonants[6]))
                            .Replace('k', rng.RandomElement(accentedConsonants[7]))
                            .Replace('l', rng.RandomElement(accentedConsonants[8]))
                            .Replace('n', rng.RandomElement(accentedConsonants[10]))
                            .Replace('r', rng.RandomElement(accentedConsonants[13]))
                            .Replace('s', rng.RandomElement(accentedConsonants[14]))
                            .Replace('t', rng.RandomElement(accentedConsonants[15]))
                            .Replace('w', rng.RandomElement(accentedConsonants[17]))
                            .Replace('y', rng.RandomElement(accentedConsonants[19]))
                            .Replace('z', rng.RandomElement(accentedConsonants[20]));

                    ret[idx] = repeats.Replace(ret[idx], rng.RandomElement(me));
                }
                for (; idx < 1000; idx++) {
                    ret[idx] = tmp[idx % tmp.Length];
                }
            } else
                return Array.Empty<string>();
            return ret;
        }

        private string[] AccentBoth(IRNG rng, string[] me, double vowelInfluence, double consonantInfluence) {
            string[] ret = new string[1000];
            int idx;
            if (me.Length > 0) {
                string[] tmp = new string[me.Length];
                rng.Shuffle(me, tmp);
                for (idx = 0; idx < 1000; idx++) {
                    bool subVowel = rng.NextDouble() < vowelInfluence, subCon = rng.NextDouble() < consonantInfluence;
                    if (subVowel && subCon) {
                        ret[idx] = tmp[idx % tmp.Length]
                        .Replace('a', rng.RandomElement(accentedVowels[0]))
                        .Replace('e', rng.RandomElement(accentedVowels[1]))
                        .Replace('i', rng.RandomElement(accentedVowels[2]))
                        .Replace('o', rng.RandomElement(accentedVowels[3]))
                        .Replace('u', rng.RandomElement(accentedVowels[4]))

                            .Replace('c', rng.RandomElement(accentedConsonants[1]))
                            .Replace('d', rng.RandomElement(accentedConsonants[2]))
                            .Replace('f', rng.RandomElement(accentedConsonants[3]))
                            .Replace('g', rng.RandomElement(accentedConsonants[4]))
                            .Replace('h', rng.RandomElement(accentedConsonants[5]))
                            .Replace('j', rng.RandomElement(accentedConsonants[6]))
                            .Replace('k', rng.RandomElement(accentedConsonants[7]))
                            .Replace('l', rng.RandomElement(accentedConsonants[8]))
                            .Replace('n', rng.RandomElement(accentedConsonants[10]))
                            .Replace('r', rng.RandomElement(accentedConsonants[13]))
                            .Replace('s', rng.RandomElement(accentedConsonants[14]))
                            .Replace('t', rng.RandomElement(accentedConsonants[15]))
                            .Replace('w', rng.RandomElement(accentedConsonants[17]))
                            .Replace('y', rng.RandomElement(accentedConsonants[19]))
                            .Replace('z', rng.RandomElement(accentedConsonants[20]));

                        ret[idx] = repeats.Replace(ret[idx], rng.RandomElement(me));
                    } else if (subVowel) {
                        ret[idx] = tmp[idx % tmp.Length]
                        .Replace('a', rng.RandomElement(accentedVowels[0]))
                        .Replace('e', rng.RandomElement(accentedVowels[1]))
                        .Replace('i', rng.RandomElement(accentedVowels[2]))
                        .Replace('o', rng.RandomElement(accentedVowels[3]))
                        .Replace('u', rng.RandomElement(accentedVowels[4]));

                        ret[idx] = repeats.Replace(ret[idx], rng.RandomElement(me));
                    } else if (subCon) {
                        ret[idx] = tmp[idx % tmp.Length]
                            .Replace('c', rng.RandomElement(accentedConsonants[1]))
                            .Replace('d', rng.RandomElement(accentedConsonants[2]))
                            .Replace('f', rng.RandomElement(accentedConsonants[3]))
                            .Replace('g', rng.RandomElement(accentedConsonants[4]))
                            .Replace('h', rng.RandomElement(accentedConsonants[5]))
                            .Replace('j', rng.RandomElement(accentedConsonants[6]))
                            .Replace('k', rng.RandomElement(accentedConsonants[7]))
                            .Replace('l', rng.RandomElement(accentedConsonants[8]))
                            .Replace('n', rng.RandomElement(accentedConsonants[10]))
                            .Replace('r', rng.RandomElement(accentedConsonants[13]))
                            .Replace('s', rng.RandomElement(accentedConsonants[14]))
                            .Replace('t', rng.RandomElement(accentedConsonants[15]))
                            .Replace('w', rng.RandomElement(accentedConsonants[17]))
                            .Replace('y', rng.RandomElement(accentedConsonants[19]))
                            .Replace('z', rng.RandomElement(accentedConsonants[20]));

                        ret[idx] = repeats.Replace(ret[idx], rng.RandomElement(me));
                    } else ret[idx] = tmp[idx % tmp.Length];

                }
            } else
                return Array.Empty<string>();
            return ret;
        }

        public static LanguageGen RandomLanguage(IRNG rng) {
            if (rng == null) rng = new RNG();
            string seed = rng.StateCode;
            int[] lengths = new int[rng.NextInt(3, 5)];
            double[] chances = new double[lengths.Length];
            for (int i = 0; i < lengths.Length; i++) {
                lengths[i] = i + 1;
            }
            chances[0] = rng.NextDouble(5, 9);
            chances[1] = rng.NextDouble(13, 22);
            chances[2] = rng.NextDouble(3, 6);
            if(lengths.Length == 4)
                chances[3] = rng.NextDouble(1, 3);
            double vowelHeavy = rng.NextDouble(0.2, 0.5), removalRate = rng.NextDouble(0.15, 0.65);
            int sz = openCons.Count;
            int[] reordering = new int[sz], vOrd = new int[openVowels.Count];
            for (int i = 0; i < sz; i++)
                reordering[i] = i;
            for (int i = 0; i < openVowels.Count; i++)
                vOrd[i] = i;
            rng.ShuffleInPlace(reordering);
            rng.ShuffleInPlace(vOrd);
            IndexedDictionary<string, string>
                    parts0 = new IndexedDictionary<string, string>(openVowels),
                    parts1 = new IndexedDictionary<string, string>(openCons),
                    parts2 = new IndexedDictionary<string, string>(midCons),
                    parts3 = new IndexedDictionary<string, string>(closeCons);
            IndexedSet<string> forbidden = new IndexedSet<string>(), missingSounds = new IndexedSet<string>();
            ArrayTools.Reorder(parts1.Ordering, reordering);
            ArrayTools.Reorder(parts2.Ordering, reordering);
            ArrayTools.Reorder(parts3.Ordering, reordering);
            ArrayTools.Reorder(parts0.Ordering, vOrd);
            int n;

            int mn = Math.Min(rng.NextInt(3), rng.NextInt(3)), sz0, p0s;

            for (n = 0; n < mn; n++) {
                missingSounds.Add(parts0[Key.At, 0]);
                forbidden.AddAll(parts0[Value.At, 0].Split(' '));
                parts0.Remove(parts0[Key.At, 0]);
            }
            p0s = parts0.Count;
            sz0 = Math.Max(rng.NextInt(1, p0s + 1), rng.NextInt(1, p0s + 1));
            char[] nextAccents = new char[sz0], unaccented = new char[sz0];
            int vowelAccent = rng.NextInt(1, 7);
            for (int i = 0; i < sz0; i++) {
                nextAccents[i] = accentedVowels[vOrd[i + mn]][vowelAccent];
                unaccented[i] = accentedVowels[vOrd[i + mn]][0];
            }
            if (rng.NextDouble() < 0.8) {
                for (int i = 0; i < sz0; i++) {
                    char ac = nextAccents[i], ua = unaccented[i];
                    String v = "", uas = new string(ua, 1);
                    Replacer rep = new Replacer("\\b([aeiou]*)(" + ua + ")([aeiou]*)\\b", "$1$2$3 $1" + ac + "$3"),
                        repLess = new Replacer("\\b([aeiou]*)(" + ua + ")([aeiou]*)\\b", "$1" + ac + "$3");
                    for (int j = 0; j < p0s; j++) {
                        String k = parts0[Key.At, j];
                        if (uas.Equals(k)) // uas is never null, always length 1
                            v = parts0[Value.At, j];
                        else {
                            String current = parts0[Value.At, j];
                            String[] splits = current.Split(' ');
                            for (int s = 0; s < splits.Length; s++) {
                                if (forbidden.Contains(uas) && splits[s].Contains(uas))
                                    forbidden.Add(splits[s].Replace(ua, ac));
                            }
                            parts0[k] = rep.Replace(current);
                        }
                    }
                    parts0[ac.ToString()] = repLess.Replace(v);
                }
            }

            n = 0;
            if (rng.NextDouble() < 0.75) {
                missingSounds.Add("z");
                forbidden.AddAll(parts1["z"].Split(' '));
                forbidden.AddAll(parts2["z"].Split(' '));
                forbidden.AddAll(parts3["z"].Split(' '));
                n++;
            }
            if (rng.NextDouble() < 0.82) {
                missingSounds.Add("x");
                forbidden.AddAll(parts1["x"].Split(' '));
                forbidden.AddAll(parts2["x"].Split(' '));
                forbidden.AddAll(parts3["x"].Split(' '));
                n++;
            }
            if (rng.NextDouble() < 0.92) {
                missingSounds.Add("qu");
                forbidden.AddAll(parts1["qu"].Split(' '));
                forbidden.AddAll(parts2["qu"].Split(' '));
                forbidden.AddAll(parts3["qu"].Split(' '));
                n++;
            }
            if (rng.NextDouble() < 0.96) {
                missingSounds.Add("q");
                forbidden.AddAll(parts1["q"].Split(' '));
                forbidden.AddAll(parts2["q"].Split(' '));
                forbidden.AddAll(parts3["q"].Split(' '));
                n++;
            }
            if (rng.NextDouble() < 0.97) {
                missingSounds.Add("tl");
                forbidden.AddAll(parts1["tl"].Split(' '));
                forbidden.AddAll(parts2["tl"].Split(' '));
                forbidden.AddAll(parts3["tl"].Split(' '));
                n++;
            }
            if (rng.NextDouble() < 0.86) {
                missingSounds.Add("ph");
                forbidden.AddAll(parts1["ph"].Split(' '));
                forbidden.AddAll(parts2["ph"].Split(' '));
                forbidden.AddAll(parts3["ph"].Split(' '));
                n++;
            }
            if (rng.NextDouble() < 0.94) {
                missingSounds.Add("kh");
                forbidden.AddAll(parts1["kh"].Split(' '));
                forbidden.AddAll(parts2["kh"].Split(' '));
                forbidden.AddAll(parts3["kh"].Split(' '));
                n++;
            }
            if (rng.NextDouble() < 0.96) {
                missingSounds.Add("bh");
                missingSounds.Add("dh");
                forbidden.AddAll(parts1["bh"].Split(' '));
                forbidden.AddAll(parts2["bh"].Split(' '));
                forbidden.AddAll(parts3["bh"].Split(' '));
                forbidden.AddAll(parts1["dh"].Split(' '));
                forbidden.AddAll(parts2["dh"].Split(' '));
                forbidden.AddAll(parts3["dh"].Split(' '));
                n++;
                n++;
            }

            for (; n < sz * removalRate; n++) {
                missingSounds.Add(parts1[Key.At, n]);
                missingSounds.Add(parts2[Key.At, n]);
                missingSounds.Add(parts3[Key.At, n]);
                forbidden.AddAll(parts1[Value.At, n].Split(' '));
                forbidden.AddAll(parts2[Value.At, n].Split(' '));
                forbidden.AddAll(parts3[Value.At, n].Split(' '));
            }

            LanguageGen lang = new LanguageGen(
                    ProcessParts(parts0, missingSounds, forbidden, rng, 0.0, p0s),
                    Array.Empty<string>(),
                    ProcessParts(openCons, missingSounds, forbidden, rng, 0.0, 4096),
                    ProcessParts(midCons, missingSounds, forbidden, rng, (rng.NextDouble() * 3 - 0.75) * 0.4444, 4096),
                    ProcessParts(closeCons, missingSounds, forbidden, rng, (rng.NextDouble() * 3 - 0.75) * 0.2857, 4096),
                    Array.Empty<string>(),
                    Array.Empty<string>(), lengths, chances, vowelHeavy, vowelHeavy * 1.8, 0.0, 0.0, GenericSanityChecks, true);
            lang.Summary = "0#" + seed + "@1";
            return lang;
        }
        private static String[] ProcessParts(IndexedDictionary<string, string> parts, IndexedSet<string> missingSounds,
                                     IndexedSet<string> forbidden, IRNG rng, double repeatSingleChance,
                                     int preferredLimit) {
            int l, sz = parts.Count;
            List<string> working = new List<string>(sz * 24);
            String pair;
            for (int e = 0; e < parts.Count; e++) {
                string snk = parts[Key.At, e];
                string snv = parts[Value.At, e];
                if (missingSounds.Contains(snk))
                    continue;
                foreach (String t in snv.Split(' ')) {
                    if (forbidden.Contains(t))
                        continue;
                    l = t.Length;
                    int num;
                    char c;
                    switch (l) {
                        case 0:
                            break;
                        case 1:
                            working.Add(t);
                            working.Add(t);
                            working.Add(t);
                            c = t[0];
                            num = 0;
                            bool repeat = true;
                            switch (c) {
                                case 'w':
                                    num += 10;
                                    repeat = false;
                                    break;
                                case 'y':
                                case 'h':
                                    num += 8;
                                    repeat = false;
                                    break;
                                case 'q':
                                case 'x':
                                    num += 4;
                                    repeat = false;
                                    break;
                                case 'i':
                                case 'u':
                                    repeat = false;
                                    num = 13;
                                    break;
                                case 'z':
                                case 'v':
                                    num = 4;
                                    break;
                                case 'j':
                                    num = 7;
                                    break;
                                default:
                                    if (e >= preferredLimit)
                                        num = 6;
                                    else
                                        num = 13;
                                    break;
                            }
                            for (int i = 0; i < num * 3; i++) {
                                if (rng.NextDouble() < 0.75) {
                                    working.Add(t);
                                }
                            }

                            if (repeat && rng.NextDouble() < repeatSingleChance) {
                                pair = t + t;
                                if (missingSounds.Contains(pair))
                                    continue;
                                working.Add(pair);
                                working.Add(pair);
                                working.Add(pair);
                                if (rng.NextDouble() < 0.7) {
                                    working.Add(pair);
                                    working.Add(pair);
                                }
                                if (rng.NextDouble() < 0.7) {
                                    working.Add(pair);
                                }
                            }

                            break;
                        case 2:
                            if (rng.NextDouble() < 0.65) {
                                c = t[1];
                                switch (c) {
                                    case 'z':
                                        num = 1;
                                        break;
                                    case 'w':
                                        num = 3;
                                        break;
                                    case 'n':
                                        num = 4;
                                        break;
                                    default:
                                        if (e >= preferredLimit)
                                            num = 2;
                                        else
                                            num = 7;
                                        break;
                                }
                                working.Add(t);
                                for (int i = 0; i < num; i++) {
                                    if (rng.NextDouble() < 0.25) {
                                        working.Add(t);
                                    }
                                }
                            }
                            break;
                        case 3:
                            if (rng.NextDouble() < 0.5) {
                                c = t[0];
                                switch (c) {
                                    case 'z':
                                        num = 1;
                                        break;
                                    case 'w':
                                        num = 3;
                                        break;
                                    case 'n':
                                        num = 4;
                                        break;
                                    default:
                                        if (e >= preferredLimit)
                                            num = 2;
                                        else
                                            num = 6;
                                        break;
                                }
                                working.Add(t);
                                for (int i = 0; i < num; i++) {
                                    if (rng.NextDouble() < 0.2) {
                                        working.Add(t);
                                    }
                                }
                            }
                            break;
                        default:
                            if (rng.NextDouble() < 0.3 && (t[l - 1] != 'z' || rng.NextDouble() < 0.1)) {
                                working.Add(t);
                            }
                            break;
                    }
                }
            }
            return working.ToArray();
        }


        #region LANGUAGES
        /*
Comparison of the ordering of JVM SquidLib's FakeLanguageGen and SquidLibSharp's LanguageGen:

  JVM                               .NET
=============================     ================================
Lovecraft                         Lovecraft
English                           English
Greek Romanized                   Greek Romanized
Greek Authentic                   Greek Authentic
French                            French
Russian Romanized                 Russian Romanized
Russian Authentic                 Russian Authentic
Japanese Romanized                Japanese Romanized
Swahili                           Swahili
Somali                            Somali
Hindi Romanized                   Hindi Romanized
Arabic Romanized                  Arabic Romanized
Inuktitut                         Inuktitut
Norse                             Norse
Nahuatl                           Nahuatl
Mongolian                         Mongolian
Fantasy                           Goblin
Fancy Fantasy                     Elf
Goblin                            Demonic
Elf                               Infernal
Demonic                           Simplish
Infernal                          Alien A
Simplish                          Korean Romanized
Alien A                           Alien E
Korean Romanized                  Alien I
Alien E                           Alien O
Alien I                           Alien U
Alien O                           Dragon
Alien U                           Kobold
Dragon                            Insect
Kobold                            Maori
Insect                            Spanish
Maori                             Deep Speech
Spanish                           Norse Simplified
Deep Speech                       Hletkip
Norse Simplified                  Ancient Egyptian
Hletkip                           Crow
Ancient Egyptian                  Imp
Crow                              Malay
Imp                               Celestial
Malay                             Chinese Romanized
Celestial                         Cherokee Romanized
Chinese Romanized                 Vietnamese
Cherokee Romanized                Fantasy
Vietnamese                        Fancy Fantasy

Fantasy and Fancy Fantasy are out of order, which is required because they combine other languages and those languages must be defined first.
All of the languages are implemented, but getting cross-VM output to be identical would be hard, and isn't currently possible with this code.
         */
        /*
Clojure script to mass-convert most of this:

(spit "languages2.txt" (clojure.string/replace (slurp "languages.txt")
#"(?s)private static FakeLanguageGen .+?return new FakeLanguageGen(\(.+?\));.+?public static final FakeLanguageGen (\w+) = .+?\.r(egister.+?);"
"public static readonly LanguageGen $2 = new LanguageGen$1.R$3;"))
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
                new string[]{"a", "a", "a", "a", "a", "o", "o", "e", "e", "e", "i", "i", "i", "i", "i", "au", "ai", "ai", "oi", "oi",
                        "ia", "io", "u", "u", "eo", "ei", "o", "o", "ou", "oi", "y", "y", "y", "y"},
                new string[] { "ui", "ui", "ei", "ei" },
                new string[]{"rh", "s", "z", "t", "t", "k", "ch", "n", "th", "kth", "m", "p", "ps", "b", "l", "kr",
                        "g", "phth", "d", "t", "k", "ch", "n", "ph", "ph", "k",},
                new string[] { "lph", "pl", "l", "l", "kr", "nch", "nx", "ps" },
                new string[]{"s", "p", "t", "ch", "n", "m", "s", "p", "t", "ch", "n", "m", "b", "g", "st", "rst",
                        "rt", "sp", "rk", "ph", "x", "z", "nk", "ng", "th", "d", "k", "n", "n",},
                new string[]{"os", "os", "os", "is", "is", "us", "um", "eum", "ium", "iam", "us", "um", "es",
                        "anes", "eros", "or", "or", "ophon", "on", "on", "ikon", "otron", "ik",},
                Array.Empty<string>(), new int[] { 1, 2, 3, 4 }, new double[] { 5, 7, 4, 1 }, 0.45, 0.45, 0.0, 0.2, null, true).Register("Greek Romanized");

        public static readonly LanguageGen GREEK_AUTHENTIC = new LanguageGen(
            new string[]{"α", "α", "α", "α", "α", "ο", "ο", "ε", "ε", "ε", "ι", "ι", "ι", "ι", "ι", "αυ", "αι", "αι", "οι", "οι",
                    "ια", "ιο", "ου", "ου", "εο", "ει", "ω", "ω", "ωυ", "ωι", "υ", "υ", "υ", "υ"},
            new string[] { "υι", "υι", "ει", "ει" },
            new string[]{"ρ", "σ", "ζ", "τ", "τ", "κ", "χ", "ν", "θ", "κθ", "μ", "π", "ψ", "β", "λ", "κρ",
                    "γ", "φθ", "δ", "τ", "κ", "χ", "ν", "φ", "φ", "κ",},
            new string[] { "λφ", "πλ", "λ", "λ", "κρ", "γχ", "γξ", "ψ" },
            new string[]{"σ", "π", "τ", "χ", "ν", "μ", "σ", "π", "τ", "χ", "ν", "μ", "β", "γ", "στ", "ρστ",
                    "ρτ", "σπ", "ρκ", "φ", "ξ", "ζ", "γκ", "γγ", "θ", "δ", "κ", "ν", "ν",},
            new string[]{"ος", "ος", "ος", "ις", "ις", "υς", "υμ", "ευμ", "ιυμ", "ιαμ", "υς", "υμ", "ες",
                    "ανες", "ερος", "ορ", "ορ", "οφον", "ον", "ον", "ικον", "οτρον", "ικ",},
            Array.Empty<string>(), new int[] { 1, 2, 3, 4 }, new double[] { 5, 7, 4, 1 }, 0.45, 0.45, 0.0, 0.2, null, true).Register("Greek Authentic");

        public static readonly LanguageGen FRENCH = new LanguageGen(
                new string[]{"a", "a", "a", "e", "e", "e", "i", "i", "o", "u", "a", "a", "a", "e", "e", "e", "i", "i", "o",
                        "a", "a", "a", "e", "e", "e", "i", "i", "o", "u", "a", "a", "a", "e", "e", "e", "i", "i", "o",
                        "a", "a", "e", "e", "i", "o", "a", "a", "a", "e", "e", "e", "i", "i", "o",
                        "ai", "oi", "oui", "au", "œu", "ou"
                },
                new string[]{
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
                new string[]{"tr", "ch", "m", "b", "b", "br", "j", "j", "j", "j", "g", "t", "t", "t", "c", "d", "f", "f", "h", "n", "l", "l",
                        "s", "s", "s", "r", "r", "r", "v", "v", "p", "pl", "pr", "bl", "br", "dr", "gl", "gr"},
                new string[]{"cqu", "gu", "qu", "rqu", "nt", "ng", "ngu", "mb", "ll", "nd", "ndr", "nct", "st",
                        "xt", "mbr", "pl", "g", "gg", "ggr", "gl", "bl", "j", "gn",
                        "m", "m", "mm", "v", "v", "f", "f", "f", "ff", "b", "b", "bb", "d", "d", "dd", "s", "s", "s", "ss", "ss", "ss",
                        "cl", "cr", "ng", "ç", "ç", "rç", "rd", "lg", "rg"},
                new string[]{"rt", "ch", "m", "b", "b", "lb", "t", "t", "t", "t", "c", "d", "f", "f", "n", "n", "l", "l",
                        "s", "s", "s", "r", "r", "p", "rd", "ff", "ss", "ll"
                },
                new string[]{"e", "e", "e", "e", "e", "é", "é", "er", "er", "er", "er", "er", "es", "es", "es", "es", "es", "es",
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
                new string[] { "a", "e", "e", "i", "i", "o", "u", "ie", "y", "e", "iu", "ia", "y", "a", "a", "o", "u" },
                Array.Empty<string>(),
                new string[]{"b", "v", "g", "d", "k", "l", "p", "r", "s", "t", "f", "kh", "ts",
                        "b", "v", "g", "d", "k", "l", "p", "r", "s", "t", "f", "kh", "ts",
                        "b", "v", "g", "d", "k", "l", "p", "r", "s", "t", "f",
                        "zh", "m", "n", "z", "ch", "sh", "shch",
                        "br", "sk", "tr", "bl", "gl", "kr", "gr"},
                new string[] { "bl", "br", "pl", "dzh", "tr", "gl", "gr", "kr" },
                new string[]{"b", "v", "g", "d", "zh", "z", "k", "l", "m", "n", "p", "r", "s", "t", "f", "kh", "ts", "ch", "sh",
                        "v", "f", "sk", "sk", "sk", "s", "b", "d", "d", "n", "r", "r"},
                new string[] { "odka", "odna", "usk", "ask", "usky", "ad", "ar", "ovich", "ev", "ov", "of", "agda", "etsky", "ich", "on", "akh", "iev", "ian" },
                Array.Empty<string>(), new int[] { 1, 2, 3, 4, 5, 6 }, new double[] { 4, 5, 6, 5, 3, 1 }, 0.1, 0.2, 0.0, 0.12, null, true).Register("Russian Romanized");

        public static readonly LanguageGen RUSSIAN_AUTHENTIC = new LanguageGen(
                new string[] { "а", "е", "ё", "и", "й", "о", "у", "ъ", "ы", "э", "ю", "я", "ы", "а", "а", "о", "у" },
                Array.Empty<string>(),
                new string[]{"б", "в", "г", "д", "к", "л", "п", "р", "с", "т", "ф", "х", "ц",
                        "б", "в", "г", "д", "к", "л", "п", "р", "с", "т", "ф", "х", "ц",
                        "б", "в", "г", "д", "к", "л", "п", "р", "с", "т", "ф",
                        "ж", "м", "н", "з", "ч", "ш", "щ",
                        "бр", "ск", "тр", "бл", "гл", "кр", "гр"},
                new string[] { "бл", "бр", "пл", "дж", "тр", "гл", "гр", "кр" },
                new string[]{"б", "в", "г", "д", "ж", "з", "к", "л", "м", "н", "п", "р", "с", "т", "ф", "х", "ц", "ч", "ш",
                        "в", "ф", "ск", "ск", "ск", "с", "б", "д", "д", "н", "р", "р"},
                new string[] { "одка", "одна", "уск", "аск", "ускы", "ад", "ар", "овйч", "ев", "ов", "оф", "агда", "ёцкы", "йч", "он", "ах", "ъв", "ян" },
                Array.Empty<string>(), new int[] { 1, 2, 3, 4, 5, 6 }, new double[] { 4, 5, 6, 5, 3, 1 }, 0.1, 0.2, 0.0, 0.12, null, true).Register("Russian Authentic");


        public static readonly LanguageGen JAPANESE_ROMANIZED = new LanguageGen(
                    new string[] { "a", "a", "a", "a", "e", "e", "i", "i", "i", "i", "o", "o", "o", "u", "ou", "u", "ai", "ai" },
                    Array.Empty<string>(),
                    new string[]{"k", "ky", "s", "sh", "t", "ts", "ch", "n", "ny", "h", "f", "hy", "m", "my", "y", "r", "ry", "g",
                        "gy", "z", "j", "d", "b", "by", "p", "py",
                        "k", "t", "n", "s", "k", "t", "d", "s", "sh", "sh", "g", "r", "b",
                        "k", "t", "n", "s", "k", "t", "b", "s", "sh", "sh", "g", "r", "b",
                        "k", "t", "n", "s", "k", "t", "z", "s", "sh", "sh", "ch", "ry", "ts"
                    },
                    new string[]{"k", "ky", "s", "sh", "t", "ts", "ch", "n", "ny", "h", "f", "hy", "m", "my", "y", "r", "ry", "g",
                        "gy", "z", "j", "d", "b", "by", "p", "py",
                        "k", "t", "d", "s", "k", "t", "d", "s", "sh", "sh", "y", "j", "p", "r", "d",
                        "k", "t", "b", "s", "k", "t", "b", "s", "sh", "sh", "y", "j", "p", "r", "d",
                        "k", "t", "z", "s", "f", "g", "z", "b", "d", "ts", "sh", "m",
                        "k", "t", "z", "s", "f", "g", "z", "b", "d", "ts", "sh", "m",
                        "nn", "nn", "nd", "nz", "mm", "kk", "tt", "ss", "ssh", "tch"},
                    new string[] { "n" },
                    new string[] { "ima", "aki", "aka", "ita", "en", "izen", "achi", "uke", "aido", "outsu", "uki", "oku", "aku", "oto", "okyo" },
                    Array.Empty<string>(), new int[] { 1, 2, 3, 4, 5 }, new double[] { 5, 4, 5, 4, 3 }, 0.3, 0.9, 0.0, 0.07, JapaneseSanityChecks, true).Register("Japanese Romanized");

        public static readonly LanguageGen SWAHILI = new LanguageGen(
                    new string[]{"a", "i", "o", "e", "u",
                        "a", "a", "i", "o", "o", "e", "u",
                        "a", "a", "i", "o", "o", "u",
                        "a", "a", "i", "i", "o",
                        "a", "a", "a", "a", "a",
                        "a", "i", "o", "e", "u",
                        "a", "a", "i", "o", "o", "e", "u",
                        "a", "a", "i", "o", "o", "u",
                        "a", "a", "i", "i", "o",
                        "a", "a", "a", "a", "a",
                        "aa", "aa", "ue", "uo", "ii", "ea"},
                    Array.Empty<string>(),
                    new string[]{
                        "b", "h", "j", "l", "s", "y", "m", "n",
                        "b", "ch", "h", "j", "l", "s", "y", "z", "m", "n",
                        "b", "ch", "f", "g", "h", "j", "k", "l", "p", "s", "y", "z", "m", "n",
                        "b", "ch", "d", "f", "g", "h", "j", "k", "l", "p", "s", "t", "y", "z", "m", "n", "kw",
                        "b", "ch", "d", "f", "g", "h", "j", "k", "l", "p", "s", "t", "v", "w", "y", "z", "m", "n", "kw",

                        "b", "h", "j", "l", "s", "y", "m", "n",
                        "b", "ch", "h", "j", "l", "s", "y", "z", "m", "n",
                        "b", "ch", "f", "g", "h", "j", "k", "l", "p", "s", "y", "z", "m", "n",
                        "b", "ch", "d", "f", "g", "h", "j", "k", "l", "p", "s", "t", "y", "z", "m", "n", "kw",
                        "b", "ch", "d", "f", "g", "h", "j", "k", "l", "p", "s", "t", "v", "w", "y", "z", "m", "n", "kw",

                        "b", "h", "j", "l", "s", "y", "m", "n",
                        "b", "ch", "h", "j", "l", "s", "y", "z", "m", "n",
                        "b", "ch", "f", "g", "h", "j", "k", "l", "p", "s", "y", "z", "m", "n",
                        "b", "ch", "d", "f", "g", "h", "j", "k", "l", "p", "s", "t", "y", "z", "m", "n", "kw",
                        "b", "ch", "d", "f", "g", "h", "j", "k", "l", "p", "s", "t", "v", "w", "y", "z", "m", "n", "kw",

                        "b", "h", "j", "l", "s", "y", "m", "n",
                        "b", "ch", "h", "j", "l", "s", "y", "z", "m", "n",
                        "b", "ch", "f", "g", "h", "j", "k", "l", "p", "s", "y", "z", "m", "n",
                        "b", "ch", "d", "f", "g", "h", "j", "k", "l", "p", "s", "t", "y", "z", "m", "n", "kw",
                        "b", "ch", "d", "f", "g", "h", "j", "k", "l", "p", "s", "t", "v", "w", "y", "z", "m", "n", "kw",

                        "nb", "nj", "ns", "nz",
                        "nb", "nch", "nj", "ns", "ny", "nz",
                        "nb", "nch", "nf", "ng", "nj", "nk", "np", "ns", "nz",
                        "nb", "nch", "nd", "nf", "ng", "nj", "nk", "np", "ns", "nt", "nz",
                        "nb", "nch", "nd", "nf", "ng", "nj", "nk", "np", "ns", "nt", "nv", "nw", "nz",

                        "mb", "ms", "my", "mz",
                        "mb", "mch", "ms", "my", "mz",
                        "mb", "mch", "mk", "mp", "ms", "my", "mz",
                        "mb", "mch", "md", "mk", "mp", "ms", "mt", "my", "mz",
                        "mb", "mch", "md", "mf", "mg", "mj", "mk", "mp", "ms", "mt", "mv", "mw", "my", "mz",
                        "sh", "sh", "sh", "ny", "kw",
                        "dh", "th", "sh", "ny",
                        "dh", "th", "sh", "gh", "r", "ny",
                        "dh", "th", "sh", "gh", "r", "ny",
                    },
                    new string[]{
                        "b", "h", "j", "l", "s", "y", "m", "n",
                        "b", "ch", "h", "j", "l", "s", "y", "z", "m", "n",
                        "b", "ch", "f", "g", "h", "j", "k", "l", "p", "s", "y", "z", "m", "n",
                        "b", "ch", "d", "f", "g", "h", "j", "k", "l", "p", "s", "t", "y", "z", "m", "n", "kw",
                        "b", "ch", "d", "f", "g", "h", "j", "k", "l", "p", "s", "t", "v", "w", "y", "z", "m", "n", "kw",

                        "b", "h", "j", "l", "s", "y", "m", "n",
                        "b", "ch", "h", "j", "l", "s", "y", "z", "m", "n",
                        "b", "ch", "f", "g", "h", "j", "k", "l", "p", "s", "y", "z", "m", "n",
                        "b", "ch", "d", "f", "g", "h", "j", "k", "l", "p", "s", "t", "y", "z", "m", "n", "kw",
                        "b", "ch", "d", "f", "g", "h", "j", "k", "l", "p", "s", "t", "v", "w", "y", "z", "m", "n", "kw",

                        "b", "h", "j", "l", "s", "y", "m", "n",
                        "b", "ch", "h", "j", "l", "s", "y", "z", "m", "n",
                        "b", "ch", "f", "g", "h", "j", "k", "l", "p", "s", "y", "z", "m", "n",
                        "b", "ch", "d", "f", "g", "h", "j", "k", "l", "p", "s", "t", "y", "z", "m", "n", "kw",
                        "b", "ch", "d", "f", "g", "h", "j", "k", "l", "p", "s", "t", "v", "w", "y", "z", "m", "n", "kw",

                        "b", "h", "j", "l", "s", "y", "m", "n",
                        "b", "ch", "h", "j", "l", "s", "y", "z", "m", "n",
                        "b", "ch", "f", "g", "h", "j", "k", "l", "p", "s", "y", "z", "m", "n",
                        "b", "ch", "d", "f", "g", "h", "j", "k", "l", "p", "s", "t", "y", "z", "m", "n", "kw",
                        "b", "ch", "d", "f", "g", "h", "j", "k", "l", "p", "s", "t", "v", "w", "y", "z", "m", "n", "kw",

                        "nb", "nj", "ns", "nz",
                        "nb", "nch", "nj", "ns", "ny", "nz",
                        "nb", "nch", "nf", "ng", "nj", "nk", "np", "ns", "nz",
                        "nb", "nch", "nd", "nf", "ng", "nj", "nk", "np", "ns", "nt", "nz",
                        "nb", "nch", "nd", "nf", "ng", "nj", "nk", "np", "ns", "nt", "nw", "nz",

                        "mb", "ms", "my", "mz",
                        "mb", "mch", "ms", "my", "mz",
                        "mb", "mch", "mk", "mp", "ms", "my", "mz",
                        "mb", "mch", "md", "mk", "mp", "ms", "mt", "my", "mz",
                        "mb", "mch", "md", "mf", "mg", "mj", "mk", "mp", "ms", "mt", "mw", "my", "mz",
                        "sh", "sh", "sh", "ny", "kw",
                        "dh", "th", "sh", "ny",
                        "dh", "th", "sh", "gh", "r", "ny",
                        "dh", "th", "sh", "gh", "r", "ny",
                        "ng", "ng", "ng", "ng", "ng"
                    },
                    new string[] { "" },
                    new string[] { "-@" },
                    Array.Empty<string>(), new int[] { 1, 2, 3, 4, 5, 6 }, new double[] { 3, 8, 6, 9, 2, 2 }, 0.2, 1.0, 0.0, 0.12, null, true).Register("Swahili");

        public static readonly LanguageGen SOMALI = new LanguageGen(
                    new string[]{"a", "a", "a", "a", "a", "a", "a", "aa", "aa", "aa",
                        "e", "e", "ee",
                        "i", "i", "i", "i", "ii",
                        "o", "o", "o", "oo",
                        "u", "u", "u", "uu", "uu",
                    },
                    Array.Empty<string>(),
                    new string[]{"b", "t", "j", "x", "kh", "d", "r", "s", "sh", "dh", "c", "g", "f", "q", "k", "l", "m",
                        "n", "w", "h", "y",
                        "x", "g", "b", "d", "s", "m", "dh", "n", "r",
                        "g", "b", "s", "dh",
                    },
                    new string[]{
                        "bb", "gg", "dd", "bb", "dd", "rr", "ddh", "cc", "gg", "ff", "ll", "mm", "nn",
                        "bb", "gg", "dd", "bb", "dd", "gg",
                        "bb", "gg", "dd", "bb", "dd", "gg",
                        "cy", "fk", "ft", "nt", "rt", "lt", "qm", "rdh", "rsh", "lq",
                        "my", "gy", "by", "lkh", "rx", "md", "bd", "dg", "fd", "mf",
                        "dh", "dh", "dh", "dh",
                    },
                    new string[]{
                        "b", "t", "j", "x", "kh", "d", "r", "s", "sh", "c", "g", "f", "q", "k", "l", "m", "n", "h",
                        "x", "g", "b", "d", "s", "m", "q", "n", "r",
                        "b", "t", "j", "x", "kh", "d", "r", "s", "sh", "c", "g", "f", "q", "k", "l", "m", "n", "h",
                        "x", "g", "b", "d", "s", "m", "q", "n", "r",
                        "b", "t", "j", "x", "kh", "d", "r", "s", "sh", "c", "g", "f", "q", "k", "l", "m", "n",
                        "g", "b", "d", "s", "q", "n", "r",
                        "b", "t", "x", "kh", "d", "r", "s", "sh", "g", "f", "q", "k", "l", "m", "n",
                        "g", "b", "d", "s", "r", "n",
                        "b", "t", "kh", "d", "r", "s", "sh", "g", "f", "q", "k", "l", "m", "n",
                        "g", "b", "d", "s", "r", "n",
                        "b", "t", "d", "r", "s", "sh", "g", "f", "q", "k", "l", "m", "n",
                        "g", "b", "d", "s", "r", "n",
                    },
                    new string[] { "aw", "ow", "ay", "ey", "oy", "ay", "ay" },
                    Array.Empty<string>(), new int[] { 1, 2, 3, 4, 5 }, new double[] { 5, 4, 5, 4, 1 }, 0.25, 0.3, 0.0, 0.08, null, true).Register("Somali");
        public static readonly LanguageGen HINDI_ROMANIZED = new LanguageGen(
                    new string[]{
                        "a", "a", "a", "a", "a", "a", "ā", "ā", "i", "i", "i", "i", "ī", "ī",
                        "u", "u", "u", "ū", "e", "ai", "ai", "o", "o", "o", "au",
                        "a", "a", "a", "a", "a", "a", "ā", "ā", "i", "i", "i", "i", "ī", "ī",
                        "u", "u", "u", "ū", "e", "ai", "ai", "o", "o", "o", "au",
                        "a", "a", "a", "a", "a", "a", "ā", "ā", "i", "i", "i", "i", "ī", "ī",
                        "u", "u", "u", "ū", "e", "ai", "ai", "o", "o", "o", "au",
                        "a", "a", "a", "a", "a", "a", "ā", "ā", "i", "i", "i", "i", "ī", "ī",
                        "u", "u", "u", "ū", "e", "ai", "ai", "o", "o", "o", "au",
                        "a", "a", "a", "a", "a", "a", "ā", "ā", "i", "i", "i", "i", "ī", "i", "i", "ī", "ī",
                        "u", "u", "u", "ū", "u", "ū", "u", "ū", "e", "ai", "ai", "o", "o", "o", "au",
                        "a", "a", "a", "a", "a", "a", "ā", "ā", "i", "i", "i", "i", "ī", "i", "i", "ī", "ī",
                        "u", "u", "u", "ū", "u", "ū", "u", "ū", "e", "ai", "ai", "o", "o", "o", "au",
                        "a", "a", "a", "a", "a", "a", "ā", "ā", "i", "i", "i", "i", "ī", "i", "i", "ī", "ī",
                        "u", "u", "u", "ū", "u", "ū", "u", "ū", "e", "ai", "ai", "o", "o", "o", "au",
                        "a", "a", "a", "a", "a", "a", "ā", "ā", "i", "i", "i", "i", "ī", "i", "i", "ī", "ī",
                        "u", "u", "u", "ū", "u", "ū", "u", "ū", "e", "ai", "ai", "o", "o", "o", "au",
                        "aĕ", "aĕ", "aĕ", "aĕ", "aĕ", "āĕ", "āĕ", "iĕ", "iĕ", "iĕ", "īĕ", "īĕ",
                        "uĕ", "uĕ", "ūĕ", "aiĕ", "aiĕ", "oĕ", "oĕ", "oĕ", "auĕ",
                        //"aĭ", "aĭ", "aĭ", "aĭ", "aĭ", "āĭ", "āĭ", "iĭ", "iĭ", "iĭ", "īĭ", "īĭ",
                        //"uĭ", "uĭ", "ūĭ", "aiĭ", "aiĭ", "oĭ", "oĭ", "oĭ", "auĭ",
                    },
                    new string[]{"á", "í", "ú", "ó", "á", "í", "ú", "ó",
                    },
                    new string[]{
                        "k", "k", "k", "k", "k", "k", "k", "k", "kŗ", "kŕ", "kļ",
                        "c", "c", "c", "c", "c", "c", "cŗ", "cŕ", "cļ",
                        "ţ", "t", "t", "t", "t", "t", "t", "t", "t", "t", "tŗ", "tŕ", "tŗ", "tŕ",
                        "p", "p", "p", "p", "p", "p", "p", "p", "p", "p", "pŗ", "pŕ", "pļ", "pĺ", "pŗ", "pŕ", "p", "p",
                        "kh", "kh", "kh", "kh", "kh", "kh", "kh", "kh", "kh", "kh", "khŗ", "khŕ", "khļ", "khĺ",
                        "ch", "ch", "ch", "ch", "ch", "ch", "ch", "ch", "ch", "chŗ", "chŕ", "chļ", "chĺ",
                        "ţh", "th", "th", "th", "th", "th", "th", "th", "th", "th", "thŗ", "thŕ", "thļ", "thĺ",
                        "ph", "ph", "ph", "ph", "ph", "ph", "ph", "phŗ", "phŕ", "phļ", "phĺ",
                        "g", "j", "đ", "d", "b", "gh", "jh", "đh", "dh", "bh",
                        "ń", "ñ", "ņ", "n", "m", "h", "y", "r", "l", "v", "ś", "ş", "s",
                        "g", "j", "đ", "d", "b", "gh", "jh", "đh", "dh", "bh",
                        "ń", "ñ", "ņ", "n", "m", "h", "y", "r", "l", "v", "ś", "ş", "s",
                        "g", "j", "đ", "d", "b", "gh", "jh", "đh", "dh", "bh",
                        "ń", "ñ", "ņ", "n", "m", "h", "y", "r", "l", "v", "ś", "ş", "s",
                        "g", "j", "đ", "d", "b", "gh", "jh", "đh", "dh", "bh",
                        "ń", "ñ", "ņ", "n", "m", "h", "y", "r", "l", "v", "ś", "ş", "s",
                        "g", "j", "đ", "d", "b", "gh", "jh", "đh", "dh", "bh",
                        "ń", "ñ", "ņ", "n", "m", "h", "y", "r", "l", "v", "ś", "ş", "s",
                        "g", "j", "đ", "d", "b", "gh", "jh", "đh", "dh", "bh",
                        "ń", "ñ", "ņ", "n", "m", "h", "y", "r", "l", "v", "ś", "ş", "s",
                        "g", "j", "đ", "d", "b", "gh", "jh", "đh", "dh", "bh",
                        "ń", "ñ", "ņ", "n", "m", "h", "y", "r", "l", "v", "ś", "ş", "s",
                        "g", "j", "đ", "d", "b", "gh", "đh", "dh", "bh",
                        "ń", "ñ", "ņ", "n", "m", "h", "y", "r", "l", "v", "ś", "ş", "s",
                        "g", "j", "đ", "d", "b", "gh", "đh", "dh", "bh",
                        "ń", "ņ", "n", "m", "h", "y", "r", "l", "v", "ş", "s",
                        "g", "j", "đ", "d", "b", "gh", "đh", "dh", "bh",
                        "ń", "ņ", "n", "m", "h", "y", "r", "l", "v", "ş", "s",
                        "g", "đ", "d", "b", "gh", "đh", "dh", "bh", "n", "m", "v", "s",
                        "g", "đ", "d", "b", "g", "d", "b", "dh", "bh", "n", "m", "v",
                        "g", "đ", "d", "b", "g", "d", "b", "dh", "bh", "n", "m", "v",
                    },
                    new string[]{
                        "k", "k", "k", "k", "k", "nk", "rk",
                        "k", "k", "k", "k", "k", "nk", "rk",
                        "k", "k", "k", "k", "k", "nk", "rk",
                        "k", "k", "k", "k", "k", "nk", "rk",
                        "k", "k", "k", "k", "k", "nk", "rk",
                        "k", "k", "k", "k", "k", "nk", "rk",
                        "k", "k", "k", "k", "k", "nk", "rk",
                        "k", "k", "k", "k", "k", "nk", "rk",
                        "kŗ", "kŗ", "kŗ", "kŗ", "kŗ", "nkŗ", "rkŗ",
                        "kŕ", "kŕ", "kŕ", "kŕ", "kŕ", "nkŕ", "rkŕ",
                        "kļ", "kļ", "kļ", "kļ", "kļ", "nkļ", "rkļ",

                        "c", "c", "c", "c", "c", "c", "cŗ", "cŕ", "cļ",
                        "ţ", "t", "t", "t", "t", "t", "nt", "rt",
                        "ţ", "t", "t", "t", "t", "nt", "rt",
                        "ţ", "t", "t", "t", "t", "nt", "rt",
                        "ţ", "t", "t", "t", "t", "nt", "rt",
                        "ţ", "t", "t", "t", "t", "nt", "rt",
                        "ţ", "t", "t", "t", "t", "nt", "rt",
                        "ţ", "t", "t", "t", "t", "nt", "rt",
                        "ţ", "t", "t", "t", "t", "nt", "rt",
                        "ţ", "t", "t", "t", "t", "nt", "rt",
                        "tŗ", "tŗ", "tŗ", "tŗ", "tŗ", "ntŗ", "rtŗ",
                        "tŕ", "tŕ", "tŕ", "tŕ", "tŕ", "ntŕ", "rtŕ",
                        "tŗ", "tŗ", "tŗ", "tŗ", "tŗ", "ntŗ", "rtŗ",
                        "tŕ", "tŕ", "tŕ", "tŕ", "tŕ", "ntŕ", "rtŕ",

                        "p", "p", "p", "p", "p", "np", "rp",
                        "p", "p", "p", "p", "p", "np", "rp",
                        "p", "p", "p", "p", "p", "np", "rp",
                        "p", "p", "p", "p", "p", "np", "rp",
                        "p", "p", "p", "p", "p", "np", "rp",
                        "p", "p", "p", "p", "p", "np", "rp",
                        "p", "p", "p", "p", "p", "np", "rp",
                        "p", "p", "p", "p", "p", "np", "rp",
                        "p", "p", "p", "p", "p", "np", "rp",
                        "p", "p", "p", "p", "p", "np", "rp",
                        "pŗ", "pŗ", "pŗ", "pŗ", "pŗ", "npŗ", "rpŗ",
                        "pŕ", "pŕ", "pŕ", "pŕ", "pŕ", "npŕ", "rpŕ",
                        "pļ", "pļ", "pļ", "pļ", "pļ", "npļ", "rpļ",
                        "pĺ", "pĺ", "pĺ", "pĺ", "pĺ", "npĺ", "rpĺ",
                        "pŗ", "pŗ", "pŗ", "pŗ", "pŗ", "npŗ", "rpŗ",
                        "pŕ", "pŕ", "pŕ", "pŕ", "pŕ", "npŕ", "rpŕ",
                        "p", "p", "p", "p", "p", "np", "rp",
                        "p", "p", "p", "p", "p", "np", "rp",

                        "kh", "kh", "kh", "kh", "kh", "nkh", "rkh",
                        "kh", "kh", "kh", "kh", "kh", "nkh", "rkh",
                        "kh", "kh", "kh", "kh", "kh", "nkh", "rkh",
                        "kh", "kh", "kh", "kh", "kh", "nkh", "rkh",
                        "kh", "kh", "kh", "kh", "kh", "nkh", "rkh",
                        "kh", "kh", "kh", "kh", "kh", "nkh", "rkh",
                        "kh", "kh", "kh", "kh", "kh", "nkh", "rkh",
                        "kh", "kh", "kh", "kh", "kh", "nkh", "rkh",
                        "kh", "kh", "kh", "kh", "kh", "nkh", "rkh",
                        "kh", "kh", "kh", "kh", "kh", "nkh", "rkh",
                        "khŗ", "khŗ", "khŗ", "khŗ", "khŗ", "nkhŗ", "rkhŗ",
                        "khŕ", "khŕ", "khŕ", "khŕ", "khŕ", "nkhŕ", "rkhŕ",
                        "khļ", "khļ", "khļ", "khļ", "khļ", "nkhļ", "rkhļ",
                        "khĺ", "khĺ", "khĺ", "khĺ", "khĺ", "nkhĺ", "rkhĺ",

                        "ch", "ch", "ch", "ch", "ch", "ch", "ch", "ch", "ch", "chŗ", "chŕ", "chļ", "chĺ",
                        "ţh", "th", "th", "th", "th", "th", "nth", "rth",
                        "th", "th", "th", "th", "th", "nth", "rth",
                        "th", "th", "th", "th", "th", "nth", "rth",
                        "th", "th", "th", "th", "th", "nth", "rth",
                        "th", "th", "th", "th", "th", "nth", "rth",
                        "th", "th", "th", "th", "th", "nth", "rth",
                        "th", "th", "th", "th", "th", "nth", "rth",
                        "th", "th", "th", "th", "th", "nth", "rth",
                        "th", "th", "th", "th", "th", "nth", "rth",
                        "thŗ", "thŗ", "thŗ", "thŗ", "thŗ", "nthŗ", "rthŗ",
                        "thŕ", "thŕ", "thŕ", "thŕ", "thŕ", "nthŕ", "rthŕ",
                        "thļ", "thļ", "thļ", "thļ", "thļ", "nthļ", "rthļ",
                        "thĺ", "thĺ", "thĺ", "thĺ", "thĺ", "nthĺ", "rthĺ",

                        "ph", "ph", "ph", "ph", "ph", "nph", "rph",
                        "ph", "ph", "ph", "ph", "ph", "nph", "rph",
                        "ph", "ph", "ph", "ph", "ph", "nph", "rph",
                        "ph", "ph", "ph", "ph", "ph", "nph", "rph",
                        "ph", "ph", "ph", "ph", "ph", "nph", "rph",
                        "ph", "ph", "ph", "ph", "ph", "nph", "rph",
                        "ph", "ph", "ph", "ph", "ph", "nph", "rph",
                        "phŗ", "phŗ", "phŗ", "phŗ", "phŗ", "nphŗ", "rphŗ",
                        "phŕ", "phŕ", "phŕ", "phŕ", "phŕ", "nphŕ", "rphŕ",
                        "phļ", "phļ", "phļ", "phļ", "phļ", "nphļ", "rphļ",
                        "phĺ", "phĺ", "phĺ", "phĺ", "phĺ", "nphĺ", "rphĺ",

                        "g", "g", "g", "g", "g", "ng", "rg",
                        "j", "j", "j", "j", "j", "nj", "rj",
                        "đ", "đ", "đ", "đ", "đ", "nđ", "rđ",
                        "d", "d", "d", "d", "d", "nd", "rd",
                        "b", "b", "b", "b", "b", "nb", "rb",
                        "gh", "gh", "gh", "gh", "gh", "ngh", "rgh",
                        "jh", "đh", "đh", "đh", "đh", "đh", "nđh", "rđh",
                        "dh", "dh", "dh", "dh", "dh", "ndh", "rdh",
                        "bh", "bh", "bh", "bh", "bh", "nbh", "rbh",

                        "ń", "ñ", "ņ", "n", "m", "m", "m", "m", "m", "nm", "rm",
                        "h", "y", "y", "y", "y", "y", "ny", "ry",
                        "r", "l", "v", "v", "v", "v", "v", "nv", "rv",
                        "ś", "ś", "ś", "ś", "ś", "nś", "rś",
                        "ş", "ş", "ş", "ş", "ş", "nş", "rş",
                        "s", "s", "s", "s", "s", "ns", "rs",

                        "g", "g", "g", "g", "g", "ng", "rg",
                        "j", "j", "j", "j", "j", "nj", "rj",
                        "đ", "đ", "đ", "đ", "đ", "nđ", "rđ",
                        "d", "d", "d", "d", "d", "nd", "rd",
                        "b", "b", "b", "b", "b", "nb", "rb",
                        "gh", "gh", "gh", "gh", "gh", "ngh", "rgh",
                        "jh", "đh", "đh", "đh", "đh", "đh", "nđh", "rđh",
                        "dh", "dh", "dh", "dh", "dh", "ndh", "rdh",
                        "bh", "bh", "bh", "bh", "bh", "nbh", "rbh",

                        "ń", "ñ", "ņ", "n", "m", "m", "m", "m", "m", "nm", "rm",
                        "h", "y", "y", "y", "y", "y", "ny", "ry",
                        "r", "l", "v", "v", "v", "v", "v", "nv", "rv",
                        "ś", "ś", "ś", "ś", "ś", "nś", "rś",
                        "ş", "ş", "ş", "ş", "ş", "nş", "rş",
                        "s", "s", "s", "s", "s", "ns", "rs",

                        "g", "g", "g", "g", "g", "ng", "rg",
                        "j", "j", "j", "j", "j", "nj", "rj",
                        "đ", "đ", "đ", "đ", "đ", "nđ", "rđ",
                        "d", "d", "d", "d", "d", "nd", "rd",
                        "b", "b", "b", "b", "b", "nb", "rb",
                        "gh", "gh", "gh", "gh", "gh", "ngh", "rgh",
                        "jh", "đh", "đh", "đh", "đh", "đh", "nđh", "rđh",
                        "dh", "dh", "dh", "dh", "dh", "ndh", "rdh",
                        "bh", "bh", "bh", "bh", "bh", "nbh", "rbh",

                        "ń", "ñ", "ņ", "n", "m", "m", "m", "m", "m", "nm", "rm",
                        "h", "y", "y", "y", "y", "y", "ny", "ry",
                        "r", "l", "v", "v", "v", "v", "v", "nv", "rv",
                        "ś", "ś", "ś", "ś", "ś", "nś", "rś",
                        "ş", "ş", "ş", "ş", "ş", "nş", "rş",
                        "s", "s", "s", "s", "s", "ns", "rs",

                        "g", "g", "g", "g", "g", "ng", "rg",
                        "j", "j", "j", "j", "j", "nj", "rj",
                        "đ", "đ", "đ", "đ", "đ", "nđ", "rđ",
                        "d", "d", "d", "d", "d", "nd", "rd",
                        "b", "b", "b", "b", "b", "nb", "rb",
                        "gh", "gh", "gh", "gh", "gh", "ngh", "rgh",
                        "jh", "đh", "đh", "đh", "đh", "đh", "nđh", "rđh",
                        "dh", "dh", "dh", "dh", "dh", "ndh", "rdh",
                        "bh", "bh", "bh", "bh", "bh", "nbh", "rbh",

                        "ń", "ñ", "ņ", "n", "m", "m", "m", "m", "m", "nm", "rm",
                        "h", "y", "y", "y", "y", "y", "ny", "ry",
                        "r", "l", "v", "v", "v", "v", "v", "nv", "rv",
                        "ś", "ś", "ś", "ś", "ś", "nś", "rś",
                        "ş", "ş", "ş", "ş", "ş", "nş", "rş",
                        "s", "s", "s", "s", "s", "ns", "rs",

                        "g", "g", "g", "g", "g", "ng", "rg",
                        "j", "j", "j", "j", "j", "nj", "rj",
                        "đ", "đ", "đ", "đ", "đ", "nđ", "rđ",
                        "d", "d", "d", "d", "d", "nd", "rd",
                        "b", "b", "b", "b", "b", "nb", "rb",
                        "gh", "gh", "gh", "gh", "gh", "ngh", "rgh",
                        "jh", "đh", "đh", "đh", "đh", "đh", "nđh", "rđh",
                        "dh", "dh", "dh", "dh", "dh", "ndh", "rdh",
                        "bh", "bh", "bh", "bh", "bh", "nbh", "rbh",

                        "ń", "ñ", "ņ", "n", "m", "m", "m", "m", "m", "nm", "rm",
                        "h", "y", "y", "y", "y", "y", "ny", "ry",
                        "r", "l", "v", "v", "v", "v", "v", "nv", "rv",
                        "ś", "ś", "ś", "ś", "ś", "nś", "rś",
                        "ş", "ş", "ş", "ş", "ş", "nş", "rş",
                        "s", "s", "s", "s", "s", "ns", "rs",

                        "g", "g", "g", "g", "g", "ng", "rg",
                        "j", "j", "j", "j", "j", "nj", "rj",
                        "đ", "đ", "đ", "đ", "đ", "nđ", "rđ",
                        "d", "d", "d", "d", "d", "nd", "rd",
                        "b", "b", "b", "b", "b", "nb", "rb",
                        "gh", "gh", "gh", "gh", "gh", "ngh", "rgh",
                        "jh", "đh", "đh", "đh", "đh", "đh", "nđh", "rđh",
                        "dh", "dh", "dh", "dh", "dh", "ndh", "rdh",
                        "bh", "bh", "bh", "bh", "bh", "nbh", "rbh",

                        "ń", "ñ", "ņ", "n", "m", "m", "m", "m", "m", "nm", "rm",
                        "h", "y", "y", "y", "y", "y", "ny", "ry",
                        "r", "l", "v", "v", "v", "v", "v", "nv", "rv",
                        "ś", "ś", "ś", "ś", "ś", "nś", "rś",
                        "ş", "ş", "ş", "ş", "ş", "nş", "rş",
                        "s", "s", "s", "s", "s", "ns", "rs",

                        "g", "g", "g", "g", "g", "ng", "rg",
                        "j", "j", "j", "j", "j", "nj", "rj",
                        "đ", "đ", "đ", "đ", "đ", "nđ", "rđ",
                        "d", "d", "d", "d", "d", "nd", "rd",
                        "b", "b", "b", "b", "b", "nb", "rb",
                        "gh", "gh", "gh", "gh", "gh", "ngh", "rgh",
                        "jh", "đh", "đh", "đh", "đh", "đh", "nđh", "rđh",
                        "dh", "dh", "dh", "dh", "dh", "ndh", "rdh",
                        "bh", "bh", "bh", "bh", "bh", "nbh", "rbh",

                        "ń", "ñ", "ņ", "n", "m", "m", "m", "m", "m", "nm", "rm",
                        "h", "y", "y", "y", "y", "y", "ny", "ry",
                        "r", "l", "v", "v", "v", "v", "v", "nv", "rv",
                        "ś", "ś", "ś", "ś", "ś", "nś", "rś",
                        "ş", "ş", "ş", "ş", "ş", "nş", "rş",
                        "s", "s", "s", "s", "s", "ns", "rs",

                        "g", "g", "g", "g", "g", "ng", "rg",
                        "j", "j", "j", "j", "j", "nj", "rj",
                        "đ", "đ", "đ", "đ", "đ", "nđ", "rđ",
                        "d", "d", "d", "d", "d", "nd", "rd",
                        "b", "b", "b", "b", "b", "nb", "rb",
                        "gh", "gh", "gh", "gh", "gh", "ngh", "rgh",
                        "đh", "đh", "đh", "đh", "đh", "nđh", "rđh",
                        "dh", "dh", "dh", "dh", "dh", "ndh", "rdh",
                        "bh", "bh", "bh", "bh", "bh", "nbh", "rbh",

                        "ń", "ñ", "ņ", "n", "m", "m", "m", "m", "m", "nm", "rm",
                        "h", "y", "y", "y", "y", "y", "ny", "ry",
                        "r", "l", "v", "v", "v", "v", "v", "nv", "rv",
                        "ś", "ś", "ś", "ś", "ś", "nś", "rś",
                        "ş", "ş", "ş", "ş", "ş", "nş", "rş",
                        "s", "s", "s", "s", "s", "ns", "rs",

                        "g", "g", "g", "g", "g", "ng", "rg",
                        "j", "j", "j", "j", "j", "nj", "rj",
                        "đ", "đ", "đ", "đ", "đ", "nđ", "rđ",
                        "d", "d", "d", "d", "d", "nd", "rd",
                        "b", "b", "b", "b", "b", "nb", "rb",
                        "gh", "gh", "gh", "gh", "gh", "ngh", "rgh",
                        "đh", "đh", "đh", "đh", "đh", "nđh", "rđh",
                        "dh", "dh", "dh", "dh", "dh", "ndh", "rdh",
                        "bh", "bh", "bh", "bh", "bh", "nbh", "rbh",

                        "ń", "ņ", "n", "m", "m", "m", "m", "m", "nm", "rm",
                        "h", "y", "y", "y", "y", "y", "ny", "ry",
                        "r", "l", "v", "v", "v", "v", "v", "nv", "rv",
                        "ş", "ş", "ş", "ş", "ş", "nş", "rş",
                        "s", "s", "s", "s", "s", "ns", "rs",

                        "g", "g", "g", "g", "g", "ng", "rg",
                        "j", "j", "j", "j", "j", "nj", "rj",
                        "đ", "đ", "đ", "đ", "đ", "nđ", "rđ",
                        "d", "d", "d", "d", "d", "nd", "rd",
                        "b", "b", "b", "b", "b", "nb", "rb",
                        "gh", "gh", "gh", "gh", "gh", "ngh", "rgh",
                        "đh", "đh", "đh", "đh", "đh", "nđh", "rđh",
                        "dh", "dh", "dh", "dh", "dh", "ndh", "rdh",
                        "bh", "bh", "bh", "bh", "bh", "nbh", "rbh",

                        "ń", "ņ", "n", "m", "m", "m", "m", "m", "nm", "rm",
                        "h", "y", "y", "y", "y", "y", "ny", "ry",
                        "r", "l", "v", "v", "v", "v", "v", "nv", "rv",
                        "ş", "ş", "ş", "ş", "ş", "nş", "rş",
                        "s", "s", "s", "s", "s", "ns", "rs",

                        "g", "g", "g", "g", "g", "ng", "rg",
                        "đ", "đ", "đ", "đ", "đ", "nđ", "rđ",
                        "d", "d", "d", "d", "d", "nd", "rd",
                        "b", "b", "b", "b", "b", "nb", "rb",
                        "gh", "gh", "gh", "gh", "gh", "ngh", "rgh",
                        "đh", "đh", "đh", "đh", "đh", "nđh", "rđh",
                        "dh", "dh", "dh", "dh", "dh", "ndh", "rdh",
                        "bh", "bh", "bh", "bh", "bh", "nbh", "rbh",
                        "n", "m", "m", "m", "m", "m", "nm", "rm",
                        "v", "v", "v", "v", "v", "nv", "rv",
                        "s", "s", "s", "s", "s", "ns", "rs",

                        "g", "g", "g", "g", "g", "ng", "rg",
                        "đ", "đ", "đ", "đ", "đ", "nđ", "rđ",
                        "d", "d", "d", "d", "d", "nd", "rd",
                        "b", "b", "b", "b", "b", "nb", "rb",
                        "g", "g", "g", "g", "g", "ng", "rg",
                        "d", "d", "d", "d", "d", "nd", "rd",
                        "b", "b", "b", "b", "b", "nb", "rb",
                        "dh", "dh", "dh", "dh", "dh", "ndh", "rdh",
                        "bh", "bh", "bh", "bh", "bh", "nbh", "rbh",
                        "n", "m", "m", "m", "m", "m", "nm", "rm",
                        "v", "v", "v", "v", "v", "nv", "rv",

                        "g", "g", "g", "g", "g", "ng", "rg",
                        "đ", "đ", "đ", "đ", "đ", "nđ", "rđ",
                        "d", "d", "d", "d", "d", "nd", "rd",
                        "b", "b", "b", "b", "b", "nb", "rb",
                        "g", "g", "g", "g", "g", "ng", "rg",
                        "d", "d", "d", "d", "d", "nd", "rd",
                        "b", "b", "b", "b", "b", "nb", "rb",
                        "dh", "dh", "dh", "dh", "dh", "ndh", "rdh",
                        "bh", "bh", "bh", "bh", "bh", "nbh", "rbh",
                        "n", "m", "m", "m", "m", "m", "nm", "rm",
                        "v", "v", "v", "v", "v", "nv", "rv",
                    },
                    new string[] { "t", "d", "m", "r", "dh", "b", "t", "d", "m", "r", "dh", "bh", "nt", "nt", "nk", "ş" },
                    new string[] { "it", "it", "ati", "adva", "aş", "arma", "ardha", "abi", "ab", "aya" },
                    Array.Empty<string>(), new int[] { 1, 2, 3, 4, 5 }, new double[] { 1, 2, 3, 3, 1 }, 0.15, 0.75, 0.0, 0.12, null, true).Register("Hindi Romanized");

        public static readonly LanguageGen ARABIC_ROMANIZED = new LanguageGen(
                    new string[]{"a", "a", "a", "a", "a", "a", "aa", "aa", "aa", "ai", "au",
                        "a", "i", "u", "a", "i", "u",
                        "i", "i", "i", "i", "i", "ii", "ii", "ii",
                        "u", "u", "u", "uu", "uu",
                    },
                    Array.Empty<string>(),
                    new string[]{"gh", "b", "t", "th", "j", "kh", "khr", "d", "dh", "r", "z", "s", "sh", "shw",
                        "zh", "khm", "g", "f", "q", "k", "l", "m", "n", "h", "w",
                        "q", "k", "q", "k", "b", "d", "f", "l", "z", "zh", "h", "h", "kh", "j", "s", "sh", "shw", "r",
                        "q", "k", "q", "k", "f", "l", "z", "h", "h", "j", "s", "r",
                        "q", "k", "f", "l", "z", "h", "h", "j", "s", "r",
                        "al-", "al-", "ibn-",
                    },
                    new string[]{
                        "kk", "kk", "kk", "kk", "kk", "dd", "dd", "dd", "dd",
                        "nj", "mj", "bj", "mj", "bj", "mj", "bj", "dj", "dtj", "dhj",
                        "nz", "nzh", "mz", "mzh", "rz", "rzh", "bz", "dz", "tz",
                        "s-h", "sh-h", "shw-h", "tw", "bn", "fq", "hz", "hl", "khm",
                        "lb", "lz", "lj", "lf", "ll", "lk", "lq", "lg", "ln"
                    },
                    new string[]{
                        "gh", "b", "t", "th", "j", "kh", "khr", "d", "dh", "r", "z", "s", "sh", "shw", "dt", "jj",
                        "zh", "khm", "g", "f", "q", "k", "l", "m", "n", "h", "w",
                        "k", "q", "k", "b", "d", "f", "l", "z", "zh", "h", "h", "kh", "j", "s", "sh", "shw", "r",
                        "k", "q", "k", "f", "l", "z", "h", "h", "j", "s", "r",
                        "k", "f", "l", "z", "h", "h", "j", "s", "r",
                        "b", "t", "th", "j", "kh", "khr", "d", "dh", "r", "z", "s", "sh", "shw", "dt", "jj",
                        "zh", "g", "f", "q", "k", "l", "m", "n", "h", "w",
                        "k", "q", "k", "b", "d", "f", "l", "z", "zh", "h", "h", "kh", "j", "s", "sh", "shw", "r",
                        "k", "q", "k", "f", "l", "z", "h", "h", "j", "s", "r",
                        "k", "f", "l", "z", "h", "h", "j", "s", "r",
                    },
                    new string[]{"aagh", "aagh", "ari", "ari", "aiid", "uuq", "ariid", "adih", "ateh", "adesh", "amiit", "it",
                        "iit", "akhmen", "akhmed", "ani", "abiib", "iib", "uuni", "iiz", "aqarii", "adiiq",
                    },
                    Array.Empty<string>(), new int[] { 1, 2, 3, 4 }, new double[] { 6, 5, 5, 1 }, 0.55, 0.65, 0.0, 0.15, ArabicSanityChecks, true).Register("Arabic Romanized");

        public static readonly LanguageGen INUKTITUT = new LanguageGen(
                    new string[] { "a", "a", "a", "a", "a", "aa", "aa", "aa", "aa", "i", "i", "i", "ii", "ii", "u", "u", "u", "uu", "uu", "ai", "ia", "iu", "ua", "ui" },
                    Array.Empty<string>(),
                    new string[]{"p", "t", "k", "q", "s", "l", "h", "v", "j", "g", "r", "m", "n",
                        "t", "t", "t", "t", "k", "k", "q", "q", "n", "n", "n", "n", "g", "l"},
                    new string[]{"pp", "tt", "kk", "pk", "tk", "gk", "kp", "kt", "kg", "pq", "tq", "gq", "ss", "ll", "rr", "mm",
                        "nn", "nng", "ng", "ng",
                        "ll", "nn", "nn", "nn",},
                    new string[] { "n", "t", "q", "k", "n", "t", "q", "k", "n", "t", "q", "k", "n", "t", "q", "k", "p", "s", "m", "g", "g", "ng", "ng", "ng" },
                    new string[]{"itut", "uit", "uq", "iuq", "iaq", "aq", "it", "aat", "aak", "aan", "ait", "ik", "uut", "un", "unnun",
                        "ung", "ang", "ing", "iin", "iit", "iik", "in",
                        "uq", "iaq", "aq", "ik", "it", "uit", "ut", "ut", "at", "un", "in"
                    },
                    Array.Empty<string>(), new int[] { 1, 2, 3, 4, 5 }, new double[] { 3, 4, 6, 5, 4 }, 0.45, 0.0, 0.0, 0.25, null, true).Register("Inuktitut");

        public static readonly LanguageGen NORSE = new LanguageGen(
                    new string[]{"a","a","a","á","á","au","e","e","e","é","é","ei","ey","i","i","í","í","y","y","ý","ý",
                        "o","o","o","ó","ó","u","u","u","ú","ú","æ","æ","æ","ö","ö",},
                    Array.Empty<string>(),
                    new string[]{"b","bl","br","bj","d","dr","dj","ð","ðl","ðr","f","fl","flj","fr","fn","fj","g","gn","gj","h",
                        "hj","hl","hr","hv","j","k","kl","kr","kn","kj","l","lj","m","mj","n","nj","p","pl","pr","pj","r",
                        "rj","s","sj","sl","sn","sp","st","str","skr","skj","sþ","sð","t","tj","v","vl","vr","vj","þ","þl","þr",

                        "d","f","fl","g","gl","gr","k","h","hr","n","k","l","m","mj","n","r","s","st","t","þ","ð",
                        "d","f","fl","g","gl","gr","k","h","hr","n","k","l","m","mj","n","r","s","st","t","þ","ð",
                        "d","f","fl","g","gl","gr","k","h","hr","n","k","l","m","mj","n","r","s","st","t","þ","ð",
                        "d","f","fl","g","gl","gr","k","h","hr","n","k","l","m","mj","n","r","s","st","t","þ","ð",
                        "d","f","fl","g","gl","gr","k","h","hr","n","k","l","m","mj","n","r","s","st","t","þ","ð",

                        "d","d","f","f","fl","g","g","g","gl","gr","k","h","hr","n","k","kl","l","n","r","r","s","st","t","t",
                        "d","d","f","f","fl","g","g","g","gl","gr","k","h","hr","n","k","kl","l","n","r","r","s","st","t","t",
                        "d","d","f","f","fl","g","g","g","gl","gr","k","h","hr","n","k","kl","l","n","r","r","s","st","t","t",
                        "d","d","f","f","fl","g","g","g","gl","gr","k","h","hr","n","k","kl","l","n","r","r","s","st","t","t",
                    },
                    new string[]{"bd","bf","bg","bk","bl","bp","br","bt","bv","bm","bn","bð","bj",
                        "db","df","dg","dk","dl","dp","dr","dt","dv","dm","dn","dð","dþ","dj","ndk","ndb","ndg","ndl","nds","nds",
                        "ðl","ðr","ðk","ðj","ðg","ðd","ðb","ðp","ðs",
                        "fb","fd","fg","fk","fl","fp","fr","fs","ft","fv","fm","fn","fð","fj",
                        "gb","gd","gf","gk","gl","gp","gr","gt","gv","gm","gn","gð","gj",
                        "h","hj","hl","hr","hv",
                        "kb","kd","kf","kp","kv","km","kn","kð","kl","kr","nkj","nkr","nkl",
                        "lbr","ldr","lfr","lg","lgr","lj","lkr","ln","ls","ltr","lv","lð","lðr","lþ",
                        "mb","md","mk","mg","ml","mp","mr","ms","mt","mv","mð","mþ","mj",
                        "nb","nl","np","nr","nv","nð","nþ","nj",
                        "ngl","ngb","ngd","ngk","ngp","ngt","ngv","ngm","ngð","ngþ","ngr",
                        "mbd","mbg","mbs","mbt","ldg","ldn","ldk","lds","rðn","rðl","gðs","gðr",
                        "pb","pd","pg","pk","pl","pr","ps","psj","pð","pj",
                        "rl","rbr","rdr","rg","rgr","rkr","rpr","rs","rts","rtr","rv","rj",
                        "sb","sbr","sd","sdr","sf","sfj","sg","skr","skl","sm","sn","str","sv","sð","sþ","sj",
                        "tr","tn","tb","td","tg","tv","tf","tj","tk","tm","tp",},
                    new string[]{"kk","ll","nn","pp","tt","kk","ll","nn","pp","tt",
                        "bs","ds","gs","x","rn","gn","gt","gs","ks","kt","nt","nd","nk","nt","ng","ngs","ns",
                        "ps","pk","pt","pts","lb","ld","lf","lk","lm","lp","lps","lt",
                        "rn","rb","rd","rk","rp","rt","rm","rð","rþ","sk","sp","st","ts",
                        "b","d","ð","f","g","gn","h","k","nk","l","m","n","ng","p","r","s","sp","st","sþ","sð","t","v","þ",
                        "b","d","ð","f","g","gn","h","k","nk","l","m","n","ng","p","r","s","sp","st","sþ","sð","t","v","þ",
                        "b","d","ð","f","g","gn","h","k","nk","l","m","n","ng","p","r","s","sp","st","sþ","sð","t","v","þ",

                        "b","b","b","d","d","d","f","f","f","g","g","k","k","nk","l","n","ng","p","p","r","r","r","s","s","st","t","t",
                        "b","b","b","d","d","d","f","f","f","g","g","k","k","nk","l","n","ng","p","p","r","r","r","s","s","st","t","t",
                        "b","b","b","d","d","d","f","f","f","g","g","k","k","nk","l","n","ng","p","p","r","r","r","s","s","st","t","t",
                    },
                    new string[]{"etta","eþa","uinn","ing","ard","eign","ef","efs","eg","ir","ir","ir","ir","ír","ír","ar","ar",
                        "ar","ár","or","or","ór","ör","on","on","ón","onn","unn","ung","ut","ett","att","ot"},
                    Array.Empty<string>(), new int[] { 1, 2, 3, 4, 5 }, new double[] { 5, 5, 4, 3, 1 }, 0.25, 0.5, 0.0, 0.08, GenericSanityChecks, true).Register("Norse");

        public static readonly LanguageGen NAHUATL = new LanguageGen(
                    new string[] { "a", "a", "a", "a", "a", "a", "a", "i", "i", "i", "i", "i", "o", "o", "o", "e", "e", "eo", "oa", "ea" },
                    Array.Empty<string>(),
                    new string[]{"ch", "c", "h", "m", "l", "n", "p", "t", "tl", "tz", "x", "y", "z", "hu", "cu",
                        "l", "l", "l", "p", "p", "t", "t", "t", "t", "t", "tl", "tl", "tz", "z", "x", "hu"},
                    new string[]{"zp", "ztl", "zc", "zt", "zl", "ct", "cl", "pl", "mt", "mc", "mch", "cz", "tc", "lc",
                        "hu", "hu", "hu", "cu"},
                    new string[]{
                        "ch", "c", "h", "m", "l", "n", "p", "t", "tl", "tz", "x", "y", "z",
                        "l", "l", "l", "l", "p", "t", "t", "t", "tl", "tl", "tz", "tz", "z", "x"
                    },
                    new string[]{"otl", "eotl", "ili", "itl", "atl", "atli", "oca", "itli", "oatl", "al", "ico", "acual",
                        "ote", "ope", "oli", "ili", "acan", "ato", "atotl", "ache", "oc", "aloc", "ax", "itziz", "iz"
                    },
                    Array.Empty<string>(), new int[] { 1, 2, 3, 4, 5, 6 }, new double[] { 3, 4, 5, 4, 3, 1 }, 0.3, 0.2, 0.0, 0.3, GenericSanityChecks, true)
                    .AddModifiers(new Modifier("c([ie])", "qu$1"),
                            new Modifier("z([ie])", "c$1")).Register("Nahuatl");

        public static readonly LanguageGen MONGOLIAN = new LanguageGen(
                    new string[]{"a", "a", "a", "a", "a", "a", "a", "aa", "aa", "e", "i", "i", "i", "i", "i", "i", "i", "i", "ii",
                        "o", "o", "o", "o", "oo", "u", "u", "u", "u", "u", "u", "u", "u", "uu", "uu", "ai", "ai"},
                    Array.Empty<string>(),
                    new string[]{"g", "m", "n", "g", "m", "n", "g", "m", "n", "n", "n", "ch", "gh", "ch", "gh", "gh", "j", "j", "j", "j",
                        "s", "s", "s", "t", "ts", "kh", "r", "r", "l", "h", "h", "h", "h", "h", "b", "b", "b", "b", "z", "z", "y", "y"},
                    Array.Empty<string>(),
                    new string[]{"g", "m", "n", "g", "m", "n", "g", "m", "n", "n", "n", "ch", "gh", "ch", "gh", "gh", "gh", "j", "j", "j",
                        "s", "s", "s", "t", "ts", "kh", "r", "r", "l", "h", "h", "h", "h", "h", "b", "b", "b", "b", "z", "z", "g", "n",
                        "g", "m", "n", "g", "m", "n", "g", "m", "n", "n", "n", "ch", "gh", "ch", "gh", "gh", "gh", "j", "j", "j", "n",
                        "s", "s", "s", "t", "ts", "kh", "r", "r", "l", "h", "h", "h", "h", "h", "b", "b", "b", "b", "z", "z", "y", "y",
                        "ng", "ng", "ng", "ngh", "ngh", "lj", "gch", "sd", "rl", "bl", "sd", "st", "md", "mg", "gd", "gd",
                        "sv", "rg", "rg", "mr", "tn", "tg", "ds", "dh", "dm", "gts", "rh", "lb", "gr", "gy", "rgh"},
                    new string[]{"ei", "ei", "ei", "uulj", "iig", "is", "is", "an", "aan", "iis", "alai", "ai", "aj", "ali"
                    },
                    Array.Empty<string>(), new int[] { 1, 2, 3, 4 }, new double[] { 5, 9, 3, 1 }, 0.3, 0.2, 0.0, 0.07, null, true).Register("Mongolian");

        public static readonly LanguageGen GOBLIN = new LanguageGen(
                    new string[]{"a", "a", "a", "a",
                        "e", "e",
                        "i", "i", "i",
                        "o", "o", "o", "o",
                        "u", "u", "u", "u", "u", "u", "u",
                    },
                    Array.Empty<string>(),
                    new string[]{"b", "g", "d", "m", "h", "n", "r", "v", "sh", "p", "w", "y", "f", "br", "dr", "gr", "pr", "fr",
                        "br", "dr", "gr", "pr", "fr", "bl", "dw", "gl", "gw", "pl", "fl", "hr",
                        "b", "g", "d", "m", "h", "n", "r", "b", "g", "d", "m", "h", "n", "r",
                        "b", "g", "d", "m", "r", "b", "g", "d", "r",
                    },
                    new string[]{
                        "br", "gr", "dr", "pr", "fr", "rb", "rd", "rg", "rp", "rf",
                        "br", "gr", "dr", "rb", "rd", "rg",
                        "mb", "mg", "md", "mp", "mf", "bm", "gm", "dm", "pm", "fm",
                        "mb", "mg", "md", "bm", "gm", "dm",
                        "bl", "gl", "dw", "pl", "fl", "lb", "ld", "lg", "lp", "lf",
                        "bl", "gl", "dw", "lb", "ld", "lg",
                        "nb", "ng", "nd", "np", "nf", "bn", "gn", "dn", "pn", "fn",
                        "nb", "ng", "nd", "bn", "gn", "dn",
                        "my", "gy", "by", "py", "mw", "gw", "bw", "pw",
                        "bg", "gb", "bd", "db", "bf", "fb",
                        "gd", "dg", "gp", "pg", "gf", "fg",
                        "dp", "pd", "df", "fd",
                        "pf", "fp",
                        "bg", "gb", "bd", "db", "gd", "dg",
                        "bg", "gb", "bd", "db", "gd", "dg",
                        "bg", "gb", "bd", "db", "gd", "dg",
                        "bg", "gb", "bd", "db", "gd", "dg",
                        "bg", "gb", "bd", "db", "gd", "dg",
                    },
                    new string[]{
                        "b", "g", "d", "m", "n", "r", "sh", "p", "f",
                        "b", "g", "d", "m", "n", "r", "b", "g", "d", "m", "n", "r", "sh",
                        "b", "g", "d", "m", "r", "b", "g", "d", "r",
                        "rb", "rd", "rg", "rp", "rf", "lb", "ld", "lg", "lp", "lf",
                    },
                    Array.Empty<string>(),
                    Array.Empty<string>(), new int[] { 1, 2, 3, 4 }, new double[] { 3, 7, 5, 1 }, 0.1, 0.15, 0.0, 0.0, GenericSanityChecks, true).Register("Goblin");

        public static readonly LanguageGen ELF = new LanguageGen(
                    new string[]{"a", "a", "a", "e", "e", "e", "i", "i", "o", "a", "a", "a", "e", "e", "e", "i", "i", "o",
                        "a", "a", "a", "e", "e", "e", "i", "i", "o", "a", "a", "a", "e", "e", "e", "i", "i", "o",
                        "a", "a", "e", "e", "i", "o", "a", "a", "a", "e", "e", "e", "i", "i", "o",
                        "ai", "ai", "ai", "ea", "ea", "ea", "ia", "ae"
                    },
                    new string[]{
                        "ai", "ai", "ae", "ea", "ia", "ie",
                        "â", "â", "ai", "âi", "aî", "aï", "î", "î", "ï", "ï", "îe", "iê", "ïe", "iê",
                        "e", "ë", "ë", "ëa", "ê", "êa", "eâ", "ei", "eî", "o", "ô",
                        "a", "a", "a", "e", "e", "e", "i", "i", "o", "a", "a", "a", "e", "e", "e", "i", "i", "o",
                        "a", "a", "e", "e", "i", "o", "a", "a", "a", "e", "e", "e", "i", "i", "o",
                        "ai", "ai", "ai", "ai", "ai", "ei", "ei", "ei", "ea", "ea", "ea", "ea",
                        "ie", "ie", "ie", "ie", "ie", "ia", "ia", "ia", "ia"
                    },
                    new string[]{"l", "r", "n", "m", "th", "v", "s", "sh", "z", "f", "p", "h", "y", "c",
                        "l", "r", "n", "m", "th", "v", "f", "y",
                        "l", "r", "n", "m", "th", "v", "f",
                        "l", "r", "n", "th", "l", "r", "n", "th",
                        "l", "r", "n", "l", "r", "n", "l", "r", "n",
                        "pl", "fy", "ly", "cl", "fr", "pr", "qu",
                    },
                    new string[] { "rm", "ln", "lv", "lth", "ml", "mv", "nv", "vr", "rv", "ny", "mn", "nm", "ns", "nth" },
                    new string[]{
                        "l", "r", "n", "m", "th", "s",
                        "l", "r", "n", "th", "l", "r", "n", "th",
                        "l", "r", "n", "l", "r", "n", "l", "r", "n",
                        "r", "n", "r", "n", "r", "n", "n", "n", "n", "n"
                    },
                    Array.Empty<string>(),
                    Array.Empty<string>(), new int[] { 1, 2, 3, 4, 5 }, new double[] { 3, 6, 6, 3, 1 }, 0.4, 0.3, 0.0, 0.0, GenericSanityChecks, true).Register("Elf");

        public static readonly LanguageGen DEMONIC = new LanguageGen(
                    new string[]{"a", "a", "a", "a",
                        "e",
                        "i", "i",
                        "o", "o", "o", "o", "o",
                        "u", "u", "u", "u", "u",
                    },
                    Array.Empty<string>(),
                    new string[]{
                        "b", "bh", "d", "dh", "t", "tl", "ts", "k", "ch", "kh", "g", "gh", "f", "x", "s", "sh", "z", "r", "v", "y",
                        "br", "bhr", "dr", "dhr", "tr", "tsr", "kr", "khr", "gr", "ghr", "fr", "shr", "vr",
                        "bl", "bhl", "tsl", "kl", "chl", "khl", "gl", "ghl", "fl", "sl", "zl", "vl",
                        "dz", "chf", "sf", "shf", "zv", "st", "sk",
                        "t", "t", "t", "ts", "ts", "k", "k", "k", "kh", "kh", "kh", "kh", "khr", "kl", "kl", "kr", "kr",
                        "z", "z", "z", "v", "v", "v", "zv", "zv", "vr", "vr", "vl", "vl", "dz", "sk", "sk", "sh", "shr",
                        "x", "x", "x", "gh", "gh", "ghr",
                        "t", "t", "t", "ts", "ts", "k", "k", "k", "kh", "kh", "kh", "kh", "khr", "kl", "kl", "kr", "kr",
                        "z", "z", "z", "v", "v", "v", "zv", "zv", "vr", "vr", "vl", "vl", "dz", "sk", "sk", "sh", "shr",
                        "x", "x", "x", "gh", "gh", "ghr",
                        "t", "t", "t", "ts", "ts", "k", "k", "k", "kh", "kh", "kh", "kh", "khr", "kl", "kl", "kr", "kr",
                        "z", "z", "z", "v", "v", "v", "zv", "zv", "vr", "vr", "vl", "vl", "dz", "sk", "sk", "sh", "shr",
                        "x", "x", "x", "gh", "gh", "ghr",
                    },
                    Array.Empty<string>(),
                    new string[]{
                        "b", "bh", "d", "dh", "t", "lt", "k", "ch", "kh", "g", "gh", "f", "x", "s", "sh", "z", "r",
                        "b", "bh", "d", "dh", "t", "lt", "k", "ch", "kh", "g", "gh", "f", "x", "s", "sh", "z", "r",
                        "b", "bh", "d", "dh", "t", "lt", "k", "ch", "kh", "g", "gh", "f", "x", "s", "sh", "z", "r",
                        "b", "bh", "d", "dh", "t", "lt", "k", "ch", "kh", "g", "gh", "f", "x", "s", "sh", "z", "r",
                        "rb", "rbs", "rbh", "rd", "rds", "rdh", "rt", "rts", "rk", "rks", "rch", "rkh", "rg", "rsh", "rv", "rz",
                        "lt", "lts", "lk", "lch", "lkh", "lg", "ls", "lz", "lx",
                        "bs", "ds", "ts", "lts", "ks", "khs", "gs", "fs", "rs", "rx",
                        "bs", "ds", "ts", "lts", "ks", "khs", "gs", "fs", "rs", "rx",
                        "rbs", "rds", "rts", "rks", "rkhs", "rgs", "rfs", "rs", "rx",
                        "lbs", "lds", "lts", "lks", "lkhs", "lgs", "lfs",
                        "rdz", "rvz", "gz", "rgz", "vd", "kt",
                        "t", "t", "t", "rt", "lt", "k", "k", "k", "k", "k", "kh", "kh", "kh", "kh", "kh", "rkh", "lk", "rk", "rk",
                        "z", "z", "z", "z", "v", "rv", "rv", "dz", "ks", "sk", "sh",
                        "x", "x", "x", "gh", "gh", "gh", "rgh",
                        "ts", "ts", "ks", "ks", "khs",
                        "t", "t", "t", "rt", "lt", "k", "k", "k", "k", "k", "kh", "kh", "kh", "kh", "kh", "rkh", "lk", "rk", "rk",
                        "z", "z", "z", "z", "v", "rv", "rv", "dz", "ks", "sk", "sh",
                        "x", "x", "x", "gh", "gh", "gh", "rgh",
                        "ts", "ts", "ks", "ks", "khs",
                        "t", "t", "t", "rt", "lt", "k", "k", "k", "k", "k", "kh", "kh", "kh", "kh", "kh", "rkh", "lk", "rk", "rk",
                        "z", "z", "z", "z", "v", "rv", "rv", "dz", "ks", "sk", "sh",
                        "x", "x", "x", "gh", "gh", "gh", "rgh",
                        "ts", "ts", "ks", "ks", "khs",
                    },
                    Array.Empty<string>(),
                    new string[] { "'" }, new int[] { 1, 2, 3 }, new double[] { 6, 7, 3 }, 0.05, 0.08, 0.11, 0.0, null, true).Register("Demonic");

        public static readonly LanguageGen INFERNAL = new LanguageGen(
                    new string[]{
                        "a", "a", "a", "à", "á", "â", "ä",
                        "e", "e", "e", "e", "e", "e", "e", "e", "è", "é", "ê", "ë",
                        "i", "i", "i", "i", "ì", "í", "î", "ï",
                        "o", "o", "ò", "ó", "ô", "ö",
                        "u", "u", "ù", "ú", "û", "ü",
                    },
                    new string[] { "æ", "ai", "aî", "i", "i", "î", "ï", "ia", "iâ", "ie", "iê", "eu", "eû", "u", "u", "û", "ü" },
                    new string[]{"b", "br", "d", "dr", "h", "m", "z", "k", "l", "ph", "t", "n", "y", "th", "s", "sh",
                        "m", "m", "m", "z", "z", "l", "l", "l", "k", "k", "b", "d", "h", "h", "y", "th", "th", "s", "sh",
                    },
                    new string[]{
                        "mm", "mm", "mm", "lb", "dd", "dd", "dd", "ddr", "bb", "bb", "bb", "bbr", "lz", "sm", "zr",
                        "thsh", "lkh", "shm", "mh", "mh",
                    },
                    new string[]{
                        "b", "d", "h", "m", "n", "z", "k", "l", "ph", "t", "th", "s", "sh", "kh",
                        "h", "m", "n", "z", "l", "ph", "t", "th", "s",
                        "h", "h", "h", "m", "m", "n", "n", "n", "n", "n", "l", "l", "l", "l", "l", "t", "t", "t",
                        "th", "th", "s", "s", "z", "z", "z", "z",
                    },
                    new string[] { "ael", "im", "on", "oth", "et", "eus", "iel", "an", "is", "ub", "ez", "ath", "esh", "ekh", "uth", "ut" },
                    new string[] { "'" }, new int[] { 1, 2, 3, 4 }, new double[] { 3, 5, 9, 4 }, 0.75, 0.35, 0.17, 0.07, GenericSanityChecks, true).Register("Infernal");

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
            new string[]{"a", "a", "a", "a", "a", "a", "a", "ai", "ai", "ao", "ao", "ae", "ae", "e", "e", "e", "e",
                        "ea", "eo", "i", "i", "i", "i", "i", "i", "ia", "ie", "io", "o", "o", "o", "oa"},
            Array.Empty<string>(),
            new string[]{"c", "f", "h", "j", "l", "m", "n", "p", "q", "r", "s", "v", "w", "x", "y", "z",
                        "c", "h", "j", "l", "m", "n", "q", "r", "s", "v", "w", "x", "y", "z",
                        "h", "j", "l", "m", "n", "q", "r", "s", "v", "w", "x", "y", "z",
                        "hc", "hf", "hj", "hl", "hm", "hn", "hq", "hr", "hv", "hw", "hy", "hz",
                        "cr", "fr", "jr", "mr", "nr", "pr", "qr", "sr", "vr", "xr", "yr", "zr",
                        "cy", "fy", "jy", "my", "ny", "py", "qy", "ry", "sy", "vy", "xy", "zy",
                        "cl", "fl", "jl", "ml", "nl", "pl", "ql", "sl", "vl", "xl", "yl", "zl",
            },
            new string[]{
                        "cr", "fr", "jr", "mr", "nr", "pr", "qr", "sr", "vr", "xr", "yr", "zr",
                        "cy", "fy", "jy", "my", "ny", "py", "qy", "ry", "sy", "vy", "xy", "zy",
                        "cl", "fl", "jl", "ml", "nl", "pl", "ql", "sl", "vl", "xl", "yl", "zl",
                                    "jc", "lc", "mc", "nc", "qc", "rc", "sc",       "wc", "yc", "zc",
                        "cf",       "jf", "lf",       "nf", "qf", "rf", "sf", "vf", "wf", "yf", "zf",
                        "cj", "fj",       "lj", "mj", "nj", "qj", "rj", "sj",       "wj", "yj", "zj",
                        "cm", "fm", "jm", "lm",       "nm", "qm", "rm", "sm", "vm", "wm", "ym", "zm",
                        "cn", "fn", "jn", "ln", "mn",       "qn", "rn", "sn", "vn", "wn", "yn", "zn",
                        "cp", "fp", "jp", "lp", "mp", "np", "qp", "rp", "sp", "vp", "wp", "yp", "zp",
                        "cq",       "jq", "lq", "mq", "nq",       "rq", "sq",       "wq", "yq", "zq",
                        "cs", "fs", "js", "ls", "ms", "ns", "qs",             "vs", "ws", "ys", "zs",
                        "cv", "fv", "jv", "lv", "mv", "nv", "qv", "rv", "sv",       "wv", "yv", "zv",
                        "cw", "fw", "jw", "lw", "mw", "nw", "qw", "rw", "sw", "vw",       "yw", "zw",
                        "cx",       "jx", "lx", "mx", "nx", "qx", "rx",       "vx", "wx", "yx", "zx",
                        "cz", "fz",       "lz", "mz", "nz", "qz", "rz", "sz", "vz", "wz", "yz",
            },
            new string[]{
                        "c", "f", "h", "j", "l", "m", "n", "p", "q", "r", "s", "v", "w", "x", "y", "z",
                        "c", "h", "j", "l", "m", "n", "q", "r", "s", "v", "w", "x", "y", "z",
                        "h", "j", "l", "m", "n", "q", "r", "s", "v", "w", "x", "y", "z",
                        "hc", "hf", "hj", "hl", "hm", "hn", "hq", "hr", "hv", "hw", "hy", "hz",
            },
            Array.Empty<string>(),
            Array.Empty<string>(), new int[] { 1, 2, 3 }, new double[] { 1, 1, 1 }, 0.65, 0.6, 0.0, 0.0, null, true).Register("Alien A");

        public static readonly LanguageGen KOREAN_ROMANIZED = new LanguageGen(
                    new string[]{
                        "a", "ae", "ya", "yae", "eo", "e", "yeo", "ye", "o", "wa", "wae",
                        "oe", "yo", "u", "wo", "we", "wi", "yu", "eu", "i",  "ui",
                        "a", "a", "a", "i", "i", "i", "i", "o", "o", "o", "o", "u", "u", "u", "u",
                        "ae", "ya", "eo", "eo", "eu", "eu", "wa", "wae", "wo", "oe", "oe",
                        "yo", "yo", "yu", "yu", "eu",
                    },
                    Array.Empty<string>(),
                    new string[]{
                        "g", "n", "d", "r", "m", "b", "s", "j", "ch", "k", "t", "p", "h",
                        "g", "n", "d", "b", "p", "k", "j", "ch", "h",
                        "g", "n", "d", "b", "p", "k", "j", "h",
                        "g", "n", "p", "k", "j",
                        "g", "p", "k",
                        "g", "p", "k",
                    },
                    new string[]{
                        "g", "kg", "ngn", "kd", "ngn", "ngm", "kb", "ks", "kj", "kch", "k-k", "kt", "kp", "k",
                        "n", "n-g", "nn", "nd", "nn", "nm", "nb", "ns", "nj", "nch", "nk", "nt", "np", "nh",
                        "d", "tg", "nn", "td", "nn", "nm", "tb", "ts", "tj", "tch", "tk", "t-t", "tp", "t",
                        "r", "lg", "nn", "ld", "ll", "lm", "lb", "ls", "lj", "lch", "lk", "lt", "lp", "lh",
                        "m", "mg", "mn", "md", "mn", "mm", "mb", "ms", "mj", "mch", "mk", "mt", "mp", "mh",
                        "b", "pg", "mn", "pd", "mn", "mm", "pb", "ps", "pj", "pch", "pk", "pt", "p-p", "p",
                        "s", "tg", "nn", "td", "nn", "nm", "tb", "ts", "tj", "tch", "tk", "t-t", "tp", "t",
                        "ng-", "ngg", "ngn", "ngd", "ngn", "ngm", "ngb", "ngs", "ngj", "ngch", "ngk", "ngt", "ngp", "ngh",
                        "j", "tg", "nn", "td", "nn", "nm", "tb", "ts", "tj", "tch", "tk", "t-t", "tp", "ch",
                        "t", "t", "t", "j", "j", "j", "g", "g", "g", "g", "n", "n", "n", "n", "n", "ng", "ng", "ng",
                        "d", "d", "d", "b", "b",
                        "tt", "nn", "kk", "kk", "ks",
                        "h", "k", "nn", "t", "nn", "nm", "p", "hs", "ch", "tch", "tk", "tt", "tp", "t",
                        "kk", "pp", "ss", "tt", "jj", "ks", "nch", "nh", "r",
                        "r", "r", "r", "r", "r", "r", "r", "r", "r", "r", "r", "r",
                        "ngg", "ngn", "ngm", "ngj", "ngch", "ngk", "ngp",
                        "mg", "mch", "mk", "md", "mb", "mp",
                        "nj", "nch", "nd", "nk", "nb", "nj", "nch", "nd", "nk",
                        "kg", "kj", "kch"
                    },
                    new string[]{
                        "k", "n", "t", "l", "m", "p", "k", "ng", "h", "n", "n",
                        "k", "n", "t", "l", "m", "p", "k", "ng", "h", "t",
                    },
                    new string[]{"ul", "eul", "eol", "ol",  "il", "yeol", "yol", "uk", "euk", "eok", "aek", "ok", "ak",
                        "on", "ong", "eong", "yang", "yong", "yeong", "ung", "wong", "om", "am", "im", "yuh", "uh", "euh",
                        "ap", "yaep", "eop", "wep", "yeop"
                    },
                    new string[] { "-" },
                    new int[] { 1, 2, 3, 4 }, new double[] { 14, 9, 3, 1 }, 0.14, 0.24, 0.02, 0.09,
                    null, true).Register("Korean Romanized");

        public static readonly LanguageGen ALIEN_E = new LanguageGen(
                    new string[]{"a", "a", "a", "a", "a", "a", "aa", "aa",
                        "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "ee", "ee", "ee", "ee",
                        "i", "i", "i", "i", "i", "ii",
                        "o", "o", "o", "o",
                        "u", "u", "u"
                    },
                    Array.Empty<string>(),
                    new string[]{"t", "k", "c", "g", "z", "s", "d", "r", "ts",
                        "tr", "kr", "cr", "gr", "zr", "st", "sk", "dr",
                        "tq", "kq", "cq", "gq", "zq", "sq", "dq",
                        "tq", "kq", "cq", "gq", "zq", "sq", "dq",
                        "tq", "kq", "cq", "gq", "zq", "sq", "dq",
                        "t", "k", "c", "g", "r", "ts", "t", "k", "c", "g", "r", "ts",
                        "t", "k", "c", "g", "r", "ts", "t", "k", "c", "g", "r", "ts",
                        "t", "k", "c", "g", "r", "ts", "t", "k", "c", "g", "r", "ts",
                        "t", "k", "ts", "t", "k", "ts", "t", "k", "ts", "t", "k", "ts",
                        "t", "k", "ts", "t", "k", "ts", "t", "k", "ts", "t", "k", "ts",
                        "t", "k", "t", "k", "t", "k", "t", "k", "t", "k", "t", "k",
                        "tr", "kr", "st", "sk", "tq", "kq", "sq"
                    },
                    new string[]{
                        "tt", "kk", "cc", "gg", "zz", "dd", "s", "r", "ts",
                        "tr", "kr", "cr", "gr", "zr", "st", "sk", "dr",
                        "tq", "kq", "cq", "gq", "zq", "sq", "dq",
                        "tq", "kq", "cq", "gq", "zq", "sq", "dq",
                        "tq", "kq", "cq", "gq", "zq", "sq", "dq",
                        "tk", "kt", "tc", "ct", "gt", "tg", "zt", "tz", "td", "dt", "rt", "rtr", "tst",
                        "kc", "ck", "gk", "kg", "zk", "kz", "kd", "dk", "rk", "rkr", "tsk", "kts",
                        "gc", "cg", "zc", "cz", "cd", "dc", "rc", "rcr", "tsc", "cts",
                        "zg", "gz", "gd", "dg", "rg", "rgr", "tsg", "gts",
                        "zd", "dz", "rz", "rzr", "tsz", "zts",
                        "rd", "rdr", "tsd", "dts",
                        "tt", "tt", "tt", "tt", "tt", "tt",
                        "tt", "tt", "tt", "tt", "tt", "tt",
                        "kk", "kk", "kk", "kk", "kk", "kk",
                        "kk", "kk", "kk", "kk", "kk", "kk",
                        "kt", "tk", "kt", "tk", "kt", "tk", "kt", "tk",
                    },
                    new string[]{
                        "t", "k", "c", "g", "z", "s", "d", "r", "ts",
                        "t", "k", "t", "k", "t", "k", "ts",
                        "t", "k", "c", "g", "z", "s", "d", "r", "ts",
                        "t", "k", "t", "k", "t", "k", "ts",
                        "st", "sk", "sc", "sg", "sz", "ds",
                        "rt", "rk", "rc", "rg", "rz", "rd", "rts"
                    },
                    Array.Empty<string>(),
                    Array.Empty<string>(), new int[] { 1, 2, 3 }, new double[] { 5, 4, 2 }, 0.45, 0.0, 0.0, 0.0, null, true).Register("Alien E");

        public static readonly LanguageGen ALIEN_I = new LanguageGen(
                    new string[]{
                        "a", "a", "a", "a", "a", "a", "à", "á", "â", "ā", "ä",
                        "e", "e", "e", "e", "e", "e", "è", "é", "ê", "ē", "ë",
                        "i", "i", "i", "i", "i", "i", "i", "i", "ì", "í", "î", "ï", "ī",
                        "i", "i", "i", "i", "i", "i", "i", "i", "ì", "í", "î", "ï", "ī",
                        "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "ò", "ó", "ô", "ō", "ö",
                        "u", "u", "u", "u", "u", "u", "ù", "ú", "û", "ū", "ü",
                    },
                    Array.Empty<string>(),
                    new string[]{
                        "r", "l", "ch", "g", "z", "zh", "s", "sh", "th", "m", "n", "p", "b", "j", "v", "h", "r", "l",
                        "r", "l", "ch", "g", "z", "zh", "s", "sh", "th", "m", "n", "p", "b", "j", "v", "h", "r", "l",
                        "r", "l", "ch", "g", "z", "zh", "s", "sh", "th", "m", "n", "p", "b", "j", "v", "h", "r", "l",
                        "r", "r", "r", "r", "r", "l", "l", "l", "l", "l",
                        "gr", "gl", "zr", "zl", "sl", "shr", "thr", "mr", "nr", "pr", "pl", "br", "bl", "vr", "vl", "hr",
                        "zv", "sp", "zg"
                    },
                    new string[]{
                        "j", "h",
                    },
                    new string[]{
                        "r", "l", "ch", "g", "z", "zh", "s", "sh", "th", "m", "n", "p", "b", "v", "r", "l",
                        "th", "zh", "sh", "th", "zh", "sh", "lth", "lzh", "lsh", "rth", "rzh", "rsh",
                    },
                    Array.Empty<string>(),
                    new string[] { "'" }, new int[] { 1, 2, 3, 4 }, new double[] { 6, 9, 5, 1 }, 0.6, 0.4, 0.075, 0.0, null, true).Register("Alien I");

        public static readonly LanguageGen ALIEN_O = new LanguageGen(
                    new string[]{
                        "a", "e", "i", "o", "o", "o", "o", "u",
                        "aa", "ea", "ia", "oa", "oa", "oa", "ua", "ae", "ai", "ao", "ao", "ao", "au",
                        "ee", "ie", "oe", "oe", "oe", "ue", "ei", "eo", "eo", "eo", "eu",
                        "ii", "oi", "oi", "oi", "ui", "io", "io", "io", "iu",
                        "oo", "ou", "uo", "oo", "ou", "uo", "oo", "ou", "uo", "uu",
                        "aa", "ea", "ia", "oa", "oa", "oa", "ua", "ae", "ai", "ao", "ao", "ao", "au",
                        "ee", "ie", "oe", "oe", "oe", "ue", "ei", "eo", "eo", "eo", "eu",
                        "ii", "oi", "ui", "io", "io", "io", "iu",
                        "oo", "ou", "uo", "oo", "ou", "uo", "oo", "ou", "uo", "uu",
                        "aea", "aia", "aoa", "aoa", "aoa", "aua", "eae", "eie", "eoe", "eoe", "eoe", "eue",
                        "iai", "iei", "ioi", "ioi", "ioi", "iui", "uau", "ueu", "uiu", "uou",
                        "oao", "oeo", "oio", "ouo", "oao", "oeo", "oio", "ouo", "oao", "oeo", "oio", "ouo",
                        "aei", "aeo", "aeo", "aeo", "aeu", "aie", "aio", "aio", "aio", "aiu",
                        "aoe", "aoi", "aou", "aoe", "aoi", "aou", "aoe", "aoi", "aou", "aue", "aui", "auo", "auo", "auo",
                        "eai", "eao", "eao", "eao", "eau", "eia", "eio", "eio", "eio", "eiu",
                        "eoa", "eoi", "eou", "eoa", "eoi", "eou", "eoa", "eoi", "eou", "eua", "eui", "euo", "euo", "euo",
                        "iae", "iao", "iao", "iao", "iau", "iea", "ieo", "ieo", "ieo", "ieu",
                        "ioa", "ioe", "iou", "ioa", "ioe", "iou", "ioa", "ioe", "iou", "iua", "iue", "iuo", "iuo", "iuo",
                        "oae", "oai", "oau", "oea", "oei", "oeu", "oia", "oie", "oiu", "oua", "oue", "oui",
                        "oae", "oai", "oau", "oea", "oei", "oeu", "oia", "oie", "oiu", "oua", "oue", "oui",
                        "oae", "oai", "oau", "oea", "oei", "oeu", "oia", "oie", "oiu", "oua", "oue", "oui",
                        "uae", "uai", "uao", "uao", "uao", "uea", "uei", "ueo", "ueo", "ueo", "uia", "uie",
                        "uio", "uoa", "uoe", "uoi", "uio", "uoa", "uoe", "uoi", "uio", "uoa", "uoe", "uoi",
                    },
                    Array.Empty<string>(),
                    new string[]{
                        "m", "n", "r", "w", "h", "v", "f", "l", "y",
                        "m", "n", "r", "w", "h", "v", "f", "l", "y",
                        "m", "n", "r", "w", "h", "v", "f", "l", "y",
                        "m", "n", "r", "w", "h", "v", "f", "l", "y",
                        "m", "n", "r", "w", "h", "v", "f", "l", "y",
                        "hm", "hn", "hr", "hw", "hv", "hl", "hy",
                        "fm", "fn", "fr", "fw", "fv", "fl", "fy",
                        "mr", "vr", "ry"
                    },
                    new string[]{
                        "m", "n", "r", "w", "h", "v", "f", "l", "y",
                        "m", "n", "r", "w", "h", "v", "f", "l", "y",
                        "m", "n", "r", "w", "h", "v", "f", "l", "y",
                        "m", "n", "r", "w", "h", "v", "f", "l", "y",
                        "mm", "nn", "rr", "ww", "hh", "vv", "ff", "ll", "yy",
                        "mm", "nn", "rr", "ww", "hh", "vv", "ff", "ll", "yy",
                        "hm", "hn", "hr", "hw", "hv", "hl", "hy",
                        "fm", "fn", "fr", "fw", "fv", "fl", "fy",
                        "mr", "vr", "ry"
                    },
                    new string[]{
                        "m", "n", "r", "h", "v", "f", "l",
                        "m", "n", "r", "h", "v", "f", "l",
                        "m", "n", "r", "h", "v", "f", "l",
                        "rm", "rn", "rv", "rf", "rl",
                        "lm", "ln", "lv", "lf"
                    },
                    Array.Empty<string>(),
                    Array.Empty<string>(), new int[] { 1, 2, 3 }, new double[] { 3, 6, 4 }, 0.0, 0.55, 0.0, 0.0, null, true).Register("Alien O");

        // àáâãäåæāăąǻǽaèéêëēĕėęěeìíîïĩīĭįıiòóôõöøōŏőœǿoùúûüũūŭůűųuýÿŷỳ
        // çðþñýćĉċčďđĝğġģĥħĵķĺļľŀłńņňŋŕŗřśŝşšţťŵŷÿźżžșțẁẃẅ
        public static readonly LanguageGen ALIEN_U = new LanguageGen(
                    new string[]{
                        "a", "a", "a", "a", "ä", "i", "o", "o", "o", "ö", "u", "u", "u", "u", "u", "u", "ü", "ü"
                    },
                    Array.Empty<string>(),
                    new string[]{
                        "b", "b", "b", "b", "d", "d", "g", "g", "ġ", "h", "h", "h", "h", "ħ",
                        "l", "l", "l", "l", "ł", "m", "m", "m", "m", "m", "n", "n", "n", "n", "ñ", "ŋ", "p", "p", "p",
                        "q", "q", "r", "r", "r", "ŕ", "s", "s", "s", "s", "ś", "v", "v", "v", "v",
                        "w", "w", "w", "w", "ẃ", "y", "y", "y", "y", "ý"
                    },
                    new string[]{
                        "b", "b", "b", "b", "d", "d", "g", "g", "ġ", "h", "h", "h", "h", "ħ",
                        "l", "l", "l", "l", "ł", "m", "m", "m", "m", "m", "n", "n", "n", "n", "ñ", "ŋ", "p", "p", "p",
                        "q", "q", "r", "r", "r", "ŕ", "s", "s", "s", "s", "ś", "v", "v", "v", "v",
                        "w", "w", "w", "w", "ẃ", "y", "y", "y", "y", "ý"
                    },
                    new string[]{
                        "b", "b", "b", "b", "d", "d", "g", "g", "ġ",
                        "l", "l", "l", "l", "ł", "m", "m", "m", "m", "m", "n", "n", "n", "n", "ñ", "ŋ", "p", "p", "p",
                        "r", "r", "r", "ŕ", "s", "s", "s", "s", "ś", "v", "v", "v", "v",
                    },
                    new string[]{"emb", "embrid", "embraŋ", "eŋ", "eŋul", "eŋov", "eẃul", "eẃuld", "eẃulb",
                        "eviś", "evim", "ełurn", "ełav", "egiġ", "ergiġ", "elgiġ", "eŕu", "eŕup", "eŕulm", "eŕuv",
                        "eħul", "eħid", "eħiŋ", "eyü", "eyür", "eyürl", "eyüld", "eyüns", "eqä", "eqäp", "eqäġ",
                        "esu", "esumb", "esulg", "esurl", "eśo", "eśold", "eśolg", "eśu", "eśur", "eśuŋ",
                        "eñu", "eñuns", "eñurn", "eño", "eñolb", "eñols"
                    },
                    new string[] { "'" }, new int[] { 1, 2, 3, 4, 5 }, new double[] { 3, 4, 7, 5, 2 }, 0.4, 0.15, 0.06, 0.5, null, true).Register("Alien U");

        public static readonly LanguageGen DRAGON = new LanguageGen(
                    new string[]{
                        "a", "a", "a", "e", "e", "i", "i", "o", "o", "u",
                        "a", "a", "a", "e", "e", "i", "i", "o", "o", "u",
                        "a", "a", "a", "e", "e", "i", "i", "o", "o", "u",
                        "a", "a", "a", "e", "e", "i", "i", "o", "o", "u",
                        "a", "a", "a", "a", "a", "a", "e", "i", "o",
                        "ai", "ai", "aa", "ae", "au", "ea", "ea", "ea",
                        "ia", "ia", "ie", "io", "io", "oa", "ou"
                    },
                    new string[]{
                        "aa", "aa", "aa", "ai", "ae", "ae", "ae", "au", "au",
                        "ea", "ea", "eo", "eo",
                        "ii", "ii", "ia", "ia", "ia", "ia", "ie", "ie", "ie", "io", "io", "io",
                        "oa", "ou", "ou", "ou", "ou"
                    },
                    new string[]{
                        "ch", "d", "f", "g", "h", "k", "l", "m", "n", "p", "r", "t", "th", "v", "w", "y", "z",
                        "ch", "d", "f", "g", "h", "k", "l", "m", "n", "p", "r", "t", "th", "v", "w", "y", "z",
                        "d", "f", "g", "h", "k", "l", "m", "n", "r", "t", "th", "v", "z",
                        "d", "f", "g", "h", "k", "l", "n", "r", "t", "th", "v", "z",
                        "d", "f", "g", "h", "l", "k", "l", "n", "r", "t", "th", "v", "z",
                        "d", "g", "h", "k", "l", "n", "r", "t", "th", "v", "z",
                        "d", "g", "h", "k", "l", "n", "r", "t", "th", "v", "z",
                        "d", "g", "k", "l", "r", "t",
                        "d", "g", "k", "l", "r", "t",
                        "d", "g", "k", "l", "r", "t",
                        "k", "k", "t", "t", "v",
                        "k", "k", "t", "t", "th",
                        "k", "k", "t", "t", "ch",
                        "dr", "fr", "gr", "hr", "kr", "tr", "thr",
                        "dr", "fr", "gr", "hr", "kr", "tr", "thr",
                        "dr", "fr", "gr", "hr", "kr", "tr", "thr",
                        "dr", "gr", "hr", "kr", "tr", "thr", "dr", "gr", "kr", "tr",
                        "dr", "gr", "hr", "kr", "tr", "thr", "dr", "gr", "kr", "tr",
                    },
                    new string[]{
                        "rch", "rd", "rg", "rk", "rm", "rn", "rp", "rt", "rth", "rv", "rw", "rz",
                        "rch", "rd", "rg", "rk", "rm", "rn", "rp", "rt", "rth", "rv", "rw", "rz",
                        "rdr", "rgr", "rkr", "rtr", "rthr",
                        "lk", "lt", "lv", "lz",
                        "ng", "nk", "ng", "nk", "ng", "nk", "ng", "nk", "nt", "nth", "nt", "nth", "nt", "nth", "nd",
                        "ngr", "nkr", "ntr", "nthr",
                        "dh", "gh", "lh", "mh", "nh", "rh",
                        "dch", "dg", "dk", "dth", "dv", "dz",
                        "kch", "kg", "kd", "kth", "kv", "kz",
                        "gch", "gd", "gk", "gth", "gv", "gz",
                        "tch", "tg", "tk", "ty", "tv", "tz",
                        "zm", "zn", "zk", "zv", "zt", "zg", "zd",

                        "ch", "d", "f", "g", "h", "k", "l", "m", "n", "p", "r", "t", "th", "v", "w", "y", "z",
                        "ch", "d", "f", "g", "h", "k", "l", "m", "n", "p", "r", "t", "th", "v", "w", "y", "z",
                        "d", "f", "g", "h", "k", "l", "m", "n", "r", "t", "th", "v", "z",
                        "d", "f", "g", "h", "k", "l", "n", "r", "t", "th", "v", "z",
                        "d", "f", "g", "h", "k", "l", "n", "r", "t", "th", "v",
                        "d", "g", "k", "l", "n", "r", "t", "th", "v",
                        "d", "g", "k", "l", "n", "r", "t", "th", "v",
                        "d", "g", "k", "l", "r", "t",
                        "d", "g", "k", "l", "r", "t",
                        "d", "g", "k", "l", "r", "t",
                        "k", "k", "t", "t", "r",
                        "k", "k", "t", "t", "r",
                        "k", "k", "t", "t", "r",
                        "dr", "fr", "gr", "hr", "kr", "tr", "thr",
                        "dr", "fr", "gr", "hr", "kr", "tr", "thr",
                        "dr", "fr", "gr", "hr", "kr", "tr", "thr",
                        "dr", "gr", "hr", "kr", "tr", "thr", "dr", "gr", "kr", "tr",
                        "dr", "gr", "hr", "kr", "tr", "thr", "dr", "gr", "kr", "tr",

                    },
                    new string[]{
                        "z", "z", "z", "t", "t", "t", "n", "r", "k", "th"
                    },
                    new string[] { "iamat", "at", "ut", "ok", "iok", "ioz", "ez", "ion", "ioth", "aaz", "iel" },
                    Array.Empty<string>(), new int[] { 2, 3, 4, 5 }, new double[] { 2, 7, 10, 3 }, 0.14, 0.04, 0.0, 0.11, GenericSanityChecks, true).Register("Dragon");

        public static readonly LanguageGen KOBOLD = new LanguageGen(
            DRAGON.OpeningVowels, DRAGON.MidVowels, DRAGON.OpeningConsonants, DRAGON.MidConsonants, DRAGON.ClosingConsonants,
            new string[] { "ik", "ak", "ek", "at", "it", "ik", "ak", "ek", "at", "it", "ik", "ak", "ek", "at", "it", "et", "ut", "ark", "irk", "erk" },
            Array.Empty<string>(), new int[] { 1, 2, 3 }, new double[] { 5, 11, 1 },
            0.1, 0.0, 0.0, 0.22, GenericSanityChecks, true).Register("Kobold");

        public static readonly LanguageGen INSECT = new LanguageGen(
                    new string[]{
                        "a", "a", "a", "a", "a", "a",
                        "e", "e", "e", "e",
                        "i", "i", "i", "i", "i", "i", "i",
                        "o", "o", "o",
                        "u", "u",
                    },
                    Array.Empty<string>(),
                    new string[]{"t", "k", "g", "sh", "s", "x", "r", "ts",
                        "tr", "kr", "gr", "shr", "st", "sk",
                        "tr", "kr", "st", "sk", "tr", "kr", "st", "sk",
                        "t", "k", "g", "sh", "s", "x", "r", "ts",
                        "t", "k", "r", "ts", "ts",
                        "t", "k", "r", "tr", "kr", "t", "k", "r", "tr", "kr", "t", "k", "r", "tr", "kr",
                        "t", "k", "t", "k", "t", "k", "t", "k", "t", "k", "t", "k",
                    },
                    new string[]{
                        "rr","rr","rr","rr","rr","rr","rr","rr","rr","rr",
                        "rt", "rk", "rg", "rsh", "rs", "rx", "rts",
                        "xt", "xk", "xg", "xr",
                        "sts", "skr", "str", "sks"
                    },
                    new string[]{
                        "t", "k", "g", "sh", "s", "x", "r", "ts", "t", "k", "g", "sh", "s", "x", "r", "ts",
                        "rt", "rk", "rg", "rsh", "rs", "rx", "rts",
                        "t", "t", "t", "t", "t", "t", "k", "k", "k", "k", "k", "k", "x", "x", "rr", "rr", "rr"
                    },
                    Array.Empty<string>(),
                    Array.Empty<string>(), new int[] { 1, 2, 3, 4 }, new double[] { 6, 4, 2, 1 }, 0.3, 0.1, 0.0, 0.0, null, true).Register("Insect");

        public static readonly LanguageGen MAORI = new LanguageGen(
                    new string[]{"a", "a", "a", "a", "a", "a", "ā", "ā", "e", "e", "e", "i", "i", "i", "i",
                        "o", "o", "o", "o", "o", "u", "u",
                        "a", "a", "a", "a", "a", "a", "ā", "ā", "e", "e", "e", "i", "i", "i", "i",
                        "o", "o", "o", "o", "o", "u", "u",
                        "a", "a", "a", "a", "a", "a", "ā", "ā", "e", "e", "e", "i", "i", "i", "i",
                        "o", "o", "o", "o", "o", "u", "u",
                        "a", "a", "a", "a", "a", "a", "ā", "ā", "e", "e", "e", "i", "i", "i", "i",
                        "o", "o", "o", "o", "o", "u", "u",
                        "a", "a", "a", "a", "a", "a", "ā", "ā", "e", "e", "e", "i", "i", "i", "i",
                        "o", "o", "o", "o", "o", "u", "u",
                        "a", "a", "a", "a", "a", "a", "ā", "ā", "e", "e", "e", "i", "i", "i", "i",
                        "o", "o", "o", "o", "o", "u", "u",
                        "a", "a", "a", "a", "a", "a", "ā", "ā", "e", "e", "e", "i", "i", "i", "i",
                        "o", "o", "o", "o", "o", "u", "u",
                        "a", "a", "a", "a", "a", "a", "ā", "ā", "e", "e", "e", "i", "i", "i", "i",
                        "o", "o", "o", "o", "o", "u", "u",
                        "a", "a", "a", "a", "a", "a", "ā", "ā", "e", "e", "e", "i", "i", "i", "i",
                        "o", "o", "o", "o", "o", "u", "u",
                        "ae", "ai", "ai", "ai", "ao", "ao", "ao", "ao", "au",
                        "ae", "ai", "ai", "ai", "ao", "ao", "ao", "ao", "au",
                        "āe", "āi", "āi", "āi", "āo", "āo", "āo", "āo", "āu", "oi", "oe", "ou",
                        "ae", "ai", "ai", "ai", "ao", "ao", "ao", "ao", "au",
                        "ae", "ai", "ai", "ai", "ao", "ao", "ao", "ao", "au",
                        "āe", "āi", "āi", "āi", "āo", "āo", "āo", "āo", "āu", "oi", "oe", "ou",
                        "āa", "āoi", "āoe", "āou",
                        "āa", "āoi", "āoe", "āou",
                        "ea", "ei", "ei", "ei", "eo", "eo", "eo", "eu", "eae", "eai", "eao", "eā", "eāe", "eāi", "eāo", "eoi", "eoe", "eou",
                        "ia", "ia", "ie", "io", "io", "iu", "iae", "iai", "iao", "iau", "iā", "iāe", "iāi", "iāo", "iāu", "ioi", "ioe", "iou",
                        "oa", "oa", "oa", "oa", "oae", "oai", "oao", "oau", "oā", "oā", "oāe", "oāi", "oāo", "oāu",
                        "oa", "oa", "oa", "oa", "oae", "oai", "oao", "oau", "oā", "oā", "oāe", "oāi", "oāo", "oāu",
                        "ua", "ue", "ui", "uo", "uae", "uai", "uao", "uau", "uā", "uāe", "uāi", "uāo", "uāu", "uoi", "uoe", "uou",
                        "aea", "aea", "aei", "aei", "aei", "aeo", "aeo", "aeo", "aeu",
                        "aia", "aia", "aia", "aia", "aie", "aio", "aio", "aiu",
                        "aoa", "aoa",
                        "aua", "aua", "aue", "aue", "aue", "aui", "aui", "auo",
                        "āea", "āea", "āei", "āei", "āei", "āeo", "āeo", "āeo", "āeu",
                        "āia", "āia", "āia", "āia", "āie", "āio", "āio", "āiu",
                        "āoa", "āoa",
                        "āua", "āua", "āue", "āue", "āue", "āui", "āui", "āuo",
                    },
                    Array.Empty<string>(),
                    new string[]{"h", "h", "k", "k", "m", "m", "m", "m", "n", "n", "p", "p",
                        "r", "r", "r", "r", "r", "t", "t", "t", "t", "w", "w", "ng", "wh", "wh", "wh",
                        "h", "k", "m", "m", "m", "m", "n", "n", "p", "p",
                        "r", "r", "r", "r", "r", "t", "t", "t", "t", "w", "w", "wh", "wh", "wh"
                    },
                    new string[]{"h", "k", "k", "k", "m", "n", "n", "n", "p", "p", "p", "p", "p",
                        "r", "r", "r", "t", "t", "t", "w", "ng", "ng", "ng", "ng", "wh", "wh"
                    },
                    new string[] { "" },
                    Array.Empty<string>(),
                    Array.Empty<string>(), new int[] { 1, 2, 3, 4 }, new double[] { 5, 5, 4, 2 }, 0.2, 1.0, 0.0, 0.0, GenericSanityChecks, true).Register("Maori");


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

        public static readonly LanguageGen NORSE_SIMPLIFIED = NORSE.AddModifiers(Modifier.ReplacementTable(
                "á", "a",
                "é", "e",
                "í", "i",
                "ý", "y",
                "ó", "o",
                "ú", "u",
                "æ", "ae",
                "ö", "ou",
                "([^aeiou])jy", "$1yai",
                "([^aeiou])j(?:[aeiouy]+)", "$1yo",
                "s([ðþ])", "st",
                "\\bf[ðþ]", "fr",
                "[ðþ]", "th")).Register("Norse Simplified");

        public static readonly LanguageGen HLETKIP = new LanguageGen(
                    new string[]{"a", "a", "a", "e", "e", "e", "e", "e", "i", "i", "i", "i",
                        "o", "o", "u", "u", "u", "u",},
                    Array.Empty<string>(),
                    new string[]{
                              "hf", "hl", "hm", "hn",                      "hr", "hs", "hv", "hw",  "hy", "hz",
                          "br", "kr", "fr", "mr", "nr", "pr", "khr", "shr", "zhr", "sr",       "vr", "thr", "zv", "zr",
                          "by", "ky", "fy", "my", "ny", "py", "khy", "shy", "zhy", "ry", "sy", "vy", "thy", "zy",
                          "bl", "kl", "fl", "ml", "nl", "pl", "khl", "shl", "zhl",       "sl", "vl", "thl", "lw", "zl",
                          "bf", "kf",       "mf", "nf", "pf",        "fsh", "shf", "fr", "sf", "fl", "fr",  "fw", "fz",
                          "bs", "ks", "fs", "ms", "ns", "ps", "skh", "shs", "khs",            "shv","shw",
                          "pkh", "psh", "pth", "pw", "tkh", "tsh", "tth", "tw", "sht", "bkh", "bsh", "bth", "bw",
                          "dkh", "dth", "dw", "dzh", "khg", "shg", "thg", "gw", "zhg", "khk", "thk", "kw",
                    },
                    new string[]{
                        "hf", "hl", "hm", "hn",                    "hr", "hs", "hv", "hw",  "hy", "hz",
                        "br", "kr", "fr", "mr", "nr", "pr", "khr", "shr", "zhr", "sr",       "vr", "thr", "zv", "zr",
                        "by", "ky", "fy", "my", "ny", "py", "khy", "shy", "zhy", "ry", "sy", "vy", "thy", "zy",
                        "bl", "kl", "fl", "ml", "nl", "pl", "khl", "shl", "zhl",       "sl", "vl", "thl", "lw", "zl",
                        "bf", "kf",       "mf", "nf", "pf",        "fsh", "shf", "fr", "sf", "fl", "fr",  "fw", "fz",
                        "bs", "ks", "fs", "ms", "ns", "ps", "skh", "shs", "khs",            "shv","shw",
                        "pkh", "psh", "pth", "pw", "tkh", "tsh", "tth", "tw", "bkh", "bsh", "bth", "bw",
                        "dkh", "dsh", "dth", "dw", "khg", "shg", "thg", "gw", "khk", "thk", "kw",
                        "rb", "rk", "rf", "rm", "rn", "rp", "rkh", "rsh", "rzh", "rh", "rv", "rw", "rz", "rl",
                        "lb", "lk", "lf", "lm", "ln", "lp", "lkh", "lsh", "lzh", "lh", "lv", "lw", "lz", "lr",
                        "sb", "sk", "sf", "sm", "sn", "sp", "skh", "gsh", "dzh", "sh", "sv", "sw", "sz", "ts", "st",
                        "mb", "md", "mk", "mf", "tm", "nm", "mp", "mkh", "msh", "mzh", "mh", "mv", "mw", "mt", "mz",
                        "nb", "nd", "nk", "nf", "tn", "mn", "np", "nkh", "nsh", "nzh", "nh", "nv", "nw", "nt", "nz",
                        "zb", "zd", "zk", "zf", "zt", "nz", "zp", "zkh", "zhz", "dz",  "hz", "zv", "zw", "tz",
                    },
                    Array.Empty<string>(),
                    new string[]{"ip", "ik", "id", "iz", "ir", "ikh", "ish", "is", "ith", "iv", "in", "im", "ib", "if",
                        "ep", "ek", "ed", "ez", "er", "ekh", "esh", "es", "eth", "ev", "en", "em", "eb", "ef",
                        "up", "ud", "uz", "ur", "ush", "us", "uth", "uv", "un", "um", "ub", "uf",
                    },
                    Array.Empty<string>(), new int[] { 1, 2, 3 }, new double[] { 1, 1, 1 }, 0.0, 0.4, 0.0, 1.0, null, true).Register("Hletkip");

        public static readonly LanguageGen ANCIENT_EGYPTIAN = new LanguageGen(
                    new string[]{"a", "a", "a", "a", "a", "aa", "e", "e", "e", "e", "e", "e", "e", "i", "i", "i",
                        "u", "u", "u",},
                    Array.Empty<string>(),
                    new string[]{
                        "b",
                        "p", "p", "p",
                        "f", "f", "f", "f", "f",
                        "m", "m", "m", "m", "m", "m",
                        "n", "n", "n", "n", "n",
                        "r", "r", "r", "r", "r", "r",
                        "h", "h", "h", "h", "h", "h", "h", "h",
                        "kh", "kh", "kh", "kh", "kh", "kh",
                        "z",
                        "s", "s", "s", "s", "s", "s", "s", "s",
                        "sh", "sh", "sh", "sh",
                        "k", "k", "k", "k", "k",
                        "g", "g", "g",
                        "t", "t", "t", "t", "t", "t",
                        "th", "th", "th",
                        "d", "d", "d",
                        "dj",
                        "w", "w", "w",
                        "pt"
                    },
                    new string[]{
                        "b",
                        "p", "p", "p", "pw", "pkh", "ps", "ps", "pt",
                        "f", "f", "f", "f", "f", "ft",
                        "m", "m", "m", "m", "m", "m", "mk", "nm",
                        "n", "n", "n", "n", "n", "nkh", "nkh", "nk", "nt", "ns",
                        "r", "r", "r", "r", "r", "r", "rs", "rt",
                        "h", "h", "h", "h", "h", "h", "h", "h",
                        "kh", "kh", "kh", "kh", "kh", "kh", "khm", "khm", "khw",
                        "z",
                        "s", "s", "s", "s", "s", "s", "s", "s", "st", "sk", "skh",
                        "sh", "sh", "sh", "sh", "shw",
                        "k", "k", "k", "k", "k", "kw",
                        "g", "g", "g",
                        "t", "t", "t", "t", "t", "t", "ts",
                        "th", "th", "th",
                        "d", "d", "d", "ds",
                        "dj",
                        "w", "w", "w",
                    },
                    new string[]{
                        "m", "n", "t", "s", "p", "sh", "m", "n", "t", "s", "p", "sh", "m", "n", "t", "s", "p", "sh",
                        "kh", "f"
                    },
                    new string[]{"amon", "amun", "ut", "epsut", "is", "is", "ipsis", "akhti", "eftu", "atsut", "amses"
                    },
                    new string[] { "-" }, new int[] { 1, 2, 3, 4 }, new double[] { 4, 7, 3, 2 }, 0.5, 0.4, 0.06, 0.09, null, true).Register("Ancient Egyptian");

        public static readonly LanguageGen CROW = new LanguageGen(
                    new string[]{"a", "a", "a", "a", "a","a", "a", "a","a", "a", "a", "á", "á", "aa", "aa", "áá", "áa",
                        "e", "e", "e", "e", "e", "e", "ee", "ée", "é", "éé",
                        "i", "i", "i", "i", "i", "i", "i", "i", "i", "i", "ii", "íí", "íi", "í",
                        "o", "o", "o", "o", "o", "o", "o", "oo", "óó", "óo", "ó",
                        "u", "u","u", "u","u", "u","u", "u", "u", "u", "uu", "úú", "úu", "ú",
                        "ia", "ua", "ia", "ua", "ia", "ua", "ia", "ua", "ía", "úa"
                    },
                    Array.Empty<string>(),
                    new string[]{
                        "b", "p", "s", "x", "k", "l", "m", "n", "d", "t", "h", "w", "ch", "sh",
                        "k", "k", "m", "k", "k", "m", "d", "s"},
                    new string[]{
                        "bb", "pp", "ss", "kk", "ll", "mm", "nn", "dd", "tt",
                        "kk", "kk", "mm", "kk", "kk", "mm", "dd", "ss",
                        "b", "p", "s", "x", "k", "l", "m", "n", "d", "t", "h", "w", "ch", "sh",
                        "k", "k", "m", "k", "k", "m", "d", "s",
                        "b", "p", "s", "x", "k", "l", "m", "n", "d", "t", "h", "w", "ch", "sh",
                        "k", "k", "m", "k", "k", "m", "d", "s",
                        "b", "p", "s", "x", "k", "l", "m", "n", "d", "t", "h", "w", "ch", "sh",
                        "k", "k", "m", "k", "k", "m", "d", "s",
                        "b", "p", "s", "x", "k", "l", "m", "n", "d", "t", "h", "w", "ch", "sh",
                        "k", "k", "m", "k", "k", "m", "d", "s",
                        "b", "p", "s", "x", "k", "l", "m", "n", "d", "t", "h", "w", "ch", "sh",
                        "k", "k", "m", "k", "k", "m", "d", "s",
                        "b", "p", "s", "x", "k", "l", "m", "n", "d", "t", "h", "w", "ch", "sh",
                        "k", "k", "m", "k", "k", "m", "d", "s"
                    },
                    new string[]{"b", "p", "s", "x", "k", "l", "m", "n", "d", "t", "h", "w", "ch", "sh",
                        "k", "k", "m", "k", "k", "m", "d", "s"
                    },
                    Array.Empty<string>(),
                    new string[] { "-" }, new int[] { 1, 2, 3, 4, 5 }, new double[] { 5, 7, 6, 4, 2 }, 0.4, 1.0, 0.12, 0.0, null, true).Register("Crow");

        public static readonly LanguageGen IMP = new LanguageGen(
                    new string[]{"a", "a", "a", "a", "a", "á", "á", "á", "aa", "aa", "aa", "aaa", "aaa", "aaa", "áá", "áá", "ááá", "ááá",
                        "e", "e", "e", "e", "e", "e",
                        "i", "i", "i", "i", "i", "i", "i", "i", "i", "i", "í", "í", "í", "í",
                        "ii", "ii", "ii", "iii", "iii", "iii", "íí", "íí", "ííí", "ííí",
                        "u", "u", "u", "u", "u", "u", "u", "u", "ú", "ú", "ú", "uu", "uu", "uu", "úú", "úú", "úúú", "úúú",
                        "ia", "ia", "ia", "ui", "ui"
                    },
                    Array.Empty<string>(),
                    new string[]{
                        "s", "k", "d", "t", "h", "f", "g", "r", "r", "r", "r", "gh", "ch",
                        "sk", "st", "skr", "str", "kr", "dr", "tr", "fr", "gr"
                    },
                    new string[]{
                        "s", "k", "d", "t", "h", "f", "g", "r", "r", "r", "r", "gh", "ch",
                        "sk", "st", "skr", "str", "kr", "dr", "tr", "fr", "gr"
                    },
                    new string[]{
                        "s", "k", "d", "t", "g", "gh", "ch"
                    },
                    Array.Empty<string>(),
                    new string[] { "-" }, new int[] { 1, 2, 3 }, new double[] { 7, 11, 4 }, 0.2, 0.5, 0.4, 0.0, null, true).Register("Imp");

        public static readonly LanguageGen MALAY = new LanguageGen(
                    new string[]{
                        "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "ai", "ai", "au",
                        "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e",
                        "i", "i", "i", "i", "i", "i", "i", "i", "ia", "ia",
                        "o", "o", "o", "o", "o", "o", "ou",
                        "u", "u", "u", "u", "u", "u", "u", "u", "u", "ua", "ua",},
                    Array.Empty<string>(),
                    new string[]{
                        "b", "b", "b", "b",
                        "ch",
                        "d", "d", "d", "d",
                        "f",
                        "g", "g",
                        "h", "h",
                        "j", "j", "j", "j",
                        "k", "k", "k", "k", "k", "k",
                        "kh",
                        "l", "l", "l", "l", "l", "l", "l",
                        "m", "m", "m", "m",
                        "n", "n", "n",
                        "p", "p", "p", "p", "p",
                        "r", "r",
                        "s", "s", "s", "s", "s",
                        "sh", "sh",
                        "t", "t", "t", "t",
                        "w",
                        "y",
                        "z",
                    },
                    new string[]{
                        "b", "b", "b", "b",
                        "ch",
                        "d", "d", "d", "d",
                        "f",
                        "g", "g",
                        "h", "h", "h", "h", "h",
                        "j", "j", "j",
                        "k", "k", "k", "k", "k", "k", "k", "k", "k",
                        "kn",
                        "kh",
                        "l", "l", "l", "l", "l", "l", "l",
                        "m", "m", "m", "m", "m", "m",
                        "n", "n", "n", "n", "n", "n", "n", "n", "n", "n",
                        "nt", "nt", "nj",
                        "ng", "ng", "ng", "ng",
                        "ngk","ngg",
                        "ny", "ny",
                        "p", "p", "p", "p", "p",
                        "r", "r", "r", "r", "r", "r", "r", "r",
                        "rb", "rd", "rg", "rk", "rs", "rt", "rn", "rn",
                        "s", "s", "s", "s", "s", "s",
                        "sh", "sh",
                        "t", "t", "t", "t", "t", "t",
                        "w",
                        "y",
                        "z",
                    },
                    new string[]{
                        "k", "k", "k", "k", "k", "k", "t", "t", "t", "n", "n", "n", "n", "n", "n", "n", "n",
                        "ng", "ng", "ng", "m", "m", "m", "s", "s", "l", "l", "l", "l", "l", "h", "h"
                    },
                    new string[]{"uk", "uk", "ok", "an", "at", "ul", "ang", "ih", "it", "is", "ung", "un", "ah"
                    },
                    Array.Empty<string>(), new int[] { 1, 2, 3 }, new double[] { 5, 3, 2 }, 0.2, 0.25, 0.0, 0.2, GenericSanityChecks, true).Register("Malay");
        public static readonly LanguageGen CELESTIAL = new LanguageGen(
                    new string[]{
                        "a", "a", "a", "a", "a", "a", "a", "e", "e", "e", "i", "i", "i", "i", "i", "o", "o", "o",
                        "a", "a", "a", "a", "a", "a", "a", "e", "e", "e", "i", "i", "i", "i", "i", "o", "o", "o",
                        "ă", "ă", "ĕ", "ĭ", "ŏ"
                    },
                    Array.Empty<string>(),
                    new string[]{
                        "l", "r", "n", "m", "v", "b", "d", "s", "th", "sh", "z", "h", "y", "w", "j",
                        "l", "r", "n", "m", "v", "b", "d", "s", "th", "sh", "z", "h", "y", "w", "j",
                        "l", "r", "n", "m", "v", "b", "d", "s", "th", "sh", "z", "h", "y", "w", "j",
                        "n", "m", "v", "s", "z", "h", "y", "w", "j",
                        "n", "m", "v", "s", "z", "h", "y", "w", "j",
                        "n", "m", "s", "h", "y", "j",
                        "n", "m", "s", "h", "y", "j",
                        "n", "m", "s", "h", "y", "j",
                        "h", "h", "h", "h", "h", "h", "h", "h",
                        "m", "m", "m", "m", "m", "m",
                        "ry", "ly", "by", "dy", "ny", "my", "vy", "by", "dy", "sy", "zy",
                        "bl", "br", "dr", "shl", "shr", "hr"
                    },
                    new string[]{
                        "j", "j", "j",
                        "mh", "mb", "md", "mr", "ms", "mz", "mv",
                        "nh", "nb", "nd", "nr", "ns", "nz", "nv",
                        "zh", "zb", "zd", "zr", "zv",
                        "bd", "db", "bm", "bn", "dm", "dn",
                        "ry", "ly", "by", "dy", "ny", "my", "vy", "by", "dy", "sy", "zy", "wy", "jy",
                        "bl", "br", "dr", "shl", "shr", "hr"
                    },
                    new string[]{
                        "l", "r", "n", "m", "v", "b", "d", "s", "th", "sh", "z",
                        "l", "r", "n", "m", "v", "b", "d", "s", "th", "sh",
                        "l", "r", "n", "m", "v", "b", "d", "th",
                        "l", "r", "n", "m", "b", "d", "th",
                        "r", "n", "m", "r", "n", "m", "r", "n", "m", "r", "n", "m", "r", "n", "m", "r", "n", "m",
                    },
                    new string[]{
                        "am", "an", "ar", "av", "em", "el", "ez", "eth", "ev", "es", "im", "id", "in", "oth", "om",
                        "ar", "el", "es", "im", "oth",
                        "ăyom", "ĕzra", "ĭdniv", "ŏlor", "evyăd", "iyĕr", "abĭl", "onrŏv"
                    },
                    new string[] { "'" }, new int[] { 1, 2, 3 }, new double[] { 5, 6, 2 }, 0.45, 0.1, 0.04, 0.14, GenericSanityChecks, true).Register("Celestial");

        public static readonly LanguageGen CHINESE_ROMANIZED = new LanguageGen(
                    new string[]{
                        "ā", "ē", "ī", "ō", "ū", "ā", "ī", "ō", "ū", "yū", "á", "é", "í", "ó", "ú", "á", "í", "ó", "ú", "yú",
                        "ǎ", "ě", "ǐ", "ǒ", "ǔ", "ǎ", "ǐ", "ǒ", "ǔ", "yǔ", "à", "è", "ì", "ò", "ù", "à", "ì", "ò", "ù", "yù",
                        "a", "e", "i", "o", "u", "a", "i", "o", "u", "yu", "a", "e", "i", "o", "u", "a", "i", "o", "u", "yu",
                    },
                    new string[]{
                        "ā", "ē", "ī", "ō", "ū", "ā", "ī", "ō", "ū",
                        "á", "é", "í", "ó", "ú", "á", "í", "ó", "ú",
                        "ǎ", "ě", "ǐ", "ǒ", "ǔ", "ǎ", "ǐ", "ǒ", "ǔ",
                        "à", "è", "ì", "ò", "ù", "à", "ì", "ò", "ù",
                        "a", "e", "i", "o", "u", "a", "i", "o", "u",
                        "ā", "ē", "ī", "ō", "ū", "ā", "ī", "ō", "ū",
                        "á", "é", "í", "ó", "ú", "á", "í", "ó", "ú",
                        "ǎ", "ě", "ǐ", "ǒ", "ǔ", "ǎ", "ǐ", "ǒ", "ǔ",
                        "à", "è", "ì", "ò", "ù", "à", "ì", "ò", "ù",
                        "a", "e", "i", "o", "u", "a", "i", "o", "u",
                        "ā", "ē", "ī", "ō", "ū", "ā", "ī", "ō", "ū",
                        "á", "é", "í", "ó", "ú", "á", "í", "ó", "ú",
                        "ǎ", "ě", "ǐ", "ǒ", "ǔ", "ǎ", "ǐ", "ǒ", "ǔ",
                        "à", "è", "ì", "ò", "ù", "à", "ì", "ò", "ù",
                        "a", "e", "i", "o", "u", "a", "i", "o", "u",
                        "ā", "ē", "ī", "ō", "ū", "ā", "ī", "ō", "ū",
                        "á", "é", "í", "ó", "ú", "á", "í", "ó", "ú",
                        "ǎ", "ě", "ǐ", "ǒ", "ǔ", "ǎ", "ǐ", "ǒ", "ǔ",
                        "à", "è", "ì", "ò", "ù", "à", "ì", "ò", "ù",
                        "a", "e", "i", "o", "u", "a", "i", "o", "u",
                        "ā", "ē", "ī", "ō", "ū", "ā", "ī", "ō", "ū",
                        "á", "é", "í", "ó", "ú", "á", "í", "ó", "ú",
                        "ǎ", "ě", "ǐ", "ǒ", "ǔ", "ǎ", "ǐ", "ǒ", "ǔ",
                        "à", "è", "ì", "ò", "ù", "à", "ì", "ò", "ù",
                        "a", "e", "i", "o", "u", "a", "i", "o", "u",
                        "ā", "ē", "ī", "ō", "ū", "ā", "ī", "ō", "ū",
                        "á", "é", "í", "ó", "ú", "á", "í", "ó", "ú",
                        "ǎ", "ě", "ǐ", "ǒ", "ǔ", "ǎ", "ǐ", "ǒ", "ǔ",
                        "à", "è", "ì", "ò", "ù", "à", "ì", "ò", "ù",
                        "a", "e", "i", "o", "u", "a", "i", "o", "u",
                        "ā", "ē", "ī", "ō", "ū", "ā", "ī", "ō", "ū",
                        "á", "é", "í", "ó", "ú", "á", "í", "ó", "ú",
                        "ǎ", "ě", "ǐ", "ǒ", "ǔ", "ǎ", "ǐ", "ǒ", "ǔ",
                        "à", "è", "ì", "ò", "ù", "à", "ì", "ò", "ù",
                        "a", "e", "i", "o", "u", "a", "i", "o", "u",
                        "ā", "ē", "ī", "ō", "ū", "ā", "ī", "ō", "ū",
                        "á", "é", "í", "ó", "ú", "á", "í", "ó", "ú",
                        "ǎ", "ě", "ǐ", "ǒ", "ǔ", "ǎ", "ǐ", "ǒ", "ǔ",
                        "à", "è", "ì", "ò", "ù", "à", "ì", "ò", "ù",
                        "a", "e", "i", "o", "u", "a", "i", "o", "u",

                        "a", "e", "i", "o", "u", "a", "i", "o", "u",
                        "a", "e", "i", "o", "u", "a", "i", "o", "u",

                        "āí", "āó", "āú", "ēá", "īá", "īú", "ōá", "ūá", "ūé",
                        "āǐ", "āǒ", "āǔ", "ēǎ", "īǎ", "īǔ", "ōǎ", "ūǎ", "ūě",
                        "āì", "āò", "āù", "ēà", "īà", "īù", "ōà", "ūà", "ūè",
                        "āi", "āo", "āu", "ēa", "īa", "īu", "ōa", "ūa", "ūe",

                        "áī", "áō", "áū", "éā", "íā", "íū", "óā", "úā", "úē",
                        "áǐ", "áǒ", "áǔ", "éǎ", "íǎ", "íǔ", "óǎ", "ǔǎ", "ǔě",
                        "áì", "áò", "áù", "éà", "íà", "íù", "óà", "ùà", "ùè",
                        "ái", "áo", "áu", "éa", "ía", "íu", "óa", "ua", "ue",

                        "ǎī", "ǎō", "ǎū", "ěā", "ǐā", "ǐū", "ǒā", "ǔā", "ǔē",
                        "ǎí", "ǎó", "ǎú", "ěá", "ǐá", "ǐú", "ǒá", "ǔá", "ǔé",
                        "ǎì", "ǎò", "ǎù", "ěà", "ǐà", "ǐù", "ǒà", "ǔà", "ǔè",
                        "ǎi", "ǎo", "ǎu", "ěa", "ǐa", "ǐu", "ǒa", "ǔa", "ǔe",

                        "àī", "àō", "àū", "èā", "ìā", "ìū", "òā", "ùā", "ùē",
                        "àí", "àó", "àú", "èá", "ìá", "ìú", "òá", "ùá", "ùé",
                        "àǐ", "àǒ", "àǔ", "èǎ", "ìǎ", "ìǔ", "òǎ", "ùǎ", "ùě",
                        "ài", "ào", "àu", "èa", "ìa", "ìu", "òa", "ùa", "ùe",

                        "aī", "aō", "aū", "eā", "iā", "iū", "oā", "uā", "uē",
                        "aí", "aó", "aú", "eá", "iá", "iú", "oá", "uá", "ué",
                        "aǐ", "aǒ", "aǔ", "eǎ", "iǎ", "iǔ", "oǎ", "uǎ", "uě",
                        "aì", "aò", "aù", "eà", "ià", "iù", "oà", "uà", "uè",

                        "yū", "yú", "yū", "yú", "yū", "yú",
                        "yǔ", "yù", "yǔ", "yù", "yǔ", "yù",
                        "yu", "yu", "yu", "yu", "yu", "yu",
                    },
                    new string[]{
                        "b", "p", "m", "f", "d", "t", "n", "l", "g", "k", "h", "j", "q", "x",
                        "zh", "ch", "sh", "r", "z", "ts", "s",

                        "b", "p", "m", "d", "t", "n", "l", "g", "k", "h", "j", "q", "x", "zh", "ch", "sh", "z", "ts", "s",
                        "b", "p", "m", "d", "t", "n", "l", "g", "k", "h", "j", "q", "x", "zh", "ch", "sh", "z", "ts", "s",
                        "d", "t", "g", "k", "j", "q", "x", "zh", "ch", "sh", "z", "ts", "s",
                        "d", "t", "g", "k", "j", "q", "x", "zh", "ch", "sh", "z", "ts", "s",
                    },
                    new string[]{
                        "nb", "np", "nf", "nd", "nt", "nl", "ng", "nk", "nj", "nq", "nx", "nzh", "nch", "nsh", "nz", "nts", "ns",
                        "nb", "np", "nf", "nd", "nt", "nl", "ng", "nk", "nj", "nq", "nx", "nzh", "nch", "nsh", "nz", "nts", "ns",

                        "b", "p", "m", "f", "d", "t", "n", "l", "g", "k", "h", "j", "q", "x", "zh", "ch", "sh", "r", "z", "ts", "s",

                        "b", "p", "m", "d", "t", "n", "l", "g", "k", "h", "j", "q", "x", "zh", "ch", "sh", "z", "ts", "s",
                        "b", "p", "m", "d", "t", "n", "l", "g", "k", "h", "j", "q", "x", "zh", "ch", "sh", "z", "ts", "s",
                        "d", "t", "g", "k", "j", "q", "x", "zh", "ch", "sh", "z", "ts", "s",
                        "d", "t", "g", "k", "j", "q", "x", "zh", "ch", "sh", "z", "ts", "s",
                    },
                    new string[]{
                        "n", "n", "n", "n", "n", "n", "n",
                        "ng", "ng", "ng", "ng", "ng", "ng",
                        "r", "r", "r",
                    },
                    Array.Empty<string>(),
                    Array.Empty<string>(), new int[] { 1, 2, 3 }, new double[] { 14, 3, 1 }, 0.175, 0.55, 0.0, 0.0, GenericSanityChecks, true).Register("Chinese Romanized");

        public static readonly LanguageGen CHEROKEE_ROMANIZED = new LanguageGen(
                    new string[]{
                        "a", "e", "i", "o", "u", "ü", "a", "e", "i", "o", "u", "ü", "a", "e", "i", "o", "u", "ü",
                        "a", "e", "i", "o", "u", "ü", "a", "e", "i", "o", "u", "ü", "a", "e", "i", "o", "u", "ü",
                        "ai", "au", "oa", "oi", "ai", "au", "oa", "oi",
                        "a", "a", "a", "a", "a", "a", "a", "a", "a",
                        "ah", "ah", "ah", "ah", "ah", "ah", "ah",
                    },
                    Array.Empty<string>(),
                    new string[]{
                        "g", "k", "h", "l", "n", "qu", "s", "d", "t", "dl", "ts", "w", "y",
                        "g", "k", "h", "l", "n", "qu", "s", "d", "t", "dl", "ts", "w", "y",
                        "g", "h", "l", "n", "qu", "s", "d", "t", "ts", "y",
                        "g", "h", "l", "n", "qu", "s", "d", "t", "ts", "y",
                        "g", "h", "l", "n", "qu", "s", "d", "t", "ts", "y",
                        "g", "h", "l", "n", "qu", "s", "d", "t", "ts", "y",
                        "g", "h", "n", "qu", "s", "d", "t",
                        "g", "h", "n", "qu", "s", "d", "t",
                        "h", "n", "s", "d", "t", "h", "n", "s", "d", "t",
                        "h", "n", "s", "d", "t", "h", "n", "s", "d", "t",
                        "h", "n", "s", "d", "t", "h", "n", "s", "d", "t",
                            },
                    new string[]{
                        "g", "k", "h", "l", "n", "qu", "s", "d", "t", "dl", "ts", "w", "y",
                        "g", "k", "h", "l", "n", "qu", "s", "d", "t", "dl", "ts", "w", "y",
                        "g", "h", "l", "n", "qu", "s", "d", "t", "ts", "y",
                        "g", "h", "l", "n", "qu", "s", "d", "t", "ts", "y",
                        "g", "h", "l", "n", "qu", "s", "d", "t", "ts", "y",
                        "g", "h", "l", "n", "qu", "s", "d", "t", "ts", "y",
                        "g", "h", "n", "qu", "s", "d", "t",
                        "g", "h", "n", "qu", "s", "d", "t",
                        "h", "n", "s", "d", "t", "h", "n", "s", "d", "t",
                        "h", "n", "s", "d", "t", "h", "n", "s", "d", "t",
                        "h", "n", "s", "d", "t", "h", "n", "s", "d", "t",
                        "sn", "sn", "st", "st", "squ", "squ",
                        "th", "kh", "sh", "th", "kh", "sh", "th", "kh", "sh",
                        "th", "sh", "th", "sh", "th", "sh", "th", "sh",
                    },
                    new string[]{
                        "s"
                    },
                    Array.Empty<string>(),
                    Array.Empty<string>(), new int[] { 1, 2, 3, 4 }, new double[] { 4, 7, 6, 2 }, 0.3, 0.96, 0.0, 0.0, null, true).Register("Cherokee Romanized");

        public static readonly LanguageGen VIETNAMESE = new LanguageGen(new string[]{
                "a", "à", "á", "â", "ä", "ā", "ă",
                "e", "è", "é", "ê", "ë", "ē", "ĕ",
                "i", "ì", "í", "î", "ï", "ī", "ĭ",
                "o", "ò", "ó", "ô", "ö", "ō", "ŏ",
                "u", "ù", "ú", "û", "ü", "ū", "ŭ",
        },
                    new string[]{
                        "a", "à", "á", "â", "ä", "ā", "ă",
                        "a", "à", "á", "â", "ä", "ā", "ă",
                        "a", "à", "á", "â", "ä", "ā", "ă",
                        "a", "à", "á", "â", "ä", "ā", "ă",
                        "e", "è", "é", "ê", "ë", "ē", "ĕ",
                        "i", "ì", "í", "î", "ï", "ī", "ĭ",
                        "o", "ò", "ó", "ô", "ö", "ō", "ŏ",
                        "o", "ò", "ó", "ô", "ö", "ō", "ŏ",
                        "o", "ò", "ó", "ô", "ö", "ō", "ŏ",
                        "u", "ù", "ú", "û", "ü", "ū", "ŭ",

                        "a", "à", "á", "â", "ä", "ā", "ă",
                        "a", "à", "á", "â", "ä", "ā", "ă",
                        "a", "à", "á", "â", "ä", "ā", "ă",
                        "a", "à", "á", "â", "ä", "ā", "ă",
                        "e", "è", "é", "ê", "ë", "ē", "ĕ",
                        "i", "ì", "í", "î", "ï", "ī", "ĭ",
                        "o", "ò", "ó", "ô", "ö", "ō", "ŏ",
                        "o", "ò", "ó", "ô", "ö", "ō", "ŏ",
                        "o", "ò", "ó", "ô", "ö", "ō", "ŏ",
                        "u", "ù", "ú", "û", "ü", "ū", "ŭ",

                        "a", "à", "á", "â", "ä", "ā", "ă",
                        "a", "à", "á", "â", "ä", "ā", "ă",
                        "a", "à", "á", "â", "ä", "ā", "ă",
                        "a", "à", "á", "â", "ä", "ā", "ă",
                        "e", "è", "é", "ê", "ë", "ē", "ĕ",
                        "i", "ì", "í", "î", "ï", "ī", "ĭ",
                        "o", "ò", "ó", "ô", "ö", "ō", "ŏ",
                        "o", "ò", "ó", "ô", "ö", "ō", "ŏ",
                        "o", "ò", "ó", "ô", "ö", "ō", "ŏ",
                        "u", "ù", "ú", "û", "ü", "ū", "ŭ",

                        "ua", "uà", "uá", "uâ", "uä", "uā", "uă",
                        "ie", "iè", "ié", "iê", "ië", "iē", "iĕ",
                        "ie", "iè", "ié", "iê", "ië", "iē", "iĕ",
                        "ie", "ìe", "íe", "îe", "ïe", "īe", "ĭe",
                        "iu", "ìu", "íu", "îu", "ïu", "īu", "ĭu",
                        "oi", "òi", "ói", "ôi", "öi", "ōi", "ŏi",
                        "uo", "ùo", "úo", "ûo", "üo", "ūo", "ŭo",
                        "uo", "ùo", "úo", "ûo", "üo", "ūo", "ŭo",

                        "y", "y", "y", "y", "y", "y", "y",
                        "ye", "yè", "yé", "yê", "yë", "yē", "yĕ",
                    },
                    new string[]{
                        "b", "c", "ch", "d", "ð", "g", "h", "k", "kh", "l", "m", "n", "ng", "nh", "p", "ph", "qu", "r",
                        "s", "t", "th", "tr", "v", "x",
                        "b", "c", "d", "ð", "h", "l", "m", "n", "ng", "p", "ph", "t", "th", "tr", "v",
                        "b", "c", "d", "ð", "h", "l", "m", "n", "ng", "p", "ph", "t", "th", "tr", "v",
                        "b", "c", "d", "h", "l", "m", "n", "ng", "p", "ph", "t", "th", "tr", "v",
                        "b", "c", "d", "l", "n", "ng", "p", "ph", "th", "tr",
                        "b", "c", "d", "l", "n", "ng", "p", "ph", "th", "tr",
                        "b", "c", "d", "l", "n", "ng", "p",
                        "b", "c", "d", "l", "n", "ng", "p",
                        "b", "c", "d", "l", "n", "ng", "p",
                    }, new string[]{
                "b", "c", "ch", "d", "ð", "g", "h", "k", "kh", "l", "m", "n", "ng", "nh", "p", "ph", "qu", "r",
                "s", "t", "th", "tr", "v", "x",
                "b", "c", "d", "ð", "h", "l", "m", "n", "ng", "p", "ph", "t", "th", "tr", "v",
                "b", "c", "d", "ð", "h", "l", "m", "n", "ng", "p", "ph", "t", "th", "tr", "v",
                "b", "c", "d", "h", "l", "m", "n", "ng", "p", "ph", "t", "th", "tr", "v",
                "b", "c", "d", "l", "n", "ng", "p", "ph", "t", "th", "tr",
                "b", "c", "d", "l", "n", "ng", "p", "ph", "t", "th", "tr",
                "b", "c", "d", "l", "n", "ng", "p", "t",
                "b", "c", "d", "l", "n", "ng", "p", "t",
                "b", "c", "d", "l", "n", "ng", "p",
            },
                    new string[]{
                        "b", "c", "ch", "d", "ð", "g", "h", "k", "kh", "m", "m", "n", "ng", "nh", "p", "ch", "r",
                        "s", "t", "x",
                        "b", "c", "d", "ð", "h", "m", "m", "n", "ng", "p", "n", "t", "nh", "ng", "c",
                        "b", "c", "d", "ð", "h", "m", "m", "n", "ng", "p", "n", "t", "nh", "ng", "c",
                        "b", "c", "d", "h", "m", "m", "n", "ng", "p", "n", "t", "nh", "ng", "c",
                        "b", "c", "d", "m", "n", "ng", "p", "n", "t", "nh", "ng",
                        "b", "c", "d", "m", "n", "ng", "p", "n", "t", "nh", "ng",
                        "b", "c", "d", "m", "n", "ng", "p", "t",
                        "b", "c", "d", "m", "n", "ng", "p", "t",
                        "b", "c", "d", "m", "n", "ng", "p",
                    }, Array.Empty<string>(), Array.Empty<string>(), new int[] { 1, 2, 3 }, new double[] { 37.0, 3.0, 1.0 },
                    0.04, 0.4, 0.0, 0.0, GenericSanityChecks, true).Register("Vietnamese");

        public static readonly LanguageGen FANTASY = MALAY.Mix(2, (FRENCH, 4), (SIMPLISH, 1), (JAPANESE_ROMANIZED, 2), (ELF, 2), (GREEK_ROMANIZED, 1), (CELESTIAL, 1))
            .RemoveAccents().Register("Fantasy");

        public static readonly LanguageGen FANCY_FANTASY = FANTASY.AddAccents(0.47, 0.07).Register("Fancy Fantasy");

        #endregion LANGUAGES
    }
}
