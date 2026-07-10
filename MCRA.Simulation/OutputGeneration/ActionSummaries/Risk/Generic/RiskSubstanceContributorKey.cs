using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public class RiskSubstanceContributorKey : IExposureContributorKey {
        public Compound Substance { get; set; }

        public string GetKey() {
            return Substance.Code;
        }
    }
}
