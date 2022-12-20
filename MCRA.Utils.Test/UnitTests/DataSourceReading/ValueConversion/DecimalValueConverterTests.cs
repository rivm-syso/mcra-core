using MCRA.Utils.DataSourceReading.ValueConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MCRA.Utils.Tests.UnitTests.DataReading.ValueConversion {
    [TestClass]
    public class DecimalValueConverterTests {

        [TestMethod]
        [DataRow("1", 1)]
        [DataRow("1.0", 1)]
        [DataRow("123.456", 123.456)]
        [DataRow("123,456", 123456)]
        [DataRow("-23.45", -23.45)]
        public void DecimalValueConverter_TestConvert(string str, double expected) {
            var converter = new DecimalValueConverter();
            var value = converter.Convert(str);
            Assert.AreEqual((decimal)expected, value);
        }

        [TestMethod]
        public void DecimalValueConverter_TestReturnType() {
            var converter = new DecimalValueConverter();
            Assert.AreEqual(typeof(decimal), converter.Convert("1").GetType());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        [DataRow("invalid")]
        [DataRow("123x")]
        [DataRow("1e0")]
        [DataRow("1e-1")]
        [DataRow("NA")]
        [DataRow("NaN")]
        [DataRow("infinity")]
        [DataRow("-infinity")]
        public void DecimalValueConverter_TestFail(string str) {
            var converter = new DecimalValueConverter();
            converter.Convert(str);
        }
    }
}
