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
        public IDictionary<string, RiskModel> GetAllRiskModels() {
            if (_data.AllRiskModels == null) {
                LoadScope(SourceTableGroup.Risks);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.Risks);
                var allRiskModels = new Dictionary<string, RiskModel>(StringComparer.OrdinalIgnoreCase);

                // If no data source specified: return immediately.
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllCompounds();

                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read target exposure models
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawRiskModels>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idModel = r.GetString(RawRiskModels.IdRiskModel, fieldMap);
                                    var idSubstance = r.GetString(RawRiskModels.IdSubstance, fieldMap);
                                    if (!IsCodeSelected(ScopingType.Compounds, idSubstance)) {
                                        continue;
                                    }
                                    var compound = _data.GetOrAddSubstance(idSubstance);
                                    var model = new RiskModel() {
                                        Code = idModel,
                                        Name = r.GetStringOrNull(RawRiskModels.Name, fieldMap),
                                        Description = r.GetStringOrNull(RawRiskModels.Description, fieldMap),
                                        Compound = compound,
                                        RiskPercentiles = new Dictionary<double, RiskPercentile>()
                                    };
                                    allRiskModels.Add(idModel, model);
                                }
                            }
                        }

                        // Read Target exposure model percentiles
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawRiskPercentiles>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idRiskModel = r.GetString(RawRiskPercentiles.IdRiskModel, fieldMap);
                                    if (!CheckLinkSelected(ScopingType.RiskModels, idRiskModel)) {
                                        continue;
                                    }
                                    var percentileRecord = new RiskPercentile() {
                                        Percentage = r.GetDouble(RawRiskPercentiles.Percentage, fieldMap),
                                        MarginOfExposure = r.GetDouble(RawRiskPercentiles.MarginOfExposure, fieldMap),
                                        MarginOfExposureUncertainties = new List<double>()
                                    };
                                    allRiskModels[idRiskModel].RiskPercentiles
                                        .Add(percentileRecord.Percentage, percentileRecord);
                                }
                            }
                        }

                        // Read Target exposure model percentile bootstraps
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawRiskPercentileUncertains>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idRiskModel = r.GetString(RawRiskPercentileUncertains.IdRiskModel, fieldMap);
                                        var valid = CheckLinkSelected(ScopingType.RiskModels, idRiskModel);
                                        if (valid) {
                                            var percentage = r.GetDouble(RawRiskPercentileUncertains.Percentage, fieldMap);
                                            var exposure = r.GetDouble(RawRiskPercentileUncertains.MarginOfExposure, fieldMap);
                                            var model = allRiskModels[idRiskModel];
                                            if (model.RiskPercentiles.TryGetValue(percentage, out var percentileRecord)) {
                                                percentileRecord.MarginOfExposureUncertainties.Add(exposure);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                _data.AllRiskModels = allRiskModels;
            }
            return _data.AllRiskModels;
        }

        private static void writeRiskModelsToCsv(string tempFolder, IEnumerable<RiskModel> RiskModels) {
            if (!RiskModels?.Any() ?? true) {
                return;
            }

            var mapper = new RawRisksDataConverter();
            var rawData = mapper.ToRaw(RiskModels);
            var writer = new CsvRawDataWriter(tempFolder);
            writer.Set(rawData);
            writer.Store();
        }
    }
}
