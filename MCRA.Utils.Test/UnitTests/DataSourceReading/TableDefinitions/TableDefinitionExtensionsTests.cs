using MCRA.Utils.DataFileReading;
using MCRA.Utils.DataSourceReading.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.DataSourceReading.TableDefinitions {

    [TestClass]
    public class TableDefinitionExtensionsTests {

        internal class TestSimple {
            public int? IntField { get; set; }
            public double? NumericField { get; set; }
            public DateTime DateField { get; set; }
            public string AlphaNumericField { get; set; }
            public bool BooleanField { get; set; }
            public FieldType FieldTypeEnum { get; set; }
        }

        [TestMethod]
        public void TableDefinition_TestCreateFromTypeSimple() {
            var tableDef = TableDefinitionExtensions.FromType(typeof(TestSimple));
            Assert.AreEqual(6, tableDef.ColumnDefinitions.Count);
            Assert.AreEqual("IntField", tableDef.ColumnDefinitions[0].Id);
            Assert.AreEqual(FieldType.Integer.ToString(), tableDef.ColumnDefinitions[0].FieldType);
            Assert.IsFalse(tableDef.ColumnDefinitions[0].Required);
            Assert.AreEqual(FieldType.Numeric.ToString(), tableDef.ColumnDefinitions[1].FieldType);
            Assert.IsFalse(tableDef.ColumnDefinitions[1].Required);
            Assert.AreEqual(FieldType.DateTime.ToString(), tableDef.ColumnDefinitions[2].FieldType);
            Assert.IsTrue(tableDef.ColumnDefinitions[2].Required);
            Assert.AreEqual(FieldType.AlphaNumeric.ToString(), tableDef.ColumnDefinitions[3].FieldType);
            Assert.IsFalse(tableDef.ColumnDefinitions[3].Required);
            Assert.AreEqual(FieldType.Boolean.ToString(), tableDef.ColumnDefinitions[4].FieldType);
            Assert.IsTrue(tableDef.ColumnDefinitions[4].Required);
            Assert.AreEqual("FieldType", tableDef.ColumnDefinitions[5].FieldType);
            Assert.IsTrue(tableDef.ColumnDefinitions[5].Required);
        }

        internal class TestIgnore {
            [IgnoreField]
            public int Ignore { get; set; }

            public int Include { get; set; }
        }

        [TestMethod]
        public void TableDefinition_TestCreateFromType_Ignore() {
            var tableDef = TableDefinitionExtensions.FromType(typeof(TestIgnore));
            Assert.AreEqual(1, tableDef.ColumnDefinitions.Count);
            Assert.AreEqual("Include", tableDef.ColumnDefinitions.First().Id);
        }

        internal class TestRequiredNullable {
            [RequiredField]
            public int? RequiredInt { get; set; }

            // Not required
            public int? NonRequiredNullableInt { get; set; }
        }

        [TestMethod]
        public void TableDefinition_TestCreateFromType_RequiredNullable() {
            var tableDef = TableDefinitionExtensions.FromType(typeof(TestRequiredNullable));
            Assert.AreEqual(2, tableDef.ColumnDefinitions.Count);
            Assert.IsTrue(tableDef.ColumnDefinitions[0].Required);
            Assert.IsFalse(tableDef.ColumnDefinitions[1].Required);
        }

        internal class TestEnum {
            public FieldType FieldTypeEnum { get; set; }
        }

        [TestMethod]
        public void TableDefinition_TestCreateFromType_EnumType() {
            var tableDef = TableDefinitionExtensions.FromType(typeof(TestEnum));
            Assert.AreEqual(1, tableDef.ColumnDefinitions.Count);
            Assert.AreEqual("FieldTypeEnum", tableDef.ColumnDefinitions.First().Id);
            // Enum types are always required
            Assert.IsTrue(tableDef.ColumnDefinitions.First().Required);
        }
    }
}
