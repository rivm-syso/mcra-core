using MCRA.General;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic {
    public class SourceRouteSubstanceContributorKey : IExposureContributorKey {
        public ExposureRoute Route { get; set; }
        public ExposureSource Source { get; set; }
        public string Substance { get; set; }
    }
}
