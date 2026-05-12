namespace MCRA.Simulation.Calculators.Stratification {
    /// <summary>
    /// Minimal interface for stratification levels.
    /// </summary>
    public interface IStratificationLevel {
        string Code { get; }
        string Name { get; }
    }
}
