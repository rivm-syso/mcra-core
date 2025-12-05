using MCRA.Utils.DataFileReading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace MCRA.Utils.Test.UnitTests.DataSourceReading.TableDefinitions {

    [TestClass]
    public class TableDefinitionTests {

        /// <summary>
        /// Creates a fake table definition with a column for each known field type.
        /// Checks whether the create data table method returns a data table with the
        /// same number of columns as there are field types.
        /// </summary>
        [TestMethod]
        public void TableDefinition_TestCreateDataTable() {
            var fieldTypes = Enum.GetValues(typeof(FieldType)).Cast<FieldType>().ToList();
            var fake = new TableDefinition() {
                ColumnDefinitions = fieldTypes
                    .Select(r => new ColumnDefinition() {
                        Id = $"{r}Field",
                        FieldType = r.ToString()
                    })
                    .ToList()
            };

            var dataTable = fake.CreateDataTable();
            Assert.IsNotNull(dataTable);
            Assert.HasCount(fieldTypes.Count, dataTable.Columns);
        }
    }
}
