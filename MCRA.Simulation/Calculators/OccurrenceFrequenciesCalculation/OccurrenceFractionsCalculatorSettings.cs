using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Calculators.OccurrenceFrequenciesCalculation {
    public sealed class OccurrenceFractionsCalculatorSettings : IOccurrenceFractionsCalculatorSettings {

        private readonly AgriculturalUseSettings _agriculturalUseSettings;
        public OccurrenceFractionsCalculatorSettings(AgriculturalUseSettings agriculturalUseSettings) {
            _agriculturalUseSettings = agriculturalUseSettings;
        }
        public bool SetMissingAgriculturalUseAsUnauthorized => _agriculturalUseSettings.SetMissingAgriculturalUseAsUnauthorized;

        public bool UseAgriculturalUsePercentage => _agriculturalUseSettings.UseAgriculturalUsePercentage;
    }
}
