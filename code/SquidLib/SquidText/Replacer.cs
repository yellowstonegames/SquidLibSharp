using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using SquidLib.SquidMath;

namespace SquidLib.SquidText {
    public class Replacer {
        public Regex Pattern { get; }
        public string Replacement { get; }

        public Replacer(string pattern, string replacement) {
            Pattern = new Regex(pattern);
            Replacement = replacement;
        }

        public string Replace(string input) => Pattern.Replace(input, Replacement);
        public virtual string MaybeReplace(IRNG random, string input) => Pattern.Replace(input, Replacement);
    }

    public class Alteration : Replacer {
        public double Chance { get; }
        public Alteration(string pattern, string replacement) : base(pattern, replacement) => Chance = 1.0;
        public Alteration(string pattern, string replacement, double chance) : base(pattern, replacement) => Chance = chance;
        public override string MaybeReplace(IRNG random, string input) {
            if (random is null) {
                throw new ArgumentNullException(nameof(random));
            }
            if (random.NextDouble() < Chance)
                return base.Replace(input);
            return input;
        }
    }

    public class Modifier {
        private readonly Alteration[] alterations;

        public Alteration[] GetAlterations() => alterations;

        public Modifier() => alterations = Array.Empty<Alteration>();

        public Modifier(string pattern, string replacement) => alterations = (new Alteration[] { new Alteration(pattern, replacement) });

        public Modifier(string pattern, string replacement, double chance) => alterations = (new Alteration[] { new Alteration(pattern, replacement, chance) });

        public Modifier(params Alteration[] alts) => alterations = alts ?? Array.Empty<Alteration>();

        public string Modify(IRNG rng, string input) {
            for (int a = 0; a < GetAlterations().Length; a++) {
                input = GetAlterations()[a].MaybeReplace(rng, input);
            }
            return input;
        }

        /// <summary>
        /// Creates a Modifier that will replace the nth string key in dict with the nth value. Because
        /// of the state of the text at the time modifiers are run, only lower-case letters need to be searched for.
        /// This overload of ReplacementTable allows full regex syntax for search and replacement strings,
        /// such as searching for "([aeiou])\\1+" to find repeated occurrences of the same vowel, and "$1" in
        /// this example to replace the repeated section with only the first vowel.
        /// The ordering of dict matters if a later search contains an earlier search(the earlier one will be replaced
        /// first, possibly making the later search not match), or if an earlier replacement causes a later one to
        /// become valid.
        /// </summary>
        /// <param name="dict">an IndexedDictionary containing string keys to replace and string values to use instead; replacements happen in order</param>
        /// <returns>a Modifier that can be added to a copy of a LanguageGen with its AddModifiers method</returns>
        public static Modifier ReplacementTable(IndexedDictionary<string, string> dict) {
            if (dict == null)
                return new Modifier();
            Alteration[] alts = new Alteration[dict.Count];
            for (int i = 0; i < dict.Count; i++) {
                alts[i] = new Alteration(dict[Key.At, i], dict[Value.At, i]);
            }
            return new Modifier(alts);
        }

        /// <summary>
        /// Creates a Modifier that will replace the (n*2)th string in pairs with the (n*2+1)th value in pairs. Because
        /// of the state of the text at the time modifiers are run, only lower-case letters need to be searched for.
        /// This overload of ReplacementTable allows full regex syntax for search and replacement strings,
        /// such as searching for "([aeiou])\\1+" to find repeated occurrences of the same vowel, and "$1" in
        /// this example to replace the repeated section with only the first vowel.
        /// The ordering of pairs matters if a later search contains an earlier search(the earlier one will be replaced
        /// first, possibly making the later search not match), or if an earlier replacement causes a later one to
        /// become valid.
        /// </summary>
        /// <param name="pairs">params array of alternating strings to search for and strings to replace with; replacements happen in order</param>
        /// <returns>a Modifier that can be added to a copy of a LanguageGen with its AddModifiers method</returns>
        public static Modifier ReplacementTable(params string[] pairs) {
            int len;
            if (pairs == null || (len = pairs.Length) <= 1)
                return new Modifier();
            Alteration[] alts = new Alteration[len >> 1];
            for (int i = 0; i < alts.Length; i++) {
                alts[i] = new Alteration(pairs[i << 1], pairs[i << 1 | 1]);
            }
            return new Modifier(alts);
        }

        /// <summary>
        /// For a character who always pronounces 's', 'ss', and 'sh' as 'th'.
        /// </summary>
        public static readonly Modifier Lisp = new Modifier("[tţťț]?[sśŝşšș]+h?", "th");

        /// <summary>
        /// For a character who always lengthens 's' and 'z' sounds not starting a word.
        /// </summary>
        public static readonly Modifier Hiss = new Modifier("(.)([sśŝşšșzźżž])+", "$1$2$2$2");

