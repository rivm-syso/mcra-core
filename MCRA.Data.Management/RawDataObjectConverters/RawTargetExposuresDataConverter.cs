using MCRA.Utils.Statistics;
using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers.Exposure;
using MCRA.Data.Raw.Objects.TargetExposures;

namespace MCRA.Data.Management.RawDataObjectConverters {
    public sealed class RawTargetExposuresDataConverter : RawTableGroupDataConverterBase<RawTargetExposuresData> {

        public override RawTargetExposuresData FromCompiledData(CompiledData data) {
            return ToRaw(data.AllTargetExposureModels.Values);
        }

        public RawTargetExposuresData ToRaw(IEnumerable<TargetExposureModel> targetExposureModels) {
            var result = new RawTargetExposuresData();
            foreach (var model in targetExposureModels) {
                var modelRecord = new RawTargetExposureModelRecord() {
                    idTargetExposureModel = model.Code,
                    Name = model.Name,
                    Description = model.Description,
                    idSubstance = model.Compound?.Code,
                    ExposureUnit = model.ExposureUnitString
                };
                result.TargetExposureModelRecords.Add(modelRecord);
                if (model.TargetExposurePercentiles?.Any() ?? false) {
                    foreach (var percentile in model.TargetExposurePercentiles.Values) {
                        var percentileRecord = new RawTargetExposurePercentileRecord() {
                            idTargetExposureModel = model.Code,
                            Exposure = percentile.Exposure,
                            Percentage = percentile.Percentage
                        };
                        result.TargetExposurePercentileRecords.Add(percentileRecord);
                        if (percentile.ExposureUncertainties?.Any() ?? false) {
                            for (int i = 0; i < percentile.ExposureUncertainties.Count; i++) {
                                var percentileUncertainRecord = new RawTargetExposurePercentileUncertainRecord() {
                                    idTargetExposureModel = model.Code,
                                    Exposure = percentile.ExposureUncertainties[i],
                                    idUncertaintySet = $"{i}",
                                    Percentage = percentile.Percentage
                                };
                                result.TargetExposurePercentileUncertainRecords.Add(percentileUncertainRecord);
                            }
                        }
                    }
                }
            }
            return result;
        }

        public RawTargetExposuresData ToRaw(
            ICollection<SimpleExposureStatistics> exposureStatisticss,
            double[] percentages
        ) {
            var result = new RawTargetExposuresData();
            foreach (var exposureStatistics in exposureStatisticss) {
                var modelRecord = new RawTargetExposureModelRecord() {
                    idTargetExposureModel = exposureStatistics.Code,
                    Name = exposureStatistics.Name,
                    Description = exposureStatistics.Description,
                    idSubstance = exposureStatistics.Substance?.Code,
                    ExposureUnit = exposureStatistics.TargetUnit?.GetShortDisplayName(false)
                };
                var percentiles = exposureStatistics.Intakes.PercentilesWithSamplingWeights(exposureStatistics.SamplingWeights, percentages);
                var percentileRecords = percentages
                    .Select((p, ix) => new RawTargetExposurePercentileRecord() {
                        idTargetExposureModel = exposureStatistics.Code,
                        Percentage = p,
                        Exposure = percentiles[ix]
                    })
                    .ToList();
                result.TargetExposureModelRecords.Add(modelRecord);
                result.TargetExposurePercentileRecords.AddRange(percentileRecords);
            }
            return result;
        }

        public void AppendUncertaintyRunValues(
            RawTargetExposuresData data,
            int bootstrap,
            ICollection<SimpleExposureStatistics> exposureStatisticss,
            double[] percentages
        ) {
            foreach (var exposureStatistics in exposureStatisticss) {
                var percentiles = exposureStatistics.Intakes.PercentilesWithSamplingWeights(exposureStatistics.SamplingWeights, percentages);
                var percentileRecords = percentages
                    .Select((p, ix) => new RawTargetExposurePercentileUncertainRecord() {
                        idTargetExposureModel = exposureStatistics.Code,
                        idUncertaintySet = $"{bootstrap}",
                        Percentage = p,
                        Exposure = percentiles[ix]
                    })
                    .ToList();
                data.TargetExposurePercentileUncertainRecords.AddRange(percentileRecords);
            }
        }
    }
}
