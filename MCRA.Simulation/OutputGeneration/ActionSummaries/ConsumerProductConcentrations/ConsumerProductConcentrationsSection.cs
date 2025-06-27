using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ConsumerProductConcentrationsSection : SummarySection {
        public List<ConsumerProductConcentrationRecord> Records { get; set; } = [];
        public List<ConsumerProductConcentrationPercentilesRecord> PercentileRecords { get; set; } = [];
        public ConcentrationUnit ConcentrationUnit { get; set; }
        public void Summarize(
            ICollection<ConsumerProductConcentration> consumerProductConcentrations,
            ConcentrationUnit concentrationUnit,
            double lowerPercentage,
            double upperPercentage
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            ConcentrationUnit = concentrationUnit;
            var groups = consumerProductConcentrations
                .GroupBy(c => (c.Substance, c.Product))
                .ToList();
            foreach (var group in groups) {
                var concentrations = group.Select(r => r.Concentration).ToList();
                var positives = concentrations.Where(c => c > 0).ToList();
                var percentiles = positives.Any() ? positives.Percentiles(percentages).ToList() : [.. percentages.Select(r => double.NaN)];
                var percentilesAll = concentrations.Percentiles(percentages);

                Records.Add(new ConsumerProductConcentrationRecord() {
                    ProductName = group.Key.Product.Name,
                    ProductCode = group.Key.Product.Code,
                    SubstanceCode = group.Key.Substance.Code,
                    SubstanceName = group.Key.Substance.Name,
                    Mean = concentrations.Average(),
                    Median = percentilesAll[1],
                    LowerPercentile = percentilesAll[0],
                    UpperPercentile = percentilesAll[2],
                    MeanPositives = positives.Any() ? positives.Average() : double.NaN,
                    MedianPositives = percentiles[1],
                    LowerPercentilePositives = percentiles[0],
                    UpperPercentilePositives = percentiles[2],
                    NumberOfPositives = positives.Count,
                    TotalNumber = group.Count(),
                });

                var boxPlotPercentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
                var boxplotPercentiles = positives.Any() ? positives
                        .Percentiles(boxPlotPercentages).ToList() : [.. boxPlotPercentages.Select(r => double.NaN)];
                var outliers = positives.Where(c => c > boxplotPercentiles[4] + 3 * (boxplotPercentiles[4] - boxplotPercentiles[2])
                   || c < boxplotPercentiles[2] - 3 * (boxplotPercentiles[4] - boxplotPercentiles[2]))
                   .ToList();
                PercentileRecords.Add(new ConsumerProductConcentrationPercentilesRecord() {
                    ProductName = group.Key.Product.Name,
                    ProductCode = group.Key.Product.Code,
                    SubstanceCode = group.Key.Substance.Code,
                    SubstanceName = group.Key.Substance.Name,
                    MinPositives = positives.Any() ? positives.Min(c => c) : 0,
                    MaxPositives = positives.Any() ? positives.Max(c => c) : 0,
                    NumberOfPositives = positives.Count,
                    Percentiles = boxplotPercentiles,
                    NumberOfMeasurements = group.Count(),
                    Percentage = positives.Count * 100d / group.Count(),
                    Outliers = [.. outliers.Select(c => c)],
                    NumberOfOutLiers = outliers.Count,
                });
            }
        }
    }
}
