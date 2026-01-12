using DocumentFormat.OpenXml.Drawing.Charts;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration.Generic.ExternalExposures.ExposuresByRoute {
    public class ExternalExposuresByRouteSection : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<ExternalExposuresByRouteRecord> ExposureRecords { get; set; }

        public virtual string PictureId { get; } = "45142be4-4274-4869-8200-f8cde245c275";

        public void SummarizeChronic<T>(
            ICollection<T> externalIndividualExposures,
            ICollection<ExposureRoute> routes,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double lowerPercentage,
            double upperPercentage,
            bool isPerPerson,
            ExposureUnitTriple exposureUnit
         ) where T : IExternalIndividualExposure {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            relativePotencyFactors = relativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = membershipProbabilities ?? substances.ToDictionary(r => r, r => 1D);

            var cancelToken = ProgressState?.CancellationToken ?? new();
            var weightedExposureAllRoutes = 0d;
            foreach (var route in routes) {
                weightedExposureAllRoutes += substances
                    .Sum(substance => externalIndividualExposures
                        .Sum(c => c.GetExposure(route, substance, isPerPerson)
                            * c.SimulatedIndividual.SamplingWeight * relativePotencyFactors[substance] * membershipProbabilities[substance]));
            }

            ExposureRecords = [];
            foreach (var route in routes) {
                cancelToken.ThrowIfCancellationRequested();
                var record = getSummaryRecord(
                    route,
                    externalIndividualExposures,
                    substances,
                    percentages,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson,
                    exposureUnit,
                    weightedExposureAllRoutes
                );
                if (record != null) {
                    ExposureRecords.Add(record);
                }
            }

            ExposureRecords = [.. ExposureRecords.OrderBy(c => c.Contribution)];
        }

        private static ExternalExposuresByRouteRecord getSummaryRecord<T>(
            ExposureRoute route,
            ICollection<T> externalIndividualExposures,
            ICollection<Compound> substances,
            double[] percentages,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson,
            ExposureUnitTriple exposureUnit,
            double weightedExposureAllRoutes
        ) where T : IExternalIndividualExposure {
            var individualExposures = substances
                .SelectMany(substance => {
                    var rpf = relativePotencyFactors[substance];
                    var membership = membershipProbabilities[substance];
                    return externalIndividualExposures.Select(e => new {
                        Exposure = e.GetExposure(route, substance, isPerPerson) * rpf * membership,
                        Substance = substance,
                        e.SimulatedIndividual,
                    });
                })
                .GroupBy(c => c.SimulatedIndividual)
                .Select(g => new {
                    SimulatedIndividual = g.Key,
                    Exposure = g.Sum(c => c.Exposure),
                    Substances = g.Select(c => c.Substance).ToHashSet(),
                })
                .ToList();

            var positives = individualExposures
                .Where(c => c.Exposure > 0)
                .ToList();
            if (positives.Count == 0) {
                return null;
            }

            var samplingWeightsAll = individualExposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var samplingWeightsPositives = positives
               .Select(r => r.SimulatedIndividual.SamplingWeight)
               .ToList();

            var percentilesPositives = positives
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(samplingWeightsPositives, percentages);
            var percentilesAll = individualExposures
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(samplingWeightsAll, percentages);

            var sumAll = individualExposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);
            var sumPositives = positives.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);
            var individualIds = externalIndividualExposures
                .Select(c => c.SimulatedIndividual.Id)
                .Distinct()
                .ToList();

            return new ExternalExposuresByRouteRecord {
                ExposureRoute = route.GetShortDisplayName(),
                Contributions = [],
                MeanAll = sumAll / samplingWeightsAll.Sum(),
                MeanPositives = sumPositives / samplingWeightsPositives.Sum(),
                Contribution = sumPositives / weightedExposureAllRoutes,
                Percentile25 = percentilesPositives[0],
                MedianPositives = percentilesPositives[1],
                Percentile75 = percentilesPositives[2],
                Percentile25All = percentilesAll[0],
                MedianAll = percentilesAll[1],
                Percentile75All = percentilesAll[2],
                FractionPositive = Convert.ToDouble(positives.Count) / Convert.ToDouble(individualIds.Count),
                NumberOfSubstances = positives.SelectMany(c => c.Substances).ToHashSet().Count,
                IndividualsWithPositiveExposure = positives.Count,
                Unit = exposureUnit.GetShortDisplayName(),
            };
        }
    }
}