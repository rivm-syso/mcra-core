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
        /// Gets upper percentile records of target individual exposure records.
        /// </summary>
        public List<T> GetUpperTargetIndividualExposuresObsolete<T>(
            ICollection<T> targetExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double percentageForUpperTail,
            TargetUnit targetUnit
        ) where T : AggregateIndividualExposure {
            var exposures = targetExposures
                .Select(c => c.GetTotalExposureAtTarget(
                    targetUnit.Target,
                    relativePotencyFactors,
                    membershipProbabilities
                ))
                .ToList();

            var weights = targetExposures.Select(c => c.IndividualSamplingWeight).ToList();
            var intakeValue = exposures.PercentilesWithSamplingWeights(weights, percentageForUpperTail);
            var individualsId = targetExposures
                .Where(c => c.GetTotalExposureAtTarget(
                    targetUnit.Target,
                    relativePotencyFactors,
                    membershipProbabilities
                ) > intakeValue)
                .Select(c => c.SimulatedIndividualId)
                .ToList();
            var result = targetExposures
                .Where(c => individualsId.Contains(c.SimulatedIndividualId))
                .ToList();
            return result;
        }

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
                    IndividualSamplingWeight: c.IndividualSamplingWeight,
                    SimulatedIndividualId: c.SimulatedIndividualId,
                    Exposure: c.GetTotalExposureAtTarget(
                        targetUnit.Target,
                        relativePotencyFactors,
                        membershipProbabilities
                    )
                ))
                .ToList();

            var exposures = aggregateExposures.Select(c => c.Exposure);

            var weights = aggregateExposures.Select(c => c.IndividualSamplingWeight).ToList();
            var intakeValue = exposures.PercentilesWithSamplingWeights(weights, percentageForUpperTail);
            var individualsId = aggregateExposures
                .Where(c => c.Exposure >= intakeValue)
                .Select(c => c.SimulatedIndividualId)
                .ToList();
            var result = targetExposures
                .Where(c => individualsId.Contains(c.SimulatedIndividualId))
                .ToList();
            return result;
        }
    }
}
