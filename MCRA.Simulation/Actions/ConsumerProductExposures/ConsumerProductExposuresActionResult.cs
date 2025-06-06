using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;

namespace MCRA.Simulation.Actions.ConsumerProductExposures {
    public class ConsumerProductExposuresActionResult : IActionResult {
        public ExposureUnitTriple ConsumerProductExposureUnit { get; set; }
        public ICollection<ConsumerProductIndividualDayIntake> ConsumerProductIndividualDayIntakes { get; set; }
        public ICollection<ConsumerProductIndividualDayExposure> ConsumerProductIndividualExposures { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
