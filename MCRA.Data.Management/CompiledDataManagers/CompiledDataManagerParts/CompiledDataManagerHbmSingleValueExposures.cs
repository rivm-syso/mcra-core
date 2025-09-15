using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management.RawDataObjectConverters;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all single value HBM concentration models.
        /// </summary>
        /// <returns></returns>
        public ICollection<HbmSingleValueExposureSet> GetAllHbmSingleValueExposures() {
            if (_data.AllHbmSingleValueExposureSets == null) {
                LoadScope(SourceTableGroup.HbmSingleValueExposures);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.HbmSingleValueExposures);
                var allHbmSingleValueExposureSets = new Dictionary<string, HbmSingleValueExposureSet>(StringComparer.OrdinalIgnoreCase);
                var surveys = new Dictionary<string, HbmSingleValueExposureSurvey>(StringComparer.OrdinalIgnoreCase);
                // If no data source specified: return immediately.
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();

                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read  surveys
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawHbmSingleValueExposureSurveys>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSurvey = r.GetString(RawHbmSingleValueExposureSurveys.Id, fieldMap);
                                    var valid = IsCodeSelected(ScopingType.HbmSingleValueExposureSurveys, idSurvey);
                                    if (valid) {
                                        var survey = new HbmSingleValueExposureSurvey() {
                                            Id = idSurvey,
                                            Description = r.GetStringOrNull(RawHbmSingleValueExposureSurveys.Description, fieldMap),
                                            Name = r.GetStringOrNull(RawHbmSingleValueExposureSurveys.Name, fieldMap),
                                            Country = r.GetStringOrNull(RawHbmSingleValueExposureSurveys.Country, fieldMap)
                                        };
                                        surveys.Add(idSurvey, survey);
                                    }
                                }
                            }
                        }

                        // Read single value HBM concentration models
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawHbmSingleValueExposureSets>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawHbmSingleValueExposureSets.IdSubstance, fieldMap);
                                    var idSurvey = r.GetString(RawHbmSingleValueExposureSets.IdSurvey, fieldMap);
                                    if (IsCodeSelected(ScopingType.Compounds, idSubstance)
                                        && CheckLinkSelected(ScopingType.HbmSingleValueExposureSurveys, idSurvey)
                                    ) {
                                        var idModel = r.GetString(RawHbmSingleValueExposureSets.Id, fieldMap);
                                        var compound = _data.GetOrAddSubstance(idSubstance);
                                        var model = new HbmSingleValueExposureSet() {
                                            Code = idModel,
                                            Survey = surveys[idSurvey],
                                            Substance = compound,
                                            BiologicalMatrix = r.GetEnum(RawHbmSingleValueExposureSets.BiologicalMatrix, fieldMap, BiologicalMatrix.Undefined),
                                            ExpressionType = r.GetEnum(RawHbmSingleValueExposureSets.ExpressionType, fieldMap, ExpressionType.None),
                                            DoseUnit = r.GetEnum(RawHbmSingleValueExposureSets.DoseUnit, fieldMap, DoseUnit.ugPerL),
                                            HbmSingleValueExposures = []
                                        };
                                        allHbmSingleValueExposureSets.Add(idModel, model);
                                    }
                                }
                            }
                        }

                        // Read single value HBM concentration model percentiles
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawHbmSingleValueExposures>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idHbmSingleValueExposureSet = r.GetString(RawHbmSingleValueExposures.IdExposureSet, fieldMap);
                                    if (allHbmSingleValueExposureSets.TryGetValue(idHbmSingleValueExposureSet, out var exposureSet)) {
                                        var percentileRecord = new HbmSingleValueExposure() {
                                            Percentage = r.GetDouble(RawHbmSingleValueExposures.Percentage, fieldMap),
                                            Value = r.GetDouble(RawHbmSingleValueExposures.Value, fieldMap),
                                        };
                                        exposureSet.HbmSingleValueExposures.Add(percentileRecord);
                                    }
                                }
                            }
                        }
                    }
                }

                _data.AllHbmSingleValueExposureSets = allHbmSingleValueExposureSets.Select(c=> c.Value).ToList();
            }
            return _data.AllHbmSingleValueExposureSets;
        }

        private static void writeHbmSingleValueExposureSetsToCsv(
            string tempFolder,
            IEnumerable<HbmSingleValueExposureSet> hbmSingleValueExposureSets
        ) {
            if (!hbmSingleValueExposureSets?.Any() ?? true) {
                return;
            }

            var mapper = new RawHbmSingleValueExposuresDataConverter();
            var rawData = mapper.ToRaw(hbmSingleValueExposureSets);
            var writer = new CsvRawDataWriter(tempFolder);
            writer.Set(rawData);
            writer.Store();
        }
    }
}
