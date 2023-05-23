using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class PercentileBootstrapSectionBase<T> : SummarySection where T : IIntakePercentileBootstrapRecord, new() {

        public UncertainDataPointCollection<double> Percentiles { get; set; } = new();

        public IList<T> GetPercentileBootstrapRecords(bool includeMedian) {
            var result = new List<T>();

            if (includeMedian) {
                result.AddRange(
                    Percentiles.Select(p => new T {
                        Percentile = p.XValue / 100,
                        Value = p.ReferenceValue
                    })
                );
            }

            result.AddRange(
                Percentiles.SelectMany(
                    p => p.UncertainValues.Select((u, i) => new { Index = i, Value = u }),
                    (p, u) => new T {
                        Bootstrap = u.Index,
                        Percentile = p.XValue / 100,
                        Value = u.Value
                    }
                ).ToList()
            );

            return result;
        }
    }
}
