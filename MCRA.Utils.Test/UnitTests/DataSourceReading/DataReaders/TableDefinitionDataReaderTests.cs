using MCRA.Utils.DataFileReading;
using MCRA.Utils.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace MCRA.Utils.Test.UnitTests.DataSourceReading.DataReaders {

    [TestClass]
    public class TableDefinitionDataReaderTests {

        [TestMethod]
        public void TableDefinitionDataReader_TestFieldTypes() {
            var tableDef = FakeTableDefinition();
            var csvFilePath = @"Resources/CsvReaderTests/Test.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var dataReader = CreateWrappedCsvReader(stream, tableDef);

                Assert.AreEqual(6, dataReader.FieldCount);
                Assert.AreEqual(typeof(int?), dataReader.GetFieldType(0));
                Assert.AreEqual(typeof(double?), dataReader.GetFieldType(1));
                Assert.AreEqual(typeof(DateTime), dataReader.GetFieldType(2));
                Assert.AreEqual(typeof(string), dataReader.GetFieldType(3));
                Assert.AreEqual(typeof(bool), dataReader.GetFieldType(4));
                Assert.AreEqual(typeof(string), dataReader.GetFieldType(5));
            }
        }

        [TestMethod]
        public void TableDefinitionDataReader_TestReadDataTable() {
            var emptyTableDef = new TableDefinition();
            var csvFilePath = @"Resources/CsvReaderTests/Test.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var dataReader = CreateWrappedCsvReader(stream, emptyTableDef);
                var table = emptyTableDef.CreateDataTable();
                table.Load(dataReader, LoadOption.OverwriteChanges);
                Assert.HasCount(7, table.Rows);
            }
        }

        [TestMethod]
        public void TableDefinitionDataReader_TestReadBoolean() {
            var tableDef = FakeTableDefinition();
            var csvFilePath = @"Resources/CsvReaderTests/Test.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var dataReader = CreateWrappedCsvReader(stream, tableDef);
                var values = ReadAllColumnValues<bool>(dataReader, "BooleanField");
                CollectionAssert.AreEquivalent(values, new[] { false, true, true, false, false, true, false });
            }
        }

        [TestMethod]
        public void TableDefinitionDataReader_TestReadDateTime() {
            var tableDef = FakeTableDefinition();
            var csvFilePath = @"Resources/CsvReaderTests/Test.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var dataReader = CreateWrappedCsvReader(stream, tableDef);
                var values = ReadAllColumnValues<DateTime>(dataReader, "DateField");
                foreach (var value in values) {
                    DateTimeAssert.AreEqual(new DateTime(2001, 2, 1), value, TimeSpan.FromDays(1));
                }
            }
        }

        [TestMethod]
        public void TableDefinitionDataReader_TestFailReadDateTime() {
            var tableDef = FakeTableDefinition();
            var csvFilePath = @"Resources/CsvReaderTests/TestInvalid.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var dataReader = CreateWrappedCsvReader(stream, tableDef);
                Assert.ThrowsExactly<FormatException>(() => ReadAllColumnValues<DateTime>(dataReader, "DateField"));
            }
        }

        [TestMethod]
        public void TableDefinitionDataReader_TestReadAlphaNumeric() {
            var tableDef = FakeTableDefinition();
            var csvFilePath = @"Resources/CsvReaderTests/Test.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var dataReader = CreateWrappedCsvReader(stream, tableDef);
                var values = ReadAllColumnValues<string>(dataReader, "AlphaNumericField");
                CollectionAssert.AreEquivalent(values, new[] { "AAA", "BBB", "CCC", "DDD", "EE E", "FF, F", null });
            }
        }

        [TestMethod]
        public void TableDefinitionDataReader_TestReadNumeric() {
            var tableDef = FakeTableDefinition();
            var csvFilePath = @"Resources/CsvReaderTests/Test.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var dataReader = CreateWrappedCsvReader(stream, tableDef);
                var values = ReadAllColumnValues<double?>(dataReader, "NumericField");
                CollectionAssert.AreEquivalent(values, new double?[] { 2.1, 2.2, null, 2.4, 2.5, null, 2.7 });
            }
        }

        [TestMethod]
        public void TableDefinitionDataReader_TestFailReadNumeric() {
            var tableDef = FakeTableDefinition();
            var csvFilePath = @"Resources/CsvReaderTests/TestInvalid.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var dataReader = CreateWrappedCsvReader(stream, tableDef);
                Assert.ThrowsExactly<FormatException>(() => ReadAllColumnValues<double?>(dataReader, "NumericField"));
            }
        }

        [TestMethod]
        public void TableDefinitionDataReader_TestReadInteger() {
            var tableDef = FakeTableDefinition();
            var csvFilePath = @"Resources/CsvReaderTests/Test.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var dataReader = CreateWrappedCsvReader(stream, tableDef);
                var values = ReadAllColumnValues<int?>(dataReader, "IntField");
                CollectionAssert.AreEquivalent(values, new int?[] { 11, 12, 13, 14, 15, 16, null });
            }
        }

        [TestMethod]
        public void TableDefinitionDataReader_TestFailReadInteger() {
            var tableDef = FakeTableDefinition();
            var csvFilePath = @"Resources/CsvReaderTests/TestInvalid.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var dataReader = CreateWrappedCsvReader(stream, tableDef);
                Assert.ThrowsExactly<FormatException>(() => ReadAllColumnValues<int?>(dataReader, "IntField"));
            }
        }

        private static TableDefinitionDataReader CreateWrappedCsvReader(Stream stream, TableDefinition tableDefinition) {
            return new TableDefinitionDataReader(new CsvDataReader(stream), tableDefinition);
        }

        private static TableDefinition FakeTableDefinition() {
            return new TableDefinition {
                ColumnDefinitions = [
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
                ]
            };
        }

        private static List<T> ReadAllColumnValues<T>(TableDefinitionDataReader reader, string fieldName) {
            var result = new List<T>();
            var colIndex = reader.GetOrdinal(fieldName);
            while (reader.Read()) {
                result.Add((T)(reader.IsDBNull(colIndex) ? null : reader.GetValue(colIndex)));
            }
            return result;
        }
    }
}
