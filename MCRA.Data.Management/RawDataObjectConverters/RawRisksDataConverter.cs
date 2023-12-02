using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Raw.Objects.RawTableGroups;
using MCRA.Data.Raw.Objects.RawTableObjects;

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
                    RiskMetric = model.RiskMetric.ToString(),
                };
                result.RiskModelRecords.Add(modelRecord);
                if (model.RiskPercentiles?.Any() ?? false) {
                    foreach (var percentile in model.RiskPercentiles.Values) {
                        var percentileRecord = new RawRiskPercentileRecord() {
                            idRiskModel = model.Code,
                            Risk = percentile.Risk,
                            Percentage = percentile.Percentage,
                        };
                        result.RiskPercentileRecords.Add(percentileRecord);
                        if (percentile.RiskUncertainties?.Any() ?? false) {
                            for (int i = 0; i < percentile.RiskUncertainties.Count; i++) {
                                var percentileUncertainRecord = new RawRiskPercentileUncertainRecord() {
                                    idRiskModel = model.Code,
                                    idUncertaintySet = $"{i}",
                                    Risk = percentile.RiskUncertainties[i],
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

        public void AppendUncertaintyRunValues(
            RawRisksData data,
            int bootstrap,
            IEnumerable<RiskModel> risksModels
        ) {
            foreach (var item in risksModels) {
                var percentileRecords = item.RiskPercentiles.Values
                    .Select((p, ix) => new RawRiskPercentileUncertainRecord() {
                        idRiskModel = item.Code,
                        idUncertaintySet = $"{bootstrap}",
                        Percentage = p.Percentage,
                        Risk = p.Risk
                    })
                    .ToList();
                data.RiskPercentileUncertainRecords.AddRange(percentileRecords);
            }
        }
    }
}
