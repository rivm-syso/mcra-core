using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.Consumptions;
using System.Collections.Generic;
using System.Linq;
using System;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmSurveySummarySection : SummarySection {
        public List<HbmSurveySummaryRecord> Records { get; set; }
        public List<HbmPopulationCharacteristicsDataRecord> HbmPopulationRecords { get; set; }

        public List<SelectedPropertyRecord> SelectedPropertyRecords { get; set; }

        public bool PopulationSubsetSelection { get; set; }

        public void Summarize(
            ICollection<HumanMonitoringSurvey> hbmSurveys, 
            ICollection<Individual> hbmIndividuals,
            Population population,
            IndividualSubsetType individualSubsetType,
            List<string> selectedHbmSubsetProperties
        ) {
            Records = hbmSurveys
                .Select(r => new HbmSurveySummaryRecord() {
                    Code = r.Code,
                    Name = r.Name,
                    Description = r.Description,
                    NumberOfSurveyDaysPerIndividual = r.NumberOfSurveyDays,
                    NumberOfIndividuals = r.Individuals.Count,
                    NumberOfIndividualDays = r.Individuals.Sum(i => i.NumberOfDaysInSurvey)
                })
                .ToList();
            SelectedPropertyRecords = summarizeSelectedProperties(population, individualSubsetType, selectedHbmSubsetProperties);
            HbmPopulationRecords = summarizePopulationCharacteristics(hbmIndividuals);
        }

        private List<HbmPopulationCharacteristicsDataRecord> summarizePopulationCharacteristics(
            ICollection<Individual> hbmIndividuals
        ) {
            var percentages = new double[] { 25, 50, 75 };
            var result = new List<HbmPopulationCharacteristicsDataRecord>();

            var samplingWeights = hbmIndividuals.Select(c => c.SamplingWeight).ToList();
            var totalSamplingWeights = samplingWeights.Sum();
            var bodyWeights = hbmIndividuals.Select(i => i.BodyWeight).ToList();
            var percentiles = bodyWeights.Select(c => (double)c).PercentilesWithSamplingWeights(samplingWeights, percentages);
            var sum = hbmIndividuals.Sum(i => i.BodyWeight * i.SamplingWeight);

            result.Add(new HbmPopulationCharacteristicsDataRecord {
                Property = "Body weight",
                Mean = sum / totalSamplingWeights,
                P25 = percentiles[0],
                Median = percentiles[1],
                P75 = percentiles[2],
                Min = bodyWeights.Min(),
                Max = bodyWeights.Max(),
                DistinctValues = bodyWeights.Distinct().Count(),
            });
            var individualProperties = hbmIndividuals.Select(i => i.IndividualPropertyValues.OrderBy(ip => ip.IndividualProperty.Name, StringComparer.OrdinalIgnoreCase).ToList()).ToList();
            var properties = individualProperties.First();

            for (int i = 0; i < properties.Count; i++) {
                var property = properties[i].IndividualProperty;
                var propertyValues = hbmIndividuals
                    .Select(r => (
                        Individual: r,
                        PropertyValue: r.IndividualPropertyValues.FirstOrDefault(pv => pv.IndividualProperty == property)
                    ))
                    .ToList();

                var countDistinct = propertyValues
                    .Where(r => r.PropertyValue != null)
                    .Select(c => c.PropertyValue.DoubleValue)
                    .Distinct().Count();

                if (property.PropertyType.GetPropertyType() == PropertyType.Covariable && countDistinct > 2) {
                    var availableValues = propertyValues
                        .Where(r => r.PropertyValue?.DoubleValue != null && !double.IsNaN((double)r.PropertyValue.DoubleValue))
                        .ToList();

                    var missingValues = propertyValues
                        .Where(r => r.PropertyValue?.DoubleValue == null || double.IsNaN((double)r.PropertyValue.DoubleValue))
                        .ToList();

                    var availableDoubleValues = availableValues.Select(r => (double)r.PropertyValue.DoubleValue).ToList();
                    var availableSamplingWeights = availableValues.Select(r => r.Individual.SamplingWeight).ToList();

                    var totalSamplingWeightMissing = missingValues.Sum(r => r.Individual.SamplingWeight);

                    percentiles = availableDoubleValues.PercentilesWithSamplingWeights(availableSamplingWeights, percentages);

                    sum = availableValues.Sum(r => r.Individual.SamplingWeight * (double)r.PropertyValue.DoubleValue);
                    result.Add(
                        new HbmPopulationCharacteristicsDataRecord {
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
                    var levels = hbmIndividuals
                        .Select(r => (
                            Individual: r,
                            Value: r.IndividualPropertyValues.FirstOrDefault(ipv => ipv.IndividualProperty == property)?.Value ?? "-"
                        ))
                        .GroupBy(r => r.Value)
                        .Select(g => new PopulationLevelStatisticRecord() {
                            Level = g.Key,
                            Frequency = g.Sum(r => r.Individual.SamplingWeight)
                        })
                        .OrderBy(r => r.Level, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    result.Add(new HbmPopulationCharacteristicsDataRecord {
                        Property = property.Name,
                        Levels = levels,
                        DistinctValues = levels.Count,
                    });
                }
            }

            return result;
        }

        private List<SelectedPropertyRecord> summarizeSelectedProperties(
            Population population,
            IndividualSubsetType individualSubsetType,
            List<string> selectedHbmSubsetProperties
        ) {
            if (individualSubsetType == IndividualSubsetType.IgnorePopulationDefinition) {
                return null;
            }
            if (population?.PopulationIndividualPropertyValues == null) {
                return null;
            }
            var selectedPropertyRecords = new List<SelectedPropertyRecord>();
            foreach (var item in population.PopulationIndividualPropertyValues) {
                if (individualSubsetType == IndividualSubsetType.MatchToPopulationDefinitionUsingSelectedProperties
                    && !selectedHbmSubsetProperties.Contains(item.Key, StringComparer.OrdinalIgnoreCase)
                ) {
                    continue;
                }
                selectedPropertyRecords.Add(new SelectedPropertyRecord() {
                    PropertyName = item.Key,
                    Levels = item.Value.Value != null ? string.Join(", ", item.Value.Value) : $"{item.Value.MinValue} - {item.Value.MaxValue}"
                });
            }
            return selectedPropertyRecords;
        }
    }
}