        /// <summary>
        /// For a character who has a 20% chance to repeat a starting consonant or vowel.
        /// </summary>
        public static readonly Modifier Stutter = new Modifier(
                new Alteration("^([^aàáâãäåæāăąǻǽeèéêëēĕėęěiìíîïĩīĭįıoòóôõöøōŏőœǿuùúûüũūŭůűųyýÿŷỳαοειυωаеёийъыэюяоу]+)", "$1-$1", 0.2),
                new Alteration("^([aàáâãäåæāăąǻǽeèéêëēĕėęěiìíîïĩīĭįıoòóôõöøōŏőœǿuùúûüũūŭůűųyýÿŷỳαοειυωаеёийъыэюяоу]+)", "$1-$1", 0.2));

        /// <summary>
        /// For a language that has a 40% chance to repeat a single Latin vowel (a, e, o, or a variant on one of them like å or ö, but not merged letters like æ and œ).
        /// </summary>
        public static readonly Modifier DoubleVowels = new Modifier(
                "([^aàáâãäåæāăąǻǽeèéêëēĕėęěiìíîïĩīĭįıoòóôõöøōŏőœǿuùúûüũūŭůűųyýÿŷỳ]|^)"
                        + "([aàáâãäåāăąǻeèéêëēĕėęěòóôõöøōŏőǿ])"
                        + "([^aàáâãäåæāăąǻǽeèéêëēĕėęěiìíîïĩīĭįıoòóôõöøōŏőœǿuùúûüũūŭůűųyýÿŷỳ]|$)", "$1$2$2$3", 0.4);


        /// <summary>
        /// For a language that has a 50% chance to repeat a single consonant.
        /// </summary>
        public static readonly Modifier DoubleConsonants = new Modifier("([aàáâãäåæāăąǻǽeèéêëēĕėęěiìíîïĩīĭįıoòóôõöøōŏőœǿuùúûüũūŭůűųyýÿŷỳαοειυωаеёийъыэюяоу])" +
                "([^aàáâãäåæāăąǻǽeèéêëēĕėęěiìíîïĩīĭįıoòóôõöøōŏőœǿuùúûüũūŭůűųyýÿŷỳαοειυωаеёийъыэюяоуqwhjx])" +
                "([aàáâãäåæāăąǻǽeèéêëēĕėęěiìíîïĩīĭįıoòóôõöøōŏőœǿuùúûüũūŭůűųyýÿŷỳαοειυωаеёийъыэюяоу]|$)", "$1$2$2$3", 0.5);

        /// <summary>
        /// For a language that never repeats the same letter twice in a row.
        /// </summary>
        public static readonly Modifier NoDoubles = new Modifier("(.)\\1", "$1");

        /// <summary>
        /// Simple changes to merge "ae" into "æ", "oe" into "œ", and any of "aé", "áe", or "áé" into "ǽ".
        /// </summary>
        public static readonly Modifier Ligatures = ReplacementTable("ae", "æ", "oe", "œ", "áe", "ǽ", "aé", "ǽ", "áé", "ǽ");

        /// <summary>
        /// Simple changes to split "æ" into "ae", "œ" into "oe", and "ǽ" into "áe".
        /// </summary>
        public static readonly Modifier SplitLigatures = ReplacementTable("æ", "ae", "œ", "oe", "ǽ", "áe");

        /// <summary>
        /// Some changes that can be applied when sanity checks (which force re-generating a new word) aren't appropriate
        /// for fixing a word that isn't pronounceable.
        /// </summary>
        public static readonly Modifier GeneralCleanup = ReplacementTable(
                "[æǽœìíîïĩīĭįıiùúûüũūŭůűųuýÿŷỳy]([æǽœýÿŷỳy])", "$1",
                "q([ùúûüũūŭůűųu])$", "q$1e",
                "([ìíîïĩīĭįıi])[ìíîïĩīĭįıi]", "$1",
                "([æǽœìíîïĩīĭįıiùúûüũūŭůűųuýÿŷỳy])[wŵẁẃẅ]$", "$1",
                "([ùúûüũūŭůűųu])([òóôõöøōŏőǿo])", "$2$1",
                "[àáâãäåāăąǻaèéêëēĕėęěeìíîïĩīĭįıiòóôõöøōŏőǿoùúûüũūŭůűųuýÿŷỳy]([æǽœ])", "$1",
                "([æǽœ])[àáâãäåāăąǻaèéêëēĕėęěeìíîïĩīĭįıiòóôõöøōŏőǿoùúûüũūŭůűųuýÿŷỳy]", "$1",
                "([wŵẁẃẅ])[wŵẁẃẅ]", "$1",
                "qq", "q");

    }
}
