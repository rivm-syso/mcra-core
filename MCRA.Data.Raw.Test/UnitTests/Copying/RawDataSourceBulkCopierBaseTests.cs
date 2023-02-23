using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.Data.Raw.Copying;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data;
using static MCRA.Data.Raw.Test.UnitTests.Copying.RawDataSourceBulkCopierTests;

namespace MCRA.Data.Raw.Test.UnitTests.Copying {

    /// <summary>
    /// Tests the RawDataSourceBulkCopierBase class by inheriting from it and
    /// testing the protected methods
    /// </summary>
    [TestClass()]
    public class RawDataSourceBulkCopierBaseTests : RawDataSourceBulkCopierBase {
        private Mock<IDataSourceWriter> _writerMock;
        private Mock<IRawDataSourceVersion> _rawDataSourceVersionMock;

        public RawDataSourceBulkCopierBaseTests() : base(null, null, null) { }

        /// <summary>
        /// TestInitialize
        /// </summary>
        [TestInitialize]
        public void TestInitialize() {
            _writerMock = new Mock<IDataSourceWriter>();
            _writerMock.SetupAllProperties();
            _dataSourceWriter = _writerMock.Object;
            _rawDataSourceVersionMock = new Mock<IRawDataSourceVersion>();
            _rawDataSourceVersionMock.SetupAllProperties();
            _rawDataSourceVersionMock.Setup(m => m.id).Returns(123);
        }

        /// <summary>
        /// RawDataSourceBulkCopierBase_SimpleBulkCopyTest
        /// </summary>
        [TestMethod]
        public void RawDataSourceBulkCopierBase_SimpleBulkCopyTest() {
            var readerMock = new Mock<IDataSourceReader>();

            string name = null;
            var getReaderTableId = "";
            var validatedTableId = "";
            var writtenTableId = "";
            var writtenTableName = "";
            var table = new DataTable();
            //set a callback on the GetDataReaderByDefinition to retrieve table id of tabledefinition
            GetDataReaderByDefinitionDelegate gdrByDefCallback =
                delegate (TableDefinition t, out string s) {
                    //Set the source table name to the table definition id
                    s = null;
                    getReaderTableId = t.Id;
                };
            //setup the method to always return a reader
            readerMock.Setup(m => m.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name))
                      .Callback(gdrByDefCallback)
                      .Returns(() => table.CreateDataReader());
            //set a callback on the Validation
            readerMock.Setup(m => m.ValidateSourceTableColumns(It.IsAny<TableDefinition>(), It.IsAny<IDataReader>()))
                      .Callback((TableDefinition tdef, IDataReader r) => {
                          validatedTableId = tdef.Id;
                      });
            _writerMock.Setup(m => m.Write(It.IsAny<IDataReader>(), It.IsAny<TableDefinition>(), It.IsAny<string>(), It.IsAny<ProgressState>()))
                       .Callback((IDataReader r, TableDefinition tdef, string tableName, ProgressState p) => {
                           writtenTableId = tdef.Id;
                           writtenTableName = tdef.TargetDataTable;
                       });

            //list of Id's which don't have a corresponding Table Definition
            var nonTableIds = new HashSet<RawDataSourceTableID> {
                RawDataSourceTableID.Unknown, //Undefined table
                RawDataSourceTableID.KineticModelDefinitions, //This is a static table of values, defined in code
            };
            var parsedIds = new List<RawDataSourceTableID>();
            //start a write action for every raw data source table id
            foreach (RawDataSourceTableID id in Enum.GetValues(typeof(RawDataSourceTableID))) {
                if (nonTableIds.Contains(id)) {
                    continue;
                }

                Assert.IsTrue(tryDoSimpleBulkCopy(readerMock.Object, id));
                Assert.AreEqual(id.ToString(), getReaderTableId, true);
                Assert.AreEqual(id.ToString(), validatedTableId, true);
                Assert.AreEqual(id.ToString(), writtenTableId, true);
                parsedIds.Add(id);
            }
            CollectionAssert.AreEquivalent(parsedIds, _parsedDataTables.ToList());
        }

