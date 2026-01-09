using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.OccupationalScenarioExposureCalculation;
using MCRA.Simulation.Calculators.OccupationalTaskModelCalculation;

namespace MCRA.Simulation.Actions.OccupationalExposures {
    public class OccupationalExposuresActionResult : IActionResult {
        public ICollection<IOccupationalTaskExposureModel> OccupationalTaskExposureModels { get; set; }
        public ICollection<OccupationalScenarioExposure> OccupationalScenarioExposures { get; set; }
        public ICollection<OccupationalScenarioTaskExposure> OccupationalScenarioTaskExposures { get; set; }
        public ExternalExposureCollection ExternalIndividualExposureCollection { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
        public OccupationalExposureUnit ExternalSystemicExposureUnit { get; set; }
    }
}
