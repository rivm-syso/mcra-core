using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.UpperIntakesCalculation {

    /// <summary>
    /// Returns the upper tail for acute or chronic based on aggregate intakes
    /// </summary>
    public class UpperAggregateIntakeCalculator {
        /// <summary>
        /// Gets upper percentile records of target individual exposure records
        /// for the specified target.
        /// </summary>
        public List<T> GetUpperTargetIndividualExposures<T>(
            ICollection<T> targetExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double percentageForUpperTail,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit
        ) where T : AggregateIndividualExposure {
            var aggregateExposures = targetExposures
                .Select(c => (
                    SimulatedIndividualId: c.SimulatedIndividual.Id,
                    c.SimulatedIndividual.SamplingWeight,
                    Exposure: c.GetTotalExposureAtTarget(
                        targetUnit.Target,
                        relativePotencyFactors,
                        membershipProbabilities
                    )
                ))
                .ToList();

            var exposures = aggregateExposures.Select(c => c.Exposure);

            var weights = aggregateExposures.Select(c => c.SamplingWeight).ToList();
            var intakeValue = exposures.PercentilesWithSamplingWeights(weights, percentageForUpperTail);
            var individualIds = aggregateExposures
                .Where(c => c.Exposure >= intakeValue)
                .Select(c => c.SimulatedIndividualId)
                .ToHashSet();
            var result = targetExposures
                .Where(c => individualIds.Contains(c.SimulatedIndividual.Id))
                .ToList();
            return result;
        }
    }
}
