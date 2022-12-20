namespace MCRA.Simulation.Calculators.ModelledFoodsCalculation {
    public interface IModelledFoodsInfosCalculatorSettings {
        bool DeriveModelledFoodsFromSampleBasedConcentrations { get; }
        bool DeriveModelledFoodsFromSingleValueConcentrations { get; }
        bool UseWorstCaseValues { get; }
        bool FoodIncludeNonDetects { get; }
        bool CompoundIncludeNonDetects { get; }
    }
}
