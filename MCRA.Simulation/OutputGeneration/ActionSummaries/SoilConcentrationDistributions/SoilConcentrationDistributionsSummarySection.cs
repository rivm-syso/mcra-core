using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SoilConcentrationDistributionsSummarySection : SubstanceConcentrationsSummarySection {

        public void Summarize(
            ICollection<SoilConcentrationDistribution> soilConcentrations,
            ConcentrationUnit concentrationUnit,
            double lowerPercentage,
            double upperPercentage
        ) {
            ConcentrationUnit = concentrationUnit.GetShortDisplayName();
            Records = summarizeConcentrations(
                soilConcentrations,
                lowerPercentage,
                upperPercentage
            );
            PercentileRecords = summarizeBoxPlotRecord(soilConcentrations);
        }

        private List<SubstanceConcentrationsSummaryRecord> summarizeConcentrations(
            ICollection<SoilConcentrationDistribution> dustConcentrations,
            double lowerPercentage,
            double upperPercentage
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var records = new List<SubstanceConcentrationsSummaryRecord>();
            var substances = dustConcentrations.Select(c => c.Substance).ToHashSet();
            foreach (var substance in substances) {
                var samples = dustConcentrations
                    .Where(r => r.Substance.Code.Equals(substance.Code, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var positives = samples.Where(r => r.Concentration > 0)
                    .Select(c => c.Concentration)
                    .ToList();
                var percentilesSampleConcentrations = positives.Any()
                    ? positives.Percentiles(percentages)
                    : percentages.Select(r => double.NaN).ToArray();

                var record = new SubstanceConcentrationsSummaryRecord() {
                    Unit = samples.First().Unit.GetShortDisplayName(),
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

        private List<SubstanceConcentrationsPercentilesRecord> summarizeBoxPlotRecord(
            ICollection<SoilConcentrationDistribution> soilConcentrations
        ) {
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            var percentilesRecords = new List<SubstanceConcentrationsPercentilesRecord>();
            var substances = soilConcentrations.Select(c => c.Substance).ToHashSet();
            foreach (var substance in substances) {
                var samples = soilConcentrations
                    .Where(r => r.Substance.Code.Equals(substance.Code, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var positives = samples.Where(r => r.Concentration > 0)
                    .Select(r => r.Concentration)
                    .ToList();
                var percentiles = positives.Any()
                    ? positives.Percentiles(percentages).ToList()
                    : percentages.Select(r => double.NaN).ToList();
                var outliers = positives.Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                        || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                        .Select(c => c).ToList();

                var record = new SubstanceConcentrationsPercentilesRecord() {
                    Unit = samples.First().Unit.GetShortDisplayName(),
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