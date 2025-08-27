using MCRA.Utils.DataFileReading;
using MCRA.Utils.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace MCRA.Utils.Test.UnitTests.DataSourceReading.DataReaders {

    [TestClass]
    public class CsvDataReaderTests {

        [TestMethod]
        [DataRow(new string[] { "", "" }, ",")]
        [DataRow(new string[] { }, "")]
        [DataRow(new string[] { "", "", "" }, ",,")]
        [DataRow(new string[] { "", "", "" }, ",,\"\"")]
        [DataRow(new string[] { "", "2, 7", "2001 - 02 - 01T19: 20:30", "" }, "  , \"2, 7\" , \"2001 - 02 - 01T19: 20:30\" ,  ")]
        [DataRow(new string[] { "", "2, 7", "\t3\t" }, "  ,   \"2, 7\" ,  \"\t3\t\"")]
        [DataRow(new string[] { "1,2", "\t3\t" }, "\"1,2\" ,  \"\t3\t\"")]
        public void CsvDataReader_ParseRow(string[] expected, string line) {
            var parsed = CsvDataReader.ParseRow(line, ',', '#');
            CollectionAssert.AreEqual(expected, parsed);
            Assert.AreEqual(expected.Length, parsed.Length);
        }

        [TestMethod]
        public void CsvDataReader_TestFieldTypes() {
            var csvFilePath = @"Resources/CsvReaderTests/Test.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var csvReader = new CsvDataReader(
                    stream,
                    fieldTypes: [typeof(int), typeof(double), typeof(DateTime), typeof(string), typeof(bool), typeof(string)]
                );

                Assert.AreEqual(6, csvReader.FieldCount);
                Assert.AreEqual(typeof(int), csvReader.GetFieldType(0));
                Assert.AreEqual(typeof(double), csvReader.GetFieldType(1));
                Assert.AreEqual(typeof(DateTime), csvReader.GetFieldType(2));
                Assert.AreEqual(typeof(string), csvReader.GetFieldType(3));
                Assert.AreEqual(typeof(bool), csvReader.GetFieldType(4));
                Assert.AreEqual(typeof(string), csvReader.GetFieldType(5));
            }
        }

        [TestMethod]
        public void CsvDataReader_TestReadDataTable() {
            var emptyTableDef = new TableDefinition();
            var csvFilePath = @"Resources/CsvReaderTests/Test.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var csvReader = new CsvDataReader(stream);

                var table = emptyTableDef.CreateDataTable();
                table.Load(csvReader, LoadOption.OverwriteChanges);
                Assert.AreEqual(7, table.Rows.Count);
            }
        }

        [TestMethod]
        public void CsvDataReader_TestReadBoolean() {
            var csvFilePath = @"Resources/CsvReaderTests/Test.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var csvReader = new CsvDataReader(stream);
                var values = ReadAllColumnValues<bool>(csvReader, "BooleanField");
                CollectionAssert.AreEquivalent(values, new[] { false, true, true, false, false, true, false });
            }
        }

        [TestMethod]
        public void CsvDataReader_TestReadDateTime() {
            var csvFilePath = @"Resources/CsvReaderTests/Test.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var csvReader = new CsvDataReader(stream);
                var values = ReadAllColumnValues<DateTime>(csvReader, "DateField");
                foreach (var value in values) {
                    DateTimeAssert.AreEqual(new DateTime(2001, 2, 1), value, TimeSpan.FromDays(1));
                }
            }
        }

        [TestMethod]
        public void CsvDataReader_TestFailReadDateTime() {
            var csvFilePath = @"Resources/CsvReaderTests/TestInvalid.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var csvReader = new CsvDataReader(stream);
                Assert.ThrowsExactly<FormatException>(() => ReadAllColumnValues<DateTime>(csvReader, "DateField"));
            }
        }

        [TestMethod]
        public void CsvDataReader_TestReadAlphaNumeric() {
            var csvFilePath = @"Resources/CsvReaderTests/Test.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var csvReader = new CsvDataReader(stream);
                var values = ReadAllColumnValues<string>(csvReader, "AlphaNumericField");
                CollectionAssert.AreEquivalent(values, new[] { "AAA", "BBB", "CCC", "DDD", "EE E", "FF, F", string.Empty});
            }
        }

        [TestMethod]
        public void CsvDataReader_TestReadNumeric() {
            var csvFilePath = @"Resources/CsvReaderTests/Test.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var csvReader = new CsvDataReader(stream);
                var values = ReadAllColumnValues<double>(csvReader, "NumericField");
                CollectionAssert.AreEquivalent(values, new double[] { 2.1, 2.2, double.NaN, 2.4, 2.5, double.NaN, 2.7 });
            }
        }

        [TestMethod]
        public void CsvDataReader_TestFailReadNumeric() {
            var csvFilePath = @"Resources/CsvReaderTests/TestInvalid.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var csvReader = new CsvDataReader(stream);
                Assert.ThrowsExactly<FormatException>(() => ReadAllColumnValues<double>(csvReader, "NumericField"));
            }
        }

        [TestMethod]
        public void CsvDataReader_TestReadInteger() {
            var csvFilePath = @"Resources/CsvReaderTests/Test.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var csvReader = new CsvDataReader(stream);
                var values = ReadAllColumnValues<int?>(csvReader, "IntField");
                CollectionAssert.AreEquivalent(values, new int?[] { 11, 12, 13, 14, 15, 16, null });
            }
        }

        [TestMethod]
        public void CsvDataReader_TestFailReadInteger() {
            var csvFilePath = @"Resources/CsvReaderTests/TestInvalid.csv";
            using (var reader = new StreamReader(csvFilePath)) {
                var stream = reader.BaseStream;
                var csvReader = new CsvDataReader(stream, fieldTypes: [typeof(int)]);
                Assert.ThrowsExactly<FormatException>(() => ReadAllColumnValues<int?>(csvReader, "IntField"));
            }
        }

        private static List<T> ReadAllColumnValues<T>(CsvDataReader reader, string fieldName) {
            var result = new List<T>();
            while (reader.Read()) {
                var colIndex = reader.GetOrdinal(fieldName);
                result.Add(reader.GetValue<T>(colIndex));
            }
            return result;
        }
    }
}
