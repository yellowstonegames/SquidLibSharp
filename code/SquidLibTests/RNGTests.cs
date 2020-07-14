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
    }
}
