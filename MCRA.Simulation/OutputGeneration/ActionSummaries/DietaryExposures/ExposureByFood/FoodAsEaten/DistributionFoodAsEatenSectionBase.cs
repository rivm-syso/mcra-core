using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class DistributionFoodAsEatenSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;

        public double _lowerPercentage;
        public double _upperPercentage;
        public List<DistributionFoodRecord> Records { get; set; }
        public bool HasOthers { get; set; }
        public UncertainDataPointCollection<string> _contribution = new UncertainDataPointCollection<string>();

        public List<DistributionFoodRecord> SummarizeAcute(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var totalIntake = dietaryIndividualDayIntakes
                .Sum(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight);

            var totalDistributionFoodAsEatenRecords = new List<DistributionFoodRecord>();
            var intakesCount = dietaryIndividualDayIntakes.Count();
            var summaryPercentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();

            var intakesPerFoodsAsEaten = dietaryIndividualDayIntakes
                .Where(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0)
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(i => i.DetailedIntakesPerFood,
                    (i, ipf) => (
                        DietaryIndividualDayIntake: i,
                        IntakePerFood: ipf
                    ))
                .GroupBy(ipf => (ipf.DietaryIndividualDayIntake, ipf.IntakePerFood.FoodAsEaten))
                .Select(g => (
                    FoodAsEaten: g.Key.FoodAsEaten,
                    IntakePerMassUnit: g.Sum(ipf => ipf.IntakePerFood.IntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)),
                    SamplingWeight: g.Key.DietaryIndividualDayIntake.IndividualSamplingWeight,
                    SimulatedIndividualDayId: g.Key.DietaryIndividualDayIntake.SimulatedIndividualDayId,
                    DistinctCompounds: g.SelectMany(c => c.IntakePerFood.IntakesPerCompound).Select(c => c.Compound).Distinct()
                ))
                .ToLookup(c => c.FoodAsEaten);

            var foodsAsEaten = intakesPerFoodsAsEaten.Select(f => f.Key).Distinct().ToList();
            var sumSamplingWeights = dietaryIndividualDayIntakes.Sum(c => c.IndividualSamplingWeight);

            totalDistributionFoodAsEatenRecords = foodsAsEaten
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(food => {
                    var allIntakes = intakesPerFoodsAsEaten[food]
                                .Where(c => c.IntakePerMassUnit > 0)
                                .Select(gr => (
                                    IntakePerMassUnit: gr.IntakePerMassUnit,
                                    SamplingWeight: gr.SamplingWeight,
                                    DistinctCompounds: gr.DistinctCompounds.ToList()
                                ))
                                .ToList();

                    var samplingWeightsZeros = sumSamplingWeights - allIntakes.Sum(c => c.SamplingWeight);

                    var weights = allIntakes
                        .Select(c => c.SamplingWeight)
                        .ToList();

                    var percentilesAll = allIntakes
                        .Select(c => c.IntakePerMassUnit)
                        .PercentilesAdditionalZeros(weights, summaryPercentages, samplingWeightsZeros);

                    var percentiles = allIntakes
                        .Select(c => c.IntakePerMassUnit)
                        .PercentilesWithSamplingWeights(weights, summaryPercentages);

                    var total = allIntakes.Sum(c => c.IntakePerMassUnit * c.SamplingWeight);
                    return new DistributionFoodRecord {
                        FoodCode = food.Code,
                        FoodName = food.Name,
                        Contributions = new List<double>(),
                        Total = total / sumSamplingWeights,
                        Contribution = total / totalIntake,
                        Percentile25 = percentiles[0],
                        Median = percentiles[1],
                        Percentile75 = percentiles[2],
                        Percentile25All = percentilesAll[0],
                        MedianAll = percentilesAll[1],
                        Percentile75All = percentilesAll[2],
                        Mean = total / weights.Sum(),
                        NumberOfSubstances = allIntakes.SelectMany(c => c.DistinctCompounds).Distinct().Count(),
                        FractionPositive = Convert.ToDouble(weights.Count) / Convert.ToDouble(intakesCount),
                        NumberOfIndividualDays = weights.Count,
                    };
                })
                .Where(c => c.Contribution > 0)
                .OrderByDescending(r => r.Contribution)
                .ToList();

            var allIntakeResults = dietaryIndividualDayIntakes
                .Select(g => (
                    IntakePerMassUnit: g.AggregateIntakesPerFood.Sum(ipf => ipf.IntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)),
                    SamplingWeight: g.IndividualSamplingWeight
                ))
                .ToList();

            var otherIntakesPerMassUnit = dietaryIndividualDayIntakes
                .AsParallel()
                .Select(c => (
                    IndividualSamplingWeight: c.IndividualSamplingWeight,
                    IntakePerMassUnit: c.TotalOtherIntakesPerCompound(relativePotencyFactors, membershipProbabilities) / (isPerPerson ? 1 : c.Individual.BodyWeight),
                    DistinctCompounds: c.OtherIntakesPerCompound.Select(d => d.Compound).Distinct()
                ))
                .ToList();

            if (otherIntakesPerMassUnit.Where(c => c.IntakePerMassUnit > 0).Any()) {
                HasOthers = true;

                var allWeights = otherIntakesPerMassUnit
                    .Select(c => c.IndividualSamplingWeight)
                    .ToList();

                var percentilesAll = otherIntakesPerMassUnit
                    .Select(c => c.IntakePerMassUnit)
                    .PercentilesWithSamplingWeights(allWeights, summaryPercentages);

                var weights = otherIntakesPerMassUnit
                    .Where(c => c.IntakePerMassUnit > 0)
                    .Select(c => c.IndividualSamplingWeight)
                    .ToList();

                var percentiles = otherIntakesPerMassUnit
                    .Where(c => c.IntakePerMassUnit > 0)
                    .Select(c => c.IntakePerMassUnit)
                    .PercentilesWithSamplingWeights(weights, summaryPercentages);

                var total = otherIntakesPerMassUnit.Sum(c => c.IndividualSamplingWeight * c.IntakePerMassUnit);
                totalDistributionFoodAsEatenRecords.Add(new DistributionFoodRecord {
                    FoodCode = "Others",
                    FoodName = "Others",
                    Contributions = new List<double>(),
                    Total = total / sumSamplingWeights,
                    Contribution = total / totalIntake,
                    Percentile25 = percentiles[0],
                    Median = percentiles[1],
                    Percentile75 = percentiles[2],
                    Percentile25All = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    Percentile75All = percentilesAll[2],
                    Mean = total / weights.Sum(),
                    FractionPositive = Convert.ToDouble(weights.Count) / Convert.ToDouble(intakesCount),
                    NumberOfIndividualDays = weights.Count,
                    NumberOfSubstances = otherIntakesPerMassUnit.SelectMany(c => c.DistinctCompounds).Distinct().Count(),
                });
            }

            if (allIntakeResults.Where(g => g.IntakePerMassUnit > 0).Count() > 0) {
                HasOthers = true;

                var allWeights = allIntakeResults
                    .Select(c => c.SamplingWeight)
                    .ToList();

                var percentilesAll = allIntakeResults
                    .Select(a => a.IntakePerMassUnit)
                    .PercentilesWithSamplingWeights(allWeights, summaryPercentages);

                var weights = allIntakeResults.Where(c => c.IntakePerMassUnit > 0)
                    .Select(w => w.SamplingWeight)
                    .ToList();

                var percentiles = allIntakeResults.Where(c => c.IntakePerMassUnit > 0)
                    .Select(c => c.IntakePerMassUnit)
                    .PercentilesWithSamplingWeights(weights, summaryPercentages);

                var total = allIntakeResults.Sum(c => c.IntakePerMassUnit * c.SamplingWeight);
                totalDistributionFoodAsEatenRecords.Add(new DistributionFoodRecord {
                    FoodCode = "Others",
                    FoodName = "Others",
                    Total = total / sumSamplingWeights,
                    Contributions = new List<double>(),
                    Contribution = total / totalIntake,
                    Percentile25 = percentiles[0],
                    Median = percentiles[1],
                    Percentile75 = percentiles[2],
                    Percentile25All = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    Percentile75All = percentilesAll[2],
                    Mean = total / weights.Sum(),
                    FractionPositive = Convert.ToDouble(weights.Count) / Convert.ToDouble(intakesCount),
                    NumberOfIndividualDays = weights.Count,
                });
            }

            _contribution.XValues = totalDistributionFoodAsEatenRecords.Select(t => t.FoodName);
            _contribution.ReferenceValues = totalDistributionFoodAsEatenRecords.Select(t => t.Contribution);

            totalDistributionFoodAsEatenRecords.TrimExcess();
            return totalDistributionFoodAsEatenRecords;
        }

        public List<DistributionFoodRecord> SummarizeChronic(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var summaryPercentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();

            var individualIds = dietaryIndividualDayIntakes
                .Select(c => c.SimulatedIndividualId).Distinct()
                .ToList();
            var individualDayCountLookup = dietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividualId)
                .ToDictionary(c => c.Key, c=> c.Count());

            var intakesCount = dietaryIndividualDayIntakes.Count();
            var totalIntakes = dietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividualId)
                .Select(c => c.Sum(i => i.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * i.IndividualSamplingWeight) / c.Count())
                .Sum();

            var totalDistributionFoodAsEatenRecords = new List<DistributionFoodRecord>();
            var intakesPerFoodsAsEaten = dietaryIndividualDayIntakes
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(i => i.DetailedIntakesPerFood,
                    (i, ipf) => (
                        SimulatedIndividualId: i.SimulatedIndividualId,
                        DietaryIndividualDayIntake: i,
                        IntakePerFood: ipf
                    ))
                .GroupBy(ipf => (ipf.DietaryIndividualDayIntake, ipf.IntakePerFood.FoodAsEaten))
                .Select(g => (
                    SimulatedIndividualId: g.First().SimulatedIndividualId,
                    NumberOfDaysInSurvey: individualDayCountLookup[g.First().SimulatedIndividualId],
                    FoodAsEaten: g.Key.FoodAsEaten,
                    IntakePerMassUnit: g.Sum(ipf => ipf.IntakePerFood.IntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)),
                    SamplingWeight: g.Key.DietaryIndividualDayIntake.IndividualSamplingWeight,
                    DistinctCompounds: g.SelectMany(c => c.IntakePerFood.IntakesPerCompound).Select(c => c.Compound).Distinct()
                ))
                .GroupBy(ipf => (ipf.SimulatedIndividualId, ipf.FoodAsEaten))
                .Select(g => (
                    FoodAsEaten: g.Key.FoodAsEaten,
                    IntakePerMassUnit: g.Sum(ipf => ipf.IntakePerMassUnit) / g.First().NumberOfDaysInSurvey,
                    SamplingWeight: g.First().SamplingWeight,
                    SimulatedIndividualId: g.First().SimulatedIndividualId,
                    DistinctCompounds: g.SelectMany(c => c.DistinctCompounds).Distinct()
                ))
                .ToLookup(c => c.FoodAsEaten);

            var foodsAsEaten = intakesPerFoodsAsEaten.Select(f => f.Key).Distinct().ToList();

            var sumSamplingWeights = dietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividualId)
                .Sum(c => c.First().IndividualSamplingWeight);

            totalDistributionFoodAsEatenRecords = foodsAsEaten
                //.AsParallel()
                //.WithCancellation(cancelToken)
                .Select(food => {
                    var allIntakes = intakesPerFoodsAsEaten[food]
                        .Where(c => c.IntakePerMassUnit > 0)
                        .Select(c => (
                            IntakePerMassUnit: c.IntakePerMassUnit,
                            SamplingWeight: c.SamplingWeight,
                            DistinctCompounds: c.DistinctCompounds.ToList()
                        ))
                        .ToList();

                    var samplingWeightsZeros = sumSamplingWeights - allIntakes.Sum(c => c.SamplingWeight);

                    var weights = allIntakes
                        .Select(c => c.SamplingWeight)
                        .ToList();

                    var percentilesAll = allIntakes
                        .Select(c => c.IntakePerMassUnit)
                        .PercentilesAdditionalZeros(weights, summaryPercentages, samplingWeightsZeros);

                    var percentiles = allIntakes
                        .Select(c => c.IntakePerMassUnit)
                        .PercentilesWithSamplingWeights(weights, summaryPercentages);

                    var total = allIntakes.Sum(c => c.IntakePerMassUnit * c.SamplingWeight);

                    return new DistributionFoodRecord {
                        FoodCode = food.Code,
                        FoodName = food.Name,
                        Contributions = new List<double>(),
                        Total = total / sumSamplingWeights,
                        Contribution = total / totalIntakes,
                        Percentile25 = percentiles[0],
                        Median = percentiles[1],
                        Percentile75 = percentiles[2],
                        Percentile25All = percentilesAll[0],
                        MedianAll = percentilesAll[1],
                        Percentile75All = percentilesAll[2],
                        Mean = total / weights.Sum(),
                        FractionPositive = Convert.ToDouble(allIntakes.Count) / Convert.ToDouble(individualIds.Count),
                        NumberOfSubstances = allIntakes.SelectMany(c => c.DistinctCompounds).Distinct().Count(),
                        NumberOfIndividualDays = weights.Count,
                    };
                })
                .Where(c => c.Contribution > 0)
                .OrderByDescending(r => r.Contribution)
                .ToList();

            var allIntakeResults = dietaryIndividualDayIntakes
                .AsParallel()
                .Select(g => (
                    aggregateIntakes: g.AggregateIntakesPerFood,
                    samplingWeight: g.IndividualSamplingWeight,
                    bodyWeight: g.Individual.BodyWeight,
                    simulatedIndividualid: g.SimulatedIndividualId
                ))
                .GroupBy(gr => gr.simulatedIndividualid)
                .Select(c => {
                    var aggregateIntakes = c.SelectMany(a => a.aggregateIntakes).ToList();
                    return (
                        IntakePerMassUnit: aggregateIntakes
                            .Sum(i => i.Intake(relativePotencyFactors, membershipProbabilities)) / (isPerPerson ? 1 : c.First().bodyWeight) / c.Count(),
                        SamplingWeight: c.First().samplingWeight
                    );
                })
                .ToList();

            var otherIntakes = dietaryIndividualDayIntakes
                .AsParallel()
                .GroupBy(c => c.SimulatedIndividualId)
                .Select(c => (
                    IndividualSamplingWeight: c.First().IndividualSamplingWeight,
                    IntakePerMassUnit: c.Sum(s => s.TotalOtherIntakesPerCompound(relativePotencyFactors, membershipProbabilities)) / c.Count() / (isPerPerson ? 1 : c.First().Individual.BodyWeight),
                    NumberOfCompounds: c.SelectMany(s => s.OtherIntakesPerCompound.Select(r => r?.Compound)).Distinct().Count()
                ))
                .ToList();

            if (otherIntakes.Where(c => c.IntakePerMassUnit > 0).Any()) {
                HasOthers = true;

                var allWeights = otherIntakes
                    .Select(c => c.IndividualSamplingWeight)
                    .ToList();

                var percentilesAll = otherIntakes
                    .Select(a => a.IntakePerMassUnit)
                    .PercentilesWithSamplingWeights(allWeights, summaryPercentages);

                var weights = otherIntakes
                    .Where(c => c.IntakePerMassUnit > 0)
                    .Select(c => c.IndividualSamplingWeight)
                    .ToList();

                var percentiles = otherIntakes
                    .Where(c => c.IntakePerMassUnit > 0)
                    .Select(c => c.IntakePerMassUnit)
                    .PercentilesWithSamplingWeights(weights, summaryPercentages);

                var total = otherIntakes.Sum(i => i.IndividualSamplingWeight * i.IntakePerMassUnit);
                totalDistributionFoodAsEatenRecords.Add(new DistributionFoodRecord {
                    FoodCode = "Others",
                    FoodName = "Others",
                    Contributions = new List<double>(),
                    Total = total / sumSamplingWeights,
                    Contribution = total / totalIntakes,
                    Percentile25 = percentiles[0],
                    Median = percentiles[1],
                    Percentile75 = percentiles[2],
                    Percentile25All = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    Percentile75All = percentilesAll[2],
                    Mean = total / weights.Sum(),
                    FractionPositive = Convert.ToDouble(weights.Count) / Convert.ToDouble(individualIds.Count),
                    NumberOfIndividualDays = weights.Count,
                    NumberOfSubstances = otherIntakes.First().NumberOfCompounds,
                });
            }

            if (allIntakeResults.Where(c => c.IntakePerMassUnit > 0).Count() > 0) {
                HasOthers = true;
                var allWeights = allIntakeResults
                    .Select(c => c.SamplingWeight)
                    .ToList();
                var percentilesAll = allIntakeResults
                    .Select(a => a.IntakePerMassUnit)
                    .PercentilesWithSamplingWeights(allWeights, summaryPercentages);

                var weights = allIntakeResults
                    .Where(c => c.IntakePerMassUnit > 0)
                    .Select(w => w.SamplingWeight)
                    .ToList();

                var percentiles = allIntakeResults
                    .Where(c => c.IntakePerMassUnit > 0)
                    .Select(c => c.IntakePerMassUnit)
                    .PercentilesWithSamplingWeights(weights, summaryPercentages);

                var total = allIntakeResults.Sum(c => c.IntakePerMassUnit * c.SamplingWeight);
                totalDistributionFoodAsEatenRecords.Add(new DistributionFoodRecord {
                    FoodCode = "Others",
                    FoodName = "Others",
                    Contributions = new List<double>(),
                    Total = total / sumSamplingWeights,
                    Contribution = total / totalIntakes,
                    Percentile25 = percentiles[0],
                    Median = percentiles[1],
                    Percentile75 = percentiles[2],
                    Percentile25All = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    Percentile75All = percentilesAll[2],
                    Mean = total / weights.Sum(),
                    FractionPositive = Convert.ToDouble(weights.Count) / Convert.ToDouble(intakesCount),
                    NumberOfIndividualDays = weights.Count,
                });
            }

            _contribution.XValues = totalDistributionFoodAsEatenRecords.Select(t => t.FoodName);
            _contribution.ReferenceValues = totalDistributionFoodAsEatenRecords.Select(t => t.Contribution);

            totalDistributionFoodAsEatenRecords.TrimExcess();
            return totalDistributionFoodAsEatenRecords;
        }

        public List<DistributionFoodRecord> SummarizeUncertaintyAcute(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var totalIntakes = dietaryIndividualDayIntakes.Sum(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson));
            var distributionFoodAsEatenRecords = new List<DistributionFoodRecord>();
            var intakesCount = dietaryIndividualDayIntakes.Count();
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();

            var intakesPerFoodsAsEaten = dietaryIndividualDayIntakes
                .Where(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0)
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(i => i.DetailedIntakesPerFood,
                    (i, ipf) => (
                        DietaryIndividualDayIntake: i,
                        IntakePerFood: ipf
                    ))
                .GroupBy(ipf => (ipf.DietaryIndividualDayIntake, ipf.IntakePerFood.FoodAsEaten))
                .Select(g => (
                    FoodAsEaten: g.Key.FoodAsEaten,
                    IntakePerMassUnit: g.Sum(ipf => ipf.IntakePerFood.IntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)),
                    SamplingWeight: g.Key.DietaryIndividualDayIntake.IndividualSamplingWeight,
                    SimulatedIndividualDayId: g.Key.DietaryIndividualDayIntake.SimulatedIndividualDayId
                ))
                .ToLookup(c => c.FoodAsEaten);

            var foodsAsEaten = intakesPerFoodsAsEaten.Select(f => f.Key).Distinct().ToList();
            var sumSamplingWeights = dietaryIndividualDayIntakes.Sum(c => c.IndividualSamplingWeight);

            distributionFoodAsEatenRecords = foodsAsEaten
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(food => {
                    var allIntakes = intakesPerFoodsAsEaten[food]
                        .GroupBy(gr => gr.SimulatedIndividualDayId)
                        .Select(gr => (
                            IntakePerMassUnit: gr.Sum(s => s.IntakePerMassUnit),
                            SamplingWeight: gr.First().SamplingWeight
                        ))
                        .ToList();

                    var total = allIntakes.Sum(c => c.IntakePerMassUnit * c.SamplingWeight);
                    return new DistributionFoodRecord {
                        FoodCode = food.Code,
                        FoodName = food.Name,
                        Contribution = total / totalIntakes,
                    };
                })
                .Where(c => c.Contribution > 0)
                .OrderByDescending(r => r.Contribution)
                .ToList();

            var allIntakeResults = dietaryIndividualDayIntakes
                .AsParallel()
                .Select(g => (
                    IntakePerMassUnit: g.AggregateIntakesPerFood.Sum(ipf => ipf.IntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)),
                    SamplingWeight: g.IndividualSamplingWeight
                ))
                .ToList();

            var otherIntakes = dietaryIndividualDayIntakes
                 .Select(c => (
                     IndividualSamplingWeight: c.IndividualSamplingWeight,
                     IntakePerMassUnit: c.TotalOtherIntakesPerCompound(relativePotencyFactors, membershipProbabilities) / (isPerPerson ? 1 : c.Individual.BodyWeight)
                 ))
                 .ToList();

            if (otherIntakes.Where(c => c.IntakePerMassUnit > 0).Any()) {
                HasOthers = true;
                var total = otherIntakes.Sum(c => c.IndividualSamplingWeight * c.IntakePerMassUnit);
                distributionFoodAsEatenRecords.Add(new DistributionFoodRecord {
                    FoodCode = "Others",
                    FoodName = "Others",
                    Contribution = total / totalIntakes,
                });
            }

            if (allIntakeResults.Where(g => g.IntakePerMassUnit > 0).Count() > 0) {
                HasOthers = true;
                var total = allIntakeResults.Sum(c => c.IntakePerMassUnit * c.SamplingWeight);
                distributionFoodAsEatenRecords.Add(new DistributionFoodRecord {
                    FoodCode = "Others",
                    FoodName = "Others",
                    Contribution = total / totalIntakes,
                });
            }
            return distributionFoodAsEatenRecords;
        }

        public List<DistributionFoodRecord> SummarizeUncertaintyChronic(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var totalIntake = dietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividualId)
                .Select(c => c.Sum(i => i.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * i.IndividualSamplingWeight) / c.Count())
                .Sum();

            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var individualDayCountLookup = dietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividualId)
                .ToDictionary(c => c.Key, c => c.Count());
            var distributionFoodAsEatenRecords = new List<DistributionFoodRecord>();
            var intakesPerFoodsAsEaten = dietaryIndividualDayIntakes
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(i => i.DetailedIntakesPerFood,
                    (i, ipf) => (
                        SimulatedIndividualId: i.SimulatedIndividualId,
                        DietaryIndividualDayIntake: i,
                        IntakePerFood: ipf

                    ))
                .GroupBy(ipf => (ipf.DietaryIndividualDayIntake, ipf.IntakePerFood.FoodAsEaten))
                .Select(g => (
                    SimulatedIndividualId: g.First().SimulatedIndividualId,
                    NumberOfDaysInSurvey: individualDayCountLookup[g.First().SimulatedIndividualId],
                    FoodAsEaten: g.Key.FoodAsEaten,
                    IntakePerMassUnit: g.Sum(ipf => ipf.IntakePerFood.IntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)),
                    SamplingWeight: g.Key.DietaryIndividualDayIntake.IndividualSamplingWeight
                ))
                .GroupBy(ipf => (ipf.SimulatedIndividualId, ipf.FoodAsEaten))
                .Select(g => (
                    FoodAsEaten: g.Key.FoodAsEaten,
                    IntakePerMassUnit: g.Sum(ipf => ipf.IntakePerMassUnit) / g.First().NumberOfDaysInSurvey,
                    SamplingWeight: g.First().SamplingWeight,
                    SimulatedIndividualId: g.First().SimulatedIndividualId
                ))
                .ToLookup(c => c.FoodAsEaten);

            var foodsAsEaten = intakesPerFoodsAsEaten.Select(f => f.Key).Distinct().ToList();
            var sumSamplingWeights = dietaryIndividualDayIntakes.GroupBy(c => c.SimulatedIndividualId)
                .Sum(c => c.First().IndividualSamplingWeight);

            distributionFoodAsEatenRecords = foodsAsEaten
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(food => {
                    var allIntakes = intakesPerFoodsAsEaten[food]
                        .GroupBy(c => c.SimulatedIndividualId)
                        .Select(c => (
                            IntakePerMassUnit: c.Sum(s => s.IntakePerMassUnit),
                            SamplingWeight: c.First().SamplingWeight
                        ))
                        .ToList();

                    var total = allIntakes.Sum(c => c.IntakePerMassUnit * c.SamplingWeight);
                    return new DistributionFoodRecord {
                        FoodCode = food.Code,
                        FoodName = food.Name,
                        Contribution = total / totalIntake,
                    };
                })
                .Where(c => c.Contribution > 0)
                .OrderByDescending(r => r.Contribution)
                .ToList();

            var allIntakeResults = dietaryIndividualDayIntakes
                .AsParallel()
                .Select(g => (
                    aggregateIntakes: g.AggregateIntakesPerFood,
                    samplingWeight: g.IndividualSamplingWeight,
                    bodyWeight: g.Individual.BodyWeight,
                    simulatedIndividualid: g.SimulatedIndividualId
                ))
                .GroupBy(gr => gr.simulatedIndividualid)
                .Select(c => {
                    var aggregateIntakes = c.SelectMany(a => a.aggregateIntakes).ToList();
                    return (
                        IntakePerMassUnit: aggregateIntakes.Sum(i => i.Intake(relativePotencyFactors, membershipProbabilities)) / (isPerPerson ? 1 : c.First().bodyWeight) / c.Count(),
                        SamplingWeight: c.First().samplingWeight
                    );
                })
                .ToList();

            var otherIntakes = dietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividualId)
                .Select(c => (
                    IndividualSamplingWeight: c.First().IndividualSamplingWeight,
                    IntakePerMassUnit: c.Sum(s => s.TotalOtherIntakesPerCompound(relativePotencyFactors, membershipProbabilities)) / c.Count() / (isPerPerson ? 1 : c.First().Individual.BodyWeight)
                ))
                .ToList();

            if (otherIntakes.Where(c => c.IntakePerMassUnit > 0).Any()) {
                HasOthers = true;
                var total = otherIntakes.Sum(i => i.IndividualSamplingWeight * i.IntakePerMassUnit);
                distributionFoodAsEatenRecords.Add(new DistributionFoodRecord {
                    FoodCode = "Others",
                    FoodName = "Others",
                    Contribution = total / totalIntake,
                });
            }

            if (allIntakeResults.Where(c => c.IntakePerMassUnit > 0).Count() > 0) {
                HasOthers = true;
                var total = allIntakeResults.Sum(c => c.IntakePerMassUnit * c.SamplingWeight);
                distributionFoodAsEatenRecords.Add(new DistributionFoodRecord {
                    FoodCode = "Others",
                    FoodName = "Others",
                    Contribution = total / totalIntake,
                });
            }
            return distributionFoodAsEatenRecords;
        }

        protected void updateContributions(List<DistributionFoodRecord> distributionFoodAsEatenRecords) {
            foreach (var record in Records) {
                var contribution = distributionFoodAsEatenRecords
                    .FirstOrDefault(c => c.FoodCode == record.FoodCode)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
