using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Objects;
using MCRA.Utils;
using MCRA.Utils.Collections;
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

        protected UncertainDataPointCollection<double> _percentiles = [];
        public UncertainDataPointCollection<double> Percentiles {
            get => _percentiles;
            set => _percentiles = value;
        }

        public List<(string, UncertainDataPointCollection<double>)> StratifiedPercentiles { get; set; }

        public int TotalNumberOfIntakes { get; set; }

        /// <summary>
        /// Summarizes this section based on the main simulation run.
        /// </summary>
        public void summarize(
            List<int> coExposureIds,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit,
            double[] percentages,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            UncertaintyLowerLimit = uncertaintyLowerLimit;
            UncertaintyUpperLimit = uncertaintyUpperLimit;
            var numberOfBins = 0;

            var numberOfIntakes = aggregateIndividualDayExposures.Count;

            //cache data for multiple passes
            var totalTargetConcentrationsList = new List<double>(numberOfIntakes);
            var allWeightsList = new List<double>(numberOfIntakes);

            //lists with positive concentrations only
            var positiveLogDataList = new List<double>();
            var positiveWeightsList = new List<double>();

            //lists with positive concentrations with co-exposure
            var coexposureLogDataList = new List<double>();
            var coexposureWeightsList = new List<double>();

            //other variables assigned in the single iteration of
            //the aggregate individual day exposures
            var emptyConcentrationsCount = 0;
            var positiveConcentrationsCount = 0;
            var minLogConcentration = double.PositiveInfinity;
            var maxLogConcentration = double.NegativeInfinity;
            var useCoExposures = coExposureIds?.Count > 0;
            var coExposureLookup = coExposureIds?.ToHashSet();

            //iterate exposure array only once, todo: make concurrent
            foreach (var exposure in aggregateIndividualDayExposures) {
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

                    if (useCoExposures && coExposureLookup.Contains(exposure.SimulatedIndividualDayId)) {
                        coexposureLogDataList.Add(logValue);
                        coexposureWeightsList.Add(individualSamplingWeight);
                    }
                } else {
                    emptyConcentrationsCount++;
                }
            }

            if (positiveConcentrationsCount > 0) {
                numberOfBins = Math.Sqrt(positiveConcentrationsCount) < 100 ? BMath.Ceiling(Math.Sqrt(positiveConcentrationsCount)) : 100;
                IntakeDistributionBins = positiveLogDataList.MakeHistogramBins(positiveWeightsList, numberOfBins, minLogConcentration, maxLogConcentration);
                PercentageZeroIntake = 100 * emptyConcentrationsCount / (double)numberOfIntakes;
            } else {
                IntakeDistributionBins = null;
                PercentageZeroIntake = 100;
            }

            // Summarize the exposures for based on a grid defined by the percentages array
            if (percentages != null && percentages.Length > 0) {
                _percentiles.XValues = percentages;
                _percentiles.ReferenceValues = totalTargetConcentrationsList
                    .PercentilesWithSamplingWeights(allWeightsList, percentages);
            }

            if (useCoExposures) {
                IntakeDistributionBinsCoExposure = coexposureLogDataList.MakeHistogramBins(coexposureWeightsList, numberOfBins, minLogConcentration, maxLogConcentration);
            }

            if (routes.Count > 1) {
                var categorizedHistogramBins = summarizeCategorizedBins(
                    aggregateIndividualDayExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    kineticConversionFactors,
                    routes,
                    externalExposureUnit,
                    targetUnit
                );
                CategorizedHistogramBins = categorizedHistogramBins;
            }
        }

        protected List<CategorizedHistogramBin<ExposureRoute>> summarizeCategorizedBins(
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit
        ) {
            // TODO: revise code
            var positiveExposures = aggregateIndividualDayExposures
                .Where(c => c.GetTotalExposureAtTarget(
                    targetUnit.Target,
                    relativePotencyFactors,
                    membershipProbabilities) > 0
                )
                .ToList();

            var weights = positiveExposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var categories = routes;

            var result = positiveExposures.MakeCategorizedHistogramBins(
                categoryExtractor: x => {
                    // TODO: route contributions are currently estimated by
                    // dividing the total external route exposure by the total
                    // external exposure. This should be reconsidered.
                    var contributions = x.GetExternalRouteExposureContributions(
                        routes,
                        relativePotencyFactors,
                        membershipProbabilities,
                        kineticConversionFactors,
                        externalExposureUnit,
                        targetUnit
                    );
                    var categoryContributions = routes
                        .Zip(contributions)
                        .Select(r => new CategoryContribution<ExposureRoute>(r.First, r.Second))
                        .ToList();
                    return categoryContributions;
                },
                valueExtractor: x => {
                    return Math.Log10(
                        x.GetTotalExposureAtTarget(
                            targetUnit.Target,
                            relativePotencyFactors,
                            membershipProbabilities
                        )
                    );
                },
                weights
            );
            return result;
        }

        /// <summary>
        /// Summarize intakes, calculates distribution and cumulative distribution
        /// </summary>
        /// <param name="exposures"></param>
        /// <param name="isTotalDistribution"></param>
        public void SummarizeUnstratifiedBinsGraph(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            PopulationStratifier populationStratifier,
            TargetUnit targetUnit,
            bool isTotalDistribution = true
        ) {
            var exposures = aggregateIndividualExposures
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

            // Summarize the exposures based on a grid defined by the percentages array
            if (isTotalDistribution) {
                var percentages = GriddingFunctions.GetPlotPercentages();
                var weights = exposures.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
                weights = weights ?? [.. Enumerable.Repeat(1D, exposures.Count)];
                _percentiles.XValues = percentages;
                _percentiles.ReferenceValues = exposures
                    .Select(c => c.Exposure)
                    .PercentilesWithSamplingWeights(weights, percentages);
            }
        }
        public void SummarizeCategorizedBins(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit
        ) {
            // TODO: revise code
            var positiveExposures = aggregateIndividualExposures
                .Where(c => c.GetTotalExposureAtTarget(
                    targetUnit.Target,
                    relativePotencyFactors,
                    membershipProbabilities) > 0
                )
                .ToList();
            var weights = positiveExposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();

            var result = positiveExposures.MakeCategorizedHistogramBins(
                categoryExtractor: (x) => {
                    // TODO: route contributions are currently estimated by
                    // dividing the total external route exposure by the total
                    // external exposure. This should be reconsidered.
                    var contributions = x.GetExternalRouteExposureContributions(
                        routes,
                        relativePotencyFactors,
                        membershipProbabilities,
                        kineticConversionFactors,
                        externalExposureUnit,
                        targetUnit
                    );
                    var categoryContributions = routes
                        .Zip(contributions)
                        .Select(r => new CategoryContribution<ExposureRoute>(r.First, r.Second))
                        .ToList();
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
                weights
            );
            CategorizedHistogramBins = result;
        }

        public void SummarizeStratifiedBinsGraph(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            PopulationStratifier populationStratifier,
            TargetUnit targetUnit,
            bool isTotalDistribution = true
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
                if (isTotalDistribution) {
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
                }
            } else {
                StratifiedIntakeDistributionBins = null;
                StratifiedPercentiles = null;
            }
        }
    }
}
