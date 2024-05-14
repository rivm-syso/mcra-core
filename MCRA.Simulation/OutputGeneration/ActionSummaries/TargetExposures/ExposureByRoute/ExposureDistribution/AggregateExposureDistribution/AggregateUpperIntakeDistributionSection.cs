using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Stores the upper tail of the transformed exposure distribution in bins,
    /// is used for plotting of the transformed exposure distribution
    /// </summary>
    public class AggregateUpperIntakeDistributionSection : AggregateDistributionSectionBase {

        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }

        public virtual void Summarize(
            List<int> coExposureIndividuals,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit,
            double percentageForUpperTail,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            UpperPercentage = 100 - percentageForUpperTail;
            var aggregateIntakes = aggregateIndividualDayExposures
                .Select(c => c.GetTotalExposureAtTarget(
                    targetUnit.Target,
                    relativePotencyFactors,
                    membershipProbabilities
                ))
                .ToList();
            var weights = aggregateIndividualDayExposures
                .Select(c => c.IndividualSamplingWeight)
                .ToList();
            var intakeValue = aggregateIntakes
                .PercentilesWithSamplingWeights(weights, percentageForUpperTail);
            var upperIntakes = aggregateIndividualDayExposures
                .Where(c => c
                    .GetTotalExposureAtTarget(
                        targetUnit.Target,
                        relativePotencyFactors,
                        membershipProbabilities
                    ) > intakeValue
                )
                .ToList();

            CalculatedUpperPercentage = upperIntakes.Sum(c => c.IndividualSamplingWeight)
                / aggregateIndividualDayExposures.Sum(c => c.IndividualSamplingWeight) * 100;

            summarize(
                coExposureIndividuals,
                upperIntakes,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                exposureRoutes,
                externalExposureUnit,
                targetUnit,
                null,
                uncertaintyLowerLimit,
                uncertaintyUpperLimit
            );
        }
    }
}
