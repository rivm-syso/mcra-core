namespace MCRA.Simulation.Calculators.FoodExtrapolationsCalculation {
    public interface IFoodExtrapolationCandidatesCalculatorSettings {
        int ThresholdForExtrapolation { get; }
        bool ConsiderAuthorisationsForExtrapolations { get; }
        bool ConsiderMrlForExtrapolations { get; }

    }
}
