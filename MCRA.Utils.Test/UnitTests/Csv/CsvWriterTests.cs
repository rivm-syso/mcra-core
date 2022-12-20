using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace MCRA.Utils.Csv.Tests {
    /// <summary>
    /// Tests CSV writing
    /// </summary>
    [TestClass]
    public class CsvWriterTests {
        private string _tempCsvName;
        /// <summary>
        /// Before every test that is run
        /// </summary>
        [TestInitialize]
        public void TestInitialize() {
            _tempCsvName = Path.Combine(Path.GetTempPath(), $"TestCsv{Guid.NewGuid()}.csv");
        }
        /// <summary>
        /// Test cleanup, delete any created file
        /// </summary>
        [TestCleanup]
        public void TestCleanup() {
            CsvWriter.SignificantDigits = 0;
            if (File.Exists(_tempCsvName)) {
                File.Delete(_tempCsvName);
            }
        }

        /// <summary>
        /// CsvWriter_WriteToCsvFileWithoutHeaderTest
        /// </summary>
        [TestMethod()]
        public void CsvWriter_WriteToCsvFileWithoutHeaderTest() {
            CsvWriter.WriteToCsvFile(new List<Tuple<string, string>>(), _tempCsvName, false);
            var data = File.ReadAllLines(_tempCsvName);
            Assert.AreEqual(0, data.Length);
        }

        /// <summary>
        /// CsvWriter_WriteToCsvFileTestHeaderOnlyTest
        /// </summary>
        [TestMethod]
        public void CsvWriter_WriteToCsvFileTestHeaderOnlyTest() {
            //write directly
            CsvWriter.WriteToCsvFile(new List<Tuple<string, string>>(), _tempCsvName);
            var data = File.ReadAllLines(_tempCsvName);
            Assert.AreEqual(1, data.Length);
            Assert.AreEqual("\"Item1\",\"Item2\"", data[0]);

            //overwrite file using type parameter
            var records = new Tuple<int, double, bool>[0];
            CsvWriter.WriteToCsvFile(records, typeof(Tuple<int, double, bool>), _tempCsvName);
            data = File.ReadAllLines(_tempCsvName);
            Assert.AreEqual(1, data.Length);
            Assert.AreEqual("\"Item1\",\"Item2\",\"Item3\"", data[0]);
        }

        /// <summary>
        /// CsvWriter_WriteToCsvFileWithDataTest
        /// </summary>
        [TestMethod]
        public void CsvWriter_WriteToCsvFileWithDataTest() {
            //create list of data
            var records = new[] {
                new Tuple<string, int, double, bool>("A", 2, .5, true),
                new Tuple<string, int, double, bool>("No", -100, double.NaN, false)
            };
            CsvWriter.WriteToCsvFile(records, _tempCsvName);
            var data = File.ReadAllLines(_tempCsvName);
            Assert.AreEqual(3, data.Length);
            Assert.AreEqual("\"Item1\",\"Item2\",\"Item3\",\"Item4\"", data[0]);
            Assert.AreEqual("\"A\",2,0.5,\"True\"", data[1]);
            Assert.AreEqual("\"No\",-100,NaN,\"False\"", data[2]);
        }

        /// <summary>
        /// CsvWriter_WriteToCsvFileWithRoundedDataTest
        /// </summary>
        [TestMethod]
        public void CsvWriter_WriteToCsvFileWithSignificantDigitsTest() {
            //create list of data
            var records = new [] {
                new Tuple<int, double>(2, .394123456E-50),
                new Tuple<int, double>(-39049874, 55555E40),
                new Tuple<int, double>(999, double.NaN),
                new Tuple<int, double>(89999, 123456.00000),
                new Tuple<int, double>(89999, 123456.7),
                new Tuple<int, double>(-454648451, double.NegativeInfinity),
                new Tuple<int, double>(999999999, double.PositiveInfinity),
                new Tuple<int, double>(int.MaxValue, 0.12345),
                new Tuple<int, double>(int.MinValue, 0.012345),
                new Tuple<int, double>(-1, 0.0012345),
                new Tuple<int, double>(0, 0.00012345),
                new Tuple<int, double>(0, 0.000012345),
                new Tuple<int, double>(-0, 0.00000000012345),
                new Tuple<int, double>(-0, 999.499999999),
                new Tuple<int, double>(-0, 999.5)
            };

            CsvWriter.SignificantDigits = 3;
            CsvWriter.WriteToCsvFile(records, _tempCsvName);

            var data = File.ReadAllLines(_tempCsvName);
            var i = 0;
            Assert.AreEqual(records.Length + 1, data.Length);
            Assert.AreEqual("\"Item1\",\"Item2\"", data[i++]);
            Assert.AreEqual("2,3.94E-51", data[i++]);
            Assert.AreEqual("-39049874,5.56E44", data[i++]);
            Assert.AreEqual("999,NaN", data[i++]);
            Assert.AreEqual("89999,1.23E05", data[i++]);
            Assert.AreEqual("89999,1.23E05", data[i++]);
            Assert.AreEqual("-454648451,-Infinity", data[i++]);
            Assert.AreEqual("999999999,Infinity", data[i++]);
            Assert.AreEqual($"{int.MaxValue},0.123", data[i++]);
            Assert.AreEqual($"{int.MinValue},0.0123", data[i++]);
            Assert.AreEqual("-1,0.00123", data[i++]);
            Assert.AreEqual("0,0.000123", data[i++]);
            Assert.AreEqual("0,1.23E-05", data[i++]);
            Assert.AreEqual("0,1.23E-10", data[i++]);
            Assert.AreEqual("0,999", data[i++]);
            Assert.AreEqual("0,1E03", data[i++]);
        }
    }
}