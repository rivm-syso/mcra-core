using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management.RawDataObjectConverters;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all dietary exposure models.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, TargetExposureModel> GetAllTargetExposureModels() {
            if (_data.AllTargetExposureModels == null) {
                LoadScope(SourceTableGroup.TargetExposures);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.TargetExposures);
                var allTargetExposureModels = new Dictionary<string, TargetExposureModel>(StringComparer.OrdinalIgnoreCase);

                // If no data source specified: return immediately.
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllCompounds();

                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read target exposure models
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawTargetExposureModels>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idModel = r.GetString(RawTargetExposureModels.IdTargetExposureModel, fieldMap);
                                    var idSubstance = r.GetString(RawTargetExposureModels.IdSubstance, fieldMap);
                                    if (!IsCodeSelected(ScopingType.Compounds, idSubstance)) {
                                        continue;
                                    }
                                    var compound = _data.GetOrAddSubstance(idSubstance);
                                    var model = new TargetExposureModel() {
                                        Code = idModel,
                                        Name = r.GetStringOrNull(RawTargetExposureModels.Name, fieldMap),
                                        Description = r.GetStringOrNull(RawTargetExposureModels.Description, fieldMap),
                                        Compound = compound,
                                        ExposureUnit = r.GetEnum(RawTargetExposureModels.ExposureUnit, fieldMap, ExternalExposureUnit.mgPerKgBWPerDay),
                                        TargetExposurePercentiles = []
                                    };
                                    allTargetExposureModels.Add(idModel, model);
                                }
                            }
                        }

                        // Read Target exposure model percentiles
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawTargetExposurePercentiles>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idTargetExposureModel = r.GetString(RawTargetExposurePercentiles.IdTargetExposureModel, fieldMap);
                                    if (!CheckLinkSelected(ScopingType.TargetExposureModels, idTargetExposureModel)) {
                                        continue;
                                    }
                                    var percentileRecord = new TargetExposurePercentile() {
                                        Percentage = r.GetDouble(RawTargetExposurePercentiles.Percentage, fieldMap),
                                        Exposure = r.GetDouble(RawTargetExposurePercentiles.Exposure, fieldMap),
                                        ExposureUncertainties = new List<double>()
                                    };
                                    allTargetExposureModels[idTargetExposureModel].TargetExposurePercentiles
                                        .Add(percentileRecord.Percentage, percentileRecord);
                                }
                            }
                        }

                        // Read Target exposure model percentile bootstraps
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawTargetExposurePercentilesUncertain>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idTargetExposureModel = r.GetString(RawTargetExposurePercentilesUncertain.IdTargetExposureModel, fieldMap);
                                        var valid = CheckLinkSelected(ScopingType.TargetExposureModels, idTargetExposureModel);
                                        if (valid) {
                                            var percentage = r.GetDouble(RawTargetExposurePercentilesUncertain.Percentage, fieldMap);
                                            var exposure = r.GetDouble(RawTargetExposurePercentilesUncertain.Exposure, fieldMap);
                                            var model = allTargetExposureModels[idTargetExposureModel];
                                            if (model.TargetExposurePercentiles.TryGetValue(percentage, out var percentileRecord)) {
                                                percentileRecord.ExposureUncertainties.Add(exposure);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                _data.AllTargetExposureModels = allTargetExposureModels;
            }
            return _data.AllTargetExposureModels;
        }

        private static void writeTargetExposureModelsToCsv(string tempFolder, IEnumerable<TargetExposureModel> TargetExposureModels) {
            if (!TargetExposureModels?.Any() ?? true) {
                return;
            }

            var mapper = new RawTargetExposuresDataConverter();
            var rawData = mapper.ToRaw(TargetExposureModels);
            var writer = new CsvRawDataWriter(tempFolder);
            writer.Set(rawData);
            writer.Store();
        }
    }
}
