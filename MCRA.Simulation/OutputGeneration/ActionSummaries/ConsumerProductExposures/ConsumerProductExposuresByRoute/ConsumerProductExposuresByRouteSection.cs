using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class ConsumerProductExposuresByRouteSection : SummarySection {
        protected readonly double _upperWhisker = 95;
        public override bool SaveTemporaryData => true;

        protected static double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public List<ConsumerProductExposureRouteRecord> Records { get; set; }

        public void SummarizeChronic(
            ICollection<ConsumerProduct> allConsumerProducts,
            ICollection<ConsumerProductIndividualExposure> cpIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
         ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            relativePotencyFactors = relativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = membershipProbabilities ?? substances.ToDictionary(r => r, r => 1D);

            var cancelToken = ProgressState?.CancellationToken ?? new();
            var individualIds = cpIndividualExposures
                .Select(c => c.SimulatedIndividual.Id)
                .Distinct()
                .ToList();
            var totalIntake = 0d;
            foreach (var route in routes) {
                totalIntake += substances.Sum(substance => cpIndividualExposures
                    .Sum(c => c.GetExposure(route, substance, isPerPerson)
                        * c.SimulatedIndividual.SamplingWeight * relativePotencyFactors[substance] * membershipProbabilities[substance]));

            }
            var sumSamplingWeights = cpIndividualExposures.
                Sum(c => c.SimulatedIndividual.SamplingWeight);

            Records = [];

            //Aggregate over routes
            Records = [.. routes
                .SelectMany(route => cpIndividualExposures
                    .AsParallel()
                    .WithCancellation(cancelToken)
                    .SelectMany(i => i.IntakesPerProduct,
                        (i, ipcp) => (
                            SimulatedIndividual: i.SimulatedIndividual,
                            IntakePerConsumerProduct: ipcp
                        ))
                    .GroupBy(ipcp => (ipcp.SimulatedIndividual, route))
                    .Select(g => (
                        SimulatedIndividual: g.Key.SimulatedIndividual,
                        Route: g.Key.route,
                        IntakePerMassUnit: g.Sum(ipf => ipf.IntakePerConsumerProduct.IntakesPerSubstance[route].Sum(c => c.EquivalentSubstanceAmount(relativePotencyFactors[c.Compound], membershipProbabilities[c.Compound])))
                            / (isPerPerson ? 1 : g.Key.SimulatedIndividual.BodyWeight),
                        Substances: g.SelectMany(ipf => ipf.IntakePerConsumerProduct.IntakesPerSubstance[route].Select(c => c.Compound))
                    )))
                    .GroupBy(ipcp => ipcp.Route)
                    .Select(g => {
                        var allIntakes = g.Where(c => c.IntakePerMassUnit > 0)
                           .Select(gr => (
                               IntakePerMassUnit: gr.IntakePerMassUnit,
                               SamplingWeight: gr.SimulatedIndividual.SamplingWeight,
                               Substances: gr.Substances
                           ))
                        .ToList();

                        var samplingWeightsZeros = sumSamplingWeights - allIntakes.Sum(c => c.SamplingWeight);

                        var weights = allIntakes
                           .Select(c => c.SamplingWeight)
                           .ToList();

                        var percentilesAll = allIntakes
                           .Select(c => c.IntakePerMassUnit)
                           .PercentilesAdditionalZeros(weights, percentages, samplingWeightsZeros);

                        var percentiles = allIntakes
                            .Where(c => c.IntakePerMassUnit > 0)
                           .Select(ipf => ipf.IntakePerMassUnit)
                           .PercentilesWithSamplingWeights(weights, percentages);

                        var total = allIntakes.Sum(ipf => ipf.IntakePerMassUnit * ipf.SamplingWeight);

                        return new ConsumerProductExposureRouteRecord {
                            ExposureRoute = g.Key.GetShortDisplayName(),
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
                .Where(c => c.Contribution > 0)];

            _ = Records.OrderBy(c => c.Contribution).ToList();
        }
    }
}