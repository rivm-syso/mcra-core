using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class AggregateDistributionSectionBase : SummarySection {
        public List<HistogramBin> IntakeDistributionBins { get; set; }
        public List<HistogramBin> IntakeDistributionBinsCoExposure { get; set; }
        public List<CategorizedHistogramBin<ExposureRouteType>> AcuteCategorizedHistogramBins { get; set; }
        public int TotalNumberOfIntakes { get; set; }
        public double PercentageZeroIntake { get; set; }
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }

        protected UncertainDataPointCollection<double> _percentiles = new UncertainDataPointCollection<double>();
        public UncertainDataPointCollection<double> Percentiles { get => _percentiles; set => _percentiles = value; }

        /// <summary>
        /// Summarizes this section based on the main simulation run.
        /// </summary>
        /// <param name="coExposureIds"></param>
        /// <param name="aggregateIndividualDayExposures"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="absorptionFactors"></param>
        /// <param name="exposureRoutes"></param>
        /// <param name="percentages"></param>
        /// <param name="referenceDose"></param>
        /// <param name="isPerPerson"></param>
        /// <param name="uncertaintyLowerLimit"></param>
        /// <param name="uncertaintyUpperLimit"></param>
        public virtual void Summarize(
            List<int> coExposureIds,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRouteType, Compound), double> absorptionFactors,
            ICollection<ExposureRouteType> exposureRoutes,
            double[] percentages,
            bool isPerPerson,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            UncertaintyLowerLimit = uncertaintyLowerLimit;
            UncertaintyUpperLimit = uncertaintyUpperLimit;
            var numberOfBins = 0;

            TotalNumberOfIntakes = aggregateIndividualDayExposures.Count;
            //cache data for multiple passes
            var totalTargetConcentrationsList = new List<double>(TotalNumberOfIntakes);
            var allWeightsList = new List<double>(TotalNumberOfIntakes);
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
            var minLogConcentration = 0D;
            var maxLogConcentration = 0D;
            var useCoExposures = coExposureIds?.Any() ?? false;
            var coExposureLookup = coExposureIds?.ToHashSet();

            //iterate exposure array only once, todo: make concurrent
            foreach(var exposure in aggregateIndividualDayExposures) {
                var totalConcentrationAtTarget = exposure.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson);
                var individualSamplingWeight = exposure.IndividualSamplingWeight;

                totalTargetConcentrationsList.Add(totalConcentrationAtTarget);
                allWeightsList.Add(individualSamplingWeight);

                if(totalConcentrationAtTarget > 0) {
                    //get the log value of the positive concentration
                    var logValue = Math.Log10(totalConcentrationAtTarget);
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

                    if(useCoExposures && coExposureLookup.Contains(exposure.SimulatedIndividualDayId)) {
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
                PercentageZeroIntake = 100 * emptyConcentrationsCount / (double)TotalNumberOfIntakes;
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

            if(useCoExposures) {
                IntakeDistributionBinsCoExposure = coexposureLogDataList.MakeHistogramBins(coexposureWeightsList, numberOfBins, minLogConcentration, maxLogConcentration);
            }

            if (exposureRoutes.Count > 1) {
                summarizeCategorizedBins(aggregateIndividualDayExposures, relativePotencyFactors, membershipProbabilities, absorptionFactors, exposureRoutes, isPerPerson);
            }
        }

        protected CategoryContribution<ExposureRouteType> getAggregateCategoryContributionFraction(
            AggregateIndividualDayExposure idi,
            ExposureRouteType route,
            ICollection<ExposureRouteType> exposureRoutes
        ) {
            double contribution = 0;
            foreach (var exposureRoute in exposureRoutes) {
                if (route == exposureRoute) {
                    contribution = idi.ExposuresPerRouteSubstance[route].Sum(c => c.Exposure) / idi.TargetExposuresBySubstance.Sum(c => c.Value.SubstanceAmount);
                }
            }
            return new CategoryContribution<ExposureRouteType>(route, contribution);
        }

        protected void summarizeCategorizedBins(
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRouteType, Compound), double> absorptionFactors,
            ICollection<ExposureRouteType> exposureRoutes,
            bool isPerPerson
        ) {
            var weights = aggregateIndividualDayExposures.Where(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0)
                .Select(c => c.IndividualSamplingWeight)
                .ToList();
            var result = aggregateIndividualDayExposures.Where(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0).ToList();
            var categories = exposureRoutes;
            Func<AggregateIndividualDayExposure, double> valueExtractor = (x) => Math.Log10(x.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson));
            Func<AggregateIndividualDayExposure, List<CategoryContribution<ExposureRouteType>>> categoryExtractor = (x) => categories.Select(r => getAggregateCategoryContributionFraction(x, r, exposureRoutes)).ToList();
            AcuteCategorizedHistogramBins = result.MakeCategorizedHistogramBins(categoryExtractor, valueExtractor, weights);
        }
    }
}
