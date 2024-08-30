using System.Data;
using MCRA.Data.Raw.Copying;
using MCRA.Data.Raw.Test.Helpers;
using MCRA.General;
using MCRA.General.TableDefinitions;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MCRA.Data.Raw.Test.UnitTests.Copying {

    /// <summary>
    /// RawDataSourceBulkCopierTests
    /// </summary>
    [TestClass]
    public class RawDataSourceBulkCopierTests {

        /// <summary>
        /// Delegate function for GetDataReaderByDefinition of mock class.
        /// </summary>
        /// <param name="tableDef">Table definition</param>
        /// <param name="sourceTableName"></param>
        public delegate void GetDataReaderByDefinitionDelegate(TableDefinition tableDef, out string sourceTableName);

        /// <summary>
        /// RawDataSourceBulkCopier_RawDataSourceBulkCopierNullTest
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), AllowDerivedTypes = false)]
        public void RawDataSourceBulkCopier_NullTest() {
            var copier = new RawDataSourceBulkCopier(null);
            copier.CopyFromDataSourceReader(null, allowEmptyDataSource: true);
        }

        /// <summary>
        /// Create a raw data source bulk copier and run the copy from data source method
        /// on an empty data source reader. Assert that:
        /// - a reader was opened for every source table group;
        /// - check all source table groups against the enum, at least all source table groups that
        ///   were called (the readTableNames) should be in the Enum names.
        /// </summary>
        [TestMethod()]
        public void RawDataSourceBulkCopier_CopyAllTablesInDataSourceEmptyReaderTest() {
            var writerMock = new Mock<IDataSourceWriter>();
            var readerMock = new Mock<IDataSourceReader>();
            string name = null;
            var readTableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            //set a callback on the GetDataReaderByDefinition to retrieve the source table groups
            GetDataReaderByDefinitionDelegate gdrByDefCallback = delegate (TableDefinition t, out string s) {
                s = null;
                readTableNames.Add(t.Id);
            };
            readerMock.Setup(m => m.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name))
                .Callback(gdrByDefCallback);
            readerMock.Setup(m => m.GetTableNames()).Returns([]);
            var copier = new RawDataSourceBulkCopier(writerMock.Object);

            // Act
            var result = copier.CopyFromDataSourceReader(readerMock.Object, allowEmptyDataSource: true);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);

            // Assert that a reader was opened for every source table group
            var enumCount = Enum.GetValues(typeof(SourceTableGroup))
                .Cast<SourceTableGroup>()
                .Count(r => r != SourceTableGroup.Unknown && r != SourceTableGroup.FocalFoods);

            readerMock.Verify(x => x.Open(), Times.Once);
            readerMock.Verify(x => x.Close(), Times.Once);
            readerMock.Verify(x => x.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name), Times.AtLeast(enumCount));

            // Check all source table groups against the enum, at least all source table groups that
            // were called (the readTableNames) should be in the Enum names
            var checkEnumList = Enum.GetNames(typeof(RawDataSourceTableID)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            Assert.IsTrue(readTableNames.All(tableName => checkEnumList.Contains(tableName)));
        }

        /// <summary>
        /// RawDataSourceBulkCopier_CopyAllTablesInDataSourceWithDataGroupsTest
        /// </summary>
        [TestMethod()]
        public void RawDataSourceBulkCopier_CopyAllTablesInDataSourceWithDataGroupsTest() {
            var writerMock = new Mock<IDataSourceWriter>();
            var readerMock = new Mock<IDataSourceReader>();
            string name = null;

            var readTableNames = new HashSet<string>();
            //set a callback on the GetDataReaderByDefinition to retrieve the source table groups
            GetDataReaderByDefinitionDelegate gdrByDefCallback = delegate (TableDefinition t, out string s) {
                s = null;
                readTableNames.Add(t.Id);
            };
            readerMock.Setup(m => m.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name))
                      .Callback(gdrByDefCallback);

            var copier = new RawDataSourceBulkCopier(writerMock.Object);
            var tableGroups = new SourceTableGroup[] {
                SourceTableGroup.AgriculturalUse,
                SourceTableGroup.DoseResponseData,
                SourceTableGroup.Effects,
                SourceTableGroup.Foods,
                SourceTableGroup.InterSpeciesFactors,
                SourceTableGroup.MarketShares,
                SourceTableGroup.QsarMembershipModels
            };
            var result = copier.CopyFromDataSourceReader(
                readerMock.Object,
                tableGroups: tableGroups,
                allowEmptyDataSource: true
            );

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);

            readerMock.Verify(x => x.Open(), Times.Once);
            readerMock.Verify(x => x.Close(), Times.Once);
            readerMock.Verify(x => x.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name), Times.Exactly(tableGroups.Length));
            //check all source table groups against the enum, all source table groups in the array should have been called
            var checkEnumList = tableGroups
                .SelectMany(r => McraTableDefinitions.Instance.GetTableGroupRawTables(r))
                .Select(r => r.ToString())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            Assert.IsTrue(readTableNames.All(tableName => checkEnumList.Contains(tableName)));
        }

        /// <summary>
        /// RawDataSourceBulkCopier integration test. CopyAllTablesInDataSourceCompoundsConcentrationsTest.
        /// </summary>
        [TestMethod()]
        public void RawDataSourceBulkCopier_CopyAllTablesInDataSourceCompoundsConcentrationsTest() {
            var writerMock = new Mock<IDataSourceWriter>();
            var readerMock = new Mock<IDataSourceReader>();
            string name = null;

            var readTableNames = new HashSet<string>();
            //set a callback on the GetDataReaderByDefinition to retrieve the source table groups
            GetDataReaderByDefinitionDelegate gdrByDefCallback = delegate (TableDefinition t, out string s) {
                s = null;
                readTableNames.Add(t.Id);
            };
            readerMock.Setup(m => m.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name))
                      .Callback(gdrByDefCallback);

            var copier = new RawDataSourceBulkCopier(writerMock.Object);
            var result = copier.CopyFromDataSourceReader(
                readerMock.Object,
                tableGroups: new SourceTableGroup[] { SourceTableGroup.Compounds },
                allowEmptyDataSource: true
            );

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);

            readerMock.Verify(x => x.Open(), Times.Once);
            readerMock.Verify(x => x.Close(), Times.Once);
            readerMock.Verify(x => x.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name), Times.Exactly(2));

            // Check all source table groups against the enum, all source table groups in the array should have been called
            Assert.AreEqual(2, readTableNames.Count);
            Assert.IsTrue(readTableNames.Contains(RawDataSourceTableID.Compounds.ToString()));
            Assert.IsTrue(readTableNames.Contains(RawDataSourceTableID.ConcentrationsSSD.ToString()));
        }

        /// <summary>
        /// RawDataSourceBulkCopier integration test: test copy compounds with compounds table provided.
        /// </summary>
        [TestMethod]
        public void RawDataSourceBulkCopier_TestCopyCompoundsWithCompoundsTable() {
            var writerMock = new Mock<IDataSourceWriter>();
            var readerMock = new Mock<IDataSourceReader>();
            string name = null;

            //create empty table objects
            var tbSubst = new DataTable();
            var tableId = "";

            //set a callback on the GetDataReaderByDefinition to retrieve the source table groups
            GetDataReaderByDefinitionDelegate gdrByDefCallback = delegate (TableDefinition t, out string s) {
                s = null;
                tableId = t.Id;
            };
            readerMock.Setup(m => m.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name))
                      .Callback(gdrByDefCallback)
                      .Returns(() => tableId == "Compounds" ? tbSubst.CreateDataReader() : null);

            var copier = new RawDataSourceBulkCopier(writerMock.Object);
            var result = copier.CopyFromDataSourceReader(
                readerMock.Object,
                tableGroups: new[] { SourceTableGroup.Compounds },
                allowEmptyDataSource: true
            );

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(SourceTableGroup.Compounds, result.Single());

            readerMock.Verify(x => x.Open(), Times.Once);
            readerMock.Verify(x => x.Close(), Times.Once);
            readerMock.Verify(x => x.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name), Times.Once);
        }

        /// <summary>
        /// RawDataSourceBulkCopier integration test: test copy compounds with provided SSD concentrations
        /// table.
        /// </summary>
        [TestMethod]
        public void RawDataSourceBulkCopier_TestCopyCompoundsDataWithSsdConcentrationsTable() {
            var writerMock = new Mock<IDataSourceWriter>();
            var readerMock = new Mock<IDataSourceReader>();

            var tbSsd = new DataTable();
            string name = null;
            var tableId = "";

            // Set a callback on the GetDataReaderByDefinition to retrieve the source table groups
            GetDataReaderByDefinitionDelegate gdrByDefCallback = delegate (TableDefinition t, out string s) {
                s = null;
                tableId = t.Id;
            };

            // Set up a reader that only returns a ConcentrationsSSD table
            readerMock.Setup(m => m.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name))
                      .Callback(gdrByDefCallback)
                      .Returns(() => tableId == "ConcentrationsSSD" ? tbSsd.CreateDataReader() : null);

            var copier = new RawDataSourceBulkCopier(writerMock.Object);
            var result = copier.CopyFromDataSourceReader(
                readerMock.Object,
                tableGroups: new[] { SourceTableGroup.Compounds },
                allowEmptyDataSource: true
            );

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(SourceTableGroup.Compounds, result.Single());

            readerMock.Verify(x => x.Open(), Times.Once);
            readerMock.Verify(x => x.Close(), Times.Once);
            readerMock.Verify(x => x.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name), Times.Exactly(2));
        }

        /// <summary>
        /// RawDataSourceBulkCopier integration test: test copy compounds with no data tables provided.
        /// </summary>
        [TestMethod]
        public void RawDataSourceBulkCopier_TestCopyCompoundsWithoutData() {
            var writerMock = new Mock<IDataSourceWriter>();
            var readerMock = new Mock<IDataSourceReader>();

            var copier = new RawDataSourceBulkCopier(writerMock.Object);

            string name = null;
            var tableId = "";
            //set a callback on the GetDataReaderByDefinition to retrieve the source table groups
            GetDataReaderByDefinitionDelegate gdrByDefCallback = delegate (TableDefinition t, out string s) {
                s = null;
                tableId = t.Id;
            };
            //set up a reader that only returns the AgriculturalUses table
            readerMock.Setup(m => m.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name))
                      .Callback(gdrByDefCallback)
                      .Returns(() => null);

            var result = copier.CopyFromDataSourceReader(
                readerMock.Object,
                tableGroups: new[] { SourceTableGroup.Compounds },
                allowEmptyDataSource: true
            );

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        /// <summary>
        /// RawDataSourceBulkCopier_CopyAllTablesInDataSourceCancelTokenTest
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(OperationCanceledException))]
        public void RawDataSourceBulkCopier_CopyAllTablesInDataSourceCancelTokenTest() {
            var writerMock = new Mock<IDataSourceWriter>();
            var readerMock = new Mock<IDataSourceReader>();
            string name = null;

            var copier = new RawDataSourceBulkCopier(writerMock.Object);
            //use a cancelled token
            var cancelToken = new CancellationToken(true);
            var progressState = new CompositeProgressState(cancelToken);

            try {
                var result = copier.CopyFromDataSourceReader(
                    readerMock.Object,
                    allowEmptyDataSource: true,
                    progressState);
            } finally {
                readerMock.Verify(x => x.Open(), Times.Once);
                readerMock.Verify(x => x.Close(), Times.Once);
                readerMock.Verify(x => x.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name), Times.Never);
            }
        }

        /// <summary>
        /// RawDataSourceBulkCopier_CopyAllTablesInDataSourceFromNonExistingFileNameTest
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(IOException), AllowDerivedTypes = false)]
        public void RawDataSourceBulkCopier_CopyAllTablesInDataSourceFromFileNameTest() {
            var writerMock = new Mock<IDataSourceWriter>();

            var copier = new RawDataSourceBulkCopier(writerMock.Object);
            copier.CopyFromDataFile("test.mdb");
        }

        /// <summary>
        /// RawDataSourceBulkCopier_TestCopyFromDataTables_AllowEmpty
        /// </summary>
        [TestMethod]
        public void RawDataSourceBulkCopier_TestCopyFromDataTables_AllowEmpty() {
            var writerMock = new Mock<IDataSourceWriter>();
            var copier = new RawDataSourceBulkCopier(writerMock.Object);

            // Use an empty data table as data source, this should not throw an exception
            var result = copier.CopyFromDataTables(new DataTable[0], allowEmptyDataSource: true);
            //but also no results
            Assert.AreEqual(0, result.Count);
        }

        /// <summary>
        /// RawDataSourceBulkCopier_CopyAllTablesInDataSourceFromRawDataTablesTest
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(RawDataSourceBulkCopyException))]
        public void RawDataSourceBulkCopier_TestCopyFromDataTables_DontAllowEmpty() {
            var writerMock = new Mock<IDataSourceWriter>();
            var copier = new RawDataSourceBulkCopier(writerMock.Object);

            // Use an empty data table as data source, this should not throw an exception
            var result = copier.CopyFromDataTables(new DataTable[0], allowEmptyDataSource: false);
            //but also no results
            Assert.AreEqual(0, result.Count);
        }

        /// <summary>
        /// Integration test, processes the data source file DataGroupsTests.mdb.
        /// </summary>
        [TestMethod]
        public void RawDataSourceBulkCopier_TestCopyDataGroups() {
            var writerMock = new Mock<IDataSourceWriter>();
            var copier = new RawDataSourceBulkCopier(writerMock.Object);
            var result = copier.CopyFromDataFile(TestUtils.GetResource("DataGroupsTests.mdb"));
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// Integration test, processes the xls data source file ConcentrationsSSD.xls and
        /// asserts whether the copied table groups are the ones expected.
        /// </summary>
        [TestMethod]
        public void RawDataSourceBulkCopier_TestCopyConcentrationsSsd() {
            var writerMock = new Mock<IDataSourceWriter>();
            var copier = new RawDataSourceBulkCopier(writerMock.Object);
            var result = copier.CopyFromDataFile(TestUtils.GetResource("Concentrations/ConcentrationsSSD.xls"));
            CollectionAssert.AreEquivalent(
                result,
                new[] {
                    SourceTableGroup.Concentrations,
                    SourceTableGroup.Compounds,
                    SourceTableGroup.FocalFoods
                });
        }
    }
}