using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinationsCalculation {
    public sealed class ScreeningCalculatorFactorySettings : IScreeningCalculatorFactorySettings {

        private readonly ScreeningSettings _screeningSettings;
        private readonly AssessmentSettings _assessmentSettings;
        public ScreeningCalculatorFactorySettings(
            ScreeningSettings screeningSettings,
            AssessmentSettings assessmentSettings
        ) {
            _screeningSettings = screeningSettings;
            _assessmentSettings = assessmentSettings;
        }
        public double CriticalExposurePercentage => _screeningSettings.CriticalExposurePercentage;

        public double CumulativeSelectionPercentage => _screeningSettings.CumulativeSelectionPercentage;

        public double ImportanceLor => _screeningSettings.ImportanceLor;

        public ExposureType ExposureType => _assessmentSettings.ExposureType;
    }
}
