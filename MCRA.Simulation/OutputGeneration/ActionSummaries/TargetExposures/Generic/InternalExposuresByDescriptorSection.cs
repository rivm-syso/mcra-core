namespace MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic {
    public abstract class InternalExposuresByDescriptorSection<S>
        : SummarySection where S : IContributorKey {

        public abstract string DescriptorName { get; }
        public abstract string DescriptorKey { get; }

    }
}
