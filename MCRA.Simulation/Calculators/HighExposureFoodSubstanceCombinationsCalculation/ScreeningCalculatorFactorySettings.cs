using MCRA.General;
using MCRA.General.Action.Settings.Dto;

namespace MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinationsCalculation {
    public sealed class ScreeningCalculatorFactorySettings : IScreeningCalculatorFactorySettings {

        private readonly ScreeningSettingsDto _screeningSettings;
        private readonly AssessmentSettingsDto _assessmentSettings;
        public ScreeningCalculatorFactorySettings(
            ScreeningSettingsDto screeningSettings, 
            AssessmentSettingsDto assessmentSettings
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
