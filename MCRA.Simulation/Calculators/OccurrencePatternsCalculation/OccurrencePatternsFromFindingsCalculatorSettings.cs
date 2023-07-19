using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Calculators.OccurrencePatternsCalculation {
    public sealed class OccurrencePatternsFromFindingsCalculatorSettings : IOccurrencePatternsFromFindingsCalculatorSettings {

        private readonly AgriculturalUseSettingsDto _agriculturalUseSettings;

        public OccurrencePatternsFromFindingsCalculatorSettings() {
        }
        public OccurrencePatternsFromFindingsCalculatorSettings(AgriculturalUseSettingsDto agriculturalUseSettings) {
            _agriculturalUseSettings = agriculturalUseSettings;
        }
        public bool Rescale => _agriculturalUseSettings?.ScaleUpOccurencePatterns ?? false;

        public bool OnlyScaleAuthorised => _agriculturalUseSettings != null 
            && _agriculturalUseSettings.ScaleUpOccurencePatterns 
            && _agriculturalUseSettings.RestrictOccurencePatternScalingToAuthorisedUses;
    }
}
