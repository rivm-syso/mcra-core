using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Calculators.OccurrencePatternsCalculation {
    public sealed class OccurrencePatternsFromFindingsCalculatorSettings : IOccurrencePatternsFromFindingsCalculatorSettings {

        private readonly AgriculturalUseSettings _agriculturalUseSettings;

        public OccurrencePatternsFromFindingsCalculatorSettings() {
        }
        public OccurrencePatternsFromFindingsCalculatorSettings(AgriculturalUseSettings agriculturalUseSettings) {
            _agriculturalUseSettings = agriculturalUseSettings;
        }
        public bool Rescale => _agriculturalUseSettings?.ScaleUpOccurencePatterns ?? false;

        public bool OnlyScaleAuthorised => _agriculturalUseSettings != null 
            && _agriculturalUseSettings.ScaleUpOccurencePatterns 
            && _agriculturalUseSettings.RestrictOccurencePatternScalingToAuthorisedUses;
    }
}
