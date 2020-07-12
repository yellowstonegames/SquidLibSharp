using System;
using System.Collections.Generic;

using SquidLib.SquidMath;

namespace Test {
    public class HashTest {
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
                h = (int)(SeededHash.PhiHash64(s));
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
                h = (int)(SeededHash.PhiHash64(s) >> 32);
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

            int sum = 0;
            //for (int i = 0; i < SeededHash.Predefined.Length; i++) {
            //    hashes.Clear();
            //    //ulong seed = RNG.Randomize((ulong)i);
            //    SeededHash sh = SeededHash.Predefined[i];
            //    foreach (string s in split) {
            //        h = (int)(sh.Hash64(s));
            //        if (hashes.ContainsKey(h)) {
            //            hashes[h].Add(s);
            //            //Console.WriteLine($"Collisions on {string.Join(",", hashes[h])}");
            //        } else {
            //            hashes[h] = new List<string>(1) {
            //                s
            //            };
            //        }
            //    }
            //    Console.WriteLine($"Testing SeededHash.Predefined[{i}]; difference of {split.Length - hashes.Count}");
            //    sum += split.Length - hashes.Count;
            //}
            //Console.WriteLine($"All {SeededHash.Predefined.Length} hash functors hit a total of {sum} collisions.");
            //Console.WriteLine($"Averaged {(double)sum / SeededHash.Predefined.Length} collisions.");
            sum = 0;
            RNG rng = new RNG(123456789UL);
            for (int i = 0; i < SeededHash.Predefined.Length; i++) {
                hashes.Clear();
                //ulong seed = RNG.Randomize((ulong)i);
                ulong seed = rng.NextULong();
                //                Console.WriteLine("Using seed: {0:X}", seed);
                foreach (string s in split) {
                    h = (int)(SeededHash.PhiHashSeeded64(seed, s));
                    //h = (int)SeededHash.Predefined[i].Hash64(s);
                    if (hashes.ContainsKey(h)) {
                        hashes[h].Add(s);
                        //Console.WriteLine($"Collisions on {string.Join(",", hashes[h])}");
                    } else {
                        hashes[h] = new List<string>(1) {
                            s
                        };
                    }
                }
                Console.WriteLine("Testing PhiHashSeeded64({0:X16}); difference of {1}", seed, split.Length - hashes.Count);
                sum += split.Length - hashes.Count;
            }
            Console.WriteLine($"All {SeededHash.Predefined.Length} hash seeds hit a total of {sum} collisions.");
            Console.WriteLine($"Averaged {(double)sum / SeededHash.Predefined.Length} collisions.");
        }
        static void Main(string[] _) {
            HashTest test = new HashTest();
            test.TestEnglishWordCollisions();
        }
        /*
Testing SeededHash.Predefined[0]; difference of 5
Testing SeededHash.Predefined[1]; difference of 5
Testing SeededHash.Predefined[2]; difference of 5
Testing SeededHash.Predefined[3]; difference of 8
Testing SeededHash.Predefined[4]; difference of 7
Testing SeededHash.Predefined[5]; difference of 10
Testing SeededHash.Predefined[6]; difference of 8
Testing SeededHash.Predefined[7]; difference of 3
Testing SeededHash.Predefined[8]; difference of 5
Testing SeededHash.Predefined[9]; difference of 5
Testing SeededHash.Predefined[10]; difference of 7
Testing SeededHash.Predefined[11]; difference of 3
Testing SeededHash.Predefined[12]; difference of 1
Testing SeededHash.Predefined[13]; difference of 5
Testing SeededHash.Predefined[14]; difference of 6
Testing SeededHash.Predefined[15]; difference of 3
Testing SeededHash.Predefined[16]; difference of 7
Testing SeededHash.Predefined[17]; difference of 2
Testing SeededHash.Predefined[18]; difference of 10
Testing SeededHash.Predefined[19]; difference of 4
Testing SeededHash.Predefined[20]; difference of 5
Testing SeededHash.Predefined[21]; difference of 7
Testing SeededHash.Predefined[22]; difference of 6
Testing SeededHash.Predefined[23]; difference of 5
Testing SeededHash.Predefined[24]; difference of 5
Testing SeededHash.Predefined[25]; difference of 4
Testing SeededHash.Predefined[26]; difference of 10
Testing SeededHash.Predefined[27]; difference of 3
Testing SeededHash.Predefined[28]; difference of 6
Testing SeededHash.Predefined[29]; difference of 3
Testing SeededHash.Predefined[30]; difference of 4
Testing SeededHash.Predefined[31]; difference of 4
Testing SeededHash.Predefined[32]; difference of 6
Testing SeededHash.Predefined[33]; difference of 5
Testing SeededHash.Predefined[34]; difference of 4
Testing SeededHash.Predefined[35]; difference of 1
Testing SeededHash.Predefined[36]; difference of 2
Testing SeededHash.Predefined[37]; difference of 6
Testing SeededHash.Predefined[38]; difference of 5
Testing SeededHash.Predefined[39]; difference of 7
Testing SeededHash.Predefined[40]; difference of 8
Testing SeededHash.Predefined[41]; difference of 9
Testing SeededHash.Predefined[42]; difference of 4
Testing SeededHash.Predefined[43]; difference of 6
Testing SeededHash.Predefined[44]; difference of 9
Testing SeededHash.Predefined[45]; difference of 7
Testing SeededHash.Predefined[46]; difference of 8
Testing SeededHash.Predefined[47]; difference of 5
Testing SeededHash.Predefined[48]; difference of 3
Testing SeededHash.Predefined[49]; difference of 5
Testing SeededHash.Predefined[50]; difference of 5
Testing SeededHash.Predefined[51]; difference of 6
Testing SeededHash.Predefined[52]; difference of 5
Testing SeededHash.Predefined[53]; difference of 10
Testing SeededHash.Predefined[54]; difference of 4
Testing SeededHash.Predefined[55]; difference of 7
Testing SeededHash.Predefined[56]; difference of 4
Testing SeededHash.Predefined[57]; difference of 2
Testing SeededHash.Predefined[58]; difference of 2
Testing SeededHash.Predefined[59]; difference of 2
Testing SeededHash.Predefined[60]; difference of 5
Testing SeededHash.Predefined[61]; difference of 2
Testing SeededHash.Predefined[62]; difference of 4
Testing SeededHash.Predefined[63]; difference of 6
Testing SeededHash.Predefined[64]; difference of 6
Testing SeededHash.Predefined[65]; difference of 3
Testing SeededHash.Predefined[66]; difference of 5
Testing SeededHash.Predefined[67]; difference of 6
Testing SeededHash.Predefined[68]; difference of 10
Testing SeededHash.Predefined[69]; difference of 5
Testing SeededHash.Predefined[70]; difference of 4
Testing SeededHash.Predefined[71]; difference of 7
Testing SeededHash.Predefined[72]; difference of 8
Testing SeededHash.Predefined[73]; difference of 7
Testing SeededHash.Predefined[74]; difference of 2
Testing SeededHash.Predefined[75]; difference of 5
Testing SeededHash.Predefined[76]; difference of 8
Testing SeededHash.Predefined[77]; difference of 5
Testing SeededHash.Predefined[78]; difference of 4
Testing SeededHash.Predefined[79]; difference of 5
Testing SeededHash.Predefined[80]; difference of 8
Testing SeededHash.Predefined[81]; difference of 3
Testing SeededHash.Predefined[82]; difference of 3
Testing SeededHash.Predefined[83]; difference of 7
Testing SeededHash.Predefined[84]; difference of 4
Testing SeededHash.Predefined[85]; difference of 9
Testing SeededHash.Predefined[86]; difference of 3
Testing SeededHash.Predefined[87]; difference of 2
Testing SeededHash.Predefined[88]; difference of 5
Testing SeededHash.Predefined[89]; difference of 5
Testing SeededHash.Predefined[90]; difference of 4
Testing SeededHash.Predefined[91]; difference of 1
Testing SeededHash.Predefined[92]; difference of 5
Testing SeededHash.Predefined[93]; difference of 3
Testing SeededHash.Predefined[94]; difference of 0
Testing SeededHash.Predefined[95]; difference of 5
Testing SeededHash.Predefined[96]; difference of 3
Testing SeededHash.Predefined[97]; difference of 4
Testing SeededHash.Predefined[98]; difference of 5
Testing SeededHash.Predefined[99]; difference of 4
Testing SeededHash.Predefined[100]; difference of 6
Testing SeededHash.Predefined[101]; difference of 7
Testing SeededHash.Predefined[102]; difference of 5
Testing SeededHash.Predefined[103]; difference of 3
Testing SeededHash.Predefined[104]; difference of 2
Testing SeededHash.Predefined[105]; difference of 2
Testing SeededHash.Predefined[106]; difference of 4
Testing SeededHash.Predefined[107]; difference of 3
Testing SeededHash.Predefined[108]; difference of 5
Testing SeededHash.Predefined[109]; difference of 9
Testing SeededHash.Predefined[110]; difference of 5
Testing SeededHash.Predefined[111]; difference of 14
Testing SeededHash.Predefined[112]; difference of 3
Testing SeededHash.Predefined[113]; difference of 3
Testing SeededHash.Predefined[114]; difference of 10
Testing SeededHash.Predefined[115]; difference of 5
Testing SeededHash.Predefined[116]; difference of 2
Testing SeededHash.Predefined[117]; difference of 7
Testing SeededHash.Predefined[118]; difference of 7
Testing SeededHash.Predefined[119]; difference of 4
Testing SeededHash.Predefined[120]; difference of 6
Testing SeededHash.Predefined[121]; difference of 5
Testing SeededHash.Predefined[122]; difference of 7
Testing SeededHash.Predefined[123]; difference of 4
Testing SeededHash.Predefined[124]; difference of 3
Testing SeededHash.Predefined[125]; difference of 5
Testing SeededHash.Predefined[126]; difference of 7
Testing SeededHash.Predefined[127]; difference of 2
Testing SeededHash.Predefined[128]; difference of 2
Testing SeededHash.Predefined[129]; difference of 6
Testing SeededHash.Predefined[130]; difference of 6
Testing SeededHash.Predefined[131]; difference of 8
Testing SeededHash.Predefined[132]; difference of 2
Testing SeededHash.Predefined[133]; difference of 5
Testing SeededHash.Predefined[134]; difference of 3
Testing SeededHash.Predefined[135]; difference of 2
Testing SeededHash.Predefined[136]; difference of 4
Testing SeededHash.Predefined[137]; difference of 5
Testing SeededHash.Predefined[138]; difference of 6
Testing SeededHash.Predefined[139]; difference of 3
Testing SeededHash.Predefined[140]; difference of 1
Testing SeededHash.Predefined[141]; difference of 8
Testing SeededHash.Predefined[142]; difference of 3
Testing SeededHash.Predefined[143]; difference of 4
Testing SeededHash.Predefined[144]; difference of 2
Testing SeededHash.Predefined[145]; difference of 12
Testing SeededHash.Predefined[146]; difference of 2
Testing SeededHash.Predefined[147]; difference of 3
Testing SeededHash.Predefined[148]; difference of 7
Testing SeededHash.Predefined[149]; difference of 4
Testing SeededHash.Predefined[150]; difference of 6
Testing SeededHash.Predefined[151]; difference of 6
Testing SeededHash.Predefined[152]; difference of 3
Testing SeededHash.Predefined[153]; difference of 6
Testing SeededHash.Predefined[154]; difference of 7
Testing SeededHash.Predefined[155]; difference of 2
Testing SeededHash.Predefined[156]; difference of 5
Testing SeededHash.Predefined[157]; difference of 2
Testing SeededHash.Predefined[158]; difference of 11
Testing SeededHash.Predefined[159]; difference of 8
Testing SeededHash.Predefined[160]; difference of 1
Testing SeededHash.Predefined[161]; difference of 5
Testing SeededHash.Predefined[162]; difference of 1
Testing SeededHash.Predefined[163]; difference of 6
Testing SeededHash.Predefined[164]; difference of 8
Testing SeededHash.Predefined[165]; difference of 5
Testing SeededHash.Predefined[166]; difference of 4
Testing SeededHash.Predefined[167]; difference of 2
Testing SeededHash.Predefined[168]; difference of 4
Testing SeededHash.Predefined[169]; difference of 4
Testing SeededHash.Predefined[170]; difference of 4
Testing SeededHash.Predefined[171]; difference of 5
Testing SeededHash.Predefined[172]; difference of 6
Testing SeededHash.Predefined[173]; difference of 9
Testing SeededHash.Predefined[174]; difference of 7
Testing SeededHash.Predefined[175]; difference of 5
Testing SeededHash.Predefined[176]; difference of 3
Testing SeededHash.Predefined[177]; difference of 5
Testing SeededHash.Predefined[178]; difference of 5
Testing SeededHash.Predefined[179]; difference of 2
Testing SeededHash.Predefined[180]; difference of 4
Testing SeededHash.Predefined[181]; difference of 7
Testing SeededHash.Predefined[182]; difference of 5
Testing SeededHash.Predefined[183]; difference of 7
Testing SeededHash.Predefined[184]; difference of 10
Testing SeededHash.Predefined[185]; difference of 3
Testing SeededHash.Predefined[186]; difference of 7
Testing SeededHash.Predefined[187]; difference of 4
Testing SeededHash.Predefined[188]; difference of 7
Testing SeededHash.Predefined[189]; difference of 6
Testing SeededHash.Predefined[190]; difference of 5
Testing SeededHash.Predefined[191]; difference of 9
All 192 hash functors hit a total of 972 collisions.
Averaged 5.0625 collisions.

        Also testing on PhiHashSeeded64:
All 192 hash seeds hit a total of 964 collisions.
Averaged 5.02083333333333 collisions.
*/
    }
}
