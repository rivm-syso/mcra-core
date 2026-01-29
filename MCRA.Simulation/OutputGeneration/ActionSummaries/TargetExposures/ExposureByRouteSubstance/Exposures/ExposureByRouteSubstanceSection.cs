using System.Linq;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Constants;
using MCRA.Simulation.Objects;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureByRouteSubstanceSection : ExposureByRouteSubstanceSectionBase {
        public override bool SaveTemporaryData => true;

        private static readonly double _upperWhisker = 95;
        private static readonly double[] _percentages = [5, 10, 25, 50, 75, 90, 95];

        public bool ShowOutliers { get; set; }
        public double? RestrictedUpperPercentile { get; set; }
        public List<ExposureByRouteSubstanceRecord> Records { get; set; }
        public List<ExposureByRouteSubstancePercentileRecord> BoxPlotRecords { get; set; }
        public TargetUnit TargetUnit { get; set; }

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
            double lowerPercentage,
            double upperPercentage,
            TargetUnit targetUnit,
            bool isPerPerson,
            bool skipPrivacySensitiveOutputs
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var routes = kineticConversionFactors.Select(c => c.Key.route).Distinct().ToList();

            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(externalIndividualExposures.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }

            ShowOutliers = !skipPrivacySensitiveOutputs;
            TargetUnit = targetUnit;

            var exposureCollection = CalculateExposures(
                externalIndividualExposures,
                substances,
                kineticConversionFactors,
                isPerPerson
            );

            Records = summarizeExposureRecords(
                exposureCollection,
                relativePotencyFactors,
                membershipProbabilities,
                percentages
            );

            BoxPlotRecords = summarizeBoxPlotsRecords(
                exposureCollection,
                targetUnit
            );
        }

        public List<ExposureByRouteSubstanceRecord> summarizeExposureRecords(
             List<(ExposureRoute ExposureRoute, Compound Substance, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> exposureCollection,
             IDictionary<Compound, double> relativePotencyFactors,
             IDictionary<Compound, double> membershipProbabilities,
             double[] percentages
         ) {
            var records = new List<ExposureByRouteSubstanceRecord>();
            foreach (var item in exposureCollection) {
                if (item.Exposures.Any(c => c.Exposure > 0)) {
                    var record = getExposureRouteSubstanceRecord(
                        item.ExposureRoute,
                        item.Substance,
                        item.Exposures,
                        relativePotencyFactors?[item.Substance] ?? null,
                        membershipProbabilities?[item.Substance] ?? null,
                        percentages
                    );
                    records.Add(record);
                }
            }
            return records;
        }

        private List<ExposureByRouteSubstancePercentileRecord> summarizeBoxPlotsRecords(
            List<(ExposureRoute ExposureRoute, Compound Substance, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> exposureRouteCollection,
            TargetUnit targetUnit
        ) {
            var records = new List<ExposureByRouteSubstancePercentileRecord>();

            foreach (var item in exposureRouteCollection) {
                if (item.Exposures.Any(c => c.Exposure > 0)) {
                    var boxPlotRecord = getBoxPlotRecord(
                        item.ExposureRoute,
                        item.Substance,
                        item.Exposures,
                        targetUnit
                    );
                    records.Add(boxPlotRecord);
                }
            }
            return records;
        }
        private ExposureByRouteSubstanceRecord getExposureRouteSubstanceRecord(
            ExposureRoute route,
            Compound substance,
            List<(SimulatedIndividual SimulatedIndividual, double Exposure)> exposures,
            double? rpf,
            double? membership,
            double[] percentages
        ) {
            var weights = exposures.Where(c => c.Exposure > 0)
                .Select(idi => idi.SimulatedIndividual.SamplingWeight)
                .ToList();
            var percentiles = exposures.Where(c => c.Exposure > 0)
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, percentages);
            var weightsAll = exposures
                .Select(idi => idi.SimulatedIndividual.SamplingWeight)
                .ToList();
            var percentilesAll = exposures
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weightsAll, percentages);
            var total = exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);
            var record = new ExposureByRouteSubstanceRecord {
                ExposureRoute = route.GetShortDisplayName(),
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                Percentage = weights.Count / (double)exposures.Count * 100,
                MeanAll = total / weightsAll.Sum(),
                Mean = total / weights.Sum(),
                Percentile25 = percentiles[0],
                Median = percentiles[1],
                Percentile75 = percentiles[2],
                Percentile25All = percentilesAll[0],
                MedianAll = percentilesAll[1],
                Percentile75All = percentilesAll[2],
                RelativePotencyFactor = rpf ?? double.NaN,
                AssessmentGroupMembership = membership ?? double.NaN,
                NumberOfIndividuals = weights.Count,
            };
            return record;
        }

        private static ExposureByRouteSubstancePercentileRecord getBoxPlotRecord(
            ExposureRoute route,
            Compound substance,
            List<(SimulatedIndividual SimulatedIndividual, double Exposure)> exposures,
            TargetUnit targetUnit
        ) {
            var weights = exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var allExposures = exposures
                .Select(c => c.Exposure)
                .ToList();
            var percentiles = allExposures
                .PercentilesWithSamplingWeights(weights, _percentages)
                .ToList();
            var positives = allExposures
                .Where(r => r > 0)
                .ToList();
            var outliers = allExposures
                .Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                    || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                .Select(c => c)
                .ToList();
            var record = new ExposureByRouteSubstancePercentileRecord() {
                ExposureRoute = route.GetDisplayName(),
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                Percentiles = percentiles,
                NumberOfPositives = positives.Count,
                Percentage = positives.Count * 100d / exposures.Count,
                Unit = targetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType),
                Outliers = outliers,
                NumberOfOutLiers = outliers.Count,
            };
            return record;
        }
    }
}
