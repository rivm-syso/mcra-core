using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class PercentileBootstrapSectionBase : SummarySection {

        public UncertainDataPointCollection<double> Percentiles { get; set; } = new();

        public IList<IntakePercentileBootstrapRecord> GetPercentileBootstrapRecords(bool includeMedian) {
            var result = new List<IntakePercentileBootstrapRecord>();

            if (includeMedian) {
                for (var i = 0; i < Percentiles.Count; i++) {
                    result.Add(new IntakePercentileBootstrapRecord {
                        Percentile = Percentiles[i].XValue / 100,
                        Exposure = Percentiles[i].ReferenceValue,
                    });
                }
            }

            for (var i = 0; i < Percentiles.Count; i++) {
                for (var j = 0; j < Percentiles[i].UncertainValues.Count; j++) {
                    result.Add(new IntakePercentileBootstrapRecord {
                        Bootstrap = j + 1,
                        Percentile = Percentiles[i].XValue / 100,
                        Exposure = Percentiles[i].UncertainValues[j],
                    });
                }
            }
            return result;
        }
    }
}
