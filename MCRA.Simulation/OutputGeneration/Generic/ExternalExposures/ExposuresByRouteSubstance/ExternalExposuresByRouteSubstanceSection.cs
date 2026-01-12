using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration.Generic.ExternalExposures.ExposuresByRouteSubstance {
    public class ExternalExposuresByRouteSubstanceSection : SummarySection {
        public override bool SaveTemporaryData => true;
        private static readonly double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public List<ExternalExposuresByRouteSubstanceRecord> ExposuresRecords { get; set; } = [];
        public SerializableDictionary<ExposureRoute, List<ExternalExposuresPercentilesRecord>> ExposuresBoxPlotRecords { get; set; } = [];
        public bool ShowOutliers { get; set; } = false;
        public virtual string PictureId { get; } = "6b9d4690-2754-4aab-984f-f2ace1f3836e";

        public void Summarize<T>(
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

            var weightedExposureAllRouteSubstances = 0d;
            foreach (var route in routes) {
                weightedExposureAllRouteSubstances += substances.Sum(substance => externalIndividualExposures
                    .Sum(c => c.GetExposure(route, substance, isPerPerson: false)
                       * c.SimulatedIndividual.SamplingWeight * relativePotencyFactors[substance] * membershipProbabilities[substance]));
            }

            var externalExposuresByRouteRecords = new List<ExternalExposuresByRouteSubstanceRecord>();
            foreach (var route in routes) {
                foreach (var substance in substances) {
                    var record = getSummaryRecord(
                        percentages,
                        route,
                        externalIndividualExposures,
                        substance,
                        relativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D),
                        membershipProbabilities ?? substances.ToDictionary(r => r, r => 1D),
                        isPerPerson,
                        exposureUnit,
                        weightedExposureAllRouteSubstances
                    );
                    if (record != null) {
                        externalExposuresByRouteRecords.Add(record);
                    }
                }
            }
            ExposuresRecords = [.. externalExposuresByRouteRecords
                .OrderBy(r => r.ExposureRoute)
                .ThenBy(r => r.SubstanceName)];

            foreach (var exposureRoute in routes) {
                var exposureRoutesPercentilesRecords =
                    getBoxPlotRecords(
                        exposureRoute,
                        externalIndividualExposures,
                        substances,
                        exposureUnit,
                        isPerPerson
                    );
                if (exposureRoutesPercentilesRecords.Count > 0) {
                    ExposuresBoxPlotRecords[exposureRoute] = exposureRoutesPercentilesRecords;
                }
            }
        }

        private static ExternalExposuresByRouteSubstanceRecord getSummaryRecord<T>(
            double[] percentages,
            ExposureRoute route,
            ICollection<T> externalIndividualExposures,
            Compound substance,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson,
            ExposureUnitTriple exposureUnit,
            double weightedExposureAllRouteSubstances
        ) where T : IExternalIndividualExposure {
            var rpf = relativePotencyFactors[substance];
            var membership = membershipProbabilities[substance];
            var individualExposures = externalIndividualExposures
                .Select(
                    r => (
                        Exposure: r.GetExposure(route, substance, isPerPerson) * rpf * membership,
                        r.SimulatedIndividual
                ))
                .ToList();

            var positives = individualExposures
                .Where(e => e.Exposure > 0)
                .ToList();
            if (positives.Count == 0) {
                return null;
            }

            var weightsAll = individualExposures
               .Select(r => r.SimulatedIndividual.SamplingWeight)
               .ToList();
            var weightsPositives = positives
                .Where(r => r.Exposure > 0)
                .Select(r => r.SimulatedIndividual.SamplingWeight)
                .ToList();

            var percentilesPositives = positives
                .Select(r => r.Exposure)
                .PercentilesWithSamplingWeights(
                    weightsPositives,
                    percentages
                );
            var percentilesAll = individualExposures
                .Select(r => r.Exposure)
                .PercentilesWithSamplingWeights(
                    weightsAll,
                    percentages
                );

            var sumAll = individualExposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);
            var sumPositives = positives.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);

            var record = new ExternalExposuresByRouteSubstanceRecord {
                ExposureRoute = route.GetDisplayName(),
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                Contribution = sumPositives / weightedExposureAllRouteSubstances,
                IndividualsWithPositiveExposure = positives.Count,
                MeanAll = sumAll / weightsAll.Sum(),
                MeanPositives = sumPositives / weightsPositives.Sum(),
                PercentagePositives = positives.Count * 100d / individualExposures.Count,
                LowerPercentilePositives = percentilesPositives[0],
                MedianPositives = percentilesPositives[1],
                UpperPercentilePositives = percentilesPositives[2],
                LowerPercentileAll = percentilesAll[0],
                MedianAll = percentilesAll[1],
                UpperPercentileAll = percentilesAll[2],
                MedianAllUncertaintyValues = [],
                Unit = exposureUnit.GetShortDisplayName()
            };
            return record;
        }

        private static List<ExternalExposuresPercentilesRecord> getBoxPlotRecords<T>(
            ExposureRoute exposureRoute,
            ICollection<T> externalIndividualExposures,
            ICollection<Compound> substances,
            ExposureUnitTriple exposureUnit,
            bool isPerPerson
        ) where T : IExternalIndividualExposure {
            var result = new List<ExternalExposuresPercentilesRecord>();

            foreach (var substance in substances) {
                var exposures = externalIndividualExposures
                    .Select(
                        r => (
                            Exposure: r.GetExposure(
                                exposureRoute,
                                substance,
                                isPerPerson
                            ),
                            r.SimulatedIndividual.SamplingWeight
                    ));

                var allExposures = exposures
                    .Select(r => r.Exposure)
                    .ToList();

                var weightsAll = exposures
                    .Select(r => r.SamplingWeight)
                    .ToList();

                var percentiles = allExposures
                    .PercentilesWithSamplingWeights(weightsAll, _percentages)
                    .ToList();
                var positives = allExposures.Where(r => r > 0).ToList();
                var p95Idx = _percentages.Length - 1;
                var substanceName = percentiles[p95Idx] > 0 ? substance.Name : $"{substance.Name} *";
                var outliers = allExposures
                    .Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                        || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                    .Select(c => c)
                    .ToList();

                var record = new ExternalExposuresPercentilesRecord() {
                    ExposureRoute = exposureRoute.ToString(),
                    MinPositives = positives.Count != 0 ? positives.Min() : double.NaN,
                    MaxPositives = positives.Count != 0 ? positives.Max() : double.NaN,
                    SubstanceCode = substance.Code,
                    SubstanceName = substanceName,
                    Percentiles = [.. percentiles],
                    NumberOfPositives = positives.Count,
                    Percentage = positives.Count * 100d / allExposures.Count,
                    Unit = exposureUnit.GetShortDisplayName(),
                    Outliers = outliers,
                    NumberOfOutLiers = outliers.Count,
                };
                if (positives.Count != 0) {
                    result.Add(record);
                }
            }
            return result;
        }
    }
}
