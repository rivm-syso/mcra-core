using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {

    /// <summary>
    /// Returns the upper tail for acute or chronic based on external intakes
    /// </summary>
    public class ExternalExposureUpperExposuresCalculator {

        /// <summary>
        /// Returns the upper tail exposures of the provided external exposures.
        /// </summary>
        /// <param name="externalIndividualDayExposures"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="exposureType"></param>
        /// <param name="upperPercentage"></param>
        /// <param name="isPerPerson"></param>
        ///
        /// <returns></returns>
        public List<IExternalIndividualDayExposure> GetUpperExposures(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            double upperPercentage,
            bool isPerPerson
        ) {
            if (exposureType == ExposureType.Acute) {
                return calculateAcute(externalIndividualDayExposures, relativePotencyFactors, membershipProbabilities, upperPercentage, isPerPerson);
            } else {
                return calculateChronic(externalIndividualDayExposures, relativePotencyFactors, membershipProbabilities, upperPercentage, isPerPerson);
            }
        }

        /// <summary>
        /// Select intake days based on individual mean exposures.
        /// </summary>
        /// <param name="externalIndividualDayExposures"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="upperPercentage"></param>
        /// <returns></returns>
        private List<IExternalIndividualDayExposure> calculateChronic(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double upperPercentage,
            bool isPerPerson
        ) {
            var oims = externalIndividualDayExposures
                .GroupBy(idi => idi.SimulatedIndividualId)
                .Select(g => new  {
                    simulatedIndividualId = g.Key,
                    samplingWeight = g.First().IndividualSamplingWeight,
                    exposure = g.Average(idi =>  idi.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson))
                })
                .ToList();

            var exposures = oims.Select(c => c.exposure).ToList();
            var weights = oims.Select(c => c.samplingWeight).ToList();
            var intakeValue = exposures.PercentilesWithSamplingWeights(weights, upperPercentage);
            var individualsId = oims.Where(c => c.exposure >= intakeValue).Select(c => c.simulatedIndividualId).ToList();
            return externalIndividualDayExposures.Where(c => individualsId.Contains(c.SimulatedIndividualId)).ToList();
        }

        private List<IExternalIndividualDayExposure> calculateAcute(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double upperPercentage,
            bool isPerPerson
        ) {
            var nondietaryIntakes = externalIndividualDayExposures.Select(c => c.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson));
            var nondietaryIntakeWeights = externalIndividualDayExposures.Select(c => c.IndividualSamplingWeight).ToList();
            var intakeValue = nondietaryIntakes.PercentilesWithSamplingWeights(nondietaryIntakeWeights, upperPercentage);
            return externalIndividualDayExposures
                .Where(c => c.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson) > intakeValue)
                .ToList();
        }
    }
}
