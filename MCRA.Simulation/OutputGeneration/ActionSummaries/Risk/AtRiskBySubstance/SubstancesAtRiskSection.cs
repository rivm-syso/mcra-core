using System.Collections.Concurrent;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SubstancesAtRiskSection : AtRiskSectionBase {
        public RiskMetricType RiskMetric { get; set; }
        public List<SubstanceAtRiskRecord> Records { get; set; }
        public override bool SaveTemporaryData => true;

        public void SummarizeSubstancesAtRisk(
                Dictionary<Compound, List<IndividualEffect>> individualEffects,
                int numberOfCumulativeIndividualEffects,
                HealthEffectType healthEffectType,
                RiskMetricType riskMetric,
                double threshold
            ) {
            HealthEffectType = healthEffectType;
            Threshold = threshold;
            RiskMetric = riskMetric;

            ConcurrentDictionary<int, double> cumulativeDict = null;
            if (individualEffects.Count > 1) {
                cumulativeDict = new();
                Parallel.ForEach(individualEffects.SelectMany(v => v.Value), idv => {
                    cumulativeDict.AddOrUpdate(
                        idv.SimulatedIndividualId,
                        idv.ExposureHazardRatio,
                        (k, v) => v + idv.ExposureHazardRatio
                    );
                });
            }

            var recordsBag = new ConcurrentBag<SubstanceAtRiskRecord>();

            Parallel.ForEach(individualEffects, kvp => {
                var record = createSubstanceAtRiskRecord(
                    riskMetric,
                    kvp.Key,
                    kvp.Value,
                    cumulativeDict,
                    numberOfCumulativeIndividualEffects
                );
                recordsBag.Add(record);
            });

            Records = recordsBag.OrderByDescending(c => c.AtRiskDueToSubstance)
                .ThenBy(c => c.SubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.SubstanceCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Calculates at risks for modelled substance.
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="cumulativeIndividualRisks"></param>
        /// <param name="substance"></param>
        /// <param name="numberOfCumulativeIndividualEffects"></param>
        /// <returns></returns>
        private SubstanceAtRiskRecord createSubstanceAtRiskRecord(
            RiskMetricType metricType,
            Compound substance,
            List<IndividualEffect> individualEffects,
            IDictionary<int, double> cumulativeIndividualRisks,
            int numberOfCumulativeIndividualEffects
        ) {
            var useMarginOfExposure = metricType == RiskMetricType.MarginOfExposure;

            var record = new SubstanceAtRiskRecord() {
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code
            };
            if (cumulativeIndividualRisks != null) {
                var atRiskDueTo = 0;
                var notAtRisk = numberOfCumulativeIndividualEffects - cumulativeIndividualRisks.Count;
                var atRiskWithOrWithout = 0;
                (atRiskDueTo, notAtRisk, atRiskWithOrWithout) = useMarginOfExposure
                    ? CalculateHazardExposureRatioAtRisks(individualEffects, cumulativeIndividualRisks, atRiskDueTo, notAtRisk, atRiskWithOrWithout)
                    : CalculateExposureHazardRatioAtRisks(individualEffects, cumulativeIndividualRisks, atRiskDueTo, notAtRisk, atRiskWithOrWithout);

                record.AtRiskDueToSubstance = 100d * atRiskDueTo / numberOfCumulativeIndividualEffects;
                record.NotAtRisk = 100d * notAtRisk / numberOfCumulativeIndividualEffects;
                record.AtRiskWithOrWithout = 100d * atRiskWithOrWithout / numberOfCumulativeIndividualEffects;
            } else {
                var atRiskDueToSubstance = useMarginOfExposure
                    ? individualEffects.Count(c => c.HazardExposureRatio <= Threshold)
                    : individualEffects.Count(c => c.ExposureHazardRatio >= Threshold);

                record.AtRiskDueToSubstance = 100d * atRiskDueToSubstance / numberOfCumulativeIndividualEffects;
                record.AtRiskWithOrWithout = 0;
                record.NotAtRisk = 100 - record.AtRiskDueToSubstance;
            };
            return record;
        }
    }
}
