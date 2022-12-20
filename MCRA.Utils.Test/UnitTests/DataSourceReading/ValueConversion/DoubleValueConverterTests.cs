using MCRA.Utils.DataSourceReading.ValueConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MCRA.Utils.Tests.UnitTests.DataReading.ValueConversion {
    [TestClass]
    public class DoubleValueConverterTests {

        [TestMethod]
        [DataRow("1", 1)]
        [DataRow("1.0", 1)]
        [DataRow("1e0", 1)]
        [DataRow("1e-1", .1)]
        [DataRow("123.456", 123.456)]
        [DataRow("123,456", 123456)]
        [DataRow("-23.45", -23.45)]
        [DataRow("NA", double.NaN)]
        [DataRow("NaN", double.NaN)]
        [DataRow("-", double.NaN)]
        [DataRow("Inf", double.PositiveInfinity)]
        [DataRow("infinity", double.PositiveInfinity)]
        [DataRow("-Inf", double.NegativeInfinity)]
        [DataRow("-infinity", double.NegativeInfinity)]
        public void DoubleValueConverter_TestConvert(string str, double expected) {
            var converter = new DoubleValueConverter();
            var value = converter.Convert(str);
            Assert.AreEqual(expected, value);
        }

        [TestMethod]
        public void DoubleValueConverter_TestReturnType() {
            var converter = new DoubleValueConverter();
            Assert.AreEqual(typeof(double), converter.Convert("1").GetType());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        [DataRow("invalid")]
        [DataRow("123x")]
        public void DoubleValueConverter_TestFail(string str) {
            var converter = new DoubleValueConverter();
            converter.Convert(str);
        }
    }
}
