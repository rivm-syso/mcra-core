using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public interface IHbmExposureContributorKey : IExposureContributorKey {
        string GetKey();
    }
}
