using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.IntakeModelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class FrequencyAmountSummarySection : SummarySection {

        private List<int> _frequencies;

        public List<FrequencyAmountRelationRecord> FrequencyAmountRelations { get; set; }

        public List<DayFrequencyRecord> DayFrequencyRecords { get; set; }

        public List<ExposureFrequencyRecord> ExposureFrequencyRecords { get; set; }

        public List<ExposureSummaryRecord> ExposureSummaryRecords { get; set; }

        /// <summary>
        /// Summarizes the exposures of the main exposure simulation.
        /// </summary>
        /// <param name="intakes"></param>
        /// <param name="percentages"></param>
        public void Summarize(
                ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
                IDictionary<Compound, double> relativePotencyFactors,
                IDictionary<Compound, double> membershipProbabilities,
                bool isPerPerson
            ) {
            getFrequencyAmountRelation(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, isPerPerson);
            getFrequency(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, isPerPerson);
            getIntakes(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, isPerPerson);
        }

        private void getFrequency(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var intakeFrequencies = dietaryIndividualDayIntakes
                .GroupBy(idi => idi.SimulatedIndividualId)
                .Select(g => new IndividualFrequency() {
                    SimulatedIndividualId = g.Key,
                    Frequency = g.Count(idi => idi.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0),
                    Nbinomial = g.Count(),
                })
                .ToList();

            var nDay = intakeFrequencies.Select(c => c.Nbinomial).Max();

            _frequencies = new List<int>();
            for (int i = 0; i < nDay + 1; i++) {
                _frequencies.Add(intakeFrequencies.Count(c => c.Frequency == i));
            }

            var numberOfConsumers = _frequencies.Sum();
            DayFrequencyRecords = new List<DayFrequencyRecord>();
            for (int i = 0; i < _frequencies.Count; i++) {
                DayFrequencyRecords.Add(new DayFrequencyRecord() {
                    Days = i,
                    PercentageDays = 100d * i / (_frequencies.Count - 1),
                    NumberOfIndividuals = _frequencies[i],
                    PercentageNumberOfIndividuals = 100d * _frequencies[i] / numberOfConsumers
                });
            }
        }

        private void getIntakes(
            IEnumerable<DietaryIndividualDayIntake> dietaryIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var allIntakes = dietaryIndividualDayExposures
                .Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .ToList();
            var allWeights = dietaryIndividualDayExposures.Select(c => c.IndividualSamplingWeight).ToList();
            var positiveIntakes = dietaryIndividualDayExposures.Where(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0)
                .Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .ToList();
            var positiveWeights = dietaryIndividualDayExposures.Where(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0)
                .Select(c => c.IndividualSamplingWeight).ToList();
            var percentages = new double[] { 25, 50, 75 };
            var percentiles = allIntakes.PercentilesWithSamplingWeights(allWeights, percentages);

            ExposureSummaryRecords = new List<ExposureSummaryRecord> {
                new ExposureSummaryRecord() {
                    Description = "All exposures (including zeros)",
                    NumberofObservations = allIntakes.Count(),
                    Mean = dietaryIndividualDayExposures.Sum(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight) / allWeights.Sum(),
                    Minimum = allIntakes.Any() ? allIntakes.Min() : double.NaN,
                    Maximum = allIntakes.Any() ? allIntakes.Max() : double.NaN,
                    LowerQuartile = percentiles[0],
                    Median = percentiles[1],
                    UpperQuartile = percentiles[2],
                }
            };

            if (positiveIntakes.Any()) {
                percentiles = positiveIntakes.PercentilesWithSamplingWeights(positiveWeights, percentages);
                ExposureSummaryRecords.Add(new ExposureSummaryRecord() {
                    Description = "Positive exposures only",
                    NumberofObservations = positiveIntakes.Count(),
                    Mean = dietaryIndividualDayExposures.Where(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0).Sum(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight) / positiveWeights.Sum(),
                    Minimum = positiveIntakes.Min(),
                    Maximum = positiveIntakes.Max(),
                    LowerQuartile = percentiles[0],
                    Median = percentiles[1],
                    UpperQuartile = percentiles[2],
                });
            }

            ExposureFrequencyRecords = new List<ExposureFrequencyRecord>();
            ExposureFrequencyRecords.Add(new ExposureFrequencyRecord() {
                Description = "Number of observations",
                NumberOfExposures = allIntakes.Count,
                NumberOfPositives = positiveIntakes.Count,
                PercentageOfPositives = 100d * positiveIntakes.Count / allIntakes.Count,
            });
            var numberOfIndividuals = dietaryIndividualDayExposures.Select(c => c.SimulatedIndividualId).Distinct().Count();
            ExposureFrequencyRecords.Add(new ExposureFrequencyRecord() {
                Description = "Number of individuals",
                NumberOfExposures = numberOfIndividuals,
                NumberOfPositives = _frequencies.Skip(1).Sum(),
                PercentageOfPositives = 100d * _frequencies.Skip(1).Sum() / numberOfIndividuals
            });
        }



        private void getFrequencyAmountRelation(
                ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
                IDictionary<Compound, double> relativePotencyFactors,
                IDictionary<Compound, double> membershipProbabilities,
                bool isPerPerson
            ) {
            var intakeAmounts = dietaryIndividualDayIntakes
                .GroupBy(idi => idi.SimulatedIndividualId)
                .Select(g => (
                    amount: g.Where(idi => idi.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0)
                        .Select(a => a.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)),
                    frequency: g.Count(idi => idi.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0),
                    samplingWeight: g.Where(idi => idi.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0).Select(a => a.IndividualSamplingWeight)
                ))
                .ToList();

            var frequencies = intakeAmounts
                .Where(f => f.frequency > 0)
                .Select(f => f.frequency)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            FrequencyAmountRelations = frequencies
                .Select(f => {
                    var result = intakeAmounts.Where(c => c.frequency == f).SelectMany(c => c.amount).ToList();
                    var weights = intakeAmounts.Where(c => c.frequency == f).SelectMany(c => c.samplingWeight).ToList();
                    var percentages = new double[] { 2.5, 25, 50, 75, 97.5 };
                    var percentiles = result.PercentilesWithSamplingWeights(weights, percentages);
                    return new FrequencyAmountRelationRecord() {
                        LowerWisker = percentiles[0],
                        LowerBox = percentiles[1],
                        Median = percentiles[2],
                        UpperBox = percentiles[3],
                        UpperWisker = percentiles[4],
                        Outliers = result.Where(c => c < percentiles[0]).ToList(),
                        NumberOfDays = f,
                    };
                })
                .ToList();
        }
    }
}
