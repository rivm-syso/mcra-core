using MCRA.Utils.DataFileReading;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Data.Raw.Objects.RawTableObjects;
using MCRA.General;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class MolecularDockingModelsBulkCopier : RawDataSourceBulkCopierBase {

        public MolecularDockingModelsBulkCopier(
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
            progressState.Update($"Processing { SourceTableGroup.MolecularDockingModels.GetDisplayName() }");

            var hasModels = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.MolecularDockingModels);
            if (hasModels) {
                var tableDefinition = _tableDefinitions[RawDataSourceTableID.MolecularBindingEnergies];
                var sourceTableReader = dataSourceReader.GetDataReaderByDefinition(tableDefinition, out string sourceTableName);
                var hasData = false;
                if (sourceTableReader != null) {
                    var columnNames = sourceTableReader.GetColumnNames();
                    var idModelColumnDefinition = tableDefinition.FindColumnDefinitionByAlias(RawMolecularBindingEnergies.IdMolecularDockingModel.ToString());
                    var idModelColumnDefinitionHeaderIndex = idModelColumnDefinition.GetMatchingHeaderIndex(columnNames, false);
                    if (idModelColumnDefinitionHeaderIndex > -1) {
                        hasData = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.MolecularBindingEnergies);
                    } else {
                        var models = readMolecularDockingModels(dataSourceReader);
                        hasData = tryCopyMolecularDockingModelBindingEnergiesTabulated(dataSourceReader, models);
                    }
                } else {
                    var message = "Cannot find molecular docking binding energies.";
                    throw new RawDataSourceBulkCopyException(message);
                }
                if (hasData) {
                    registerTableGroup(SourceTableGroup.MolecularDockingModels);
                }
            }

            progressState.Update(100);
        }

        private List<RawMolecularDockingModel> readMolecularDockingModels(IDataSourceReader dataSourceReader) {
            string sourceTableName = null;
            try {
                var tableDefinition = _tableDefinitions[RawDataSourceTableID.MolecularDockingModels];
                var sourceTableReader = dataSourceReader.GetDataReaderByDefinition(tableDefinition, out sourceTableName);
                if (sourceTableReader == null) {
                    return null;
                }
                dataSourceReader.ValidateSourceTableColumns(tableDefinition, sourceTableReader);
                var records = dataSourceReader.ReadDataTable<RawMolecularDockingModel>(tableDefinition);
                return records;
            } catch (Exception ex) {
                var defaultMessage = $"An error occurred in table '{sourceTableName}': {ex.Message}";
                throw new RawDataSourceBulkCopyException(defaultMessage, sourceTableName);
            }
        }

        private bool tryCopyMolecularDockingModelBindingEnergiesTabulated(IDataSourceReader dataSourceReader, ICollection<RawMolecularDockingModel> models) {
            var tableDefinition = _tableDefinitions[RawDataSourceTableID.MolecularBindingEnergies];
            var substanceColumnDef = tableDefinition.FindColumnDefinitionByAlias(RawMolecularBindingEnergies.IdCompound.ToString());
            string sourceTableName = null;
            try {
                var scores = new List<RawMolecularBindingEnergy>();
                foreach (var model in models) {
                    var idModel = model.id;
                    var sourceTableReader = dataSourceReader.GetDataReaderByDefinition(tableDefinition, out sourceTableName);

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
                            var record = new RawMolecularBindingEnergy() {
                                idMolecularDockingModel = idModel,
                                idCompound = idSubstance,
                                BindingEnergy = sourceTableReader.GetDouble(modelScoresColumnIndex)
                            };
                            scores.Add(record);
                        }
                    }
                }
                var scoresTable = scores.ToDataTable();
                tryCopyDataTable(scoresTable, RawDataSourceTableID.MolecularBindingEnergies);
            } catch (Exception ex) {
                var msg = $"An error occurred in table '{sourceTableName}': {ex.Message}";
                throw new RawDataSourceBulkCopyException(msg, sourceTableName);
            }
            return true;
        }
    }
}
