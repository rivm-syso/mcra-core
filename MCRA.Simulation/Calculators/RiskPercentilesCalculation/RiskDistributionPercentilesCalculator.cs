using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.SingleValueRisksCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.RiskPercentilesCalculation {
    public sealed class RiskDistributionPercentilesCalculator {

        public HealthEffectType HealthEffectType { get; private set; }
        public RiskMetricType RiskMetricType { get; private set; }
        public double[] Percentages { get; private set; }
        public bool UseInverseDistribution { get; private set; }

        public RiskDistributionPercentilesCalculator(IIndividualSingleValueRisksCalculatorSettings settings) {
            HealthEffectType = settings.HealthEffectType;
            RiskMetricType = settings.RiskMetricType;
            Percentages = new double[] { settings.Percentage };
            UseInverseDistribution = settings.UseInverseDistribution;
        }

        public RiskDistributionPercentilesCalculator(
            HealthEffectType healthEffectType,
            RiskMetricType riskMetricType,
            double[] percentages,
            bool useInverseDistribution
        ) {
            HealthEffectType = healthEffectType;
            RiskMetricType = riskMetricType;
            Percentages = percentages;
            UseInverseDistribution = useInverseDistribution;
        }

        public ICollection<RiskDistributionPercentileRecord> Compute(
            ICollection<IndividualEffect> individualRisks
        ) {
            var result = new List<RiskDistributionPercentileRecord>();

            var thresholdExposuresRatios = individualRisks
                .Select(c => c.ThresholdExposureRatio)
                .ToList();
            var exposures = individualRisks
                .Select(c => c.ExposureConcentration)
                .ToList();
            var criticalEffects = individualRisks
                .Select(c => c.CriticalEffectDose)
                .ToList();
            var isHazardCharacterisationDistribution = criticalEffects.Distinct().Count() > 1;
            var exposureHazardRatios = individualRisks
                .Select(c => c.ExposureThresholdRatio)
                .ToList();
            var weights = individualRisks
                .Select(c => c.SamplingWeight)
                .ToList();

            var exposureHazardRatio = double.NaN;
            var risk = double.NaN;
            var exposure = double.NaN;
            var criticalEffect = double.NaN;

            foreach (var selectedPercentage in Percentages) {
                var percentage = selectedPercentage;
                if (RiskMetricType == RiskMetricType.MarginOfExposure) {
                    if (UseInverseDistribution) {
                        exposure = exposures.PercentilesWithSamplingWeights(weights, 100 - percentage);
                        risk = 1 / exposureHazardRatios.PercentilesWithSamplingWeights(weights, 100 - percentage);
                    } else {
                        risk = thresholdExposuresRatios.PercentilesWithSamplingWeights(weights, percentage);
                        exposure = 1 / exposures.Select(c => 1 / c).PercentilesWithSamplingWeights(weights, percentage);
                    }
                } else {
                    if (UseInverseDistribution) {
                        exposureHazardRatio = 1 / thresholdExposuresRatios.PercentilesWithSamplingWeights(weights, 100 - percentage);
                        exposure = 1 / exposures.Select(c => 1 / c).PercentilesWithSamplingWeights(weights, 100 - percentage);
                    } else {
                        exposureHazardRatio = exposureHazardRatios.PercentilesWithSamplingWeights(weights, percentage);
                        exposure = exposures.PercentilesWithSamplingWeights(weights, percentage);
                    }
                }

                var singleValueRiskCalculationResult = new RiskDistributionPercentileRecord() {
                    ThresholdExposureRatio = risk,
                    HazardQuotient = exposureHazardRatio,
                    Exposure = exposure,
                    Percentage = percentage,
                    HazardCharacterisation = isHazardCharacterisationDistribution
                        ? criticalEffect
                        : criticalEffects[0]
                };
                result.Add(singleValueRiskCalculationResult);
            }

            return result;
        }
    }
}
