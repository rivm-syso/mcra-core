using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all kinetic absorption factors.
        /// </summary>
        /// <returns></returns>
        public IList<KineticAbsorptionFactor> GetAllKineticAbsorptionFactors() {
            if (_data.AllKineticAbsorptionFactors == null) {
                var allKineticAbsorptionFactors = new List<KineticAbsorptionFactor>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.KineticModels);
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawKineticAbsorptionFactors>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawKineticAbsorptionFactors.IdCompound, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var kaf = new KineticAbsorptionFactor {
                                            Compound = _data.GetOrAddSubstance(idSubstance),
                                            RouteTypeString = r.GetString(RawKineticAbsorptionFactors.Route, fieldMap),
                                            AbsorptionFactor = r.GetDouble(RawKineticAbsorptionFactors.AbsorptionFactor, fieldMap),
                                        };
                                        allKineticAbsorptionFactors.Add(kaf);
                                    }
                                }
                            }
                        }
                    }
                }

                _data.AllKineticAbsorptionFactors = allKineticAbsorptionFactors;
            }
            return _data.AllKineticAbsorptionFactors;
        }

        /// <summary>
        /// Gets all kinetic models.
        /// </summary>
        /// <returns></returns>
        public IList<KineticModelInstance> GetAllKineticModels() {
            if (_data.AllKineticModelInstances == null) {
                LoadScope(SourceTableGroup.KineticModels);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.KineticModels);
                var kineticModelDictionary = new Dictionary<string, List<KineticModelInstance>>(StringComparer.OrdinalIgnoreCase);
                if (rawDataSourceIds?.Any() ?? false) {

                    GetAllCompounds();

                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read kinetic model instances
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawKineticModelInstances>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idModelDefinition = r.GetString(RawKineticModelInstances.IdModelDefinition, fieldMap);
                                    var idSubstance = r.GetStringOrNull(RawKineticModelInstances.IdSubstance, fieldMap);
                                    var valid = MCRAKineticModelDefinitions.TryGetDefinitionByAlias(idModelDefinition, out var modelDefinition)
                                        & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var idModelInstance = r.GetString(RawKineticModelInstances.IdModelInstance, fieldMap);
                                        if (!kineticModelDictionary.TryGetValue(idModelInstance, out List<KineticModelInstance> kmList)) {
                                            kmList = new List<KineticModelInstance>();
                                            kineticModelDictionary.Add(idModelInstance, kmList);
                                        }
                                        var km = new KineticModelInstance {
                                            IdModelInstance = idModelInstance,
                                            IdModelDefinition = modelDefinition.Id,
                                            IdTestSystem = r.GetString(RawKineticModelInstances.IdTestSystem, fieldMap),
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            Reference = r.GetStringOrNull(RawKineticModelInstances.Reference, fieldMap),
                                            Name = r.GetStringOrNull(RawKineticModelInstances.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawKineticModelInstances.Description, fieldMap),
                                            KineticModelDefinition = modelDefinition,
                                        };
                                        km.KineticModelInstanceParameters = new Dictionary<string, KineticModelInstanceParameter>(StringComparer.OrdinalIgnoreCase);
                                        kmList.Add(km);
                                    }
                                };
                            }
                        }

                        // Read kinetic model parameters
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawKineticModelInstanceParameters>(rawDataSourceId, out int[] fieldMap)) {
                                var allKineticModelParameters = new List<KineticModelInstanceParameter>();
                                while (r?.Read() ?? false) {
                                    var variability = r.GetDoubleOrNull(RawKineticModelInstanceParameters.CvVariability, fieldMap);
                                    var uncertainty = r.GetDoubleOrNull(RawKineticModelInstanceParameters.CvUncertainty, fieldMap);
                                    var idModelInstance = r.GetString(RawKineticModelInstanceParameters.IdModelInstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.KineticModelInstances, false, idModelInstance);
                                    if (valid) {
                                        var kmp = new KineticModelInstanceParameter {
                                            IdModelInstance = idModelInstance,
                                            Parameter = r.GetString(RawKineticModelInstanceParameters.Parameter, fieldMap),
                                            Description = r.GetStringOrNull(RawKineticModelInstanceParameters.Description, fieldMap),
                                            Value = r.GetDouble(RawKineticModelInstanceParameters.Value, fieldMap),
                                            DistributionTypeString = r.GetStringOrNull(RawKineticModelInstanceParameters.DistributionType, fieldMap),
                                            CvVariability = variability ?? 0,
                                            CvUncertainty = uncertainty ?? 0,
                                        };
                                        allKineticModelParameters.Add(kmp);
                                        foreach (var instance in kineticModelDictionary[idModelInstance]) {
                                            instance.KineticModelInstanceParameters[kmp.Parameter] = kmp;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllKineticModelInstances = kineticModelDictionary.SelectMany(m => m.Value).ToList();
            }
            return _data.AllKineticModelInstances;
        }

        private static void writeKineticAbsorptionFactorDataToCsv(string tempFolder, IEnumerable<KineticAbsorptionFactor> factors) {
            if (!factors?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.KineticAbsorptionFactors);
            var dt = td.CreateDataTable();

            foreach (var factor in factors) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawKineticAbsorptionFactors.IdCompound, factor.Compound?.Code);
                row.WriteNonEmptyString(RawKineticAbsorptionFactors.Route, factor.RouteTypeString);
                row.WriteNonNaNDouble(RawKineticAbsorptionFactors.AbsorptionFactor, factor.AbsorptionFactor);

                dt.Rows.Add(row);
            }

            writeToCsv(tempFolder, td, dt);
        }

        private static void writeKineticModelDataToCsv(string tempFolder, IEnumerable<KineticModelInstance> instances) {
            if (!instances?.Any() ?? true) {
                return;
            }

            var tdi = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.KineticModelInstances);
            var dti = tdi.CreateDataTable();
            var tdp = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.KineticModelInstanceParameters);
            var dtp = tdp.CreateDataTable();

            foreach (var instance in instances) {
                var row = dti.NewRow();
                row.WriteNonEmptyString(RawKineticModelInstances.IdTestSystem, instance.IdTestSystem);
                row.WriteNonEmptyString(RawKineticModelInstances.IdSubstance, instance.Substance?.Code);
                row.WriteNonEmptyString(RawKineticModelInstances.IdModelInstance, instance.IdModelInstance);
                row.WriteNonEmptyString(RawKineticModelInstances.IdModelDefinition, instance.IdModelDefinition);
                row.WriteNonEmptyString(RawKineticModelInstances.Reference, instance.Reference);
                row.WriteNonEmptyString(RawKineticModelInstances.IdTestSystem, instance.IdTestSystem);

                dti.Rows.Add(row);
                if (instance.KineticModelInstanceParameters != null) {
                    foreach (var param in instance.KineticModelInstanceParameters.Values) {
                        var rowp = dtp.NewRow();
                        rowp.WriteNonEmptyString(RawKineticModelInstanceParameters.IdModelInstance, param.IdModelInstance);
                        rowp.WriteNonEmptyString(RawKineticModelInstanceParameters.Parameter, param.Parameter);
                        rowp.WriteNonEmptyString(RawKineticModelInstanceParameters.Description, param.Description);
                        rowp.WriteNonNaNDouble(RawKineticModelInstanceParameters.Value, param.Value);
                        rowp.WriteNonEmptyString(RawKineticModelInstanceParameters.DistributionType, param.DistributionTypeString);
                        rowp.WriteNonNullDouble(RawKineticModelInstanceParameters.CvVariability, param.CvVariability);
                        rowp.WriteNonNullDouble(RawKineticModelInstanceParameters.CvUncertainty, param.CvUncertainty);
                        dtp.Rows.Add(rowp);
                    }
                }
            }
            writeToCsv(tempFolder, tdi, dti);
            writeToCsv(tempFolder, tdp, dtp);
        }
    }
}
