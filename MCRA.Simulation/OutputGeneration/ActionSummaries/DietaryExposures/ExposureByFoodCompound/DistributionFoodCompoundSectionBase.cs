using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Utils.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MCRA.Simulation.OutputGeneration {
    public class DistributionFoodCompoundSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;

        public List<DistributionFoodCompoundRecord> Records { get; set; }

        public double LowerPercentage { get; set; }
        public double UpperPercentage { get; set; }

        public List<DistributionFoodCompoundRecord> SummarizeAcute(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Food> modelledFoods,
            ICollection<Compound> substances,
            bool isPerPerson
        ) {
            var numberOfIntakes = (double)dietaryIndividualDayIntakes.Count;
            var sumSamplingWeights = dietaryIndividualDayIntakes.Sum(c => c.IndividualSamplingWeight);
            var summaryPercentages = new double[] { LowerPercentage, 50, UpperPercentage };
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var totalIntake = relativePotencyFactors != null
                ? dietaryIndividualDayIntakes.Sum(r => r.IndividualSamplingWeight * r.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                : double.NaN;

            // Compute total exposures for each individual, food, and substance.
            var foodSubstanceIntakes = collectFoodSubstanceIndividualDayIntakes(dietaryIndividualDayIntakes, cancelToken);

            // With the resulting data from the previous step, create the output records
            var result = foodSubstanceIntakes
                .AsParallel()
                .WithDegreeOfParallelism(100)
                .WithCancellation(cancelToken)
                .Select(fc => {
                    var positiveIntakes = isPerPerson
                        ? fc.Select(r => r.Intake).ToList()
                        : fc.Select(r => r.Intake / r.IndividualDay.Individual.BodyWeight).ToList();

                    var samplingWeightsPositiveIntakes = fc
                        .Select(r => r.IndividualDay.IndividualSamplingWeight)
                        .ToList();

                    var sumSamplingWeightsPositives = samplingWeightsPositiveIntakes.Sum();
                    var samplingWeightsZeros = sumSamplingWeights - sumSamplingWeightsPositives;

                    var percentilesAll = positiveIntakes
                        .PercentilesAdditionalZeros(samplingWeightsPositiveIntakes, summaryPercentages, samplingWeightsZeros);

                    var percentiles = positiveIntakes
                        .PercentilesWithSamplingWeights(samplingWeightsPositiveIntakes, summaryPercentages);

                    var total = positiveIntakes.Zip(samplingWeightsPositiveIntakes, (i, w) => w * i).Sum();

                    var cumulativeTotal = total;

                    // Compute cumulative total
                    if (relativePotencyFactors != null) {
                        cumulativeTotal *= relativePotencyFactors[fc.Key.Compound];
                    }

                    if (membershipProbabilities != null) {
                        cumulativeTotal *= membershipProbabilities[fc.Key.Compound];
                    }

                    return new DistributionFoodCompoundRecord {
                        FoodName = fc.Key.Food.Name,
                        FoodCode = fc.Key.Food.Code,
                        CompoundName = fc.Key.Compound.Name,
                        CompoundCode = fc.Key.Compound.Code,
                        Contributions = new List<double>(),
                        Contribution = cumulativeTotal / totalIntake,
                        MeanPositives = total / sumSamplingWeightsPositives,
                        MedianPositives = percentiles[1],
                        LowerPercentilePositives = percentiles[0],
                        UpperPercentilePositives = percentiles[2],
                        MeanAll = total / sumSamplingWeights,
                        MedianAll = percentilesAll[1],
                        LowerPercentileAll = percentilesAll[0],
                        UpperPercentileAll = percentilesAll[2],
                        FractionPositives = positiveIntakes.Count / numberOfIntakes,
                        NumberOfPositives = positiveIntakes.Count,
                    };
                })
                .ToList();

            if (modelledFoods != null) {
                var combinations = result.Select(c => c.CompoundCode + c.FoodCode).ToList();
                foreach (var substance in substances) {
                    foreach (var food in modelledFoods) {
                        if (!combinations.Contains(substance.Code + food.Code)) {
                            result.Add(new DistributionFoodCompoundRecord() {
                                FoodName = food.Name,
                                FoodCode = food.Code,
                                CompoundName = substance.Name,
                                CompoundCode = substance.Code,
                                Contributions = new List<double>(),
                            });
                        }
                    }
                }
            }
            return result;
        }

        public List<DistributionFoodCompoundRecord> SummarizeChronic(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();

            var summaryPercentages = new double[] { LowerPercentage, 50, UpperPercentage };

            var groupedIndividualDayIntakes = dietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividualId);
            var surveyDayCounts = groupedIndividualDayIntakes.ToDictionary(r => r.Key, r => r.Count());
            var numberOfIndividuals = groupedIndividualDayIntakes.Count();
            var sumSamplingWeights = groupedIndividualDayIntakes.Sum(c => c.First().IndividualSamplingWeight);

            var totalIntake = relativePotencyFactors != null
                ? groupedIndividualDayIntakes
                    .Select(c => c.Sum(i => i.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * i.IndividualSamplingWeight) / c.Count())
                    .Sum()
                : double.NaN;

            // Compute total exposures for each individual, food, and substance.
            var foodSubstanceIntakes = collectFoodSubstanceIndividualDayIntakes(dietaryIndividualDayIntakes, cancelToken);

            // With the resulting data from the previous step, create the output records
            var result = foodSubstanceIntakes
                .AsParallel()
                .WithDegreeOfParallelism(100)
                .WithCancellation(cancelToken)
                .Select(fc => {
                    var individualExposures = fc
                        .GroupBy(r => r.IndividualDay.SimulatedIndividualId)
                        .Select(g => (
                            g.First().IndividualDay.Individual,
                            AverageExposure: g.Select(r => r.Intake).Sum() / surveyDayCounts[g.Key],
                            g.First().IndividualDay.IndividualSamplingWeight
                        ))
                        .ToList();

                    var positiveIntakes = isPerPerson
                        ? individualExposures.Select(r => r.AverageExposure).ToList()
                        : individualExposures.Select(r => r.AverageExposure / r.Individual.BodyWeight).ToList();

                    var samplingWeightsPositiveIntakes = individualExposures
                        .Select(r => r.IndividualSamplingWeight)
                        .ToList();

                    var sumSamplingWeightsPositives = samplingWeightsPositiveIntakes.Sum();
                    var samplingWeightsZeros = sumSamplingWeights - sumSamplingWeightsPositives;

                    var percentilesAll = positiveIntakes
                        .PercentilesAdditionalZeros(samplingWeightsPositiveIntakes, summaryPercentages, samplingWeightsZeros);

                    var percentiles = positiveIntakes
                        .PercentilesWithSamplingWeights(samplingWeightsPositiveIntakes, summaryPercentages);

                    var total = positiveIntakes.Zip(samplingWeightsPositiveIntakes, (i, w) => w * i).Sum();

                    var cumulativeTotal = total;

                    // Compute cumulative total
                    if (relativePotencyFactors != null) {
                        cumulativeTotal *= relativePotencyFactors[fc.Key.Compound];
                    }

                    if (membershipProbabilities != null) {
                        cumulativeTotal *= membershipProbabilities[fc.Key.Compound];
                    }

                    return new DistributionFoodCompoundRecord {
                        FoodName = fc.Key.Food.Name,
                        FoodCode = fc.Key.Food.Code,
                        CompoundName = fc.Key.Compound.Name,
                        CompoundCode = fc.Key.Compound.Code,
                        Contributions = new List<double>(),
                        Contribution = cumulativeTotal / totalIntake,
                        MeanPositives = total / sumSamplingWeightsPositives,
                        MedianPositives = percentiles[1],
                        LowerPercentilePositives = percentiles[0],
                        UpperPercentilePositives = percentiles[2],
                        MeanAll = total / sumSamplingWeights,
                        MedianAll = percentilesAll[1],
                        LowerPercentileAll = percentilesAll[0],
                        UpperPercentileAll = percentilesAll[2],
                        FractionPositives = Convert.ToDouble(positiveIntakes.Count) / Convert.ToDouble(numberOfIndividuals),
                        NumberOfPositives = positiveIntakes.Count,
                    };
                })
                .ToList();

            return result;
        }

        private static List<
            IGrouping<(Food Food, Compound Compound),
            (DietaryIndividualDayIntake IndividualDay, Food Food, Compound Compound, double Intake)
            >> collectFoodSubstanceIndividualDayIntakes(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            CancellationToken cancelToken
        ) {
            // Create stub-food for other intakes per substance
            var othersFood = new Food() { Code = "Others", Name = "Others" };

            var result = dietaryIndividualDayIntakes
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(idi => {
                    var foodSubstanceExposures = new Dictionary<(Food, Compound), double>();
                    foreach (var itpf in idi.IntakesPerFood) {
                        foreach (var itpc in itpf.IntakesPerCompound) {
                            var exposure = itpc.Exposure;
                            var key = (itpf.FoodAsMeasured, itpc.Compound);
                            if (!foodSubstanceExposures.ContainsKey(key)) {
                                foodSubstanceExposures.Add(key, exposure);
                            } else {
                                foodSubstanceExposures[key] += exposure;
                            }
                        }
                    }
                    foreach (var itpc in idi.OtherIntakesPerCompound) {
                        var exposure = itpc.Exposure;
                        foodSubstanceExposures[(othersFood, itpc.Compound)] = exposure;
                    }
                    return foodSubstanceExposures
                        .Select(r => (
                            IndividualDay: idi,
                            Food: r.Key.Item1,
                            Compound: r.Key.Item2,
                            Intake: r.Value
                        ));
                })
                .Where(r => r.Intake > 0)
                .GroupBy(g => (g.Food, g.Compound))
                .ToList();
            return result;
        }

        /// <summary>
        /// This is the correct way to do it. The list of Records of the nominal run should contain all combinations.
        /// For combinations that are not found in the bootstrap, zeros's are added to Contributions.
        /// So at the end, the length of Contributions is equal for all records. 
        /// If you do it the opposite around e.g. a foreach over bootstrapped records, a combination that is not available
        /// in the nominal run should be added (see example => D) with the corresponding contribution, when found, the Contributions 
        /// should be updated (see example A) and all remaining records should be updated with a zero (see example => B and C).
        /// Remaining records are the records found in the nominal run and NOT in de bootstrapped run.
        /// E.g. a nominal run with A, B and C; a bootstrapped run with A and D
        /// </summary>
        /// <param name="distributionFoodCompoundRecords"></param>
        protected void updateContributions(List<DistributionFoodCompoundRecord> distributionFoodCompoundRecords) {
            foreach (var record in Records) {
                var contribution = distributionFoodCompoundRecords
                    .FirstOrDefault(c => c.CompoundCode == record.CompoundCode && c.FoodCode == record.FoodCode)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
