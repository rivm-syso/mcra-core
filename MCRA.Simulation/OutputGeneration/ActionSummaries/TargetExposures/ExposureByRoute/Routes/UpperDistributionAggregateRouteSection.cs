using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class UpperDistributionAggregateRouteSection : DistributionAggregateRouteSectionBase {

        public List<AggregateDistributionExposureRouteTotalRecord> Records { get; set; }

        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NRecords { get; set; }

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> absorptionFactors,
            ICollection<ExposurePathType> exposureRoutes,
            double lowerPercentage,
            double upperPercentage,
            double percentageForUpperTail,
            ExposureType exposureType,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);
            Percentages = new double[] { lowerPercentage, 50, upperPercentage };
            UpperPercentage = 100 - percentageForUpperTail;
            var upperIntakeCalculator = new UpperAggregateIntakeCalculator();
            if (exposureType == ExposureType.Chronic) {
                var upperIntakes = upperIntakeCalculator
                    .GetUpperTargetIndividualExposures(
                        aggregateIndividualExposures,
                        relativePotencyFactors,
                        membershipProbabilities,
                        percentageForUpperTail,
                        isPerPerson
                    );
                Records = Summarize(
                    upperIntakes,
                    exposureRoutes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    absorptionFactors,
                    isPerPerson
                );
                NRecords = upperIntakes.Count;
                CalculatedUpperPercentage = upperIntakes.Sum(c => c.IndividualSamplingWeight) / aggregateIndividualExposures.Sum(c => c.IndividualSamplingWeight) * 100;
                if (NRecords > 0) {
                    LowPercentileValue = upperIntakes.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).Min();
                    HighPercentileValue = upperIntakes.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).Max();
                }
            } else {
                var upperIntakes = upperIntakeCalculator.GetUpperTargetIndividualDayExposures(aggregateIndividualDayExposures, relativePotencyFactors, membershipProbabilities, percentageForUpperTail, isPerPerson);
                Records = Summarize(
                    upperIntakes,
                    exposureRoutes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    absorptionFactors,
                    isPerPerson
                );
                NRecords = upperIntakes.Count;
                CalculatedUpperPercentage = upperIntakes.Sum(c => c.IndividualSamplingWeight) / aggregateIndividualDayExposures.Sum(c => c.IndividualSamplingWeight) * 100;
                if (NRecords > 0) {
                    LowPercentileValue = upperIntakes.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).Min();
                    HighPercentileValue = upperIntakes.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).Max();
                }
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
                records = SummarizeUncertainty(
                    aggregateIndividualExposures,
                    exposureRoutes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    absorptionFactors,
                    isPerPerson
                );
            } else {
                records = SummarizeUncertainty(
                    aggregateIndividualDayExposures,
                    exposureRoutes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    absorptionFactors,
                    isPerPerson
                );
            }
            updateContributions(records);
        }

        private void updateContributions(List<AggregateDistributionExposureRouteTotalRecord> distributionRouteUpperRecords) {
            foreach (var record in Records) {
                var contribution = distributionRouteUpperRecords.FirstOrDefault(c => c.ExposureRoute == record.ExposureRoute)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
