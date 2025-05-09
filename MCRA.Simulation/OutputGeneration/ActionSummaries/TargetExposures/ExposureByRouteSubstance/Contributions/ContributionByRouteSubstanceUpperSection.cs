﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ContributionByRouteSubstanceUpperSection : ContributionByRouteSubstanceSectionBase {
        public double? UpperPercentage { get; set; }
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
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double percentageForUpperTail,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            TargetUnit targetUnit,
            ExposureUnitTriple externalExposureUnit
        ) {
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
            Records = getContributionRecords(
                upperIntakes,
                activeSubstances,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                externalExposureUnit,
                uncertaintyLowerBound,
                uncertaintyUpperBound
            );
            NumberOfIntakes = upperIntakes.Count;
            if (upperIntakes.Any()) {
                var upperTargetExposures = upperIntakes
                    .Select(c => c.GetTotalExternalExposure(
                        relativePotencyFactors,
                        membershipProbabilities,
                        kineticConversionFactors,
                        externalExposureUnit.IsPerUnit
                    ))
                    .ToList();
                LowPercentileValue = upperTargetExposures.Min();
                HighPercentileValue = upperTargetExposures.Max();
            }
            CalculatedUpperPercentage = upperIntakes
                .Sum(c => c.SimulatedIndividual.SamplingWeight)
                    / aggregateExposures
                        .Sum(c => c.SimulatedIndividual.SamplingWeight) * 100;
        }

        public void SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ICollection<Compound> substances,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit,
            double percentageForUpperTail
        ) {
            if (substances.Count == 1 || relativePotencyFactors != null) {
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
        }
    }
}
