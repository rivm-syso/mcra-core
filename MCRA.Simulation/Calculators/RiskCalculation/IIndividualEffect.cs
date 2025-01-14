using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public interface IIndividualEffect {
        SimulatedIndividual SimulatedIndividual { get; }
        double IntraSpeciesDraw { get; }
    }
}
