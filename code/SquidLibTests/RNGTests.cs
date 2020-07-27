using System;
using System.ComponentModel;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SquidLib.SquidMath;

namespace SquidLibTests {
    [TestClass]
    public class RNGTests {
        private static readonly int iterations = 1000;

        private static readonly RNG[] rngs = {
                new RNG(),
                new RNG(new RNG()),
                new RNG(0),
                new RNG(int.MinValue),
                new RNG(int.MaxValue),
                new RNG(0L),
                new RNG(long.MinValue),
                new RNG(long.MaxValue),
                new RNG(string.Empty),
                new RNG("testing"),
                new RNG(0U),
                new RNG(ulong.MinValue),
                new RNG(ulong.MaxValue),
                new RNG(0UL, 0UL),
                new RNG(ulong.MaxValue, ulong.MinValue)
            };

        [TestMethod]
        public void TestBools() {
            foreach (RNG rng in rngs) {
                for (int count = 0; count < iterations; count++) {
                    bool value = rng.NextBoolean();
                    Assert.AreEqual(value, rng.PreviousBoolean());
                }
            }
        }

        [TestMethod]
        public void TestInts() {
            foreach (RNG rng in rngs) {
                for (int count = 0; count < iterations; count++) {
                    Assert.AreEqual(0, rng.Next(0, 0));
                    Assert.AreEqual(12, rng.Next(12, 13));


                    int value = rng.NextInt();
                    Assert.AreEqual(value, rng.PreviousInt());
                    Assert.AreEqual(0, rng.NextInt(-1));
                    Assert.AreEqual(0, rng.NextSignedInt(-1));

                    int[] boundaries = {
                        int.MaxValue,
                        int.MinValue,
                        0,
                        int.MaxValue / 2,
                        int.MinValue / 2,
                        rng.Next()
                    };

                    foreach (int bound in boundaries) {
                        value = rng.NextInt(bound);
                        Assert.AreEqual(value, rng.PreviousInt(bound));
                        switch (bound) {
                            case 0:
                                Assert.AreEqual(0, value);
                                break;
                            case { } _ when bound >= 0:
                                Assert.IsTrue(value >= 0, $"Boundary fail with value {value} and bound {bound}");
                                Assert.IsTrue(value < bound, $"Boundary fail with value {value} and bound {bound}");
                                break;
                            case { } _ when bound < 0:
                                Assert.IsTrue(value <= 0, $"Boundary fail with value {value} and bound {bound}");
                                Assert.IsTrue(value > bound, $"Boundary fail with value {value} and bound {bound}");
                                break;
                            default:
                                Assert.Fail($"Untested value: {bound}");
                                break;
                        }

                        value = rng.NextSignedInt(bound);
                        Assert.AreEqual(value, rng.PreviousSignedInt(bound));
                        switch (bound) {
                            case 0:
                                Assert.AreEqual(0, value);
                                break;
                            case { } _ when bound >= 0:
                                Assert.IsTrue(value >= 0, $"Boundary fail with value {value} and bound {bound}");
                                Assert.IsTrue(value < bound, $"Boundary fail with value {value} and bound {bound}");
                                break;
                            case { } _ when bound < 0:
                                Assert.IsTrue(value <= 0, $"Boundary fail with value {value} and bound {bound}");
                                Assert.IsTrue(value > bound, $"Boundary fail with value {value} and bound {bound}");
                                break;
                            default:
                                Assert.Fail($"Untested value: {bound}");
                                break;
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestUInts() {
            foreach (RNG rng in rngs) {
                for (int count = 0; count < iterations; count++) {
                    uint value = rng.NextUInt();
                    Assert.AreEqual(value, rng.PreviousUInt());

                    uint[] boundaries = {
                        uint.MaxValue,
                        uint.MinValue,
                        0U,
                        uint.MaxValue / 2U,
                        uint.MinValue / 2U,
                        rng.NextUInt()
                    };

                    foreach (uint bound in boundaries) {
                        value = rng.NextUInt(bound);
                        Assert.AreEqual(value, rng.PreviousUInt(bound));
                        switch (bound) {
                            case 0U:
                                Assert.AreEqual(0U, value);
                                break;
                            case { } _ when bound >= 0U:
                                Assert.IsTrue(value < bound, $"Boundary fail with value {value} and bound {bound}");
                                break;
                            default:
                                Assert.Fail($"Untested value: {bound}");
                                break;
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestLongs() {
            foreach (RNG rng in rngs) {
                for (int count = 0; count < iterations; count++) {
                    long value = rng.NextLong();
                    Assert.AreEqual(value, rng.PreviousLong());
                    Assert.AreEqual(0L, rng.NextLong(-1));
                    Assert.AreEqual(0L, rng.NextSignedLong(-1L));

                    long[] boundaries = {
                        long.MaxValue,
                        long.MinValue,
                        0L,
                        long.MaxValue / 2L,
                        long.MinValue / 2L,
                        rng.Next()
                    };

                    foreach (long bound in boundaries) {
                        value = rng.NextLong(bound);
                        Assert.AreEqual(value, rng.PreviousLong(bound));
                        switch (bound) {
                            case 0L:
                                Assert.AreEqual(0L, value);
                                break;
                            case { } _ when bound >= 0L:
                                Assert.IsTrue(value >= 0L, $"Boundary fail with value {value} and bound {bound}");
                                Assert.IsTrue(value < bound, $"Boundary fail with value {value} and bound {bound}");
                                break;
                            case { } _ when bound < 0L:
                                Assert.IsTrue(value <= 0L, $"Boundary fail with value {value} and bound {bound}");
                                Assert.IsTrue(value > bound, $"Boundary fail with value {value} and bound {bound}");
                                break;
                            default:
                                Assert.Fail($"Untested value: {bound}");
                                break;
                        }

                        value = rng.NextSignedLong(bound);
                        Assert.AreEqual(value, rng.PreviousSignedLong(bound));
                        switch (bound) {
                            case 0L:
                                Assert.AreEqual(0L, value);
                                break;
                            case { } _ when bound >= 0L:
                                Assert.IsTrue(value >= 0L, $"Boundary fail with value {value} and bound {bound}");
                                Assert.IsTrue(value < bound, $"Boundary fail with value {value} and bound {bound}");
                                break;
                            case { } _ when bound < 0L:
                                Assert.IsTrue(value <= 0L, $"Boundary fail with value {value} and bound {bound}");
                                Assert.IsTrue(value > bound, $"Boundary fail with value {value} and bound {bound}");
                                break;
                            default:
                                Assert.Fail($"Untested value: {bound}");
                                break;
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestULongs() {
            foreach (RNG rng in rngs) {
                for (int count = 0; count < iterations; count++) {
                    ulong value = rng.NextULong();
                    Assert.AreEqual(value, rng.PreviousULong());

                    ulong[] boundaries = {
                        ulong.MaxValue,
                        ulong.MinValue,
                        0UL,
                        ulong.MaxValue / 2UL,
                        ulong.MinValue / 2UL,
                        rng.NextUInt()
                    };

                    foreach (ulong bound in boundaries) {
                        value = rng.NextULong(bound);
                        Assert.AreEqual(value, rng.PreviousULong(bound));
                        switch (bound) {
                            case 0UL:
                                Assert.AreEqual(0UL, value);
                                break;
                            case { } _ when bound >= 0UL:
                                Assert.IsTrue(value < bound, $"Boundary fail with value {value} and bound {bound}");
                                break;
                            default:
                                Assert.Fail($"Untested value: {bound}");
                                break;
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestDoubles() {
            foreach (RNG rng in rngs) {
                for (int count = 0; count < iterations; count++) {
                    double value = rng.NextDouble();
                    Assert.AreEqual(value, rng.PreviousDouble());
                    Assert.IsTrue(value >= 0.0);
                    Assert.IsTrue(value < 1.0);

                    double[] doubleValuesToTest = {
                        double.MaxValue,
                        double.MinValue,
                        0.0,
                        double.MaxValue / 2.0,
                        double.MinValue / 2.0,
                        double.Epsilon,
                        -double.Epsilon,
                        double.NaN,
                        double.NegativeInfinity,
                        double.PositiveInfinity,
                        rng.NextDouble(double.MinValue, double.MaxValue)
                    };

                    foreach (double bound in doubleValuesToTest) {
                        value = rng.NextDouble(bound);
                        Assert.AreEqual(value, rng.PreviousDouble(bound));
                        switch (bound) {
                            case 0.0:
                            case double.Epsilon:
                                Assert.AreEqual(0.0, value);
                                break;
                            case double.NegativeInfinity:
                            case double.PositiveInfinity:
                                Assert.AreEqual(bound, value); // In the special case of infinities, only infinities should be returned
                                break;
                            case double.NaN:
                                Assert.IsTrue(double.IsNaN(value));
                                break;
                            case { } _ when bound > 0.0:
                                Assert.IsTrue(value >= 0.0, $"Boundary fail with value {value} and bound {bound}");
                                Assert.IsTrue(value < bound, $"Boundary fail with value {value} and bound {bound}");
                                break;
                            case { } _ when bound < 0.0:
                                Assert.IsTrue(value <= 0.0, $"Boundary fail with value {value} and bound {bound}");
                                Assert.IsTrue(value > bound, $"Boundary fail on with value {value} and bound {bound}");
                                break;
                            default:
                                Assert.Fail($"Untested value: {bound}");
                                break;
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestFloats() {
            foreach (RNG rng in rngs) {
                for (int count = 0; count < iterations; count++) {
                    float value = rng.NextFloat();
                    Assert.AreEqual(value, rng.PreviousFloat());
                    Assert.IsTrue(value >= 0.0f);
                    Assert.IsTrue(value < 1.0f);

                    float[] boundaries = {
                        float.MaxValue,
                        float.MinValue,
                        0.0f,
                        float.MaxValue / 2.0f,
                        float.MinValue / 2.0f,
                        float.Epsilon,
                        -float.Epsilon,
                        float.NaN,
                        float.NegativeInfinity,
                        float.PositiveInfinity
                    };

                    foreach (float bound in boundaries) {
                        value = rng.NextFloat(bound);
                        Assert.AreEqual(value, rng.PreviousFloat(bound));
                        switch (bound) {
                            case 0.0f:
                            case float.Epsilon:
                                Assert.AreEqual(0.0f, value);
                                break;
                            case float.NegativeInfinity:
                            case float.PositiveInfinity:
                                Assert.AreEqual(bound, value); // In the special case of infinities, only infinities should be returned
                                break;
                            case float.NaN:
                                Assert.IsTrue(float.IsNaN(value));
                                break;
                            case { } _ when bound > 0.0f:
                                Assert.IsTrue(value >= 0.0f, $"Boundary fail with value {value} and bound {bound}");
                                Assert.IsTrue(value < bound, $"Boundary fail with value {value} and bound {bound}");
                                break;
                            case { } _ when bound < 0.0f:
                                Assert.IsTrue(value <= 0.0f, $"Boundary fail with value {value} and bound {bound}");
                                Assert.IsTrue(value > bound, $"Boundary fail with value {value} and bound {bound}");
                                break;
                            default:
                                Assert.Fail($"Untested value: {bound}");
                                break;
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestDetermineFloat() {
            for (int count = 0; count < iterations; count++) {
                ulong[] states = {
                    ulong.MinValue,
                    ulong.MaxValue,
                    ulong.MaxValue / 2
                };

                foreach (ulong state in states) {
                    for (ulong adding = 0; adding <= 300; adding += 3) {
                        float value = RNG.DetermineFloat(state + adding);
                        Assert.AreEqual(value, RNG.DetermineFloat(state + adding));
                        Assert.IsTrue(value >= 0.0f);
                        Assert.IsTrue(value < 1.0f);
                    }
                }
            }
        }

        [TestMethod]
        public void TestDetermineDouble() {
            for (int count = 0; count < iterations; count++) {
                ulong[] states = {
                    ulong.MinValue,
                    ulong.MaxValue,
                    ulong.MaxValue / 2
                };

                foreach (ulong state in states) {
                    for (ulong adding = 0; adding <= 300; adding += 3) {
                        double value = RNG.DetermineDouble(state + adding);
                        Assert.AreEqual(value, RNG.DetermineDouble(state + adding));
                        Assert.IsTrue(value >= 0.0);
                        Assert.IsTrue(value < 1.0);
                    }
                }
            }
        }

        [TestMethod]
        public void TestRandomizeFloat() {
            for (int count = 0; count < iterations; count++) {
                ulong[] states = {
                    ulong.MinValue,
                    ulong.MaxValue,
                    ulong.MaxValue / 2
                };

                foreach (ulong state in states) {
                    for (ulong adding = 0; adding <= 300; adding += 3) {
                        float value = RNG.RandomizeFloat(state + adding);
                        Assert.AreEqual(value, RNG.RandomizeFloat(state + adding));
                        Assert.IsTrue(value >= 0.0f);
                        Assert.IsTrue(value < 1.0f);
                    }
                }
            }
        }

        [TestMethod]
        public void TestRandomizeDouble() {
            for (int count = 0; count < iterations; count++) {
                ulong[] states = {
                    ulong.MinValue,
                    ulong.MaxValue,
                    ulong.MaxValue / 2
                };

                foreach (ulong state in states) {
                    for (ulong adding = 0; adding <= 300; adding += 3) {
                        double value = RNG.RandomizeDouble(state + adding);
                        Assert.AreEqual(value, RNG.RandomizeDouble(state + adding));
                        Assert.IsTrue(value >= 0.0);
                        Assert.IsTrue(value < 1.0);
                    }
                }
            }
        }

        [TestMethod]
        public void TestDetermineBounded() {
            for (int count = 0; count < iterations; count++) {
                ulong[] states = {
                        ulong.MinValue,
                        ulong.MaxValue,
                        ulong.MaxValue / 2
                    };

                int[] boundaries = {
                        int.MaxValue,
                        int.MinValue,
                        0,
                        1,
                        -1,
                        2,
                        -2,
                        int.MaxValue / 2,
                        int.MinValue / 2
                    };

                foreach (ulong state in states) {
                    for (ulong adding = 0; adding <= 300; adding += 3) {
                        foreach (int bound in boundaries) {
                            int value = RNG.DetermineBounded(state + adding, bound);
                            Assert.AreEqual(value, RNG.DetermineBounded(state + adding, bound));
                            switch (bound) {
                                case 0:
                                    Assert.AreEqual(0, value);
                                    break;
                                case { } _ when bound > 0:
                                    Assert.IsTrue(value >= 0, $"Boundary fail with value {value} and bound {bound}");
                                    Assert.IsTrue(value < bound, $"Boundary fail with value {value} and bound {bound}");
                                    break;
                                case { } _ when bound < 0:
                                    Assert.IsTrue(value <= 0, $"Boundary fail with value {value} and bound {bound}");
                                    Assert.IsTrue(value > bound, $"Boundary fail with value {value} and bound {bound}");
                                    break;
                                default:
                                    Assert.Fail($"Untested value: {bound}");
                                    break;
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestRandomizeBounded() {
            for (int count = 0; count < iterations; count++) {
                ulong[] states = {
                        ulong.MinValue,
                        ulong.MaxValue,
                        ulong.MaxValue / 2
                    };

                int[] boundaries = {
                        int.MaxValue,
                        int.MinValue,
                        0,
                        1,
                        -1,
                        2,
                        -2,
                        int.MaxValue / 2,
                        int.MinValue / 2
                    };

                foreach (ulong state in states) {
                    for (ulong adding = 0; adding <= 300; adding += 3) {
                        foreach (int bound in boundaries) {
                            int value = RNG.RandomizeBounded(state + adding, bound);
                            Assert.AreEqual(value, RNG.RandomizeBounded(state + adding, bound));
                            switch (bound) {
                                case 0:
                                    Assert.AreEqual(0, value);
                                    break;
                                case { } _ when bound > 0:
                                    Assert.IsTrue(value >= 0, $"Boundary fail with value {value} and bound {bound}");
                                    Assert.IsTrue(value < bound, $"Boundary fail with value {value} and bound {bound}");
                                    break;
                                case { } _ when bound < 0:
                                    Assert.IsTrue(value <= 0, $"Boundary fail with value {value} and bound {bound}");
                                    Assert.IsTrue(value > bound, $"Boundary fail with value {value} and bound {bound}");
                                    break;
                                default:
                                    Assert.Fail($"Untested value: {bound}");
                                    break;
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestStateCode() {
            foreach (RNG rng in rngs) {
                // make sure invalid input is considered invalid
                AssertHasException.Throws<ArgumentException>(
                    () => rng.StateCode = null
                );

                string previousState;
                string currentState;
                for (int iteration = 0; iteration < iterations; iteration++) {
                    previousState = rng.StateCode;
                    int previousInt = rng.NextInt();
                    int currentInt = rng.PreviousInt();

                    Assert.AreEqual(previousInt, currentInt);

                    currentState = rng.StateCode;

                    Assert.AreEqual(previousState, currentState);

                    for (int i = 0; i < rng.NextInt(10, 20); i++) {
                        rng.Next();
                    }

                    rng.StateCode = currentState;
                    currentInt = rng.NextInt();


                    Assert.AreEqual(previousInt, currentInt);
                }
            }
        }

        [TestMethod]
        public void TestState() {
            foreach (RNG rng in rngs) {
                for (int iteration = 0; iteration < iterations; iteration++) {
                    var previousState = rng.State;
                    int previousInt = rng.NextInt();
                    int currentInt = rng.PreviousInt();

                    Assert.AreEqual(previousInt, currentInt);

                    var currentState = rng.State;

                    Assert.AreEqual(previousState, currentState);

                    for (int i = 0; i < rng.NextInt(10, 20); i++) {
                        rng.Next();
                    }

                    rng.State = currentState;
                    currentInt = rng.NextInt();


                    Assert.AreEqual(previousInt, currentInt);
                }
            }
        }

        [TestMethod]
        public void TestToString() {
            foreach (RNG rng in rngs) {
                for (int iteration = 0; iteration < iterations; iteration++) {
                    Assert.IsNotNull(rng.ToString());
                    Assert.AreNotEqual("", rng.ToString().Trim());
                }
            }
        }

        [TestMethod]
        public void TestBytes() {
            byte[] noBytes = { };
            byte[] manyBytes = new byte[100];
            byte[] nullBytes = null;
            foreach (RNG rng in rngs) {
                for (int iteration = 0; iteration < iterations; iteration++) {
                    rng.NextBytes(manyBytes);
                    rng.NextBytes(noBytes);
                    rng.NextBytes(nullBytes);
                    Assert.IsNull(nullBytes);
                }
            }
        }
    }
}
