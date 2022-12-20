namespace MCRA.Simulation.OutputGeneration {
    public sealed class ModelBasedDistributionSection : DistributionSectionBase {
        public bool IsAcuteCovariateModelling { get; set; }
        public ModelBasedDistributionSection() {
            IsTotalDistribution = true;
        }
    }
}
