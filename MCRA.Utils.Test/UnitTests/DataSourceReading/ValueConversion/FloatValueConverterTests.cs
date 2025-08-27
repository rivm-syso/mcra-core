using MCRA.Utils.DataSourceReading.ValueConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Tests.UnitTests.DataReading.ValueConversion {
    [TestClass]
    public class FloatValueConverterTests {

        [TestMethod]
        [DataRow("1", 1F)]
        [DataRow("1.0", 1F)]
        [DataRow("1e0", 1F)]
        [DataRow("1e-1", .1F)]
        [DataRow("123.456", 123.456F)]
        [DataRow("123,456", 123456F)]
        [DataRow("-23.45", -23.45F)]
        [DataRow("NA", float.NaN)]
        [DataRow("NaN", float.NaN)]
        public void FloatValueConverter_TestConvert(string str, float expected) {
            var converter = new FloatValueConverter();
            var value = converter.Convert(str);
            Assert.AreEqual(expected, value);
        }

        [TestMethod]
        public void FloatValueConverter_TestReturnType() {
            var converter = new FloatValueConverter();
            Assert.AreEqual(typeof(float), converter.Convert("1").GetType());
        }

        [TestMethod]
        [DataRow("invalid")]
        [DataRow("123x")]
        public void FloatValueConverter_TestFail(string str) {
            var converter = new FloatValueConverter();
            Assert.ThrowsExactly<FormatException>(() => converter.Convert(str));
        }
    }
}
