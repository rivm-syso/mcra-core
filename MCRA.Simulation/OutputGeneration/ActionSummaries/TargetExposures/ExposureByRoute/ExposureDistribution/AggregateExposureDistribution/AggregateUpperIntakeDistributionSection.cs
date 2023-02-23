using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Stores the upper tail of the transformed exposure distribution in bins, is used for plotting of the transformed exposure distribution
    /// </summary>
    public class AggregateUpperIntakeDistributionSection : AggregateDistributionSectionBase {

        public double UpperPercentage { get; set; }

        public virtual void Summarize(
            List<int> coExposureIndividuals,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRouteType, Compound), double> absorptionFactors,
            ICollection<ExposureRouteType> exposureRoutes,
            double percentageForUpperTail,
            bool isPerPerson,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            UpperPercentage = percentageForUpperTail;
            var aggregateIntakes = aggregateIndividualDayExposures.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson));
            var weights = aggregateIndividualDayExposures.Select(c => c.IndividualSamplingWeight).ToList();
            var intakeValue = aggregateIntakes.PercentilesWithSamplingWeights(weights, UpperPercentage);
            var upperIntakes = aggregateIndividualDayExposures
                 .Where(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)> intakeValue)
                 .ToList();
            UpperPercentage = 100 - upperIntakes.Sum(c => c.IndividualSamplingWeight) / aggregateIndividualDayExposures.Sum(c => c.IndividualSamplingWeight) * 100;
            Summarize(
                coExposureIndividuals,
                upperIntakes,
                relativePotencyFactors,
                membershipProbabilities,
                absorptionFactors,
                exposureRoutes,
                null,
                isPerPerson,
                uncertaintyLowerLimit,
                UncertaintyUpperLimit
            );
            summarizeCategorizedBins(
                upperIntakes,
                relativePotencyFactors,
                membershipProbabilities,
                absorptionFactors,
                exposureRoutes,
                isPerPerson
            );
        }
    }
}
