using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// GetAllDoseResponseModels
        /// </summary>
        /// <returns></returns>
        public IList<DoseResponseModel> GetAllDoseResponseModels() {
            if (_data.AllDoseResponseModels == null) {
                LoadScope(SourceTableGroup.DoseResponseModels);
                var allDoseResponseModels = new Dictionary<string, DoseResponseModel>(StringComparer.OrdinalIgnoreCase);

                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.DoseResponseModels);
                //if no data source specified: return immediately.
                if (rawDataSourceIds?.Count > 0) {
                    GetAllResponses();
                    GetAllCompounds();
                    GetAllDoseResponseExperiments();

                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read dose response models
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawDoseResponseModels>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idModel = r.GetString(RawDoseResponseModels.IdDoseResponseModel, fieldMap);
                                    var substanceCodes = r.GetString(RawDoseResponseModels.Substances, fieldMap).Split(',').Select(c => c.Trim()).ToArray();
                                    var responseCode = r.GetString(RawDoseResponseModels.IdResponse, fieldMap);
                                    var experimentId = r.GetString(RawDoseResponseModels.IdExperiment, fieldMap);
                                    var valid = IsCodeSelected(ScopingType.DoseResponseModels, idModel)
                                              & CheckLinkSelected(ScopingType.Compounds, substanceCodes)
                                              & CheckLinkSelected(ScopingType.Responses, responseCode)
                                              & CheckLinkSelected(ScopingType.DoseResponseExperiments, experimentId);
                                    if (valid) {
                                        var substances = substanceCodes.Select(code => _data.GetOrAddSubstance(code));
                                        var response = _data.AllResponses[responseCode];

                                        // Get covariates
                                        var covariates = new List<string>();
                                        if (r.GetStringOrNull(RawDoseResponseModels.Covariates, fieldMap) != null) {
                                            covariates = (r.GetStringOrNull(RawDoseResponseModels.Covariates, fieldMap))
                                                .Split(',')
                                                .Select(c => c.Trim())
                                                .ToList();
                                        }

                                        var drm = new DoseResponseModel {
                                            IdDoseResponseModel = idModel,
                                            Name = r.GetStringOrNull(RawDoseResponseModels.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawDoseResponseModels.Description, fieldMap),
                                            Substances = substances.ToList(),
                                            Response = response,
                                            Covariates = covariates,
                                            CriticalEffectSize = r.GetDouble(RawDoseResponseModels.CriticalEffectSize, fieldMap),
                                            ModelEquation = r.GetStringOrNull(RawDoseResponseModels.ModelEquation, fieldMap),
                                            ModelParameterValues = r.GetStringOrNull(RawDoseResponseModels.ModelParameterValues, fieldMap),
                                            LogLikelihood = r.GetDoubleOrNull(RawDoseResponseModels.LogLikelihood, fieldMap),
                                            DoseUnit = r.GetEnum(RawDoseResponseModels.DoseUnit, fieldMap, DoseUnit.mgPerKgBWPerDay),
                                            IdExperiment = experimentId,
                                            DoseResponseModelBenchmarkDoses = new Dictionary<string, DoseResponseModelBenchmarkDose>(StringComparer.OrdinalIgnoreCase),
                                        };
                                        allDoseResponseModels.Add(idModel, drm);
                                    }
                                }
                            }
                        }

                        // Read benchmark doses
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawDoseResponseModelBenchmarkDoses>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {

                                    // Get linked dose response model
                                    var idDoseResponseModel = r.GetString(RawDoseResponseModelBenchmarkDoses.IdDoseResponseModel, fieldMap);
                                    var idSubstance = r.GetString(RawDoseResponseModelBenchmarkDoses.IdSubstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.DoseResponseModels, idDoseResponseModel)
                                              & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        // Restrict to filter, if used:
                                        var benchmarkDoseLower = r.GetDoubleOrNull(RawDoseResponseModelBenchmarkDoses.BenchmarkDoseLower, fieldMap);
                                        var benchmarkDoseUpper = r.GetDoubleOrNull(RawDoseResponseModelBenchmarkDoses.BenchmarkDoseUpper, fieldMap);
                                        var covariateLevel = r.GetStringOrNull(RawDoseResponseModelBenchmarkDoses.Covariates, fieldMap);
                                        var drmbd = new DoseResponseModelBenchmarkDose {
                                            IdDoseResponseModel = idDoseResponseModel,
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            CovariateLevel = covariateLevel ?? string.Empty,
                                            ModelParameterValues = r.GetStringOrNull(RawDoseResponseModelBenchmarkDoses.ModelParameterValues, fieldMap),
                                            BenchmarkDose = r.GetDouble(RawDoseResponseModelBenchmarkDoses.BenchmarkDose, fieldMap),
                                            BenchmarkDoseLower = benchmarkDoseLower ?? double.NaN,
                                            BenchmarkDoseUpper = benchmarkDoseUpper ?? double.NaN,
                                            BenchmarkResponse = r.GetDouble(RawDoseResponseModelBenchmarkDoses.BenchmarkResponse, fieldMap),
                                        };
                                        allDoseResponseModels[idDoseResponseModel].DoseResponseModelBenchmarkDoses.Add(drmbd.Key, drmbd);
                                    }
                                }
                            }
                        }

                        // Read benchmark doses bootstraps
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawDoseResponseModelBenchmarkDosesUncertain>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idDoseResponseModel = r.GetString(RawDoseResponseModelBenchmarkDosesUncertain.IdDoseResponseModel, fieldMap);
                                        var idSubstance = r.GetString(RawDoseResponseModelBenchmarkDosesUncertain.IdSubstance, fieldMap);
                                        var valid = CheckLinkSelected(ScopingType.DoseResponseModels, idDoseResponseModel)
                                                  & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                        if (valid) {
                                            var covariateLevel = r.GetStringOrNull(RawDoseResponseModelBenchmarkDosesUncertain.Covariates, fieldMap);
                                            var drmbdu = new DoseResponseModelBenchmarkDoseUncertain {
                                                IdDoseResponseModel = idDoseResponseModel,
                                                IdUncertaintySet = r.GetStringOrNull(RawDoseResponseModelBenchmarkDosesUncertain.IdUncertaintySet, fieldMap),
                                                Substance = _data.GetOrAddSubstance(idSubstance),
                                                CovariateLevel = covariateLevel ?? string.Empty,
                                                BenchmarkDose = r.GetDouble(RawDoseResponseModelBenchmarkDosesUncertain.BenchmarkDose, fieldMap)
                                            };
                                            if (allDoseResponseModels[idDoseResponseModel].DoseResponseModelBenchmarkDoses.TryGetValue(drmbdu.Key, out var drmbd)) {
                                                drmbd.DoseResponseModelBenchmarkDoseUncertains.Add(drmbdu);
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllDoseResponseModels = allDoseResponseModels;
            }
            return _data.AllDoseResponseModels.Values.ToList();
        }
    }
}
