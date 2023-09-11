using MCRA.General;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.IndividualTargetExposureCalculation {
    public class IndividualTargetExposureCalculatorFactory {
        private IIndividualTargetExposureCalculatorFactorySettings _settings;
        public IndividualTargetExposureCalculatorFactory(IIndividualTargetExposureCalculatorFactorySettings settings) {
            _settings = settings;
        }

        /// <summary>
        /// Creates a target exposure calculator for the specified project.
        /// </summary>
        /// <returns></returns>
        public IndividualTargetExposureCalculatorBase Create() {
            switch (_settings.ExposureType) {
                case ExposureType.Acute:
                    return new AcuteIndividualTargetExposureCalculator();
                case ExposureType.Chronic:
                    return new ChronicIndividualTargetExposureCalculator();
                default:
                    throw new Exception($"No calculator found for exposure type {_settings.ExposureType}");
            }
        }
    }
}
