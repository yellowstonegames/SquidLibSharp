using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SquidLib.SquidText {
    public class Replacer {
        public Regex Pattern { get; }
        public string Replacement { get; }

        public Replacer(string pattern, string replacement) {
            Pattern = new Regex(pattern);
            Replacement = replacement;
        }

        public string Replace(string input) => Pattern.Replace(input, Replacement);
    }
}
