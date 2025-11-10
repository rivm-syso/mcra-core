namespace MCRA.Simulation.OutputGeneration.CombinedActionSummaries {

    /// <summary>
    /// Represents a collection of all simulation data that has been post-processed for visualization.
    /// </summary>
    public class CombinedPercentilesSectionBase : SummarySection {
        public double UncertaintyLowerLimit { get; protected set; } = 2.5;
        public double UncertaintyUpperLimit { get; protected set; } = 97.5;
        public List<double> Percentages { get; protected set; }
        public List<ModelSummaryRecord> ModelSummaryRecords { get; protected set; }
        public List<CombinedPercentileRecord> CombinedPercentileRecords { get; protected set; }

        public CombinedPercentileRecord GetPercentileRecord(string idModel, double percentage) {
            return CombinedPercentileRecords.FirstOrDefault(r => r.IdModel == idModel
                && Math.Abs(r.Percentage - percentage) < 0.0000001);
        }
    }
}
