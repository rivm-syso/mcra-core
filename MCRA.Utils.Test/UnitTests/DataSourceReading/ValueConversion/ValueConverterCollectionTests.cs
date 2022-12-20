using MCRA.Utils.DataSourceReading.ValueConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MCRA.Utils.Tests.UnitTests.DataReading.ValueConversion {
    [TestClass]
    public class ValueConverterCollectionTests {

        [TestMethod]
        public void CsvReaderExtensions_TestDefaultConverterCollection() {
            var converters = ValueConverterCollection.Default();
            Assert.IsTrue(converters.Get(typeof(bool)) is BoolValueConverter);
            Assert.IsTrue(converters.Get(typeof(DateTime)) is DateTimeValueConverter);
            Assert.IsTrue(converters.Get(typeof(decimal)) is DecimalValueConverter);
            Assert.IsTrue(converters.Get(typeof(double)) is DoubleValueConverter);
            Assert.IsTrue(converters.Get(typeof(float)) is FloatValueConverter);
            Assert.IsTrue(converters.Get(typeof(short)) is Int16ValueConverter);
            Assert.IsTrue(converters.Get(typeof(int)) is Int32ValueConverter);
            Assert.IsTrue(converters.Get(typeof(long)) is Int64ValueConverter);
            Assert.IsTrue(converters.Get(typeof(string)) is StringValueConverter);
        }

        [TestMethod]
        [DataRow("1", typeof(int), 1)]
        [DataRow("1e3", typeof(double), 1e3)]
        [DataRow("1.23", typeof(float), 1.23F)]
        [DataRow("true", typeof(bool), true)]
        [DataRow("0", typeof(bool), false)]
        [DataRow("-3", typeof(short), (short)-3)]
        [DataRow("-30", typeof(long), -30L)]
        public void CsvReaderExtensions_TestConvert(string value, Type type, object expected) {
            var converters = ValueConverterCollection.Default();
            Assert.AreEqual(expected, converters.Convert(value, type));
        }
    }
}
