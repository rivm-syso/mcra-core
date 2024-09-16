using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.AbsorptionFactorsCollectionsGeneration {
    public sealed class AbsorptionFactorsCollectionBuilderSettings : IAbsorptionFactorsCollectionBuilderSettings {

        private readonly KineticModelsModuleConfig _configuration;

        public AbsorptionFactorsCollectionBuilderSettings(KineticModelsModuleConfig config) {
            _configuration = config;
        }

        public double DefaultFactorDietary => _configuration.OralAbsorptionFactorForDietaryExposure;

        public double DefaultFactorDermalNonDietary => _configuration.DermalAbsorptionFactor;

        public double DefaultFactorOralNonDietary => _configuration.OralAbsorptionFactor;

        public double DefaultFactorInhalationNonDietary => _configuration.InhalationAbsorptionFactor;
    }
}
