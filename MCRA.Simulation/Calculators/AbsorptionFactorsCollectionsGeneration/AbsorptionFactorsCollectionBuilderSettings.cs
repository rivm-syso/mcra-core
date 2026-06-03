using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Calculators.KineticConversionCalculation.AbsorptionFactorsCollectionsGeneration {
    public sealed class AbsorptionFactorsCollectionBuilderSettings : IAbsorptionFactorsCollectionBuilderSettings {

        private readonly KineticModelsModuleConfig _configuration;

        public AbsorptionFactorsCollectionBuilderSettings(KineticModelsModuleConfig config) {
            _configuration = config;
        }

        public double DefaultFactorOral => _configuration.OralAbsorptionFactor;

        public double DefaultFactorDermal => _configuration.DermalAbsorptionFactor;

        public double DefaultFactorInhalation => _configuration.InhalationAbsorptionFactor;
    }
}
