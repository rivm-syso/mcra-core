using MCRA.General;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic {
    public class SourceSubstanceContributorKey : IExposureContributorKey {
        public ExposureSource Source { get; set; }
        public string Substance { get; set; }
    }
}
