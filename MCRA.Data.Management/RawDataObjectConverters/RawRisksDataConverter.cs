using MCRA.Utils.Statistics;
using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers.Exposure;
using MCRA.Data.Raw.Objects.Risks;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Management.RawDataObjectConverters {
    public sealed class RawRisksDataConverter : RawTableGroupDataConverterBase<RawRisksData> {

        public override RawRisksData FromCompiledData(CompiledData data) {
            return ToRaw(data.AllRiskModels.Values);
        }

        public RawRisksData ToRaw(IEnumerable<RiskModel> risksModels) {
            var result = new RawRisksData();
            foreach (var model in risksModels) {
                var modelRecord = new RawRiskModelRecord() {
                    idRiskModel = model.Code,
                    idSubstance = model.Compound?.Code,
                    Description = model.Description,
                    Name = model.Name,
                };
                result.RiskModelRecords.Add(modelRecord);
                if (model.RiskPercentiles?.Any() ?? false) {
                    foreach (var percentile in model.RiskPercentiles.Values) {
                        var percentileRecord = new RawRiskPercentileRecord() {
                            idRiskModel = model.Code,
                            MarginOfExposure = percentile.MarginOfExposure,
                            Percentage = percentile.Percentage,
                        };
                        result.RiskPercentileRecords.Add(percentileRecord);
                        if (percentile.MarginOfExposureUncertainties?.Any() ?? false) {
                            for (int i = 0; i < percentile.MarginOfExposureUncertainties.Count; i++) {
                                var percentileUncertainRecord = new RawRiskPercentileUncertainRecord() {
                                    idRiskModel = model.Code,
                                    idUncertaintySet = $"{i}",
                                    MarginOfExposure = percentile.MarginOfExposureUncertainties[i],
                                    Percentage = percentile.Percentage,
                                };
                                result.RiskPercentileUncertainRecords.Add(percentileUncertainRecord);
                            }
                        }
                    }
                }
            }
            return result;
        }

        public RawRisksData ToRaw(
            ICollection<SimpleMarginOfExposureStatistics> exposureStatisticss,
            double[] percentages
        ) {
            var result = new RawRisksData();
            foreach (var exposureStatistics in exposureStatisticss) {
                var modelRecord = new RawRiskModelRecord() {
                    idRiskModel = exposureStatistics.Code,
                    Name = exposureStatistics.Name,
                    Description = exposureStatistics.Description,
                    idSubstance = exposureStatistics.Substance.Code,
                };
                var percentiles = exposureStatistics.Intakes.PercentilesWithSamplingWeights(exposureStatistics.SamplingWeights, percentages);
                var percentileRecords = percentages
                    .Select((p, ix) => new RawRiskPercentileRecord() {
                        idRiskModel = exposureStatistics.Code,
                        Percentage = p,
                        MarginOfExposure = percentiles[ix]
                    })
                    .ToList();
                result.RiskModelRecords.Add(modelRecord);
                result.RiskPercentileRecords.AddRange(percentileRecords);
            }
            return result;
        }

        public void AppendUncertaintyRunValues(
            RawRisksData data,
            int bootstrap,
            ICollection<SimpleMarginOfExposureStatistics> exposureStatisticss,
            double[] percentages
        ) {
            foreach (var exposureStatistics in exposureStatisticss) {
                var percentiles = exposureStatistics.Intakes.PercentilesWithSamplingWeights(exposureStatistics.SamplingWeights, percentages);
                var percentileRecords = percentages
                    .Select((p, ix) => new RawRiskPercentileUncertainRecord() {
                        idRiskModel = exposureStatistics.Code,
                        idUncertaintySet = $"{bootstrap}",
                        Percentage = p,
                        MarginOfExposure = percentiles[ix]
                    })
                    .ToList();
                data.RiskPercentileUncertainRecords.AddRange(percentileRecords);
            }
        }
    }
}
