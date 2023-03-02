namespace MCRA.Simulation.Calculators.RiskCalculation {
    public interface IIndividualEffect {
        int SimulatedIndividualId { get; }
        double SamplingWeight { get; }
        double IntraSpeciesDraw { get; }
    }
}
