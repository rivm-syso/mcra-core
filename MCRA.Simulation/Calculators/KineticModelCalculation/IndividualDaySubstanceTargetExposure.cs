using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.KineticModelCalculation {
    public class IndividualDaySubstanceTargetExposure {
        public int SimulatedIndividualDayId { get; set; }
        public List<ISubstanceTargetExposure> SubstanceTargetExposures { get; set; }
    }
}
