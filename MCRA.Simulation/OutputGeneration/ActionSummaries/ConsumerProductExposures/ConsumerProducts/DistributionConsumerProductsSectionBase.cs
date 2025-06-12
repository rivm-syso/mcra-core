using MCRA.Utils.Hierarchies;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public class DistributionConsumerProductsSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        protected double[] Percentages { get; set; }
        public List<DistributionConsumerProductRecord> Records { get; set; }

        public List<DistributionConsumerProductRecord> HierarchicalNodes { get; set; }

        /// <summary>
        /// Returns whether this section has hierarchical data or not.
        /// </summary>
        public bool HasHierarchicalData {
            get {
                return HierarchicalNodes?.Count > 0;
            }
        }

        public void SummarizeChronic(
            ICollection<ConsumerProduct> allConsumerProducts,
            ICollection<ConsumerProductIndividualIntake> cpIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new();
            var individualIds = cpIndividualExposures
                .Select(c => c.SimulatedIndividual.Id)
                .Distinct()
                .ToList();
            var individualDayCountLookup = cpIndividualExposures
                .GroupBy(c => c.SimulatedIndividual.Id)
                .ToDictionary(c => c.Key, c => c.Count());
            var totalIntake = cpIndividualExposures
                .GroupBy(c => c.SimulatedIndividual.Id)
                .Select(c => c.Sum(i => i.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * i.SimulatedIndividual.SamplingWeight) / c.Count())
                .Sum();

            var sumSamplingWeights = cpIndividualExposures.GroupBy(c => c.SimulatedIndividual.Id)
                .Sum(c => c.First().SimulatedIndividual.SamplingWeight);
            Records = [];
            //TODO, when a consumer product has more than one route, this goes wrong 
            foreach (var route in routes) {
                var records = cpIndividualExposures
                    .AsParallel()
                    .WithCancellation(cancelToken)
                    .SelectMany(i => i.IntakesPerProduct,
                        (i, ipcp) => (
                            SimulatedIndividualId: i.SimulatedIndividual.Id,
                            CPIndividualDayIntake: i,
                            IntakePerConsumerProduct: ipcp
                        ))
                    .GroupBy(ipcp => (ipcp.CPIndividualDayIntake, ipcp.IntakePerConsumerProduct.Product))
                    .Select(g => (
                        SimulatedIndividualId: g.First().SimulatedIndividualId,
                        NumberOfDaysInSurvey: individualDayCountLookup[g.First().SimulatedIndividualId],
                        ConsumerProduct: g.Key.Product,
                        IntakePerMassUnit: g.Sum(ipf => ipf.IntakePerConsumerProduct.IntakesPerSubstance[route].Sum(c => c.EquivalentSubstanceAmount(relativePotencyFactors[c.Compound], membershipProbabilities[c.Compound])))
                            / (isPerPerson ? 1 : g.Key.CPIndividualDayIntake.SimulatedIndividual.BodyWeight),
                        SamplingWeight: g.Key.CPIndividualDayIntake.SimulatedIndividual.SamplingWeight,
                        DistinctSubstances: g.SelectMany(ipf => ipf.IntakePerConsumerProduct.IntakesPerSubstance[route].Select(c => c.Compound)).Distinct()
                    ))
                    .GroupBy(gr => (gr.ConsumerProduct, gr.SimulatedIndividualId))
                    .Select(c => (
                        Product: c.Key.ConsumerProduct,
                        IntakePerMassUnit: c.Sum(ipf => ipf.IntakePerMassUnit) / c.First().NumberOfDaysInSurvey,
                        SamplingWeight: c.First().SamplingWeight,
                        SimulatedIndividualId: c.First().SimulatedIndividualId,
                        DistinctSubstances: c.SelectMany(d => d.DistinctSubstances).Distinct()
                    ))
                    .GroupBy(g => g.Product)
                    .Select(g => {
                        var allIntakes = g.Where(c => c.IntakePerMassUnit > 0)
                           .Select(gr => (
                               IntakePerMassUnit: gr.IntakePerMassUnit,
                               SamplingWeight: gr.SamplingWeight,
                               DistinctSubstances: gr.DistinctSubstances.Distinct().ToList()
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
                        return new DistributionConsumerProductRecord {
                            __Id = g.Key.Code,
                            __IdParent = g.Key.Parent?.Code,
                            __IsSummaryRecord = false,
                            ConsumerProductCode = g.Key.Code,
                            ConsumerProductName = g.Key.Name,
                            Contributions = [],
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
                            NumberOfSubstances = allIntakes.SelectMany(c => c.DistinctSubstances).Distinct().Count(),
                            NumberOfIndividualDays = weights.Count,
                        };
                    })
                    .Where(c => c.Contribution > 0)
                    .ToList();
                Records.AddRange(records);
            }

            Records.OrderBy(c =>c.Contribution).ToList();

            HierarchicalNodes = getHierarchicalRecords(allConsumerProducts, cpIndividualExposures, Records, cancelToken);
        }


        /// <summary>
        /// Hierarchical food records
        /// </summary>
        /// <param name="allConsumerProducts"></param>
        /// <param name="cpIndividualIntakes"></param>
        /// <param name="distributionConsumerProductRecords"></param>
        /// <param name="cancelToken"></param>
        private List<DistributionConsumerProductRecord> getHierarchicalRecords(
            ICollection<ConsumerProduct> allConsumerProducts,
            ICollection<ConsumerProductIndividualIntake> cpIndividualIntakes,
            List<DistributionConsumerProductRecord> distributionConsumerProductRecords,
            CancellationToken cancelToken
        ) {
            var consumerProducts = cpIndividualIntakes
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(i => i.IntakesPerProduct).Select(c => c.Product).Distinct().ToList();

            //Create foods hierarchy
            var consumerProductHierarchy = HierarchyUtilities.BuildHierarchy(consumerProducts, allConsumerProducts, (ConsumerProduct f) => f.Code, (ConsumerProduct f) => f.Parent?.Code);
            var allConsumerProductSubTrees = consumerProductHierarchy.Traverse().ToList();

            var exposuresPerConsumerProductLookup = distributionConsumerProductRecords.ToLookup(r => r.ConsumerProductCode);
            // Create summary records
            var hierarchicalNodes = allConsumerProductSubTrees
                .Where(n => n.Children.Any())
                .Select(n => {
                    var f = n.Node;
                    var allNodes = n.AllNodes();
                    var exposures = allNodes.SelectMany(r => exposuresPerConsumerProductLookup[r.Code]);
                    return summarizeHierarchicalExposures(f, exposures, true);
                })
                .ToList();

            foreach (var product in distributionConsumerProductRecords) {
                var node = hierarchicalNodes.FirstOrDefault(c => c.ConsumerProductCode == product.ConsumerProductCode);
                if (node != null) {
                    //repair nodes
                    node.ConsumerProductCode += "-group";
                    node.__Id = node.ConsumerProductCode;
                    product.__IdParent = node.ConsumerProductCode;
                    product.ConsumerProductName += "-unspecified";
                    //assign node to children
                    var children = Records.Where(c => c.__IdParent == product.ConsumerProductCode).ToList();
                    foreach (var child in children) {
                        child.__IdParent = node.ConsumerProductCode;
                    }
                }
            }

            return [.. hierarchicalNodes];
        }

        private DistributionConsumerProductRecord summarizeHierarchicalExposures(
                ConsumerProduct consumerProduct,
                IEnumerable<DistributionConsumerProductRecord> records,
                bool isSummaryRecord
            ) {
            return new DistributionConsumerProductRecord() {
                __Id = consumerProduct.Code,
                __IdParent = consumerProduct.Parent?.Code,
                __IsSummaryRecord = isSummaryRecord,
                Contributions = [],
                Mean = double.NaN,
                Contribution = records.Sum(c => c.Contribution),
                ConsumerProductCode = consumerProduct.Code,
                ConsumerProductName = consumerProduct.Name,
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
    }
}
