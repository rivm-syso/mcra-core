using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class TotalDistributionAggregateRouteSection : DistributionAggregateRouteSectionBase {

        public List<AggregateDistributionExposureRouteTotalRecord> Records { get; set; }

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<ExposurePathType> exposureRoutes,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> absorptionFactors,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);
            Percentages = new double[] { lowerPercentage, 50, upperPercentage };
            if (aggregateIndividualExposures != null) {
                Records = Summarize(
                    aggregateIndividualExposures,
                    exposureRoutes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    absorptionFactors,
                    isPerPerson
                );
            } else {
                Records = Summarize(
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
            foreach (var item in Records) {
                item.UncertaintyLowerBound = uncertaintyLowerBound;
                item.UncertaintyUpperBound = uncertaintyUpperBound;
            }
        }

        public void SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> absorptionFactors,
            ICollection<ExposurePathType> exposureRoutes,
            bool isPerPerson
        ) {
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);
            List<AggregateDistributionExposureRouteTotalRecord> records;
            if (aggregateIndividualExposures != null) {
                records = SummarizeUncertainty(aggregateIndividualExposures, exposureRoutes, relativePotencyFactors, membershipProbabilities, absorptionFactors, isPerPerson);
            } else {
                records = SummarizeUncertainty(aggregateIndividualDayExposures, exposureRoutes, relativePotencyFactors, membershipProbabilities, absorptionFactors, isPerPerson);
            }
            updateContributions(records);
        }

        private void updateContributions(List<AggregateDistributionExposureRouteTotalRecord> distributionRouteTotalRecords) {
            foreach (var record in Records) {
                var contribution = distributionRouteTotalRecords.FirstOrDefault(c => c.ExposureRoute == record.ExposureRoute)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
