using System.Collections.Generic;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ExperimentalUnit {
        public string Code { get; set; }

        public Dictionary<Compound, double> Doses { get; set; }
        public Dictionary<Response, DoseResponseExperimentMeasurement> Responses { get; set; }
        public Dictionary<string, string> Covariates { get; set; }
        public Dictionary<string, string> DesignFactors { get; set; }
        public double? Times { get; set; }
    }
}
