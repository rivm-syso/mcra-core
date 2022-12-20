using MCRA.General.Action.Settings.Dto;

namespace MCRA.Simulation.Calculators.OccurrenceFrequenciesCalculation {
    public sealed class OccurrenceFractionsCalculatorSettings : IOccurrenceFractionsCalculatorSettings {

        private readonly AgriculturalUseSettingsDto _agriculturalUseSettings;
        public OccurrenceFractionsCalculatorSettings(AgriculturalUseSettingsDto agriculturalUseSettings) {
            _agriculturalUseSettings = agriculturalUseSettings;
        }
        public bool SetMissingAgriculturalUseAsUnauthorized => _agriculturalUseSettings.SetMissingAgriculturalUseAsUnauthorized;

        public bool UseAgriculturalUsePercentage => _agriculturalUseSettings.UseAgriculturalUsePercentage;
    }
}
