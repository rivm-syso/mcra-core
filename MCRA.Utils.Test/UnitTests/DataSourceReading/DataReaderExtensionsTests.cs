using MCRA.Utils.DataFileReading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.DataSourceReading {

    /// <summary>
    /// FieldTypeConverterTests
    /// </summary>
    [TestClass]
    public class DataReaderExtensionsTests {

        [TestMethod]
        public void FieldTypeConverter_TestToSystemTypeMappings() {
            var csvFilePath = @"Resources\CsvReaderTests\Test.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var csvReader = new CsvDataReader(
                    stream,
                    fieldTypes: new[] { typeof(int), typeof(double), typeof(DateTime), typeof(string), typeof(bool), typeof(string) }
                );
                var records = csvReader.ReadRecords<Test>(FakeTableDefinition());
                var record = records[4];
                Assert.AreEqual(15, record.IntField);
                Assert.AreEqual(2.5, record.NumericField);
                Assert.AreEqual(2001, record.DateField.Year);
                Assert.AreEqual(2, record.DateField.Month);
                Assert.AreEqual(1, record.DateField.Day);
                Assert.AreEqual("EE E", record.AlphaNumericField);
                Assert.AreEqual(false, record.BooleanField);
                Assert.AreEqual(7, records.Count);
            }
        }

        internal class Test {
            public int Unmapped { get; set; }
            public int? IntField { get; set; }
            public double? NumericField { get; set; }
            public DateTime DateField { get; set; }
            public string AlphaNumericField { get; set; }
            public bool BooleanField { get; set; }
        }

        private static TableDefinition FakeTableDefinition() {
            return new TableDefinition {
                ColumnDefinitions = new List<ColumnDefinition>() {
                    new ColumnDefinition() {
                        Id = "IntField",
                        FieldType = "Integer",
                        Required = false
                    },
                    new ColumnDefinition() {
                        Id = "NumericField",
                        FieldType = "Numeric",
                        Required = false
                    },
                    new ColumnDefinition() {
                        Id = "DateField",
                        FieldType = "DateTime",
                        Required = true
                    },
                    new ColumnDefinition() {
                        Id = "AlphaNumericField",
                        FieldType = "AlphaNumeric",
                        Required = false
                    },
                    new ColumnDefinition() {
                        Id = "BooleanField",
                        FieldType = "Boolean",
                        Required = true
                    }
                }
            };
        }
    }
}
