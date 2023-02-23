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

        [TestMethod]
        public void FieldTypeConverter_TestFromSystemTypeString() {
            var testCases = new List<(string Str, FieldType FieldType)>() {
                ("Double", FieldType.Numeric),
                ("String", FieldType.AlphaNumeric),
                ("Int", FieldType.Integer),
                ("Int32", FieldType.Integer),
                ("Int64", FieldType.Integer),
                ("Bool", FieldType.Boolean),
                ("Boolean", FieldType.Boolean),
                ("Byte", FieldType.Integer),
                ("DateTime", FieldType.DateTime),
                ("XXX", FieldType.Undefined),
                ("", FieldType.Undefined)
            };
            foreach (var testCase in testCases) {
                var fieldType = FieldTypeConverter.FromSystemTypeString(testCase.Str);
                Assert.AreEqual(testCase.FieldType, fieldType);
            }
        }
    }
}
