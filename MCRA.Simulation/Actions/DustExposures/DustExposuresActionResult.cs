using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.DustExposureCalculation;

namespace MCRA.Simulation.Actions.DustExposures {
    public class DustExposuresActionResult : IActionResult {
        
        public ICollection<DustIndividualDayExposure> IndividualDustExposures { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
