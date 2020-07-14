using Microsoft.VisualStudio.TestTools.UnitTesting;
using SquidLib.SquidMath;

namespace SquidLibTests {
    [TestClass]
    public class RNGTests {
        [TestMethod]
        public void PreviousMustMatch() {
            RNG rng = new RNG();

            ulong ul = rng.NextULong();
            Assert.AreEqual(ul, rng.PreviousULong());

            long l = rng.NextLong();
            Assert.AreEqual(l, rng.PreviousLong());

            uint ui = rng.NextUInt();
            Assert.AreEqual(ui, rng.PreviousUInt());

            int i = rng.NextInt();
            Assert.AreEqual(i, rng.PreviousInt());
        }
        [TestMethod]
        public void ExclusiveOuterBound() {
            RNG rng = new RNG();
            for (uint b = 1; b < 10; b++) {
                for (int i = 0; i < 256; i++) {
                    uint n = rng.NextUInt(b);
                    Assert.AreNotEqual(n, b);
                }
            }
            for (ulong b = 1; b < 10; b++) {
                for (int i = 0; i < 256; i++) {
                    ulong n = rng.NextULong(b);
                    Assert.AreNotEqual(n, b);
                }
            }
            for (int b = -9; b < 11; b += 2) {
                for (int i = 0; i < 256; i++) {
                    int n = rng.NextSignedInt(b);
                    Assert.AreNotEqual(n, b);
                }
            }
            for (long b = -9; b < 11; b += 2) {
                for (int i = 0; i < 256; i++) {
                    long n = rng.NextSignedLong(b);
                    Assert.AreNotEqual(n, b);
                }
            }
        }
    }
}
