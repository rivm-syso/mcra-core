using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Calculators.ModelledFoodsCalculation {
    public sealed class ModelledFoodsInfosCalculatorSettings : IModelledFoodsInfosCalculatorSettings {

        private readonly ModelledFoodsModuleConfig _configuration;
        public ModelledFoodsInfosCalculatorSettings(ModelledFoodsModuleConfig config) {
            _configuration = config;
        }
        public bool DeriveModelledFoodsFromSampleBasedConcentrations => _configuration.DeriveModelledFoodsFromSampleBasedConcentrations;

        public bool DeriveModelledFoodsFromSingleValueConcentrations => _configuration.DeriveModelledFoodsFromSingleValueConcentrations;

        public bool UseWorstCaseValues => _configuration.UseWorstCaseValues;

        public bool FoodIncludeNonDetects => _configuration.FoodIncludeNonDetects;

        public bool CompoundIncludeNonDetects => _configuration.CompoundIncludeNonDetects;
    }
}
