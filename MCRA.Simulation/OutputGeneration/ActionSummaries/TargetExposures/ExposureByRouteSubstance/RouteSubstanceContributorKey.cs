using MCRA.General;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic {
    public class RouteSubstanceContributorKey : IExposureContributorKey {
        public ExposureRoute Route { get; set; }
        public string Substance { get; set; }
    }
}
