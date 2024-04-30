using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Calculators.OccurrenceFrequenciesCalculation {
    public sealed class OccurrenceFractionsCalculatorSettings : IOccurrenceFractionsCalculatorSettings {

        private readonly OccurrenceFrequenciesModuleConfig _configuration;
        public OccurrenceFractionsCalculatorSettings(OccurrenceFrequenciesModuleConfig config) {
            _configuration = config;
        }
        public bool SetMissingAgriculturalUseAsUnauthorized => _configuration.SetMissingAgriculturalUseAsUnauthorized;

        public bool UseAgriculturalUsePercentage => _configuration.UseAgriculturalUsePercentage;
    }
}
