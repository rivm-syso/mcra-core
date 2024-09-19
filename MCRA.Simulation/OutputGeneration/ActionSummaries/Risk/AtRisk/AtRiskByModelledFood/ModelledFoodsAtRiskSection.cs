using System.Collections.Concurrent;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ModelledFoodsAtRiskSection : AtRiskSectionBase {
        public RiskMetricType RiskMetric { get; set; }
        public List<ModelledFoodAtRiskRecord> Records { get; set; }
        public override bool SaveTemporaryData => true;

        public void SummarizeModelledFoodsAtRisk(
            IDictionary<Food, List<IndividualEffect>> individualEffects,
            int numberOfCumulativeIndividualEffects,
            HealthEffectType healthEffectType,
            RiskMetricType riskMetric,
            double threshold
        ) {
            HealthEffectType = healthEffectType;
            Threshold = threshold;

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
            var recordsBag = new ConcurrentBag<ModelledFoodAtRiskRecord>();

            Parallel.ForEach(individualEffects, kvp => {
                var record = createFoodAtRiskRecord(
                    riskMetric,
                    kvp.Key,
                    kvp.Value,
                    cumulativeDict,
                    numberOfCumulativeIndividualEffects
                );
                recordsBag.Add(record);
            });

            Records = recordsBag.OrderByDescending(c => c.AtRiskDueToFood)
                .ThenBy(c => c.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Calculates at risks for modelled food.
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="cumulativeIndividualRisks"></param>
        /// <param name="food"></param>
        /// <param name="numberOfCumulativeIndividualEffects"></param>
        /// <returns></returns>
        private ModelledFoodAtRiskRecord createFoodAtRiskRecord(
            RiskMetricType metricType,
            Food food,
            List<IndividualEffect> individualEffects,
            IDictionary<int, double> cumulativeIndividualRisks,
            int numberOfCumulativeIndividualEffects
        ) {
            var useMarginOfExposure = metricType == RiskMetricType.HazardExposureRatio;

            var record = new ModelledFoodAtRiskRecord() {
                FoodName = food.Name,
                FoodCode = food.Code
            };
            if (cumulativeIndividualRisks != null) {
                var atRiskDueTo = 0;
                var notAtRisk = numberOfCumulativeIndividualEffects - cumulativeIndividualRisks.Count;
                var atRiskWithOrWithout = 0;
                (atRiskDueTo, notAtRisk, atRiskWithOrWithout) = useMarginOfExposure
                    ? CalculateHazardExposureRatioAtRisks(individualEffects, cumulativeIndividualRisks, atRiskDueTo, notAtRisk, atRiskWithOrWithout)
                    : CalculateExposureHazardRatioAtRisks(individualEffects, cumulativeIndividualRisks, atRiskDueTo, notAtRisk, atRiskWithOrWithout);

                record.AtRiskDueToFood = 100d * atRiskDueTo / numberOfCumulativeIndividualEffects;
                record.NotAtRisk = 100d * notAtRisk / numberOfCumulativeIndividualEffects;
                record.AtRiskWithOrWithout = 100d * atRiskWithOrWithout / numberOfCumulativeIndividualEffects;
            } else {
                var atRiskDueToFood = useMarginOfExposure
                    ? individualEffects.Count(c => c.HazardExposureRatio <= Threshold)
                    : individualEffects.Count(c => c.ExposureHazardRatio >= Threshold);

                record.AtRiskDueToFood = 100d * atRiskDueToFood / numberOfCumulativeIndividualEffects;
                record.AtRiskWithOrWithout = 0;
                record.NotAtRisk = 100 - record.AtRiskDueToFood;
            };
            return record;
        }
    }
}
