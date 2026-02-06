using MCRA.Data.Compiled.Interfaces;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class SubstanceConcentrationsSummarySection : SummarySection {
        public string UnitDisplayName { get; private set; }
        public List<SubstanceConcentrationsSummaryRecord> Records { get; set; }
        public List<SubstanceConcentrationsPercentilesRecord> PercentileRecords { get; set; }

        public void Summarize<T>(
            ICollection<T> concentrations,
            string unitDisplayName,
            double lowerPercentage,
            double upperPercentage
        ) where T : ISubstanceConcentration {
            UnitDisplayName = unitDisplayName;
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var records = new List<SubstanceConcentrationsSummaryRecord>();
            var substances = concentrations.Select(c => c.Substance).ToHashSet();
            foreach (var substance in substances) {
                var samples = concentrations
                    .Where(r => r.Substance.Code.Equals(substance.Code, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var positives = samples.Where(r => r.Concentration > 0)
                    .Select(c => c.Concentration)
                    .ToList();
                var percentilesSampleConcentrations = positives.Any()
                    ? positives.Percentiles(percentages)
                    : [.. percentages.Select(r => double.NaN)];

                var record = new SubstanceConcentrationsSummaryRecord() {
                    Unit = samples.First().UnitTypeString,
                    SubstanceCode = substance.Code,
                    SubstanceName = substance.Name,
                    SamplesTotal = samples.Count,
                    MeanPositives = positives.Any() ? positives.Average() : double.NaN,
                    LowerPercentilePositives = percentilesSampleConcentrations[0],
                    MedianPositives = percentilesSampleConcentrations[1],
                    UpperPercentilePositives = percentilesSampleConcentrations[2],
                    PositiveMeasurements = positives.Count(),
                };
                records.Add(record);
            }
            Records = records;
            PercentileRecords = summarizeBoxPlotRecords(concentrations);

        }
        private List<SubstanceConcentrationsPercentilesRecord> summarizeBoxPlotRecords<T> (
            ICollection<T> concentrations
        ) where T : ISubstanceConcentration {
            var percentages = BoxPlotChartCreatorBase.BoxPlotPercentages;
            var percentilesRecords = new List<SubstanceConcentrationsPercentilesRecord>();
            var substances = concentrations.Select(c => c.Substance).ToHashSet();
            foreach (var substance in substances) {
                var samples = concentrations
                    .Where(r => r.Substance.Code.Equals(substance.Code, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var positives = samples.Where(r => r.Concentration > 0)
                    .Select(r => r.Concentration)
                    .ToList();
                var percentiles = positives.Any()
                    ? [.. positives.Percentiles(percentages)]
                    : percentages.Select(r => double.NaN).ToList();
                var outliers = positives.Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                        || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                        .Select(c => c).ToList();

                var record = new SubstanceConcentrationsPercentilesRecord() {
                    Unit = UnitDisplayName,
                    MinPositives = positives.Any() ? positives.Min() : 0,
                    MaxPositives = positives.Any() ? positives.Max() : 0,
                    SubstanceCode = substance.Code,
                    SubstanceName = substance.Name,
                    Percentiles = percentiles,
                    NumberOfMeasurements = samples.Count(),
                    NumberOfPositives = positives.Count,
                    Percentage = positives.Count * 100d / samples.Count,
                    Outliers = outliers,
                };
                if (record.NumberOfMeasurements > 0) {
                    percentilesRecords.Add(record);
                }
            }
            return percentilesRecords;
        }
    }
}
