﻿using MCRA.Utils.DataSourceReading.ValueConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Tests.UnitTests.DataReading.ValueConversion {
    [TestClass]
    public class BoolValueConverterTests {

        [TestMethod]
        public void BoolValueConverter_TestConvert() {
            var converter = new BoolValueConverter();
            Assert.IsTrue((bool)converter.Convert("1"));
            Assert.IsTrue((bool)converter.Convert("true"));
            Assert.IsTrue((bool)converter.Convert("True"));
            Assert.IsTrue((bool)converter.Convert("4"));
            Assert.IsFalse((bool)converter.Convert("0"));
            Assert.IsFalse((bool)converter.Convert("False"));
            Assert.IsFalse((bool)converter.Convert("false"));
        }

        [TestMethod]
        public void BoolValueConverter_TestReturnType() {
            var converter = new BoolValueConverter();
            Assert.AreEqual(typeof(bool), converter.Convert("true").GetType());
        }

        [TestMethod]
        [DataRow("invalid")]
        [DataRow("123x")]
        [DataRow("f")]
        public void BoolValueConverter_TestFail(string str) {
            var converter = new BoolValueConverter();
            Assert.ThrowsException<FormatException>(() => converter.Convert(str));
        }
    }
}