        /// <summary>
        /// RawDataSourceBulkCopierBase_CopyWithDynamicPropertiesTest
        /// </summary>
        [TestMethod]
        public void RawDataSourceBulkCopierBase_CopyWithDynamicPropertiesTest() {
            var readerMock = new Mock<IDataSourceReader>();
            //get a table definition for a table
            var tableIdTest = RawDataSourceTableID.Individuals;
            var tableDefTest = _tableDefinitions[tableIdTest];
            //create the table for this definition
            var tableTest = tableDefTest.CreateDataTable();
            //add some 'dynamic' columns
            tableTest.Columns.Add(new DataColumn("TestCStr0001", typeof(string)));
            tableTest.Columns.Add(new DataColumn("TestCDbl0001", typeof(double)));
            tableTest.Columns.Add(new DataColumn("TestCDbl0002", typeof(double)));
            tableTest.Columns.Add(new DataColumn("TestCInteger", typeof(int)));
            tableTest.Columns.Add(new DataColumn("TestCDblStr1", typeof(string)));
            tableTest.Columns.Add(new DataColumn("TestCDblStr2", typeof(string)));

            var testRow = tableTest.NewRow();
            testRow["TestCStr0001"] = "testvaluestr001";
            testRow["TestCDbl0001"] = 123.456;
            testRow["TestCDbl0002"] = "NaN";
            testRow["TestCInteger"] = -111;
            testRow["TestCDblStr1"] = "12,34"; //test the conversion of decimal comma
            testRow["TestCDblStr2"] = "12.34"; //and decimal point
            tableTest.Rows.Add(testRow);

            string name = null;
            var getReaderTableId = "";
            var validatedTableId = "";
            var writtenTableId = "";
            var writtenTableName = "";

            //set a callback on the GetDataReaderByDefinition to retrieve table id of tabledefinition
            GetDataReaderByDefinitionDelegate gdrByDefCallback = delegate (TableDefinition t, out string s) {
                //Set the source table name to the table definition id
                s = null;
                getReaderTableId = t.Id;
            };
            //setup the method to always return a reader
            readerMock.Setup(m => m.GetDataReaderByDefinition(It.IsAny<TableDefinition>(), out name))
                      .Callback(gdrByDefCallback)
                      .Returns(() => tableTest.CreateDataReader());
            //set a callback on the Validation
            readerMock.Setup(m => m.ValidateSourceTableColumns(It.IsAny<TableDefinition>(), It.IsAny<IDataReader>()))
                      .Callback((TableDefinition tdef, IDataReader r) => {
                          validatedTableId = tdef.Id;
                      });

            //setup writer callback
            _writerMock.Setup(m => m.Write(It.IsAny<IDataReader>(), It.IsAny<TableDefinition>(), It.IsAny<string>(), It.IsAny<ProgressState>()))
                       .Callback((IDataReader r, TableDefinition tdef, string tableName, ProgressState p) => {
                           writtenTableId = tdef.Id;
                           writtenTableName = tdef.TargetDataTable;
                       });
            var propertyTables = new Dictionary<string, DataTable>();
            _writerMock.Setup(m => m.Write(It.IsAny<DataTable>(), It.IsAny<string>(), It.IsAny<TableDefinition>()))
                       .Callback((DataTable dt, string tname, TableDefinition c) => {
                           propertyTables.Add(tname, dt);
                       });

            //start a write action for every raw data source table id
            Assert.IsTrue(tryDoBulkCopyWithDynamicProperties(
                readerMock.Object,
                RawDataSourceTableID.Individuals,
                RawDataSourceTableID.IndividualProperties,
                RawDataSourceTableID.IndividualPropertyValues)
            );

            Assert.AreEqual(tableIdTest.ToString(), getReaderTableId, true);
            Assert.AreEqual(tableIdTest.ToString(), validatedTableId, true);
            Assert.AreEqual(tableIdTest.ToString(), writtenTableId, true);
            Assert.AreEqual(tableDefTest.TargetDataTable, writtenTableName);

            Assert.AreEqual(2, propertyTables.Count);
            var propTable = propertyTables["RawIndividualProperties"];
            var valTable = propertyTables["RawIndividualPropertyValues"];
            Assert.AreEqual(6, propTable.Rows.Count);
            Assert.AreEqual(6, valTable.Rows.Count);

            Assert.AreEqual("TestCStr0001", propTable.Rows[0]["idIndividualProperty"].ToString());
            Assert.AreEqual("TestCDbl0001", propTable.Rows[1]["idIndividualProperty"].ToString());
            Assert.AreEqual("TestCDbl0002", propTable.Rows[2]["idIndividualProperty"].ToString());
            Assert.AreEqual("TestCInteger", propTable.Rows[3]["idIndividualProperty"].ToString());
            Assert.AreEqual("TestCDblStr1", propTable.Rows[4]["idIndividualProperty"].ToString());
            Assert.AreEqual("TestCDblStr2", propTable.Rows[5]["idIndividualProperty"].ToString());

            Assert.AreEqual("TestCStr0001", valTable.Rows[0]["PropertyName"].ToString());
            Assert.AreEqual("TestCDbl0001", valTable.Rows[1]["PropertyName"].ToString());
            Assert.AreEqual("TestCDbl0002", valTable.Rows[2]["PropertyName"].ToString());
            Assert.AreEqual("TestCInteger", valTable.Rows[3]["PropertyName"].ToString());
            Assert.AreEqual("TestCDblStr1", valTable.Rows[4]["PropertyName"].ToString());
            Assert.AreEqual("TestCDblStr2", valTable.Rows[5]["PropertyName"].ToString());

            Assert.AreEqual("testvaluestr001", valTable.Rows[0]["TextValue"]);
            Assert.AreEqual(DBNull.Value, valTable.Rows[1]["TextValue"]);
            Assert.AreEqual(DBNull.Value, valTable.Rows[2]["TextValue"]);
            Assert.AreEqual(DBNull.Value, valTable.Rows[3]["TextValue"]);
            Assert.AreEqual(DBNull.Value, valTable.Rows[4]["TextValue"]);
            Assert.AreEqual(DBNull.Value, valTable.Rows[5]["TextValue"]);

            Assert.AreEqual(DBNull.Value, valTable.Rows[0]["DoubleValue"]);
            Assert.AreEqual(123.456, valTable.Rows[1]["DoubleValue"]);
            Assert.AreEqual(double.NaN, valTable.Rows[2]["DoubleValue"]);
            Assert.AreEqual(-111D, valTable.Rows[3]["DoubleValue"]);
            Assert.AreEqual(12.34D, valTable.Rows[4]["DoubleValue"]);
            Assert.AreEqual(12.34D, valTable.Rows[5]["DoubleValue"]);
        }

