using MCRA.Utils.DataFileReading;
using MCRA.Data.Raw.Copying;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data;

namespace MCRA.Data.Raw.Test.UnitTests.Copying.BulkCopiers {

    /// <summary>
    /// AssessmentGroupsBulkCopierTests
    /// </summary>
    [TestClass]
    public class AssessmentGroupsBulkCopierTests : BulkCopierTestsBase {

        /// <summary>
        /// AssessmentGroupsBulkCopierTest1
        /// </summary>
        [TestMethod]
        public void AssessmentGroupsBulkCopierTest1() {
            var writerMock = new Mock<IDataSourceWriter>();
            var readerMock = new Mock<IDataSourceReader>();

            var copier = new RawDataSourceBulkCopier(writerMock.Object);
            //create empty table objects
            var tbUses = new DataTable();

            string name = null;
            var tableId = "";
            //set a callback on the GetDataReaderByDefinition to retrieve the source table groups
            GetDataReaderByDefinitionDelegate gdrByDefCallback = delegate (TableDefinition t, out string s) {
                //Set the source table name to the table definition id
                s = null;
                tableId = t.Id;
            };
            //set up a reader that only returns the AssessmentGroupMembershipModels table
            readerMock.Setup(m => m.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name))
                      .Callback(gdrByDefCallback)
                      .Returns(() => tableId == "AssessmentGroupMembershipModels" ? tbUses.CreateDataReader() : null);

            var result = copier.CopyFromDataSourceReader(
                readerMock.Object, 
                tableGroups: new[] { SourceTableGroup.AssessmentGroupMemberships },
                allowEmptyDataSource: true
            );

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        /// <summary>
        /// AssessmentGroupsBulkCopierTest2
        /// </summary>
        [TestMethod]
        public void AssessmentGroupsBulkCopierTest2() {
            var writerMock = new Mock<IDataSourceWriter>();
            var readerMock = new Mock<IDataSourceReader>();

            var copier = new RawDataSourceBulkCopier(writerMock.Object);
            //create empty table objects
            var tbModels = new DataTable();
            var tbMemberships = new DataTable();

            string name = null;
            var tableId = "";
            //set a callback on the GetDataReaderByDefinition to retrieve the source table groups
            GetDataReaderByDefinitionDelegate gdrByDefCallback = delegate (TableDefinition t, out string s) {
                //Set the source table name to the table definition id
                s = null;
                tableId = t.Id;
            };
            //set up a reader that only returns the AgriculturalUses table
            readerMock.Setup(m => m.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name))
                      .Callback(gdrByDefCallback)
                      .Returns(() => tableId == "AssessmentGroupMembershipModels" ||
                                     tableId == "AssessmentGroupMemberships" ? tbModels.CreateDataReader() : null);

            var result = copier.CopyFromDataSourceReader(
                readerMock.Object,
                tableGroups: new[] { SourceTableGroup.AssessmentGroupMemberships },
                allowEmptyDataSource: true
            );

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(SourceTableGroup.AssessmentGroupMemberships, result.Single());

            readerMock.Verify(x => x.Open(), Times.Once);
            readerMock.Verify(x => x.Close(), Times.Once);
            readerMock.Verify(x => x.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name), Times.Exactly(2));
        }
    }
}
