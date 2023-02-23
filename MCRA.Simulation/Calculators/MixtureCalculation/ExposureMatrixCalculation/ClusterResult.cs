namespace MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation {
    public sealed class ClusterResult {
        public List<ClusterRecord> Clusters { get; set; }
        public int[,] Merge { get; set; }
        public List<double> Height { get; set; }
        public List<int> Order { get; set; }
    }
}
