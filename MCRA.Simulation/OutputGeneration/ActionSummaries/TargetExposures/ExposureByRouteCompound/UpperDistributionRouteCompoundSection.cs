using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class UpperDistributionRouteCompoundSection : DistributionRouteCompoundSectionBase {

        public List<DistributionRouteCompoundRecord> Records { get; set; }
        public int NumberOfIntakes { get; set; }
        public double UpperPercentage { get; set; }
        public double CalcalatedUpperPercentage { get; set; }
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
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            double percentageForUpperTail,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            setPercentages(lowerPercentage, upperPercentage);
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
                    activeSubstances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    absorptionFactors,
                    isPerPerson
                );
                NumberOfIntakes = upperIntakes.Count;
                NRecords = NumberOfIntakes;
                if (NRecords > 0) {
                    LowPercentileValue = upperIntakes.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).Min();
                    HighPercentileValue = upperIntakes.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).Max();
                }
                CalcalatedUpperPercentage = 100 - upperIntakes.Sum(c => c.IndividualSamplingWeight) / aggregateIndividualExposures.Sum(c => c.IndividualSamplingWeight) * 100;
            } else {
                var upperIntakes = upperIntakeCalculator
                    .GetUpperTargetIndividualDayExposures(
                        aggregateIndividualDayExposures,
                        relativePotencyFactors,
                        membershipProbabilities,
                        percentageForUpperTail,
                        isPerPerson
                    );
                Records = Summarize(
                    upperIntakes,
                    activeSubstances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    absorptionFactors,
                    isPerPerson
                );
                NumberOfIntakes = upperIntakes.Count;
                NRecords = NumberOfIntakes;
                if (NumberOfIntakes > 0) {
                    LowPercentileValue = upperIntakes.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).Min();
                    HighPercentileValue = upperIntakes.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).Max();
                }
                CalcalatedUpperPercentage = 100 - upperIntakes.Sum(c => c.IndividualSamplingWeight) / aggregateIndividualDayExposures.Sum(c => c.IndividualSamplingWeight) * 100;
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
            IDictionary<(ExposurePathType, Compound), double> absorptionFactors,
            ICollection<Compound> selectedCompounds,
            bool isPerPerson
        ) {
            List<DistributionRouteCompoundRecord> records;
            if (aggregateIndividualExposures != null) {
                records = base.SummarizeUncertainty(aggregateIndividualExposures, selectedCompounds, relativePotencyFactors, membershipProbabilities, absorptionFactors, isPerPerson);
            } else {
                records = base.SummarizeUncertainty(aggregateIndividualDayExposures, selectedCompounds, relativePotencyFactors, membershipProbabilities, absorptionFactors, isPerPerson);
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
