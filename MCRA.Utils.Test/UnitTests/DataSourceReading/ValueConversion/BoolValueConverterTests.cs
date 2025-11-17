using MCRA.Utils.DataSourceReading.ValueConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Tests.UnitTests.DataReading.ValueConversion {
    [TestClass]
    public class BoolValueConverterTests {

        [TestMethod]
        [DataRow("1")]
        [DataRow("true")]
        [DataRow("t")]
        [DataRow("True")]
        [DataRow("T")]
        [DataRow("YES")]
        [DataRow("y")]
        [DataRow("4")]
        public void BoolValueConverter_TestConvertTrue(string str) {
            var converter = new BoolValueConverter();
            Assert.IsTrue((bool)converter.Convert(str));
        }

        [TestMethod]
        [DataRow("0")]
        [DataRow("False")]
        [DataRow("F")]
        [DataRow("false")]
        [DataRow("f")]
        [DataRow("N")]
        [DataRow("n")]
        [DataRow("No")]
        public void BoolValueConverter_TestConvertFalse(string str) {
            var converter = new BoolValueConverter();
            Assert.IsFalse((bool)converter.Convert(str));
        }

        [TestMethod]
        public void BoolValueConverter_TestReturnType() {
            var converter = new BoolValueConverter();
            Assert.AreEqual(typeof(bool), converter.Convert("true").GetType());
        }

        [TestMethod]
        [DataRow("invalid")]
        [DataRow("123x")]
        [DataRow("Nein")]
        [DataRow("0.1")]
        [DataRow("9,0")]
        public void BoolValueConverter_TestFail(string str) {
            var converter = new BoolValueConverter();
            Assert.ThrowsExactly<FormatException>(() => converter.Convert(str));
        }
    }
}
