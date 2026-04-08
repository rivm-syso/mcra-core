using MCRA.General;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic {
    public class SourceRouteContributorKey : IExposureContributorKey {
        public ExposureRoute Route { get; set; }
        public ExposureSource Source { get; set; }
    }
}
