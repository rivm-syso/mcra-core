using MCRA.Utils.Hierarchies;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class DistributionFoodAsMeasuredSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;

        protected double[] Percentages { get; set; }
        public bool HasOthers { get; set; }

        public List<DistributionFoodRecord> Records { get; set; }

        public List<DistributionFoodRecord> HierarchicalNodes { get; set; }


        /// <summary>
        /// Returns whether this section has hierarchical data or not.
        /// </summary>
        public bool HasHierarchicalData {
            get {
                return HierarchicalNodes?.Count > 0;
            }
        }

        public void SummarizeAcute(
            ICollection<Food> allFoods,
            ICollection<Food> modelledFoods,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var totalIntake = dietaryIndividualDayIntakes
                .Sum(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight);
            var intakesCount = dietaryIndividualDayIntakes.Count;
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();

            var sumSamplingWeights = dietaryIndividualDayIntakes.Sum(c => c.IndividualSamplingWeight);
            Records = dietaryIndividualDayIntakes
                .Where(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0)
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(i => i.IntakesPerFood,
                    (i, ipf) => (
                        DietaryIndividualDayIntake: i,
                        IntakePerFood: ipf
                    ))
                .GroupBy(ipf => (
                    ipf.DietaryIndividualDayIntake,
                    ipf.IntakePerFood.FoodAsMeasured
                ))
                .Select(g => (
                    FoodAsMeasured: g.Key.FoodAsMeasured,
                    IntakePerMassUnit: g.Sum(ipf => ipf.IntakePerFood.IntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)),
                    SamplingWeight: g.Key.DietaryIndividualDayIntake.IndividualSamplingWeight,
                    SimulatedIndividualDayId: g.Key.DietaryIndividualDayIntake.SimulatedIndividualDayId,
                    DistinctCompounds: g.SelectMany(ipf => ipf.IntakePerFood.IntakesPerCompound.Select(r => r.Compound)).Distinct()
                ))
                .GroupBy(g => g.FoodAsMeasured)
                .Select(g => {
                    var allIntakes = g.Where(c => c.IntakePerMassUnit > 0)
                       .Select(gr => (
                           SimulatedIndividualDayId: gr.SimulatedIndividualDayId,
                           IntakePerMassUnit: gr.IntakePerMassUnit,
                           SamplingWeight: gr.SamplingWeight,
                           DistinctCompounds: gr.DistinctCompounds.Distinct().ToList()
                       ))
                       .ToList();

                    var samplingWeightsZeros = sumSamplingWeights - allIntakes.Sum(c => c.SamplingWeight);
                    var weights = allIntakes
                       .Select(c => c.SamplingWeight).ToList();

                    var percentilesAll = allIntakes
                       .Select(c => c.IntakePerMassUnit)
                       .PercentilesAdditionalZeros(weights, Percentages, samplingWeightsZeros);

                    var percentiles = allIntakes
                       .Select(ipf => ipf.IntakePerMassUnit)
                       .PercentilesWithSamplingWeights(weights, Percentages);

                    var total = allIntakes.Sum(ipf => ipf.IntakePerMassUnit * ipf.SamplingWeight);
                    return new DistributionFoodRecord {
                        __Id = g.Key.Code,
                        __IdParent = g.Key.Parent?.Code,
                        __IsSummaryRecord = false,
                        FoodCode = g.Key.Code,
                        FoodName = g.Key.Name,
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
                        NumberOfSubstances = allIntakes.SelectMany(c => c.DistinctCompounds).Distinct().Count(),
                        NumberOfIndividualDays = weights.Count,
                    };
                })
                 .Where(c => c.Contribution > 0)
                 .OrderByDescending(r => r.Contribution)
                 .ToList();

            var daysWithOtherIntakes = dietaryIndividualDayIntakes
                .Select(c => (
                    IndividualSamplingWeight: c.IndividualSamplingWeight,
                    IntakePerMassUnit: c.TotalOtherIntakesPerCompound(relativePotencyFactors, membershipProbabilities) / (isPerPerson ? 1 : c.Individual.BodyWeight),
                    NumberOfCompounds: c.OtherIntakesPerCompound?.Select(r => r.Compound).Distinct().Count() ?? 0
                ))
                .ToList();

            if (daysWithOtherIntakes.Any(c => c.IntakePerMassUnit > 0)) {
                HasOthers = true;

                var allWeights = daysWithOtherIntakes
                    .Select(c => c.IndividualSamplingWeight)
                    .ToList();
                var percentilesAll = daysWithOtherIntakes
                    .Select(a => a.IntakePerMassUnit)
                    .PercentilesWithSamplingWeights(allWeights, Percentages);

                var weights = daysWithOtherIntakes
                    .Where(c => c.IntakePerMassUnit > 0)
                    .Select(c => c.IndividualSamplingWeight)
                    .ToList();

                var percentiles = daysWithOtherIntakes
                    .Where(c => c.IntakePerMassUnit > 0)
                    .Select(c => c.IntakePerMassUnit)
                    .PercentilesWithSamplingWeights(weights, Percentages);

                var total = daysWithOtherIntakes.Sum(c => c.IntakePerMassUnit * c.IndividualSamplingWeight);
                Records.Add(new DistributionFoodRecord {
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
                    NumberOfSubstances = daysWithOtherIntakes.First().NumberOfCompounds,
                });
            }

            addMissingModelledFoodRecords(modelledFoods);

            HierarchicalNodes = getHierarchicalRecords(allFoods, dietaryIndividualDayIntakes, Records, cancelToken);
        }

        public void SummarizeChronic(
            ICollection<Food> allFoods,
            ICollection<Food> modelledFoods,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var individualIds = dietaryIndividualDayIntakes
                .Select(c => c.SimulatedIndividualId)
                .Distinct()
                .ToList();
            var individualDayCountLookup = dietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividualId)
                .ToDictionary(c => c.Key, c => c.Count());
            var totalIntake = dietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividualId)
                .Select(c => c.Sum(i => i.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * i.IndividualSamplingWeight) / c.Count())
                .Sum();

            var sumSamplingWeights = dietaryIndividualDayIntakes.GroupBy(c => c.SimulatedIndividualId)
                .Sum(c => c.First().IndividualSamplingWeight);

            Records = dietaryIndividualDayIntakes
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(i => i.IntakesPerFood,
                    (i, ipf) => (
                        SimulatedIndividualId: i.SimulatedIndividualId,
                        DietaryIndividualDayIntake: i,
                        IntakePerFood: ipf
                    ))
                .GroupBy(ipf => (ipf.DietaryIndividualDayIntake, ipf.IntakePerFood.FoodAsMeasured))
                .Select(g => (
                    SimulatedIndividualId: g.First().SimulatedIndividualId,
                    NumberOfDaysInSurvey: individualDayCountLookup[g.First().SimulatedIndividualId],
                    FoodAsMeasured: g.Key.FoodAsMeasured,
                    IntakePerMassUnit: g.Sum(ipf => ipf.IntakePerFood.IntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)),
                    SamplingWeight: g.Key.DietaryIndividualDayIntake.IndividualSamplingWeight,
                    DistinctCompounds: g.SelectMany(ipf => ipf.IntakePerFood.IntakesPerCompound.Select(c => c.Compound)).Distinct()
                ))
                .GroupBy(gr => (gr.FoodAsMeasured, gr.SimulatedIndividualId))
                .Select(c => (
                    FoodAsMeasured: c.Key.FoodAsMeasured,
                    IntakePerMassUnit: c.Sum(ipf => ipf.IntakePerMassUnit) / c.First().NumberOfDaysInSurvey,
                    SamplingWeight: c.First().SamplingWeight,
                    SimulatedIndividualId: c.First().SimulatedIndividualId,
                    DistinctCompounds: c.SelectMany(d => d.DistinctCompounds).Distinct()
                ))
                .GroupBy(g => g.FoodAsMeasured)
                .Select(g => {
                    var allIntakes = g.Where(c => c.IntakePerMassUnit > 0)
                       .Select(gr => (
                           IntakePerMassUnit: gr.IntakePerMassUnit,
                           SamplingWeight: gr.SamplingWeight,
                           DistinctCompounds: gr.DistinctCompounds.Distinct().ToList()
                       ))
                    .ToList();

                    var samplingWeightsZeros = sumSamplingWeights - allIntakes.Sum(c => c.SamplingWeight);

                    var weights = allIntakes
                       .Select(c => c.SamplingWeight)
                       .ToList();

                    var percentilesAll = allIntakes
                       .Select(c => c.IntakePerMassUnit)
                       .PercentilesAdditionalZeros(weights, Percentages, samplingWeightsZeros);

                    var percentiles = allIntakes
                       .Select(ipf => ipf.IntakePerMassUnit)
                       .PercentilesWithSamplingWeights(weights, Percentages);

                    var total = allIntakes.Sum(ipf => ipf.IntakePerMassUnit * ipf.SamplingWeight);
                    return new DistributionFoodRecord {
                        __Id = g.Key.Code,
                        __IdParent = g.Key.Parent?.Code,
                        __IsSummaryRecord = false,
                        FoodCode = g.Key.Code,
                        FoodName = g.Key.Name,
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
                        FractionPositive = Convert.ToDouble(weights.Count) / Convert.ToDouble(individualIds.Count),
                        NumberOfSubstances = allIntakes.SelectMany(c => c.DistinctCompounds).Distinct().Count(),
                        NumberOfIndividualDays = weights.Count,
                    };
                })
                 .Where(c => c.Contribution > 0)
                 .OrderByDescending(r => r.Contribution)
                 .ToList();

            var daysWithOtherIntakes = dietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividualId)
                .Select(c => (
                    Individual: c.Key,
                    IndividualSamplingWeight: c.First().IndividualSamplingWeight,
                    IntakePerMassUnit: c.Sum(s => s.TotalOtherIntakesPerCompound(relativePotencyFactors, membershipProbabilities)) / c.Count() / (isPerPerson ? 1 : c.First().Individual.BodyWeight),
                    NumberOfCompounds: c.SelectMany(s => s.OtherIntakesPerCompound.Select(r => r.Compound)).Distinct().Count()
                ))
                .ToList();

            if (daysWithOtherIntakes.Any(c => c.IntakePerMassUnit > 0)) {
                HasOthers = true;

                var allWeights = daysWithOtherIntakes
                    .Select(c => c.IndividualSamplingWeight)
                    .ToList();

                var percentilesAll = daysWithOtherIntakes
                    .Select(a => a.IntakePerMassUnit)
                    .PercentilesWithSamplingWeights(allWeights, Percentages);

                var otherIntakes = daysWithOtherIntakes
                    .Where(c => c.IntakePerMassUnit > 0)
                    .Select(c => c.IntakePerMassUnit * c.IndividualSamplingWeight)
                    .ToList();

                var weights = daysWithOtherIntakes
                    .Where(c => c.IntakePerMassUnit > 0)
                    .Select(c => c.IndividualSamplingWeight)
                    .ToList();

                var percentiles = daysWithOtherIntakes
                    .Where(c => c.IntakePerMassUnit > 0)
                    .Select(c => c.IntakePerMassUnit)
                    .PercentilesWithSamplingWeights(weights, Percentages);

                var total = daysWithOtherIntakes.Sum(c => c.IntakePerMassUnit * c.IndividualSamplingWeight);
                Records.Add(new DistributionFoodRecord {
                    __IsSummaryRecord = false,
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
                    FractionPositive = Convert.ToDouble(weights.Count) / Convert.ToDouble(daysWithOtherIntakes.Count),
                    NumberOfIndividualDays = weights.Count,
                    NumberOfSubstances = daysWithOtherIntakes.First().NumberOfCompounds,
                });
            }
            Records.TrimExcess();

            addMissingModelledFoodRecords(modelledFoods);

            HierarchicalNodes = getHierarchicalRecords(allFoods, dietaryIndividualDayIntakes, Records, cancelToken);
        }

        public void SummarizeUncertaintyAcute(
            ICollection<Food> allFoods,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var totalIntake = dietaryIndividualDayIntakes
                .Sum(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight);
            var intakesCount = dietaryIndividualDayIntakes.Count;
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();

            var uncertaintyRecords = dietaryIndividualDayIntakes
                .Where(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0)
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(i => i.IntakesPerFood,
                    (i, ipf) => (
                        DietaryIndividualDayIntake: i,
                        IntakePerFood: ipf
                    ))
                .GroupBy(ipf => (
                    ipf.DietaryIndividualDayIntake,
                    ipf.IntakePerFood.FoodAsMeasured
                ))
                .Select(g => (
                    FoodAsMeasured: g.Key.FoodAsMeasured,
                    IntakePerMassUnit: g.Sum(ipf => ipf.IntakePerFood.IntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)),
                    SamplingWeight: g.Key.DietaryIndividualDayIntake.IndividualSamplingWeight
                ))
                .GroupBy(g => g.FoodAsMeasured)
                .Select(g => {
                    var total = g.Sum(ipf => ipf.IntakePerMassUnit * ipf.SamplingWeight);
                    return new DistributionFoodRecord {
                        __Id = g.Key.Code,
                        __IdParent = g.Key.Parent?.Code,
                        __IsSummaryRecord = false,
                        FoodCode = g.Key.Code,
                        FoodName = g.Key.Name,
                        Contribution = total / totalIntake,
                    };
                })
                 .Where(c => c.Contribution > 0)
                 .OrderByDescending(r => r.Contribution)
                 .ToList();

            var daysWithOtherIntakes = dietaryIndividualDayIntakes
                .Select(c => (
                    IndividualSamplingWeight: c.IndividualSamplingWeight,
                    IntakePerMassUnit: c.TotalOtherIntakesPerCompound(relativePotencyFactors, membershipProbabilities) / (isPerPerson ? 1 : c.Individual.BodyWeight)
                ))
                .ToList();

            if (daysWithOtherIntakes.Any(c => c.IntakePerMassUnit > 0)) {
                var total = daysWithOtherIntakes.Sum(c => c.IntakePerMassUnit * c.IndividualSamplingWeight);
                uncertaintyRecords.Add(new DistributionFoodRecord {
                    FoodCode = "Others",
                    FoodName = "Others",
                    Contribution = total / totalIntake,

                });
            }
            updateContributions(uncertaintyRecords);
            var hierarchicalNodeUncertaintyRecords = getHierarchicalRecords(allFoods, dietaryIndividualDayIntakes, uncertaintyRecords, cancelToken);
            updateContributionsHierarchicalNodes(hierarchicalNodeUncertaintyRecords);
        }

        public void SummarizeUncertaintyChronic(
            ICollection<Food> allFoods,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var individualIds = dietaryIndividualDayIntakes.Select(c => c.SimulatedIndividualId).Distinct().ToList();
            var intakesCount = dietaryIndividualDayIntakes.Count;

            var totalIntake = dietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividualId)
                .Select(c => c.Sum(i => i.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * i.IndividualSamplingWeight) / c.Count())
                .Sum();
            var individualDayCountLookup = dietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividualId)
                .ToDictionary(c => c.Key, c => c.Count());
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();

            var uncertaintyRecords = dietaryIndividualDayIntakes
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(i => i.IntakesPerFood,
                    (i, ipf) => (
                        SimulatedIndividualId: i.SimulatedIndividualId,
                        DietaryIndividualDayIntake: i,
                        IntakePerFood: ipf
                    ))
                .GroupBy(ipf => (ipf.DietaryIndividualDayIntake, ipf.IntakePerFood.FoodAsMeasured))
                .Select(g => (
                    SimulatedIndividualId: g.First().SimulatedIndividualId,
                    NumberOfDaysInSurvey: individualDayCountLookup[g.First().SimulatedIndividualId],
                    FoodAsMeasured: g.Key.FoodAsMeasured,
                    IntakePerMassUnit: g.Sum(ipf => ipf.IntakePerFood.IntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)),
                    SamplingWeight: g.Key.DietaryIndividualDayIntake.IndividualSamplingWeight
                ))
                .GroupBy(gr => (gr.FoodAsMeasured, gr.SimulatedIndividualId))
                .Select(c => (
                    FoodAsMeasured: c.Key.FoodAsMeasured,
                    IntakePerMassUnit: c.Sum(ipf => ipf.IntakePerMassUnit) / c.First().NumberOfDaysInSurvey,
                    SamplingWeight: c.First().SamplingWeight
                ))
                .GroupBy(g => g.FoodAsMeasured)
                .Select(g => {
                    var total = g.Sum(ipf => ipf.IntakePerMassUnit * ipf.SamplingWeight);
                    return new DistributionFoodRecord {
                        __Id = g.Key.Code,
                        __IdParent = g.Key.Parent?.Code,
                        __IsSummaryRecord = false,
                        FoodCode = g.Key.Code,
                        FoodName = g.Key.Name,
                        Contribution = total / totalIntake,
                    };
                })
                 .Where(c => c.Contribution > 0)
                 .OrderByDescending(r => r.Contribution)
                 .ToList();

            var daysWithOtherIntakes = dietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividualId)
                .Select(c => (
                    IndividualSamplingWeight: c.First().IndividualSamplingWeight,
                    IntakePerMassUnit: c.Sum(s => s.TotalOtherIntakesPerCompound(relativePotencyFactors, membershipProbabilities)) / c.Count() / (isPerPerson ? 1 : c.First().Individual.BodyWeight)
                ))
                .ToList();

            if (daysWithOtherIntakes.Any(c => c.IntakePerMassUnit > 0)) {
                HasOthers = true;
                var total = daysWithOtherIntakes.Sum(c => c.IntakePerMassUnit * c.IndividualSamplingWeight);
                uncertaintyRecords.Add(new DistributionFoodRecord {
                    FoodCode = "Others",
                    FoodName = "Others",
                    Contribution = total / totalIntake,
                });
            }
            updateContributions(uncertaintyRecords);
            var hierarchicalNodeUncertaintyRecords = getHierarchicalRecords(allFoods, dietaryIndividualDayIntakes, uncertaintyRecords, cancelToken);
            updateContributionsHierarchicalNodes(hierarchicalNodeUncertaintyRecords);
        }

        protected void addMissingModelledFoodRecords(ICollection<Food> modelledFoods) {
            var foodCodes = Records.Select(c => c.FoodCode).ToHashSet();
            foreach (var food in modelledFoods) {
                if (!foodCodes.Contains(food.Code)) {
                    Records.Add(new DistributionFoodRecord() {
                        __Id = food.Code,
                        __IdParent = food.Parent?.Code,
                        __IsSummaryRecord = false,
                        FoodCode = food.Code,
                        FoodName = food.Name,
                        Contributions = new List<double>(),
                    });
                }
            }
        }

        /// <summary>
        /// Hierarchical food records
        /// </summary>
        /// <param name="allFoods"></param>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="distributionFoodRecords"></param>
        /// <param name="cancelToken"></param>
        private List<DistributionFoodRecord> getHierarchicalRecords(
            ICollection<Food> allFoods,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            List<DistributionFoodRecord> distributionFoodRecords,
            CancellationToken cancelToken
        ) {
            var foodsAsMeasured = dietaryIndividualDayIntakes
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(i => i.IntakesPerFood).Select(c => c.FoodAsMeasured).Distinct().ToList();

            //Create foods hierarchy
            var foodHierarchy = HierarchyUtilities.BuildHierarchy(foodsAsMeasured, allFoods, (Food f) => f.Code, (Food f) => f.Parent?.Code);
            var allFoodSubTrees = foodHierarchy.Traverse().ToList();

            var exposuresPerFoodAsMeasuredLookup = distributionFoodRecords.ToLookup(r => r.FoodCode);
            // Create summary records
            var hierarchicalNodes = allFoodSubTrees
                .Where(n => n.Children.Any())
                .Select(n => {
                    var f = n.Node;
                    var allNodes = n.AllNodes();
                    var exposures = allNodes.SelectMany(r => exposuresPerFoodAsMeasuredLookup[r.Code]);
                    return summarizeHierarchicalExposures(f, exposures, true);
                })
                .ToList();

            foreach (var foodAsMeasured in distributionFoodRecords) {
                var node = hierarchicalNodes.FirstOrDefault(c => c.FoodCode == foodAsMeasured.FoodCode);
                if (node != null) {
                    //repair nodes
                    node.FoodCode += "-group";
                    node.__Id = node.FoodCode;
                    foodAsMeasured.__IdParent = node.FoodCode;
                    foodAsMeasured.FoodName += "-unspecified";
                    //assign node to children
                    var children = Records.Where(c => c.__IdParent == foodAsMeasured.FoodCode).ToList();
                    foreach (var child in children) {
                        child.__IdParent = node.FoodCode;
                    }
                }
            }

            return hierarchicalNodes.ToList();
        }

        private DistributionFoodRecord summarizeHierarchicalExposures(
                Food foodAsMeasured,
                IEnumerable<DistributionFoodRecord> records,
                bool isSummaryRecord
            ) {
            return new DistributionFoodRecord() {
                __Id = foodAsMeasured.Code,
                __IdParent = foodAsMeasured.Parent?.Code,
                __IsSummaryRecord = isSummaryRecord,
                Contributions = new List<double>(),
                Mean = double.NaN,
                Contribution = records.Sum(c => c.Contribution),
                FoodCode = foodAsMeasured.Code,
                FoodName = foodAsMeasured.Name,
                Total = double.NaN,
                Percentile25 = double.NaN,
                Median = double.NaN,
                Percentile75 = double.NaN,
                Percentile25All = double.NaN,
                MedianAll = double.NaN,
                Percentile75All = double.NaN,
                FractionPositive = double.NaN,
                NumberOfSubstances = null,
                NumberOfIndividualDays = null
            };
        }

        private void updateContributions(List<DistributionFoodRecord> distributionFoodRecords) {
            foreach (var record in Records) {
                var contribution = distributionFoodRecords
                    .FirstOrDefault(c => c.FoodCode == record.FoodCode)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }

        private void updateContributionsHierarchicalNodes(List<DistributionFoodRecord> distributionFoodRecords) {
            foreach (var record in HierarchicalNodes) {
                var contribution = distributionFoodRecords
                    .FirstOrDefault(c => c.FoodCode == record.FoodCode)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
