using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Calculators.ModelledFoodsCalculation {
    public sealed class ModelledFoodsInfosCalculatorSettings : IModelledFoodsInfosCalculatorSettings {

        private readonly ConversionSettings _conversionSettings;
        public ModelledFoodsInfosCalculatorSettings(ConversionSettings conversionSettings) {
            _conversionSettings = conversionSettings;
        }
        public bool DeriveModelledFoodsFromSampleBasedConcentrations => _conversionSettings.DeriveModelledFoodsFromSampleBasedConcentrations;

        public bool DeriveModelledFoodsFromSingleValueConcentrations => _conversionSettings.DeriveModelledFoodsFromSingleValueConcentrations;

        public bool UseWorstCaseValues => _conversionSettings.UseWorstCaseValues;

        public bool FoodIncludeNonDetects => _conversionSettings.FoodIncludeNonDetects;

        public bool CompoundIncludeNonDetects => _conversionSettings.CompoundIncludeNonDetects;
    }
}
