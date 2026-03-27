namespace MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic {
    public abstract class InternalExposuresByDescriptorSection<S>
        : SummarySection where S : IExposureContributorKey {

        public abstract string DescriptorName { get; }
        public abstract string DescriptorKey { get; }

    }
}
