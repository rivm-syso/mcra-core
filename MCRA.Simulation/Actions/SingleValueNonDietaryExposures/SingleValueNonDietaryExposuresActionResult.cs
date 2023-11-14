using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.SingleValueInternalExposuresCalculation;

namespace MCRA.Simulation.Actions.SingleValueNonDietaryExposures {
    public class SingleValueNonDietaryExposuresActionResult : IActionResult {
        public TargetUnit ExposureUnit { get; set; }
        public ICollection<ISingleValueNonDietaryExposure> Exposures { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
