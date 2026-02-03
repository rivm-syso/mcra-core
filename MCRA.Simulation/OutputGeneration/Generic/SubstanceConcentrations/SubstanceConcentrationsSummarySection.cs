using System.Linq;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration.Generic;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class SubstanceConcentrationsSummarySection : SummarySection {
        public string ConcentrationUnit { get; set; }
        public List<SubstanceConcentrationsSummaryRecord> Records { get; set; }
        public List<SubstanceConcentrationsPercentilesRecord> PercentileRecords { get; set; }

        protected static List<SubstanceConcentrationsSummaryRecord> summarizeConcentrations(
            ICollection<SimpleSubstanceConcentration> concentrations,
            double lowerPercentage,
            double upperPercentage
        ) {
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
                    Unit = samples.First().UnitString,
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
            return records;
        }
        protected static List<SubstanceConcentrationsPercentilesRecord> summarizeBoxPlotRecords(
            ICollection<SimpleSubstanceConcentration> concentrations
        ) {
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
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
                    Unit = samples.First().UnitString,
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
