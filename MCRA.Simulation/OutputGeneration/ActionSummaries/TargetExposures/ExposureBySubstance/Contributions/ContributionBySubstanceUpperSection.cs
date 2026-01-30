using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

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
            TargetUnit targetUnit,
            bool isPerPerson
        ) {
            if (substances.Count == 1 || relativePotencyFactors != null) {
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
                        percentageForUpperTail,
                        targetUnit
                    );

                Records = SummarizeContributions(
                    upperIntakes,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    kineticConversionFactors,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound,
                    isPerPerson
                );

                NumberOfIntakes = upperIntakes.Count;
                if (NumberOfIntakes > 0) {
                    var upperAggregateExposures = upperIntakes
                        .Select(c => c.GetTotalExternalExposure(
                            relativePotencyFactors,
                            membershipProbabilities,
                            kineticConversionFactors,
                            isPerPerson
                        ))
                        .ToList();
                    LowPercentileValue = upperAggregateExposures.Min();
                    HighPercentileValue = upperAggregateExposures.Max();
                }
                CalculatedUpperPercentage = upperIntakes
                    .Sum(c => c.SimulatedIndividual.SamplingWeight)
                        / aggregateExposures
                        .Sum(c => c.SimulatedIndividual.SamplingWeight) * 100;
            }
        }

        public void SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
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
                        percentageForUpperTail,
                        targetUnit
                    );
                var records = summarizeUncertainty(
                    upperIntakes,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    kineticConversionFactors,
                    targetUnit.IsPerBodyWeight
                );
                updateContributions(records);
            }
        }
    }
}
