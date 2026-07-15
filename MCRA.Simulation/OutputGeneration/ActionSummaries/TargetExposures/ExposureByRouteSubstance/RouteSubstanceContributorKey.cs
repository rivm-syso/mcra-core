using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic {
    public class RouteSubstanceContributorKey : IContributorKey {
        public ExposureRoute Route { get; set; }
        public Compound Substance { get; set; }
    }
}
