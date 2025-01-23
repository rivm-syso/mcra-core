namespace MCRA.General.ModuleDefinitions.Interfaces {
    public interface IFoodExtrapolationCandidatesCalculatorSettings {
        int ThresholdForExtrapolation { get; }
        bool ConsiderAuthorisationsForExtrapolations { get; }
        bool ConsiderMrlForExtrapolations { get; }

    }
}
