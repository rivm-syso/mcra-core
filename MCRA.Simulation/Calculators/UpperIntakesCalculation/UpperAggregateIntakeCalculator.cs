using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.UpperIntakesCalculation {

    /// <summary>
    /// Returns the upper tail for acute or chronic based on aggregate intakes
    /// </summary>
    public class UpperAggregateIntakeCalculator {

        private ExposureType _exposureType;

        public UpperAggregateIntakeCalculator(ExposureType exposureType) {
            _exposureType = exposureType;
        }

        /// <summary>
        /// Gets upper percentile records of target individual day exposure records.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targetExposures"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="upperPercentage"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public List<T> GetUpperTargetIndividualDayExposures<T>(
            ICollection<T> targetExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double upperPercentage,
            bool isPerPerson
        ) where T : ITargetIndividualDayExposure {
            var exposures = targetExposures.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson));
            var weights = targetExposures.Select(c => c.IndividualSamplingWeight).ToList();
            var intakeValue = exposures.PercentilesWithSamplingWeights(weights, upperPercentage);
            return targetExposures.Where(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)> intakeValue).ToList();
        }

        /// <summary>
        /// Gets upper percentile records of target individual exposure records.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targetExposures"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="upperPercentage"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public List<T> GetUpperTargetIndividualExposures<T>(
            ICollection<T> targetExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double upperPercentage,
            bool isPerPerson
        ) where T : ITargetIndividualExposure {
            var exposures = targetExposures.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).ToList();
            var weights = targetExposures.Select(c => c.IndividualSamplingWeight).ToList();
            var intakeValue = exposures.PercentilesWithSamplingWeights(weights, upperPercentage);
            var individualsId = targetExposures.Where(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)> intakeValue).Select(c => c.SimulatedIndividualId).ToList();
            return targetExposures.Where(c => individualsId.Contains(c.SimulatedIndividualId)).ToList();
        }
    }
}
