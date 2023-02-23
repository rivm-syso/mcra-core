using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation;

namespace MCRA.Simulation.Actions.SingleValueDietaryExposures {
    public class SingleValueDietaryExposuresActionResult : IActionResult {
        public TargetUnit ExposureUnit { get; set; }
        public ICollection<ISingleValueDietaryExposure> Exposures { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
