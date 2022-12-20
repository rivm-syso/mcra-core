using MCRA.General;
using MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinationsCalculation;

namespace MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinations {
    public sealed class ScreeningCalculatorFactory {

        private readonly IScreeningCalculatorFactorySettings _settings;
        private readonly bool _isPerPerson;

        public ScreeningCalculatorFactory(
            IScreeningCalculatorFactorySettings settings,
            bool isPerPerson
        ) {
            _settings = settings;
            _isPerPerson = isPerPerson;
        }
        public ScreeningCalculator Create() {
            if (_settings.ExposureType == ExposureType.Acute) {
                return new AcuteScreeningCalculator(
                    _settings.CriticalExposurePercentage, 
                    _settings.CumulativeSelectionPercentage, 
                    _settings.ImportanceLor, 
                    _isPerPerson
                );
            } else {
                return new ChronicScreeningCalculator(
                    _settings.CriticalExposurePercentage, 
                    _settings.CumulativeSelectionPercentage, 
                    _settings.ImportanceLor, 
                    _isPerPerson
                );
            }
        }
    }
}
