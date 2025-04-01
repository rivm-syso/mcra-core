using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public interface IIndividualEffect {
        SimulatedIndividual SimulatedIndividual { get; }
        double IntraSpeciesDraw { get; }
    }
}
