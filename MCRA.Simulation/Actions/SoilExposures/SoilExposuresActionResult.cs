using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.SoilExposureCalculation;

namespace MCRA.Simulation.Actions.SoilExposures {
    public class SoilExposuresActionResult : IActionResult {
        public ExposureUnitTriple SoilExposureUnit { get; set; }
        public ICollection<SoilIndividualExposure> IndividualSoilExposures { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
