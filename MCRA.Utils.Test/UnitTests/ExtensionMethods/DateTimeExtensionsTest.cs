using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {

    [TestClass()]
    public class DateTimeExtensionsTest {

        [TestMethod]
        public void IsBetweenTest1() {
            DateTime source = new DateTime(2011, 3, 1);
            DateTime lower = new DateTime(2011, 3, 1);
            DateTime upper = new DateTime(2011, 4, 1);
            bool expected = true;
            bool actual;
            actual = DateTimeExtensions.IsBetween(source, lower, upper);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void IsBetweenTest2() {
            DateTime source = new DateTime(2011, 4, 1);
            DateTime lower = new DateTime(2011, 3, 1);
            DateTime upper = new DateTime(2011, 4, 1);
            bool expected = true;
            bool actual;
            actual = DateTimeExtensions.IsBetween(source, lower, upper);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void IsBetweenTest3() {
            DateTime source = new DateTime(2011, 3, 20);
            DateTime lower = new DateTime(2011, 3, 1);
            DateTime upper = new DateTime(2011, 4, 1);
            bool expected = true;
            bool actual;
            actual = DateTimeExtensions.IsBetween(source, lower, upper);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void IsBetweenTest4() {
            DateTime source = new DateTime(2011, 4, 8);
            DateTime lower = new DateTime(2011, 3, 1);
            DateTime upper = new DateTime(2011, 4, 1);
            bool expected = false;
            bool actual;
            actual = DateTimeExtensions.IsBetween(source, lower, upper);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void IsBetweenTest5() {
            DateTime source = new DateTime(2011, 2, 8);
            DateTime lower = new DateTime(2011, 3, 1);
            DateTime upper = new DateTime(2011, 4, 1);
            bool expected = false;
            bool actual;
            actual = DateTimeExtensions.IsBetween(source, lower, upper);
            Assert.AreEqual(expected, actual);
        }

    }
}
