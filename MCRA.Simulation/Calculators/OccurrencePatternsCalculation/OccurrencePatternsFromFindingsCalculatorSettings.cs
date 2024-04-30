using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Calculators.OccurrencePatternsCalculation {
    public sealed class OccurrencePatternsFromFindingsCalculatorSettings : IOccurrencePatternsFromFindingsCalculatorSettings {

        private readonly OccurrencePatternsModuleConfig _configuration;

        public OccurrencePatternsFromFindingsCalculatorSettings() {
        }
        public OccurrencePatternsFromFindingsCalculatorSettings(OccurrencePatternsModuleConfig config) {
            _configuration = config;
        }
        public bool Rescale => _configuration?.ScaleUpOccurencePatterns ?? false;

        public bool OnlyScaleAuthorised => _configuration != null
            && _configuration.ScaleUpOccurencePatterns
            && _configuration.RestrictOccurencePatternScalingToAuthorisedUses;
    }
}
