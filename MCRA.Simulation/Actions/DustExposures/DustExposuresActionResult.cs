using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.DustExposureCalculation;

namespace MCRA.Simulation.Actions.DustExposures {
    public class DustExposuresActionResult : IActionResult {
        
        /*
        public ICollection<ExposureRoute> DustExposureRoutes { get; set; }       
        public IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> DustExposures { get; set; }
        public ICollection<NonDietaryExposureSet> DustExposureSets { get; set; }
        */

        public ICollection<DustIndividualDayExposure> IndividualDustExposures { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
