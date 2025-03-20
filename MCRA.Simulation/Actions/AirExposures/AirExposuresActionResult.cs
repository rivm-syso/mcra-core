using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.AirExposureCalculation;

namespace MCRA.Simulation.Actions.AirExposures {
    public class AirExposuresActionResult : IActionResult {
        public ExposureUnitTriple AirExposureUnit { get; set; }
        public ICollection<AirIndividualDayExposure> IndividualAirExposures { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
