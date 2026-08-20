using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CoExposureUpperDistributionSubstanceSection : CoExposureDistributionSubstanceSectionBase {

        public void SummarizeCoExposure<T>(
            ICollection<T> aggregateExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double percentageForUpperTail,
            TargetUnit targetUnit
        ) where T : AggregateIndividualExposureBase {
            var upperPercentage = percentageForUpperTail;
            var upperIntakeCalculator = new UpperAggregateIntakeCalculator();
            var upperIntakes = upperIntakeCalculator
                .GetUpperTargetIndividualExposures(
                    aggregateExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    upperPercentage,
                    targetUnit
                );
            summarize(upperIntakes, substances, targetUnit);
        }
    }
}
