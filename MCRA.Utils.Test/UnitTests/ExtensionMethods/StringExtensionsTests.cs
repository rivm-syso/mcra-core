using System.Diagnostics;
using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {

    [TestClass]
    public class StringExtensionsTests {

        [TestMethod]
        public void GetRangeStringsTest1() {
            var s = "-10,12,13,20-50,60-100,300-";
            foreach (var r in s.GetRangeStrings()) {
                Trace.WriteLine(r);
            }
            Assert.AreEqual(2, s.GetRangeStrings().Count());
        }

        [TestMethod]
        public void GetSmallerThanString() {
            var s = "12,13,20-50,-10,60-100,300-";
            var actual = s.GetSmallerEqualString();
            Assert.AreEqual("-10", actual);
        }

        [TestMethod]
        public void GetGreaterThanString() {
            var s = "12,13,20-50,-10,60-100,300-";
            var actual = s.GetGreaterEqualString();
            Assert.AreEqual("300-", actual);
        }

        /// <summary>
        /// Tests the method that concatenates a string to another string
        /// and generates a new Guid from this. This test starts with an
        /// all-zero guid.
        /// </summary>
        [TestMethod]
        public void StringExtensions_MungeHashStringTest1() {
            var guid = Guid.Empty;
            var mungeString = "Blablabla";
            var guidString = guid.ToString();
            var result = guidString.MungeToGuid(mungeString);
            Assert.AreEqual("619a5d4b-af7f-5f59-9dee-a86b338443eb", result);
            result = guidString.MungeToGuid(mungeString + "a");
            Assert.AreNotEqual("619a5d4b-af7f-5f59-9dee-a86b338443eb", result);
        }

        /// <summary>
        /// Tests the method that concatenates a string to another string
        /// and generates a new Guid from this. This test starts with a random
        /// guid.
        /// </summary>
        [TestMethod]
        public void StringExtensions_MungeHashStringTest2() {
            var guid = new Guid("619a5d4b-af7f-5f59-9dee-a86b338443eb");
            var mungeString = "Blablabla";
            var guidString = guid.ToString();
            var result = guidString.MungeToGuid(mungeString);
            Assert.AreEqual("b97ca4f2-6f00-3502-c25e-3a9efe58343d", result);
            result = guidString.MungeToGuid(mungeString + "a");
            Assert.AreNotEqual("b97ca4f2-6f00-3502-c25e-3a9efe58343d", result);
        }

        [TestMethod]
        public void SplitStringInToInt() {
            var s = " 1 2 3     4   ";
            var actual = s.SplitToIntArray();
            Assert.HasCount(4, actual);
        }

        [TestMethod]
        public void SplitStringInToDouble() {
            var s = "  1.0 2 3   4   ";
            var actual = s.SplitToInvariantDoubleArray();
            Assert.HasCount(4, actual);
        }
    }
}
