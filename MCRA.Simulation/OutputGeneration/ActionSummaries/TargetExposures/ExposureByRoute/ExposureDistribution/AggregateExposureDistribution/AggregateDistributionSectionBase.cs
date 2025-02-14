using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class AggregateDistributionSectionBase : SummarySection {

        public List<HistogramBin> IntakeDistributionBins { get; set; }
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

        /// <summary>
        /// Summarizes this section based on the main simulation run.
        /// </summary>
        protected void summarize(
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
            foreach(var exposure in aggregateIndividualDayExposures) {
                var totalExposureAtTarget = exposure
                    .GetTotalExposureAtTarget(targetUnit.Target, relativePotencyFactors, membershipProbabilities);
                var individualSamplingWeight = exposure.IndividualSamplingWeight;

                totalTargetConcentrationsList.Add(totalExposureAtTarget);
                allWeightsList.Add(individualSamplingWeight);

                if(totalExposureAtTarget > 0) {
                    //get the log value of the positive concentration
                    var logValue = Math.Log10(totalExposureAtTarget);
                    //adjust the min and max value if necessary
                    if(minLogConcentration > logValue) {
                        minLogConcentration = logValue;
                    }
                    if(maxLogConcentration < logValue) {
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
                .Select(c => c.IndividualSamplingWeight)
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
    }
}
