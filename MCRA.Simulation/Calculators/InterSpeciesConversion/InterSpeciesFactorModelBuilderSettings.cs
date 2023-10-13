using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Calculators.InterSpeciesConversion {
    public sealed class InterSpeciesFactorModelBuilderSettings : IInterSpeciesFactorModelBuilderSettings {

        private readonly RisksSettings _effectModelSettingsDto;

        public InterSpeciesFactorModelBuilderSettings(RisksSettings effectModelSettingsDto) {
            _effectModelSettingsDto = effectModelSettingsDto;
        }
        public double DefaultInterSpeciesFactorGeometricMean => _effectModelSettingsDto.DefaultInterSpeciesFactorGeometricMean;

        public double DefaultInterSpeciesFactorGeometricStandardDeviation => _effectModelSettingsDto.DefaultInterSpeciesFactorGeometricStandardDeviation;
    }
}
