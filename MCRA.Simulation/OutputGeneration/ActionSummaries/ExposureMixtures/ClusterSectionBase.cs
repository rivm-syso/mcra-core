using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.ExposureMixtures;
using MCRA.Utils;
using MCRA.Utils.Statistics;
using System.Text;

namespace MCRA.Simulation.OutputGeneration {
    public class ClusterSectionBase : SummarySection {
        public List<SimulatedIndividual> Individuals { get; set; }
        public List<int> Clusters { get; set; }
        public List<string> IndividualCodes { get; set; }
        public List<string> ComponentCodes { get; set; }
        public GeneralMatrix VMatrix { get; set; }
        public ClusterResult ClusterResult { get; set; }
        public int MaximumSize { get; set; }
        public int MinimumSize { get; set; }
        public int LargestCluster { get; set; }
        public int SmallestCluster { get; set; }
        public bool AutomaticallyDetermineNumberOfClusters { get; set; }

        public List<IndividualPropertyRecord> Records { get; set; }
        private List<string> _rowNames;
        private List<string> _propertiesNumeric;
        private List<string> _propertiesAlfaNumeric;

        /// <summary>
        /// Summarize clustering results
        /// </summary>
        /// <param name="individualMatrix"></param>
        /// <param name="automaticallyDetermineNumberOfClusters"></param>
        public void SummarizeClustering(
            IndividualMatrix individualMatrix,
            bool automaticallyDetermineNumberOfClusters = false
        ) {
            AutomaticallyDetermineNumberOfClusters = automaticallyDetermineNumberOfClusters;
            Individuals = individualMatrix.SimulatedIndividuals.ToList();
            ClusterResult = individualMatrix.ClusterResult;
            Clusters = individualMatrix.ClusterResult.Clusters.Select(c => c.SimulatedIndividuals.Count).ToList();
            MaximumSize = Clusters.Max();
            LargestCluster = Clusters.FindIndex(c => c == MaximumSize) + 1;
            MinimumSize = Clusters.Min();
            SmallestCluster = Clusters.FindIndex(c => c == MinimumSize) + 1;
            Records = [.. summarizePopulationCharacteristics(Individuals, "Population")];
            var ix = 1;
            foreach (var cluster in individualMatrix.ClusterResult.Clusters) {
                Records.AddRange(summarizePopulationCharacteristics(cluster.SimulatedIndividuals, $"Subgroup {ix}"));
                ix++;
            }
            _propertiesNumeric = Records.Where(c => c.Mean != null)
                            .Select(c => c.Property)
                            .Distinct()
                            .ToList();
            _propertiesAlfaNumeric = Records.Where(c => c.Mean == null)
                .Select(c => c.Property).Distinct()
                .ToList();
            _rowNames = Records.Select(c => c.Group).Distinct().ToList();
        }

