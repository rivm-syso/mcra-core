namespace MCRA.Simulation.Calculators.RiskCalculation {
    public interface IIndividualEffect {
        int SimulationId { get; }
        double SamplingWeight { get; }
        double IntraSpeciesDraw { get; }
    }
}
