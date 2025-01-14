using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Constants;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class AtRiskSectionBase : SummarySection {

        public HealthEffectType HealthEffectType;
        public double Threshold;
        public double[] _riskPercentages;
        public bool _isInverseDistribution;

        /// </summary>
        /// Calculate percentages at risk. For single foods no background is available, calculate only foreground at risk.
        /// Note, threshold is riskmetric dependent.
        /// The following is calculated for a Risk (MOE, HI) with threshold = 1. Suppose three days with MOE's are available.
        /// For MOE: Background Background+Food  Background Background+Food
        ///                                      MOE <= threshold = at risk
        ///                 1.2             0.9           0               1     At risk due to food (%)
        ///                 1.3             1.1           0               0     Not at risk (%)
        ///                 0.8             0.5           1               1     At risk with or without food (%)
        /// For HI:  Background Background+Food  Background Background+Food
        ///                                       HI >= threshold = at risk
        ///                 0.8             1.1           0               1     At risk due to food (%)
        ///                 0.8             0.9           0               0     Not at risk (%)
        ///                 1.3             2.0           1               1     At risk with or without food (%)
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="cumulativeIndividualRisks"></param>
        /// <param name="atRiskDueTo"></param>
        /// <param name="notAtRisk"></param>
        /// <param name="atRiskWithOrWithout"></param>
        /// <returns></returns>
        public (int atRiskDueTo, int notAtRisk, int atRiskWithOrWithout) CalculateHazardExposureRatioAtRisks(
            List<IndividualEffect> individualEffects,
            IDictionary<int, double> cumulativeIndividualRisks,
            int atRiskDueTo,
            int notAtRisk,
            int atRiskWithOrWithout
        ) {
            var maxRisk = calculateHazardExposureRatio(double.MaxValue, 0, HealthEffectType.Risk);
            var risksDict = individualEffects
                .ToDictionary(
                    r => r.SimulatedIndividualId,
                    r => r.ExposureHazardRatio
                );

            foreach (var kvp in cumulativeIndividualRisks) {
                var cumulativeETR = kvp.Value;
                var cumulativeTER = 1 / kvp.Value;
                //not at risk:
                if (cumulativeTER > Threshold) {
                    notAtRisk++;
                }

                var background = 0D;
                if (risksDict.TryGetValue(kvp.Key, out var individualHi)) {
                    if (cumulativeETR - individualHi == 0) {
                        background = maxRisk;
                    } else {
                        background = 1 / (cumulativeETR - individualHi);
                    }
                } else {
                    background = cumulativeTER;
                }

                if (cumulativeTER <= Threshold) {
                    if (background > Threshold) {
                        atRiskDueTo++;
                    } else {
                        atRiskWithOrWithout++;
                    }
                }
            }
            return (atRiskDueTo, notAtRisk, atRiskWithOrWithout);
        }

        /// <summary>
        /// Calculate percentages at risk. For single foods no background is available, calculate only foreground at risk.
        /// Note, threshold is riskmetric dependent.
        /// The following is calculated for a MOE, HI with threshold = 1. Suppose three days with MOE's are available.
        /// For MOE: Background Background+Food  Background Background+Food
        ///                                      MOE <= threshold = at risk
        ///                 1.2             0.9           0               1     At risk due to food (%)
        ///                 1.3             1.1           0               0     Not at risk (%)
        ///                 0.8             0.5           1               1     At risk with or without food (%)
        /// For HI:  Background Background+Food  Background Background+Food
        ///                                       HI >= threshold = at risk
        ///                 0.8             1.1           0               1     At risk due to food (%)
        ///                 0.8             0.9           0               0     Not at risk (%)
        ///                 1.3             2.0           1               1     At risk with or without food (%)
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="cumulativeIndividualRisks"></param>
        /// <param name="atRiskDueTo"></param>
        /// <param name="notAtRisk"></param>
        /// <param name="atRiskWithOrWithout"></param>
        /// <returns></returns>
        public (int atRiskDueTo, int notAtRisk, int atRiskWithOrWithout) CalculateExposureHazardRatioAtRisks(
            List<IndividualEffect> individualEffects,
            IDictionary<int, double> cumulativeIndividualRisks,
            int atRiskDueTo,
            int notAtRisk,
            int atRiskWithOrWithout
        ) {
            var riskDict = individualEffects
                .ToDictionary(
                    r => r.SimulatedIndividualId,
                    r => r.ExposureHazardRatio
                );

            foreach (var kvp in cumulativeIndividualRisks) {
                var cumulativeRisk = kvp.Value;
                //not at risk:
                if (cumulativeRisk < Threshold) {
                    notAtRisk++;
                }

                var background = 0D;
                if (riskDict.TryGetValue(kvp.Key, out var individualHi)) {
                    background = cumulativeRisk - individualHi;
                } else {
                    background = cumulativeRisk;
                }

                if (cumulativeRisk >= Threshold) {
                    if (background >= Threshold) {
                        atRiskWithOrWithout++;
                    } else {
                        atRiskDueTo++;
                    }
                }
            }
            return (atRiskDueTo, notAtRisk, atRiskWithOrWithout);
        }

        private double calculateHazardExposureRatio(double iced, double iexp, HealthEffectType healthEffectType) {
            if (healthEffectType == HealthEffectType.Benefit) {
                return iced > iexp / SimulationConstants.MOE_eps ? iexp / iced : SimulationConstants.MOE_eps;
            } else {
                return iexp > iced / SimulationConstants.MOE_eps ? iced / iexp : SimulationConstants.MOE_eps;
            }
        }

        public (List<double>, List<double>, List<double>, List<double>, double, double) CalculateHazardExposurePercentiles(
            List<IndividualEffect> individualEffects
        ) {
            var allWeights = individualEffects
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var sumSamplingWeights = allWeights.Sum();
            var weights = individualEffects
                .Where(c => c.IsPositive)
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var samplingWeightsZeros = sumSamplingWeights - weights.Sum();

            var percentilesAll = new List<double>();
            var percentiles = new List<double>();
            var total = CalculateExposureHazardWeightedTotal(individualEffects);
            //TODO HazardExposure ratios are never infinity, should be fixed later
            if (_isInverseDistribution) {
                var complementPercentages = _riskPercentages.Select(c => 100 - c);
                var risksAll = individualEffects
                    .Select(c => c.ExposureHazardRatio);
                percentilesAll = risksAll.PercentilesWithSamplingWeights(allWeights, complementPercentages)
                    .Select(c => c == 0 ? SimulationConstants.MOE_eps : 1 / c)
                    .ToList();
                var risks = individualEffects
                    .Where(c => c.IsPositive)
                    .Select(c => c.ExposureHazardRatio);
                percentiles = risks.PercentilesWithSamplingWeights(weights, complementPercentages)
                    .Select(c => c == 0 ? SimulationConstants.MOE_eps : 1 / c)
                    .ToList();
            } else {
                percentilesAll = individualEffects
                    .Select(c => c.HazardExposureRatio)
                    .PercentilesWithSamplingWeights(allWeights, _riskPercentages)
                    .ToList();
                percentiles = individualEffects
                    .Where(c => c.IsPositive)
                    .Select(c => c.HazardExposureRatio)
                    .PercentilesWithSamplingWeights(weights, _riskPercentages)
                    .ToList();
            }
            return (percentiles, percentilesAll, weights, allWeights, total, sumSamplingWeights);
        }

        public (List<double>, List<double>, List<double>, List<double>, double, double) CalculateExposureHazardPercentiles(
            List<IndividualEffect> individualEffects
        ) {
            var allWeights = individualEffects
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var sumSamplingWeights = allWeights.Sum();
            var weights = individualEffects
                .Where(c => c.IsPositive)
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var samplingWeightsZeros = sumSamplingWeights - weights.Sum();

            var percentilesAll = new List<double>();
            var percentiles = new List<double>();
            var total = CalculateExposureHazardWeightedTotal(individualEffects);
            if (_isInverseDistribution) {
                var complementPercentages = _riskPercentages.Select(c => 100 - c).ToList();
                var risksAll = individualEffects
                    .Select(c => c.HazardExposureRatio);
                percentilesAll = risksAll.PercentilesWithSamplingWeights(allWeights, complementPercentages)
                    .Select(c => 1 / c)
                    .ToList();
                var risks = individualEffects
                    .Where(c => c.IsPositive)
                    .Select(c => c.HazardExposureRatio);
                percentiles = risks.PercentilesWithSamplingWeights(weights, complementPercentages)
                    .Select(c => 1 / c)
                    .ToList();
            } else {
                percentilesAll = individualEffects
                    .Select(c => c.ExposureHazardRatio)
                    .PercentilesWithSamplingWeights(allWeights, _riskPercentages)
                    .ToList();
                percentiles = individualEffects
                    .Where(c => c.IsPositive)
                    .Select(c => c.ExposureHazardRatio)
                    .PercentilesWithSamplingWeights(weights, _riskPercentages)
                    .ToList();
            }
            return (percentiles, percentilesAll, weights, allWeights, total, sumSamplingWeights);
        }

        /// <summary>
        /// Calculates sum of risks EH
        /// </summary>
        /// <param name="allIndividualEffects"></param>
        /// <returns></returns>
        public double CalculateExposureHazardWeightedTotal(List<IndividualEffect> allIndividualEffects) {
            var result = allIndividualEffects.Sum(c => c.ExposureHazardRatio * c.SimulatedIndividual.SamplingWeight);
            return result;
        }
    }
}
