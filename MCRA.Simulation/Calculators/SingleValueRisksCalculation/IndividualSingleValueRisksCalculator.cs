using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.Calculators.SingleValueRisksCalculation {
    public sealed class IndividualSingleValueRisksCalculator {

        private IIndividualSingleValueRisksCalculatorSettings _settings;

        public IndividualSingleValueRisksCalculator(IIndividualSingleValueRisksCalculatorSettings settings) {
            _settings = settings;
        }

        public ICollection<SingleValueRiskCalculationResult> Compute(
                ICollection<IndividualEffect> cumulativeIndividualEffects
            ) {
            var result = new List<SingleValueRiskCalculationResult>();

            var marginOfExposures = cumulativeIndividualEffects.Select(c => c.MarginOfExposure(_settings.HealthEffectType)).ToList();
            var exposures = cumulativeIndividualEffects.Select(c => c.ExposureConcentration).ToList();
            var criticalEffects = cumulativeIndividualEffects.Select(c => c.CriticalEffectDose).ToList();
            var isHazardCharacterisationDistribution = criticalEffects.Distinct().Count() > 1;
            var hazardIndices = cumulativeIndividualEffects.Select(c => c.HazardIndex(_settings.HealthEffectType)).ToList();
            var weights = cumulativeIndividualEffects.Select(c => c.SamplingWeight).ToList();

            var hazardIndex = double.NaN;
            var marginOfExposure = double.NaN;
            var exposure = double.NaN;
            var criticalEffect = double.NaN;

            if (_settings.RiskMetricType == RiskMetricType.MarginOfExposure) {
                if (_settings.UseInverseDistribution) {
                    exposure = exposures.PercentilesWithSamplingWeights(weights, 100 - _settings.Percentage);
                    marginOfExposure = 1 / hazardIndices.PercentilesWithSamplingWeights(weights, 100 - _settings.Percentage);
                } else {
                    marginOfExposure = marginOfExposures.PercentilesWithSamplingWeights(weights, _settings.Percentage);
                    exposure = 1 / exposures.Select(c => 1 / c).PercentilesWithSamplingWeights(weights, _settings.Percentage);
                }
            } else {
                if (_settings.UseInverseDistribution) {
                    hazardIndex = 1 / marginOfExposures.PercentilesWithSamplingWeights(weights, 100 - _settings.Percentage);
                    exposure = 1 / exposures.Select(c => 1 / c).PercentilesWithSamplingWeights(weights, 100 - _settings.Percentage);

                } else {
                    hazardIndex = hazardIndices.PercentilesWithSamplingWeights(weights, _settings.Percentage);
                    exposure = exposures.PercentilesWithSamplingWeights(weights, _settings.Percentage);
                }

            }
            var singleValueRiskCalculationResult = new SingleValueRiskCalculationResult() {
                MarginOfExposure = marginOfExposure,
                HazardQuotient = hazardIndex,
                Exposure = exposure,
                HazardCharacterisation = isHazardCharacterisationDistribution ? criticalEffect : criticalEffects[0]
            };
            result.Add(singleValueRiskCalculationResult);
            return result;
        }


    }
}
