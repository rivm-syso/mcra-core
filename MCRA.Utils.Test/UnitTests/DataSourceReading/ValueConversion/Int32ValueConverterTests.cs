using MCRA.Utils.DataSourceReading.ValueConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MCRA.Utils.Tests.UnitTests.DataReading.ValueConversion {
    [TestClass]
    public class Int32ValueConverterTests {

        [TestMethod]
        public void Int32ValueConverter_TestConvert() {
            var converter = new Int32ValueConverter();
            Assert.AreEqual(1, (int)converter.Convert("1"));
            Assert.AreEqual(-23, (int)converter.Convert("-23"));
        }

        [TestMethod]
        public void Int32ValueConverter_TestReturnType() {
            var converter = new Int32ValueConverter();
            Assert.AreEqual(typeof(int), converter.Convert("1").GetType());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        [DataRow("invalid")]
        [DataRow("123x")]
        [DataRow("1e-1")]
        [DataRow("123,456")]
        [DataRow("123.456")]
        [DataRow("-23.45")]
        public void Int32ValueConverter_TestFail(string str) {
            var converter = new Int32ValueConverter();
            converter.Convert(str);
        }
    }
}
