using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.KineticModelCalculation {
    public class IndividualSubstanceTargetExposure {
        public int SimulatedIndividualId { get; set; }
        public Individual Individual { get; set; }
        public double IndividualSamplingWeight { get; set; }
        public List<ISubstanceTargetExposure> SubstanceTargetExposures { get; set; }
    }
}
