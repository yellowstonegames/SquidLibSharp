using System;
using System.Collections.Generic;
using SquidLib.SquidMath;

namespace Test
{
    public class HashTest
    {
        public void TestEnglishWordCollisions() {
            string[] split = Properties.Resources.WORD_LIST.Split('\n');
            Dictionary<int, List<string>> hashes = new Dictionary<int, List<string>>(split.Length);
            int h;
            foreach (string s in split) {
                h = s.GetHashCode();
                if (hashes.ContainsKey(h)) {
                    hashes[h].Add(s);
                    Console.WriteLine($"Collisions on {string.Join(",", hashes[h])}");
                } else {
                    hashes[h] = new List<string>(1) {
                        s
                    };
                }
            }
            Console.WriteLine($"Testing String.GetHashCode(); difference of {split.Length - hashes.Count}");
            hashes.Clear();
            foreach (string s in split) {
                h = (int)SeededHash.OverkillHash(s);
                if (hashes.ContainsKey(h)) {
                    hashes[h].Add(s);
                    Console.WriteLine($"Collisions on {string.Join(",", hashes[h])}");
                } else {
                    hashes[h] = new List<string>(1) {
                        s
                    };
                }
            }
            Console.WriteLine($"Testing OverkillHash; difference of {split.Length - hashes.Count}");
            hashes.Clear();
            foreach (string s in split) {
                h = (int)(SeededHash.PhiHash(s));
                if (hashes.ContainsKey(h)) {
                    hashes[h].Add(s);
                    Console.WriteLine($"Collisions on {string.Join(",", hashes[h])}");
                } else {
                    hashes[h] = new List<string>(1) {
                        s
                    };
                }
            }
            Console.WriteLine($"Testing PhiHash Low; difference of {split.Length - hashes.Count}");
            hashes.Clear();
            foreach (string s in split) {
                h = (int)(SeededHash.PhiHash(s) >> 32);
                if (hashes.ContainsKey(h)) {
                    hashes[h].Add(s);
                    Console.WriteLine($"Collisions on {string.Join(",", hashes[h])}");
                } else {
                    hashes[h] = new List<string>(1) {
                        s
                    };
                }
            }
            Console.WriteLine($"Testing PhiHash High; difference of {split.Length - hashes.Count}");
            hashes.Clear();
            foreach (string s in split) {
                h = SeededHash.PhiHash32(s);
                if (hashes.ContainsKey(h)) {
                    hashes[h].Add(s);
                    Console.WriteLine($"Collisions on {string.Join(",", hashes[h])}");
                } else {
                    hashes[h] = new List<string>(1) {
                        s
                    };
                }
            }


            //ulong m = 0xC6BC279692B5C323UL;
            //for (int i = 0; i < 100; i++) {
            //    hashes.Clear();
            //    foreach (string s in split) {
            //        h = (int)(SeededHash.PhiHash(s, m));
            //        if (hashes.ContainsKey(h)) {
            //            hashes[h].Add(s);
            //            //Console.WriteLine($"Collisions on {string.Join(",", hashes[h])}");
            //        } else {
            //            hashes[h] = new List<string>(1) {
            //            s
            //        };
            //        }
            //    }
            //    //Console.WriteLine($"Testing PhiHash Low; difference of {split.Length - hashes.Count}");
            //    hashes.Clear();
            //    foreach (string s in split) {
            //        h = (int)(SeededHash.PhiHash(s, m) >> 32);
            //        if (hashes.ContainsKey(h)) {
            //            hashes[h].Add(s);
            //            //Console.WriteLine($"Collisions on {string.Join(",", hashes[h])}");
            //        } else {
            //            hashes[h] = new List<string>(1) {
            //            s
            //        };
            //        }
            //    }
            //    if (split.Length == hashes.Count)
            //        Console.WriteLine("PhiHash High works for start 0x{0:X}UL", m);
            //    m ^= RNG.Determine((ulong)i + m) & RNG.Randomize((ulong)i - m) & RNG.Randomize((ulong)i);
            //    //Console.WriteLine($"Testing PhiHash High; difference of {split.Length - hashes.Count}");
            //}


            Console.WriteLine($"Testing PhiHash int; difference of {split.Length - hashes.Count}");
            for (int i = 1; i <= 10; i++) {
                hashes.Clear();
                ulong seed = RNG.Randomize((ulong)i);
                SeededHash sh = new SeededHash(seed);
                foreach (string s in split) {
                    h = (int)(sh.Hash64Alt(s));
                    if (hashes.ContainsKey(h)) {
                        hashes[h].Add(s);
                        Console.WriteLine($"Collisions on {string.Join(",", hashes[h])}");
                    } else {
                        hashes[h] = new List<string>(1) {
                            s
                        };
                    }
                }
                Console.WriteLine($"Testing SeededHash({seed}); difference of {split.Length - hashes.Count}");

            }
        }
        static void Main(string[] args) {
            HashTest test = new HashTest();
            test.TestEnglishWordCollisions();
        }
    }
}
