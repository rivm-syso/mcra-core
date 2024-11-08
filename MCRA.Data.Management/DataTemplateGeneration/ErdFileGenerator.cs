using System.Text;
using MCRA.General;
using MCRA.General.TableDefinitions;

namespace MCRA.Data.Management.DataTemplateGeneration {

    /// <summary>
    /// Generator class for creating an Entity Relation Diagram (EDR) template, with datasets for tables
    /// of specific table groups.
    /// </summary>
    public class ErdFileGenerator : IDatasetTemplateGenerator {

        private readonly string _targetFile;

        /// <summary>
        /// Creates a new <see cref="ErdFileGenerator"/> instance.
        /// </summary>
        /// <param name="targetFileName"></param>
        public ErdFileGenerator(string targetFileName) {
            _targetFile = targetFileName;
        }

        /// <summary>
        /// Method to generate the template for the specified data source.
        /// </summary>
        /// <param name="sourceTableGroup">Source table group of the tables to create</param>
        /// <param name="dataFormatId">Optional data format id for a subset of the table group</param>
        public void Create(SourceTableGroup sourceTableGroup, string dataFormatId = null) {
            var sb = new StringBuilder();
            createTables(sourceTableGroup, dataFormatId, sb);
            File.WriteAllText(_targetFile, sb.ToString());
        }

        private void createTables(SourceTableGroup sourceTableGroup, string dataFormatId, StringBuilder sb) {
            var tableIds = McraTableDefinitions.Instance.GetTableGroupRawTables(sourceTableGroup, dataFormatId);
            foreach (var tableId in tableIds) {
                var table = McraTableDefinitions.Instance.GetTableDefinition(tableId);
                sb.AppendLine($"[{table.Id}]");
                foreach (var column in table.ColumnDefinitions) {
                    var primaryKey = column.IsPrimaryKey ? "*" : string.Empty;
                    var foreignKey = (column.ForeignKeyTables?.Count > 0) ? "*" : string.Empty;
                    sb.AppendLine($"\t{primaryKey}{foreignKey}{column.Id} {{label:\"{column.FieldType}\"}}");
                }
                foreach (var column in table.ColumnDefinitions) {
                    if (column.ForeignKeyTables != null) {
                        foreach (var fk in column.ForeignKeyTables) {
                            var foreignTableId = (RawDataSourceTableID)Enum.Parse(typeof(RawDataSourceTableID), fk);
                            if (tableIds.Contains(foreignTableId)) {
                                sb.AppendLine($"{table.Id} *--1 {foreignTableId} {{label: \"{column.Id}\"}}");
                            } else {
                                sb.AppendLine($"#{table.Id} *--1 {foreignTableId} {{label: \"{column.Id}\"}}");
                            }
                        }
                    }
                }
                sb.AppendLine();
            }
        }
    }
}
