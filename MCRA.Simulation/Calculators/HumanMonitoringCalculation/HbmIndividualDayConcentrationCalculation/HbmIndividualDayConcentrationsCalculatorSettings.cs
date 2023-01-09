using MCRA.General;
using MCRA.General.Action.Settings.Dto;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmIndividualDayConcentrationsCalculatorSettings : IHbmIndividualDayConcentrationsCalculatorSettings {

        private readonly HumanMonitoringSettingsDto _humanMonitoringSettings;
        public HbmIndividualDayConcentrationsCalculatorSettings(HumanMonitoringSettingsDto humanMonitoringSettings) {
            _humanMonitoringSettings = humanMonitoringSettings;
            _ = humanMonitoringSettings.NonDetectsHandlingMethod;
        }
        public NonDetectsHandlingMethod NonDetectsHandlingMethod => _humanMonitoringSettings.NonDetectsHandlingMethod;

        public double LorReplacementFactor => _humanMonitoringSettings.FractionOfLor;

        public MissingValueImputationMethod MissingValueImputationMethod => _humanMonitoringSettings.MissingValueImputationMethod;

        public NonDetectImputationMethod NonDetectImputationMethod => _humanMonitoringSettings.NonDetectImputationMethod;
    }
}
