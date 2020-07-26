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
        public Alteration(string pattern, string replacement) : base(pattern, replacement) {
            Chance = 1.0;
        }
        public Alteration(string pattern, string replacement, double chance) : base(pattern, replacement) {
            Chance = chance;
        }
        public override string MaybeReplace(IRNG random, string input) {
            if (random.NextDouble() < Chance)
                return base.Replace(input);
            return input;
        }
    }

    public class Modifier {
        public readonly Alteration[] alterations;

        public Modifier() {
            alterations = Array.Empty<Alteration>();
        }

        public Modifier(String pattern, String replacement) {
            alterations = new Alteration[] { new Alteration(pattern, replacement) };
        }

        public Modifier(String pattern, String replacement, double chance) {
            alterations = new Alteration[] { new Alteration(pattern, replacement, chance) };
        }

        public Modifier(params Alteration[] alts) {
            alterations = alts ?? Array.Empty<Alteration>();
        }

        public string Modify(IRNG rng, string input) {
            for (int a = 0; a < alterations.Length; a++) {
                input = alterations[a].MaybeReplace(rng, input);
            }
            return input;
        }
    }
}
