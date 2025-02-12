using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ContributionUpperDistributionByRouteSection : DistributionByRouteSectionBase {

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
            ICollection<ExposurePathType> exposureRoutes,
            double percentageForUpperTail,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            TargetUnit targetUnit,
            ExposureUnitTriple externalExposureUnit
        ) {
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);
            UpperPercentage = 100 - percentageForUpperTail;
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

            NumberOfIntakes = upperIntakes.Count;
            CalculatedUpperPercentage = upperIntakes.Sum(c => c.IndividualSamplingWeight) / aggregateExposures.Sum(c => c.IndividualSamplingWeight) * 100;
            if (NumberOfIntakes > 0) {
                var upperAggregateExposures = upperIntakes
                    .Select(c => c.GetTotalExternalExposure(
                        relativePotencyFactors,
                        membershipProbabilities,
                        kineticConversionFactors,
                        externalExposureUnit.IsPerUnit()
                    ))
                    .ToList();
                LowPercentileValue = upperAggregateExposures.Min();
                HighPercentileValue = upperAggregateExposures.Max();
            }
            ContributionRecords = SummarizeContributions(
                upperIntakes,
                exposureRoutes,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                externalExposureUnit
            );
            ContributionRecords.ForEach(record => record.UncertaintyLowerBound = uncertaintyLowerBound);
            ContributionRecords.ForEach(record => record.UncertaintyUpperBound = uncertaintyUpperBound);
        }

        public void SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit,
            double percentageForUpperTail
        ) {
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);
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
                exposureRoutes,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                externalExposureUnit
            );
            UpdateContributions(records);
        }
    }
}
