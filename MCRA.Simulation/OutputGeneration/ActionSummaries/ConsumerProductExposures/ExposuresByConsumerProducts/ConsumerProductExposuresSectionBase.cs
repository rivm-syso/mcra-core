using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Hierarchies;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class ConsumerProductExposuresSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        protected double[] Percentages { get; set; }
        public List<ConsumerProductExposureRecord> Records { get; set; }

        public List<ConsumerProductExposureRecord> HierarchicalNodes { get; set; }

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
            ICollection<ConsumerProductIndividualExposure> cpIndividualExposures,
            ICollection<Compound> substances,
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
            var totalIntake = 0d;
            foreach (var route in routes) {
                //TODO Overleg dit even met Tijmen of Johannes

                //foreach (var substance in substances) {
                //    totalIntake += cpIndividualExposures
                //    .Select(c => c.GetExposure(route, substance, isPerPerson)
                //        * c.SimulatedIndividual.SamplingWeight * relativePotencyFactors[substance] * membershipProbabilities[substance])
                //    .Sum();
                //}
                totalIntake += substances.Sum(substance => cpIndividualExposures
                    .Sum(c => c.GetExposure(route, substance, isPerPerson)
                        * c.SimulatedIndividual.SamplingWeight * relativePotencyFactors[substance] * membershipProbabilities[substance]));

            }
            var sumSamplingWeights = cpIndividualExposures.
                Sum(c => c.SimulatedIndividual.SamplingWeight);

            Records = [];

            //Aggregate over routes
            var records = routes
                .SelectMany(route => cpIndividualExposures
                    .AsParallel()
                    .WithCancellation(cancelToken)
                    .SelectMany(i => i.IntakesPerProduct,
                        (i, ipcp) => (
                            SimulatedIndividual: i.SimulatedIndividual,
                            IntakePerConsumerProduct: ipcp
                        ))
                    .GroupBy(ipcp => (ipcp.SimulatedIndividual, ipcp.IntakePerConsumerProduct.Product))
                    .Select(g => (
                        SimulatedIndividual: g.Key.SimulatedIndividual,
                        Product: g.Key.Product,
                        IntakePerMassUnit: g.Sum(ipf => ipf.IntakePerConsumerProduct.IntakesPerSubstance[route].Sum(c => c.EquivalentSubstanceAmount(relativePotencyFactors[c.Compound], membershipProbabilities[c.Compound])))
                            / (isPerPerson ? 1 : g.Key.SimulatedIndividual.BodyWeight),
                        Substances: g.SelectMany(ipf => ipf.IntakePerConsumerProduct.IntakesPerSubstance[route].Select(c => c.Compound))
                    )))
                    .GroupBy(ipcp => (ipcp.SimulatedIndividual, ipcp.Product))
                    .Select(g => (
                        Product: g.Key.Product,
                        IntakePerMassUnit: g.Sum(ipf => ipf.IntakePerMassUnit),
                        SamplingWeight: g.Key.SimulatedIndividual.SamplingWeight,
                        Substances: g.SelectMany(ipf => ipf.Substances)
                        )
                    )
                    .GroupBy(ipcp => ipcp.Product)
                    .Select(g => {
                        var allIntakes = g.Where(c => c.IntakePerMassUnit > 0)
                           .Select(gr => (
                               IntakePerMassUnit: gr.IntakePerMassUnit,
                               SamplingWeight: gr.SamplingWeight,
                               Substances: gr.Substances
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
                            .Where(c => c.IntakePerMassUnit > 0)
                           .Select(ipf => ipf.IntakePerMassUnit)
                           .PercentilesWithSamplingWeights(weights, Percentages);

                        var total = allIntakes.Sum(ipf => ipf.IntakePerMassUnit * ipf.SamplingWeight);

                        return new ConsumerProductExposureRecord {
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
                            NumberOfSubstances = allIntakes.SelectMany(c => c.Substances).ToHashSet().Count(),
                            NumberOfIndividualDays = weights.Count,
                        };
                    })
                .Where(c => c.Contribution > 0)
                .ToList();

            Records.AddRange(records);

            _ = Records.OrderBy(c => c.Contribution).ToList();

            HierarchicalNodes = getHierarchicalRecords(
                allConsumerProducts,
                cpIndividualExposures,
                Records,
                cancelToken
            );
        }


        /// <summary>
        /// Hierarchical food records
        /// </summary>
        /// <param name="allConsumerProducts"></param>
        /// <param name="cpIndividualIntakes"></param>
        /// <param name="distributionConsumerProductRecords"></param>
        /// <param name="cancelToken"></param>
        private List<ConsumerProductExposureRecord> getHierarchicalRecords(
            ICollection<ConsumerProduct> allConsumerProducts,
            ICollection<ConsumerProductIndividualExposure> cpIndividualIntakes,
            List<ConsumerProductExposureRecord> distributionConsumerProductRecords,
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

        private static ConsumerProductExposureRecord summarizeHierarchicalExposures(
            ConsumerProduct consumerProduct,
            IEnumerable<ConsumerProductExposureRecord> records,
            bool isSummaryRecord
        ) {
            return new ConsumerProductExposureRecord() {
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
