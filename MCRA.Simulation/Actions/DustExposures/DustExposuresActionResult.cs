﻿using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DustExposureCalculation;

namespace MCRA.Simulation.Actions.DustExposures {
    public class DustExposuresActionResult : IActionResult {
        
        /*
        public ICollection<ExposureRoute> DustExposureRoutes { get; set; }       
        public IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> DustExposures { get; set; }
        public ICollection<NonDietaryExposureSet> DustExposureSets { get; set; }
        */

        public ICollection<IndividualDustExposureRecord> IndividualDustExposures { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}