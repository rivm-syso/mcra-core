using MCRA.Data.Raw.Copying;
using MCRA.General;
using MCRA.Utils.DataFileReading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data;

namespace MCRA.Data.Raw.Test.UnitTests.Copying.BulkCopiers {

    /// <summary>
    /// AdverseOutcomePathwayNetworksBulkCopierTests
    /// </summary>
    [TestClass]
    public class AdverseOutcomePathwayNetworksBulkCopierTests : BulkCopierTestsBase {

        /// <summary>
        /// AdverseOutcomePathwayNetworksBulkCopier_TryCopyWithoutEffectRelationsTest
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(RawDataSourceBulkCopyException), AllowDerivedTypes = false)]
        public void AdverseOutcomePathwayNetworksBulkCopier_TryCopyWithoutEffectRelationsTest() {
            var writerMock = new Mock<IDataSourceWriter>();
            //set a callback on the GetDataReaderByDefinition to retrieve the source table groups
            var copier = new RawDataSourceBulkCopier(writerMock.Object);

            string name = null;
            var tableId = "";
            var table = new DataTable();
            //set a callback on the GetDataReaderByDefinition to retrieve the source table groups
            GetDataReaderByDefinitionDelegate gdrByDefCallback = delegate (TableDefinition t, out string s) {
                //Set the source table name to the table definition id
                s = null;
                tableId = t.Id;
            };
            var readerMock = new Mock<IDataSourceReader>();
            readerMock.Setup(m => m.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name))
                      .Callback(gdrByDefCallback)
                      .Returns(() => tableId == "AdverseOutcomePathwayNetworks" ? table.CreateDataReader() : null);

            copier.CopyFromDataSourceReader(
                readerMock.Object,
                tableGroups: new[] { SourceTableGroup.AdverseOutcomePathwayNetworks },
                allowEmptyDataSource: true
            );
        }

        /// <summary>
        /// AdverseOutcomePathwayNetworksBulkCopier_TryCopyTest
        /// </summary>
        [TestMethod]
        public void AdverseOutcomePathwayNetworksBulkCopier_TryCopyTest() {
            var writerMock = new Mock<IDataSourceWriter>();

            var copier = new RawDataSourceBulkCopier(writerMock.Object);
            //create empty table objects
            var tbAopNet = new DataTable();

            string name = null;
            var tableId = "";
            //set a callback on the GetDataReaderByDefinition to retrieve the source table groups
            GetDataReaderByDefinitionDelegate gdrByDefCallback = delegate (TableDefinition t, out string s) {
                //Set the source table name to the table definition id
                s = null;
                tableId = t.Id;
            };

            var readerMock = new Mock<IDataSourceReader>();
            readerMock.Setup(m => m.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name))
                      .Callback(gdrByDefCallback)
                      .Returns(() => tableId == "AdverseOutcomePathwayNetworks" ||
                                     tableId == "EffectRelations" ? tbAopNet.CreateDataReader() : null);

            var result = copier.CopyFromDataSourceReader(
                readerMock.Object,
                tableGroups: new[] { SourceTableGroup.AdverseOutcomePathwayNetworks },
                allowEmptyDataSource: true
            );

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(SourceTableGroup.AdverseOutcomePathwayNetworks, result.Single());

            readerMock.Verify(x => x.Open(), Times.Once);
            readerMock.Verify(x => x.Close(), Times.Once);
            readerMock.Verify(x => x.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name), Times.Exactly(2));
        }
    }
}