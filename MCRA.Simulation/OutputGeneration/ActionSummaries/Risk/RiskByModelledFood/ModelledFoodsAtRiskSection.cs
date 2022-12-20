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
                .GroupBy(v => v.SimulationId)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.HazardIndex(HealthEffectType)));

            Records = new List<ModelledFoodAtRiskRecord>();
            var useCumulative = individualEffects.Count > 1;

            foreach (var kvp in individualEffects) {
                var record = (riskMetric == RiskMetricType.MarginOfExposure)
                    ? createFoodAtRiskMOERecord(
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
        /// Calculates at risks for margin of exposure
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="cumulativeIndividualHazardIndices"></param>
        /// <param name="food"></param>
        /// <param name="numberOfCumulativeIndividualEffects"></param>
        /// <returns></returns>
        private ModelledFoodAtRiskRecord createFoodAtRiskMOERecord(
            List<IndividualEffect> individualEffects,
            IDictionary<int, double> cumulativeIndividualHazardIndices,
            Food food,
            int numberOfCumulativeIndividualEffects

        ) {
            var record = new ModelledFoodAtRiskRecord() {
                FoodName = food.Name,
                FoodCode = food.Code
            };
            if (cumulativeIndividualHazardIndices != null) {
                var atRiskDueTo = 0;
                var notAtRisk = numberOfCumulativeIndividualEffects - cumulativeIndividualHazardIndices.Count;
                var atRiskWithOrWithout = 0;
                (atRiskDueTo, notAtRisk, atRiskWithOrWithout) = CalculateMOEAtRisks(
                    individualEffects,
                    cumulativeIndividualHazardIndices,
                    atRiskDueTo,
                    notAtRisk,
                    atRiskWithOrWithout
                );
                record.AtRiskDueToFood = 100d * atRiskDueTo / numberOfCumulativeIndividualEffects;
                record.NotAtRisk = 100d * notAtRisk / numberOfCumulativeIndividualEffects;
                record.AtRiskWithOrWithout = 100d * atRiskWithOrWithout / numberOfCumulativeIndividualEffects;
            } else {
                var atRiskDueToFood = individualEffects
                   .Count(c => c.MarginOfExposure(HealthEffectType) <= Threshold);

                record.AtRiskDueToFood = 100d * atRiskDueToFood / numberOfCumulativeIndividualEffects;
                record.AtRiskWithOrWithout = 0;
                record.NotAtRisk = 100 - record.AtRiskDueToFood;
            };
            return record;
        }

        /// <summary>
        /// Calculates risk for hazard indices
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="cumulativeIndividualHazardIndices"></param>
        /// <param name="food"></param>
        /// <param name="numberOfCumulativeIndividualEffects"></param>
        /// <returns></returns>
        private ModelledFoodAtRiskRecord createFoodAtRiskHIRecord(
            List<IndividualEffect> individualEffects,
            IDictionary<int, double> cumulativeIndividualHazardIndices,
            Food food,
            int numberOfCumulativeIndividualEffects
        ) {
            var record = new ModelledFoodAtRiskRecord() {
                FoodName = food.Name,
                FoodCode = food.Code
            };
            if (cumulativeIndividualHazardIndices != null) {
                var atRiskDueTo = 0;
                var notAtRisk = numberOfCumulativeIndividualEffects - cumulativeIndividualHazardIndices.Count;
                var atRiskWithOrWithout = 0;

                (atRiskDueTo, notAtRisk, atRiskWithOrWithout) = CalculateHIAtRisks(
                    individualEffects,
                    cumulativeIndividualHazardIndices,
                    atRiskDueTo,
                    notAtRisk,
                    atRiskWithOrWithout
                );
                record.AtRiskWithOrWithout = 100d * atRiskWithOrWithout / numberOfCumulativeIndividualEffects;
                record.NotAtRisk = 100d * notAtRisk / numberOfCumulativeIndividualEffects;
                record.AtRiskDueToFood = 100d * atRiskDueTo / numberOfCumulativeIndividualEffects;
            } else {
                var atRiskDueToFood = individualEffects
                    .Count(c => c.HazardIndex(HealthEffectType) >= Threshold);

                record.AtRiskDueToFood = 100d * atRiskDueToFood / numberOfCumulativeIndividualEffects;
                record.AtRiskWithOrWithout = 0;
                record.NotAtRisk = 100 - record.AtRiskDueToFood;
            };
            return record;
        }
    }
}
