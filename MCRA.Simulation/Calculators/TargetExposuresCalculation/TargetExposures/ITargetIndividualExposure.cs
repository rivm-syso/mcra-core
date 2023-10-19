using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public interface ITargetIndividualExposure : ITargetExposure {
        int SimulatedIndividualId { get; }
        double IndividualSamplingWeight { get; }
        double GetExposureForSubstance(Compound compound);
        ICollection<Compound> Substances { get; }
    }
}
