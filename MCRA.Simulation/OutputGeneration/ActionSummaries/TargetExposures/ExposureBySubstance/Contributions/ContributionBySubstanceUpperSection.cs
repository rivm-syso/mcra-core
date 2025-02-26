using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ContributionBySubstanceUpperSection : ContributionBySubstanceSectionBase {
        public double? UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NumberOfIntakes { get; set; }

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double percentageForUpperTail,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit
        ) {
            if (substances.Count ==1 || relativePotencyFactors != null) {
                var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
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

                Records = getContributionsRecords(
                    upperIntakes,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    kineticConversionFactors,
                    externalExposureUnit,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound
                );

                NumberOfIntakes = upperIntakes.Count;
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
                CalculatedUpperPercentage = upperIntakes
                    .Sum(c => c.IndividualSamplingWeight)
                        / aggregateExposures
                        .Sum(c => c.IndividualSamplingWeight) * 100;
            } 
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
                    membershipProbabilities,
                    kineticConversionFactors,
                    externalExposureUnit
                );
                updateContributions(records);
            }
        }
    }
}
