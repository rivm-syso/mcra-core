using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Calculators.InterSpeciesConversion {
    public sealed class InterSpeciesFactorModelBuilderSettings : IInterSpeciesFactorModelBuilderSettings {

        private readonly EffectModelSettings _effectModelSettingsDto;

        public InterSpeciesFactorModelBuilderSettings(EffectModelSettings effectModelSettingsDto) {
            _effectModelSettingsDto = effectModelSettingsDto;
        }
        public double DefaultInterSpeciesFactorGeometricMean => _effectModelSettingsDto.DefaultInterSpeciesFactorGeometricMean;

        public double DefaultInterSpeciesFactorGeometricStandardDeviation => _effectModelSettingsDto.DefaultInterSpeciesFactorGeometricStandardDeviation;
    }
}
