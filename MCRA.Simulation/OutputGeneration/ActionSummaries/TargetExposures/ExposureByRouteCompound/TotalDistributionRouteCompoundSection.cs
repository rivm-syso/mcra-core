using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TotalDistributionRouteCompoundSection : DistributionRouteCompoundSectionBase {

        public List<DistributionRouteCompoundRecord> Records { get; set; }
        public int NumberOfIntakes { get; set; }

        public void Summarize(
                ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
                ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
                ICollection<Compound> selectedCompounds,
                IDictionary<Compound, double> relativePotencyFactors,
                IDictionary<Compound, double> membershipProbabilities,
                IDictionary<(ExposureRouteType, Compound), double> absorptionFactors,
                double lowerPercentage,
                double upperPercentage,
                double uncertaintyLowerBound,
                double uncertaintyUpperBound,
                bool isPerPerson
            ) {
            setPercentages(lowerPercentage, upperPercentage);

            if (aggregateIndividualExposures != null) {
                NumberOfIntakes = aggregateIndividualExposures.Count;
                Records = Summarize(
                    aggregateIndividualExposures,
                    selectedCompounds,
                    relativePotencyFactors,
                    membershipProbabilities,
                    absorptionFactors,
                    isPerPerson
                 );
            } else {
                NumberOfIntakes = aggregateIndividualDayExposures.Count;
                Records = Summarize(
                    aggregateIndividualDayExposures,
                    selectedCompounds,
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
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRouteType, Compound), double> absorptionFactors,
            ICollection<Compound> selectedCompounds,
            bool isPerPerson
        ) {
            var records = new List<DistributionRouteCompoundRecord>();
            if (aggregateIndividualExposures != null) {
                records = SummarizeUncertainty(
                    aggregateIndividualExposures,
                    selectedCompounds,
                    relativePotencyFactors,
                    membershipProbabilities,
                    absorptionFactors,
                    isPerPerson
                 );
            } else {
                records = SummarizeUncertainty(
                    aggregateIndividualDayExposures,
                    selectedCompounds,
                    relativePotencyFactors,
                    membershipProbabilities,
                    absorptionFactors,
                    isPerPerson
                 );
            }
            updateContributions(records);
        }

        private void updateContributions(List<DistributionRouteCompoundRecord> distributionRouteCompoundRecords) {
            foreach (var record in Records) {
                var contribution = distributionRouteCompoundRecords.FirstOrDefault(c => c.ExposureRoute == record.ExposureRoute && c.CompoundCode == record.CompoundCode)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
