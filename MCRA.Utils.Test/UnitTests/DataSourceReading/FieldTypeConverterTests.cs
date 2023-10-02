using MCRA.Utils.DataFileReading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.DataSourceReading {

    /// <summary>
    /// FieldTypeConverterTests
    /// </summary>
    [TestClass]
    public class FieldTypeConverterTests {

        [TestMethod]
        public void FieldTypeConverter_TestToSystemTypeMappings() {
            var enumValues = Enum.GetValues(typeof(FieldType)).Cast<FieldType>();
            foreach (var value in enumValues) {
                var systemType = FieldTypeConverter.ToSystemType(value);
                Assert.IsNotNull(systemType);
            }
        }
    }
}
