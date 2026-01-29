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
    public sealed class ExposureBySubstanceSection : ExposureBySubstanceSectionBase {
        public override bool SaveTemporaryData => true;

        private static readonly double[] _percentages = [5, 10, 25, 50, 75, 90, 95];

        private static readonly double _upperWhisker = 95;
        public int NumberOfIntakes { get; set; }
        public bool ShowOutliers { get; set; }
        public double? RestrictedUpperPercentile { get; set; }
        public List<ExposureBySubstanceRecord> Records { get; set; }
        public List<ExposureBySubstancePercentileRecord> BoxPlotRecords { get; set; }
        public TargetUnit TargetUnit { get; set; }

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ICollection<Compound> substances,
            double lowerPercentage,
            double upperPercentage,
            TargetUnit targetUnit,
            bool skipPrivacySensitiveOutputs,
            bool isPerPerson
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };

            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(externalIndividualExposures.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }

            ShowOutliers = !skipPrivacySensitiveOutputs;
            TargetUnit = targetUnit;
            NumberOfIntakes = externalIndividualExposures.Count;

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

            BoxPlotRecords = summarizeBoxPlotRecords(
                exposureCollection,
                targetUnit
            );
        }

        public List<ExposureBySubstanceRecord> summarizeExposureRecords(
            List<(Compound Substance, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> exposureSourceCollection,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double[] percentages
        ) {
            var records = new List<ExposureBySubstanceRecord>();
            foreach (var item in exposureSourceCollection) {
                if (item.Exposures.Any(c => c.Exposure > 0)) {
                    var record = getExposureSubstanceRecord(
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

        private ExposureBySubstanceRecord getExposureSubstanceRecord(
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
            var record = new ExposureBySubstanceRecord {
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


        /// <summary>
        /// Calculate summary statistics for boxplots target exposures chronic.
        /// </summary>
        private List<ExposureBySubstancePercentileRecord> summarizeBoxPlotRecords(
            List<(Compound Substance, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> exposureCollection,
            TargetUnit targetUnit
        ) {
            var records = new List<ExposureBySubstancePercentileRecord>();

            foreach (var item in exposureCollection) {
                if (item.Exposures.Any(c => c.Exposure > 0)) {
                    var boxPlotRecord = getBoxPlotRecord(
                        item.Substance,
                        item.Exposures,
                        targetUnit
                    );
                    records.Add(boxPlotRecord);
                }
            }
            return records;
        }

        private static ExposureBySubstancePercentileRecord getBoxPlotRecord(
            Compound substance,
            List<(SimulatedIndividual SimulatedIndividual, double Exposure)> exposures,
            TargetUnit targetUnit
        ) {
            var weights = exposures
                .Select(a => a.SimulatedIndividual.SamplingWeight)
                .ToList();
            var allExposures = exposures
                .Select(a => a.Exposure)
                .ToList();
            var percentiles = allExposures
                .PercentilesWithSamplingWeights(weights, _percentages)
                .ToList();
            var positives = allExposures.Where(r => r > 0).ToList();
            var outliers = allExposures
                .Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                    || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                .Select(c => c)
                .ToList();
            return new ExposureBySubstancePercentileRecord() {
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                Percentiles = percentiles,
                NumberOfPositives = positives.Count,
                Percentage = positives.Count * 100d / exposures.Count,
                Outliers = outliers,
                Unit = targetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType),
                NumberOfOutLiers = outliers.Count,
            };
        }
    }
}
