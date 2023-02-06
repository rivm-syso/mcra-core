using MCRA.Utils.DataFileReading;
using MCRA.Utils.DataSourceReading.DataSourceReaders;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Data.Raw.Objects.RawTableObjects;
using MCRA.General;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class QsarMembershipModelsBulkCopier : RawDataSourceBulkCopierBase {

        public QsarMembershipModelsBulkCopier(
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
            progressState.Update($"Processing { SourceTableGroup.QsarMembershipModels.GetDisplayName() }");

            var hasModels = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.QsarMembershipModels);
            if (hasModels) {
                var tableDefinition = _tableDefinitions[RawDataSourceTableID.QsarMembershipScores];
                using var sourceTableReader = dataSourceReader.GetDataReaderByDefinition(tableDefinition, out string sourceTableName);
                var hasData = false;
                if (sourceTableReader != null) {
                    var columnNames = sourceTableReader.GetColumnNames();
                    var idModelColumnDefinition = tableDefinition.FindColumnDefinitionByAlias(RawQSARMembershipScores.IdQSARMembershipModel.ToString());
                    var idModelColumnDefinitionHeaderIndex = idModelColumnDefinition.GetMatchingHeaderIndex(columnNames, false);
                    if (idModelColumnDefinitionHeaderIndex > -1) {
                        hasData = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.QsarMembershipScores);
                    } else {
                        var models = readQsarMembershipModels(dataSourceReader);
                        hasData = tryCopyQsarMembershipScoresTabulated(dataSourceReader, models);
                    }
                } else {
                    var message = "Cannot find QSAR membership scores.";
                    throw new RawDataSourceBulkCopyException(message);
                }
                if (hasData) {
                    registerTableGroup(SourceTableGroup.QsarMembershipModels);
                }
            }

            progressState.Update(100);
        }

        private List<RawQsarMembershipModel> readQsarMembershipModels(IDataSourceReader dataSourceReader) {
            string sourceTableName = null;
            try {
                var tableDefinition = _tableDefinitions[RawDataSourceTableID.QsarMembershipModels];
                using var sourceTableReader = dataSourceReader.GetDataReaderByDefinition(tableDefinition, out sourceTableName);
                if (sourceTableReader == null) {
                    return null;
                }
                dataSourceReader.ValidateSourceTableColumns(tableDefinition, sourceTableReader);
                var records = dataSourceReader.ReadDataTable<RawQsarMembershipModel>(tableDefinition);
                return records;
            } catch (Exception ex) {
                var defaultMessage = $"An error occurred in table '{sourceTableName}': {ex.Message}";
                throw new RawDataSourceBulkCopyException(defaultMessage, sourceTableName);
            }
        }

        private bool tryCopyQsarMembershipScoresTabulated(IDataSourceReader dataSourceReader, ICollection<RawQsarMembershipModel> models) {
            var tableDefinition = _tableDefinitions[RawDataSourceTableID.QsarMembershipScores];
            var substanceColumnDef = tableDefinition.FindColumnDefinitionByAlias(RawQSARMembershipScores.IdSubstance.ToString());
            string sourceTableName = null;
            try {
                var scores = new List<RawQsarMembershipScore>();
                foreach (var model in models) {
                    var idModel = model.id;
                    using var sourceTableReader = dataSourceReader.GetDataReaderByDefinition(tableDefinition, out sourceTableName);

                    if (sourceTableReader == null) {
                        return false;
                    }

                    var colnames = sourceTableReader.GetColumnNames();
                    var idSubstanceColumnIndex = substanceColumnDef.GetMatchingHeaderIndex(colnames);
                    var modelScoresColumnName = colnames.FirstOrDefault(r => r.Equals(idModel, StringComparison.OrdinalIgnoreCase));
                    if (string.IsNullOrEmpty(modelScoresColumnName)) {
                        throw new Exception($"Scores for model {idModel} not found!");
                    }
                    var modelScoresColumnIndex = sourceTableReader.GetOrdinal(modelScoresColumnName);
                    // Write row-by-row using the column mappings
                    while (sourceTableReader.Read()) {
                        if (!sourceTableReader.IsDBNull(modelScoresColumnIndex)) {
                            var idSubstance = sourceTableReader.GetString(idSubstanceColumnIndex);
                            var record = new RawQsarMembershipScore() {
                                idQSARMembershipModel = idModel,
                                idSubstance = idSubstance,
                                MembershipScore = sourceTableReader.GetDouble(modelScoresColumnIndex)
                            };
                            scores.Add(record);
                        }
                    }
                }
                var scoresTable = scores.ToDataTable();
                tryCopyDataTable(scoresTable, RawDataSourceTableID.QsarMembershipScores);
            } catch (Exception ex) {
                var msg = $"An error occurred in table '{sourceTableName}': {ex.Message}";
                throw new RawDataSourceBulkCopyException(msg, sourceTableName);
            }
            return true;
        }
    }
}
