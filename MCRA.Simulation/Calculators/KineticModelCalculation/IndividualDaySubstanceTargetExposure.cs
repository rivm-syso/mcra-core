using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.KineticModelCalculation {
    public class IndividualDaySubstanceTargetExposure {
        public int SimulatedIndividualDayId { get; set; }
        public ISubstanceTargetExposure SubstanceTargetExposure { get; set; }
    }
}
