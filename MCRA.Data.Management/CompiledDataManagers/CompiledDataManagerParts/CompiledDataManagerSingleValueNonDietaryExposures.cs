using System.Data;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {
        public IDictionary<string, ExposureScenario> GetAllSingleValuNonDietaryExposureScenarios() {
            if (_data.AllSingleValueNonDietaryExposureScenarios == null) {
                LoadScope(SourceTableGroup.SingleValueNonDietaryExposures);
                var exposureScenarios = new Dictionary<string, ExposureScenario>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.SingleValueNonDietaryExposures);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllPopulations();
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read exposure scenarios
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawExposureScenarios>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idScenario = r.GetString(RawExposureScenarios.IdExposureScenario, fieldMap);
                                    var idPopulation = r.GetString(RawExposureScenarios.IdPopulation, fieldMap);
                                    var valid = IsCodeSelected(ScopingType.ExposureScenarios, idScenario)
                                        && IsCodeSelected(ScopingType.Populations, idPopulation);
                                    if (valid) {
                                        var exposureScenario = new ExposureScenario() {
                                            Code = idScenario,
                                            Description = r.GetStringOrNull(RawExposureScenarios.Description, fieldMap),
                                            Name = r.GetStringOrNull(RawExposureScenarios.Name, fieldMap),
                                            ExposureType = r.GetEnum<ExposureType>(RawExposureScenarios.ExposureType, fieldMap),
                                            ExposureLevel = r.GetEnum<TargetLevelType>(RawExposureScenarios.ExposureLevel, fieldMap),
                                            ExposureRoutes = r.GetStringOrNull(RawExposureScenarios.ExposureRoutes, fieldMap),
                                            ExposureUnit = r.GetEnum<ExternalExposureUnit>(RawExposureScenarios.ExposureUnit, fieldMap),
                                            Population = _data.GetOrAddPopulation(idPopulation)
                                        };
                                        exposureScenarios.Add(idScenario, exposureScenario);
                                    }
                                }
                            }
                        }
                        _data.AllSingleValueNonDietaryExposureScenarios = exposureScenarios;
                    }
                }
            }
            return _data.AllSingleValueNonDietaryExposureScenarios;
        }

        public IDictionary<string, ExposureDeterminantCombination> GetAllSingleValueNonDietaryExposureDeterminantCombinations() {
            if (_data.AllSingleValueNonDietaryExposureDeterminantCombinations == null) {
                LoadScope(SourceTableGroup.SingleValueNonDietaryExposures);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.SingleValueNonDietaryExposures);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read exposure determinants
                        var exposureDeterminants = new Dictionary<string, ExposureDeterminant>(StringComparer.OrdinalIgnoreCase);
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawExposureDeterminants>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idExposureDeterminant = r.GetString(RawExposureDeterminants.IdExposureDeterminant, fieldMap);
                                    var exposureDeterminant = new ExposureDeterminant {
                                        Code = idExposureDeterminant,
                                        Name = r.GetStringOrNull(RawExposureDeterminants.Name, fieldMap),
                                        Description = r.GetStringOrNull(RawExposureDeterminants.Description, fieldMap),
                                        PropertyType = r.GetEnum<IndividualPropertyType>(RawExposureDeterminants.Type, fieldMap),
                                    };
                                    exposureDeterminants.Add(exposureDeterminant.Code, exposureDeterminant);
                                }
                            }
                        }

                        // Read exposure determinant combinations
                        var exposureDeterminantCombinations = new Dictionary<string, ExposureDeterminantCombination>(StringComparer.OrdinalIgnoreCase);
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawExposureDeterminantCombinations>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {

                                    var idExposureDeterminantCombination = r.GetString(RawExposureDeterminantCombinations.IdExposureDeterminantCombination, fieldMap);
                                    var exposureDeterminantCombination = new ExposureDeterminantCombination {
                                        Code = idExposureDeterminantCombination,
                                        Name = r.GetStringOrNull(RawExposureDeterminants.Name, fieldMap),
                                        Description = r.GetStringOrNull(RawExposureDeterminants.Description, fieldMap),
                                    };
                                    exposureDeterminantCombinations.Add(exposureDeterminantCombination.Code, exposureDeterminantCombination);
                                }
                            }
                        }

                        // Read exposure determinant values
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawExposureDeterminantValues>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idExposureDeterminant = r.GetString(RawExposureDeterminantValues.PropertyName, fieldMap);
                                    var exposureDeterminantValue = new ExposureDeterminantValue {
                                        Property = exposureDeterminants[idExposureDeterminant],
                                        TextValue = r.GetStringOrNull(RawExposureDeterminantValues.TextValue, fieldMap),
                                    };
                                    SetPropertyValue(r, fieldMap, ref exposureDeterminantValue);

                                    var idExposureDeterminantCombination = r.GetString(RawExposureDeterminantValues.IdExposureDeterminantCombination, fieldMap);
                                    if (exposureDeterminantCombinations.ContainsKey(idExposureDeterminantCombination)) {
                                        var exposureDeterminantCombination = exposureDeterminantCombinations[idExposureDeterminantCombination];
                                        exposureDeterminantCombination.Properties.Add(exposureDeterminantValue.Property.Code, exposureDeterminantValue);
                                    }
                                }
                            }
                        }
                        _data.AllSingleValueNonDietaryExposureDeterminantCombinations = exposureDeterminantCombinations;

                    }
                }
            }
            return _data.AllSingleValueNonDietaryExposureDeterminantCombinations;
        }

        public IList<ExposureEstimate> GetAllSingleValueNonDietaryExposures() {
            if (_data.AllSingleValueNonDietaryExposureEstimates == null) {
                LoadScope(SourceTableGroup.SingleValueNonDietaryExposures);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.SingleValueNonDietaryExposures);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllPopulations();
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // NOTE: possibly the two above methods can be reused here, to avoid code duplication

                        // Read exposure scenarios
                        var exposureScenarios = new Dictionary<string, ExposureScenario>(StringComparer.OrdinalIgnoreCase);
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawExposureScenarios>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idScenario = r.GetString(RawExposureScenarios.IdExposureScenario, fieldMap);
                                    var idPopulation = r.GetString(RawExposureScenarios.IdPopulation, fieldMap);
                                    var valid = IsCodeSelected(ScopingType.ExposureScenarios, idScenario)
                                        && IsCodeSelected(ScopingType.Populations, idPopulation);
                                    if (valid) {
                                        var exposureScenario = new ExposureScenario() {
                                            Code = idScenario,
                                            Description = r.GetStringOrNull(RawExposureScenarios.Description, fieldMap),
                                            Name = r.GetStringOrNull(RawExposureScenarios.Name, fieldMap),
                                            Population = _data.GetOrAddPopulation(idPopulation),
                                            ExposureType = r.GetEnum<ExposureType>(RawExposureScenarios.ExposureType, fieldMap),
                                            ExposureLevel = r.GetEnum<TargetLevelType>(RawExposureScenarios.ExposureLevel, fieldMap),
                                            ExposureRoutes = r.GetStringOrNull(RawExposureScenarios.ExposureRoutes, fieldMap),
                                            ExposureUnit = r.GetEnum<ExternalExposureUnit>(RawExposureScenarios.ExposureUnit, fieldMap),
                                        };
                                        exposureScenarios.Add(idScenario, exposureScenario);
                                    }
                                }
                            }
                        }
                        _data.AllSingleValueNonDietaryExposureScenarios = exposureScenarios;

                        // Read exposure determinants
                        var exposureDeterminants = new Dictionary<string, ExposureDeterminant>(StringComparer.OrdinalIgnoreCase);
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawExposureDeterminants>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idExposureDeterminant = r.GetString(RawExposureDeterminants.IdExposureDeterminant, fieldMap);
                                    var exposureDeterminant = new ExposureDeterminant {
                                        Code = idExposureDeterminant,
                                        Name = r.GetStringOrNull(RawExposureDeterminants.Name, fieldMap),
                                        Description = r.GetStringOrNull(RawExposureDeterminants.Description, fieldMap),
                                        PropertyType = r.GetEnum<IndividualPropertyType>(RawExposureDeterminants.Type, fieldMap),
                                    };
                                    exposureDeterminants.Add(exposureDeterminant.Code, exposureDeterminant);
                                }
                            }
                        }

                        // Read exposure determinant combinations
                        var exposureDeterminantCombinations = new Dictionary<string, ExposureDeterminantCombination>(StringComparer.OrdinalIgnoreCase);
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawExposureDeterminantCombinations>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {

                                    var idExposureDeterminantCombination = r.GetString(RawExposureDeterminantCombinations.IdExposureDeterminantCombination, fieldMap);
                                    var exposureDeterminantCombination = new ExposureDeterminantCombination {
                                        Code = idExposureDeterminantCombination,
                                        Name = r.GetStringOrNull(RawExposureDeterminants.Name, fieldMap),
                                        Description = r.GetStringOrNull(RawExposureDeterminants.Description, fieldMap),
                                    };
                                    exposureDeterminantCombinations.Add(exposureDeterminantCombination.Code, exposureDeterminantCombination);
                                }
                            }
                        }
                        _data.AllSingleValueNonDietaryExposureDeterminantCombinations = exposureDeterminantCombinations;

                        // Read exposure determinant values
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawExposureDeterminantValues>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idExposureDeterminant = r.GetString(RawExposureDeterminantValues.PropertyName, fieldMap);
                                    var exposureDeterminantValue = new ExposureDeterminantValue {
                                        Property = exposureDeterminants[idExposureDeterminant],
                                        TextValue = r.GetStringOrNull(RawExposureDeterminantValues.TextValue, fieldMap),
                                    };
                                    SetPropertyValue(r, fieldMap, ref exposureDeterminantValue);

                                    var idExposureDeterminantCombination = r.GetString(RawExposureDeterminantValues.IdExposureDeterminantCombination, fieldMap);
                                    if (exposureDeterminantCombinations.ContainsKey(idExposureDeterminantCombination)) {
                                        var exposureDeterminantCombination = exposureDeterminantCombinations[idExposureDeterminantCombination];
                                        exposureDeterminantCombination.Properties.Add(exposureDeterminantValue.Property.Code, exposureDeterminantValue);
                                    }
                                }
                            }
                        }

                        // Read exposure estimates
                        var exposureEstimates = new List<ExposureEstimate>();
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawExposureEstimates>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idExposureScenario = r.GetString(RawExposureEstimates.IdExposureScenario, fieldMap);
                                    var idExposureDeterminantCombination = r.GetString(RawExposureEstimates.IdExposureDeterminantCombination, fieldMap);
                                    var idSubstance = r.GetString(RawExposureEstimates.IdSubstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.ExposureScenarios, idExposureScenario)
                                              && (string.IsNullOrEmpty(idExposureDeterminantCombination) || CheckLinkSelected(ScopingType.ExposureDeterminantCombinations, idExposureDeterminantCombination))
                                              && CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var exposureEstimate = new ExposureEstimate {
                                            ExposureScenario = exposureScenarios[idExposureScenario],
                                            ExposureDeterminantCombination = string.IsNullOrEmpty(idExposureDeterminantCombination) ? null : exposureDeterminantCombinations[idExposureDeterminantCombination],
                                            ExposureSource = r.GetString(RawExposureEstimates.ExposureSource, fieldMap),
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            ExposureRoute = r.GetEnum(RawExposureEstimates.ExposureRoute, fieldMap, ExposureRoute.Undefined),
                                            Value = r.GetDouble(RawExposureEstimates.Value, fieldMap),
                                            EstimateType = r.GetString(RawExposureEstimates.EstimateType, fieldMap),
                                        };
                                        exposureEstimates.Add(exposureEstimate);
                                    }
                                }
                            }
                        }
                        _data.AllSingleValueNonDietaryExposureEstimates = exposureEstimates;
                    }
                }
            }
            return _data.AllSingleValueNonDietaryExposureEstimates;
        }

        private void SetPropertyValue(IDataReader r, int[] fieldMap, ref ExposureDeterminantValue value) {
            var strValue = r.GetStringOrNull(RawExposureDeterminantValues.TextValue, fieldMap);
            switch (value.Property.PropertyType) {
                case IndividualPropertyType.Boolean: {
                        value.TextValue = BooleanTypeConverter.FromString(strValue, BooleanType.False).GetDisplayName();
                    }
                    break;
                case IndividualPropertyType.Integer:
                case IndividualPropertyType.Numeric: {
                        value.DoubleValue = r.GetDoubleOrNull(RawExposureDeterminantValues.DoubleValue, fieldMap);
                    }
                    break;
                default: {
                        value.TextValue = strValue; break;
                    }
            }
        }
    }
}
