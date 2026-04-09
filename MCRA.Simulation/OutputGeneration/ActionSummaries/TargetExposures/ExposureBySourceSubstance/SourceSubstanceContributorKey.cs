using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic {
    public class SourceSubstanceContributorKey : IExposureContributorKey {
        public ExposureSource Source { get; set; }
        public Compound Substance { get; set; }
    }
}
