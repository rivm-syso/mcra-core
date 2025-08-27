using MCRA.Utils.DataSourceReading.ValueConversion;

namespace MCRA.Utils.Tests.UnitTests.DataReading.ValueConversion {
    [TestClass]
    public class Int64ValueConverterTests {

        [TestMethod]
        public void Int64ValueConverter_TestConvert() {
            var converter = new Int64ValueConverter();
            Assert.AreEqual(1L, (long)converter.Convert("1"));
            Assert.AreEqual(-23L, (long)converter.Convert("-23"));
        }

        [TestMethod]
        public void Int64ValueConverter_TestReturnType() {
            var converter = new Int64ValueConverter();
            Assert.AreEqual(typeof(long), converter.Convert("1").GetType());
        }

        [TestMethod]
        [DataRow("invalid")]
        [DataRow("123x")]
        [DataRow("123,456")]
        [DataRow("123.456")]
        [DataRow("-23.45")]
        [DataRow("1e-1")]
        public void Int64ValueConverter_TestFail(string str) {
            var converter = new Int64ValueConverter();
            Assert.ThrowsExactly<FormatException>(() => converter.Convert(str));
        }
    }
}