        /// <summary>
        /// RawDataSourceBulkCopierBase_CopyDataTableTest
        /// </summary>
        [TestMethod]
        public void RawDataSourceBulkCopierBase_CopyDataTableTest() {
            //setup writer callback
            DataTable writtenTable;
            var writtenTableName = "";

            _writerMock.Setup(m => m.Write(It.IsAny<DataTable>(), It.IsAny<string>(), It.IsAny<TableDefinition>()))
                       .Callback((DataTable dt, string tname, TableDefinition c) => {
                           writtenTable = dt;
                           writtenTableName = tname;
                       });

            //list of Id's which don't have a corresponding Table Definition
            var nonTableIds = new HashSet<RawDataSourceTableID> {
                RawDataSourceTableID.Unknown, //Undefined table
                RawDataSourceTableID.KineticModelDefinitions, //This is a static table of values, defined in code
                RawDataSourceTableID.DietaryExposureModels,
                RawDataSourceTableID.DietaryExposurePercentiles,
                RawDataSourceTableID.DietaryExposurePercentilesUncertain,
            };
            var parsedIds = new List<RawDataSourceTableID>();
            //start a write action for every raw data source table id
            foreach (RawDataSourceTableID id in Enum.GetValues(typeof(RawDataSourceTableID))) {
                if (nonTableIds.Contains(id)) {
                    continue;
                }
                var tableDefinition = _tableDefinitions[id];
                var dataTable = tableDefinition.CreateDataTable();
                Assert.IsTrue(tryCopyDataTable(dataTable, id));
                Assert.AreEqual(tableDefinition.TargetDataTable, writtenTableName);
                parsedIds.Add(id);
            }
            CollectionAssert.AreEquivalent(parsedIds, _parsedDataTables.ToList());
        }

        /// <summary>
        /// RawDataSourceBulkCopierBase_RegisterDataTableTest
        /// </summary>
        [TestMethod]
        public void RawDataSourceBulkCopierBase_RegisterDataTableTest() {
            var parsedIds = new List<RawDataSourceTableID>();
            //start a write action for every raw data source table id
            foreach (RawDataSourceTableID id in Enum.GetValues(typeof(RawDataSourceTableID))) {
                registerDataTable(id);
                parsedIds.Add(id);
            }
            CollectionAssert.AreEquivalent(parsedIds, _parsedDataTables.ToList());
        }

        /// <summary>
        /// Override abstract method
        /// </summary>
        /// <param name="dataSourceReader"></param>
        /// <param name="progressState"></param>
        public override void TryCopy(IDataSourceReader dataSourceReader, ProgressState progressState) {
        }
    }
}
