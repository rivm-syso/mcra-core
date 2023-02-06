using System;
using System.Collections.Generic;
using System.Data;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using MCRA.General.TableDefinitions;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class CompoundsBulkCopier : RawDataSourceBulkCopierBase {

        public CompoundsBulkCopier(
            IDataSourceWriter dataSourceWriter,
            HashSet<SourceTableGroup> parsedTableGroups,
            HashSet<RawDataSourceTableID> parsedDataTables)
            : base(
                  dataSourceWriter,
                  parsedTableGroups,
                  parsedDataTables
        ) {
        }

        public override void TryCopy(IDataSourceReader dataSourceReader, ProgressState progressState) {
            progressState.Update("Processing Compounds");
            // If no compounds are found in this data source and there is an SSD data table, get the 
            // compounds from the SSD table
            if (tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.Compounds)
                || tryDoSsdConcentrationsCompoundsBulkCopy(dataSourceReader)) {
                registerTableGroup(SourceTableGroup.Compounds);
            }
            progressState.Update(100);
        }

        private bool tryDoSsdConcentrationsCompoundsBulkCopy(IDataSourceReader dataSourceReader) {
            var ssdTableDefinition = _tableDefinitions[RawDataSourceTableID.ConcentrationsSSD];
            using var ssdTableReader = dataSourceReader.GetDataReaderByDefinition(ssdTableDefinition, out var sourceTableName);
            if (ssdTableReader == null) {
                return false;
            }

            // Parse table headers
            var fields = ssdTableReader.FieldCount;
            var columnNames = ssdTableReader.GetColumnNames();
            var ixCompoundCode = columnNames.FindIndex(c => string.Equals(c, "paramCode", StringComparison.InvariantCultureIgnoreCase));
            var ixCompoundName = columnNames.FindIndex(c => string.Equals(c, "paramCode", StringComparison.InvariantCultureIgnoreCase));
            ixCompoundName = ixCompoundName >= 0 ? ixCompoundName : ixCompoundCode;

            // Prepare data tables
            var tableDef = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.Compounds);
            var rawCompoundsTable = tableDef.CreateDataTable();

            // Read SSD records
            var compoundsHash = new HashSet<string>();
            while (ssdTableReader.Read()) {
                var code = ssdTableReader.IsDBNull(ixCompoundCode) ? null : ssdTableReader.GetValue(ixCompoundCode).ToString();
                if (!string.IsNullOrEmpty(code) && !compoundsHash.Contains(code)) {
                    var name = ssdTableReader.IsDBNull(ixCompoundName) ? null : ssdTableReader.GetValue(ixCompoundName).ToString();
                    var dr = rawCompoundsTable.NewRow();
                    dr["idCompound"] = code;
                    dr["Name"] = name;
                    rawCompoundsTable.Rows.Add(dr);
                    compoundsHash.Add(code);
                }
            }

            // Bulkcopy raw compounds
            tryCopyDataTable(rawCompoundsTable, RawDataSourceTableID.Compounds);

            return true;
        }
    }
}
