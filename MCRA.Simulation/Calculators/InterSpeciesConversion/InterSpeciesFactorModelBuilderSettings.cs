using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Calculators.InterSpeciesConversion {
    public sealed class InterSpeciesFactorModelBuilderSettings : IInterSpeciesFactorModelBuilderSettings {

        private readonly InterSpeciesConversionsModuleConfig _configuration;

        public InterSpeciesFactorModelBuilderSettings(InterSpeciesConversionsModuleConfig config) {
            _configuration = config;
        }
        public double DefaultInterSpeciesFactorGeometricMean => _configuration.DefaultInterSpeciesFactorGeometricMean;

        public double DefaultInterSpeciesFactorGeometricStandardDeviation => _configuration.DefaultInterSpeciesFactorGeometricStandardDeviation;
    }
}
