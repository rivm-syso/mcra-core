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
            var useCumulative = individualEffects.Count > 1;
            var cumulativeDict = individualEffects.SelectMany(v => v.Value)
                .GroupBy(v => v.SimulationId)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.HazardIndex(HealthEffectType)));

            Records = new List<SubstanceAtRiskRecord>();
            foreach (var kvp in individualEffects) {
                var record = (riskMetric == RiskMetricType.MarginOfExposure)
                    ? createSubstanceAtRiskMOERecord(
                        kvp.Value,
                        useCumulative ? cumulativeDict : null,
                        kvp.Key,
                        numberOfCumulativeIndividualEffects
                      )
                    : createSubstanceAtRiskHIRecord(
                        kvp.Value,
                        useCumulative ? cumulativeDict : null,
                        kvp.Key,
                        numberOfCumulativeIndividualEffects
                      );
                Records.Add(record);
            }
            Records = Records.OrderByDescending(c => c.AtRiskDueToSubstance).ThenBy(c => c.SubstanceName, StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>
        /// Calculates at risks for margin of exposure
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="cumulativeHazardIndices"></param>
        /// <param name="substance"></param>
        /// <param name="numberOfCumulativeIndividualEffects"></param>
        /// <returns></returns>
        private SubstanceAtRiskRecord createSubstanceAtRiskMOERecord(
           List<IndividualEffect> individualEffects,
           IDictionary<int, double> cumulativeHazardIndices,
           Compound substance,
           int numberOfCumulativeIndividualEffects
        ) {
            var record = new SubstanceAtRiskRecord() {
                SubstanceName = $"{substance.Name}",
                SubstanceCode = $"{substance.Code}"
            };
            if (cumulativeHazardIndices != null) {
                var atRiskDueTo = 0;
                var notAtRisk = numberOfCumulativeIndividualEffects - cumulativeHazardIndices.Count;
                var atRiskWithOrWithout = 0;
                (atRiskDueTo, notAtRisk, atRiskWithOrWithout) = CalculateMOEAtRisks(
                    individualEffects,
                    cumulativeHazardIndices,
                    atRiskDueTo,
                    notAtRisk,
                    atRiskWithOrWithout
                );
                record.AtRiskDueToSubstance = 100d * atRiskDueTo / individualEffects.Count;
                record.NotAtRisk = 100d * notAtRisk / individualEffects.Count;
                record.AtRiskWithOrWithout = 100d * atRiskWithOrWithout / individualEffects.Count;
            } else {
                var atRiskDueToSubstance = individualEffects
                   .Count(c => c.MarginOfExposure(HealthEffectType) <= Threshold);

                record.AtRiskDueToSubstance = 100d * atRiskDueToSubstance / individualEffects.Count;
                record.AtRiskWithOrWithout = 0;
                record.NotAtRisk = 100 - record.AtRiskDueToSubstance;
            };
            return record;
        }

        /// <summary>
        /// Calculates risk for hazard indices
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="cumulativeHazardIndices"></param>
        /// <param name="substance"></param>
        /// <param name="numberOfCumulativeIndividualEffects"></param>
        /// <returns></returns>
        private SubstanceAtRiskRecord createSubstanceAtRiskHIRecord(
           List<IndividualEffect> individualEffects,
           IDictionary<int, double> cumulativeHazardIndices,
           Compound substance,
           int numberOfCumulativeIndividualEffects
      ) {
            var record = new SubstanceAtRiskRecord() {
                SubstanceName = $"{substance.Name}",
                SubstanceCode = $"{substance.Code}"
            };
            if (cumulativeHazardIndices != null) {
                var atRiskDueTo = 0;
                var notAtRisk = numberOfCumulativeIndividualEffects - cumulativeHazardIndices.Count;
                var atRiskWithOrWithout = 0;
                (atRiskDueTo, notAtRisk, atRiskWithOrWithout) = CalculateHIAtRisks(
                    individualEffects,
                    cumulativeHazardIndices,
                    atRiskDueTo,
                    notAtRisk,
                    atRiskWithOrWithout
                );
                record.AtRiskDueToSubstance = 100d * atRiskDueTo / numberOfCumulativeIndividualEffects;
                record.NotAtRisk = 100d * notAtRisk / numberOfCumulativeIndividualEffects;
                record.AtRiskWithOrWithout = 100d * atRiskWithOrWithout / numberOfCumulativeIndividualEffects;
            } else {
                var atRiskDueToFood = individualEffects
                    .Count(c => c.HazardIndex(HealthEffectType) >= Threshold);

                record.AtRiskDueToSubstance = 100d * atRiskDueToFood / individualEffects.Count;
                record.AtRiskWithOrWithout = 0;
                record.NotAtRisk = 100 - record.AtRiskDueToSubstance;
            };
            return record;
        }
    }
}
