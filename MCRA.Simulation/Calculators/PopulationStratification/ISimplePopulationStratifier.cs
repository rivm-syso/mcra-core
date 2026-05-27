using MCRA.General;

namespace MCRA.Simulation.Calculators.Stratification {
    public interface ISimplePopulationStratifier : IPopulationStratifier {
        OutputStratificationVariable StratificationVariable { get; }
    }
}