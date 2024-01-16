using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class TotalDistributionAggregateRouteSection : DistributionAggregateRouteSectionBase {

        public List<AggregateDistributionExposureRouteTotalRecord> DistributionRouteTotalRecords { get; set; }

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<ExposurePathType> exposureRoutes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> absorptionFactors,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            if (aggregateIndividualExposures != null) {
                DistributionRouteTotalRecords = Summarize(
                    aggregateIndividualExposures,
                    exposureRoutes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    absorptionFactors,
                    isPerPerson
                );
            } else {
                DistributionRouteTotalRecords = Summarize(
                    aggregateIndividualDayExposures,
                    exposureRoutes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    absorptionFactors,
                    isPerPerson
                );
            }
            setUncertaintyBounds(uncertaintyLowerBound, uncertaintyUpperBound);
        }

        private void setUncertaintyBounds(double uncertaintyLowerBound, double uncertaintyUpperBound) {
            foreach (var item in DistributionRouteTotalRecords) {
                item.UncertaintyLowerBound = uncertaintyLowerBound;
                item.UncertaintyUpperBound = uncertaintyUpperBound;
            }
        }

        public void SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> absorptionFactors,
            ICollection<ExposurePathType> exposureRoutes,
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

        private void updateContributions(List<AggregateDistributionExposureRouteTotalRecord> distributionRouteTotalRecords) {
            foreach (var record in DistributionRouteTotalRecords) {
                var contribution = distributionRouteTotalRecords.FirstOrDefault(c => c.ExposureRoute == record.ExposureRoute)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
