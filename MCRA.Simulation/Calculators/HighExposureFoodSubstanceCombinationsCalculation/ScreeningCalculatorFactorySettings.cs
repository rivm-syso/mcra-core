using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinationsCalculation {
    public sealed class ScreeningCalculatorFactorySettings : IScreeningCalculatorFactorySettings {
        private readonly HighExposureFoodSubstanceCombinationsModuleConfig _configuration;

        public ScreeningCalculatorFactorySettings(HighExposureFoodSubstanceCombinationsModuleConfig config) {
            _configuration = config;
        }
        public double CriticalExposurePercentage => _configuration.CriticalExposurePercentage;

        public double CumulativeSelectionPercentage => _configuration.CumulativeSelectionPercentage;

        public double ImportanceLor => _configuration.ImportanceLor;

        public ExposureType ExposureType => _configuration.ExposureType;
    }
}
