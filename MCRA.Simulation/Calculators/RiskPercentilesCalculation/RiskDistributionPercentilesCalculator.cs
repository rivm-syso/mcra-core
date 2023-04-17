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

            var marginOfExposures = individualRisks
                .Select(c => c.MarginOfExposure)
                .ToList();
            var exposures = individualRisks
                .Select(c => c.ExposureConcentration)
                .ToList();
            var criticalEffects = individualRisks
                .Select(c => c.CriticalEffectDose)
                .ToList();
            var isHazardCharacterisationDistribution = criticalEffects.Distinct().Count() > 1;
            var hazardIndices = individualRisks
                .Select(c => c.HazardIndex)
                .ToList();
            var weights = individualRisks
                .Select(c => c.SamplingWeight)
                .ToList();

            var hazardIndex = double.NaN;
            var marginOfExposure = double.NaN;
            var exposure = double.NaN;
            var criticalEffect = double.NaN;

            foreach (var selectedPercentage in Percentages) {
                var percentage = selectedPercentage;
                if (RiskMetricType == RiskMetricType.MarginOfExposure) {
                    if (UseInverseDistribution) {
                        exposure = exposures.PercentilesWithSamplingWeights(weights, 100 - percentage);
                        marginOfExposure = 1 / hazardIndices.PercentilesWithSamplingWeights(weights, 100 - percentage);
                    } else {
                        marginOfExposure = marginOfExposures.PercentilesWithSamplingWeights(weights, percentage);
                        exposure = 1 / exposures.Select(c => 1 / c).PercentilesWithSamplingWeights(weights, percentage);
                    }
                } else {
                    if (UseInverseDistribution) {
                        hazardIndex = 1 / marginOfExposures.PercentilesWithSamplingWeights(weights, 100 - percentage);
                        exposure = 1 / exposures.Select(c => 1 / c).PercentilesWithSamplingWeights(weights, 100 - percentage);
                    } else {
                        hazardIndex = hazardIndices.PercentilesWithSamplingWeights(weights, percentage);
                        exposure = exposures.PercentilesWithSamplingWeights(weights, percentage);
                    }
                }

                var singleValueRiskCalculationResult = new RiskDistributionPercentileRecord() {
                    MarginOfExposure = marginOfExposure,
                    HazardQuotient = hazardIndex,
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
