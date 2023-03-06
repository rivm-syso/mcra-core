using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Returns all molecular docking models.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, MolecularDockingModel> GetAllMolecularDockingModels() {
            if (_data.AllMolecularDockingModels == null) {
                LoadScope(SourceTableGroup.MolecularDockingModels);
                var allMolecularDockingModels = new Dictionary<string, MolecularDockingModel>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.MolecularDockingModels);
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllEffects();
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read molecular docking models
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawMolecularDockingModels>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idModel = r.GetString(RawMolecularDockingModels.Id, fieldMap);
                                        var idEffect = r.GetString(RawMolecularDockingModels.IdEffect, fieldMap);
                                        var valid = IsCodeSelected(ScopingType.MolecularDockingModels, idModel)
                                                  & CheckLinkSelected(ScopingType.Effects, idEffect);
                                        if (valid) {
                                            var molecularDockingModel = new MolecularDockingModel() {
                                                Code = idModel,
                                                Name = r.GetStringOrNull(RawMolecularDockingModels.Name, fieldMap),
                                                Description = r.GetStringOrNull(RawMolecularDockingModels.Description, fieldMap),
                                                Effect = _data.GetOrAddEffect(idEffect),
                                                Threshold = r.GetDoubleOrNull(RawMolecularDockingModels.Threshold, fieldMap),
                                                NumberOfReceptors = r.GetIntOrNull(RawMolecularDockingModels.NumberOfReceptors, fieldMap),
                                                Reference = r.GetStringOrNull(RawMolecularDockingModels.Reference, fieldMap),
                                                BindingEnergies = new Dictionary<Compound, double>(),
                                            };
                                            allMolecularDockingModels[idModel] = molecularDockingModel;
                                        }
                                    }
                                }
                            }
                        }

                        // Read binding energies
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawMolecularBindingEnergies>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idSubstance = r.GetString(RawMolecularBindingEnergies.IdCompound, fieldMap);
                                        var idModel = r.GetString(RawMolecularBindingEnergies.IdMolecularDockingModel, fieldMap);
                                        var valid = CheckLinkSelected(ScopingType.MolecularDockingModels, idModel)
                                                  & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                        if (valid) {
                                            var model = allMolecularDockingModels[idModel];
                                            var substance = _data.GetOrAddSubstance(idSubstance);
                                            if (model.BindingEnergies.ContainsKey(substance)) {
                                                throw new Exception($"Duplicate compound code {substance} in table {RawDataSourceTableID.MolecularBindingEnergies} for docking model {idModel}.");
                                            }
                                            var bindingEnergy = r.GetDoubleOrNull(RawMolecularBindingEnergies.BindingEnergy, fieldMap);
                                            if (bindingEnergy.HasValue) {
                                                model.BindingEnergies.Add(substance, bindingEnergy.Value);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllMolecularDockingModels = allMolecularDockingModels;
            }
            return _data.AllMolecularDockingModels;
        }

        private static void writeMolecularDockingModelDataToCsv(string tempFolder, IEnumerable<MolecularDockingModel> models) {
            if (!models?.Any() ?? true) {
                return;
            }

            var tdm = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.MolecularDockingModels);
            var dtm = tdm.CreateDataTable();
            var tdb = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.MolecularBindingEnergies);
            var dtb = tdb.CreateDataTable();

            foreach (var instance in models) {
                var row = dtm.NewRow();
                row.WriteNonEmptyString(RawMolecularDockingModels.Id, instance.Code);
                row.WriteNonEmptyString(RawMolecularDockingModels.IdEffect, instance.Effect?.Code);
                row.WriteNonEmptyString(RawMolecularDockingModels.Name, instance.Name);
                row.WriteNonEmptyString(RawMolecularDockingModels.Description, instance.Description);
                row.WriteNonEmptyString(RawMolecularDockingModels.Reference, instance.Reference);
                row.WriteNonNullDouble(RawMolecularDockingModels.Threshold, instance.Threshold);
                row.WriteNonNullInt32(RawMolecularDockingModels.NumberOfReceptors, instance.NumberOfReceptors);

                dtm.Rows.Add(row);
                if (instance.BindingEnergies != null) {
                    foreach (var param in instance.BindingEnergies) {
                        var rowb = dtb.NewRow();
                        rowb.WriteNonEmptyString(RawMolecularBindingEnergies.IdMolecularDockingModel, instance.Code);
                        rowb.WriteNonEmptyString(RawMolecularBindingEnergies.IdCompound, param.Key.Code);
                        rowb.WriteNonNaNDouble(RawMolecularBindingEnergies.BindingEnergy, param.Value);
                        dtb.Rows.Add(rowb);
                    }
                }
            }
            writeToCsv(tempFolder, tdm, dtm);
            writeToCsv(tempFolder, tdb, dtb);
        }
    }
}
