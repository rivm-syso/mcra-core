using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Objects;
using MCRA.Simulation.OutputGeneration.ActionSummaries.Consumptions;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes Individuals
    /// </summary>
    public sealed class IndividualsSummarySection : SummarySection {

        [Display(AutoGenerateField = false)]
        public List<SelectedPropertyRecord> SelectedPropertyRecords { get; set; }
        public List<PopulationCharacteristicsDataRecord> Records { get; set; }

        [DisplayName("Total sampling weights for individuals")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double TotalSamplingWeights { get; set; }

        [DisplayName("Individual-days")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalIndividualDays { get; set; }

        /// <summary>
        /// Summarize the individuals
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="population"></param>
        public void Summarize(
            ICollection<IIndividualDay> individuals,
            Population population
        ) {
            TotalIndividualDays = individuals.Count;
            TotalSamplingWeights = individuals.Sum(w => w.SimulatedIndividual.SamplingWeight);
            summarizeIndividualCharacteristics(individuals);
            summarizeSelectedProperties(population);
        }

        private void summarizeSelectedProperties(
            Population population
        ) {
            SelectedPropertyRecords = [];
            foreach (var item in population.PopulationIndividualPropertyValues) {
                SelectedPropertyRecords.Add(new SelectedPropertyRecord() {
                    PropertyName = item.Key,
                    Levels = item.Value.Value != null ? string.Join(", ", item.Value.Value) : $"{item.Value.MinValue} - {item.Value.MaxValue}"
                });
            }
        }

        /// <summary>
        /// Individual characteristics
        /// </summary>
        /// <param name="dataSource"></param>
        private void summarizeIndividualCharacteristics(ICollection<IIndividualDay> individualDays) {
            var simulatedIndividuals = individualDays
                .Select(c => c.SimulatedIndividual)
                .ToList();
            if (!simulatedIndividuals?.Any() ?? true) {
                return;
            }
            var percentages = new double[] { 25, 50, 75 };
            var samplingWeights = simulatedIndividuals
                .Select(c => c.SamplingWeight)
                .ToList();
            var bodyWeights = simulatedIndividuals
                .Select(i => i.BodyWeight)
                .ToList();
            var percentiles = bodyWeights.PercentilesWithSamplingWeights(samplingWeights, percentages);
            var sum = simulatedIndividuals.Sum(i => i.BodyWeight * i.SamplingWeight);

            Records = [
                new () {
                    Property = "Body weight",
                    Mean = sum / samplingWeights.Sum(),
                    P25 = percentiles[0],
                    Median = percentiles[1],
                    P75 = percentiles[2],
                    Min = bodyWeights.Min(),
                    Max = bodyWeights.Max(),
                    DistinctValues = bodyWeights.Distinct().Count(),
                }
            ];
            var individualProperties = simulatedIndividuals
                .Select(c => c.Individual.IndividualPropertyValues
                    .OrderBy(ip => ip.IndividualProperty.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList()
                ).ToList();

            var properties = individualProperties.First();

            for (int i = 0; i < properties.Count; i++) {
                var property = properties[i].IndividualProperty;
                var propertyValues = simulatedIndividuals.Select(c => c.Individual)
                    .Select(r => (
                        Individual: r,
                        PropertyValue: r.GetPropertyValue(property)
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
                    var availableDoubleValues = availableValues
                        .Select(r => r.PropertyValue.DoubleValue.Value)
                        .ToList();
                    var availableSamplingWeights = availableValues
                        .Select(r => r.Individual.SamplingWeight)
                        .ToList();

                    var totalSamplingWeightMissing = missingValues.Sum(r => r.Individual.SamplingWeight);

                    percentiles = availableDoubleValues.PercentilesWithSamplingWeights(availableSamplingWeights, percentages);

                    sum = availableValues.Sum(r => r.Individual.SamplingWeight * r.PropertyValue.DoubleValue.Value);
                    Records.Add(
                        new PopulationCharacteristicsDataRecord {
                            Property = property.Name,
                            Mean = sum / samplingWeights.Sum(),
                            P25 = percentiles[0],
                            Median = percentiles[1],
                            P75 = percentiles[2],
                            Min = availableDoubleValues.Min(c => c),
                            Max = availableDoubleValues.Max(c => c),
                            DistinctValues = countDistinct,
                            Missing = totalSamplingWeightMissing
                        });
                } else {
                    var levels = simulatedIndividuals.Select(c => c.Individual)
                        .Select(r => (
                            Individual: r,
                            Value: r.GetPropertyValue(property)?.Value ?? "-"
                        ))
                        .GroupBy(r => r.Value)
                        .Select(g => new PopulationLevelStatisticRecord() {
                            Level = g.Key,
                            Frequency = g.Count()
                        })
                        .OrderBy(r => r.Level, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    Records.Add(new PopulationCharacteristicsDataRecord {
                        Property = property.Name,
                        Levels = levels,
                        DistinctValues = levels.Count,
                    });
                }
            }
        }
    }
}
