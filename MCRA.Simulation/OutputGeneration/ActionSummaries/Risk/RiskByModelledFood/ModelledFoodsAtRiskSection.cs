using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ModelledFoodsAtRiskSection : AtRiskSectionBase {
        public RiskMetricType RiskMetric { get; set; }
        public List<ModelledFoodAtRiskRecord> Records { get; set; }
        public override bool SaveTemporaryData => true;

        public void SummarizeModelledFoodsAtRisk(
            Dictionary<Food, List<IndividualEffect>> individualEffects,
            int numberOfCumulativeIndividualEffects,
            HealthEffectType healthEffectType,
            RiskMetricType riskMetric,
            double threshold
        ) {
            HealthEffectType = healthEffectType;
            Threshold = threshold;
            var cumulativeDict = individualEffects.SelectMany(v => v.Value)
                .GroupBy(v => v.SimulatedIndividualId)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.ExposureThresholdRatio));

            Records = new List<ModelledFoodAtRiskRecord>();
            var useCumulative = individualEffects.Count > 1;

            foreach (var kvp in individualEffects) {
                var record = (riskMetric == RiskMetricType.MarginOfExposure)
                    ? createFoodAtRiskRecord(
                        kvp.Value,
                        useCumulative ? cumulativeDict : null,
                        kvp.Key,
                        numberOfCumulativeIndividualEffects
                      )
                    : createFoodAtRiskHIRecord(
                        kvp.Value,
                        useCumulative ? cumulativeDict : null,
                        kvp.Key,
                        numberOfCumulativeIndividualEffects
                      );
                Records.Add(record);
            }
            Records = Records.OrderByDescending(c => c.AtRiskDueToFood).ThenBy(c => c.FoodName, StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>
        /// Calculates at risks for threshold value/exposure
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="cumulativeIndividualRisks"></param>
        /// <param name="food"></param>
        /// <param name="numberOfCumulativeIndividualEffects"></param>
        /// <returns></returns>
        private ModelledFoodAtRiskRecord createFoodAtRiskRecord(
            List<IndividualEffect> individualEffects,
            IDictionary<int, double> cumulativeIndividualRisks,
            Food food,
            int numberOfCumulativeIndividualEffects

        ) {
            var record = new ModelledFoodAtRiskRecord() {
                FoodName = food.Name,
                FoodCode = food.Code
            };
            if (cumulativeIndividualRisks != null) {
                var atRiskDueTo = 0;
                var notAtRisk = numberOfCumulativeIndividualEffects - cumulativeIndividualRisks.Count;
                var atRiskWithOrWithout = 0;
                (atRiskDueTo, notAtRisk, atRiskWithOrWithout) = CalculateThresholdExposureRatioAtRisks(
                    individualEffects,
                    cumulativeIndividualRisks,
                    atRiskDueTo,
                    notAtRisk,
                    atRiskWithOrWithout
                );
                record.AtRiskDueToFood = 100d * atRiskDueTo / numberOfCumulativeIndividualEffects;
                record.NotAtRisk = 100d * notAtRisk / numberOfCumulativeIndividualEffects;
                record.AtRiskWithOrWithout = 100d * atRiskWithOrWithout / numberOfCumulativeIndividualEffects;
            } else {
                var atRiskDueToFood = individualEffects
                   .Count(c => c.ThresholdExposureRatio <= Threshold);

                record.AtRiskDueToFood = 100d * atRiskDueToFood / numberOfCumulativeIndividualEffects;
                record.AtRiskWithOrWithout = 0;
                record.NotAtRisk = 100 - record.AtRiskDueToFood;
            };
            return record;
        }

        /// <summary>
        /// Calculates risk for exposure/threshold value
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="cumulativeIndividualRisks"></param>
        /// <param name="food"></param>
        /// <param name="numberOfCumulativeIndividualEffects"></param>
        /// <returns></returns>
        private ModelledFoodAtRiskRecord createFoodAtRiskHIRecord(
            List<IndividualEffect> individualEffects,
            IDictionary<int, double> cumulativeIndividualRisks,
            Food food,
            int numberOfCumulativeIndividualEffects
        ) {
            var record = new ModelledFoodAtRiskRecord() {
                FoodName = food.Name,
                FoodCode = food.Code
            };
            if (cumulativeIndividualRisks != null) {
                var atRiskDueTo = 0;
                var notAtRisk = numberOfCumulativeIndividualEffects - cumulativeIndividualRisks.Count;
                var atRiskWithOrWithout = 0;

                (atRiskDueTo, notAtRisk, atRiskWithOrWithout) = CalculateExposureThresholdRatioAtRisks(
                    individualEffects,
                    cumulativeIndividualRisks,
                    atRiskDueTo,
                    notAtRisk,
                    atRiskWithOrWithout
                );
                record.AtRiskWithOrWithout = 100d * atRiskWithOrWithout / numberOfCumulativeIndividualEffects;
                record.NotAtRisk = 100d * notAtRisk / numberOfCumulativeIndividualEffects;
                record.AtRiskDueToFood = 100d * atRiskDueTo / numberOfCumulativeIndividualEffects;
            } else {
                var atRiskDueToFood = individualEffects
                    .Count(c => c.ExposureThresholdRatio >= Threshold);

                record.AtRiskDueToFood = 100d * atRiskDueToFood / numberOfCumulativeIndividualEffects;
                record.AtRiskWithOrWithout = 0;
                record.NotAtRisk = 100 - record.AtRiskDueToFood;
            };
            return record;
        }
    }
}
