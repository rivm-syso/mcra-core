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
        public IDictionary<string, DietaryExposureModel> GetAllDietaryExposureModels() {
            if (_data.AllDietaryExposureModels == null) {
                LoadScope(SourceTableGroup.DietaryExposures);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.DietaryExposures);
                var allDietaryExposureModels = new Dictionary<string, DietaryExposureModel>(StringComparer.OrdinalIgnoreCase);

                // If no data source specified: return immediately.
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllCompounds();

                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read dietary exposure models
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawDietaryExposureModels>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idModel = r.GetString(RawDietaryExposureModels.IdDietaryExposureModel, fieldMap);
                                    var idSubstance = r.GetString(RawDietaryExposureModels.IdSubstance, fieldMap);
                                    if (!IsCodeSelected(ScopingType.Compounds, idSubstance)) {
                                        continue;
                                    }
                                    var compound = _data.GetOrAddSubstance(idSubstance);
                                    var model = new DietaryExposureModel() {
                                        Code = idModel,
                                        Name = r.GetStringOrNull(RawDietaryExposureModels.Name, fieldMap),
                                        Description = r.GetStringOrNull(RawDietaryExposureModels.Description, fieldMap),
                                        Compound = compound,
                                        ExposureUnit = r.GetEnum(RawDietaryExposureModels.ExposureUnit, fieldMap, ExternalExposureUnit.mgPerKgBWPerDay),
                                        DietaryExposurePercentiles = []
                                    };
                                    allDietaryExposureModels.Add(idModel, model);
                                }
                            }
                        }

                        // Read dietary exposure model percentiles
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawDietaryExposurePercentiles>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idDietaryExposureModel = r.GetString(RawDietaryExposurePercentiles.IdDietaryExposureModel, fieldMap);
                                    if (!CheckLinkSelected(ScopingType.DietaryExposureModels, idDietaryExposureModel)) {
                                        continue;
                                    }
                                    var percentileRecord = new DietaryExposurePercentile() {
                                        Percentage = r.GetDouble(RawDietaryExposurePercentiles.Percentage, fieldMap),
                                        Exposure = r.GetDouble(RawDietaryExposurePercentiles.Exposure, fieldMap),
                                        ExposureUncertainties = new List<double>()
                                    };
                                    allDietaryExposureModels[idDietaryExposureModel].DietaryExposurePercentiles
                                        .Add(percentileRecord.Percentage, percentileRecord);
                                }
                            }
                        }

                        // Read dietary exposure model percentile bootstraps
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawDietaryExposurePercentilesUncertain>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idDietaryExposureModel = r.GetString(RawDietaryExposurePercentilesUncertain.IdDietaryExposureModel, fieldMap);
                                        var valid = CheckLinkSelected(ScopingType.DietaryExposureModels, idDietaryExposureModel);
                                        if (valid) {
                                            var percentage = r.GetDouble(RawDietaryExposurePercentilesUncertain.Percentage, fieldMap);
                                            var exposure = r.GetDouble(RawDietaryExposurePercentilesUncertain.Exposure, fieldMap);
                                            var model = allDietaryExposureModels[idDietaryExposureModel];
                                            if (model.DietaryExposurePercentiles.TryGetValue(percentage, out var percentileRecord)) {
                                                percentileRecord.ExposureUncertainties.Add(exposure);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                _data.AllDietaryExposureModels = allDietaryExposureModels;
            }
            return _data.AllDietaryExposureModels;
        }

        private static void writeDietaryExposureModelsToCsv(string tempFolder, IEnumerable<DietaryExposureModel> dietaryExposureModels) {
            if (!dietaryExposureModels?.Any() ?? true) {
                return;
            }

            var mapper = new RawDietaryExposuresDataConverter();
            var rawData = mapper.ToRaw(dietaryExposureModels);
            var writer = new CsvRawDataWriter(tempFolder);
            writer.Set(rawData);
            writer.Store();
        }
    }
}
