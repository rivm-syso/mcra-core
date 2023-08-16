using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ModelledFoodSubstancesAtRiskSection : AtRiskSectionBase {
        public RiskMetricType RiskMetric { get; set; }
        public List<ModelledFoodSubstanceAtRiskRecord> Records { get; set; }
        public override bool SaveTemporaryData => true;

        public void SummarizeModelledFoodSubstancesAtRisk(
                IDictionary<(Food Food, Compound Compound), List<IndividualEffect>> individualEffects,
                int numberOfCumulativeIndividualEffects,
                HealthEffectType healthEffectType,
                RiskMetricType riskMetric,
                double threshold
            ) {
            HealthEffectType = healthEffectType;
            Threshold = threshold;
            RiskMetric = riskMetric;

            var cumulativeDict = individualEffects.SelectMany(v => v.Value)
                .GroupBy(v => v.SimulatedIndividualId)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.ExposureThresholdRatio));

            Records = new List<ModelledFoodSubstanceAtRiskRecord>();
            var useCumulative = individualEffects.Count > 1;

            foreach (var kvp in individualEffects) {
                var record = (riskMetric == RiskMetricType.MarginOfExposure)
                    ? createModelledFoodSubstanceAtRiskMOERecord(
                        kvp.Value,
                        useCumulative ? cumulativeDict : null,
                        kvp.Key.Food,
                        kvp.Key.Compound,
                        numberOfCumulativeIndividualEffects
                      )
                    : createModelledFoodSubstanceAtRiskHIRecord(
                        kvp.Value,
                        useCumulative ? cumulativeDict : null,
                        kvp.Key.Food,
                        kvp.Key.Compound,
                        numberOfCumulativeIndividualEffects
                      );
                Records.Add(record);
            }
            Records = Records.OrderByDescending(c => c.AtRiskDueToModelledFoodSubstance)
                .ThenBy(c => c.SubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.SubstanceCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Calculates at risks for threshold value/exposure
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="cumulativeExposureThresholdRatios"></param>
        /// <param name="food"></param>
        /// <param name="substance"></param>
        /// <param name="numberOfCumulativeIndividualEffects"></param>
        /// <returns></returns>
        private ModelledFoodSubstanceAtRiskRecord createModelledFoodSubstanceAtRiskMOERecord(
            List<IndividualEffect> individualEffects,
            IDictionary<int, double> cumulativeExposureThresholdRatios,
            Food food,
            Compound substance,
            int numberOfCumulativeIndividualEffects
        ) {
            var record = new ModelledFoodSubstanceAtRiskRecord() {
                FoodName = food.Name,
                FoodCode = food.Code,
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code
            };
            if (cumulativeExposureThresholdRatios != null) {
                var maxRisk = CalculateThresholdExposureRatio(double.MaxValue, 0);
                var atRiskDueTo = 0;
                var notAtRisk = numberOfCumulativeIndividualEffects - cumulativeExposureThresholdRatios.Count;
                var atRiskWithOrWithout = 0;
                (atRiskDueTo, notAtRisk, atRiskWithOrWithout) = CalculateThresholdExposureRatioAtRisks(
                    individualEffects,
                    cumulativeExposureThresholdRatios,
                    atRiskDueTo,
                    notAtRisk,
                    atRiskWithOrWithout
                );
                record.AtRiskDueToModelledFoodSubstance = 100d * atRiskDueTo/ numberOfCumulativeIndividualEffects;
                record.NotAtRisk = 100d * notAtRisk / numberOfCumulativeIndividualEffects;
                record.AtRiskWithOrWithout = 100d * atRiskWithOrWithout / numberOfCumulativeIndividualEffects;
            } else {
                var atRiskDueTo = individualEffects
                   .Count(c => c.ThresholdExposureRatio <= Threshold);

                record.AtRiskDueToModelledFoodSubstance = 100d * atRiskDueTo / numberOfCumulativeIndividualEffects;
                record.AtRiskWithOrWithout = 0;
                record.NotAtRisk = 100 - record.AtRiskDueToModelledFoodSubstance;
            };
            return record;
        }

        /// <summary>
        ///Calculates at risks for risk.
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="cumulativeExposureThresholdRatios"></param>
        /// <param name="food"></param>
        /// <param name="substance"></param>
        /// <param name="numberOfCumulativeIndividualEffects"></param>
        /// <returns></returns>
        private ModelledFoodSubstanceAtRiskRecord createModelledFoodSubstanceAtRiskHIRecord(
            List<IndividualEffect> individualEffects,
            IDictionary<int, double> cumulativeExposureThresholdRatios,
            Food food,
            Compound substance,
            int numberOfCumulativeIndividualEffects
        ) {
            var record = new ModelledFoodSubstanceAtRiskRecord() {
                FoodName = food.Name,
                FoodCode = food.Code,
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code
            };

            if (cumulativeExposureThresholdRatios != null) {
                var atRiskDueTo = 0;
                var notAtRisk = numberOfCumulativeIndividualEffects - cumulativeExposureThresholdRatios.Count;    
                var atRiskWithOrWithout = 0;
                (atRiskDueTo, notAtRisk, atRiskWithOrWithout) = CalculateExposureThresholdRatioAtRisks(
                    individualEffects,
                    cumulativeExposureThresholdRatios,
                    atRiskDueTo,
                    notAtRisk,
                    atRiskWithOrWithout
                );
                
                record.AtRiskWithOrWithout = 100d * atRiskWithOrWithout / numberOfCumulativeIndividualEffects;
                record.NotAtRisk = 100d * notAtRisk / numberOfCumulativeIndividualEffects;
                record.AtRiskDueToModelledFoodSubstance = 100d * atRiskDueTo / numberOfCumulativeIndividualEffects;
            } else {
                var atRiskDueTo = individualEffects
                    .Count(c => c.ExposureThresholdRatio >= Threshold);

                record.AtRiskDueToModelledFoodSubstance = 100d * atRiskDueTo / numberOfCumulativeIndividualEffects;
                record.AtRiskWithOrWithout = 0;
                record.NotAtRisk = 100 - record.AtRiskDueToModelledFoodSubstance;
            };
            return record;
        }
    }
}
