namespace MCRA.Simulation.Calculators.OccurrencePatternsCalculation {
    public interface IOccurrencePatternsFromFindingsCalculatorSettings {
        bool Rescale { get; }
        bool OnlyScaleAuthorised { get; }
        bool IsOnlyScaleAuthorised { get; }
    }
}
