using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class InternalDistributionSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<HistogramBin> IntakeDistributionBins { get; set; }
        public List<CategorizedHistogramBin<string>> StratifiedIntakeDistributionBins { get; set; }
        public List<HistogramBin> IntakeDistributionBinsCoExposure { get; set; }
        public List<CategorizedHistogramBin<ExposureRoute>> CategorizedHistogramBins { get; set; }
        public double PercentageZeroIntake { get; set; }
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }

        public TargetUnit TargetUnit { get; set; }

        protected UncertainDataPointCollection<double> _percentiles = [];
        public UncertainDataPointCollection<double> Percentiles {
            get => _percentiles;
            set => _percentiles = value;
        }

        public List<(string, UncertainDataPointCollection<double>)> StratifiedPercentiles { get; set; }

        public int TotalNumberOfIntakes { get; set; }

        /// <summary>
        /// Summarize intakes, calculates distribution and cumulative distribution
        /// </summary>
        /// <param name="exposures"></param>
        /// <param name="isTotalDistribution"></param>
        public void SummarizeUnstratifiedBinsGraph(
            ICollection<AggregateIndividualExposure> aggregates,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            PopulationStratifier populationStratifier,
            TargetUnit targetUnit,
            List<int> coExposureIds = null
        ) {
            var exposures = aggregates
                .Select(c => (
                    Exposure: c.GetTotalExposureAtTarget(
                        targetUnit.Target,
                        relativePotencyFactors,
                        membershipProbabilities
                    ),
                    SimulatedIndividual: c.SimulatedIndividual
                ))
                .ToList();

            TotalNumberOfIntakes = exposures.Count;
            var logExposures = exposures.Where(c => c.Exposure > 0)
                .Select(c => (
                    Exposure: Math.Log10(c.Exposure),
                    SampingWeight: c.SimulatedIndividual.SamplingWeight)
                ).ToList();

            if (logExposures.Any()) {
                var weights = logExposures.Select(c => c.SampingWeight).ToList();
                var intakes = logExposures.Select(c => c.Exposure).ToList();
                // Take all intakes for a better resolution
                var numberOfBins = Math.Sqrt(TotalNumberOfIntakes) < 100 ? BMath.Ceiling(Math.Sqrt(TotalNumberOfIntakes)) : 100;
                IntakeDistributionBins = intakes.MakeHistogramBins(
                    weights,
                    numberOfBins,
                    intakes.Min(),
                    intakes.Max()
                );
                PercentageZeroIntake = exposures.Count(c => c.Exposure == 0) / (double)TotalNumberOfIntakes * 100;
            } else {
                IntakeDistributionBins = null;
                PercentageZeroIntake = 100;
            }
            {
                // Summarize the exposures based on a grid defined by the percentages array
                var percentages = GriddingFunctions.GetPlotPercentages();
                var weights = exposures.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
                weights = weights ?? [.. Enumerable.Repeat(1D, exposures.Count)];
                _percentiles.XValues = percentages;
                _percentiles.ReferenceValues = exposures
                    .Select(c => c.Exposure)
                    .PercentilesWithSamplingWeights(weights, percentages);
            }

            IntakeDistributionBinsCoExposure = getCoExposureBins(
                aggregates,
                relativePotencyFactors,
                membershipProbabilities,
                targetUnit,
                coExposureIds
            );
        }

        private List<HistogramBin> getCoExposureBins(
            ICollection<AggregateIndividualExposure> aggregrates,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            TargetUnit targetUnit,
            List<int> coExposureIds
        ) {
            if (coExposureIds?.Count > 0) {
                var coExposureLookup = coExposureIds.ToHashSet();
                //cache data for multiple passes
                var numberOfIntakes = aggregrates.Count;
                var numberOfBins = Math.Sqrt(TotalNumberOfIntakes) < 100 ? BMath.Ceiling(Math.Sqrt(TotalNumberOfIntakes)) : 100;
                var totalTargetConcentrationsList = new List<double>(numberOfIntakes);
                var allWeightsList = new List<double>(numberOfIntakes);

                //lists with positive concentrations only
                var positiveLogDataList = new List<double>();
                var positiveWeightsList = new List<double>();

                //lists with positive concentrations with co-exposure
                var coexposureLogDataList = new List<double>();
                var coexposureWeightsList = new List<double>();
                var minLogConcentration = double.PositiveInfinity;
                var maxLogConcentration = double.NegativeInfinity;
                var emptyConcentrationsCount = 0;
                var positiveConcentrationsCount = 0;
                foreach (var exposure in aggregrates) {
                    var totalExposureAtTarget = exposure
                        .GetTotalExposureAtTarget(targetUnit.Target, relativePotencyFactors, membershipProbabilities);
                    var individualSamplingWeight = exposure.SimulatedIndividual.SamplingWeight;

                    totalTargetConcentrationsList.Add(totalExposureAtTarget);
                    allWeightsList.Add(individualSamplingWeight);

                    if (totalExposureAtTarget > 0) {
                        //get the log value of the positive concentration
                        var logValue = Math.Log10(totalExposureAtTarget);
                        //adjust the min and max value if necessary
                        if (minLogConcentration > logValue) {
                            minLogConcentration = logValue;
                        }
                        if (maxLogConcentration < logValue) {
                            maxLogConcentration = logValue;
                        }
                        positiveLogDataList.Add(logValue);
                        positiveWeightsList.Add(individualSamplingWeight);
                        positiveConcentrationsCount++;

                        if (coExposureIds?.Count > 0 && coExposureLookup.Contains(exposure.SimulatedIndividual.Id)) {
                            coexposureLogDataList.Add(logValue);
                            coexposureWeightsList.Add(individualSamplingWeight);
                        }
                    } else {
                        emptyConcentrationsCount++;
                    }
                }
                var result = coexposureLogDataList.MakeHistogramBins(
                    coexposureWeightsList,
                    numberOfBins,
                    minLogConcentration,
                    maxLogConcentration
                );
                return result;
            } else {
                return null;
            }
        }

        public void SummarizeStratifiedBinsGraph(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            PopulationStratifier populationStratifier,
            TargetUnit targetUnit
        ) {

            if (populationStratifier != null) {
                {
                    var positiveExposures = aggregateIndividualExposures
                        .Where(c => c.GetTotalExposureAtTarget(
                            targetUnit.Target,
                            relativePotencyFactors,
                            membershipProbabilities) > 0
                        )
                        .ToList();
                    var positiveWeights = positiveExposures
                        .Select(c => c.SimulatedIndividual.SamplingWeight)
                        .ToList();

                    var result = positiveExposures.MakeCategorizedHistogramBins(
                        categoryExtractor: (x) => {
                            var level = populationStratifier.GetLevel(x.SimulatedIndividual);
                            var categoryContributions = new List<CategoryContribution<string>> { new(level.Code, 1) };
                            return categoryContributions;
                        },
                        valueExtractor: (x) => {
                            var totalExposure = x.GetTotalExposureAtTarget(
                                targetUnit.Target,
                                relativePotencyFactors,
                                membershipProbabilities
                            );
                            return Math.Log10(totalExposure);
                        },
                        positiveWeights
                    );
                    StratifiedIntakeDistributionBins = result;
                }
                // Summarize the exposures based on a grid defined by the percentages array
                var exposures = aggregateIndividualExposures
                    .Select(c => (
                        Exposure: c.GetTotalExposureAtTarget(
                            targetUnit.Target,
                            relativePotencyFactors,
                            membershipProbabilities
                        ),
                        SimulatedIndividual: c.SimulatedIndividual,
                        StratificationLevel: populationStratifier.GetLevel(c.SimulatedIndividual).Code
                    ))
                    .GroupBy(c => c.StratificationLevel)
                    .ToList();

                var percentages = GriddingFunctions.GetPlotPercentages();
                var stratifiedPercentiles = new List<(string, UncertainDataPointCollection<double>)>();
                foreach (var group in exposures) {
                    var weights = group.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
                    weights = weights ?? [.. Enumerable.Repeat(1D, exposures.Count)];
                    var udpCollection = new UncertainDataPointCollection<double> {
                        XValues = percentages,
                        ReferenceValues = group
                            .Select(c => c.Exposure)
                            .PercentilesWithSamplingWeights(weights, percentages)
                    };
                    stratifiedPercentiles.Add((group.Key, udpCollection));
                }
                StratifiedPercentiles = [.. stratifiedPercentiles.OrderBy(c => c.Item1, StringComparer.OrdinalIgnoreCase)];
            } else {
                StratifiedIntakeDistributionBins = null;
                StratifiedPercentiles = null;
            }
        }
    }
}
