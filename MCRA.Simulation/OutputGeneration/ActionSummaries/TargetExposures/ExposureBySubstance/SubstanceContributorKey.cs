using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic {
    public class SubstanceContributorKey : IExposureContributorKey {
        public Compound Substance { get; set; }
    }
}
