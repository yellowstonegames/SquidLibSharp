using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SquidLibTests {
    // from https://stackoverflow.com/questions/1944774/in-mstest-how-can-i-verify-exact-error-message-using-expectedexceptiontypeof
    public static class AssertHasException {
        public static void Throws<TException>(Action action, string message)
            where TException : Exception {
            try {
                action();
                Assert.Fail("Exception of type {0} expected; did not get exception", typeof(TException).Name);
            } catch (TException ex) {
                Assert.AreEqual(message, ex.Message);
            } catch (Exception ex) {
                Assert.Fail("Exception of type {0} expected; got exception of type {1}", typeof(TException).Name, ex.GetType().Name);
            }
        }

        public static void Throws<TException>(Action action)
            where TException : Exception {
            try {
                action();
                Assert.Fail("Exception of type {0} expected; did not get exception", typeof(TException).Name);
            } catch (TException ex) {
                // Success path, ignoring message
            } catch (Exception ex) {
                Assert.Fail("Exception of type {0} expected; got exception of type {1}", typeof(TException).Name, ex.GetType().Name);
            }
        }
    }
}
