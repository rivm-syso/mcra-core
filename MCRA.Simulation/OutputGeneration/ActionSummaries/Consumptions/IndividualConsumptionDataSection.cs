using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.Consumptions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes the consumption of foods as eaten from input data
    /// </summary>
    public sealed class IndividualConsumptionDataSection : SummarySection {

        [Display(AutoGenerateField = false)]
        public List<SelectedPropertyRecord> SelectedPropertyRecords { get; set; }
        public List<PopulationCharacteristicsDataRecord> Records { get; set; }

        [DisplayName("Consumption survey")]
        public string Survey { get; set; }

        [DisplayName("Population")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalIndividuals { get; set; }

        [DisplayName("Individual-days")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalIndividualDays { get; set; }

        [DisplayName("Consumption-days")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalIndividualUserDays { get; set; }

        public bool UseSamplingWeights { get; set; }

        [DisplayName("Total sampling weights for individuals")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double TotalSamplingWeights { get; set; }

        [DisplayName("Total sampling weights for individual-days")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double TotalSamplingWeightsPerDay { get; set; }

        [DisplayName("Total sampling weights for consumption-days")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double TotalSamplingWeightsPerUserDay { get; set; }

        public List<DateTimeMonthRecord> DateTimeMonthRecords { get; set; }

        public bool PopulationSubsetSelection { get; set; }

        /// <summary>
        /// Summarize the consumption days of the individuals in the survey.
        /// </summary>
        /// <param name="foodSurvey"></param>
        /// <param name="individuals"></param>
        /// <param name="individualDays"></param>
        /// <param name="consumptions"></param>
        /// <param name="consumptionsByModelledFood"></param>
        public void Summarize(
            FoodSurvey foodSurvey,
            ICollection<Individual> individuals,
            ICollection<IndividualDay> individualDays,
            ICollection<FoodConsumption> consumptions,
            ICollection<ConsumptionsByModelledFood> consumptionsByModelledFood,
            IndividualSubsetType individualSubsetType,
            bool populationSubsetSelection,
            Population population
        ) {
            //Add above things to summary
            Survey = foodSurvey.Name;
            TotalIndividuals = individuals.Count;
            TotalIndividualDays = individualDays.Count;
            PopulationSubsetSelection = populationSubsetSelection && individualSubsetType != IndividualSubsetType.IgnorePopulationDefinition ;

            if (consumptionsByModelledFood != null) {
                var consumerDays = consumptionsByModelledFood.Select(r => r.IndividualDay).Distinct().ToList();
                TotalIndividualUserDays = consumerDays.Count;
                TotalSamplingWeightsPerUserDay = consumerDays.Sum(c => c.Individual.SamplingWeight);
            } else {
                var consumerDays = consumptions.Select(r => r.IndividualDay).Distinct().ToList();
                TotalIndividualUserDays = consumerDays.Count;
                TotalSamplingWeightsPerUserDay = consumerDays.Sum(r => r.Individual.SamplingWeight);
            }

            TotalSamplingWeights = individuals.Sum(w => w.SamplingWeight);
            TotalSamplingWeightsPerDay = individualDays.Sum(c => c.Individual.SamplingWeight);
            DateTimeMonthRecords = individualDays.GroupBy(c => c.Date?.Month)
                .OrderBy(c => c.Key)
                .Select(c => {
                    var count = c.Count();
                    return new DateTimeMonthRecord {
                        Month = Enum.TryParse(c.Key.ToString(), out MonthType monthType) ? monthType.ToString() : "Unknown",
                        NumberOfDays = count,
                        Percentage = 100.0 * count / TotalIndividualDays
                    };
                })
                .ToList();
            if (individuals.Any(indDay => indDay.SamplingWeight != 1)) {
                UseSamplingWeights = true;
            }

            SummarizePopulationCharacteristics(individuals);
            SummarizeSelectedProperties(population, populationSubsetSelection);
        }

        public void SummarizeSelectedProperties(
                Population population,
                bool populationSubsetSelection
            ) {
            SelectedPropertyRecords = [];
            if (populationSubsetSelection) {
                foreach (var item in population.PopulationIndividualPropertyValues) {
                    SelectedPropertyRecords.Add(new SelectedPropertyRecord() {
                        PropertyName = item.Key,
                        Levels = item.Value.Value != null ? string.Join(", ", item.Value.Value) : $"{item.Value.MinValue} - {item.Value.MaxValue}"
                    });
                }
            }
        }

        /// <summary>
        /// Individual characteristics
        /// </summary>
        /// <param name="dataSource"></param>
        public void SummarizePopulationCharacteristics(ICollection<Individual> individuals) {
            if (!individuals?.Any() ?? true) {
                return;
            }

            var percentages = new double[] { 25, 50, 75 };
            var samplingWeights = individuals.Select(c => c.SamplingWeight).ToList();
            var totalSamplingWeights = samplingWeights.Sum();
            var bodyWeights = individuals.Select(i => i.BodyWeight).ToList();
            var percentiles = bodyWeights.PercentilesWithSamplingWeights(samplingWeights, percentages);
            var sum = individuals.Sum(i => i.BodyWeight * i.SamplingWeight);

            Records = [
                new () {
                    Property = "Body weight",
                    Mean = sum / totalSamplingWeights,
                    P25 = percentiles[0],
                    Median = percentiles[1],
                    P75 = percentiles[2],
                    Min = bodyWeights.Min(),
                    Max = bodyWeights.Max(),
                    DistinctValues = bodyWeights.Distinct().Count(),
                }
            ];

            var individualProperties = individuals
                .Select(i => i.IndividualPropertyValues
                    .OrderBy(ip => ip.IndividualProperty.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList()
                ).ToList();
            var properties = individualProperties.First();

            for (int i = 0; i < properties.Count; i++) {
                var property = properties[i].IndividualProperty;
                var propertyValues = individuals
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

                    var availableDoubleValues = availableValues.Select(r => r.PropertyValue.DoubleValue.Value).ToList();
                    var availableSamplingWeights = availableValues.Select(r => r.Individual.SamplingWeight).ToList();

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
                    var levels = individuals
                        .Select(r => (
                            Individual: r,
                            Value: r.GetPropertyValue(property)?.Value ?? "-"
                        ))
                        .GroupBy(r => r.Value)
                        .Select(g => new PopulationLevelStatisticRecord() {
                            Level = g.Key,
                            Frequency = g.Sum(r => r.Individual.SamplingWeight)
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
