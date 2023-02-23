using MCRA.Utils.DataSourceReading.ValueConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Tests.UnitTests.DataReading.ValueConversion {
    [TestClass]
    public class Int16ValueConverterTests {

        [TestMethod]
        public void Int16ValueConverter_TestConvert() {
            var converter = new Int16ValueConverter();
            Assert.AreEqual(1, (short)converter.Convert("1"));
            Assert.AreEqual(-23, (short)converter.Convert("-23"));
        }

        [TestMethod]
        public void Int16ValueConverter_TestReturnType() {
            var converter = new Int16ValueConverter();
            Assert.AreEqual(typeof(short), converter.Convert("1").GetType());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        [DataRow("invalid")]
        [DataRow("123x")]
        [DataRow("1e-1")]
        [DataRow("123,456")]
        [DataRow("123.456")]
        [DataRow("-23.45")]
        public void Int16ValueConverter_TestFail(string str) {
            var converter = new Int16ValueConverter();
            converter.Convert(str);
        }
    }
}
