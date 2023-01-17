
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public class TargetIndividualDayExposure : TargetIndividualExposure, ITargetIndividualDayExposure {
        public string Day { get; set; }
        public int SimulatedIndividualDayId { get; set; }
    }
}
