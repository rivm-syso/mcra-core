using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class UpperDistributionRouteCompoundSection : DistributionRouteCompoundSectionBase {

        public List<DistributionRouteCompoundRecord> Records { get; set; }
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NumberOfIntakes { get; set; }

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            double lowerPercentage,
            double upperPercentage,
            double percentageForUpperTail,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            TargetUnit targetUnit,
            ExposureUnitTriple externalExposureUnit
        ) {
            UpperPercentage = 100 - percentageForUpperTail;
            Percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var upperIntakeCalculator = new UpperAggregateIntakeCalculator();
            var aggregateExposures = aggregateIndividualExposures != null
                ? aggregateIndividualExposures
                : aggregateIndividualDayExposures.Cast<AggregateIndividualExposure>().ToList();

            var upperIntakes = upperIntakeCalculator
                .GetUpperTargetIndividualExposures(
                    aggregateExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    kineticConversionFactors,
                    percentageForUpperTail,
                    externalExposureUnit,
                    targetUnit
                );
            Records = Summarize(
                upperIntakes,
                activeSubstances,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                externalExposureUnit
            );
            NumberOfIntakes = upperIntakes.Count;
            if (upperIntakes.Any()) {
                var upperTargetExposures = upperIntakes
                    .Select(c => c.GetTotalExternalExposure(
                        relativePotencyFactors,
                        membershipProbabilities,
                        kineticConversionFactors,
                        externalExposureUnit.IsPerUnit()
                    ))
                    .ToList();
                LowPercentileValue = upperTargetExposures.Min();
                HighPercentileValue = upperTargetExposures.Max();
            }
            CalculatedUpperPercentage = upperIntakes
                .Sum(c => c.IndividualSamplingWeight)
                    / aggregateExposures
                        .Sum(c => c.IndividualSamplingWeight) * 100;

            setUncertaintyBounds(uncertaintyLowerBound, uncertaintyUpperBound);
        }

        private void setUncertaintyBounds(
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
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
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            ICollection<Compound> substances,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit,
            double percentageForUpperTail
        ) {
            var aggregateExposures = aggregateIndividualExposures != null
                ? aggregateIndividualExposures
                : aggregateIndividualDayExposures.Cast<AggregateIndividualExposure>().ToList();
            var upperIntakeCalculator = new UpperAggregateIntakeCalculator();
            var upperIntakes = upperIntakeCalculator
                .GetUpperTargetIndividualExposures(
                    aggregateExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    kineticConversionFactors,
                    percentageForUpperTail,
                    externalExposureUnit,
                    targetUnit
                );
            var records = SummarizeUncertainty(
                upperIntakes,
                substances,
                relativePotencyFactors,
                kineticConversionFactors,
                externalExposureUnit
            );
            updateContributions(records);
        }

        private void updateContributions(List<DistributionRouteCompoundRecord> records) {
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.ExposureRoute == record.ExposureRoute && c.CompoundCode == record.CompoundCode)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
