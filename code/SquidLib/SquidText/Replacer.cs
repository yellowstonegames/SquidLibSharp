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
            if (random.NextDouble() < Chance)
                return base.Replace(input);
            return input;
        }
    }

    public class Modifier {
        public Alteration[] Alterations { get; }

        public Modifier() => Alterations = Array.Empty<Alteration>();

        public Modifier(String pattern, String replacement) => Alterations = new Alteration[] { new Alteration(pattern, replacement) };

        public Modifier(String pattern, String replacement, double chance) => Alterations = new Alteration[] { new Alteration(pattern, replacement, chance) };

        public Modifier(params Alteration[] alts) => Alterations = alts ?? Array.Empty<Alteration>();

        public string Modify(IRNG rng, string input) {
            for (int a = 0; a < Alterations.Length; a++) {
                input = Alterations[a].MaybeReplace(rng, input);
            }
            return input;
        }

        /// <summary>
        /// Creates a Modifier that will replace the nth String key in dict with the nth value. Because
        /// of the state of the text at the time modifiers are run, only lower-case letters need to be searched for.
        /// This overload of replacementTable allows full regex syntax for search and replacement Strings,
        /// such as searching for "([aeiou])\\1+" to find repeated occurrences of the same vowel, and "$1" in
        /// this example to replace the repeated section with only the first vowel.
        /// The ordering of dict matters if a later search contains an earlier search(the earlier one will be replaced
        /// first, possibly making the later search not match), or if an earlier replacement causes a later one to
        /// become valid.
        /// </summary>
        /// <param name="dict">an IndexedDictionary containing String keys to replace and String values to use instead; replacements happen in order</param>
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
        /// Creates a Modifier that will replace the (n*2)th String in pairs with the (n*2+1)th value in pairs. Because
        /// of the state of the text at the time modifiers are run, only lower-case letters need to be searched for.
        /// This overload of replacementTable allows full regex syntax for search and replacement Strings,
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

    }
}
