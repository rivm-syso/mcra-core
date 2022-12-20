using MCRA.Utils;
using MCRA.Utils.Collections;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public class DistributionSectionBase : SummarySection, IIntakeDistributionSection {
        public override bool SaveTemporaryData => true;
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public List<HistogramBin> IntakeDistributionBins { get; set; }
        public List<CategorizedHistogramBin<ExposureRouteType>> CategorizedHistogramBins { get; set; }
        public int TotalNumberOfIntakes { get; set; }
        public double PercentageZeroIntake { get; set; }
        public bool IsTotalDistribution { get; set; }
        public bool IsAggregate { get; set; }

        protected UncertainDataPointCollection<double> _percentiles = new UncertainDataPointCollection<double>();
        public UncertainDataPointCollection<double> Percentiles {
            get {
                return _percentiles;
            }
        }
        /// <summary>
        /// Summarize intakes, calculates distribution and cumulative distribution
        /// </summary>
        /// <param name="intakes"></param>
        /// <param name="weights"></param>
        public void Summarize(
            List<double> intakes,
            List<double> weights
        ) {
            TotalNumberOfIntakes = intakes.Count;
            var percentages = GriddingFunctions.GetPlotPercentages();
            if (weights == null) {
                weights = Enumerable.Repeat(1D, intakes.Count).ToList();
            }
            var zipResults = intakes.Zip(weights, (x, w) => (x, w)).Where(c => c.x > 0);
            var exposures = zipResults.Select(c => Math.Log10(c.x)).ToList();
            var sampleWeights = zipResults.Select(c => c.w).ToList();
            if (exposures.Any()) {
                var min = exposures.Min();
                var max = exposures.Max();
                //Take all intakes for a better resolution
                var numberOfBins = Math.Sqrt(TotalNumberOfIntakes) < 100 ? BMath.Ceiling(Math.Sqrt(TotalNumberOfIntakes)) : 100;
                IntakeDistributionBins = exposures.MakeHistogramBins(sampleWeights, numberOfBins, min, max);
                PercentageZeroIntake = intakes.Count(c => c == 0) / (double)TotalNumberOfIntakes * 100;
            } else {
                IntakeDistributionBins = null;
                PercentageZeroIntake = 100;
            }

            // Summarize the exposures for based on a grid defined by the percentages array
            if (percentages.Length > 0 && IsTotalDistribution) {
                _percentiles.XValues = percentages;
                _percentiles.ReferenceValues = intakes
                    .PercentilesWithSamplingWeights(weights, percentages);
            }
        }

        public void SummarizeUncertainty(
                List<double> intakes, 
                List<double> weights, 
                double uncertaintyLowerBound, 
                double uncertaintyUpperBound
            ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            _percentiles.AddUncertaintyValues(intakes.PercentilesWithSamplingWeights(weights, _percentiles.XValues.ToArray()));
        }

        private CategoryContribution<ExposureRouteType> getAggregateCategoryContributionFraction(AggregateIndividualExposure oim, ExposureRouteType route, IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities, ICollection<ExposureRouteType> exposureRoutes) {
            double contribution = 0;
            foreach (var exposureRoute in exposureRoutes) {
                if (route == exposureRoute) {
                    contribution = oim.ExposuresPerRouteSubstance[route].Sum(c => c.Exposure) / oim.TargetExposuresBySubstance.Sum(c => c.Value.SubstanceAmount);
                }
            }
            return new CategoryContribution<ExposureRouteType>(route, contribution);
        }

        public void SummarizeCategorizedBins(
                ICollection<AggregateIndividualExposure> aggregateIndividualMeans, 
                IDictionary<Compound, double> relativePotencyFactors, 
                IDictionary<Compound, double> membershipProbabilities, 
                ICollection<ExposureRouteType> exposureRoutes,
                bool isPerPerson
            ) {
            var weights = aggregateIndividualMeans.Where(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0)
                .Select(c => c.IndividualSamplingWeight)
                .ToList();
            var result = aggregateIndividualMeans.Where(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0).ToList();
            var categories = exposureRoutes;
            Func<AggregateIndividualExposure, double> valueExtractor = (x) => Math.Log10(x.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson));
            Func<AggregateIndividualExposure, List<CategoryContribution<ExposureRouteType>>> categoryExtractor = (x) => categories.Select(r => getAggregateCategoryContributionFraction(x, r, relativePotencyFactors, membershipProbabilities, exposureRoutes)).ToList();
            CategorizedHistogramBins = result.MakeCategorizedHistogramBins<AggregateIndividualExposure, ExposureRouteType>(categoryExtractor, valueExtractor, weights);
        }

        public void SummarizeCategorizedBins(
            ICollection<AggregateIndividualExposure> aggregateIndividualMeans,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRouteType> exposureRoutes,
            TwoKeyDictionary<ExposureRouteType,Compound, double> absorptionFactors,
            bool isPerPerson
        ) {
            var weights = aggregateIndividualMeans
                .Where(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0)
                .Select(c => c.IndividualSamplingWeight)
                .ToList();
            var result = aggregateIndividualMeans
                .Where(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0)
                .ToList();
            var categories = exposureRoutes;
            Func<AggregateIndividualExposure, double> valueExtractor = (x) => Math.Log10(x.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson));
            Func<AggregateIndividualExposure, List<CategoryContribution<ExposureRouteType>>> categoryExtractor = (x) => categories.Select(r => getAggregateCategoryContributionFraction(x, r, absorptionFactors)).ToList();
            CategorizedHistogramBins = result.MakeCategorizedHistogramBins(categoryExtractor, valueExtractor, weights);
        }

        private CategoryContribution<ExposureRouteType> getAggregateCategoryContributionFraction(
            AggregateIndividualExposure oim,
            ExposureRouteType route,
            TwoKeyDictionary<ExposureRouteType, Compound, double> absorptionFactors
        ) {
            double contribution = 0;
            if (oim.ExposuresPerRouteSubstance.TryGetValue(route, out var routeSubstanceExposures)) {
                var positives = routeSubstanceExposures.Where(r => r.Exposure > 0).ToList();
                if (positives.Any()) {
                    contribution = routeSubstanceExposures.Sum(c => c.Exposure * absorptionFactors[route, c.Compound]) / oim.TargetExposuresBySubstance.Sum(c => c.Value.SubstanceAmount);
                }
            }
            return new CategoryContribution<ExposureRouteType>(route, contribution);
        }

        /// <summary>
        /// Summarizes the exposures for ISUF. Percentiles (output) from specified percentages (input).
        /// </summary>
        /// <param name="isufModel"></param>
        /// <param name="percentages"></param>
        public void Summarize(ISUFModel isufModel, double[] percentages) {
            var xIdev = new List<double>();
            foreach (var p in percentages) {
                xIdev.Add(Math.Sqrt(isufModel.TransformationResult.VarianceBetweenUnit) * NormalDistribution.InvCDF(0, 1, p / 100));
            }
            Percentiles.XValues = percentages;
            Percentiles.ReferenceValues = UtilityFunctions.LinearInterpolate(isufModel.UsualIntakeResult.UsualIntakes.Select(c => c.UsualIntake).ToList(), isufModel.UsualIntakeResult.UsualIntakes.Select(c => c.Deviate).ToList(), xIdev.ToList());
        }

        /// <summary>
        /// Summarizes the uncertainty exposures for ISUF. Percentiles (output) from specified percentages (input).
        /// </summary>
        /// <param name="isufModel"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        public void SummarizeUncertainty(
                ISUFModel isufModel, 
                double uncertaintyLowerBound, 
                double uncertaintyUpperBound
            ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            var xIdev = new List<double>();
            foreach (var p in _percentiles.XValues) {
                xIdev.Add(Math.Sqrt(isufModel.TransformationResult.VarianceBetweenUnit) * NormalDistribution.InvCDF(0, 1, p / 100));
            }
            Percentiles.AddUncertaintyValues(UtilityFunctions.LinearInterpolate(isufModel.UsualIntakeResult.UsualIntakes.Select(c => c.UsualIntake).ToList(), isufModel.UsualIntakeResult.UsualIntakes.Select(c => c.Deviate).ToList(), xIdev.ToList()));
        }
    }
}
