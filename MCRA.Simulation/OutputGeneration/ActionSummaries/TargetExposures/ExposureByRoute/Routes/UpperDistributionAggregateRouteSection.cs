using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class UpperDistributionAggregateRouteSection : DistributionAggregateRouteSectionBase {

        public List<AggregateDistributionExposureRouteTotalRecord> DistributionRouteUpperRecords { get; set; }

        public double UpperPercentage { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NRecords { get; set; }

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRouteType, Compound), double> absorptionFactors,
            ICollection<ExposureRouteType> exposureRoutes,
            double lowerPercentage,
            double upperPercentage,
            double percentageForUpperTail,
            ExposureType exposureType,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            UpperPercentage = percentageForUpperTail;
            var upperIntakeCalculator = new UpperAggregateIntakeCalculator(exposureType);
            if (aggregateIndividualExposures != null) {

                var upperIntakes = upperIntakeCalculator.GetUpperTargetIndividualExposures(aggregateIndividualExposures, relativePotencyFactors, membershipProbabilities, UpperPercentage, isPerPerson);
                DistributionRouteUpperRecords = Summarize(
                    upperIntakes,
                    exposureRoutes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    absorptionFactors,
                    isPerPerson
                );
                NRecords = upperIntakes.Count;
                UpperPercentage = 100 - upperIntakes.Sum(c => c.IndividualSamplingWeight) / aggregateIndividualExposures.Sum(c => c.IndividualSamplingWeight) * 100;
                if (NRecords > 0) {
                    LowPercentileValue = upperIntakes.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).Min();
                    HighPercentileValue = upperIntakes.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).Max();
                }
            } else {
                var upperIntakes = upperIntakeCalculator.GetUpperTargetIndividualDayExposures(aggregateIndividualDayExposures, relativePotencyFactors, membershipProbabilities, UpperPercentage, isPerPerson);
                DistributionRouteUpperRecords = Summarize(
                    upperIntakes,
                    exposureRoutes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    absorptionFactors,
                    isPerPerson
                );
                NRecords = upperIntakes.Count;
                UpperPercentage = 100 - upperIntakes.Sum(c => c.IndividualSamplingWeight) / aggregateIndividualDayExposures.Sum(c => c.IndividualSamplingWeight) * 100;
                if (NRecords > 0) {
                    LowPercentileValue = upperIntakes.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).Min();
                    HighPercentileValue = upperIntakes.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).Max();
                }
            }
            setUncertaintyBounds(uncertaintyLowerBound, uncertaintyUpperBound);
        }
        private void setUncertaintyBounds(double uncertaintyLowerBound, double uncertaintyUpperBound) {
            foreach (var item in DistributionRouteUpperRecords) {
                item.UncertaintyLowerBound = uncertaintyLowerBound;
                item.UncertaintyUpperBound = uncertaintyUpperBound;
            }
        }
        public void SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRouteType, Compound), double> absorptionFactors,
            ICollection<ExposureRouteType> exposureRoutes,
            bool isPerPerson
        ) {
            var records = new List<AggregateDistributionExposureRouteTotalRecord>();
            if (aggregateIndividualExposures != null) {
                records = SummarizeUncertainty(aggregateIndividualExposures, exposureRoutes, relativePotencyFactors, membershipProbabilities, absorptionFactors, isPerPerson);
            } else {
                records = SummarizeUncertainty(aggregateIndividualDayExposures, exposureRoutes, relativePotencyFactors, membershipProbabilities, absorptionFactors, isPerPerson);
            }
            updateContributions(records);
        }

        private void updateContributions(List<AggregateDistributionExposureRouteTotalRecord> distributionRouteUpperRecords) {
            foreach (var record in DistributionRouteUpperRecords) {
                var contribution = distributionRouteUpperRecords.Where(c => c.ExposureRoute == record.ExposureRoute).FirstOrDefault()?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