        private List<IndividualPropertyRecord> summarizePopulationCharacteristics(
            ICollection<SimulatedIndividual> hbmIndividuals,
            string groupIdentifier
        ) {
            var percentages = new double[] { 25, 50, 75 };
            var result = new List<IndividualPropertyRecord>();

            var samplingWeights = hbmIndividuals.Select(c => c.SamplingWeight).ToList();
            var totalSamplingWeights = samplingWeights.Sum();
            var bodyWeights = hbmIndividuals.Select(i => i.BodyWeight).ToList();
            var percentiles = bodyWeights.PercentilesWithSamplingWeights(samplingWeights, percentages);
            var sum = hbmIndividuals.Sum(i => i.BodyWeight * i.SamplingWeight);

            result.Add(new IndividualPropertyRecord {
                Group = groupIdentifier,
                Number = hbmIndividuals.Count,
                Property = "Body weight",
                Mean = sum / totalSamplingWeights,
                P25 = percentiles[0],
                Median = percentiles[1],
                P75 = percentiles[2],
                Min = bodyWeights.Min(),
                Max = bodyWeights.Max(),
                DistinctValues = bodyWeights.Distinct().Count(),
            });
            var individualProperties = hbmIndividuals
                .SelectMany(i => i.Individual.IndividualPropertyValues.OrderBy(ip => ip.IndividualProperty.Name, StringComparer.OrdinalIgnoreCase).ToList())
                .Select(c => c.IndividualProperty)
                .Distinct().ToList();

            foreach (var property in individualProperties) {
                var propertyValues = hbmIndividuals
                    .Select(r => (
                        Individual: r,
                        PropertyValue: r.Individual.GetPropertyValue(property)
                    ))
                    .ToList();

                var countDistinct = propertyValues
                    .Where(r => r.PropertyValue != null)
                    .Select(c => c.PropertyValue.DoubleValue)
                    .Distinct().Count();

                if (property.PropertyType.GetPropertyType() == PropertyType.Covariable && countDistinct > 2) {
                    var availableValues = propertyValues
                        .Where(r => r.PropertyValue?.DoubleValue != null && !double.IsNaN(r.PropertyValue.DoubleValue.Value))
                        .ToList();

                    var missingValues = propertyValues
                        .Where(r => r.PropertyValue?.DoubleValue == null || double.IsNaN(r.PropertyValue.DoubleValue.Value))
                        .ToList();

                    var availableDoubleValues = availableValues.Select(r => r.PropertyValue.DoubleValue.Value).ToList();
                    var availableSamplingWeights = availableValues.Select(r => r.Individual.SamplingWeight).ToList();

                    var totalSamplingWeightMissing = missingValues.Sum(r => r.Individual.SamplingWeight);

                    percentiles = availableDoubleValues.PercentilesWithSamplingWeights(availableSamplingWeights, percentages);

                    sum = availableValues.Sum(r => r.Individual.SamplingWeight * r.PropertyValue.DoubleValue.Value);
                    result.Add(
                        new IndividualPropertyRecord {
                            Group = groupIdentifier,
                            Number = hbmIndividuals.Count,
                            Property = property.Name,
                            Mean = sum / samplingWeights.Sum(),
                            P25 = percentiles[0],
                            Median = percentiles[1],
                            P75 = percentiles[2],
                            Min = availableDoubleValues.Min(),
                            Max = availableDoubleValues.Max(),
                            DistinctValues = countDistinct,
                            Missing = totalSamplingWeightMissing
                        });
                } else {
                    var levels = hbmIndividuals
                        .Select(r => (
                            Individual: r,
                            Value: r.Individual.GetPropertyValue(property)?.Value ?? "-"
                        ))
                        .GroupBy(r => r.Value)
                        .Select(g => new PopulationLevelStatisticRecord() {
                            Level = g.Key,
                            Frequency = g.Sum(r => r.Individual.SamplingWeight)
                        })
                        .OrderBy(r => r.Level, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    result.Add(new IndividualPropertyRecord {
                        Group = groupIdentifier,
                        Number = hbmIndividuals.Count,
                        Property = property.Name,
                        Levels = levels,
                        DistinctValues = levels.Count,
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Write properties matrix to csv file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string WritePropertiesCsv(string filename) {
            if (_propertiesAlfaNumeric == null || _propertiesNumeric == null) {
                return null;
            }
            return WriteToCsvFile(filename, Records, _rowNames, _propertiesNumeric, _propertiesAlfaNumeric);
        }

        public string WriteToCsvFile(
            string filename,
            List<IndividualPropertyRecord> records,
            List<string> rowNames,
            List<string> propertiesNumeric,
            List<string> propertiesAlfaNumeric
        ) {
            var colNames = new List<string> { "Group", "Number" };
            foreach (var name in propertiesNumeric) {
                colNames.Add($"{name}-Mean");
                colNames.Add($"{name}-P25");
                colNames.Add($"{name}-Median");
                colNames.Add($"{name}-P75");
                colNames.Add($"{name}-Max");
            }
            foreach (var name in propertiesAlfaNumeric) {
                colNames.Add($"{name}-Labels");
            }

            using (var stream = new FileStream(filename, FileMode.Create)) {
                using (var streamWriter = new StreamWriter(stream, Encoding.Default)) {
                    streamWriter.WriteLine(string.Join(",", colNames));
                    for (int i = 0; i < rowNames.Count; i++) {
                        var row = new List<string>() { rowNames[i] };
                        var subset = records.Where(c => c.Group == rowNames[i]).ToList();
                        row.Add(subset.Select(c => c.Number).First().ToString());
                        foreach (var name in propertiesNumeric) {
                            var property = subset.First(c => c.Property == name);
                            row.Add($"{property.Mean}");
                            row.Add($"{property.P25}");
                            row.Add($"{property.Median}");
                            row.Add($"{property.P75}");
                            row.Add($"{property.Max}");
                        }
                        foreach (var name in propertiesAlfaNumeric) {
                            var property = subset.FirstOrDefault(c => c.Property == name);
                            var replaceString = property != null ? property.Labels.Replace(',', ' ') : " ";
                            row.Add(replaceString);
                        }
                        streamWriter.WriteLine(string.Join(",", row));
                    }
                }
            }
            return filename;
        }
    }
}
