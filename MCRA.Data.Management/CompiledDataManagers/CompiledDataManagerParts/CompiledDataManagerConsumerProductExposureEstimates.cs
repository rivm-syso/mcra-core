using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {
        private readonly char _split = '|';

        public IDictionary<string, ConsumerProductExposureAssessment> GetAllConsumerProductExposureAssessments() {
            if (_data.AllConsumerProductExposureAssessments == null) {
                LoadScope(SourceTableGroup.ConsumerProductExposureEstimates);
                var allExposureAssessments = new Dictionary<string, ConsumerProductExposureAssessment>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.ConsumerProductExposureEstimates);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            // Read consumer product exposure percentile records
                            var exposurePercentiles = new List<ConsumerProductExposurePercentile>();
                            using (var r = rdm.OpenDataReader<RawConsumerProductExposurePercentiles>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var record = new ConsumerProductExposurePercentile() {
                                        IdExposureSet = r.GetString(RawConsumerProductExposurePercentiles.IdExposureSet, fieldMap),
                                        Percentage = r.GetDouble(RawConsumerProductExposurePercentiles.Percentage, fieldMap),
                                        Value = r.GetDouble(RawConsumerProductExposurePercentiles.Value, fieldMap)
                                    };
                                    exposurePercentiles.Add(record);
                                }
                            }

                            // Read consumer product exposure set records
                            GetAllCompounds();
                            GetAllConsumerProducts();
                            var exposureSets = new List<ConsumerProductExposureSet>();
                            using (var r = rdm.OpenDataReader<RawConsumerProductExposureSets>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawConsumerProductExposureSets.IdSubstance, fieldMap);
                                    var exposureRoutesString = r.GetStringOrNull(RawConsumerProductExposureSets.ExposureRoutes, fieldMap);
                                    var productsString = r.GetStringOrNull(RawConsumerProductExposureSets.IdProducts, fieldMap);
                                    var idConsumerProducts = productsString?.Split(_split).Select(cp => cp.Trim()).ToList();
                                    var consumerProducts = idConsumerProducts?.Where(cp => CheckLinkSelected(ScopingType.ConsumerProducts, cp))
                                        .Select(cp => _data.GetOrAddConsumerProduct(cp))
                                        .OrderBy(cp => cp.Name)
                                        .ToList();
                                    var exposureRoutes = exposureRoutesString?.Split(_split)
                                        .Select(er => (ExposureRoute)Enum.Parse(typeof(ExposureRoute), er.Trim()))
                                        .OrderBy(cp => cp)
                                        .ToList();
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance) && consumerProducts?.Count > 0 && exposureRoutes?.Count == 1;
                                    if (valid) {
                                        var estimateType = r.GetString(RawConsumerProductExposureSets.EstimateType, fieldMap);
                                        var record = new ConsumerProductExposureSet() {
                                            IdExposureSet = r.GetString(RawConsumerProductExposureSets.IdExposureSet, fieldMap),
                                            IdAssessment = r.GetString(RawConsumerProductExposureSets.IdAssessment, fieldMap),
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            EstimateType = r.GetEnum(RawConsumerProductExposureSets.EstimateType, fieldMap, defaultValue: ConsumerProductExposureEstimateType.Systemic),
                                            ExposureRoutes = exposureRoutes,
                                            Products = consumerProducts,
                                            AbsorptionFactor = r.GetDoubleOrNull(RawConsumerProductExposureSets.AbsorptionFactor, fieldMap) ?? 1,
                                            PercentageZeros = r.GetDoubleOrNull(RawConsumerProductExposureSets.PercentageZeros, fieldMap),
                                            ConsumerProductExposureEstimateUnit = r.GetEnum(RawConsumerProductExposureSets.Unit, fieldMap, defaultValue: ConsumerProductExposureEstimateUnit.mgPerKgBWPerDay)
                                        };
                                        record.ExposurePercentiles = [.. exposurePercentiles.Where(p => p.IdExposureSet == record.IdExposureSet)];
                                        if (record.ExposurePercentiles?.Count > 0) {
                                            record.ExposurePercentiles.Sort((a, b) => a.Percentage.CompareTo(b.Percentage));
                                            exposureSets.Add(record);
                                        }
                                    }
                                }
                            }
                            // Read consumer product exposure assessment records
                            using (var r = rdm.OpenDataReader<RawConsumerProductExposureAssessments>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    //TODO: check if the scoping is valid for the assessment, and if not, skip it
                                    var idAssessment = r.GetString(RawConsumerProductExposureAssessments.IdAssessment, fieldMap);
                                    var record = new ConsumerProductExposureAssessment() {
                                        IdAssessment = idAssessment,
                                        Name = r.GetString(RawConsumerProductExposureAssessments.Name, fieldMap),
                                        Description = r.GetStringOrNull(RawConsumerProductExposureAssessments.Description, fieldMap),
                                        ExposureType = r.GetEnum<ExposureType>(RawConsumerProductExposureAssessments.ExposureType, fieldMap),
                                        Location = r.GetStringOrNull(RawConsumerProductExposureAssessments.Location, fieldMap),
                                        AgeMin = r.GetDoubleOrNull(RawConsumerProductExposureAssessments.AgeMin, fieldMap),
                                        AgeMax = r.GetDoubleOrNull(RawConsumerProductExposureAssessments.AgeMax, fieldMap),
                                        Gender = r.GetEnum<GenderType>(RawConsumerProductExposureAssessments.Gender, fieldMap, defaultValue: GenderType.Undefined, allowInvalidString: true),
                                        Reference = r.GetStringOrNull(RawConsumerProductExposureAssessments.Reference, fieldMap)
                                    };
                                    record.ExposureSets = [.. exposureSets.Where(s => s.IdAssessment == record.IdAssessment)];
                                    if (record.ExposureSets != null && record.ExposureSets.Count > 0) {
                                        allExposureAssessments.Add(idAssessment, record);
                                    }
                                }
                            }
                        }
                    }
                    _data.AllConsumerProductExposureAssessments = allExposureAssessments;
                }
            }
            return _data.AllConsumerProductExposureAssessments;
        }
    }
}
